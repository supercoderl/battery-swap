using BatterySwap.Application.Common.Models;
using BatterySwap.Application.DTOs.Stations;

namespace BatterySwap.Application.Services.Stations;

public interface IStationService
{
    Task<PagedResult<StationDto>> GetPagedAsync(QueryParameters query, CancellationToken ct = default);
    Task<StationDto> GetByIdAsync(long id, CancellationToken ct = default);
    Task<StationDto> CreateAsync(CreateStationDto dto, CancellationToken ct = default);
    Task<StationDto> UpdateAsync(long id, UpdateStationDto dto, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
}
