namespace SolarPanel.Application.DTOs;

public class SolarDataResponseDto
{
    public IEnumerable<SolarDataDto> Data { get; set; } = new List<SolarDataDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}