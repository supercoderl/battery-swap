using AutoMapper;
using BatterySwap.Application.DTOs.Auth;
using BatterySwap.Application.DTOs.Batteries;
using BatterySwap.Application.DTOs.Cabinets;
using BatterySwap.Application.DTOs.Sessions;
using BatterySwap.Application.DTOs.Slots;
using BatterySwap.Application.DTOs.Stations;
using BatterySwap.Application.DTOs.Transactions;
using BatterySwap.Application.DTOs.Users;
using BatterySwap.Domain.Entities;

namespace BatterySwap.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Station
        CreateMap<Station, StationDto>()
            .ForMember(d => d.CabinetCount, o => o.MapFrom(s => s.Cabinets.Count));
        CreateMap<CreateStationDto, Station>();
        CreateMap<UpdateStationDto, Station>();

        // Cabinet
        CreateMap<Cabinet, CabinetDto>()
            .ForMember(d => d.StationAddress, o => o.MapFrom(s => s.Station != null ? s.Station.Address : string.Empty))
            .ForMember(d => d.SlotCount, o => o.MapFrom(s => s.Slots.Count))
            .ForMember(d => d.OccupiedSlots, o => o.MapFrom(s => s.Slots.Count(x => x.CurrentBatteryId != null)));
        CreateMap<CreateCabinetDto, Cabinet>();
        CreateMap<UpdateCabinetDto, Cabinet>();

        // Slot
        CreateMap<Slot, SlotDto>()
            .ForMember(d => d.CabinetModel, o => o.MapFrom(s => s.Cabinet != null ? s.Cabinet.CabinetModel : string.Empty))
            .ForMember(d => d.BatterySoc, o => o.MapFrom(s => s.CurrentBattery != null ? (int?)s.CurrentBattery.Soc : null));
        CreateMap<CreateSlotDto, Slot>();
        CreateMap<UpdateSlotDto, Slot>();

        // User
        CreateMap<User, UserDto>()
            .ForMember(d => d.TransactionCount, o => o.MapFrom(s => s.Transactions.Count));
        CreateMap<CreateUserDto, User>();
        CreateMap<UpdateUserDto, User>();

        // Battery
        CreateMap<Battery, BatteryDto>();
        CreateMap<CreateBatteryDto, Battery>();
        CreateMap<UpdateBatteryDto, Battery>();
        CreateMap<BatteryLog, BatteryLogDto>();

        // Session
        CreateMap<ActiveSwappingSession, ActiveSessionDto>()
            .ForMember(d => d.UserFullName, o => o.MapFrom(s => s.User != null ? s.User.FullName : string.Empty))
            .ForMember(d => d.CabinetModel, o => o.MapFrom(s => s.Cabinet != null ? s.Cabinet.CabinetModel : string.Empty));

        // Transaction
        CreateMap<SwappingTransaction, TransactionDto>()
            .ForMember(d => d.UserFullName, o => o.MapFrom(s => s.User != null ? s.User.FullName : string.Empty))
            .ForMember(d => d.CabinetModel, o => o.MapFrom(s => s.Cabinet != null ? s.Cabinet.CabinetModel : string.Empty))
            .ForMember(d => d.DurationSeconds, o => o.MapFrom(s =>
                (s.ReturnedAt != null && s.DispensedAt != null)
                    ? (double?)(s.DispensedAt.Value - s.ReturnedAt.Value).TotalSeconds
                    : null));

        // Account
        CreateMap<Account, AccountDto>();
    }
}
