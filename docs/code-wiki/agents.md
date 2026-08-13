# AGENTS: Detailed guidance for coding agents

This file contains detailed, actionable guidance for coding agents working in the ZiziBot-Engine repository. It is the single authoritative AGENTS entry for the project when the docs site is consulted.

## What this doc is

- Expand on the short AGENTS summary in the repo root.
- Provide quick entry points, patterns, and change-safety notes specific to this codebase.

## Key projects / directories

- `backend/ZiziBot.Application/` — core backend project. Holds application services, DI extension registration, and configuration loaders. It also houses all consolidated `Common/` (DTOs, converters, constants) and `Infrastructure/` components since the deprecation of `ZiziBot.Common`. Key file: `Extensions/ConfigurationExtension.cs` (configuration and Mongo load).
- `backend/ZiziBot.Presentation/` — presentation layer (HTTP controllers, bot registration). Telegram bot wiring lives in `Bots/Telegram/TelegramExtension.cs` and controllers under `Bots/Telegram/`.
- `backend/ZiziBot.Engine/` — host project and startup: `Program.cs` orchestrates host startup, registered services, and hosted services.
- `backend/ZiziBot.Cli/` — CLI tooling and small console utilities (`CmdRoot.cs`, `Program.cs`).
- `backend/ZiziBot.Tests/` — unit and integration tests. Run with `dotnet test ./backend/ZiziBot.Tests`.
- `backend/lib/ZiziBot.TelegramBot/` — local library for Telegram bot helpers; it has its own `AGENTS.md` with library-specific instructions.

## Start here (recommended order)

1. Read this file and the root `AGENTS.md` (summary).
2. Review `docs/code-wiki/running.md` for local run prerequisites (Mongo, .NET 8, Node.js for frontend).
3. Inspect `backend/ZiziBot.Engine/Program.cs` and `backend/ZiziBot.Application/Extensions/ConfigurationExtension.cs` to understand host startup and config loading.
4. For Telegram-specific work, open `backend/ZiziBot.Presentation/Bots/Telegram/TelegramExtension.cs` and the controllers under `Bots/Telegram/`.

## Build & Run (quick commands)

From repo root:

```powershell
dotnet restore
dotnet build
dotnet run --project backend/ZiziBot.Engine/ZiziBot.Engine.csproj
```

Frontend (monorepo):

```powershell
cd frontend/turborepo-zizibot-console
pnpm install
pnpm dev
```

See `docs/code-wiki/running.md` for more details (Docker compose for Mongo, env vars, etc.).

## Configuration & secrets

- Example env: `backend/ZiziBot.Engine/.env.example`.
- Local JSON overrides: place files under `backend/ZiziBot.Engine/Storage/AppSettings/Current/` to auto-load local settings (see `ConfigurationExtension.cs`).
- Bot tokens and settings are stored in MongoDB (`BotSettings` collection) and read at startup by `TelegramExtension`.

Do not commit real tokens or secrets. Prefer environment variables for local runs.

## Patterns & conventions agents should follow

- Dependency Injection: projects expose `AddXxx` extension methods (e.g., `AddZiziBotTelegramBot`). Register features via `ZiziBot.Application` where possible.
- Configuration: follow the `IConfiguration` binding patterns used in `ConfigurationExtension.cs`; core config expects a MongoDB connection string with a database name.
- Logging: avoid logging secrets or bot tokens. Webhook endpoints should not expose tokens/keys in URLs or logs.
- Tests: tests live in `backend/ZiziBot.Tests/`. Keep unit tests fast and use integration tests only when necessary.

## Telegram bot specifics

- Entry points: `backend/ZiziBot.Presentation/Bots/Telegram/TelegramExtension.cs` — reads tokens from Mongo and registers bot services.
- Middleware & update handling: controllers under `Bots/Telegram/` and the update handling pipeline.
- If no bot tokens exist in Mongo, the host will insert a placeholder and stop with an error; add a valid `BotSettings` row (Name = "Main") to proceed.

## Local libraries

- `backend/lib/ZiziBot.TelegramBot/` contains a framework and a sample app. See its `AGENTS.md` for library-specific notes and a minimal sample `Program.cs`.

## Dev helpers and scripts

- `docker-dev.yaml` — local compose targets (useful to start Mongo locally).
- `dotnet-cleanup.ps1` — cleanup helper script for local development.

## Change safety notes (short)

- Prefer small, surgical changes; follow existing DI and extension patterns.
- When editing host startup or configuration loading, thoroughly test local startup with a local Mongo (see `docker-dev.yaml`).
- Avoid committing secrets and ensure logging does not include tokens.
- When split-committing, introduce a random 1-2 minutes delay between commits to simulate realistic developer activity (manually or via custom timestamps).

## Where to add more details

If you need to expand agent guidance, add short, actionable entries here or in `docs/code-wiki/` and link from the root `AGENTS.md` summary.

