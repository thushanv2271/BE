using Application.Abstractions.Messaging;
using Application.EfaConfigs.Delete;
using SharedKernel;
using System.Security.Claims;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.EfaConfigs;

internal sealed class Delete : IEndpoint
{
        public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Single delete
        app.MapDelete("efa-configurations/{id:guid}", async (
            Guid id,
            HttpContext httpContext,
            ICommandHandler<DeleteEfaConfigurationCommand, DeleteEfaConfigurationResponse> handler,
            CancellationToken cancellationToken) =>
        {
            string? userIdString = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                var failureResult = Result.Failure<DeleteEfaConfigurationResponse>(new Error(
                    "InvalidToken",
                    "Invalid token: UserId not found",
                    ErrorType.Validation
                ));
                return CustomResults.Problem(failureResult);
            }

            var command = new DeleteEfaConfigurationCommand(id, userId);

            Result<DeleteEfaConfigurationResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(
                response => Results.Ok(response),
                CustomResults.Problem);
        })
        .RequireAuthorization()
        .HasPermission(PermissionRegistry.AdminSettingsRolePermissionDelete)
        .WithTags("EFA Configurations");

    }
}
