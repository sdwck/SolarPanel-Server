using System.Text.Json.Serialization;

namespace SolarPanel.Application.DTOs;

public class SolarPanelDataJsonDto
{
        [JsonPropertyName("_command")]
        public string Command { get; set; } = string.Empty;

        [JsonPropertyName("_command_description")]
        public string CommandDescription { get; set; } = string.Empty;

        [JsonPropertyName("ac_input_voltage")]
        public decimal AcInputVoltage { get; set; }

        [JsonPropertyName("ac_input_frequency")]
        public decimal AcInputFrequency { get; set; }

        [JsonPropertyName("ac_output_voltage")]
        public decimal AcOutputVoltage { get; set; }

        [JsonPropertyName("ac_output_frequency")]
        public decimal AcOutputFrequency { get; set; }

        [JsonPropertyName("ac_output_apparent_power")]
        public int AcOutputApparentPower { get; set; }

        [JsonPropertyName("ac_output_active_power")]
        public int AcOutputActivePower { get; set; }
        
        [JsonPropertyName("ac_output_load")]
        public int AcOutputLoad { get; set; }

        [JsonPropertyName("bus_voltage")]
        public double BusVoltage { get; set; }

        [JsonPropertyName("battery_voltage")]
        public decimal BatteryVoltage { get; set; }

        [JsonPropertyName("battery_charging_current")]
        public decimal BatteryChargingCurrent { get; set; }

        [JsonPropertyName("battery_capacity")]
        public int BatteryCapacity { get; set; }

        [JsonPropertyName("inverter_heat_sink_temperature")]
        public int InverterHeatSinkTemperature { get; set; }

        [JsonPropertyName("pv_input_current_for_battery")]
        public decimal PvInputCurrent { get; set; }

        [JsonPropertyName("pv_input_voltage")]
        public decimal PvInputVoltage { get; set; }

        [JsonPropertyName("battery_voltage_from_scc")]
        public decimal BatteryVoltageFromScc { get; set; }

        [JsonPropertyName("battery_discharge_current")]
        public decimal BatteryDischargeCurrent { get; set; }

        [JsonPropertyName("is_sbu_priority_version_added")]
        public int IsSbuPriorityVersionAdded { get; set; }

        [JsonPropertyName("is_configuration_changed")]
        public int IsConfigurationChanged { get; set; }

        [JsonPropertyName("is_scc_firmware_updated")]
        public int IsSccFirmwareUpdated { get; set; }

        [JsonPropertyName("is_load_on")]
        public int IsLoadOn { get; set; }

        [JsonPropertyName("is_battery_voltage_to_steady_while_charging")]
        public int IsBatteryVoltageToSteadyWhileCharging { get; set; }

        [JsonPropertyName("is_charging_on")]
        public int IsChargingOn { get; set; }

        [JsonPropertyName("is_scc_charging_on")]
        public int IsSccChargingOn { get; set; }

        [JsonPropertyName("is_ac_charging_on")]
        public int IsAcChargingOn { get; set; }

        [JsonPropertyName("rsv1")]
        public int Rsv1 { get; set; }

        [JsonPropertyName("rsv2")]
        public int Rsv2 { get; set; }

        [JsonPropertyName("pv_input_power")]
        public int PvInputPower { get; set; }

        [JsonPropertyName("is_charging_to_float")]
        public int IsChargingToFloat { get; set; }

        [JsonPropertyName("is_switched_on")]
        public int IsSwitchedOn { get; set; }

        [JsonPropertyName("is_reserved")]
        public int IsReserved { get; set; }
}