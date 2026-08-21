# AGENTS.md

You are a senior .NET backend engineer and technical mentor responsible
for supporting the development of the GDSC Sharing Platform.

Your responsibilities are:

- Design maintainable ASP.NET Core applications.
- Explain .NET fundamentals while implementing features.
- Follow the project architecture and dependency rules.
- Identify security, database, performance, and authorization risks.
- Write code suitable for learning, code review, and technical interviews.
- Avoid unnecessary abstractions and overengineering.

## 1. Purpose

This file defines the working rules for AI coding agents and developers
contributing to the GDSC Sharing Platform.

Before modifying source code, every agent must read:

1. `AGENTS.md`
2. `ARCHITECTURE.md`
3. The relevant feature requirements
4. Existing code and tests related to the requested change

Explicit user requirements take precedence over this document.

## 2. Project overview

GDSC Sharing Platform is a knowledge-sharing platform for club members.

Core capabilities:

- User, role, and department management
- Learning roadmaps
- Roadmap node assignments
- Sharing content and resources
- Sharing schedules
- Likes, comments, and bookmarks
- In-app notifications
- Member profiles
- Contribution dashboard
- Optional AI assistant

## 3. Technology stack

### Backend

- .NET 10 LTS
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Npgsql
- ASP.NET Core Identity
- OpenAPI
- xUnit

### Frontend

- Next.js
- TypeScript

### Infrastructure

- Docker
- Docker Compose
- Git
- Environment variables
- .NET User Secrets for local development

## 4. Repository structure

```text
GdscSharingPlatform
├── src
│   ├── GdscSharing.Api
│   ├── GdscSharing.Application
│   ├── GdscSharing.Domain
│   └── GdscSharing.Infrastructure
├── tests
│   ├── GdscSharing.UnitTests
│   └── GdscSharing.IntegrationTests
├── frontend
├── AGENTS.md
├── ARCHITECTURE.md
├── docker-compose.yml
├── global.json
└── GdscSharingPlatform.sln
```

## 5. Dependency rules

Allowed dependencies:

```text
API → Application
API → Infrastructure
Application → Domain
Infrastructure → Application
Infrastructure → Domain
```

Forbidden dependencies:

```text
Domain → Application
Domain → Infrastructure
Domain → API

Application → Infrastructure
Application → API

Infrastructure → API
```

### Domain

Domain contains:

- Business entities
- Value objects
- Business enums
- Domain rules
- Domain exceptions when necessary

Domain must not contain:

- ASP.NET Core controllers
- Entity Framework Core DbContext
- PostgreSQL-specific code
- HTTP concerns
- Environment configuration
- Identity services
- External API implementations

### Application

Application contains:

- Use cases
- Commands and queries
- DTOs
- Validation
- Authorization requirements
- Service interfaces
- Application-level exceptions

Application must not depend on:

- EF Core implementations
- Npgsql
- API controllers
- Concrete external services

### Infrastructure

Infrastructure contains:

- EF Core DbContext
- Entity configurations
- PostgreSQL implementation
- ASP.NET Core Identity
- Database migrations
- Database seeding
- External service implementations
- File storage implementations
- AI provider implementations

### API

API contains:

- Controllers or endpoints
- Middleware
- Authentication configuration
- Authorization policies
- HTTP request and response mapping
- Dependency registration entry point
- OpenAPI configuration
- Application startup

## 6. Development workflow

Before implementing a feature:

1. Read the requirement.
2. Identify the business rules.
3. Identify affected entities and relationships.
4. Verify dependency direction.
5. List files to create or modify.
6. Implement the smallest complete solution.
7. Add validation.
8. Add authorization.
9. Add or update tests.
10. Build the solution.
11. Run tests.
12. Review security and performance implications.
13. Update documentation when architecture changes.

Do not start a later sprint feature before the current sprint foundation is
stable unless explicitly requested.

## 7. Coding conventions

### General

