FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY src/NexusGrid.Shared/NexusGrid.Shared.csproj src/NexusGrid.Shared/
COPY src/NexusGrid.OrderService/NexusGrid.OrderService.csproj src/NexusGrid.OrderService/
RUN dotnet restore src/NexusGrid.OrderService/NexusGrid.OrderService.csproj

COPY src/NexusGrid.Shared/ src/NexusGrid.Shared/
COPY src/NexusGrid.OrderService/ src/NexusGrid.OrderService/
RUN dotnet publish src/NexusGrid.OrderService/NexusGrid.OrderService.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NexusGrid.OrderService.dll"]
