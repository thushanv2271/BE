# GitHub Copilot Instructions for ASP.NET Core 8 Project

## Language and Framework Guidelines
- Use C# 12 language features where appropriate
- Target ASP.NET Core 8 framework
- Follow SOLID principles in class and interface design
- Implement dependency injection for loose coupling

## Code Structure and Patterns
- Use primary constructors for dependency injection in services, use cases, etc.
- Use async/await for I/O-bound operations
- Prefer record types for immutable data structures
- Favor explicit typing (this is very important). Only use `var` when type is evident from context
- Make types `internal` and `sealed` by default unless otherwise specified
- Prefer `Guid` for identifiers unless otherwise specified

## API Design
- Prefer controller endpoints over minimal APIs
- Utilize minimal APIs only for simple endpoints when explicitly stated or when it makes sense
- Implement proper versioning for APIs
- Use Swagger/OpenAPI for API documentation

## Data Access and Validation
- Use Entity Framework Core for database operations
- Implement proper model validation using data annotations and FluentValidation
- Use strongly-typed configuration with IOptions pattern

## Security and Authentication
- Implement proper authentication and authorization
- Use secure communication with HTTPS
- Implement proper CORS policies

## Error Handling and Logging
- Implement proper exception handling and logging
- Use structured logging with ILogger
- Implement health checks for the application

## Performance and Caching
- Implement proper caching strategies where appropriate
- Use middleware for cross-cutting concerns
- Use background services for long-running tasks

## Testing
- Implement unit tests for business logic using xUnit
- Use integration tests for API endpoints
- Follow AAA pattern (Arrange, Act, Assert) in tests

## Configuration and Environment
- Use environment-specific configuration files (appsettings.json, appsettings.Development.json)
- Implement proper configuration validation

## Code Quality Guidelines
- Use `is null` checks instead of `== null`
- Use `is not null` checks instead of `!= null`
- Prefer explicit null checks over null-conditional operators when clarity is important
- Use meaningful variable and method names
- Keep methods small and focused on single responsibility

## When generating code, always:
1. Include proper error handling
2. Add appropriate logging statements
3. Follow async/await patterns for I/O operations
4. Include XML documentation comments for public APIs
5. Use appropriate HTTP status codes in API responses
6. Implement proper validation for input parameters
7. Follow RESTful conventions for API endpoints
8. Include appropriate using statements and namespace declarations
