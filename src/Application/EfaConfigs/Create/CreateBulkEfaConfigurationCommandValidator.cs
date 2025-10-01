using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using FluentValidation;

namespace Application.EfaConfigs.Create;

internal sealed class CreateBulkEfaConfigurationCommandValidator
    : AbstractValidator<CreateBulkEfaConfigurationCommand>
{
    public CreateBulkEfaConfigurationCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("At least one EFA configuration item is required");

        RuleFor(x => x.UpdatedBy)
            .NotEmpty()
            .WithMessage("UpdatedBy is required");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Year)
                .GreaterThan(1900)
                .LessThanOrEqualTo(2100)
                .WithMessage("Year must be between 1900 and 2100");

            item.RuleFor(x => x.EfaRate)
                .GreaterThanOrEqualTo(0)
                .WithMessage("EFA rate must be greater than or equal to 0");
        });

        // Check for duplicate years in the same request
        RuleFor(x => x.Items)
            .Must(items => items.Select(i => i.Year).Distinct().Count() == items.Count)
            .WithMessage("Duplicate years are not allowed in the same request");
    }
}
