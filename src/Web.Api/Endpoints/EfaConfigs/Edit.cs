using Application.Abstractions.Messaging;
using Application.EfaConfigs.Edit;
using SharedKernel;
using System.Security.Claims;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.EfaConfigs;

internal sealed class Edit : IEndpoint
{
    public sealed record EditRequest(Guid Id, decimal EfaRate);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("efa-configurations", async (
            EditRequest request,
            HttpContext httpContext,
            ICommandHandler<EditEfaConfigurationCommand, EditEfaConfigurationResponse> handler,
            CancellationToken cancellationToken) =>
        {
            string? userIdString = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                var failureResult = Result.Failure<EditEfaConfigurationResponse>(new Error(
                    "InvalidToken",
                    "Invalid token: UserId not found",
                    ErrorType.Validation
                ));
                return CustomResults.Problem(failureResult);
            }

            var command = new EditEfaConfigurationCommand(
                request.Id,
                request.EfaRate,
                userId);

            Result<EditEfaConfigurationResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(
                response => Results.Ok(response),
                CustomResults.Problem);
        })
        .RequireAuthorization()
        .HasPermission(PermissionRegistry.AdminSettingsRolePermissionEdit)
        .WithTags("EFA Configurations");
    }
}
