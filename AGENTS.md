# ZiziBot-Engine Code Wiki (AGENTS summary)

This file is a concise, agent-focused summary. The full, actionable guidance for coding agents is stored here in `docs/code-wiki/agents.md`.

## Quick Orientation

**Platform & language:** .NET 8 (backend), TypeScript/React (frontend)

**Solution & projects:**
- Backend solution: [ZiziBot.slnx](./ZiziBot.slnx)
- Backend host entrypoint: [backend/ZiziBot.Engine/Program.cs](./backend/ZiziBot.Engine/Program.cs)
- Core backend projects: `backend/ZiziBot.Application/`, `backend/ZiziBot.Presentation/`
- Tests: `backend/ZiziBot.Tests/` (run: `dotnet test ./backend/ZiziBot.Tests`)
- Local library: `backend/lib/ZiziBot.TelegramBot/` (has its own `AGENTS.md` with library-specific notes)
- Frontend monorepo root: [frontend/turborepo-zizibot-console](./frontend/turborepo-zizibot-console)

**Core architectural pattern:** Cortex Mediator request/handler pipeline
- Requests are dispatched through `IAppMediator` with cross-cutting pipeline behaviors (logging, feature flags, RBAC, validation)
- Handlers organized by inbound channel: Telegram, REST API, Web console
- See `backend/ZiziBot.Application/Extensions/CortexExtension.cs` for pipeline setup

**Database & config:**
- **MongoDB required** (must include database name in connection string - missing DB name will cause startup failure)
- Local config: `backend/ZiziBot.Engine/.env.example`
- JSON overrides: place at `backend/ZiziBot.Engine/Storage/AppSettings/Current/` for auto-loading
- Config sources merged in order: dotenv, Mongo-backed config, local JSON overrides

**Key services & integrations:**
- Background jobs: Hangfire (supports both scheduled and instant execution)
- Caching: FusionCache + CacheTower
- HTTP client: Flurl (centralized logging and JSON handling)
- Logging: Serilog (request logging via middleware)

**Quick build & run (from repo root):**

```powershell
dotnet restore
dotnet build
dotnet run --project backend/ZiziBot.Engine/ZiziBot.Engine.csproj
```

**Dev helpers:** `docker-dev.yaml` (local compose targets), `dotnet-cleanup.ps1`

For detailed agent instructions (project layout, edit pointers, build/run, config, DI patterns, safety notes) see: [docs/code-wiki/agents.md](./docs/code-wiki/agents.md).

## Code Organization & Adding Features

**Request/Handler model (Cortex Mediator):**
- All "business actions" follow request → handler pattern through `IAppMediator`
- Requests: `backend/ZiziBot.Application/Core/` (request contracts)
- Handlers: `backend/ZiziBot.Application/Handlers/` (grouped by channel: `Telegram/`, `RestApis/`, `Web/`)
- Pre-process pipeline: feature flags, RBAC, validation, anti-spam (runs before handler)
- Post-process pipeline: Telegram side effects (runs after handler)

**Where to add new features:**
- New Telegram command/feature: Add controller method in `backend/ZiziBot.Presentation/Bots/Telegram/Controllers/`, create request/handler in `backend/ZiziBot.Application/Handlers/Telegram/`
- New REST endpoint: Add controller in `backend/ZiziBot.Presentation/Http/Rest/Controllers/`, create request/handler in `backend/ZiziBot.Application/Handlers/RestApis/`
- New persistence: Add entity in `backend/ZiziBot.Application/Database/MongoDb/Entities/`, add repository logic in `backend/ZiziBot.Application/Database/Repository/`

**Data and service access:**
- Use `DataFacade` to access repositories and MongoDB context
- Use `ServiceFacade` to access mediator, job service, and external client integrations
- Register services via `AddXxx` extension methods (e.g., `AddZiziBotTelegramBot`)

## Patterns & Key Safety Notes

- Dependencies: projects expose `AddXxx` extension methods; register features via `ZiziBot.Application` where possible
- Configuration: follow `IConfiguration` binding patterns in `ConfigurationExtension.cs`; MongoDB connection string MUST include database name
- Logging: **never log secrets, bot tokens, or API keys**; webhook endpoints must not expose tokens in URLs or logs
- Tests: `backend/ZiziBot.Tests/`; keep unit tests fast, use integration tests sparingly
- Telegram bot: reads tokens from `BotSettings` collection in MongoDB; if missing at startup, host will error (add valid row with `Name = "Main"` to proceed)
- Git commits: when split-committing, introduce a random 1-2 minutes delay between commits to simulate realistic developer activity (manually or via custom timestamps).

## Agent Capabilities
- Activate the current dir as project using serena
- Activate the current project with Serena and read initial instructions