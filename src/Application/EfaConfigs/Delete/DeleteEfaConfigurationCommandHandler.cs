using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.EfaConfigs;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.EfaConfigs.Delete;

internal sealed class DeleteEfaConfigurationCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<DeleteEfaConfigurationCommand, DeleteEfaConfigurationResponse>
{
    public async Task<Result<DeleteEfaConfigurationResponse>> Handle(
        DeleteEfaConfigurationCommand command,
        CancellationToken cancellationToken)
    {
        EfaConfiguration? efaConfig = await context.EfaConfigurations
            .FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);

        if (efaConfig is null)
        {
            return Result.Failure<DeleteEfaConfigurationResponse>(
                EfaConfigurationErrors.NotFound(command.Id));
        }

        context.EfaConfigurations.Remove(efaConfig);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new DeleteEfaConfigurationResponse(
            efaConfig.Id,
            efaConfig.Year,
            dateTimeProvider.UtcNow
        ));
    }
}
