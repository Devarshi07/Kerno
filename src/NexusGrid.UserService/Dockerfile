FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY src/NexusGrid.Shared/NexusGrid.Shared.csproj src/NexusGrid.Shared/
COPY src/NexusGrid.UserService/NexusGrid.UserService.csproj src/NexusGrid.UserService/
RUN dotnet restore src/NexusGrid.UserService/NexusGrid.UserService.csproj

COPY src/NexusGrid.Shared/ src/NexusGrid.Shared/
COPY src/NexusGrid.UserService/ src/NexusGrid.UserService/
RUN dotnet publish src/NexusGrid.UserService/NexusGrid.UserService.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NexusGrid.UserService.dll"]
