# GDSC Sharing Platform

A knowledge-sharing platform for the GDSC community, supporting members in roadmap-based learning, content publishing, sharing schedule organization, and community interaction.

> Current Status: **Sprint 0 – Foundation**.

## Tech Stack

- ASP.NET Core on .NET 10
- PostgreSQL 17
- Entity Framework Core 10
- ASP.NET Core Identity with `Guid` keys
- Next.js 16
- React 19
- TypeScript
- Swagger/OpenAPI
- xUnit
- Testcontainers
- Docker Compose

## Solution Architecture

```text
GdscSharing.slnx
├── src
│   ├── GdscSharing.Api
│   ├── GdscSharing.Application
│   ├── GdscSharing.Domain
│   └── GdscSharing.Infrastructure
├── tests
│   ├── GdscSharing.UnitTests
│   └── GdscSharing.IntegrationTests
├── web
├── deploy
└── docs
```

Project Responsibilities:

| Project                        | Responsibility                                          |
| ------------------------------ | ------------------------------------------------------- |
| `GdscSharing.Domain`           | Entities, enums, constants, and business rules          |
| `GdscSharing.Application`      | Use cases and application contracts                     |
| `GdscSharing.Infrastructure`   | EF Core, PostgreSQL, Identity, migrations, and seeders  |
| `GdscSharing.Api`              | HTTP endpoints, middleware, and dependency registration |
| `GdscSharing.UnitTests`        | Independent business rule testing                       |
| `GdscSharing.IntegrationTests` | API, migration, and real database testing               |
| `web`                          | Next.js frontend                                        |
| `deploy`                       | Docker Compose and environment configuration            |

Dependency Principles:

```text
API ───────► Application
 │               ▲
 └──────► Infrastructure ───────► Domain
```

The `Domain` layer does not depend on ASP.NET Core, Entity Framework Core, or Infrastructure.

Sprint 0 specification is located at:

```text
docs/SPRINT_0_SPECIFICATION.md
```

## Sprint 0 Database Scope

First migration:

```text
202608150001_InitialIdentity
```

This migration only creates foundational tables:

- `gdsc.Users`
- `gdsc.Roles`
- `gdsc.UserRoles`
- `gdsc.UserClaims`
- `gdsc.UserLogins`
- `gdsc.UserTokens`
- `gdsc.RoleClaims`
- `gdsc.Departments`
- `__EFMigrationsHistory`

The following tables are not part of Sprint 0:

- Dashboard
- AI Assistant
- Roadmap
- Sharing Content
- Sharing Schedule
- Social Interaction
- Notification

## Seed Data

When the API starts outside the `Testing` environment, the `DatabaseSeeder` will:

1. Apply pending migrations.
2. Create the `Admin` role.
3. Create the `Member` role.
4. Create default Departments.
5. Create the Admin account from configuration.
6. Assign the system role to the Admin.

Default Departments:

- `Management`
- `Software`
- `Design`
- `Photography`
- `Media`

The seeder is **idempotent**, meaning running it multiple times will not create duplicate data.

Production Admin passwords must not be stored in:

- Source code
- `appsettings.json`
- Migrations
- Git repository

Passwords must be provided via:

- Environment variables
- `deploy/.env`
- .NET User Secrets
- Production environment secret managers

## Running the System via Docker

### Step 1: Create the Environment File

```bash
cp deploy/.env.example deploy/.env
```

Open `deploy/.env` and update:

```dotenv
POSTGRES_DB=gdsc_sharing
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your-strong-database-password

ADMIN_EMAIL=admin@gdsc.local
ADMIN_PASSWORD=your-strong-admin-password
ADMIN_DISPLAY_NAME=System Administrator
```

Do not commit the `deploy/.env` file.

### Step 2: Build and Start Containers

Run from the directory containing `GdscSharing.slnx`:

```bash
docker compose \
  --env-file deploy/.env \
  -f deploy/docker-compose.yml \
  up -d --build
```

### Step 3: Check Containers

```bash
docker compose \
  --env-file deploy/.env \
  -f deploy/docker-compose.yml \
  ps
```

Main services:

