# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project state

SistemaBancario ("banking system") is an ASP.NET Core **minimal API** on **.NET 10** (`net10.0`, `Nullable` and `ImplicitUsings` enabled). It exposes the banking requirements under `/api/clientes`, `/api/contas` and `/api/transferencias`, and `sistemabancario.client/` holds its front-end (see below). The domain lives in the `SistemaBancario.Dominio` class library (entities, `IdadeNaoPermitidaException`, and the provider-agnostic EF Core `BancoDbContext` with Fluent API configurations and `HasData` seed for RF-18). The API registers it with **SQLite** (connection string `ConnectionStrings:Banco` → `sistemabancario.db` in the project root) and applies pending migrations at startup via `Database.Migrate()`. There are no tests. Requirements are in `requisitos-funcionais.md` (RF-xx IDs are referenced in code comments).

## Commands

```bash
dotnet build                          # build (solution file: SistemaBancario.slnx)
dotnet run                            # run with the "http" profile → http://localhost:5285
dotnet run --launch-profile https     # https://localhost:7091 + http://localhost:5285
dotnet watch                          # run with hot reload
dotnet tool restore                   # restores the local dotnet-ef 10 (dotnet-tools.json)
dotnet dotnet-ef migrations add Nome  # new migration (lands in ./Migrations of the API project)
```

- Use the local `dotnet dotnet-ef`, not a global `dotnet ef`: a global 8.x tool cannot load EF Core 10.
- Migrations live in the API project (`MigrationsAssembly` in `Program.cs`), not in `SistemaBancario.Dominio`, so the domain stays provider-agnostic. Changing `HasData` or any mapping requires a new migration; EF refuses to `Migrate()` with pending model changes.
- SQLite stores `decimal` as TEXT: equality works in SQL, but comparisons (`<`, `>`) and `OrderBy` on `Saldo` are not translated — avoid them in LINQ queries or evaluate client-side.

- Both launch profiles set `ASPNETCORE_ENVIRONMENT=Development`, which is what enables the OpenAPI document at `/openapi/v1.json` (`app.MapOpenApi()` is gated on `IsDevelopment()`). There is no Swagger UI package installed.
- `SistemaBancario.http` holds manual request samples (VS / VS Code REST client); update it when adding endpoints.
- No test project exists. If one is added (e.g. `dotnet new xunit -o SistemaBancario.Tests` and add it to the `.slnx`), run a single test with `dotnet test --filter "FullyQualifiedName~TestName"`.

## Architecture

- `Program.cs` (top-level statements) registers services and middleware and calls the `Map*` extension methods in `Endpoints/` (one static class per route group; request/response records in `Endpoints/Contratos.cs`). Errors are returned as ProblemDetails, whose `detail` field the front-end displays.
- Configuration comes from `appsettings.json` / `appsettings.{Environment}.json` (logging and `ConnectionStrings:Banco`).
- The csproj sits at the repository root alongside `Program.cs`, so any new project added inside this folder (e.g. a test project) will be globbed into the main project's compilation unless it is listed in `DefaultItemExcludes` in `SistemaBancario.csproj` (as `SistemaBancario.Dominio` and `sistemabancario.client` are) or placed in a sibling directory.
- Domain entities keep private setters and enforce rules in constructors/methods; EF mapping is done only via `IEntityTypeConfiguration<T>` classes in `Persistencia/Configuracoes` (no data annotations on entities). `ContaCorrente.Saldo` is a `decimal(18,2)` concurrency token.

## Front-end (`sistemabancario.client/`)

Vue 3 + Vuetify 4 + Pinia + Vue Router, built with Vite (TypeScript). It is not part of the .NET build; run npm commands from that folder:

```bash
npm install
npm run dev        # Vite dev server → http://localhost:3000 (falls back to 3001+ if taken)
npm run build      # vue-tsc type-check + vite build
npm run type-check
npm run lint       # eslint-config-vuetify; `npm run lint:fix` fixes style and import order
```

- API calls go through `src/api/http.ts` / `src/api/index.ts` using relative `/api/...` URLs, which rely on a Vite `server.proxy` forwarding `/api` to the .NET API (the API has no CORS policy). The API must be listening on port 5285 for `npm run dev` to work (both launch profiles do). `UseHttpsRedirection()` is skipped in Development on purpose: a 307 to `https://localhost:7091` would make the browser leave the proxy and fail on CORS.
- `src/api/tipos.ts` mirrors the records in `Endpoints/Contratos.cs` by hand — change both together. Non-2xx responses become `ApiError` with the ProblemDetails `detail` as message.
- State lives in Pinia setup stores: `stores/banco.ts` holds clients + accounts and wraps every mutation (`executar`) to notify and reload both lists, since they affect each other (titular, new account on client signup); `stores/notificacoes.ts` feeds the `<v-snackbar-queue>` in `App.vue`.
- Pages (`src/pages/`) are registered manually in `src/router/index.ts` (no file-based routing): Painel `/`, `/clientes`, `/contas`, `/transferencias`. Shared pieces in `src/components/` (`ConsultaPorId` for RF-01/RF-10, form dialogs, `ConfirmacaoDialog`, `SituacaoChip`); form rules and BRL formatting in `src/utils/formatacao.ts`.
- Vuetify is configured with `utilities: false` and locale `pt`: Vuetify utility classes (`d-flex`, `ma-4`, `text-h5`…) do **not** exist — use Tailwind classes for layout/typography. Vuetify 4 also uses MD3 elevation (0–5) and a gap-based grid.
- UI text and identifiers are in Portuguese, matching the back-end.
