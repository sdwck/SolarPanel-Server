using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolarPanel.Application.DTOs;
using SolarPanel.Application.Interfaces;
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

            _logger.LogInformation("Real MQTT Background Service started");

            try
            {
                Console.WriteLine("MQTT Settings:");
                Console.WriteLine($"  BrokerHost: {_settings.BrokerHost}");
                Console.WriteLine($"  Port: {_settings.Port}");
                Console.WriteLine($"  Topic: {_settings.Topic}");
                Console.WriteLine($"  UseMockData: {_settings.UseMockData}");
                Console.WriteLine($"  Username: {_settings.Username}");
                Console.WriteLine($"  Password: {_settings.Password}");
                var connected = await _mqttService.ConnectAsync();
                if (!connected)
                {
                    _logger.LogError("Failed to connect to MQTT broker. Service stopped.");
                    return;
                }
                
                await _mqttService.SubscribeAsync(_settings.Topic, OnMessageReceived);

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Real MQTT background service");
            }
        }

        private async Task OnMessageReceived(string jsonMessage)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var solarDataService = scope.ServiceProvider.GetRequiredService<ISolarDataService>();

                var reading = JsonSerializer.Deserialize<SolarPanelDataJsonDto>(jsonMessage, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                if (reading != null)
                {
                    await solarDataService.SaveSolarDataAsync(reading);
                    _logger.LogInformation("Solar data saved successfully from MQTT");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MQTT message: {Message}", jsonMessage);
            }
        }
    }