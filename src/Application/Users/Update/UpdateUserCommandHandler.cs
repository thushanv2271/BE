using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Users.AssignRole;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Update;

internal sealed class UpdateUserCommandHandler(
    IApplicationDbContext context,
    ICommandHandler<AssignRoleToUserCommand> assignRoleHandler
) : ICommandHandler<UpdateUserCommand>
{
    public async Task<Result> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(command.UserId));
        }


        // Update mutable fields only
        user.FirstName = command.FirstName;
        user.LastName = command.LastName;
        user.UserStatus = command.UserStatus;
        user.ModifiedAt = DateTime.UtcNow;

        // Save updated user info
        await context.SaveChangesAsync(cancellationToken);

        // Assign roles
        if (command.RoleIds is { Count: > 0 })
        {
            var assignCommand = new AssignRoleToUserCommand(user.Id, command.RoleIds);
            Result assignResult = await assignRoleHandler.Handle(assignCommand, cancellationToken);

            if (assignResult.IsFailure)
            {
                return Result.Failure(assignResult.Error);
            }

        }

        return Result.Success();
    }
}
