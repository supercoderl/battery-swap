using BatterySwap.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BatterySwap.Infrastructure.Persistence.Configurations;

public class StationConfiguration : IEntityTypeConfiguration<Station>
{
    public void Configure(EntityTypeBuilder<Station> b)
    {
        b.ToTable("Stations");
        b.HasKey(x => x.Id);
        b.Property(x => x.Address).IsRequired().HasMaxLength(300);
        b.Property(x => x.Latitude);
        b.Property(x => x.Longitude);
        b.HasMany(x => x.Cabinets).WithOne(c => c.Station)
            .HasForeignKey(c => c.StationId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class CabinetConfiguration : IEntityTypeConfiguration<Cabinet>
{
    public void Configure(EntityTypeBuilder<Cabinet> b)
    {
        b.ToTable("Cabinets");
        b.HasKey(x => x.Id);
        b.Property(x => x.CabinetModel).IsRequired().HasMaxLength(100);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.HasMany(x => x.Slots).WithOne(s => s.Cabinet)
            .HasForeignKey(s => s.CabinetId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SlotConfiguration : IEntityTypeConfiguration<Slot>
{
    public void Configure(EntityTypeBuilder<Slot> b)
    {
        b.ToTable("Slots");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.CabinetId, x.SlotNumber }).IsUnique();
        b.HasOne(x => x.CurrentBattery).WithMany()
            .HasForeignKey(x => x.CurrentBatteryId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("Users");
        b.HasKey(x => x.Id);
        b.Property(x => x.FullName).IsRequired().HasMaxLength(150);
        b.Property(x => x.Phone).IsRequired().HasMaxLength(20);
        b.HasIndex(x => x.Phone).IsUnique();
    }
}

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> b)
    {
        b.ToTable("Accounts");
        b.HasKey(x => x.Id);
        b.Property(x => x.Username).IsRequired().HasMaxLength(100);
        b.Property(x => x.Email).IsRequired().HasMaxLength(200);
        b.Property(x => x.PasswordHash).IsRequired();
        b.Property(x => x.FullName).HasMaxLength(150);
        b.Property(x => x.Role).IsRequired().HasMaxLength(20);
        b.HasIndex(x => x.Username).IsUnique();
        b.HasIndex(x => x.Email).IsUnique();
    }
}

public class BatteryConfiguration : IEntityTypeConfiguration<Battery>
{
    public void Configure(EntityTypeBuilder<Battery> b)
    {
        b.ToTable("Batteries");
        b.HasKey(x => x.Id);
        b.Property(x => x.HealthState).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.LocationType).HasConversion<string>().HasMaxLength(20);
        b.HasMany(x => x.Logs).WithOne(l => l.Battery)
            .HasForeignKey(l => l.BatteryId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class BatteryLogConfiguration : IEntityTypeConfiguration<BatteryLog>
{
    public void Configure(EntityTypeBuilder<BatteryLog> b)
    {
        b.ToTable("BatteryLogs");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.BatteryId, x.RecordedAt });
    }
}

public class ActiveSwappingSessionConfiguration : IEntityTypeConfiguration<ActiveSwappingSession>
{
    public void Configure(EntityTypeBuilder<ActiveSwappingSession> b)
    {
        b.ToTable("ActiveSwappingSessions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.HasIndex(x => x.UserId).IsUnique(); // one active session per user
        b.HasOne(x => x.User).WithMany(u => u.Sessions)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Cabinet).WithMany(c => c.Sessions)
            .HasForeignKey(x => x.CabinetId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SwappingTransactionConfiguration : IEntityTypeConfiguration<SwappingTransaction>
{
    public void Configure(EntityTypeBuilder<SwappingTransaction> b)
    {
        b.ToTable("SwappingTransactions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.HasIndex(x => x.CreatedAt);

        b.HasOne(x => x.User).WithMany(u => u.Transactions)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Cabinet).WithMany(c => c.Transactions)
            .HasForeignKey(x => x.CabinetId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.SlotIn).WithMany()
            .HasForeignKey(x => x.SlotInId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.SlotOut).WithMany()
            .HasForeignKey(x => x.SlotOutId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.BatteryIn).WithMany()
            .HasForeignKey(x => x.BatteryInId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.BatteryOut).WithMany()
            .HasForeignKey(x => x.BatteryOutId).OnDelete(DeleteBehavior.NoAction);
    }
}
