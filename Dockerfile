# syntax=docker/dockerfile:1.7
FROM mcr.microsoft.com/dotnet/sdk:10.0.401 AS restore
WORKDIR /source

COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/Service.Api/Service.Api.csproj src/Service.Api/
COPY src/Service.Application/Service.Application.csproj src/Service.Application/
COPY src/Service.Domain/Service.Domain.csproj src/Service.Domain/
COPY src/Service.Infrastructure/Service.Infrastructure.csproj src/Service.Infrastructure/
RUN dotnet restore src/Service.Api/Service.Api.csproj

FROM restore AS build
COPY src/ src/
RUN dotnet build src/Service.Api/Service.Api.csproj --configuration Release --no-restore

FROM build AS publish
RUN dotnet publish src/Service.Api/Service.Api.csproj \
    --configuration Release \
    --no-build \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=publish --chown=$APP_UID:$APP_UID /app/publish .
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Service.Api.dll"]
