using System.Security.Authentication;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using SolarPanel.Infrastructure.BackgroundServices;

namespace SolarPanel.Infrastructure.Services;

public class MqttService : IDisposable
{
    private IMqttClient? _mqttClient;
    private readonly MqttSettings _settings;
    private readonly ILogger<MqttService> _logger;
    private bool _disposed;

    public MqttService(IOptions<MqttSettings> settings, ILogger<MqttService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public bool IsConnected()
    {
        return _mqttClient?.IsConnected ?? false;
    }

    public async Task<bool> ConnectAsync()
    {
        try
        {
            if (_mqttClient?.IsConnected == true)
            {
                _logger.LogDebug("MQTT client already connected");
                return true;
            }

            if (_mqttClient != null)
            {
                try
                {
                    await _mqttClient.DisconnectAsync();
                }
                catch
                {
                    // ignored
                }

                _mqttClient.Dispose();
            }

            var factory = new MqttClientFactory();
            _mqttClient = factory.CreateMqttClient();

            _mqttClient.DisconnectedAsync += OnDisconnected;

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(_settings.BrokerHost, _settings.Port)
                .WithCredentials(_settings.Username, _settings.Password)
                .WithClientId($"SolarPanel-{Environment.MachineName}-{Guid.NewGuid()}")
                .WithTlsOptions(tlsOptions =>
                {
                    tlsOptions.UseTls();
                    tlsOptions.WithSslProtocols(SslProtocols.Tls12);
                })
                .WithCleanSession()
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(60))
                .WithTimeout(TimeSpan.FromSeconds(10))
                .Build();

            var result = await _mqttClient.ConnectAsync(options);

            if (result.ResultCode == MqttClientConnectResultCode.Success)
            {
                _logger.LogInformation("Connected to MQTT broker: {Broker}:{Port}", _settings.BrokerHost,
                    _settings.Port);
                return true;
            }

            _logger.LogError("Failed to connect to MQTT broker. Result: {ResultCode}, Reason: {Reason}",
                result.ResultCode, result.ReasonString);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to MQTT broker");
            return false;
        }
    }

    public async Task SubscribeAsync(string topic, Func<string, Task> onMessageReceived)
    {
        if (_mqttClient is not { IsConnected: true })
        {
            _logger.LogWarning("MQTT client not connected, attempting to connect before subscribing");
            var connected = await ConnectAsync();
            if (!connected)
            {
                throw new InvalidOperationException("Cannot subscribe - MQTT client is not connected");
            }
        }

        if (_mqttClient != null)
        {
            _mqttClient.ApplicationMessageReceivedAsync -= OnApplicationMessageReceived;
            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                try
                {
                    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    _logger.LogDebug("Received MQTT message on topic {Topic}: {Payload}", e.ApplicationMessage.Topic,
                        payload);

                    await onMessageReceived(payload);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling MQTT message");
                }
            };

            var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(topic)
                .Build();

            await _mqttClient.SubscribeAsync(subscribeOptions);
        }

        _logger.LogInformation("Subscribed to MQTT topic: {Topic}", topic);
    }

    public async Task DisconnectAsync()
    {
        try
        {
            if (_mqttClient?.IsConnected == true)
            {
                await _mqttClient.DisconnectAsync();
                _logger.LogInformation("Disconnected from MQTT broker");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting from MQTT broker");
        }
    }

    private async Task OnDisconnected(MqttClientDisconnectedEventArgs args)
    {
        if (_disposed)
            return;

        _logger.LogWarning("MQTT client disconnected: {Reason}", args.Reason);

        if (args.ClientWasConnected &&
            args.Reason != MqttClientDisconnectReason.NormalDisconnection &&
            !args.ReasonString?.Contains("shutdown") == true)
        {
            _logger.LogInformation("Attempting to reconnect to MQTT broker in 5 seconds...");
            await Task.Delay(TimeSpan.FromSeconds(5));

            try
            {
                await ConnectAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to auto-reconnect to MQTT broker");
            }
        }
    }

    private Task OnApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs args)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            if (_mqttClient?.IsConnected == true)
            {
                _mqttClient.DisconnectAsync().Wait(5000);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during MQTT service disposal");
        }
        finally
        {
            _mqttClient?.Dispose();
        }
    }
}