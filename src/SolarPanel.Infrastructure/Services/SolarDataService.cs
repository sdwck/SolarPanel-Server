using SolarPanel.Application.DTOs;
using SolarPanel.Application.Interfaces;
using SolarPanel.Core.Entities;
using SolarPanel.Core.Interfaces;

namespace SolarPanel.Infrastructure.Services;

public class SolarDataService : ISolarDataService
{
    private readonly ISolarDataRepository _repository;

    public SolarDataService(ISolarDataRepository repository)
    {
        _repository = repository;
    }

    public async Task<SolarDataDto> SaveSolarDataAsync(SolarPanelDataJsonDto dataJsonDto)
    {
        var solarData = new SolarData
        {
            Command = dataJsonDto.Command,
            CommandDescription = dataJsonDto.CommandDescription,
            InverterHeatSinkTemperature = dataJsonDto.InverterHeatSinkTemperature,
            BusVoltage = dataJsonDto.BusVoltage,
            IsLoadOn = dataJsonDto.IsLoadOn == 1,
            IsChargingOn = dataJsonDto.IsChargingOn == 1,
            IsSccChargingOn = dataJsonDto.IsSccChargingOn == 1,
            IsAcChargingOn = dataJsonDto.IsAcChargingOn == 1,
            IsSwitchedOn = dataJsonDto.IsSwitchedOn == 1,
            IsSbuPriorityVersionAdded = dataJsonDto.IsSbuPriorityVersionAdded == 1,
            IsConfigurationChanged = dataJsonDto.IsConfigurationChanged == 1,
            IsSccFirmwareUpdated = dataJsonDto.IsSccFirmwareUpdated == 1,
            IsBatteryVoltageToSteadyWhileCharging = dataJsonDto.IsBatteryVoltageToSteadyWhileCharging == 1,
            IsChargingToFloat = dataJsonDto.IsChargingToFloat == 1,
            IsReserved = dataJsonDto.IsReserved == 1,
            Rsv1 = dataJsonDto.Rsv1,
            Rsv2 = dataJsonDto.Rsv2,
            BatteryData = new BatteryData
            {
                BatteryVoltage = dataJsonDto.BatteryVoltage,
                BatteryChargingCurrent = dataJsonDto.BatteryChargingCurrent,
                BatteryCapacity = dataJsonDto.BatteryCapacity,
                BatteryDischargeCurrent = dataJsonDto.BatteryDischargeCurrent,
                BatteryVoltageFromScc = dataJsonDto.BatteryVoltageFromScc
            },
            PowerData = new PowerData
            {
                AcInputVoltage = dataJsonDto.AcInputVoltage,
                AcInputFrequency = dataJsonDto.AcInputFrequency,
                AcOutputVoltage = dataJsonDto.AcOutputVoltage,
                AcOutputFrequency = dataJsonDto.AcOutputFrequency,
                AcOutputApparentPower = dataJsonDto.AcOutputApparentPower,
                AcOutputActivePower = dataJsonDto.AcOutputActivePower,
                AcOutputLoad = dataJsonDto.AcOutputLoad,
                PvInputCurrent = dataJsonDto.PvInputCurrent,
                PvInputVoltage = dataJsonDto.PvInputVoltage,
                PvInputPower = dataJsonDto.PvInputPower
            }
        };

        var savedData = await _repository.AddAsync(solarData);
        return MapToDto(savedData);
    }

    public async Task<SolarDataDto?> GetLatestDataAsync()
    {
        var data = await _repository.GetLatestAsync();
        return data != null ? MapToDto(data) : null;
    }

    public async Task<SolarDataDto?> GetByIdAsync(int id)
    {
        var data = await _repository.GetByIdAsync(id);
        return data != null ? MapToDto(data) : null;
    }

