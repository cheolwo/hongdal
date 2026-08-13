FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["Ssalddel.Simulation.Server/Ssalddel.Simulation.Server.csproj", "Ssalddel.Simulation.Server/"]
COPY ["Ssalddel.Simulation.Application/Ssalddel.Simulation.Application.csproj", "Ssalddel.Simulation.Application/"]
COPY ["Ssalddel.Simulation.Contracts/Ssalddel.Simulation.Contracts.csproj", "Ssalddel.Simulation.Contracts/"]
COPY ["Ssalddel.Simulation.Domain/Ssalddel.Simulation.Domain.csproj", "Ssalddel.Simulation.Domain/"]
COPY ["Ssalddel.Simulation.Infrastructure/Ssalddel.Simulation.Infrastructure.csproj", "Ssalddel.Simulation.Infrastructure/"]
COPY ["Ssalddel.Simulation.Persistence/Ssalddel.Simulation.Persistence.csproj", "Ssalddel.Simulation.Persistence/"]
COPY ["Ssalddel.Contracts/Ssalddel.Contracts.csproj", "Ssalddel.Contracts/"]
COPY ["Ssalddel.Domain/Ssalddel.Domain.csproj", "Ssalddel.Domain/"]
COPY ["Ssalddel.Infrastructure/Ssalddel.Infrastructure.csproj", "Ssalddel.Infrastructure/"]
COPY ["Ssalddel.WorkflowRules.Contracts/Ssalddel.WorkflowRules.Contracts.csproj", "Ssalddel.WorkflowRules.Contracts/"]
COPY ["Ssalddel.WorkflowRules/Ssalddel.WorkflowRules.csproj", "Ssalddel.WorkflowRules/"]
RUN dotnet restore "./Ssalddel.Simulation.Server/Ssalddel.Simulation.Server.csproj"
COPY . .
WORKDIR "/src/Ssalddel.Simulation.Server"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Ssalddel.Simulation.Server.csproj" \
    -c $BUILD_CONFIGURATION \
    --no-restore \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app
USER root
RUN mkdir -p /app/logs \
    && chown -R $APP_UID:$APP_UID /app/logs
COPY --from=publish --chown=$APP_UID:$APP_UID /app/publish .
USER $APP_UID
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl --fail --silent --show-error http://127.0.0.1:8080/health/ready || exit 1
ENTRYPOINT ["dotnet", "Ssalddel.Simulation.Server.dll"]
