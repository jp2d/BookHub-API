# BookHub

Application to search, register and maintain **genres**, **authors** and **books**, built as a technical challenge. One repository holds the whole solution:

- **Backend:** .NET 10 REST API (`BookHub.*` projects)
- **Frontend:** React single-page application (`BookHub.Web`)
- **Database:** PostgreSQL 17
- **Delivery:** Docker Compose runs the three pieces with one command

**Contents:** [1. Solution overview](#1-solution-overview) · [2. Running locally](#2-running-locally) · [3. API reference](#3-api-reference) · [4. Chosen architecture](#4-chosen-architecture) · [5. Backend organization](#5-backend-organization) · [6. Frontend organization](#6-frontend-organization) · [7. Database choice](#7-database-choice) · [8. Main trade-offs](#8-main-trade-offs) · [9. Testing strategy](#9-testing-strategy) · [10. Known limitations](#10-known-limitations) · [11. Improvements with more time](#11-improvements-with-more-time)

---

## 1. Solution overview

| Concern | Choice |
|---|---|
| API | ASP.NET Core 10 controllers, Swagger/OpenAPI (Swashbuckle) |
| Persistence | EF Core 10 + Npgsql, code-first migrations |
| Errors | Global `IExceptionHandler` returning RFC 9457 `ProblemDetails` |
| Logging | Serilog: structured JSON in containers, readable console in Visual Studio; request logging |
| Frontend | React 19 + TypeScript, built with Vite |
| UI and styles | MUI (Material UI) with the theme exposed as CSS variables; CSS Modules, one file per component |
| Server state and routing | TanStack Query (caching, loading/error states, invalidation); React Router (filters, search and sorting live in the URL) |
| Tests | Back: xUnit, NSubstitute, `WebApplicationFactory`, Testcontainers (real PostgreSQL). Front: Vitest, Testing Library, user-event, jsdom |
| Lint and format | `.editorconfig` + `dotnet format` (back); oxlint + Prettier (front) |
| Delivery | `docker-compose.yml` with PostgreSQL, the API (multi-stage `Dockerfile`) and the web app (Node build → nginx) |

Repository layout:

```
BookHub.Domain/  BookHub.Application/  BookHub.Infrastructure/  BookHub.API/    .NET back-end (layered)
BookHub.Tests/                                                                   back-end tests
BookHub.Web/                                                                     React front-end
docker-compose.yml                                                               PostgreSQL + API + web
```

Domain rules implemented:

- A genre has many books; an author has many books; each book belongs to exactly **one author** and **one genre**.
- Genre names are **unique** (case-insensitive). Author names may repeat (two different people can share a name).
- A book requires a title and a publication year (1 up to the current year); ISBN is optional (10 or 13 characters, hyphens/spaces ignored).
- An author or genre that still has books **cannot be deleted** (`409 Conflict`).

What the user can do in the application:

- **Genres and authors:** list with search and pagination, create, edit and delete. Deleting one that still has books is refused by the API and the message is shown.
- **Books:** list with search, sorting (title, year) and pagination; create, edit and delete.
- **See the relationship book ↔ author ↔ genre:** every book row shows its author and genre as chips. Clicking a chip filters the list by it; active filters are shown as removable chips. "View books" on an author or genre opens the books list already filtered, and the filter survives a page reload because it lives in the URL.
- **Feedback:** inline validation in the forms (same rules as the API), success and error notifications (the server's own message is shown), confirmation before deleting.

## 2. Running locally

**Prerequisites:** Docker for everything; the .NET 10 SDK to develop or test the back-end; Node.js 24 to develop or test the front-end.

| Piece | URL |
|---|---|
| Web application | <http://localhost:5173> |
| API | <http://localhost:5254> (Swagger at <http://localhost:5254/swagger>, health check at `/health`) |
| PostgreSQL | `localhost:5432` (db `bookhubdb`, user `bookhub`, password `bookhubpwd`) |

### Option A — Everything in Docker (recommended)

```bash
docker compose up -d --build   # builds the API and web images; starts PostgreSQL, API and web
docker compose ps              # postgres, api and web become "healthy"
docker compose logs -f api     # structured (JSON) logs
docker compose down            # stop; add -v to also delete the database volume
```

The services start in dependency order (PostgreSQL → API → web), each waiting for the previous one to be healthy. The API container runs with `ASPNETCORE_ENVIRONMENT=Development` on purpose: it applies migrations, seeds sample data and exposes Swagger, so a reviewer gets a working system with one command. A production deployment would run migrations as an explicit step and use `Production`. The web container serves the static bundle with nginx: hashed assets are cached for a year and every other route falls back to `index.html`, so deep links such as `/books?authorId=…` work on reload.

### Option B — Development with hot reload

The API and the web app use ports 5254 and 5173. When you run one of them from your machine, do not start its container.

| Goal | Commands |
|---|---|
| Develop the front-end (API in Docker) | `docker compose up -d postgres api`, then `cd BookHub.Web && npm install && npm run dev` |
| Develop the back-end (database in Docker) | `docker compose up -d postgres`, then run the API from Visual Studio or the CLI (below); add `npm run dev` in `BookHub.Web` if you also need the UI |

- **Visual Studio 2026:** open `BookHub-API.slnx`. The `BookHub-API.slnLaunch` file makes `BookHub.API` the startup project (or right-click it → *Set as Startup Project*). Pick the `http` or `https` profile and press **F5**; the browser opens at `/swagger`. (`https` serves `https://localhost:7189`; the `http` profile serves `http://localhost:5254`.)
- **.NET CLI:** `dotnet run --project BookHub.API --launch-profile http`
- **Front-end:** `npm run dev` serves <http://localhost:5173>. The port is fixed on purpose (`strictPort`) so it always matches the API's CORS allow-list.

In Development the API applies pending migrations and seeds sample data on startup.

**Sample data (seed).** On startup in Development, after migrations, the API checks whether the database already has any genre, author or book. If it is **completely empty**, it inserts a sample catalogue (8 genres, 10 authors, 25 books) in a single transaction; otherwise it does nothing, so existing data is never touched or duplicated. The data lives in `BookHub.Infrastructure/Persistence/Seed/DatabaseSeeder.cs`. To start from scratch, drop the volume (`docker compose down -v`) and start again, or disable the seed with `Seed:Enabled=false`.

**Configuration**

| Key | Default |
|---|---|
| `ConnectionStrings:DefaultConnection` | `Host=localhost;...` in `appsettings.Development.json`; `Host=postgres;...` in `appsettings.json` (Docker network name; also set explicitly in `docker-compose.yml`) |
| `Seed:Enabled` | `true` (Development only); set to `false` to skip the sample data |
| `Cors:AllowedOrigins` | `http://localhost:5173` and `http://localhost:3000` (React dev servers). Override with `Cors__AllowedOrigins__0=...`; if empty, any origin is allowed |
| `Logging:UseJson` | `false` (readable console); `true` in `docker-compose.yml` for structured JSON logs |
| `VITE_API_URL` (front-end) | `http://localhost:5254`. Base URL of the API. Copy `BookHub.Web/.env.example` to `BookHub.Web/.env` for local development. In Docker it is a build argument (`VITE_API_URL=... docker compose up -d --build`); it is baked into the bundle, so changing it requires a rebuild |

Back-end settings live in `BookHub.API/appsettings*.json` and can be overridden with environment variables.

**Database setup without running the API** (e.g. CI or Production):

```bash
dotnet tool install --global dotnet-ef   # once
dotnet ef database update --project BookHub.Infrastructure
```

`DesignTimeDbContextFactory` reads `ConnectionStrings__DefaultConnection` or falls back to the local Docker database.

**Tests**

```bash
dotnet test BookHub.Tests/BookHub.Tests.csproj    # integration tests need Docker running; they start their own PostgreSQL container
cd BookHub.Web && npm ci && npm test              # no API or Docker needed: fetch is mocked
```

**Front-end scripts** (run inside `BookHub.Web`)

| Command | What it does |
|---|---|
| `npm run dev` | Dev server with hot reload |
| `npm run build` | Type-check (`tsc -b`) and production build |
| `npm run lint` | oxlint |
| `npm run format` / `npm run format:check` | Format the code with Prettier / check that it is formatted |
| `npm test` | Runs all tests once |
| `npm run test:watch` | Tests in watch mode |

## 3. API reference

Base path `/api`. Resources: `genres`, `authors`, `books`.

| Method | Route | Success | Notes |
|---|---|---|---|
| GET | `/api/{resource}` | 200 | Paged list: `search`, `page`, `pageSize` (max 100) |
| GET | `/api/{resource}/{id}` | 200 | |
| POST | `/api/{resource}` | 201 | `Location` header points to the new item |
| PUT | `/api/{resource}/{id}` | 200 | Returns the updated item |
| DELETE | `/api/{resource}/{id}` | 204 | |
| GET | `/health` | 200 / 503 | Health check that probes the database (`Healthy` / `Unhealthy`); used by the container's health check |

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

**Back-end.** A layered ("clean-ish") architecture with dependencies pointing inwards:

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

**Front-end.** A feature-oriented SPA with a thin, generic core. Dependencies point in one direction:

```
pages  ──►  components, hooks, validation  ──►  api  ──►  (HTTP) BookHub API
```

- **`api/`** knows how to talk to the API and nothing about screens.
- **`components/`** and **`hooks/`** are generic building blocks (no entity-specific knowledge).
- **`pages/`** only configure those blocks for genres, authors and books.

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
- **Referential integrity twice.** Services check business rules and return friendly messages; the database enforces them anyway (`ON DELETE RESTRICT`, and a unique index on `lower(name)` for genres, so "Terror" and "TERROR" are the same genre even if two concurrent requests race past the service's check). EF Core cannot model expression indexes, so that index is created with raw SQL in the `CaseInsensitiveUniqueGenreName` migration. `BookHubContext.SaveChangesAsync` translates PostgreSQL unique/foreign-key violations into `409`, so concurrent requests that pass the pre-check still get a correct response.
- **Thin controllers.** They only map HTTP to service calls; error-to-status mapping is centralised in `GlobalExceptionHandler`.
- **Search** uses `ILIKE` with escaped wildcards, so searches are case-insensitive and `%`/`_` are treated literally. Pagination is stable (ordered by the sort field, then `Id`).
- **Input hardening.** `pageSize` is capped at 100 and the page number at 1,000,000 (so the offset cannot overflow); request bodies are limited to 1 MB (`413` beyond that); the `Server` header is removed; client errors raised by the server keep their own status instead of becoming `500`.
- **Accurate logs.** The request-logging middleware wraps the exception handler, so a handled `409`/`404`/`400` is logged at Information with its real status, and only genuine `500`s are logged as errors (with the exception).

## 6. Frontend organization

```
BookHub.Web/
  src/
    api/          client, ApiError, buildQuery, config, types, createCrudHooks, genres/authors/books hooks
    components/   generic UI: Layout, DataTable, PageHeader, FormDialog, NameFormDialog, AsyncAutocomplete,
                  SearchField, FilterChips, QueryState, ConfirmDialog, RowActions, Notification
    hooks/        useCrudPage, useListState, useDebouncedValue
    pages/        Genres/, Authors/, Books/ (+ BookFormDialog/)
    validation/   pure validation rules mirroring the API's domain limits
    test/         test helpers (mockApi, renderWithProviders, setup)
    routes.ts     route paths and main navigation
    theme.ts      single source of design tokens
  Dockerfile, nginx.conf, vite.config.ts, package.json, ...
```

### Code conventions

Each file has one responsibility, so a component folder is split by role:

```
components/Name/
  Name.tsx          markup only
  useName.ts        state, effects and handlers (only when there is logic)
  Name.module.css   styles (only when needed)
  index.ts          barrel
```

- **Markup / logic / styles are separated.** Components render; hooks decide; CSS Modules style. There are no inline `sx` or `style` props for layout.
- **Design tokens have one source.** `theme.ts` uses MUI's `cssVariables`, so CSS Modules read `var(--mui-palette-*)`, `var(--mui-spacing)` and `var(--mui-shape-borderRadius)`. `StyledEngineProvider injectFirst` makes the project's CSS override MUI's.
- **Imports use the `@/` alias** (`src`) and barrels (`@/components`, `@/api`), avoiding deep relative paths.
- **`src/api/` is split by responsibility:** `config.ts` (base URL), `ApiError.ts` (error type and parsing of `problem+json`), `buildQuery.ts`, `client.ts` (fetch wrapper), `types.ts`. Routes are constants in `routes.ts`; there are no magic path strings.

### Generic building blocks (reuse over repetition)

A screen for a new entity is mostly configuration. The same blocks serve genres, authors and books:

| Block | Responsibility | Used by |
|---|---|---|
| `createCrudHooks(resource)` | TanStack Query hooks (list, one, create, update, delete) and cache invalidation for any REST resource following the API convention | genres, authors, books |
| `useCrudPage` | Queries, form/confirm dialogs and mutations with user feedback; receives the screen's list state | all three pages |
| `useListState` | Search and pagination state (0-based UI page ↔ 1-based API page) | genres, authors |
| `DataTable` | Table with server-side pagination and optional sorting; screens only describe columns | all three pages |
| `FormDialog` + `NameFormDialog` | Dialog shell and a one-field create/edit dialog | genres, authors (the shell also by books) |
| `AsyncAutocomplete` | Select-one field that searches the API as the user types | book form (author, genre) |
| `QueryState`, `PageHeader`, `RowActions`, `FilterChips`, `ConfirmDialog` | Loading/error/empty states, header, row actions, removable filters, confirmation | all pages |

Genres and authors differ only in their columns, texts and validator. Books add what is genuinely specific: filters/sorting in the URL (`useBooksListState`), chip columns (`booksColumns`) and a multi-field form (`useBookForm`).

### Technical decisions

- **MUI + CSS Modules.** MUI provides accessible, consistent tables, dialogs, forms and an autocomplete, which is the fastest route to a functional and organized interface. Styles still live in separate CSS files, as required by the code conventions, using the theme as the shared token source.
- **TanStack Query for server state.** It removes hand-written loading/error/caching code. `keepPreviousData` avoids flicker while paging. Renaming a genre or author also invalidates the cached books list, whose rows embed those names.
- **No form library.** The forms are small (one field, five fields). Plain state plus pure validation functions keep the dependency list short and make the rules trivially testable.
- **Filters, search and sorting in the URL (books).** "View books of this author" is just a link, a reload keeps the view, and views can be shared. Pagination stays local state.
- **Validation mirrors the API.** The same limits and rules (required fields, name/title length, year between 1 and the current year, ISBN with 10 or 13 digits) give instant feedback; the API remains the source of truth and its messages are displayed when it rejects something.
- **Server-side search, sorting and pagination.** The UI never downloads the whole catalog; the autocompletes ask for at most 20 suggestions at a time, so they keep working with large catalogs.
- **Notifications.** Success messages disappear after 3 s with a fade-out; errors stay 5 s because they need to be read.
- **nginx for the container.** A static bundle needs no Node runtime in production: the image is small and serves deep links correctly.

## 7. Database choice

**PostgreSQL**, because:

- The domain is relational with clear foreign keys, which a relational engine enforces natively.
- Free, open source and identical in local, CI and production environments; it runs in a single Docker image.
- Native `uuid` type fits Guid keys; `ILIKE` gives simple case-insensitive search.
- Strong EF Core support through Npgsql, and Testcontainers makes real-database integration tests cheap.

_Alternatives considered:_ SQL Server (excellent tooling, but heavier image and licensing for non-dev use); MySQL (works, but weaker defaults for constraints and no `ILIKE`).

## 8. Main trade-offs

**Back-end**

| Decision | Benefit | Cost |
|---|---|---|
| Layered architecture for a small domain | Testable, evolvable, easy to explain | More files than a single-project API |
| Repository + Unit of Work over EF | Services are unit-testable with mocks; EF stays in one layer | An extra abstraction on top of `DbContext` |
| Exceptions for business failures | Simple service code, one place to map to HTTP | Less explicit than a `Result` type; exceptions as control flow |
| Domain validation reports the first error only | No validation library, rules stay in the entity | Client fixes errors one at a time |
| Simple services, no MediatR/CQRS | Low complexity | Cross-cutting behaviours (validation pipeline) need manual wiring |
| Offset pagination | Easy for a UI with page numbers | Degrades on very large tables |
| Migrations and seed applied on startup in Development only (the containers run in Development) | Zero-setup local run with sample data, one-command demo | Production needs an explicit migration step; seed is all-or-nothing (skipped if any data exists) |
| No authentication | The challenge defines none; keeps the API and the SPA focused on the required CRUD | Endpoints are open; unsuitable for a public deployment as is |
| Configurable CORS allow-list | Only the SPA origins can call the API from a browser | Origins must be updated when the frontend host/port changes |

**Front-end**

| Decision | Benefit | Cost |
|---|---|---|
| One folder per component (tsx + hook + css + index) | Single responsibility per file, easy to navigate | More files than a single-file component |
| CSS Modules instead of MUI `sx` | Styles separated from markup, plain CSS | Needs `injectFirst` and CSS variables to cooperate with MUI |
| Generic CRUD blocks | New entities are mostly configuration; no duplicated logic | One level of indirection to learn (`createCrudHooks`, `useCrudPage`) |
| No form library | Fewer dependencies, simple tests | Manual wiring if forms grow much larger |
| Client validation duplicates API rules | Instant feedback | Rules must be kept in sync with the back-end |
| API URL baked at build time | Simple static image, no runtime config | Changing the API address needs a rebuild |

**Repository**

| Decision | Benefit | Cost |
|---|---|---|
| Monorepo with a single `docker-compose.yml` | One clone, one command, one place to review; back and front versioned together | They cannot be released independently; the repository mixes two toolchains (.NET and Node) |

## 9. Testing strategy

**Back-end: 128 automated tests, three levels**

- **Domain unit tests (`Domain/`)** — every invariant of the three entities, including ISBN normalisation and year boundaries.
- **Service unit tests (`Application/`)** — business rules with mocked repositories: conflicts, not-found, blocked deletes, "nothing is saved when a rule fails", query normalisation (including the page cap), and the mapping of exceptions to HTTP statuses in the global handler.
- **Integration tests (`Integration/`)** — the real API in-process (`WebApplicationFactory`) against a real PostgreSQL started by Testcontainers. They verify status codes, that every error (including the ones raised by the framework: malformed JSON, wrong media type, unknown route) is `application/problem+json`, `Location` header, CORS headers, filters, sorting, paging, and database-enforced rules. The seeder and the case-insensitive genre index are tested against fresh databases (the seeder populates an empty one, is idempotent and leaves existing data alone; the database itself rejects `"TERROR"` when `"Terror"` exists). `/health` is covered too. The API tests run with the seed disabled so their data stays deterministic.

Real PostgreSQL was chosen over EF's in-memory provider because the in-memory provider ignores constraints, `ILIKE` and migrations, which are exactly what we want to verify. The cost is a Docker requirement for the integration tests. Tests use unique names instead of cleaning the database, so they are independent and can share one container.

**Front-end: 76 automated tests in 7 files**, none of which need the API or Docker because `fetch` is replaced by a mock that also records every request:

- **API client (`api/*.test.ts`)** — query-string building, JSON bodies, `204 No Content`, conversion of `problem+json` and ASP.NET validation errors into `ApiError`, fallback for non-JSON errors, and the "API unreachable" case.
- **Validation (`validation/*.test.ts`)** — required/length rules, ISBN normalisation (10 and 13 digits, check digit `X`), year boundaries, and that the book form reports every error at once.
- **Books screen (`pages/Books/BooksPage.test.tsx`)** — rendered with the same providers as the app: listing with author/genre, empty and error states with retry, chip and URL filters, form validation without calling the API, the exact POST payload (normalised ISBN), a server `400` keeping the dialog open, and delete with confirmation, cancellation and a `409` refusal.
- **Genres and authors screens (`pages/Genres|Authors/*.test.tsx`)** — both screens do the same thing with different texts, so one parametrised suite (`src/test/nameCrudScreenSuite.tsx`) describes the behaviour and each screen supplies its configuration: listing, empty state, debounced server-side search, next page, "View books" navigation, create (validation, trimmed payload, server rejection keeping the dialog open), edit (pre-filled, `PUT` payload) and delete (confirm, cancel, refusal by the API).

The front-end tests assert behaviour the user sees, not implementation details. As a sanity check, deliberately removing the ISBN normalisation was caught by the payload test.

**Code style.** A single `.editorconfig` covers the repository. The back-end is checked with `dotnet format whitespace BookHub-API.slnx --verify-no-changes`; the front-end with `npm run format:check` (Prettier) and `npm run lint` (oxlint).

## 10. Known limitations

- **No authentication or authorization**, in the API or the SPA. The challenge does not require it, so all endpoints are open (a JWT scaffold was deliberately removed rather than shipping half-implemented security).
- **Credentials in configuration.** The development database password is in `appsettings.json` and `docker-compose.yml`; it must be replaced (secrets store / environment variables) for any real deployment.
- **CORS is an allow-list in configuration**, but falls back to allowing any origin if the list is empty.
- **Plain HTTP in the containers** (no TLS termination). The API logs a harmless "Failed to determine the https port" warning from the HTTPS-redirection middleware, and the API address is baked into the web bundle at build time, so changing it needs a rebuild.
- **Observability** is limited to structured console logs and a health check: no centralised log shipping, metrics or tracing.
- **ISBN checksum is not verified**, only its format.
- **No optimistic concurrency:** concurrent updates to the same item are last-write-wins.
- **No end-to-end tests** (for example Playwright): the screens are tested with a mocked `fetch`, never against the real API.
- **UI texts are in Portuguese only**; there is no i18n layer.
- **Bundle size.** The production build is one chunk above 500 kB (mostly MUI); routes are not code-split.
- **Only filters, search and sorting are in the URL**; the current page and page size reset on reload. A filter with an unknown id in the URL shows an empty list and a chip without the name.
- **Accessibility** is not audited beyond accessible names on row actions, chips and form fields.

## 11. Improvements with more time

- Authentication/authorization (login endpoint, roles), secrets moved out of the repository, and TLS in front of the containers.
- A concurrency token (`xmin`/`RowVersion`) for optimistic concurrency.
- FluentValidation (or similar) in a validation pipeline to return **all** errors at once.
- OpenTelemetry tracing/metrics and log shipping.
- A CI pipeline (build, lint, test, publish images).
- Rate limiting.
- Keyset pagination for large datasets; ISBN checksum validation.
- Architecture decision records (ADRs) for the decisions above.
- Front-end: end-to-end tests against the real API (for example Playwright), code-splitting per route, runtime configuration of the API URL (for example an `env.js` generated at container start), pagination in the URL with cancellation of in-flight searches, i18n, a dark theme and an accessibility pass.