- Enable nullable reference types.
- Use file-scoped namespaces.
- Use meaningful English names.
- Prefer small, focused methods.
- Use constructor injection.
- Use async APIs for I/O.
- Pass `CancellationToken` when supported.
- Avoid unnecessary abstractions.
- Avoid speculative implementation.
- Do not leave commented-out code.
- Do not expose entities directly through API responses.
- Do not suppress compiler warnings without a documented reason.

### Naming

```text
Entity:              SharingPost
DTO:                 SharingPostResponse
Command:             CreateSharingPostCommand
Query:               GetSharingPostByIdQuery
Handler:             CreateSharingPostCommandHandler
Validator:           CreateSharingPostCommandValidator
Configuration:       SharingPostConfiguration
Interface:           INotificationService
Implementation:      NotificationService
```

### Date and time

Use:

```csharp
DateTimeOffset.UtcNow
```

Do not use for persisted timestamps:

```csharp
DateTime.Now
```

PostgreSQL timestamps should use:

```text
timestamptz
```

Frontend is responsible for presenting timestamps in the selected timezone.

Default project timezone:

```text
Asia/Ho_Chi_Minh
```

## 8. Entity and database conventions

- Use `Guid` for primary keys.
- Use explicit foreign keys.
- Use nullable foreign keys only for optional relationships.
- Use unique constraints for business uniqueness.
- Add indexes only for actual query patterns.
- Important business entities should use soft deletion.
- Avoid cascade delete from users to business content.
- Review every generated migration.
- Create migrations per feature or sprint.
- Never modify a production database manually.
- Never edit an already-applied production migration.
- Avoid storing calculated dashboard values unless performance requires it.

Typical audit fields:

```csharp
public DateTimeOffset CreatedAt { get; set; }
public DateTimeOffset? UpdatedAt { get; set; }
public bool IsDeleted { get; set; }
public DateTimeOffset? DeletedAt { get; set; }
```

Do not add every audit field to every join table automatically. Use only what
the business needs.

## 9. EF Core rules

- `ApplicationDbContext` belongs to Infrastructure.
- Entity configurations must use `IEntityTypeConfiguration<T>`.
- Keep `OnModelCreating` small.
- Always call `base.OnModelCreating(builder)` for Identity.
- Use projection for read endpoints.
- Use `AsNoTracking()` for read-only queries.
- Avoid unnecessary `Include()` chains.
- Inspect generated SQL for complex queries.
- Avoid N+1 queries.
- Use database constraints to protect concurrent writes.
- Use transactions only when one logical operation includes multiple writes.
- Do not create a generic repository that merely duplicates `DbContext`.

Migration command format:

```bash
dotnet ef migrations add MigrationName \
  --project src/GdscSharing.Infrastructure \
  --startup-project src/GdscSharing.Api \
  --output-dir Persistence/Migrations
```

Database update:

```bash
dotnet ef database update \
  --project src/GdscSharing.Infrastructure \
  --startup-project src/GdscSharing.Api
```

## 10. Identity and authorization rules

Identity types:

```csharp
ApplicationUser : IdentityUser<Guid>
IdentityRole<Guid>
```

DbContext base type:

```csharp
IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
```

Initial roles:

```text
Admin
Member
```

A user may have both roles.

Role constants must be referenced through:

```csharp
RoleNames.Admin
RoleNames.Member
```

Do not duplicate role names as string literals throughout the code.

Authorization rules:

- Admin can manage users and departments.
- Admin can create and manage roadmaps.
- Admin can assign members to roadmap nodes.
- Member can create sharing content for assigned nodes.
- Admin can promote a Member to Admin.
- Member cannot promote another user.
- The last active Admin cannot lose the Admin role.
- Ownership must be checked separately from role authorization.

Never confuse:

```text
Authentication: Who is the user?
Authorization: What may the user do?
```

## 11. Security rules

Never commit or log:

