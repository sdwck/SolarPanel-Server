using System.Security.Authentication;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using SolarPanel.Infrastructure.BackgroundServices;

namespace SolarPanel.Infrastructure.Services;

public class MqttService : IDisposable
    {
        private readonly IMqttClient _mqttClient;
        private readonly MqttSettings _settings;
        private readonly ILogger<MqttService> _logger;

        public MqttService(IOptions<MqttSettings> settings, ILogger<MqttService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
            
            var factory = new MqttClientFactory();
            _mqttClient = factory.CreateMqttClient();
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer(_settings.BrokerHost, _settings.Port)
                    .WithCredentials(_settings.Username, _settings.Password)
                    .WithClientId($"SolarPanel-{Environment.MachineName}-{Guid.NewGuid()}")
                    .WithTlsOptions(options => options.UseTls())
                    .Build();

                await _mqttClient.ConnectAsync(options);
                _logger.LogInformation("Connected to MQTT broker: {Broker}:{Port}", _settings.BrokerHost, _settings.Port);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to MQTT broker");
                return false;
            }
        }

        public async Task SubscribeAsync(string topic, Func<string, Task> onMessageReceived)
        {
            if (!_mqttClient.IsConnected)
            {
                await ConnectAsync();
            }

            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                _logger.LogDebug("Received MQTT message on topic {Topic}: {Payload}", e.ApplicationMessage.Topic, payload);
                
                await onMessageReceived(payload);
            };

            await _mqttClient.SubscribeAsync(topic);
            _logger.LogInformation("Subscribed to MQTT topic: {Topic}", topic);
        }

        public void Dispose()
        {
            _mqttClient?.DisconnectAsync().Wait(5000);
            _mqttClient?.Dispose();
        }
    }