| Service          | Address                              |
| ---------------- | ------------------------------------ |
| Web              | `http://localhost:3000`              |
| API              | `http://localhost:5080/api/v1`       |
| Swagger          | `http://localhost:5080/swagger`      |
| Aggregate Health | `http://localhost:5080/health`       |
| Liveness         | `http://localhost:5080/health/live`  |
| Readiness        | `http://localhost:5080/health/ready` |

### Step 4: Check Health Status

```bash
curl -i http://localhost:5080/health/live
curl -i http://localhost:5080/health/ready
```

Expected output:

```text
HTTP/1.1 200 OK
```

Meaning:

- `/health/live`: The API process is running.
- `/health/ready`: The API has successfully connected to PostgreSQL.
- `/health`: The aggregate health status of the system.

### Step 5: View API Logs

```bash
docker compose \
  --env-file deploy/.env \
  -f deploy/docker-compose.yml \
  logs -f api
```

### Step 6: Stop the System

```bash
docker compose \
  --env-file deploy/.env \
  -f deploy/docker-compose.yml \
  down
```

The above command does not delete PostgreSQL data.

Only use the following command when you intentionally want to delete the local database:

```bash
docker compose \
  --env-file deploy/.env \
  -f deploy/docker-compose.yml \
  down --volumes
```

## Running the Backend Locally

Prerequisites:

- PostgreSQL is running.
- Correct .NET SDK installed according to `global.json`.

On macOS or Linux:

```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=gdsc_sharing;Username=postgres;Password=your-password"

export AdminSeed__Email="admin@gdsc.local"
export AdminSeed__Password="your-strong-admin-password"
export AdminSeed__DisplayName="System Administrator"
```

Restore and build:

```bash
dotnet restore GdscSharing.slnx
dotnet build GdscSharing.slnx --no-restore
```

Run API:

```bash
dotnet run --project src/GdscSharing.Api
```

## Configuration via .NET User Secrets

Initialize User Secrets:

```bash
dotnet user-secrets init \
  --project src/GdscSharing.Api
```

Configure connection string:

```bash
dotnet user-secrets set \
  "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5432;Database=gdsc_sharing;Username=postgres;Password=your-password" \
  --project src/GdscSharing.Api
```

Configure Admin:

```bash
dotnet user-secrets set \
  "AdminSeed:Email" \
  "admin@gdsc.local" \
  --project src/GdscSharing.Api
```

```bash
dotnet user-secrets set \
  "AdminSeed:Password" \
  "your-strong-admin-password" \
  --project src/GdscSharing.Api
```

```bash
dotnet user-secrets set \
  "AdminSeed:DisplayName" \
  "System Administrator" \
  --project src/GdscSharing.Api
```

## Running the Frontend Locally

```bash
cd frontend
cp .env.example .env
pnpm install --frozen-lockfile
pnpm run dev
```

Frontend runs at:

```text
http://localhost:3000
```

### Google login through the Next.js BFF

Set the server-only variables in `frontend/.env`:

| Variable                | Purpose                                             | Local example           |
| ----------------------- | --------------------------------------------------- | ----------------------- |
| `APP_ORIGIN`            | Canonical frontend origin and mutation origin check | `http://localhost:3000` |
| `BACKEND_PUBLIC_ORIGIN` | Browser-accessible HTTPS API origin for Google start | `https://localhost:7160` |
| `INTERNAL_API_URL`      | API origin used by Next.js server requests          | `http://localhost:5184` |
| `BACKEND_API_URL`       | Public uploads proxy and fallback API origin        | `http://localhost:5184` |

Use HTTPS origins in production. Browser API calls stay on the frontend origin;
The browser-facing backend must also use HTTPS locally because OIDC correlation,
nonce and external-login cookies are Secure. Trust the .NET development certificate
and register `https://localhost:7160/api/auth/google/callback` in Google Console.
`NEXT_PUBLIC_API_URL` can remain empty. Keep Google credentials exclusively in
backend User Secrets or environment variables.

