using BatterySwap.Application.DTOs.Batteries;
using BatterySwap.Application.DTOs.Cabinets;
using BatterySwap.Application.DTOs.Slots;
using BatterySwap.Application.DTOs.Users;
using FluentValidation;

namespace BatterySwap.Application.Validators;

public class CreateCabinetValidator : AbstractValidator<CreateCabinetDto>
{
    public CreateCabinetValidator()
    {
        RuleFor(x => x.StationId).GreaterThan(0);
        RuleFor(x => x.CabinetModel).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Status).IsInEnum();
    }
}

public class UpdateCabinetValidator : AbstractValidator<UpdateCabinetDto>
{
    public UpdateCabinetValidator()
    {
        RuleFor(x => x.StationId).GreaterThan(0);
        RuleFor(x => x.CabinetModel).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Status).IsInEnum();
    }
}

public class CreateSlotValidator : AbstractValidator<CreateSlotDto>
{
    public CreateSlotValidator()
    {
        RuleFor(x => x.CabinetId).GreaterThan(0);
        RuleFor(x => x.SlotNumber).GreaterThan(0);
    }
}

public class UpdateSlotValidator : AbstractValidator<UpdateSlotDto>
{
    public UpdateSlotValidator()
    {
        RuleFor(x => x.SlotNumber).GreaterThan(0);
    }
}

public class CreateUserValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Phone).NotEmpty().Matches(@"^[0-9+\-\s]{6,20}$").WithMessage("Invalid phone number.");
        RuleFor(x => x.BalanceTrips).GreaterThanOrEqualTo(0);
    }
}

public class UpdateUserValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Phone).NotEmpty().Matches(@"^[0-9+\-\s]{6,20}$").WithMessage("Invalid phone number.");
        RuleFor(x => x.BalanceTrips).GreaterThanOrEqualTo(0);
    }
}

public class CreateBatteryValidator : AbstractValidator<CreateBatteryDto>
{
    public CreateBatteryValidator()
    {
        RuleFor(x => x.Soc).InclusiveBetween(0, 100);
        RuleFor(x => x.Temperature).InclusiveBetween(-40, 150);
        RuleFor(x => x.Voltage).InclusiveBetween(0, 100);
        RuleFor(x => x.HealthState).IsInEnum();
        RuleFor(x => x.LocationType).IsInEnum();
    }
}

public class UpdateBatteryValidator : AbstractValidator<UpdateBatteryDto>
{
    public UpdateBatteryValidator()
    {
        RuleFor(x => x.Soc).InclusiveBetween(0, 100);
        RuleFor(x => x.Temperature).InclusiveBetween(-40, 150);
        RuleFor(x => x.Voltage).InclusiveBetween(0, 100);
        RuleFor(x => x.HealthState).IsInEnum();
        RuleFor(x => x.LocationType).IsInEnum();
    }
}
