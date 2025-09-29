using Application.Abstractions.Messaging;
using Application.EfaConfigs.Create;
using SharedKernel;
using System.Security.Claims;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.EfaConfigs;

/// <summary>
/// Endpoint for creating a new EFA configuration.
/// Maps a POST request to handle creating an EFA configuration using the command handler.
/// </summary>
internal sealed class Create : IEndpoint
{
    public sealed record CreateRequest(int Year, decimal EfaRate);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("efa-configurations", async (
            CreateRequest request,
            HttpContext httpContext,
            ICommandHandler<CreateEfaConfigurationCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            // Extract UserId from token claims
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
            // Create the command from request + userId
            var command = new CreateEfaConfigurationCommand(
                request.Year,
                request.EfaRate,
                userId);

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            //Map result to HTTP response
            return result.Match(
                id => Results.Created($"/efa-configurations/{id}", new { id }),
                CustomResults.Problem);
        })
        .RequireAuthorization()
        .HasPermission(PermissionRegistry.AdminSettingsRolePermissionCreate)
        .WithTags("EFA Configurations");
    }
}
