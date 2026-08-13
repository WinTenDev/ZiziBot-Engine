# Core Notes

Repository layout highlights useful for agents and on-boarding:

- Use the `backend/` folder for .NET services, tests, and core application code.
- Use the `frontend/` folder for the TypeScript/monorepo UI code.
- Subprojects (e.g., `backend/lib/ZiziBot.TelegramBot`) may already contain their own `.serena/` configs.

Development tips:

- Build and run backend locally:

  dotnet restore; dotnet build; dotnet run --project backend/ZiziBot.Engine/ZiziBot.Engine.csproj

- Tests: dotnet test ./backend/ZiziBot.Tests