    public async Task<PaginatedResponse<SolarDataDto>> GetAllAsync(int page = 1, int pageSize = 50)
    {
        var data = await _repository.GetAllAsync(page, pageSize);
        var count = await _repository.GetTotalCountAsync();
        return new PaginatedResponse<SolarDataDto>
        {
            Items = data.Select(MapToDto),
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = count,
            TotalPages = (int)Math.Ceiling(count / (double)pageSize)
        };
    }

    public async Task<IEnumerable<SolarDataDto>> GetByDateRangeAsync(DateTime from, DateTime to, int? gapInRecords = null, int? count = null)
    {
        if (from > to) throw new ArgumentException("'from' cannot be greater than 'to'");
        var data = await _repository.GetByDateRangeAsync(from, to, gapInRecords, count);
        return data.Select(MapToDto);
    }

    private static SolarDataDto MapToDto(SolarData data)
    {
        return new SolarDataDto
        {
            Id = data.Id,
            Timestamp = data.Timestamp,
            Command = data.Command,
            CommandDescription = data.CommandDescription,
            InverterHeatSinkTemperature = data.InverterHeatSinkTemperature,
            BusVoltage = data.BusVoltage,
            IsLoadOn = data.IsLoadOn,
            IsChargingOn = data.IsChargingOn,
            IsSccChargingOn = data.IsSccChargingOn,
            IsAcChargingOn = data.IsAcChargingOn,
            IsSwitchedOn = data.IsSwitchedOn,
            BatteryData = data.BatteryData != null
                ? new BatteryDataDto
                {
                    BatteryVoltage = data.BatteryData.BatteryVoltage,
                    BatteryChargingCurrent = data.BatteryData.BatteryChargingCurrent,
                    BatteryCapacity = data.BatteryData.BatteryCapacity,
                    BatteryDischargeCurrent = data.BatteryData.BatteryDischargeCurrent
                }
                : null,
            PowerData = data.PowerData != null
                ? new PowerDataDto
                {
                    AcInputVoltage = data.PowerData.AcInputVoltage,
                    AcInputFrequency = data.PowerData.AcInputFrequency,
                    AcOutputVoltage = data.PowerData.AcOutputVoltage,
                    AcOutputFrequency = data.PowerData.AcOutputFrequency,
                    AcOutputActivePower = data.PowerData.AcOutputActivePower,
                    AcOutputLoad = data.PowerData.AcOutputLoad,
                    PvInputVoltage = data.PowerData.PvInputVoltage,
                    PvInputCurrent = data.PowerData.PvInputCurrent,
                    PvInputPower = data.PowerData.PvInputPower
                }
                : null
        };
    }

    public async Task<EnergyResponseDto> GetEnergyProducedAsync(DateTime from, DateTime to, string source = "pv")
    {
        if (from > to) throw new ArgumentException("'from' cannot be greater than 'to'");

        source = (source ?? "pv").Trim().ToLowerInvariant();
        if (source != "pv" && source != "ac") throw new ArgumentException("source must be 'pv' or 'ac'");

        var data = await _repository.GetByDateRangeAsync(from, to);

        var ordered = data
            .Where(d => d.PowerData != null)
            .OrderBy(d => d.Timestamp)
            .ToList();

        double energyWh = 0.0;
        int samplesUsed = 0;

        for (int i = 1; i < ordered.Count; i++)
        {
            var prev = ordered[i - 1];
            var curr = ordered[i];

            if (prev.PowerData == null || curr.PowerData == null) continue;

            double p1 = source == "pv" ? prev.PowerData.PvInputPower : prev.PowerData.AcOutputActivePower;
            double p2 = source == "pv" ? curr.PowerData.PvInputPower : curr.PowerData.AcOutputActivePower;

            if (p1 < 0 || p2 < 0) continue;

            var segmentKWh = (p1 + p2) / 2.0 * (1 / 60.0);
            energyWh += segmentKWh;
            samplesUsed++;
        }

        return new EnergyResponseDto
        {
            From = from,
            To = to,
            EnergyKWh = Math.Round(energyWh / 1000.0, 4),
            SamplesUsed = samplesUsed + 1,
            Source = source
        };
    }
}
