using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.EfaConfigs.GetAll;
/// <summary>
/// Handles the query to get all EFA configurations from the database.
/// Returns a list of <see cref="EfaConfigurationResponse"/> ordered by year (most recent first).
/// </summary>
internal sealed class GetAllEfaConfigurationsQueryHandler(
    IApplicationDbContext context)
    : IQueryHandler<GetAllEfaConfigurationsQuery, List<EfaConfigurationResponse>>
{
    public async Task<Result<List<EfaConfigurationResponse>>> Handle(
        GetAllEfaConfigurationsQuery query,
        CancellationToken cancellationToken)
    {
        List<EfaConfigurationResponse> configurations = await context.EfaConfigurations
            .AsNoTracking()                       //improves performance since we don't need EF to track changes
            .OrderByDescending(e => e.Year)       
            .Select(e => new EfaConfigurationResponse
            {
                Id = e.Id,
                Year = e.Year,
                EfaRate = e.EfaRate,
                UpdatedAt = e.UpdatedAt,
                UpdatedBy = e.UpdatedBy
            })
            .ToListAsync(cancellationToken);

        return Result.Success(configurations);
    }
}
