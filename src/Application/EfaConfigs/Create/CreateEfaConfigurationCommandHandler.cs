using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.EfaConfigs;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.EfaConfigs.Create;

/// <summary>
/// Handles the command for creating a new EFA configuration.
/// </summary>
/// <param name="command">
/// The command containing details of the EFA configuration to create,
/// such as year, rate, and updated information.
/// </param>
/// <param name="cancellationToken">
/// Token to stop the process if the request is cancelled.
/// </param>
/// <returns>
/// A <see cref="Result{T}"/> with the ID of the created EFA configuration,
/// or a failure if a configuration for the same year already exists.
/// </returns>
internal sealed class CreateEfaConfigurationCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreateEfaConfigurationCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateEfaConfigurationCommand command,
        CancellationToken cancellationToken)
    {
        // Check if configuration for this year already exists
        bool yearExists = await context.EfaConfigurations
            .AnyAsync(e => e.Year == command.Year, cancellationToken);

        if (yearExists)
        {
            return Result.Failure<Guid>(
                EfaConfigurationErrors.YearAlreadyExists(command.Year));
        }

        var efaConfiguration = new EfaConfiguration
        {
            Id = Guid.CreateVersion7(),
            Year = command.Year,
            EfaRate = command.EfaRate,
            UpdatedAt = dateTimeProvider.UtcNow,
            UpdatedBy = command.UpdatedBy
        };

        efaConfiguration.Raise(new EfaConfigurationCreatedDomainEvent(efaConfiguration.Id));

        context.EfaConfigurations.Add(efaConfiguration);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(efaConfiguration.Id);
    }
}
