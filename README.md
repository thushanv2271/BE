# Clean Architecture Template

What's included in the template?

- SharedKernel project with common Domain-Driven Design abstractions.
- Domain layer with sample entities.
- Application layer with abstractions for:
  - CQRS
  - Example use cases
  - Cross-cutting concerns (logging, validation)
- Infrastructure layer with:
  - Authentication
  - Permission authorization
  - EF Core, PostgreSQL
  - Serilog
- Seq for searching and analyzing structured logs
  - Seq is available at http://localhost:8081 by default
- Testing projects
  - Architecture testing

## Configuration

### Key Configuration Settings

The application requires several configuration settings in `appsettings.json`:

#### File Path Configurations

- **`SegmentMasterDataPath`**: Path to the Excel file containing segment and sub-segment master data used for seeding the database. This file should contain two columns:
  - Column 1: Segment names
  - Column 2: Sub-segment names
  
  The data from this file is loaded during database seeding to populate the `SegmentMaster` table, which provides reference data for business segments and their corresponding sub-segments.

- **`UserExportPath`**: Directory path where user export files are saved. This is used by the application's export functionality to store generated Excel files or other export formats when users request data exports.

Both paths should be absolute paths accessible by the application with appropriate read/write permissions.

I'm open to hearing your feedback about the template and what you'd like to see in future iterations.

If you're ready to learn more, check out [**Pragmatic Clean Architecture**](https://www.milanjovanovic.tech/pragmatic-clean-architecture?utm_source=ca-template):

- Domain-Driven Design
- Role-based authorization
- Permission-based authorization
- Distributed caching with Redis
- OpenTelemetry
- Outbox pattern
- API Versioning
- Unit testing
- Functional testing
- Integration testing

Stay awesome!