Set backend `Authentication__Google__BffCallbackUrl` to
`http://localhost:3000/api/auth/google/callback` locally, or the corresponding
HTTPS frontend URL in production. The Google Console authorized redirect URI
remains the **backend** OIDC callback (`/api/auth/google/callback` on the API origin).
The frontend callback receives a short-lived, single-use handoff code bound to an
HttpOnly verifier cookie, exchanges it server-side, and writes HttpOnly session
cookies. Access and refresh tokens never enter the redirect URL or client state.

The current handoff store and refresh coordination are in memory. Run a single
instance of each service until shared atomic storage/coordination is implemented.

## Entity Framework Core Migrations

### Install EF Core CLI

```bash
dotnet tool install \
  --global dotnet-ef \
  --version 10.0.0
```

### Add a New Migration

```bash
dotnet ef migrations add MigrationName \
  --project src/GdscSharing.Infrastructure \
  --startup-project src/GdscSharing.Api \
  --output-dir Persistence/Migrations
```

### List Migrations

```bash
dotnet ef migrations list \
  --project src/GdscSharing.Infrastructure \
  --startup-project src/GdscSharing.Api
```

Do not modify migrations that have already been applied to a shared environment.

When the database schema changes, create a new migration.

## Verifying Database Migrations and Seeding

Access PostgreSQL inside the container:

```bash
docker compose \
  --env-file deploy/.env \
  -f deploy/docker-compose.yml \
  exec postgres \
  sh -lc 'psql -U "$POSTGRES_USER" -d "$POSTGRES_DB"'
```

Check migrations:

```sql
SELECT *
FROM "__EFMigrationsHistory";
```

Check roles:

```sql
SELECT "Name"
FROM gdsc."Roles"
ORDER BY "Name";
```

Check Departments:

```sql
SELECT "Name"
FROM gdsc."Departments"
ORDER BY "Name";
```

Check Admin account:

```sql
SELECT
    "Email",
    "DisplayName",
    "Status",
    "EmailConfirmed"
FROM gdsc."Users";
```

Check User–Role relationships:

```sql
SELECT
    users."Email",
    roles."Name" AS "RoleName"
FROM gdsc."UserRoles" AS user_roles
INNER JOIN gdsc."Users" AS users
    ON users."Id" = user_roles."UserId"
INNER JOIN gdsc."Roles" AS roles
    ON roles."Id" = user_roles."RoleId";
```

Exit `psql`:

```text
\q
```

## Unit Tests

Unit tests do not require Docker:

```bash
dotnet test tests/GdscSharing.UnitTests
```

Unit tests verify:

- `Department` names are automatically trimmed.
- `Department` rejects empty names.
- The default Department list contains no duplicates.
- The role list correctly contains `Admin` and `Member`.

## Integration Tests

Docker must be running:

```bash
dotnet test tests/GdscSharing.IntegrationTests
```

Integration tests use Testcontainers to:

1. Spin up a temporary PostgreSQL 17 container.
2. Create an isolated test database.
3. Apply real migrations.
4. Run the database seeder.
5. Verify data.
6. Tear down the container after tests complete.

The development database is unaffected.

Integration tests verify:

- `/health/live`
- `/health/ready`
- `InitialIdentity` migration
- Pending migrations
- Role seeding
- Department seeding
- Admin seeding
- User–Role relationships
- Seeder idempotency

## Running All Tests

```bash
dotnet test GdscSharing.slnx
```

Expected output:

```text
Failed: 0
```

## Sprint 0 Acceptance Checklist

```bash
dotnet restore GdscSharing.slnx

dotnet build \
  GdscSharing.slnx \
  --no-restore

dotnet test \
  GdscSharing.slnx \
  --no-build

npm --prefix web ci
npm --prefix web run build

docker compose \
  --env-file deploy/.env \
  -f deploy/docker-compose.yml \
  up -d --build

curl --fail http://localhost:5080/health/live
curl --fail http://localhost:5080/health/ready
```

Sprint 0 can only be closed when:

- Build succeeds.
- Unit tests pass.
- Integration tests pass.
- Frontend builds successfully.
- Containers are stable.
- Health Checks return `200 OK`.
- Database migrations are applied.
- Seed data is correct.
- Git history contains no secrets.
