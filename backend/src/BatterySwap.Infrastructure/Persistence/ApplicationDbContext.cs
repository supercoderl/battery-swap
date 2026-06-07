using BatterySwap.Application.Common.Interfaces;
using BatterySwap.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BatterySwap.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Station> Stations => Set<Station>();
    public DbSet<Cabinet> Cabinets => Set<Cabinet>();
    public DbSet<Slot> Slots => Set<Slot>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Battery> Batteries => Set<Battery>();
    public DbSet<BatteryLog> BatteryLogs => Set<BatteryLog>();
    public DbSet<ActiveSwappingSession> ActiveSwappingSessions => Set<ActiveSwappingSession>();
    public DbSet<SwappingTransaction> SwappingTransactions => Set<SwappingTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
