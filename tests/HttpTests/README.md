# HTTP Testing Guide

This directory contains HTTP files for testing the User Management and Permission Management API endpoints.

## Files:

### 1. `UsermanagementAndPermissionTests.http`
**Comprehensive test suite with automated assertions and global variable management.**

**Features:**
- ✅ Automatic token management with global variables
- ✅ Automated test assertions with pass/fail validation
- ✅ Full end-to-end testing workflow
- ✅ Error handling test cases
- ✅ Complete test coverage of all implemented features
- ✅ SSL verification disabled for development certificates

**How to use:**
1. Start the application: `dotnet run --project src/Web.Api --launch-profile https`
2. Open the file in VS Code or any HTTP client that supports `.http` files
3. Run tests sequentially from top to bottom
4. Tokens are automatically captured and reused

### 2. `QuickUserTests.http`
**Simple manual testing for quick verification (HTTPS with SSL bypass).**

**Features:**
- 📝 Manual token replacement (copy/paste)
- 🚀 Quick setup for basic functionality testing
- 💡 Good for debugging individual endpoints
- ✅ SSL verification disabled for development certificates

### 3. `QuickUserTests-HTTP.http`
**Simple manual testing using HTTP (no SSL issues).**

**Features:**
- 📝 Manual token replacement (copy/paste)
- 🚀 Quick setup for basic functionality testing
- 🔓 Uses HTTP to avoid SSL certificate issues entirely
- 💡 Good for environments with certificate problems

**How to use:**
1. Run the login request first
2. Copy the `accessToken` from the response
3. Replace `{{token}}` manually in subsequent requests
4. Replace `{{testUserId}}` with actual user IDs as needed

## Application Setup:

### Prerequisites:
- PostgreSQL database running and configured
- .NET 9 installed
- Application built successfully

### Start the Application:
```bash
# From the Backend root directory
dotnet run --project src/Web.Api --launch-profile https
```

The application will be available at:
- **HTTPS**: https://localhost:5001
- **HTTP**: http://localhost:5000

### Default Admin Credentials:
- **Email**: admin@saral.com
- **Password**: Admin123!

## Test Coverage:

### Authentication:
- ✅ Admin login
- ✅ Token refresh
- ✅ Unauthorized access handling

### User Management:
- ✅ Get all users (NEW endpoint)
- ✅ Get user by ID
- ✅ User registration
- ✅ User effective permissions
- ✅ User direct permissions

### Permission Management:
- ✅ Get permission tree
- ✅ Assign permission to user
- ✅ Revoke permission from user
- ✅ Permission validation

### Role Management:
- ✅ Get all roles
- ✅ Create new role
- ✅ Get role by ID
- ✅ Assign role to user
- ✅ Remove role from user

### Error Handling:
- ✅ Invalid permissions
- ✅ Invalid user IDs
- ✅ Unauthorized access
- ✅ Validation errors

## Expected Results:

All tests should pass when:
1. Database is properly seeded with admin user and permissions
2. Application is running on the correct ports
3. Authentication tokens are valid
4. User has appropriate permissions for each operation

## Troubleshooting:

### SSL Certificate Issues:
**Problem**: "unable to verify the first certificate" or SSL verification errors

**Solutions**:
1. **Use the provided files with SSL bypass**: Both `UsermanagementAndPermissionTests.http` and `QuickUserTests.http` include `# @no-reject-unauthorized` directive
2. **Use HTTP version**: Use `QuickUserTests-HTTP.http` which connects to `http://localhost:5000`
3. **VS Code REST Client**: Add this to your VS Code settings:
   ```json
   {
     "rest-client.requestGotOptions": {
       "rejectUnauthorized": false
     }
   }
   ```
4. **Alternative HTTP clients**: Most HTTP clients have an option to disable SSL verification for development

### Common Issues:
1. **401 Unauthorized**: Check if token is valid and properly included in Authorization header
2. **403 Forbidden**: User lacks required permissions for the operation
3. **404 Not Found**: Check if user/role/permission IDs exist
4. **400 Bad Request**: Check request body format and required fields

### Debug Tips:
1. Check application logs for detailed error information
2. Verify database seeding completed successfully
3. Ensure admin user has all required permissions
4. Test with fresh tokens if authentication fails

## Permission Requirements:

Each endpoint requires specific permissions:
- **Get All Users**: `Admin.User.Management.Read`
- **Assign/Revoke Permissions**: `Admin.Permission.Management.Assign/Revoke`
- **View Permissions**: `Admin.Permission.Management.View`
- **Role Management**: `Admin.Role.Management.Create`
- **User Access**: `Users.Access`

The admin user should have all these permissions by default.
