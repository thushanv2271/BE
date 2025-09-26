# Hierarchical Claim-Based Authentication System

## Overview

This document provides comprehensive documentation for the hierarchical claim-based authentication and authorization system implemented in the ASP.NET Core 8 application. The system provides fine-grained permission control with role-based access, in-memory caching, complete REST API for management, and extensive HTTP testing capabilities.

### Key Features

- **Hierarchical Permissions**: Dot-notation permission system (e.g., `Admin.User.Management.Create`)
- **Role-Based Access Control**: Users inherit permissions through roles
- **Direct Permission Assignment**: Additional granular control via direct user permissions
- **JWT Integration**: Seamless integration with existing JWT authentication
- **Performance Optimization**: In-memory caching with intelligent invalidation
- **Comprehensive API**: Full REST endpoints for all operations
- **Extensive Testing**: HTTP test files with automated token management and SSL handling
- **Auto-Seeding**: Automatic database seeding with admin user and permissions

### Quick Start

For developers who want to start testing immediately:

1. **Run the Application**: `dotnet run --project src/Web.Api`
2. **Open Test Files**: Navigate to `tests/HttpTests/QuickUserTests.http`
3. **Login as Admin**: Use credentials `admin@saral.com` / `Admin123!`
4. **Test Endpoints**: Execute HTTP requests using VS Code REST Client
5. **View API Documentation**: Visit `https://localhost:5001/swagger`

### System Access Levels

