FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY src/AgriFleet.Api/AgriFleet.Api.csproj src/AgriFleet.Api/
COPY src/AgriFleet.Core/AgriFleet.Core.csproj src/AgriFleet.Core/
COPY src/AgriFleet.Infrastructure/AgriFleet.Infrastructure.csproj src/AgriFleet.Infrastructure/
RUN dotnet restore src/AgriFleet.Api/AgriFleet.Api.csproj

COPY src/ src/
RUN dotnet publish src/AgriFleet.Api/AgriFleet.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "AgriFleet.Api.dll"]
