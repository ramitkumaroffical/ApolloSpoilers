# ---------- Backend API Dockerfile (multi-stage) ----------

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY src/ApolloSpoilers.Domain/ApolloSpoilers.Domain.csproj src/ApolloSpoilers.Domain/
COPY src/ApolloSpoilers.Application/ApolloSpoilers.Application.csproj src/ApolloSpoilers.Application/
COPY src/ApolloSpoilers.Infrastructure/ApolloSpoilers.Infrastructure.csproj src/ApolloSpoilers.Infrastructure/
COPY src/ApolloSpoilers.Api/ApolloSpoilers.Api.csproj src/ApolloSpoilers.Api/

RUN dotnet restore src/ApolloSpoilers.Api/ApolloSpoilers.Api.csproj

COPY . .

RUN dotnet publish src/ApolloSpoilers.Api/ApolloSpoilers.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# ---------- Runtime ----------

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .


# Copy wwwroot static files (images)
COPY --from=build /src/src/ApolloSpoilers.Api/wwwroot ./wwwroot


ENV ASPNETCORE_URLS=http://+:8080

ENV ASPNETCORE_ENVIRONMENT=Production


EXPOSE 8080


ENTRYPOINT ["dotnet", "ApolloSpoilers.Api.dll"]