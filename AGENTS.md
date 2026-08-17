# AGENTS.md

## Project overview

Chinese ERP system for mold manufacturing. Two-part architecture:
- **`backend/`** — .NET 9 Web API (Entity Framework Core 9, SQL Server, JWT auth)
- **`frontend/`** — Vue 3 SPA (Vite 6, Element Plus, Pinia, no TypeScript)

No monorepo, no solution file, no CI, no tests, no linting.

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
dotnet run --urls http://localhost:5080
```
Must be port 5080 — the Vite proxy targets this port. The `launchSettings.json` default (5199) is wrong for local dev.

**Frontend** (from `frontend/`):
```sh
npm run dev        # Vite dev server on :5173, proxies /api -> localhost:5080
npm run build      # outputs to dist/ (already committed, no .gitignore)
```

Start backend first, then frontend. Frontend depends on backend being up.

## Architecture facts

- **No EF migrations.** DB is created via `EnsureCreated()` + `DbSeeder.Seed()` on first startup.
- **Generic CRUD base** (`CrudBaseController<T>`): reflection-based keyword search across string properties. New simple entities inherit this; complex entities get their own controller.
- **Single Axios instance** (`src/api/index.js`) handles all HTTP + JWT. API modules are in `src/api/modules.js`.
- **All routes lazy-loaded** in `src/router/index.js`. Auth guard checks Pinia auth store.
- **`CrudPage.vue`** is the reusable table+dialog component used by most base-data views.
- **One Pinia store** (`auth`). Component state is local `ref()`/`reactive()`.

## Conventions

- **All UI text, comments, seed data, and error messages are Chinese.**
- **Status values are Chinese strings** in the DB (e.g., "进行中", "已完成"), not enums.
- **No TypeScript, no ESLint, no Prettier, no tests.** Frontend is plain JS with `<script setup>` Composition API.
- **JWT token lives in localStorage**, attached via Axios request interceptor.
- **Swagger UI** available at `/swagger` when backend is running.

## Pitfalls

- `appsettings.json` has a hardcoded JWT secret — do not commit real credentials.
- `frontend/dist/` is committed; rebuild with `npm run build` after frontend changes.
- No `.gitignore` at project root — be careful with `git add .`.
- The `CrudBaseController` search is fragile with non-string properties — verify before adding new searchable fields.
