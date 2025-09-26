using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolarPanel.Application.DTOs;
using SolarPanel.Application.Interfaces;
using SolarPanel.Core.Entities;
using SolarPanel.Core.Interfaces;
using SolarPanel.Infrastructure.Services;

namespace SolarPanel.Infrastructure.BackgroundServices;

public class MqttBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MqttBackgroundService> _logger;
    private readonly MqttSettings _settings;
    private readonly MqttService _mqttService;

    public MqttBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<MqttBackgroundService> logger,
        IOptions<MqttSettings> settings,
        MqttService mqttService)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _settings = settings.Value;
        _mqttService = mqttService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_settings.UseMockData)
        {
            _logger.LogInformation("Real MQTT service is disabled (UseMockData = true)");
            return;
        }

        _logger.LogInformation("Starting MQTT Background Service...");

        var retryDelay = TimeSpan.FromSeconds(30);
        var maxRetryDelay = TimeSpan.FromMinutes(5);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Attempting to connect to MQTT broker...");

                var connected = await _mqttService.ConnectAsync();
                if (!connected)
                {
                    _logger.LogWarning("Failed to connect to MQTT broker. Retrying in {Delay} seconds...",
                        retryDelay.TotalSeconds);
                    await Task.Delay(retryDelay, stoppingToken);

                    retryDelay = retryDelay.TotalSeconds < maxRetryDelay.TotalSeconds
                        ? TimeSpan.FromSeconds(Math.Min(retryDelay.TotalSeconds * 1.5, maxRetryDelay.TotalSeconds))
                        : maxRetryDelay;

                    continue;
                }

                retryDelay = TimeSpan.FromSeconds(30);
                _logger.LogInformation("Successfully connected to MQTT broker, subscribing to topic: {Topic}",
                    _settings.DataTopic);

                await _mqttService.SubscribeAsync(_settings.DataTopic, OnSolarDataMessageReceived);
                _logger.LogInformation("Successfully subscribed to MQTT topic: {Topic}", _settings.DataTopic);

                if (!string.IsNullOrWhiteSpace(_settings.ModeResultTopic))
                {
                    _logger.LogInformation("Subscribing to mode result topic: {Topic}", _settings.ModeResultTopic);
                    await _mqttService.SubscribeAsync(_settings.ModeResultTopic, OnModeResultMessageReceived);
                    _logger.LogInformation("Successfully subscribed to MQTT topic: {Topic}", _settings.ModeResultTopic);
                }

                while (!stoppingToken.IsCancellationRequested && _mqttService.IsConnected())
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }

                if (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning("MQTT connection lost, attempting to reconnect...");
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("MQTT Background Service is stopping due to cancellation request");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MQTT background service. Retrying in {Delay} seconds...",
                    retryDelay.TotalSeconds);

                try
                {
                    await Task.Delay(retryDelay, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                retryDelay = retryDelay.TotalSeconds < maxRetryDelay.TotalSeconds
                    ? TimeSpan.FromSeconds(Math.Min(retryDelay.TotalSeconds * 1.5, maxRetryDelay.TotalSeconds))
                    : maxRetryDelay;
            }
        }

        _logger.LogInformation("MQTT Background Service stopped");
    }

    private async Task OnModeResultMessageReceived(string jsonMessage)
    {
        try
        {
            _logger.LogDebug("Processing MQTT mode result message: {Message}", jsonMessage);

            using var scope = _scopeFactory.CreateScope();
            var modeResultRepository = scope.ServiceProvider.GetRequiredService<IModeResultRepository>();

            var modeResultDto = JsonSerializer.Deserialize<ModeResultDto>(jsonMessage, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            });

            var modeResult = modeResultDto != null
                ? new ModeResult
                {
                    BatteryMode = modeResultDto.BatteryMode,
                    LoadMode = modeResultDto.LoadMode
                }
                : null;

            if (modeResult != null)
            {
                await modeResultRepository.SaveModeResultAsync(modeResult);
                _logger.LogInformation("Inverter mode result saved successfully from MQTT message");
            }
            else
            {
                _logger.LogWarning("Failed to deserialize MQTT mode result message - result was null: {Message}",
                    jsonMessage);
            }
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON deserialization error for MQTT mode result message: {Message}", jsonMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MQTT mode result message: {Message}", jsonMessage);
        }
    }

    private async Task OnSolarDataMessageReceived(string jsonMessage)
    {
        try
        {
            _logger.LogDebug("Processing MQTT message: {Message}", jsonMessage);

            using var scope = _scopeFactory.CreateScope();
            var solarDataService = scope.ServiceProvider.GetRequiredService<ISolarDataService>();

            var reading = JsonSerializer.Deserialize<SolarPanelDataJsonDto>(jsonMessage, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                PropertyNameCaseInsensitive = true
            });

            if (reading != null)
            {
                await solarDataService.SaveSolarDataAsync(reading);
                _logger.LogInformation("Solar data saved successfully from MQTT message");
            }
            else
            {
                _logger.LogWarning("Failed to deserialize MQTT message - result was null: {Message}", jsonMessage);
            }
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON deserialization error for MQTT message: {Message}", jsonMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MQTT message: {Message}", jsonMessage);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping MQTT Background Service...");

        try
        {
            await _mqttService.DisconnectAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting MQTT service during shutdown");
        }

        await base.StopAsync(cancellationToken);
    }
}