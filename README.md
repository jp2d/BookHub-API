# BookHub

Application to search, register and maintain **genres**, **authors** and **books**, built as a technical challenge.

- **Backend:** .NET 10 REST API (this repository, `BookHub.*` projects)
- **Database:** PostgreSQL 17
- **Frontend:** React SPA in the [BookHub-Front](https://github.com/jp2d/BookHub-Front) repository (see [Frontend organization](#frontend-organization))

---

## 1. Solution overview

| Concern | Choice |
|---|---|
| API | ASP.NET Core 10 controllers, Swagger/OpenAPI (Swashbuckle) |
| Persistence | EF Core 10 + Npgsql, code-first migrations |
| Errors | Global `IExceptionHandler` returning RFC 9457 `ProblemDetails` |
| Logging | Serilog: structured JSON in containers, readable console in Visual Studio; request logging |
| Tests | xUnit, NSubstitute, `WebApplicationFactory`, Testcontainers (real PostgreSQL) |
| Local infra | `docker-compose.yml` with PostgreSQL and the API (multi-stage `Dockerfile`) |

Domain rules implemented:

- A genre has many books; an author has many books; each book belongs to exactly **one author** and **one genre**.
- Genre names are **unique** (case-insensitive). Author names may repeat (two different people can share a name).
- A book requires a title and a publication year (1 up to the current year); ISBN is optional (10 or 13 characters, hyphens/spaces ignored).
- An author or genre that still has books **cannot be deleted** (`409 Conflict`).

## 2. Running locally

**Prerequisites:** Docker (both options) and the .NET 10 SDK (option B and tests). In both options the API is served at `http://localhost:5254`, with Swagger at <http://localhost:5254/swagger>. **Run only one option at a time**, since both use port 5254.

### Option A — Docker (API + database)

```bash
docker compose up -d --build   # builds the API image and starts PostgreSQL + API
docker compose logs -f api     # structured (JSON) logs
docker compose down            # stop; add -v to also delete the database volume
```

The container runs with `ASPNETCORE_ENVIRONMENT=Development` on purpose: it applies migrations, seeds sample data and exposes Swagger, so a reviewer gets a working system with one command. A production deployment would run migrations as an explicit step and use `Production`.

### Option B — Visual Studio (or the .NET CLI)

```bash
docker compose up -d postgres   # database only (db: bookhubdb, user: bookhub, password: bookhubpwd, port 5432)
```

- **Visual Studio 2026:** open `BookHub-API.slnx`. The `BookHub-API.slnLaunch` file makes `BookHub.API` the startup project (or right-click it → *Set as Startup Project*). Pick the `http` or `https` profile and press **F5**; the browser opens at `/swagger`. (`https` serves `https://localhost:7189`; the `http` profile serves `http://localhost:5254`.)
- **.NET CLI:** `dotnet run --project BookHub.API --launch-profile http`

In Development the API applies pending migrations and seeds sample data on startup.

**Sample data (seed).** On startup in Development, after migrations, the API checks whether the database already has any genre, author or book. If it is **completely empty**, it inserts a sample catalogue (8 genres, 10 authors, 25 books) in a single transaction; otherwise it does nothing, so existing data is never touched or duplicated. The data lives in `BookHub.Infrastructure/Persistence/Seed/DatabaseSeeder.cs`. To start from scratch, drop the volume (`docker compose down -v`) and start again, or disable the seed with `Seed:Enabled=false`.

**Configuration** (`BookHub.API/appsettings*.json`, overridable with environment variables):

| Key | Default |
|---|---|
| `ConnectionStrings:DefaultConnection` | `Host=localhost;...` in `appsettings.Development.json`; `Host=postgres;...` in `appsettings.json` (Docker network name; also set explicitly in `docker-compose.yml`) |
| `Seed:Enabled` | `true` (Development only); set to `false` to skip the sample data |
| `Cors:AllowedOrigins` | `http://localhost:5173` and `http://localhost:3000` (React dev servers). Override with `Cors__AllowedOrigins__0=...`; if empty, any origin is allowed |
| `Logging:UseJson` | `false` (readable console); `true` in `docker-compose.yml` for structured JSON logs |

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
| Migrations and seed applied on startup in Development only (the container runs in Development) | Zero-setup local run with sample data, one-command demo | Production needs an explicit migration step; seed is all-or-nothing (skipped if any data exists) |
| No authentication | The challenge defines none; keeps the API and the SPA focused on the required CRUD | Endpoints are open; unsuitable for a public deployment as is |
| Configurable CORS allow-list | Only the SPA origins can call the API from a browser | Origins must be updated when the frontend host/port changes |

## 8. Testing strategy

108 automated tests, three levels:

- **Domain unit tests (`Domain/`)** — every invariant of the three entities, including ISBN normalisation and year boundaries.
- **Service unit tests (`Application/`)** — business rules with mocked repositories: conflicts, not-found, blocked deletes, "nothing is saved when a rule fails", query normalisation.
- **Integration tests (`Integration/`)** — the real API in-process (`WebApplicationFactory`) against a real PostgreSQL started by Testcontainers. They verify status codes, `ProblemDetails` content type, `Location` header, CORS headers, filters, sorting, paging, and database-enforced rules. The seeder is tested against fresh databases (populates an empty one, is idempotent, and leaves existing data alone). The API tests run with the seed disabled so their data stays deterministic.

Real PostgreSQL was chosen over EF's in-memory provider because the in-memory provider ignores constraints, `ILIKE` and migrations, which are exactly what we want to verify. The cost is a Docker requirement for the integration tests. Tests use unique names instead of cleaning the database, so they are independent and can share one container.

## 9. Known limitations

- **No authentication or authorization.** The challenge does not require it, so all endpoints are open (a JWT scaffold was deliberately removed rather than shipping half-implemented security).
- **Credentials in configuration.** The development database password is in `appsettings.json` and `docker-compose.yml`; it must be replaced (secrets store / environment variables) for any real deployment.
- **CORS is an allow-list in configuration**, but falls back to allowing any origin if the list is empty.
- **Genre-name uniqueness is case-insensitive in the service but the database index is case-sensitive**; a race between names differing only by case could slip through.
- **ISBN checksum is not verified**, only its format.
- **No optimistic concurrency:** concurrent updates to the same item are last-write-wins.
- **The container serves plain HTTP** (no TLS termination) and logs a harmless "Failed to determine the https port" warning from the HTTPS-redirection middleware.
- **Observability** is limited to structured console logs: no centralised log shipping, metrics, tracing or health checks.

## 10. Improvements with more time

- Authentication/authorization (login endpoint, roles) and secrets moved out of the repository.
- Case-insensitive unique index (`citext` or `lower(name)`), and a concurrency token (`xmin`/`RowVersion`).
- FluentValidation (or similar) in a validation pipeline to return **all** errors at once.
- Health checks (`/health` with a database probe), OpenTelemetry tracing/metrics, log shipping.
- A CI pipeline (build, test, publish image) and TLS termination in front of the container.
- Rate limiting.
- Keyset pagination for large datasets; ISBN checksum validation.
- Architecture decision records (ADRs) for the decisions above.

## Frontend organization

The frontend is a React 19 + TypeScript single-page application (Vite, MUI, TanStack Query, React Router) in its own repository: **[BookHub-Front](https://github.com/jp2d/BookHub-Front)**. Its README documents the full structure, conventions, decisions, trade-offs, tests and limitations; this is the summary.

**What it does:** list, create, edit and delete genres, authors and books, with server-side search and pagination. The books screen sorts by title/year and shows each book's author and genre as clickable chips that filter the list; "View books" on an author or genre opens the books list already filtered (filters live in the URL, so they survive a reload).

**How it consumes this API:** a thin `fetch` client turns `application/problem+json` responses into typed errors, so the API's messages (400 validation, 409 conflicts) are shown to the user as they are. Generic hooks and components (`createCrudHooks`, `useCrudPage`, `DataTable`, `AsyncAutocomplete`, ...) are configured per entity, so genres, authors and books share the same code. Client-side validation mirrors the domain rules of this API; the API remains the source of truth.

**Organization:** every component is split into markup (`.tsx`), logic (hook `.ts`) and styles (`.module.css`); design tokens come from a single MUI theme exposed as CSS variables. 52 automated tests (Vitest + Testing Library) run without the API because `fetch` is mocked.

**Running the whole system**

```bash
# 1) This repository: API + PostgreSQL (migrations and sample data are applied on startup)
docker compose up -d --build

# 2) The BookHub-Front repository: React app served by nginx
git clone https://github.com/jp2d/BookHub-Front.git
cd BookHub-Front
docker compose up -d --build
```

Open <http://localhost:5173> (API at <http://localhost:5254>, Swagger at <http://localhost:5254/swagger>). For frontend development with hot reload, run `npm install && npm run dev` in the BookHub-Front repository instead of its container. Both use port 5173, which is the origin allowed by this API's CORS allow-list (`Cors:AllowedOrigins`).

**Note on repositories:** the challenge asks for back-end and front-end in one repository. They are kept in two repositories here (independent build and deploy), linked from both READMEs; they can be merged into a monorepo or a submodule without code changes.
