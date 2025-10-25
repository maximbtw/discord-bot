FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY Directory.Packages.props ./
COPY ["bot/Bot.Host/Bot.Host.csproj", "Bot.Host/"]
COPY ["bot/Bot.Contracts/Bot.Contracts.csproj", "Bot.Contracts/"]
COPY ["bot/Bot.Application/Bot.Application.csproj", "Bot.Application/"]
COPY ["bot/Bot.Domain/Bot.Domain.csproj", "Bot.Domain/"]
COPY ["bot/Bot.Commands/Bot.Commands.csproj", "Bot.Commands/"]

RUN dotnet restore "Bot.Host/Bot.Host.csproj"

COPY ["bot/Bot.Host/", "Bot.Host/"]
COPY ["bot/Bot.Contracts/", "Bot.Contracts/"]
COPY ["bot/Bot.Application/", "Bot.Application/"]
COPY ["bot/Bot.Domain/", "Bot.Domain/"]
COPY ["bot/Bot.Commands/", "Bot.Commands/"]

WORKDIR "/src/Bot.Host"
RUN dotnet publish "Bot.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
RUN apt-get update && apt-get install -y curl
ENTRYPOINT ["dotnet", "Bot.Host.dll"]