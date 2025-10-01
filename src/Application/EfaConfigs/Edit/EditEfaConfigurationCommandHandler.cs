using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.EfaConfigs;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.EfaConfigs.Edit;

internal sealed class EditEfaConfigurationCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<EditEfaConfigurationCommand, EditEfaConfigurationResponse>
{
    public async Task<Result<EditEfaConfigurationResponse>> Handle(
        EditEfaConfigurationCommand command,
        CancellationToken cancellationToken)
    {
        EfaConfiguration? efaConfig = await context.EfaConfigurations
            .FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);

        if (efaConfig is null)
        {
            return Result.Failure<EditEfaConfigurationResponse>(
                EfaConfigurationErrors.NotFound(command.Id));
        }

        efaConfig.EfaRate = command.EfaRate;
        efaConfig.UpdatedAt = dateTimeProvider.UtcNow;
        efaConfig.UpdatedBy = command.UpdatedBy;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new EditEfaConfigurationResponse(
            efaConfig.Id,
            efaConfig.Year,
            efaConfig.EfaRate,
            efaConfig.UpdatedAt
        ));
    }
}
