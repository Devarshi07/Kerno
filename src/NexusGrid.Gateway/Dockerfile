FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy csproj files first for layer caching
COPY src/NexusGrid.Shared/NexusGrid.Shared.csproj src/NexusGrid.Shared/
COPY src/NexusGrid.Gateway/NexusGrid.Gateway.csproj src/NexusGrid.Gateway/
RUN dotnet restore src/NexusGrid.Gateway/NexusGrid.Gateway.csproj

# Copy source and build
COPY src/NexusGrid.Shared/ src/NexusGrid.Shared/
COPY src/NexusGrid.Gateway/ src/NexusGrid.Gateway/
RUN dotnet publish src/NexusGrid.Gateway/NexusGrid.Gateway.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NexusGrid.Gateway.dll"]
