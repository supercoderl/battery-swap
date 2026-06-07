using BatterySwap.Application.DTOs.Stations;
using FluentValidation;

namespace BatterySwap.Application.Validators;

public class CreateStationValidator : AbstractValidator<CreateStationDto>
{
    public CreateStationValidator()
    {
        RuleFor(x => x.Address).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
    }
}

public class UpdateStationValidator : AbstractValidator<UpdateStationDto>
{
    public UpdateStationValidator()
    {
        RuleFor(x => x.Address).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
    }
}