- **Public**: Registration, login endpoints
- **Authenticated**: Basic user operations (requires valid JWT)
- **Permission-Based**: Protected operations (requires specific permissions)
- **Admin-Only**: User/role management (requires Admin permissions)

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Domain Model](#domain-model)
3. [Permission System](#permission-system)
4. [Database Schema](#database-schema)
5. [Application Layer](#application-layer)
6. [Web API Endpoints](#web-api-endpoints)
7. [Authentication & Authorization](#authentication--authorization)
8. [Caching Strategy](#caching-strategy)
9. [Database Seeding](#database-seeding)
10. [HTTP API Testing](#http-api-testing)
11. [Usage Examples](#usage-examples)
12. [Configuration](#configuration)
13. [Testing](#testing)
14. [Troubleshooting](#troubleshooting)

## Architecture Overview

The system follows Clean Architecture principles with clear separation of concerns:

```
+-------------------+    +-------------------+    +-------------------+
|     Web.Api       |--->|   Application     |--->|      Domain       |
|   (Endpoints)     |    |   (Use Cases)     |    |    (Entities)     |
+-------------------+    +-------------------+    +-------------------+
        |                       |                         ^
        v                       v                         |
+-------------------+    +-------------------+            |
|  Infrastructure   |--->|   SharedKernel    |------------+
|  (Data, Auth)     |    |     (Common)      |
+-------------------+    +-------------------+

```

### Key Components

- **Domain Layer**: Core business entities and rules
- **Application Layer**: Use cases, commands, queries, and abstractions
- **Infrastructure Layer**: Data access, authentication, and external services
- **Web.Api Layer**: REST endpoints and presentation logic

## Domain Model

### Core Entities

#### Permission
```csharp
public sealed class Permission : Entity
{
    public Guid Id { get; private set; }
    public string Key { get; private set; }           // e.g., "Admin.User.Management.Create"
    public string DisplayName { get; private set; }   // e.g., "Create Users"
    public string Category { get; private set; }      // e.g., "Admin"
    public string? Description { get; private set; }  // Optional description
}
```

#### Role
```csharp
public sealed class Role : Entity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }          // e.g., "Administrator"
    public string Description { get; private set; }   // Role description
    public bool IsSystemRole { get; private set; }    // Cannot be deleted
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }
}
```

#### UserRole (Junction Entity)
```csharp
public sealed class UserRole : Entity
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public DateTime AssignedAt { get; private set; }
}
```

#### RolePermission (Junction Entity)
```csharp
public sealed class RolePermission : Entity
{
    public Guid Id { get; private set; }
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }
    public DateTime AssignedAt { get; private set; }
}
```

#### UserPermission (Direct Assignment)
```csharp
public sealed class UserPermission : Entity
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid PermissionId { get; private set; }
    public DateTime AssignedAt { get; private set; }
}
```

## Permission System

### Hierarchical Permission Structure

Permissions follow a hierarchical dot-notation format:

```
{Category}.{Module}.{Action}
```

Examples (from your current PermissionRegistry):
- `Admin.Dashboard.Read`
- `Admin.UserManagement.Create`
- `Admin.UserManagement.Read`
- `Admin.UserManagement.Edit`
- `Admin.UserManagement.Delete`
- `Admin.Settings.Profile.Read`
- `Admin.Settings.Profile.Edit`
- `Admin.Settings.Password.Change`
- `Admin.Settings.RolePermission.Create`
- `Admin.Settings.RolePermission.Read`
- `Admin.Settings.RolePermission.Edit`
- `Admin.Settings.RolePermission.Delete`
- `Users.Access`

### Permission Registry

All permissions are defined in `PermissionRegistry.cs`:

```csharp
public static class PermissionRegistry
{
    // Category definitions
    public static readonly CategoryInfo CategoryAdminDashboard = new() { Category = "Dashboard", CategoryName = "Dashboard" };
    public static readonly CategoryInfo CategoryAdminUserManagement = new() { Category = "UserManagement", CategoryName = "User Management" };
    public static readonly CategoryInfo CategoryAdminSettingsProfile = new() { Category = "Profile", CategoryName = "Profile" };
    public static readonly CategoryInfo CategoryAdminSettingsPassword = new() { Category = "Password", CategoryName = "Password" };
    public static readonly CategoryInfo CategoryAdminSettingsRolePermission = new() { Category = "RolePermission", CategoryName = "Role Permission" };
    public static readonly CategoryInfo CategoryUsers = new() { Category = "Users", CategoryName = "Users" };

    // Permission keys
    public const string AdminDashboardRead = "Admin.Dashboard.Read";
    public const string AdminUserManagementRead = "Admin.UserManagement.Read";
    public const string AdminUserManagementEdit = "Admin.UserManagement.Edit";
    public const string AdminUserManagementDelete = "Admin.UserManagement.Delete";
    public const string AdminUserManagementCreate = "Admin.UserManagement.Create";
    public const string AdminSettingsProfileRead = "Admin.Settings.Profile.Read";
    public const string AdminSettingsProfileEdit = "Admin.Settings.Profile.Edit";
    public const string AdminSettingsPasswordChange = "Admin.Settings.Password.Change";
    public const string AdminSettingsRolePermissionRead = "Admin.Settings.RolePermission.Read";
    public const string AdminSettingsRolePermissionEdit = "Admin.Settings.RolePermission.Edit";
    public const string AdminSettingsRolePermissionDelete = "Admin.Settings.RolePermission.Delete";
    public const string AdminSettingsRolePermissionCreate = "Admin.Settings.RolePermission.Create";
    public const string UsersAccess = "Users.Access";

    // Get all permissions with metadata
    public static IReadOnlyList<PermissionDefinition> GetAllPermissions() { ... }
}
```

Each permission is described with a display name, category, and description. For example:

```csharp
new(AdminUserManagementCreate, "Create User", CategoryAdminUserManagement.Category, CategoryAdminUserManagement.CategoryName, "Allows creating new users")
```

### PermissionDefinition Structure

```csharp
public sealed record PermissionDefinition(
    string Key,
    string DisplayName,
    string Category,
    string CategoryName,
    string Description
);
```

### Permission Categories

- **Dashboard**: `Admin.Dashboard.Read`
- **User Management**: `Admin.UserManagement.Create`, `Admin.UserManagement.Read`, `Admin.UserManagement.Edit`, `Admin.UserManagement.Delete`
- **Profile Settings**: `Admin.Settings.Profile.Read`, `Admin.Settings.Profile.Edit`
- **Password Settings**: `Admin.Settings.Password.Change`
- **Role Permission Settings**: `Admin.Settings.RolePermission.Create`, `Admin.Settings.RolePermission.Read`, `Admin.Settings.RolePermission.Edit`, `Admin.Settings.RolePermission.Delete`
- **General User**: `Users.Access`

### Permission Validation

The registry provides a method to validate permission keys:

```csharp
public static bool IsValidPermission(string permissionKey)
```

### Grouping by Category

Permissions can be grouped by category for UI display:

```csharp
public static IReadOnlyDictionary<string, IReadOnlyList<PermissionDefinition>> GetPermissionsByCategory()
```

---

**Note:**  
Update any references to old permission keys or categories in your documentation, tests, and API descriptions to match the new structure and naming conventions from your `PermissionRegistry.cs`.

## Database Schema

### Tables Structure

```sql
-- Core permission table
permissions (
    id UUID PRIMARY KEY,
    key VARCHAR(255) UNIQUE NOT NULL,
    display_name VARCHAR(255) NOT NULL,
    category VARCHAR(100) NOT NULL,
    description TEXT
);

-- Role definitions
roles (
    id UUID PRIMARY KEY,
    name VARCHAR(255) UNIQUE NOT NULL,
    description TEXT NOT NULL,
    is_system_role BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW()
);

-- User-Role assignments
user_roles (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id),
    role_id UUID NOT NULL REFERENCES roles(id),
    assigned_at TIMESTAMP DEFAULT NOW(),
    UNIQUE(user_id, role_id)
);

-- Role-Permission assignments
role_permissions (
    id UUID PRIMARY KEY,
    role_id UUID NOT NULL REFERENCES roles(id),
    permission_id UUID NOT NULL REFERENCES permissions(id),
    assigned_at TIMESTAMP DEFAULT NOW(),
    UNIQUE(role_id, permission_id)
);

-- Direct User-Permission assignments
user_permissions (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id),
    permission_id UUID NOT NULL REFERENCES permissions(id),
    assigned_at TIMESTAMP DEFAULT NOW(),
    UNIQUE(user_id, permission_id)
);
```

### Indexes for Performance

```sql
-- Optimized indexes for permission resolution
CREATE INDEX ix_user_roles_user_id ON user_roles(user_id);
CREATE INDEX ix_role_permissions_role_id ON role_permissions(role_id);
CREATE INDEX ix_user_permissions_user_id ON user_permissions(user_id);
CREATE INDEX ix_permissions_key ON permissions(key);
CREATE INDEX ix_permissions_category ON permissions(category);
```

## Application Layer

### Commands and Queries

#### Permission Queries

```csharp
// Get user's effective permissions
public sealed record GetUserEffectivePermissionsQuery(Guid UserId) : IQuery<HashSet<string>>;

// Get permission tree with isPresent flags
public sealed record GetPermissionTreeQuery(Guid UserId) : IQuery<PermissionTreeResponse>;
```

#### Role Management Commands

```csharp
// Create new role
public sealed record CreateRoleCommand(
    string Name,
    string? Description,
    IReadOnlyList<string> PermissionKeys) : ICommand<Guid>;

// Get role details
public sealed record GetRoleByIdQuery(Guid RoleId) : IQuery<RoleResponse>;

// List all roles
public sealed record GetRolesQuery : IQuery<List<RoleListResponse>>;
```

#### User Role Management Commands

```csharp
// Assign role to user
public sealed record AssignRoleToUserCommand(Guid UserId, Guid RoleId) : ICommand;

// Remove role from user
public sealed record RemoveRoleFromUserCommand(Guid UserId, Guid RoleId) : ICommand;
```

### Response Models

#### Permission Tree Response
```csharp
public sealed record PermissionTreeResponse(IReadOnlyList<PermissionTreeNode> Permissions);

public sealed record PermissionTreeNode(
    string Key,
    string DisplayName,
    string Category,
    string? Description,
    bool IsPresent);
```

#### Role Responses
```csharp
public sealed record RoleResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsSystemRole,
    DateTime CreatedAt,
    IReadOnlyList<string> PermissionKeys);

public sealed record RoleListResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsSystemRole,
    DateTime CreatedAt,
    int PermissionCount);
```

## Web API Endpoints

### Permission Management

#### GET /permissions/tree/{userId}
Returns all permissions with `isPresent` flags indicating if the user has each permission.

**Authorization**: Requires `Admin.Permission.Management.View`

**Response**:
```json
{
  "permissions": [
    {
      "key": "Admin.User.Management.Create",
      "displayName": "Create Users",
      "category": "Admin",
      "description": "Allows creating new user accounts",
      "isPresent": true
    }
  ]
}
```

#### GET /users/{userId}/permissions
Returns user's effective permissions as a string array.

**Authorization**: Requires `Admin.User.Management.View`

**Response**:
```json
[
  "Admin.User.Management.Create",
  "Admin.User.Management.View",
  "Finance.Invoice.Management.View"
]
```

### Role Management

#### POST /roles
Creates a new role with specified permissions.

**Authorization**: Requires `Admin.Role.Management.Create`

**Request**:
```json
{
  "name": "Finance Manager",
  "description": "Manages financial operations",
  "permissionKeys": [
    "Finance.Invoice.Management.Create",
    "Finance.Invoice.Management.View",
    "Finance.Report.Management.View"
  ]
}
```

**Response**: `201 Created` with role ID

#### GET /roles
Lists all roles with summary information.

**Authorization**: Requires `Admin.Role.Management.Create`

**Response**:
```json
[
  {
    "id": "guid",
    "name": "Administrator",
    "description": "Full system access",
    "isSystemRole": true,
    "createdAt": "2025-07-23T10:00:00Z",
    "permissionCount": 52
  }
]
```

#### GET /roles/{roleId}
Gets detailed information about a specific role.

**Authorization**: Requires `Admin.Role.Management.Create`

**Response**:
```json
{
  "id": "guid",
  "name": "Finance Manager",
  "description": "Manages financial operations",
  "isSystemRole": false,
  "createdAt": "2025-07-23T10:00:00Z",
  "permissionKeys": [
    "Finance.Invoice.Management.Create",
    "Finance.Invoice.Management.View"
  ]
}
```

### User Role Management

#### POST /users/{userId}/roles
Assigns a role to a user.

**Authorization**: Requires `Admin.User.Management.View`

**Request**:
```json
{
  "roleId": "guid"
}
```

**Response**: `200 OK` with success message

#### DELETE /users/{userId}/roles/{roleId}
Removes a role from a user.

**Authorization**: Requires `Admin.User.Management.View`

**Response**: `200 OK` with success message

## Authentication & Authorization

### JWT Token Integration

The system integrates with existing JWT authentication:

```csharp
// Permission-based authorization
app.MapGet("admin/users", handler)
   .RequireAuthorization()
   .HasPermission("Admin.User.Management.View");
```

### Permission Authorization Handler

```csharp
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Reject unauthenticated users
        if (context.User?.Identity?.IsAuthenticated != true)
            return;

        // Get user permissions from cache/database
        var userId = context.User.GetUserId();
        var permissions = await permissionProvider.GetForUserIdAsync(userId);

        // Check if user has required permission
        if (permissions.Contains(requirement.Permission))
            context.Succeed(requirement);
    }
}
```

### HasPermission Extension

```csharp
public static RouteHandlerBuilder HasPermission(
    this RouteHandlerBuilder builder, 
    string permission)
{
    return builder.RequireAuthorization(policy => 
        policy.Requirements.Add(new PermissionRequirement(permission)));
}
```

## Caching Strategy

### In-Memory Caching

User permissions are cached in memory for 15 minutes to optimize performance:

```csharp
public class PermissionProvider
{
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(15);

    public async Task<HashSet<string>> GetForUserIdAsync(Guid userId)
    {
        string cacheKey = $"user_permissions_{userId}";
        
        if (memoryCache.TryGetValue(cacheKey, out HashSet<string>? cached))
            return cached;

        var permissions = await ResolveUserPermissions(userId);
        memoryCache.Set(cacheKey, permissions, CacheExpiration);
        
        return permissions;
    }
}
```

### Cache Invalidation

Cache is automatically invalidated when user roles change:

```csharp
// In AssignRoleToUserCommandHandler
await dbContext.SaveChangesAsync(cancellationToken);
permissionCache.InvalidateUserPermissions(request.UserId);
```

### Cache Abstraction

```csharp
public interface IPermissionCacheService
{
    void InvalidateUserPermissions(Guid userId);
    void InvalidateAllUserPermissions();
}
```

## HTTP API Testing

### Overview

The system includes comprehensive HTTP test files for testing all authentication and authorization endpoints. These files provide automated testing capabilities with token management and SSL certificate handling.

### Test Files Location

```
tests/HttpTests/
├── QuickUserTests.http                    # Quick test scenarios
├── QuickUserTests-HTTP.http              # HTTP-only version (port 5000)
├── UsermanagementAndPermissionTests.http # Comprehensive test suite
└── README.md                             # Testing documentation
```

### Quick Testing Setup

#### QuickUserTests.http

This file provides a streamlined way to test basic functionality:

```http
### Quick Test Setup for User Management
# @no-reject-unauthorized
@baseUrl = https://localhost:5001

### Step 1: Login as Admin
POST {{baseUrl}}/users/login
Content-Type: application/json

{
  "email": "admin@saral.com",
  "password": "Admin123!"
}

### Step 2: Get All Users (using token from Step 1)
GET {{baseUrl}}/users
Authorization: Bearer {{token}}

### Step 3: Get Permission Tree
GET {{baseUrl}}/permissions/tree/{{adminUserId}}
Authorization: Bearer {{token}}

### Step 4: Register Test User
POST {{baseUrl}}/users/register
Content-Type: application/json

{
  "email": "quicktest@saral.com",
  "firstName": "Quick",
  "lastName": "Test",
  "password": "QuickTest123!"
}

### Step 5: Assign Permission to Test User
POST {{baseUrl}}/users/{{testUserId}}/permissions
Content-Type: application/json
Authorization: Bearer {{token}}

{
  "permissionKey": "Admin.Permission.Management.View"
}

### Step 6: Get User Direct Permissions
GET {{baseUrl}}/users/{{testUserId}}/permissions/direct
Authorization: Bearer {{token}}

### Step 7: Revoke Permission
DELETE {{baseUrl}}/users/{{testUserId}}/permissions/Admin.Permission.Management.View
Authorization: Bearer {{token}}
```

#### Comprehensive Test Suite

The `UsermanagementAndPermissionTests.http` file includes:

1. **Authentication Tests**
   - Admin login with token storage
   - Refresh token validation
   - Automated token management

2. **User Management Tests**
   - Get all users (with AdminUserManagementRead permission)
   - Get user by ID
   - User registration
   - User permission assignment/revocation

3. **Permission Management Tests**
   - Get permission tree
   - Direct permission queries
   - Permission validation

4. **Role Management Tests**
   - Create roles with permissions
   - Assign/remove roles to/from users
   - Role-based permission inheritance

5. **Error Handling Tests**
   - Unauthorized access attempts
   - Invalid permission handling
   - Non-existent user scenarios

### Test Automation Features

#### Global Variable Management

```http
### Global Variables (automatically managed)
@accessToken = 
@refreshToken = 
@userId = 
@testUserId = 
@testPermissionKey = Admin.Permission.Management.View

> {%
  // Automatic token storage after login
  client.global.set("accessToken", response.body.accessToken);
  client.global.set("refreshToken", response.body.refreshToken);
  client.global.set("userId", response.body.user.id);
%}
```

#### Automated Assertions

```http
> {%
  client.test("Login successful", function() {
    client.assert(response.status === 200, "Response status is not 200");
    client.assert(response.body.accessToken, "Access token not found");
    client.assert(response.body.refreshToken, "Refresh token not found");
  });
%}
```

#### SSL Certificate Handling

For development environments with self-signed certificates:

```http
# Add this directive to bypass SSL certificate validation
# @no-reject-unauthorized
@baseUrl = https://localhost:5001
```

Alternative HTTP-only version available at `http://localhost:5000`

### Running the Tests

#### Prerequisites

1. **Application Running**: Ensure the API is running on `https://localhost:5001` or `http://localhost:5000`
2. **Database Seeded**: Admin user must exist with email `admin@saral.com` and password `Admin123!`
3. **SSL Certificates**: Either valid certificates or use `@no-reject-unauthorized` directive

#### Using VS Code REST Client

1. Install the **REST Client** extension in VS Code
2. Open any `.http` file in the `tests/HttpTests/` directory
3. Click "Send Request" above each HTTP request
4. Use global variables for token management across requests

#### Using IntelliJ/WebStorm HTTP Client

1. Open `.http` files in IntelliJ IDEA or WebStorm
2. Use the built-in HTTP client to execute requests
3. Variables are automatically managed between requests

#### Using curl (Alternative)

```bash
# Step 1: Login and capture token
curl -X POST https://localhost:5001/users/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@saral.com","password":"Admin123!"}' \
  -k

# Step 2: Use token in subsequent requests
curl -X GET https://localhost:5001/users \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -k
```

### Test Scenarios

#### Full Authentication Cycle

1. **Login** → Get access token and refresh token
2. **Token Usage** → Access protected endpoints
3. **Token Refresh** → Extend session without re-login
4. **User Management** → Create, read, update user permissions
5. **Role Management** → Assign/remove roles and test inherited permissions

#### Permission Testing Workflow

1. **Get Permission Tree** → See all available permissions
2. **Assign Direct Permission** → Test direct user permissions
3. **Verify Assignment** → Confirm permission is active
4. **Create Role** → Test role-based permissions
5. **Assign Role** → Test permission inheritance
6. **Revoke Permissions** → Test permission removal

#### Error Handling Validation

1. **No Token** → Test 401 Unauthorized responses
2. **Invalid Token** → Test token validation
3. **Insufficient Permissions** → Test 403 Forbidden responses
4. **Invalid Data** → Test 400 Bad Request handling

### API Endpoints Tested

#### Authentication Endpoints
- `POST /users/login` - User authentication
- `POST /users/refresh-token` - Token refresh
- `POST /users/register` - User registration

#### User Management Endpoints
- `GET /users` - List all users (Admin only)
- `GET /users/{userId}` - Get user details
- `POST /users/{userId}/permissions` - Assign permission
- `GET /users/{userId}/permissions/direct` - Get direct permissions
- `DELETE /users/{userId}/permissions/{permissionKey}` - Revoke permission

#### Role Management Endpoints
- `GET /roles` - List all roles
- `POST /roles` - Create new role
- `GET /roles/{roleId}` - Get role details
- `POST /users/{userId}/roles` - Assign role
- `DELETE /users/{userId}/roles/{roleId}` - Remove role

#### Permission Endpoints
- `GET /permissions/tree/{userId}` - Get permission tree with user flags

### Environment-Specific Testing

#### Development Environment
- Uses `https://localhost:5001` with SSL bypass
- Includes detailed logging and debugging
- Auto-seeded database with test data

#### HTTP Alternative
- Uses `http://localhost:5000` without SSL
- Suitable for environments with certificate issues
- Same functionality without HTTPS complexity

### Best Practices for Testing

1. **Sequential Execution**: Run authentication tests first to establish tokens
2. **Variable Management**: Use global variables for user IDs and tokens
3. **Error Validation**: Always test both success and failure scenarios
4. **Clean State**: Consider cleanup operations after testing
5. **Documentation**: Comment complex test scenarios for maintainability

### Integration with CI/CD

The HTTP test files can be automated using tools like:

- **Newman** (Postman CLI) - Convert HTTP files to Postman collections
- **REST Client CLI** - Command-line execution of .http files
- **Custom Scripts** - PowerShell/Bash scripts using curl

Example PowerShell automation:
```powershell
# Test script for CI/CD pipeline
$loginResponse = Invoke-RestMethod -Uri "https://localhost:5001/users/login" `
  -Method POST -Body (@{email="admin@saral.com";password="Admin123!"} | ConvertTo-Json) `
  -ContentType "application/json" -SkipCertificateCheck

$token = $loginResponse.accessToken

$usersResponse = Invoke-RestMethod -Uri "https://localhost:5001/users" `
  -Method GET -Headers @{Authorization="Bearer $token"} -SkipCertificateCheck

Write-Host "Found $($usersResponse.Count) users"
```

### Automatic Seeding on Startup

The system automatically seeds data during application startup:

```csharp
// In Program.cs
if (app.Environment.IsDevelopment())
{
    app.ApplyMigrations();
    await app.SeedDatabaseAsync();
}
```

### Seeded Data

#### Permissions
- All 50+ permissions from `PermissionRegistry` are seeded
- Organized by categories: Admin, Finance, HR, Todos

#### Roles
- **Administrator**: Has all permissions
- **User**: Has basic todo permissions

#### Admin User
- **Email**: `admin@saral.com`
- **Password**: `Admin123!`
- **Role**: Administrator

### DatabaseSeeder Implementation

```csharp
public class DatabaseSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedPermissionsAsync(cancellationToken);
        await SeedRolesAsync(cancellationToken);
        await SeedAdminUserAsync(cancellationToken);
    }

    private async Task SeedPermissionsAsync(CancellationToken cancellationToken)
    {
        // Seeds permissions from PermissionRegistry
    }

    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        // Creates Administrator and User roles
    }

    private async Task SeedAdminUserAsync(CancellationToken cancellationToken)
    {
        // Creates admin user with Administrator role
    }
}
```

## Usage Examples

### Creating a New Role

```csharp
POST /roles
Content-Type: application/json
Authorization: Bearer {jwt-token}

{
  "name": "HR Manager",
  "description": "Human Resources Management",
  "permissionKeys": [
    "HR.Employee.Management.Create",
    "HR.Employee.Management.View",
    "HR.Employee.Management.Update"
  ]
}
```

### Assigning Role to User

```csharp
POST /users/12345678-1234-1234-1234-123456789012/roles
Content-Type: application/json
Authorization: Bearer {jwt-token}

{
  "roleId": "87654321-4321-4321-4321-210987654321"
}
```

### Checking User Permissions

```csharp
GET /users/12345678-1234-1234-1234-123456789012/permissions
Authorization: Bearer {jwt-token}

// Response:
[
  "HR.Employee.Management.Create",
  "HR.Employee.Management.View",
  "HR.Employee.Management.Update"
]
```

### Getting Permission Tree

```csharp
GET /permissions/tree/12345678-1234-1234-1234-123456789012
Authorization: Bearer {jwt-token}

// Response shows all permissions with isPresent flags
{
  "permissions": [
    {
      "key": "Admin.User.Management.Create",
      "displayName": "Create Users",
      "category": "Admin",
      "description": "Allows creating new user accounts",
      "isPresent": false
    },
    {
      "key": "HR.Employee.Management.Create",
      "displayName": "Create Employees",
      "category": "HR", 
      "description": "Allows creating new employee records",
      "isPresent": true
    }
  ]
}
```

## Configuration

### Required Settings

```json
// appsettings.json
{
  "ConnectionStrings": {
    "Database": "Host=localhost;Database=saral;Username=user;Password=pass"
  },
  "Jwt": {
    "Key": "your-secret-key",
    "Issuer": "saral-api",
    "Audience": "saral-client"
  }
}
```

### Dependency Injection Registration

```csharp
// Infrastructure/DependencyInjection.cs
services.AddScoped<PermissionProvider>();
services.AddScoped<IPermissionCacheService, PermissionCacheService>();
services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
services.AddScoped<DatabaseSeeder>();
```

## Testing

### HTTP API Testing

The authentication system includes comprehensive HTTP test files for manual and automated testing. See the [HTTP API Testing](#http-api-testing) section above for detailed information about:

- Quick testing scenarios with `QuickUserTests.http`
- Comprehensive test suite with `UsermanagementAndPermissionTests.http`
- Automated token management and global variables
- SSL certificate handling for development environments
- Full authentication, authorization, and permission workflows

### Integration Testing

Test the permission system with real database:

```csharp
[Test]
public async Task AssignRole_ValidRequest_ShouldAssignRole()
{
    // Arrange
    var userId = Guid.NewGuid();
    var roleId = await CreateTestRole();
    
    // Act
    var response = await client.PostAsync(
        $"/users/{userId}/roles", 
        JsonContent.Create(new { roleId }));
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    
    var permissions = await GetUserPermissions(userId);
    permissions.Should().Contain("Test.Permission.Key");
}
```

### Unit Testing

Test individual components:

```csharp
[Test]
public async Task GetUserEffectivePermissions_ShouldReturnCombinedPermissions()
{
    // Arrange
    var handler = new GetUserEffectivePermissionsQueryHandler(dbContext);
    var query = new GetUserEffectivePermissionsQuery(userId);
    
    // Act
    var result = await handler.Handle(query, CancellationToken.None);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeEmpty();
}
```

### Testing Authentication

```csharp
[Test]
public async Task ProtectedEndpoint_WithoutPermission_ShouldReturn403()
{
    // Arrange
    var token = GenerateTokenWithoutPermission();
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", token);
    
    // Act
    var response = await client.GetAsync("/admin/users");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
}
```

## Error Handling

### Domain Errors

```csharp
public static class RoleErrors
{
    public static readonly Error NameAlreadyExists = Error.Conflict(
        "Role.NameAlreadyExists",
        "A role with this name already exists.");
        
    public static Error NotFound(Guid id) => Error.NotFound(
        "Role.NotFound",
        $"The role with ID '{id}' was not found.");
}
```

### API Error Responses

```json
// 400 Bad Request
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Validation Error",
  "status": 400,
  "errors": {
    "Role.NameAlreadyExists": ["A role with this name already exists."]
  }
}

// 404 Not Found
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5", 
  "title": "Not Found",
  "status": 404,
  "detail": "The role with ID 'guid' was not found."
}
```

## Performance Considerations

### Database Optimization

1. **Indexes**: Strategic indexes on foreign keys and lookup columns
2. **Query Optimization**: Single queries to fetch user permissions
3. **Connection Pooling**: EF Core connection pooling enabled

### Caching Strategy

1. **Memory Cache**: 15-minute expiration for user permissions
2. **Cache Invalidation**: Automatic on role changes
3. **Cache Keys**: Consistent naming pattern `user_permissions_{userId}`

### API Performance

1. **Async/Await**: All operations are asynchronous
2. **Minimal Allocations**: Using HashSet for permission lookups
3. **Efficient Queries**: JOINs instead of multiple round trips

## Security Considerations

### Permission Validation

1. **Server-side Validation**: All permissions validated on server
2. **Exact Match**: No wildcard or partial permission matching
3. **Case Sensitive**: Permission keys are case-sensitive

### JWT Integration

1. **Token Validation**: Standard JWT validation with signature verification
2. **User ID Extraction**: Secure user ID extraction from JWT claims
3. **Authorization**: Permission-based authorization on all endpoints

### Data Protection

1. **SQL Injection**: EF Core parameterized queries
2. **Input Validation**: All inputs validated through domain rules
3. **Error Disclosure**: Generic error messages to prevent information leakage

## Monitoring and Logging

### Permission Resolution Logging

```csharp
logger.LogDebug("Retrieved and cached {PermissionCount} permissions for user {UserId}", 
    permissions.Count, userId);

logger.LogWarning("Failed to get permissions for user {UserId}: {Error}", 
    userId, result.Error);
```

### Authorization Failures

```csharp
logger.LogWarning("User {UserId} denied access to {Permission}", 
    userId, requirement.Permission);
```

### Performance Metrics

```csharp
logger.LogDebug("Retrieved permissions from cache for user {UserId}", userId);
logger.LogDebug("Permission cache miss for user {UserId}, querying database", userId);
```

## Troubleshooting

### Common Issues and Solutions

#### SSL Certificate Issues

**Problem**: "unable to verify the first certificate" error when testing HTTPS endpoints

**Solutions**:

1. **Use SSL Bypass Directive** (Recommended for development):
   ```http
   # Add to the top of your .http files
   # @no-reject-unauthorized
   @baseUrl = https://localhost:5001
   ```

2. **Use HTTP Alternative**:
   ```http
   @baseUrl = http://localhost:5000
   ```

3. **VS Code Settings** (Global fix):
   ```json
   // In VS Code settings.json
   {
     "rest-client.enableTelemetry": false,
     "rest-client.environmentVariables": {
       "$shared": {
         "ssl_verify": false
       }
     }
   }
   ```

4. **Install Development Certificate**:
   ```bash
   dotnet dev-certs https --trust
   ```

#### Authentication Issues

**Problem**: "401 Unauthorized" responses

**Solutions**:

1. **Check Admin User Seeding**:
   - Ensure database is seeded with admin user
   - Default credentials: `admin@saral.com` / `Admin123!`

2. **Verify JWT Configuration**:
   ```json
   // appsettings.json
   {
     "Jwt": {
       "Key": "your-secret-key-here-minimum-32-characters",
       "Issuer": "saral-api",
       "Audience": "saral-client",
       "ExpirationInMinutes": 60
     }
   }
   ```

3. **Check Token Format**:
   ```http
   # Correct format
   Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
   
   # Common mistakes
   Authorization: eyJhbGciOiJIUzI1NiIs...  # Missing "Bearer "
   Authorization: Bearer "eyJhbGciOiJIUzI1NiIs..."  # Extra quotes
   ```

#### Permission Denied (403 Forbidden)

**Problem**: User authenticated but lacks required permissions

**Solutions**:

1. **Check User Permissions**:
   ```http
   GET {{baseUrl}}/users/{{userId}}/permissions
   Authorization: Bearer {{token}}
   ```

2. **Verify Permission Keys**:
   - Use exact permission keys from `PermissionRegistry`
   - Permission keys are case-sensitive
   - Example: `Admin.User.Management.Read` (not `admin.user.management.read`)

3. **Check Role Assignments**:
   ```http
   GET {{baseUrl}}/users/{{userId}}
   Authorization: Bearer {{token}}
   ```

#### Database Connection Issues

**Problem**: Entity Framework connection errors

**Solutions**:

1. **Check Connection String**:
   ```json
   {
     "ConnectionStrings": {
       "Database": "Host=localhost;Database=saral;Username=postgres;Password=yourpassword;Port=5432"
     }
   }
   ```

2. **Verify Database Exists**:
   ```bash
   # For PostgreSQL
   psql -h localhost -U postgres -l
   
   # For SQL Server
   sqlcmd -S localhost -Q "SELECT name FROM sys.databases"
   ```

3. **Apply Migrations**:
   ```bash
   dotnet ef database update
   ```

#### API Endpoint Not Found (404)

**Problem**: Endpoint returns 404 Not Found

**Solutions**:

1. **Check Endpoint URLs**: Compare with OpenAPI schema at `/swagger`

2. **Verify Correct Endpoints**:
   ```http
   # Correct endpoints
   POST /users/login          # NOT /auth/login
   POST /users/register       # NOT /auth/register
   GET /permissions/tree/{userId}  # Requires userId parameter
   ```

3. **Check Route Registration**:
   ```csharp
   // In endpoint files
   app.MapPost("/users/login", handler)
      .WithOpenApi();
   ```

#### Performance Issues

**Problem**: Slow permission resolution

**Solutions**:

1. **Check Cache Status**:
   ```csharp
   // Enable debug logging to see cache hits/misses
   "Logging": {
     "LogLevel": {
       "Infrastructure.Authorization": "Debug"
     }
   }
   ```

2. **Verify Database Indexes**:
   ```sql
   -- Check index usage
   SELECT * FROM pg_stat_user_indexes WHERE relname IN (
     'user_roles', 'role_permissions', 'user_permissions'
   );
   ```

3. **Monitor Cache Performance**:
   ```csharp
   // Add performance counters
   logger.LogInformation("Permission resolution took {ElapsedMs}ms for user {UserId}", 
     stopwatch.ElapsedMilliseconds, userId);
   ```

#### Test Execution Issues

**Problem**: HTTP tests fail or behave unexpectedly

**Solutions**:

1. **Sequential Execution**: Run tests in order (login first, then others)

2. **Variable Management**: Ensure global variables are set correctly
   ```http
   > {%
     client.test("Store token", function() {
       client.assert(response.body.accessToken, "Token should be present");
       client.global.set("accessToken", response.body.accessToken);
     });
   %}
   ```

3. **Clear Variables**: Reset between test sessions
   ```http
   # Reset global variables
   @accessToken = 
   @userId = 
   @testUserId = 
   ```

### Development Environment Setup

#### Required Software

1. **.NET 8 SDK**: Download from Microsoft
2. **PostgreSQL** or **SQL Server**: Database server
3. **VS Code** with extensions:
   - REST Client
   - C# Dev Kit
   - Entity Framework Core Power Tools

#### Environment Variables

```bash
# Development environment
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://localhost:5001;http://localhost:5000

# Database
ConnectionStrings__Database=Host=localhost;Database=saral;Username=postgres;Password=yourpassword

# JWT Configuration
Jwt__Key=your-super-secret-key-here-minimum-32-characters-long
Jwt__Issuer=saral-api
Jwt__Audience=saral-client
```

#### First-Time Setup

1. **Clone Repository**:
   ```bash
   git clone <repository-url>
   cd Backend
   ```

2. **Install Dependencies**:
   ```bash
   dotnet restore
   ```

3. **Setup Database**:
   ```bash
   dotnet ef database update
   ```

4. **Run Application**:
   ```bash
   dotnet run --project src/Web.Api
   ```

5. **Verify Setup**:
   - Navigate to `https://localhost:5001/swagger`
   - Run quick tests from `tests/HttpTests/QuickUserTests.http`

### Logging and Diagnostics

#### Enable Detailed Logging

```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Infrastructure.Authorization": "Debug",
      "Application.Users": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

#### Common Log Messages

```
# Successful authentication
info: Application.Users.Login.LoginCommandHandler[0] User admin@saral.com logged in successfully

# Permission cache hit
dbug: Infrastructure.Authorization.PermissionProvider[0] Retrieved permissions from cache for user 12345678-1234-5678-9012-123456789012

# Permission denied
warn: Infrastructure.Authorization.PermissionAuthorizationHandler[0] User 12345678-1234-5678-9012-123456789012 denied access to Admin.User.Management.View

# Database query
info: Microsoft.EntityFrameworkCore.Database.Command[20101] Executed DbCommand (23ms) SELECT ... FROM user_permissions
```

### Support and Maintenance

#### Regular Maintenance Tasks

1. **Permission Audit**: Review and update permission assignments quarterly
2. **Cache Monitoring**: Monitor cache hit rates and adjust expiration if needed
3. **Performance Review**: Analyze slow queries and optimize indexes
4. **Security Updates**: Keep JWT keys rotated and dependencies updated

#### Monitoring Checklist

- [ ] Authentication success/failure rates
- [ ] Permission cache hit/miss ratios
- [ ] Database query performance
- [ ] API response times
- [ ] SSL certificate expiration
- [ ] Failed authorization attempts

This troubleshooting guide should help resolve most common issues encountered during development and testing of the authentication system.

