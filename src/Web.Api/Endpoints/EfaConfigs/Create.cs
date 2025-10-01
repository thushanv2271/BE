using Application.Abstractions.Messaging;
using Application.EfaConfigs.Create;
using SharedKernel;
using System.Security.Claims;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.EfaConfigs;

internal sealed class Create : IEndpoint
{
    public sealed record CreateRequest(int Year, decimal EfaRate);
    public sealed record CreateBulkRequest(List<EfaConfigurationItemDto> Items);
    public sealed record EfaConfigurationItemDto(int Year, decimal EfaRate);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Single create (kept for backward compatibility)
        app.MapPost("efa-configurations", async (
            CreateRequest request,
            HttpContext httpContext,
            ICommandHandler<CreateEfaConfigurationCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            string? userIdString = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                var failureResult = Result.Failure<Guid>(new Error(
                    "InvalidToken",
                    "Invalid token: UserId not found",
                    ErrorType.Validation
                ));
                return CustomResults.Problem(failureResult);
            }

            var command = new CreateEfaConfigurationCommand(
                request.Year,
                request.EfaRate,
                userId);

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(
                id => Results.Created($"/efa-configurations/{id}", new { id }),
                CustomResults.Problem);
        })
        .RequireAuthorization()
        .HasPermission(PermissionRegistry.AdminSettingsRolePermissionCreate)
        .WithTags("EFA Configurations");

        // Bulk save/update
        app.MapPost("efa-configurations/bulk", async (
            CreateBulkRequest request,
            HttpContext httpContext,
            ICommandHandler<CreateBulkEfaConfigurationCommand, BulkEfaConfigurationResponse> handler,
            CancellationToken cancellationToken) =>
        {
            string? userIdString = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                var failureResult = Result.Failure<BulkEfaConfigurationResponse>(new Error(
                    "InvalidToken",
                    "Invalid token: UserId not found",
                    ErrorType.Validation
                ));
                return CustomResults.Problem(failureResult);
            }

            var items = request.Items
                .Select(i => new EfaConfigurationItem(i.Year, i.EfaRate))
                .ToList();

            var command = new CreateBulkEfaConfigurationCommand(items, userId);

            Result<BulkEfaConfigurationResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(
                response => Results.Ok(response),
                CustomResults.Problem);
        })
        .RequireAuthorization()
        .HasPermission(PermissionRegistry.AdminSettingsRolePermissionCreate)
        .WithTags("EFA Configurations");
    }
}
