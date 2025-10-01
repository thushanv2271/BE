using FluentValidation;

namespace Application.EfaConfigs.Edit;

internal sealed class EditEfaConfigurationCommandValidator
    : AbstractValidator<EditEfaConfigurationCommand>
{
    public EditEfaConfigurationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required");

        RuleFor(x => x.EfaRate)
            .GreaterThanOrEqualTo(0)
            .WithMessage("EFA rate must be greater than or equal to 0");

        RuleFor(x => x.UpdatedBy)
            .NotEmpty()
            .WithMessage("UpdatedBy is required");
    }
}
