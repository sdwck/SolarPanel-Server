using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolarPanel.Application.DTOs;
using SolarPanel.Application.Interfaces;

namespace SolarPanel.Infrastructure.BackgroundServices;

public class MockMqttBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MockMqttBackgroundService> _logger;
    private readonly MqttSettings _settings;
    private readonly Random _random = new();

    public MockMqttBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<MockMqttBackgroundService> logger,
        IOptions<MqttSettings> settings)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.UseMockData)
        {
            _logger.LogInformation("Mock data service is disabled");
            return;
        }

        _logger.LogInformation("Mock Data Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var solarDataService = scope.ServiceProvider.GetRequiredService<ISolarDataService>();

                var mockData = GenerateMockData();
                await solarDataService.SaveSolarDataAsync(mockData);

                _logger.LogInformation("Mock solar data generated and saved");

                await Task.Delay(TimeSpan.FromSeconds(_settings.DataReadingIntervalSeconds), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Mock Data background service");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }

    private SolarPanelDataJsonDto GenerateMockData()
    {
        return new SolarPanelDataJsonDto
        {
            Command = "QPIGS",
            CommandDescription = "General Status Parameters inquiry",
            AcInputVoltage = 235m + (decimal)(_random.NextDouble() * 10 - 5),
            AcInputFrequency = 50.0m + ((decimal)_random.NextDouble() * 0.2m - 0.1m),
            AcOutputVoltage = 225m + (decimal)(_random.NextDouble() * 10 - 5),
            AcOutputFrequency = 50.0m + ((decimal)_random.NextDouble() * 0.2m - 0.1m),
            AcOutputApparentPower = 250 + _random.Next(-50, 100),
            AcOutputActivePower = 240 + _random.Next(-40, 80),
            AcOutputLoad = 5 + _random.Next(0, 20),
            BusVoltage = 440 + _random.NextDouble() * 20 - 10,
            BatteryVoltage = 27m + (decimal)(_random.NextDouble() * 2),
            BatteryChargingCurrent = (decimal)(_random.NextDouble() * 15),
            BatteryCapacity = 90 + _random.Next(0, 11),
            InverterHeatSinkTemperature = 40 + _random.Next(0, 20),
            PvInputCurrent = (decimal)(_random.NextDouble() * 5),
            PvInputVoltage = 200m + (decimal)(_random.NextDouble() * 50),
            BatteryVoltageFromScc = (decimal)(_random.NextDouble() * 2),
            BatteryDischargeCurrent = (decimal)(_random.NextDouble() * 3),
            IsSbuPriorityVersionAdded = 0,
            IsConfigurationChanged = _random.Next(0, 100) < 5 ? 1 : 0,
            IsSccFirmwareUpdated = 0,
            IsLoadOn = 1,
            IsBatteryVoltageToSteadyWhileCharging = _random.Next(0, 2),
            IsChargingOn = _random.Next(0, 10) < 8 ? 1 : 0,
            IsSccChargingOn = _random.Next(0, 10) < 7 ? 1 : 0,
            IsAcChargingOn = _random.Next(0, 10) < 3 ? 1 : 0,
            Rsv1 = 0,
            Rsv2 = 0,
            PvInputPower = 400 + _random.Next(-100, 200),
            IsChargingToFloat = _random.Next(0, 5) == 1 ? 1 : 0,
            IsSwitchedOn = 1,
            IsReserved = 0
        };
    }
}