﻿FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["backend/ZiziBot.Engine/ZiziBot.Engine.csproj", "backend/ZiziBot.Engine/"]
COPY ["backend/ZiziBot.Allowed.TelegramBot/ZiziBot.Allowed.TelegramBot.csproj", "backend/ZiziBot.Allowed.TelegramBot/"]
COPY ["backend/ZiziBot.Application/ZiziBot.Application.csproj", "backend/ZiziBot.Application/"]
COPY ["backend/ZiziBot.Exceptions/ZiziBot.Exceptions.csproj", "backend/ZiziBot.Exceptions/"]
COPY ["backend/ZiziBot.Attributes/ZiziBot.Attributes.csproj", "backend/ZiziBot.Attributes/"]
COPY ["backend/ZiziBot.Parsers/ZiziBot.Parsers.csproj", "backend/ZiziBot.Parsers/"]
COPY ["backend/ZiziBot.Contracts/ZiziBot.Contracts.csproj", "backend/ZiziBot.Contracts/"]
COPY ["backend/ZiziBot.Types/ZiziBot.Types.csproj", "backend/ZiziBot.Types/"]
COPY ["backend/ZiziBot.Utils/ZiziBot.Utils.csproj", "backend/ZiziBot.Utils/"]
COPY ["backend/ZiziBot.DataSource/ZiziBot.DataSource.csproj", "backend/ZiziBot.DataSource/"]
COPY ["backend/ZiziBot.Services/ZiziBot.Services.csproj", "backend/ZiziBot.Services/"]
COPY ["backend/ZiziBot.Hangfire/ZiziBot.Hangfire.csproj", "backend/ZiziBot.Hangfire/"]
COPY ["backend/ZiziBot.Console/ZiziBot.Console.csproj", "backend/ZiziBot.Console/"]
COPY ["backend/ZiziBot.DiscordNet.DiscordBot/ZiziBot.DiscordNet.DiscordBot.csproj", "backend/ZiziBot.DiscordNet.DiscordBot/"]
COPY ["backend/ZiziBot.Infrastructure/ZiziBot.Infrastructure.csproj", "backend/ZiziBot.Infrastructure/"]
COPY ["backend/ZiziBot.Caching/ZiziBot.Caching.csproj", "backend/ZiziBot.Caching/"]
COPY ["backend/ZiziBot.Kot.MongoMigrations/ZiziBot.Kot.MongoMigrations.csproj", "backend/ZiziBot.Kot.MongoMigrations/"]
COPY ["backend/ZiziBot.WebApi/ZiziBot.WebApi.csproj", "backend/ZiziBot.WebApi/"]
COPY ["backend/ZiziBot.Interfaces/ZiziBot.Interfaces.csproj", "backend/ZiziBot.Interfaces/"]
RUN dotnet restore "backend/ZiziBot.Engine/ZiziBot.Engine.csproj"
COPY . .
WORKDIR "/src/backend/ZiziBot.Engine"
RUN dotnet build "ZiziBot.Engine.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ZiziBot.Engine.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ZiziBot.Engine.dll"]
