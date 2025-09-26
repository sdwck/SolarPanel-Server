namespace SolarPanel.Infrastructure.BackgroundServices;

public class MqttSettings
{
    public string BrokerHost { get; init; } = "localhost";
    public int Port { get; init; } = 8883;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string DataTopic { get; init; } = "data";
    public string ModeResultTopic { get; init; } = "mode_result";
    public bool UseMockData { get; init; } = true;
    public int DataReadingIntervalSeconds { get; init; } = 60;
}