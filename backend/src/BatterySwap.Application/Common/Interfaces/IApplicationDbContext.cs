using BatterySwap.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BatterySwap.Application.Common.Interfaces;

/// <summary>Read-side abstraction of the EF DbContext for the Application layer.</summary>
public interface IApplicationDbContext
{
    DbSet<Station> Stations { get; }
    DbSet<Cabinet> Cabinets { get; }
    DbSet<Slot> Slots { get; }
    DbSet<User> Users { get; }
    DbSet<Account> Accounts { get; }
    DbSet<Battery> Batteries { get; }
    DbSet<BatteryLog> BatteryLogs { get; }
    DbSet<ActiveSwappingSession> ActiveSwappingSessions { get; }
    DbSet<SwappingTransaction> SwappingTransactions { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
