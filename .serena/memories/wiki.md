# ZiziBot-Engine Project Wiki

This memory contains high-level notes about the repository and where to find important components.

- Root solution: `ZiziBot.sln` / `ZiziBot.slnx`
- Backend host entrypoint: `backend/ZiziBot.Engine/Program.cs`
- Core backend projects: `backend/ZiziBot.Application/`, `backend/ZiziBot.Presentation/`
- Frontend monorepo: `frontend/turborepo-zizibot-console/`
- Local library: `backend/lib/ZiziBot.TelegramBot/` (has its own `.serena/` and `AGENTS.md`)

Quick commands (backend):

- dotnet restore; dotnet build; dotnet run --project backend/ZiziBot.Engine/ZiziBot.Engine.csproj

See `AGENTS.md` at repository root for additional agent-focused guidance.

