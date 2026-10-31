# BookHub

Application to search, register and maintain **genres**, **authors** and **books**, built as a technical challenge.

- **Backend:** .NET 10 REST API (this repository, `BookHub.*` projects)
- **Database:** PostgreSQL 17
- **Frontend:** React SPA _(to be added — see [Frontend organization](#frontend-organization))_

---

## 1. Solution overview

| Concern | Choice |
|---|---|
| API | ASP.NET Core 10 controllers, Swagger/OpenAPI (Swashbuckle) |
| Persistence | EF Core 10 + Npgsql, code-first migrations |
| Errors | Global `IExceptionHandler` returning RFC 9457 `ProblemDetails` |
| Tests | xUnit, NSubstitute, `WebApplicationFactory`, Testcontainers (real PostgreSQL) |
| Local infra | `docker-compose.yml` with PostgreSQL |

Domain rules implemented:

- A genre has many books; an author has many books; each book belongs to exactly **one author** and **one genre**.
- Genre names are **unique** (case-insensitive). Author names may repeat (two different people can share a name).
- A book requires a title and a publication year (1 up to the current year); ISBN is optional (10 or 13 characters, hyphens/spaces ignored).
- An author or genre that still has books **cannot be deleted** (`409 Conflict`).

## 2. Running locally

**Prerequisites:** .NET 10 SDK, Docker.

```bash
# 1. Start PostgreSQL (db: bookhubdb, user: bookhub, password: bookhubpwd, port 5432)
docker compose up -d postgres

# 2. Run the API (in Development it applies pending migrations and seeds sample data)
dotnet run --project BookHub.API --launch-profile http
```

**Sample data (seed).** On startup in Development, after migrations, the API checks whether the database already has any genre, author or book. If it is **completely empty**, it inserts a sample catalogue (8 genres, 10 authors, 25 books) in a single transaction; otherwise it does nothing, so existing data is never touched or duplicated. The data lives in `BookHub.Infrastructure/Persistence/Seed/DatabaseSeeder.cs`. To start from scratch, drop the volume and start again (`docker compose down -v`), or disable the seed with `Seed:Enabled=false`.

- Swagger UI: <http://localhost:5254/swagger>
- Visual Studio: open `BookHub-API.slnx`; the `BookHub-API.slnLaunch` file sets the API as startup project.

**Configuration** (`BookHub.API/appsettings*.json`, overridable with environment variables):

| Key | Default |
|---|---|
| `ConnectionStrings:DefaultConnection` | `Host=localhost;...` in `appsettings.Development.json`; `Host=postgres;...` in `appsettings.json` (Docker network name) |
| `Seed:Enabled` | `true` (Development only); set to `false` to skip the sample data |
| `Jwt:Key`, `Jwt:Issuer` | See [Known limitations](#9-known-limitations) |

**Database setup without running the API** (e.g. CI or Production):

```bash
dotnet tool install --global dotnet-ef   # once
dotnet ef database update --project BookHub.Infrastructure
```

`DesignTimeDbContextFactory` reads `ConnectionStrings__DefaultConnection` or falls back to the local Docker database.

**Tests** (integration tests need Docker running; they start their own PostgreSQL container):

```bash
dotnet test BookHub.Tests/BookHub.Tests.csproj
```

## 3. API reference

Base path `/api`. Resources: `genres`, `authors`, `books`.

| Method | Route | Success | Notes |
|---|---|---|---|
| GET | `/api/{resource}` | 200 | Paged list: `search`, `page`, `pageSize` (max 100) |
| GET | `/api/{resource}/{id}` | 200 | |
| POST | `/api/{resource}` | 201 | `Location` header points to the new item |
| PUT | `/api/{resource}/{id}` | 200 | Returns the updated item |
| DELETE | `/api/{resource}/{id}` | 204 | |

Extra filters on `GET /api/books`: `authorId`, `genreId`, `sortBy` (`title` default, or `year`), `descending`.
Book responses include `authorName` and `genreName`, so the relationship is visible without extra calls.

List responses: `{ "items": [...], "page": 1, "pageSize": 10, "totalCount": 42, "totalPages": 5 }`.

**Errors** are always `application/problem+json`:

| Situation | Status |
|---|---|
| Invalid data / broken domain rule / unknown author or genre in a book / malformed JSON | 400 |
| Item not found | 404 |
| Duplicate genre name; deleting an author/genre that has books | 409 |
| Anything unexpected | 500 (details logged, never returned to the client) |

## 4. Chosen architecture

A layered ("clean-ish") architecture with dependencies pointing inwards:

```
BookHub.API  ─────────►  BookHub.Application  ─────────►  BookHub.Domain
     │                          ▲
     └──►  BookHub.Infrastructure ┘   (implements Application's abstractions)
```

- **Domain** knows nothing about other layers.
- **Application** defines the repository and unit-of-work interfaces it needs; it does not reference EF Core.
- **Infrastructure** implements those interfaces with EF Core/PostgreSQL (dependency inversion).
- **API** is the composition root: all dependency injection is declared in `Program.cs`.

**Why this shape:** the problem is small, but separating responsibilities keeps rules testable without a database, makes the persistence technology replaceable, and gives new developers an obvious place for each kind of change. I kept it deliberately lean: no CQRS/MediatR, no generic repository, no mapping library.

## 5. Backend organization

```
BookHub.Domain/          Entities (Genre, Author, Book) + DomainException
BookHub.Application/     Services, DTOs, queries, repository interfaces, Not-found/Conflict exceptions
  Genres/ Authors/ Books/   one folder per feature: DTOs + IService + Service
  Abstractions/             IGenreRepository, IAuthorRepository, IBookRepository, IUnitOfWork
  Common/                   PagedResult, ListQuery, BookListQuery
BookHub.Infrastructure/  BookHubContext, EF mappings, repositories, migrations, seed (Persistence/Seed)
BookHub.API/             Controllers, global exception handler, Program.cs
BookHub.Tests/           Domain/, Application/, Integration/
```

Key decisions:

- **Rich entities.** Invariants (required name, length limits, year range, ISBN format) live in the entity constructor/`Update` method, so an invalid entity cannot exist. Setters are private and collections are read-only.
- **Guid identifiers** generated by the application: no database round-trip needed to know an id, safe to expose, and merge-friendly.
- **Referential integrity twice.** Services check business rules and return friendly messages; the database enforces them anyway (`ON DELETE RESTRICT`, unique index on genre name). `BookHubContext.SaveChangesAsync` translates PostgreSQL unique/foreign-key violations into `409`, so concurrent requests that pass the pre-check still get a correct response.
- **Thin controllers.** They only map HTTP to service calls; error-to-status mapping is centralised in `GlobalExceptionHandler`.
- **Search** uses `ILIKE` with escaped wildcards, so searches are case-insensitive and `%`/`_` are treated literally. Pagination is stable (ordered by the sort field, then `Id`).

## 6. Database choice

**PostgreSQL**, because:

- The domain is relational with clear foreign keys, which a relational engine enforces natively.
- Free, open source and identical in local, CI and production environments; it runs in a single Docker image.
- Native `uuid` type fits Guid keys; `ILIKE` gives simple case-insensitive search.
- Strong EF Core support through Npgsql, and Testcontainers makes real-database integration tests cheap.

_Alternatives considered:_ SQL Server (excellent tooling, but heavier image and licensing for non-dev use); MySQL (works, but weaker defaults for constraints and no `ILIKE`).

## 7. Main trade-offs

| Decision | Benefit | Cost |
|---|---|---|
| Layered architecture for a small domain | Testable, evolvable, easy to explain | More files than a single-project API |
| Repository + Unit of Work over EF | Services are unit-testable with mocks; EF stays in one layer | An extra abstraction on top of `DbContext` |
| Exceptions for business failures | Simple service code, one place to map to HTTP | Less explicit than a `Result` type; exceptions as control flow |
| Domain validation reports the first error only | No validation library, rules stay in the entity | Client fixes errors one at a time |
| Simple services, no MediatR/CQRS | Low complexity | Cross-cutting behaviours (validation pipeline) need manual wiring |
| Offset pagination | Easy for a UI with page numbers | Degrades on very large tables |
| Migrations and seed applied on startup in Development only | Zero-setup local run with sample data | Production needs an explicit migration step; seed is all-or-nothing (skipped if any data exists) |

## 8. Testing strategy

106 automated tests, three levels:

- **Domain unit tests (`Domain/`)** — every invariant of the three entities, including ISBN normalisation and year boundaries.
- **Service unit tests (`Application/`)** — business rules with mocked repositories: conflicts, not-found, blocked deletes, "nothing is saved when a rule fails", query normalisation.
- **Integration tests (`Integration/`)** — the real API in-process (`WebApplicationFactory`) against a real PostgreSQL started by Testcontainers. They verify status codes, `ProblemDetails` content type, `Location` header, filters, sorting, paging, and database-enforced rules. The seeder is tested against fresh databases (populates an empty one, is idempotent, and leaves existing data alone). The API tests run with the seed disabled so their data stays deterministic.

Real PostgreSQL was chosen over EF's in-memory provider because the in-memory provider ignores constraints, `ILIKE` and migrations, which are exactly what we want to verify. The cost is a Docker requirement for the integration tests. Tests use unique names instead of cleaning the database, so they are independent and can share one container.

## 9. Known limitations

- **Authentication is scaffolded but not enforced.** JWT bearer validation and the Swagger "Authorize" button are configured, but there is no login endpoint and no `[Authorize]` on controllers.
- **Secrets in configuration.** `Jwt:Key` and the database password are in `appsettings.json` for convenience; they are development values and must be replaced (user secrets / environment variables) for any real deployment.
- **CORS allows any origin** (`AllowAll`), acceptable for local development only.
- **Genre-name uniqueness is case-insensitive in the service but the database index is case-sensitive**; a race between names differing only by case could slip through.
- **ISBN checksum is not verified**, only its format.
- **No optimistic concurrency:** concurrent updates to the same item are last-write-wins.
- **The API is not yet part of `docker-compose.yml`** (only PostgreSQL is); a `Dockerfile` exists for the API but is not wired in.
- **Observability** is limited to default ASP.NET Core logging (no structured/centralised logs, metrics or health checks).

## 10. Improvements with more time

- Enforce authentication/authorization (login endpoint, roles) and move secrets out of the repository.
- Case-insensitive unique index (`citext` or `lower(name)`), and a concurrency token (`xmin`/`RowVersion`).
- FluentValidation (or similar) in a validation pipeline to return **all** errors at once.
- Structured logging (Serilog), OpenTelemetry tracing/metrics, health checks.
- Add the API to `docker-compose.yml` and a CI pipeline (build, test, publish image).
- Restrict CORS to configured origins; add rate limiting.
- Keyset pagination for large datasets; ISBN checksum validation.
- Architecture decision records (ADRs) for the decisions above.

## Frontend organization

_To be completed with the React application (structure, API consumption, state handling, how to run)._
