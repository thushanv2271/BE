using Application.Abstractions.Messaging;
using Application.EfaConfigs.GetAll;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.EfaConfigs;

internal sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("efa-configurations", async (
            IQueryHandler<GetAllEfaConfigurationsQuery, List<EfaConfigurationResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAllEfaConfigurationsQuery();

            Result<List<EfaConfigurationResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .HasPermission(PermissionRegistry.AdminSettingsRolePermissionRead)
        .WithTags("EFA Configurations");
    }
}
