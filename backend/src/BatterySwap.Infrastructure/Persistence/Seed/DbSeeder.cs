using BatterySwap.Application.Common.Interfaces;
using BatterySwap.Domain.Entities;
using BatterySwap.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BatterySwap.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        await db.Database.MigrateAsync();

        var rnd = new Random(20240601);

        // ---- Accounts (login) ----
        if (!await db.Accounts.AnyAsync())
        {
            db.Accounts.AddRange(
                new Account { Username = "admin", Email = "admin@vinfast.local", FullName = "System Administrator", Role = "ADMIN", PasswordHash = hasher.Hash("Admin@123"), IsActive = true },
                new Account { Username = "operator", Email = "operator@vinfast.local", FullName = "Station Operator", Role = "OPERATOR", PasswordHash = hasher.Hash("Operator@123"), IsActive = true },
                new Account { Username = "supervisor", Email = "supervisor@vinfast.local", FullName = "System Supervisor", Role = "SUPERVISOR", PasswordHash = hasher.Hash("Supervisor@123"), IsActive = true }
            );
            await db.SaveChangesAsync();
            logger.LogInformation("Seeded accounts.");
        }

        if (await db.Stations.AnyAsync())
        {
            logger.LogInformation("Domain data already seeded; skipping.");
            return;
        }

        // ---- Stations ----
        var stationSeeds = new (string Address, double Lat, double Lng)[]
        {
            ("12 Nguyen Hue, District 1, Ho Chi Minh City", 10.7740, 106.7040),
            ("88 Le Loi, District 1, Ho Chi Minh City", 10.7725, 106.6990),
            ("245 Cau Giay, Cau Giay, Hanoi", 21.0333, 105.7990),
            ("36 Tran Phu, Hai Chau, Da Nang", 16.0678, 108.2208),
            ("17 Pham Van Dong, Thu Duc, Ho Chi Minh City", 10.8500, 106.7700)
        };
        var stations = stationSeeds.Select(s => new Station
        {
            Address = s.Address, Latitude = s.Lat, Longitude = s.Lng,
            CreatedAt = DateTime.UtcNow.AddDays(-rnd.Next(60, 180))
        }).ToList();
        db.Stations.AddRange(stations);
        await db.SaveChangesAsync();

        // ---- Cabinets + Slots + Batteries ----
        var models = new[] { "VF-SWAP-X1", "VF-SWAP-X2", "VF-SWAP-PRO" };
        var statuses = new[] { CabinetStatus.Online, CabinetStatus.Online, CabinetStatus.Online, CabinetStatus.Offline, CabinetStatus.Maintenance };
        var batteries = new List<Battery>();

        foreach (var station in stations)
        {
            var cabinetCount = rnd.Next(2, 4);
            for (var c = 0; c < cabinetCount; c++)
            {
                var cabinet = new Cabinet
                {
                    StationId = station.Id,
                    CabinetModel = models[rnd.Next(models.Length)],
                    Status = statuses[rnd.Next(statuses.Length)],
                    CreatedAt = station.CreatedAt.AddDays(rnd.Next(1, 10))
                };
                db.Cabinets.Add(cabinet);
                await db.SaveChangesAsync();

                var slotCount = rnd.Next(6, 10);
                for (var s = 1; s <= slotCount; s++)
                {
                    var slot = new Slot
                    {
                        CabinetId = cabinet.Id,
                        SlotNumber = s,
                        IsHardwareLocked = rnd.NextDouble() < 0.1
                    };
                    db.Slots.Add(slot);
                    await db.SaveChangesAsync();

                    // ~75% of slots hold a battery
                    if (rnd.NextDouble() < 0.75)
                    {
                        var soc = rnd.Next(10, 101);
                        var battery = new Battery
                        {
                            Soc = soc,
                            Temperature = Math.Round(25 + rnd.NextDouble() * 15, 1),
                            Voltage = Math.Round(48 + rnd.NextDouble() * 6, 2),
                            HealthState = rnd.NextDouble() < 0.85 ? BatteryHealthState.Good : BatteryHealthState.Degraded,
                            LocationType = BatteryLocationType.InSlot,
                            HolderId = slot.Id,
                            UpdatedAt = DateTime.UtcNow.AddMinutes(-rnd.Next(0, 600))
                        };
                        db.Batteries.Add(battery);
                        await db.SaveChangesAsync();

                        slot.CurrentBatteryId = battery.Id;
                        db.Slots.Update(slot);
                        await db.SaveChangesAsync();
                        batteries.Add(battery);
                    }
                }
            }
        }

        // ---- Rented batteries (held by users) ----
        for (var i = 0; i < 15; i++)
        {
            var battery = new Battery
            {
                Soc = rnd.Next(20, 90),
                Temperature = Math.Round(28 + rnd.NextDouble() * 12, 1),
                Voltage = Math.Round(48 + rnd.NextDouble() * 6, 2),
                HealthState = rnd.NextDouble() < 0.85 ? BatteryHealthState.Good : BatteryHealthState.Degraded,
                LocationType = BatteryLocationType.RentedByUser,
                UpdatedAt = DateTime.UtcNow.AddMinutes(-rnd.Next(0, 1200))
            };
            db.Batteries.Add(battery);
            batteries.Add(battery);
        }
        await db.SaveChangesAsync();

        // ---- Battery logs (telemetry history) ----
        foreach (var battery in batteries)
        {
            var logs = new List<BatteryLog>();
            var soc = battery.Soc;
            for (var h = 24; h >= 0; h--)
            {
                soc = Math.Clamp(soc + rnd.Next(-4, 6), 5, 100);
                logs.Add(new BatteryLog
                {
                    BatteryId = battery.Id,
                    Soc = soc,
                    Temperature = Math.Round(25 + rnd.NextDouble() * 15, 1),
                    Voltage = Math.Round(48 + rnd.NextDouble() * 6, 2),
                    RecordedAt = DateTime.UtcNow.AddHours(-h)
                });
            }
            db.BatteryLogs.AddRange(logs);
        }
        await db.SaveChangesAsync();

        // ---- Users (EV drivers) ----
        var firstNames = new[] { "Nguyen", "Tran", "Le", "Pham", "Hoang", "Vu", "Dang", "Bui", "Do", "Ho" };
        var lastNames = new[] { "An", "Binh", "Cuong", "Dung", "Em", "Phuc", "Giang", "Hieu", "Khanh", "Long" };
        var users = new List<User>();
        for (var i = 0; i < 40; i++)
        {
            users.Add(new User
            {
                FullName = $"{firstNames[rnd.Next(firstNames.Length)]} Van {lastNames[rnd.Next(lastNames.Length)]}",
                Phone = $"09{rnd.Next(10000000, 99999999)}",
                BalanceTrips = rnd.Next(0, 30),
                IsActive = rnd.NextDouble() < 0.9,
                CreatedAt = DateTime.UtcNow.AddDays(-rnd.Next(1, 200))
            });
        }
        db.Users.AddRange(users);
        await db.SaveChangesAsync();

        var cabinets = await db.Cabinets.ToListAsync();
        var slots = await db.Slots.ToListAsync();

        // ---- Swapping transactions (last 60 days) ----
        var transactions = new List<SwappingTransaction>();
        for (var i = 0; i < 300; i++)
        {
            var user = users[rnd.Next(users.Count)];
            var cabinet = cabinets[rnd.Next(cabinets.Count)];
            var cabinetSlots = slots.Where(s => s.CabinetId == cabinet.Id).ToList();
            if (cabinetSlots.Count < 2) continue;

            var slotIn = cabinetSlots[rnd.Next(cabinetSlots.Count)];
            var slotOut = cabinetSlots[rnd.Next(cabinetSlots.Count)];
            var created = DateTime.UtcNow.AddDays(-rnd.Next(0, 60)).AddMinutes(-rnd.Next(0, 1440));
            var returnedAt = created.AddSeconds(rnd.Next(10, 60));
            var success = rnd.NextDouble() < 0.92;

            transactions.Add(new SwappingTransaction
            {
                UserId = user.Id,
                CabinetId = cabinet.Id,
                SlotInId = slotIn.Id,
                BatteryInId = slotIn.CurrentBatteryId,
                ReturnedAt = returnedAt,
                SlotOutId = success ? slotOut.Id : null,
                BatteryOutId = success ? slotOut.CurrentBatteryId : null,
                DispensedAt = success ? returnedAt.AddSeconds(rnd.Next(10, 90)) : null,
                Status = success ? SwappingTransactionStatus.Success : SwappingTransactionStatus.FailedSlotOut,
                CreatedAt = created
            });
        }
        db.SwappingTransactions.AddRange(transactions);
        await db.SaveChangesAsync();

        // ---- Active sessions ----
        var activeUsers = users.Where(u => u.IsActive).OrderBy(_ => rnd.Next()).Take(5).ToList();
        foreach (var u in activeUsers)
        {
            db.ActiveSwappingSessions.Add(new ActiveSwappingSession
            {
                UserId = u.Id,
                CabinetId = cabinets[rnd.Next(cabinets.Count)].Id,
                Status = rnd.NextDouble() < 0.5 ? SwappingSessionStatus.Processing : SwappingSessionStatus.HardwareWaiting,
                CreatedAt = DateTime.UtcNow.AddMinutes(-rnd.Next(1, 30))
            });
        }
        await db.SaveChangesAsync();

        logger.LogInformation("Database seeding completed.");
    }
}
