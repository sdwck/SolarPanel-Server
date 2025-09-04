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

    public async Task<SolarDataResponseDto> GetAllAsync(int page = 1, int pageSize = 50)
    {
        var data = await _repository.GetAllAsync(page, pageSize);
        var count = await _repository.GetTotalCountAsync();
        return new SolarDataResponseDto
        {
            Data = data.Select(MapToDto),
            Page = page,
            PageSize = pageSize,
            TotalCount = count
        };
    }

    public async Task<IEnumerable<SolarDataDto>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        var data = await _repository.GetByDateRangeAsync(from, to);
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
}