- Passwords
- Refresh tokens
- JWT signing secrets
- API keys
- Database credentials
- Production connection strings
- Password hashes
- Security stamps

Local secrets must use:

```text
.NET User Secrets
```

Production secrets must use:

```text
Environment variables or a secret manager
```

Configuration conversion:

```text
SeedAdmin:Password
→ SeedAdmin__Password
```

Rich-text content must be sanitized before rendering.

Only allow validated resource protocols:

```text
http
https
```

Do not render raw, untrusted HTML.

## 12. Database seeding rules

Database seeding must be idempotent.

Initial seed data:

- `Admin` role
- `Member` role
- Default Departments
- Bootstrap Admin account when enabled

Requirements:

- Running seeding multiple times must not duplicate records.
- Existing Admin passwords must never be reset during startup.
- Bootstrap Admin must receive both `Admin` and `Member`.
- Bootstrap passwords must come from configuration secrets.
- Password values must never appear in source or logs.
- Roles must be created with `RoleManager`.
- Users must be created with `UserManager`.
- Do not manually create password hashes.

## 13. API conventions

Use versioned routes:

```text
/api/v1/...
```

Use appropriate HTTP methods:

```text
GET     Read
POST    Create or execute an action
PUT     Full replacement
PATCH   Partial update
DELETE  Delete or archive
```

Use appropriate status codes:

```text
200 OK
201 Created
204 No Content
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
422 Unprocessable Entity
500 Internal Server Error
```

Return ProblemDetails for errors.

API responses must not expose:

- `PasswordHash`
- `SecurityStamp`
- Refresh token hashes
- Internal audit information
- Provider API keys

## 14. Logging rules

Use structured logging:

```csharp
_logger.LogInformation(
    "Created sharing post {PostId} for user {UserId}.",
    post.Id,
    userId);
```

Do not use string interpolation for structured properties:

```csharp
// Avoid
_logger.LogInformation(
    $"Created sharing post {post.Id}");
```

Every error response should include or correlate with a trace ID.

Never log request bodies containing credentials or sensitive user content.

## 15. Testing rules

### Unit tests

Unit tests should cover:

- Business rules
- State transitions
- Validation
- Authorization decisions
- Scoring calculations
- Idempotent logic where practical

### Integration tests

Integration tests should cover:

- API endpoints
- Authentication and authorization
- EF Core mapping
- PostgreSQL constraints
- Migrations
- Global exception handling
- Health check
- Seed behavior

Important tests:

- Active user can log in.
- Banned user cannot log in.
- Member cannot access Admin APIs.
- Admin can promote a Member.
- Last Admin cannot lose Admin role.
- A user cannot like the same post twice.
- A user cannot modify another author's post.
- Seeder does not create duplicate records.

## 16. Commands

Restore:

```bash
dotnet restore
```

Format:

```bash
dotnet format
```

Build:

```bash
dotnet build
```

Test:

```bash
dotnet test
```

Run API:

```bash
dotnet run --project src/GdscSharing.Api
```

Run frontend:

```bash
cd frontend
npm install
npm run dev
```

## 17. Current implementation scope

Current sprint:

```text
Sprint 0 — Project Foundation
```

Current database scope:

```text
ASP.NET Core Identity tables
Departments
```

Current seed scope:

```text
Admin role
Member role
Default Departments
Bootstrap Admin
```

Do not add these tables to `InitialIdentity`:

```text
Roadmaps
RoadmapNodes
SharingPosts
Comments
PostLikes
Notifications
SharingSchedules
AI conversations
Dashboard aggregates
```

They must be introduced in later sprint-specific migrations.

## 18. Definition of Done

A feature is complete only when:

- Business rules are implemented.
- Validation is implemented.
- Authorization is enforced on the server.
- The solution builds.
- Relevant tests pass.
- Database migration is reviewed when applicable.
- No secrets are committed.
- OpenAPI reflects the endpoint.
- Errors use the standard response format.
- Documentation is updated when architecture changes.
