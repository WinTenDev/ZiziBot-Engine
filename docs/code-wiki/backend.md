# Backend (.NET) Code Map

Solution: [ZiziBot.slnx](../../ZiziBot.slnx)

## Module Responsibilities

### ZiziBot.Engine (Host / Composition Root)

- What it is: the primary ASP.NET Core host process.
- Entry point: [Program.cs](../../backend/ZiziBot.Engine/Program.cs#L1-L35)
- Responsibilities:
  - Load configuration (dotenv + Mongo config + local overrides)
  - Register services, caching, DB migration, Cortex mediator pipeline
  - Start Telegram bot engine and REST API
  - Start scheduler/job systems

### ZiziBot.Application (Use Cases + Cortex Mediator Pipeline)

- What it is: application-layer logic (requests, handlers, services, scheduler tasks).
- Key wiring:
  - Service composition: [ServiceExtension.ConfigureServices](../../backend/ZiziBot.Application/Extensions/ServiceExtension.cs#L23-L36)
  - Cortex mediator setup + behaviors (+ `IAppMediator` adapter): [CortexExtension.AddApplicationCortexMediator](../../backend/ZiziBot.Application/Extensions/CortexExtension.cs#L7-L35)
  - Pre-handler stage for Telegram guard/short-circuit checks: [PreProcessPipeline](../../backend/ZiziBot.Application/Pipelines/PrePipeline/PreProcessPipeline.cs) + [IPreProcessPipeline](../../backend/ZiziBot.Application/Pipelines/PrePipeline/IPreProcessPipeline.cs)
  - Post-handler stage for Telegram side effects: [PostProcessPipeline](../../backend/ZiziBot.Application/Pipelines/PostPipeline/PostProcessPipeline.cs) + [IPostProcessPipeline](../../backend/ZiziBot.Application/Pipelines/PostPipeline/IPostProcessPipeline.cs)
- Key concepts:
  - Request contracts live under [Core](../../backend/ZiziBot.Application/Core)
  - Handlers are grouped by inbound channel:
    - Telegram: [Handlers/Telegram](../../backend/ZiziBot.Application/Handlers/Telegram)
    - REST: [Handlers/RestApis](../../backend/ZiziBot.Application/Handlers/RestApis)
    - Web console / web: [Handlers/Web](../../backend/ZiziBot.Application/Handlers/Web)

### ZiziBot.Presentation.TelegramBot (Telegram Delivery Layer)

- What it is: Telegram “controller” layer that maps updates/commands to application requests.
- Engine integration:
  - Bot token discovery from Mongo: [ConfigureTelegramBot](../../backend/ZiziBot.Presentation/Bots/Telegram/TelegramExtension.cs#L16-L37)
  - Bot startup: [RunTelegramBot](../../backend/ZiziBot.Presentation/Bots/Telegram/TelegramExtension.cs#L39-L76)
- Controllers:
  - Controllers live at [Bots/Telegram/Controllers](../../backend/ZiziBot.Presentation/Bots/Telegram/Controllers)
  - Example: [ChatController](../../backend/ZiziBot.Presentation/Bots/Telegram/Controllers/ChatController.cs)

### ZiziBot.TelegramBot.Framework (Vendored Framework)

Location: [backend/lib/ZiziBot.TelegramBot](../../backend/lib/ZiziBot.TelegramBot)

- What it is: internal framework for bot routing/middleware and Telegram API interaction.
- Key types:
  - Update routing/dispatch: [BotUpdateHandler](../../backend/lib/ZiziBot.TelegramBot/ZiziBot.TelegramBot.Framework/Handlers/BotUpdateHandler.cs)
  - Polling engine: [BotPollingEngine](../../backend/lib/ZiziBot.TelegramBot/ZiziBot.TelegramBot.Framework/Engines/BotPollingEngine.cs)
  - Controller base: [BotCommandController](../../backend/lib/ZiziBot.TelegramBot/ZiziBot.TelegramBot.Framework/Models/BotCommandController.cs)

### ZiziBot.Presentation.WebApi (REST Delivery Layer)

- What it is: ASP.NET Controllers + middleware for HTTP API.
- API wiring: [RestApiExtension](../../backend/ZiziBot.Presentation/Extensions/RestApiExtension.cs#L21-L193)
- Base controller pattern:
  - [ApiControllerBase](../../backend/ZiziBot.Presentation/Http/Rest/Controllers/ApiControllerBase.cs#L9-L45) standardizes sending requests through the MediatorService and mapping ApiResponseBase to HTTP.
- Cross-cutting:
  - Swagger + validation + JWT: [AddRestApi](../../backend/ZiziBot.Presentation/Extensions/RestApiExtension.cs#L25-L140)
  - Middleware chain: [ConfigureApi](../../backend/ZiziBot.Presentation/Extensions/RestApiExtension.cs#L169-L193)

### ZiziBot.Application.Database (MongoDB + Repositories + Caching)

- What it is: persistence, migrations, and cache backends.
- Mongo EF provider context:
  - [MongoDbContext](../../backend/ZiziBot.Application/Database/MongoDb/MongoDbContext.cs#L10-L149)
  - Collections defined by DbSet fields for each domain entity.
- Data access facade:
  - [DataFacade](../../backend/ZiziBot.Application/Database/Service/DataFacade.cs#L6-L36) aggregates repositories and the MongoDbContext.
- Repositories:
  - [Repository](../../backend/ZiziBot.Application/Database/Repository)
- Outbox sample:
  - Entity: [OutboxMessageEntity](../../backend/ZiziBot.Application/Database/MongoDb/Entities/OutboxMessageEntity.cs)
  - Writer service: [OutboxService](../../backend/ZiziBot.Application/Services/OutboxService.cs)
- Migrations:
  - Runner + steps: [MongoDb/Migrations](../../backend/ZiziBot.Application/Database/MongoDb/Migrations)

### ZiziBot.Application/Common (Shared Contracts — Formerly ZiziBot.Common)

- What it is: DTOs, constants, enums, common exceptions, utilities, vendor API models, custom attributes, and parsers integrated into the main Application project.
- Key subfolders:
  - Attributes: [Attributes](../../backend/ZiziBot.Application/Common/Attributes)
  - Constants + env var names: [Constants/Env](../../backend/ZiziBot.Application/Common/Constants/Env.cs)
  - Utilities: [Utils](../../backend/ZiziBot.Application/Common/Utils)

### ZiziBot.Cli (Command-Line Tooling)

- What it is: a separate console host for maintenance/automation commands.
- Entry point: [Program.cs](../../backend/ZiziBot.Cli/Program.cs)
- Root command structure: [CmdRoot](../../backend/ZiziBot.Cli/CmdRoot.cs)

## Key Types and How They Interact

### ConfigurationExtension

File: [ConfigurationExtension](../../backend/ZiziBot.Application/Extensions/ConfigurationExtension.cs)

- `LoadSettings`: merges dotenv, Mongo-backed configuration, and optional local JSON overrides.
- `ConfigureSettings`: binds strongly-typed config objects into DI (CacheConfig, EngineConfig, JwtConfig, etc.).
- `PrefetchRepository`: preloads feature flags and optionally sets Env values (e.g., Sentry DSN) from the DB.

### ServiceExtension (DI Composition)

File: [ServiceExtension](../../backend/ZiziBot.Application/Extensions/ServiceExtension.cs)

- `ConfigureServices`: the backend’s DI composition root (services + cache + migration + Cortex mediator + background queue + Flurl defaults).
- `ConfigureFlurl`: centralizes outbound HTTP defaults and request/response logging.

### MediatorService (Unified Dispatcher)

File: [MediatorService](../../backend/ZiziBot.Application/Services/MediatorService.cs)

- `EnqueueAsync(BotRequestBase)`: routes bot requests to Hangfire, background queue, or instant execution.
- `EnqueueAsync(ApiRequestBase<T>)`: executes or enqueues API requests depending on execution strategy.
- `Send`: Hangfire bridge method for executing requests.
- Uses `IAppMediator` instead of directly depending on Cortex or controller-local dispatch details, so the mediator implementation can be swapped again later.

### Facades

- [ServiceFacade](../../backend/ZiziBot.Application/Facades/ServiceFacade.cs#L3-L34): aggregates core application services (external integrations, mediator, job service).
- [DataFacade](../../backend/ZiziBot.Application/Database/Service/DataFacade.cs#L6-L36): aggregates MongoDbContext + repositories + cache.

## Where to Add New Features

- New Telegram commands:
  - Add a controller method under [Bots/Telegram/Controllers](../../backend/ZiziBot.Presentation/Bots/Telegram/Controllers)
  - Add a request + handler under [ZiziBot.Application/Handlers/Telegram](../../backend/ZiziBot.Application/Handlers/Telegram)
- New REST endpoints:
  - Add a controller under [Presentation/Http/Rest/Controllers](../../backend/ZiziBot.Presentation/Http/Rest/Controllers)
  - Add an API request + handler under [ZiziBot.Application/Handlers/RestApis](../../backend/ZiziBot.Application/Handlers/RestApis)
- New persistence models:
  - Add an entity under [ZiziBot.Application/Database/MongoDb/Entities](../../backend/ZiziBot.Application/Database/MongoDb/Entities)
  - Add repository logic under [ZiziBot.Application/Database/Repository](../../backend/ZiziBot.Application/Database/Repository)
