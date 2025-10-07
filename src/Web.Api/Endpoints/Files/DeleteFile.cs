using System.Security.Claims;
using Application.Abstractions.Messaging;
using Application.Files.DeleteFile;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Files;

/// <summary>
/// Endpoint for deleting uploaded files.
/// Removes both the physical file from storage and metadata from the database.
/// </summary>
internal sealed class DeleteFile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/files/{id:guid}", async (
            Guid id,
            HttpContext httpContext,
            ICommandHandler<DeleteFileCommand, DeleteFileResponse> handler,
            CancellationToken cancellationToken) =>
        {
            // Extract and validate UserId from JWT token
            string? userIdString = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                var failure = Result.Failure<DeleteFileResponse>(new Error(
                    "InvalidToken",
                    "Invalid token: UserId not found",
                    ErrorType.Validation
                ));
                return CustomResults.Problem(failure);
            }

            // Create delete command
            var command = new DeleteFileCommand(id, userId);

            // Execute command via handler
            Result<DeleteFileResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(
                data => Results.Ok(new
                {
                    Message = "File deleted successfully",
                    Data = data
                }),
                CustomResults.Problem
            );
        })
        .RequireAuthorization()
        .HasPermission(PermissionRegistry.PDSetupAccess)
        .WithTags("Files")
        .WithName("DeleteFile");
    }
}
