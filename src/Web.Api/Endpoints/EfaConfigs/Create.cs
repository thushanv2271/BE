using Application.Abstractions.Messaging;
using Application.EfaConfigs.Create;
using SharedKernel;
using System.Security.Claims;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.EfaConfigs;

internal sealed class Create : IEndpoint
{
    public sealed record EfaConfigurationItemDto(int Year, decimal EfaRate);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // save/update - now accepts array directly
        app.MapPost("efa-configurations", async (
            List<EfaConfigurationItemDto> request,
            HttpContext httpContext,
            ICommandHandler<CreateEfaConfigurationCommand, EfaConfigurationResponse> handler,
            CancellationToken cancellationToken) =>
        {
            string? userIdString = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                var failureResult = Result.Failure<EfaConfigurationResponse>(new Error(
                    "InvalidToken",
                    "Invalid token: UserId not found",
                    ErrorType.Validation
                ));
                return CustomResults.Problem(failureResult);
            }

            var items = request
                .Select(i => new EfaConfigurationItem(i.Year, i.EfaRate))
                .ToList();

            var command = new CreateEfaConfigurationCommand(items, userId);

            Result<EfaConfigurationResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(
                response => Results.Ok(response.Created.Concat(response.Updated).ToList()),
                CustomResults.Problem);
        })
        .RequireAuthorization()
        .HasPermission(PermissionRegistry.AdminSettingsRolePermissionCreate)
        .WithTags("EFA Configurations");
    }
}
