using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.EfaConfigs;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.EfaConfigs.Create;

/// <summary>
/// Handles bulk creation/update of EFA configurations.
/// If a year exists, it updates; otherwise, it creates a new record.
/// </summary>
internal sealed class CreateBulkEfaConfigurationCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreateBulkEfaConfigurationCommand, BulkEfaConfigurationResponse>
{
    public async Task<Result<BulkEfaConfigurationResponse>> Handle(
        CreateBulkEfaConfigurationCommand command,
        CancellationToken cancellationToken)
    {
        List<EfaConfigurationSummary> created = new();
        List<EfaConfigurationSummary> updated = new();

        // Get all years from the command
        var years = command.Items.Select(i => i.Year).ToList();

        // Fetch existing configurations for these years
        List<EfaConfiguration> existingConfigs = await context.EfaConfigurations
            .Where(e => years.Contains(e.Year))
            .ToListAsync(cancellationToken);

        var existingYears = existingConfigs.ToDictionary(e => e.Year);

        foreach (EfaConfigurationItem item in command.Items)
        {
            if (existingYears.TryGetValue(item.Year, out EfaConfiguration? existing))
            {
                // Update existing
                existing.EfaRate = item.EfaRate;
                existing.UpdatedAt = dateTimeProvider.UtcNow;
                existing.UpdatedBy = command.UpdatedBy;

                updated.Add(new EfaConfigurationSummary(
                    existing.Id,
                    existing.Year,
                    existing.EfaRate,
                    existing.UpdatedAt
                ));
            }
            else
            {
                // Create new
                EfaConfiguration newConfig = new()
                {
                    Id = Guid.CreateVersion7(),
                    Year = item.Year,
                    EfaRate = item.EfaRate,
                    UpdatedAt = dateTimeProvider.UtcNow,
                    UpdatedBy = command.UpdatedBy
                };

                newConfig.Raise(new EfaConfigurationCreatedDomainEvent(newConfig.Id));
                context.EfaConfigurations.Add(newConfig);

                created.Add(new EfaConfigurationSummary(
                    newConfig.Id,
                    newConfig.Year,
                    newConfig.EfaRate,
                    newConfig.UpdatedAt
                ));
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new BulkEfaConfigurationResponse(
            created,
            updated,
            created.Count + updated.Count
        ));
    }
}
