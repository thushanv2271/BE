using Application.Abstractions.Data;
using Domain.Permissions;
using Domain.Roles;
using Domain.RolePermissions;
using Domain.Users;
using Domain.UserRoles;
using Domain.MasterData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Infrastructure.Authentication;
using SharedKernel;
using Application.Abstractions.Authentication;
using OfficeOpenXml;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Database.Seeding;

public sealed class DatabaseSeeder(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    ILogger<DatabaseSeeder> logger)
{
    private const string AdministratorRoleName = "Administrator";
    private const string AdminEmail = "admin@saral.com";
    private const string AdminPass = "Admin123!";
    private readonly string AdminPasswordHash = passwordHasher.Hash(AdminPass);

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting database seeding...");

        await SeedPermissionsAsync(cancellationToken);
        await SeedAdministratorRoleAndUserAsync(cancellationToken);
        await SeedSegmentMasterAsync(cancellationToken);

        logger.LogInformation("Database seeding completed successfully.");
    }

    private async Task SeedPermissionsAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Seeding permissions...");

        HashSet<string> existingPermissions = await context.Permissions
            .Select(p => p.Key)
            .ToHashSetAsync(cancellationToken);

        var permissionsToAdd = PermissionRegistry.GetAllPermissions()
            .Where(permissionDef => !existingPermissions.Contains(permissionDef.Key))
            .Select(permissionDef => new Permission(
                Guid.CreateVersion7(),
                permissionDef.Key,
                permissionDef.DisplayName,
                permissionDef.Category,
                permissionDef.Description))
            .ToList();

        if (permissionsToAdd.Count > 0)
        {
            context.Permissions.AddRange(permissionsToAdd);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Added {Count} new permissions", permissionsToAdd.Count);
        }
        else
        {
            logger.LogInformation("All permissions already exist, skipping permission seeding");
        }
    }

    private async Task SeedAdministratorRoleAndUserAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Seeding administrator role and user...");

        // Create Administrator role if it doesn't exist
        Role? adminRole = await context.Roles
            .FirstOrDefaultAsync(r => r.Name == AdministratorRoleName, cancellationToken);

        if (adminRole is null)
        {
            adminRole = new Role(
                Guid.CreateVersion7(),
                AdministratorRoleName,
                "System administrator with full access to all features",
                isSystemRole: true);

            context.Roles.Add(adminRole);
            logger.LogInformation("Created Administrator role");
            await context.SaveChangesAsync(cancellationToken);
        }

        // Assign all permissions to Administrator role
        List<Permission> allPermissions = await context.Permissions.ToListAsync(cancellationToken);
        List<Guid> existingRolePermissionIds = await context.RolePermissions
            .Where(rp => rp.RoleId == adminRole.Id)
            .Select(rp => rp.PermissionId)
            .ToListAsync(cancellationToken);

        var newRolePermissions = allPermissions
            .Where(p => !existingRolePermissionIds.Contains(p.Id))
            .Select(p => new RolePermission(adminRole.Id, p.Id))
            .ToList();

        if (newRolePermissions.Count > 0)
        {
            context.RolePermissions.AddRange(newRolePermissions);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Assigned {Count} permissions to Administrator role", newRolePermissions.Count);
        }

        // Create admin user if it doesn't exist
        User? adminUser = await context.Users
            .FirstOrDefaultAsync(u => u.Email == AdminEmail, cancellationToken);

        if (adminUser is null)
        {
            adminUser = new User
            {
                Id = Guid.CreateVersion7(),
                Email = AdminEmail,
                FirstName = "System",
                LastName = "Administrator",
                PasswordHash = AdminPasswordHash
            };

            context.Users.Add(adminUser);
            logger.LogInformation("Created admin user: {Email}", AdminEmail);
            await context.SaveChangesAsync(cancellationToken);
        }

        // Assign Administrator role to admin user if not already assigned
        bool hasAdminRole = await context.UserRoles
            .AnyAsync(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id, cancellationToken);

        if (!hasAdminRole)
        {
            UserRole userRole = new(adminUser.Id, adminRole.Id);
            context.UserRoles.Add(userRole);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Assigned Administrator role to admin user");
        }
    }

    private async Task SeedSegmentMasterAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Seeding SegmentMaster data...");

        bool hasSegments = await context.SegmentMasters.AnyAsync(cancellationToken);
        if (hasSegments)
        {
            logger.LogInformation("SegmentMaster data already exists, skipping seeding.");
            return;
        }

        IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        string excelPath = config["SegmentMasterDataPath"];
        if (string.IsNullOrWhiteSpace(excelPath) || !File.Exists(excelPath))
        {
            logger.LogWarning("SegmentMasterDataPath not found or file missing: {Path}", excelPath);
            return;
        }

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using ExcelPackage package = new(excelPath);
        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

        Dictionary<string, List<string>> segmentDict = new();

        int rowCount = worksheet.Dimension.Rows;
        for (int row = 2; row <= rowCount; row++)
        {
            string segment = worksheet.Cells[row, 1].Text.Trim();
            string subsegment = worksheet.Cells[row, 2].Text.Trim();

            if (string.IsNullOrWhiteSpace(segment) || string.IsNullOrWhiteSpace(subsegment))
            {
                continue;
            }

            if (!segmentDict.TryGetValue(segment, out List<string> subSegments))
            {
                subSegments = new List<string>();
                segmentDict[segment] = subSegments;
            }

            subSegments.Add(subsegment);
        }

        var entities = segmentDict
            .Select(kvp => new SegmentMaster
            {
                Id = Guid.NewGuid(),
                Segment = kvp.Key,
                SubSegments = kvp.Value,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            })
            .ToList();

        context.SegmentMasters.AddRange(entities);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded {Count} SegmentMaster records.", entities.Count);
    }
}