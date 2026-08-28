# AGENTS.md

## Project overview

Chinese ERP system for mold manufacturing. Two-part architecture:
- **`backend/`** — .NET 9 Web API (Entity Framework Core 9, SQL Server, JWT auth)
- **`frontend/`** — Vue 3 SPA (Vite 6, Element Plus, Pinia, no TypeScript)

No solution file, no CI, no tests, no linting.

## Default accounts (seeded on first startup)

| Role | Username | Password |
|------|----------|----------|
| admin | `admin` | `admin123` |
| production | `pro` | `123456` |
| warehouse | `wh` | `123456` |
| quality | `qa` | `123456` |
| sales | `sale` | `123456` |

**Note:** README.md lists different usernames (`production`, `warehouse`, `quality`, `sales`). Trust `DbSeeder.cs` — the actual usernames are `pro`, `wh`, `qa`, `sale`.

## Run commands

**Backend** (from `backend/QuanliERP.Api/`):
```sh
dotnet run --urls http://localhost:5199
```
Must be port 5199 — the Vite proxy targets this port. The `launchSettings.json` default (also 5199) matches, but README.md's 5080 is stale.

**Frontend** (from `frontend/`):
```sh
npm run dev        # Vite dev server on :5173, proxies /api -> localhost:5199
npm run build      # outputs to dist/ (gitignored)
```

Start backend first, then frontend. Frontend depends on backend being up.

**Publish (production deploy):** run `npm run build` **before** `dotnet publish`. The `.csproj` has a `CopyFrontendOnPublish` target that copies `frontend/dist/**` into `wwwroot/`, so publish fails/omits the SPA if `dist/` is missing. `dotnet build` alone does not run this target.

## Architecture facts

- **No EF migrations.** DB is created via `EnsureCreated()` + `DbSeeder.Seed()` on first startup. A `dotnet-ef` tool manifest exists (`backend/QuanliERP.Api/.config/dotnet-tools.json`, v10.0.11) but is not used — do not introduce a migrations workflow.
- **Schema changes are destructive for dev:** `EnsureCreated()` only runs when the DB does not exist. Changing models won't update an existing `QuanliERP` DB; drop it (or change the connection string) to re-seed.
- **Generic CRUD base** (`CrudBaseController<T>`): reflection-based keyword search across string properties only. New simple entities inherit this; complex entities get their own controller.
- **Single Axios instance** (`src/api/index.js`) handles all HTTP + JWT. API modules are in `src/api/modules.js`.
- **All routes lazy-loaded** in `src/router/index.js`. Auth guard checks Pinia auth store.
- **`CrudPage.vue`** is the reusable table+dialog component used by most base-data views.
- **Two Pinia stores** (`auth`, `tabs`). Component state is local `ref()`/`reactive()`.
- **`@` alias** resolves to `frontend/src` (Vite + JS).
- **API responses are unwrapped** by the Axios interceptor (`res => res.data`), so frontend code uses the payload directly, not `res.data.data`.
- **401 handling is centralized** in the same interceptor: clears `token`/`user` from `localStorage` and redirects to `/login`. The router guard (`src/router/index.js`) independently checks `localStorage.token`; keep both in sync.
- **JSON serialization:** camelCase on the wire (`JsonNamingPolicy.CamelCase`) + `IgnoreCycles` — do not remove these; navigation properties depend on them.

## Conventions

- **Business logic lives in controllers.** There is no service/repository layer — multi-table side effects (e.g., delivery decrements inventory + writes ledger + updates order status) are inline in controller actions. Follow this pattern for new features.
- **All UI text, comments, seed data, and error messages are Chinese.**
- **Status values are Chinese strings** in the DB (e.g., "进行中", "已完成"), not enums.
- **No TypeScript, no ESLint, no Prettier, no tests.** Frontend is plain JS with `<script setup>` Composition API.
- **JWT token lives in localStorage**, attached via Axios request interceptor.
- **`/api/Users` is admin-only** in the UI (`meta.adminOnly`) and the backend (`UsersController` has `[Authorize(Roles = "admin")]`). The router guard reads `user.role` from `localStorage`.
- **Passwords are BCrypt-hashed** (`BCrypt.Net-Next`); never store plaintext.
- **Single-document numbers** (`SO`/`PO`/`DH`/`SH`/`SC`/`QC`/etc.) embed a timestamp via `DateTime.Now` string formatting — they are business keys, not DB identity/foreign keys.
- **Swagger UI** available at `/swagger` when backend is running.

## Pitfalls

- **Login requires a captcha.** `POST /api/Auth/login` takes `captchaKey` + `captchaCode` fetched from `GET /api/Auth/captcha`. The auth store's `login()` and the login view already handle this.
- **`captchaKey` is stored in a singleton `CaptchaService`** keyed by generated key; captchas do not survive a backend restart. Don't try to call `login` with a stale key.
- **DB requires a local SQL Server** (Windows auth, `Server=.;Database=QuanliERP`). First startup is slow (10–30s) — creating schema + seeding.
- `appsettings.json` has a hardcoded JWT secret — do not commit real credentials.
- No lint/test/typecheck commands exist; verify via `dotnet build` and `npm run build`.
