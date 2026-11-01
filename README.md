# BookHub Front

React single-page application to **search, register and maintain genres, authors and books**. It is the frontend of the BookHub technical challenge and consumes the REST API from the [BookHub-API](https://github.com/jp2d/BookHub-API) repository (.NET 10 + PostgreSQL).

---

## 1. Solution overview

| Concern | Choice |
|---|---|
| Framework | React 19 + TypeScript, built with Vite |
| UI | MUI (Material UI) with the theme exposed as CSS variables |
| Styles | CSS Modules, one file per component |
| Server state | TanStack Query (caching, loading/error states, invalidation) |
| Routing | React Router (filters, search and sorting live in the URL) |
| Tests | Vitest, Testing Library, user-event, jsdom |
| Lint | oxlint |
| Delivery | Multi-stage `Dockerfile` (Node build → nginx) and `docker-compose.yml` |

**What the user can do**

- **Genres and authors:** list with search and pagination, create, edit and delete. Deleting one that still has books is refused by the API and the message is shown.
- **Books:** list with search, sorting (title, year) and pagination; create, edit and delete.
- **See the relationship book ↔ author ↔ genre:** every book row shows its author and genre as chips. Clicking a chip filters the list by it; active filters are shown as removable chips. "View books" on an author or genre opens the books list already filtered, and the filter survives a page reload because it lives in the URL.
- **Feedback:** inline validation in the forms (same rules as the API), success and error notifications (the server's own message is shown), confirmation before deleting.

## 2. Running the application

**Prerequisites:** Docker (both options) and Node.js 24 (option B and the tests). The frontend needs the API running at `http://localhost:5254`.

### The whole system (recommended)

```bash
# 1) API + PostgreSQL (from the BookHub-API repository; applies migrations and seeds sample data)
docker compose up -d --build

# 2) Frontend (from this repository)
docker compose up -d --build
```

Open <http://localhost:5173>. Stop each stack with `docker compose down` (add `-v` in the API repository to also drop the database volume).

### Option A — Frontend in Docker

`docker compose up -d --build` builds the static bundle and serves it with nginx on port **5173** (the origin allowed by the API's CORS configuration). The container has a health check, caches the hashed assets for a year, and falls back to `index.html` for every other route, so deep links such as `/books?authorId=…` work on reload.

### Option B — Local development (hot reload)

```bash
npm install
npm run dev        # http://localhost:5173
```

Option A and option B both use port 5173: stop the container (`docker compose down`) before running `npm run dev`. The port is fixed on purpose (`strictPort`) so it always matches the API's CORS allow-list.

### Configuration

| Variable | Default | Notes |
|---|---|---|
| `VITE_API_URL` | `http://localhost:5254` | Base URL of the API. Copy `.env.example` to `.env` for local development. For Docker it is a build argument (`VITE_API_URL=... docker compose up -d --build`); it is baked into the bundle, so changing it requires a rebuild. |

### Scripts

| Command | What it does |
|---|---|
| `npm run dev` | Dev server with hot reload |
| `npm run build` | Type-check (`tsc -b`) and production build |
| `npm run lint` | oxlint |
| `npm test` | Runs all tests once (no API or Docker needed: `fetch` is mocked) |
| `npm run test:watch` | Tests in watch mode |

## 3. Chosen architecture

A feature-oriented SPA with a thin, generic core. Dependencies point in one direction:

```
pages  ──►  components, hooks, validation  ──►  api  ──►  (HTTP) BookHub API
```

- **`api/`** knows how to talk to the API and nothing about screens.
- **`components/`** and **`hooks/`** are generic building blocks (no entity-specific knowledge).
- **`pages/`** only configure those blocks for genres, authors and books.

## 4. Frontend organization

```
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

## 5. Main technical decisions

- **MUI + CSS Modules.** MUI provides accessible, consistent tables, dialogs, forms and an autocomplete, which is the fastest route to a functional and organized interface. Styles still live in separate CSS files, as required by the code conventions, using the theme as the shared token source.
- **TanStack Query for server state.** It removes hand-written loading/error/caching code. `keepPreviousData` avoids flicker while paging. Renaming a genre or author also invalidates the cached books list, whose rows embed those names.
- **No form library.** The forms are small (one field, five fields). Plain state plus pure validation functions keep the dependency list short and make the rules trivially testable.
- **Filters, search and sorting in the URL (books).** "View books of this author" is just a link, a reload keeps the view, and views can be shared. Pagination stays local state.
- **Validation mirrors the API.** The same limits and rules (required fields, name/title length, year between 1 and the current year, ISBN with 10 or 13 digits) give instant feedback; the API remains the source of truth and its messages are displayed when it rejects something.
- **Server-side search, sorting and pagination.** The UI never downloads the whole catalog; the autocompletes ask for at most 20 suggestions at a time, so they keep working with large catalogs.
- **Notifications.** Success messages disappear after 3 s with a fade-out; errors stay 5 s because they need to be read.
- **nginx for the container.** A static bundle needs no Node runtime in production: the image is small and serves deep links correctly.

## 6. Main trade-offs

| Decision | Benefit | Cost |
|---|---|---|
| One folder per component (tsx + hook + css + index) | Single responsibility per file, easy to navigate | More files than a single-file component |
| CSS Modules instead of MUI `sx` | Styles separated from markup, plain CSS | Needs `injectFirst` and CSS variables to cooperate with MUI |
| Generic CRUD blocks | New entities are mostly configuration; no duplicated logic | One level of indirection to learn (`createCrudHooks`, `useCrudPage`) |
| No form library | Fewer dependencies, simple tests | Manual wiring if forms grow much larger |
| Client validation duplicates API rules | Instant feedback | Rules must be kept in sync with the back-end |
| API URL baked at build time | Simple static image, no runtime config | Changing the API address needs a rebuild |
| Separate repository from the API | Independent build and deploy | Two repositories to clone (see limitations) |

## 7. Testing strategy

52 automated tests (`npm test`), none of which need the API or Docker because `fetch` is replaced by a mock that also records every request:

- **API client (`api/*.test.ts`)** — query-string building, JSON bodies, `204 No Content`, conversion of `problem+json` and ASP.NET validation errors into `ApiError`, fallback for non-JSON errors, and the "API unreachable" case.
- **Validation (`validation/*.test.ts`)** — required/length rules, ISBN normalisation (10 and 13 digits, check digit `X`), year boundaries, and that the book form reports every error at once.
- **Books screen (`pages/Books/BooksPage.test.tsx`)** — rendered with the same providers as the app: listing with author/genre, empty and error states with retry, chip and URL filters, form validation without calling the API, the exact POST payload (normalised ISBN), a server `400` keeping the dialog open, and delete with confirmation, cancellation and a `409` refusal.

The tests assert behaviour the user sees, not implementation details. As a sanity check, deliberately removing the ISBN normalisation was caught by the payload test.

## 8. Known limitations

- **Two repositories.** The challenge asks for back-end and front-end in one repository; this front-end lives in its own repository and is linked from the API's README. They can be consolidated (monorepo or submodule) without code changes.
- **Genres and authors screens have no tests of their own.** They use the same generic blocks that are exercised through the books screen. There are no end-to-end tests (for example Playwright).
- **No authentication or authorization** (the API has none either).
- **UI texts are in Portuguese only**; there is no i18n layer.
- **Bundle size.** The production build is one chunk above 500 kB (mostly MUI); routes are not code-split.
- **Only filters, search and sorting are in the URL**; the current page and page size reset on reload.
- **A filter with an unknown id in the URL** shows an empty list and a chip without the name.
- **Plain HTTP in the container** (no TLS termination) and no runtime configuration of the API address.
- **Accessibility** is not audited beyond accessible names on row actions, chips and form fields.

## 9. Improvements with more time

- End-to-end tests for the main flows and tests for the genres/authors screens.
- Code-splitting per route (lazy loading) to reduce the initial bundle.
- Runtime configuration of the API URL (for example an `env.js` generated at container start).
- Keep pagination in the URL; debounce and cancel in-flight searches.
- i18n, dark theme and an accessibility pass.
- Authentication once the API supports it, and TLS in front of the container.
- A CI pipeline (lint, type-check, test, build and publish the image).
