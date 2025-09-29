using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace Application.EfaConfigs.Create;

/// <summary>
/// Validates the input data for creating a new EFA configuration.
/// Ensures the year, EFA rate, and updated-by fields are correct.
/// </summary>
internal sealed class CreateEfaConfigurationCommandValidator
    : AbstractValidator<CreateEfaConfigurationCommand>
{
    //Ensures input data is valid before sending it to the database.
    public CreateEfaConfigurationCommandValidator()
    {
        RuleFor(x => x.Year)
            .GreaterThan(1900)
            .LessThanOrEqualTo(2100)
            .WithMessage("Year must be between 1900 and 2100");

        RuleFor(x => x.EfaRate)
            .GreaterThanOrEqualTo(0)
            .WithMessage("EFA rate must be greater than or equal to 0");

        RuleFor(x => x.UpdatedBy)
            .NotEmpty()
            .WithMessage("UpdatedBy is required");
    }
}
