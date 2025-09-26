using Domain.Todos;
using Domain.Authentication;
using Domain.Users;
using Domain.PasswordResetTokens;
using Domain.Permissions;
using Domain.Roles;
using Domain.UserRoles;
using Domain.RolePermissions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using Domain.Exports;
using Domain.PDTempData;
using Domain.MasterData;
using Domain.Files;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<TodoItem> TodoItems { get; }

    DbSet<PasswordResetToken> PasswordResetTokens { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    // Role-based permission system
    DbSet<Permission> Permissions { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<ExportAudit> ExportAudits { get; }

    DbSet<PDTempData> PDTempDatas { get; }

    DbSet<SegmentMaster> SegmentMasters { get; }

    DbSet<UploadedFile> UploadedFiles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
