# syntax=docker/dockerfile:1.6

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Symbio.API/*.csproj Symbio.API/
COPY Symbio.Core/*.csproj Symbio.Core/
COPY Symbio.Infrastructure/*.csproj Symbio.Infrastructure/
RUN dotnet restore Symbio.API/Symbio.API.csproj

COPY . .
WORKDIR /src/Symbio.API

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Development runtime for docker compose with hot reload via bind mount.
CMD ["dotnet", "watch", "run", "--no-launch-profile", "--urls", "http://0.0.0.0:8080"]
