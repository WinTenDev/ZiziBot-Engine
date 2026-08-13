# Running Locally

## Prerequisites

- .NET 8 SDK (backend)
- MongoDB (required)
- Node.js >= 20 and pnpm (frontend)
- Docker (optional, for local MongoDB and containers)

## Backend: ZiziBot.Engine

### 1) Start MongoDB

Using docker compose:

```bash
docker compose -f docker-dev.yaml up -d mongo
```

Compose file: [docker-dev.yaml](../../docker-dev.yaml#L1-L20)

### 2) Configure Environment Variables

Start from the example file:

- [.env.example](../../backend/ZiziBot.Engine/.env.example)

At minimum:

- `MONGODB_CONNECTION_STRING` must be set and include a database name.
  - Config load will throw if the DB name is missing: [AddMongoConfigurationSource](../../backend/ZiziBot.Application/Extensions/ConfigurationExtension.cs#L84-L97)
- `TELEGRAM_WEBHOOK_URL` is read during bot configuration:
  - [ConfigureTelegramBot](../../backend/ZiziBot.Presentation/Bots/Telegram/TelegramExtension.cs#L16-L37)

Optional local JSON overrides:

- Place JSON config files under `backend/ZiziBot.Engine/Storage/AppSettings/Current/`.
- These are auto-loaded when the folder exists:
  - [LoadLocalSettings](../../backend/ZiziBot.Application/Extensions/ConfigurationExtension.cs#L44-L60)

### 3) Run the host

From repo root:

```bash
dotnet restore
dotnet run --project backend/ZiziBot.Engine/ZiziBot.Engine.csproj
```

Host startup orchestration is in [Program.cs](../../backend/ZiziBot.Engine/Program.cs#L6-L35).

### 4) First run: bot tokens in MongoDB

Bot tokens are read from the `BotSettings` collection.

If none exist, the host will insert a placeholder token and stop with an error:

- [RunTelegramBot](../../backend/ZiziBot.Presentation/Bots/Telegram/TelegramExtension.cs#L39-L76)

Fix by inserting a valid bot token row into `BotSettings` (at least one with `Name = "Main"`).

## Frontend: Turborepo workspace

Workspace root: [frontend/turborepo-zizibot-console](../../frontend/turborepo-zizibot-console)

### Install and run

```bash
cd frontend/turborepo-zizibot-console
pnpm install
pnpm dev
```

Or run the console-focused dev command:

```bash
pnpm zizibot-console
```

Scripts: [package.json](../../frontend/turborepo-zizibot-console/package.json#L6-L15)

## Docker Build (Backend)

Build and run the backend container:

```bash
docker build -t zizibot-engine .
docker run --rm -p 80:80 --env-file backend/ZiziBot.Engine/.env.example zizibot-engine
```

Dockerfile: [Dockerfile](../../Dockerfile#L1-L31)
