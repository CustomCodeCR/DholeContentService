# syntax=docker/dockerfile:1

# Base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update && \
  apt-get install -y tzdata apt-utils libgdiplus libc6-dev libgssapi-krb5-2 && \
  ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll && \
  rm -rf /var/lib/apt/lists/*

WORKDIR /app
ENV TZ=America/Costa_Rica

# Build image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG GITHUB_ACTOR=github-actions
WORKDIR /src

COPY ["NuGet.Config", "./NuGet.Config"]
COPY ["src/Dhole.Content.Workers/Dhole.Content.Workers.csproj", "src/Dhole.Content.Workers/"]
COPY ["src/Dhole.Content.Api/Dhole.Content.Api.csproj", "src/Dhole.Content.Api/"]
COPY ["src/Dhole.Content.Application/Dhole.Content.Application.csproj", "src/Dhole.Content.Application/"]
COPY ["src/Dhole.Content.Domain/Dhole.Content.Domain.csproj", "src/Dhole.Content.Domain/"]
COPY ["src/Dhole.Content.Persistence/Dhole.Content.Persistence.csproj", "src/Dhole.Content.Persistence/"]
COPY ["src/Dhole.Content.Infrastructure/Dhole.Content.Infrastructure.csproj", "src/Dhole.Content.Infrastructure/"]
COPY ["src/Dhole.Content.Contracts/Dhole.Content.Contracts.csproj", "src/Dhole.Content.Contracts/"]

RUN --mount=type=secret,id=github_token,required=true \
  export NuGetPackageSourceCredentials_github="Username=${GITHUB_ACTOR};Password=$(cat /run/secrets/github_token);ValidAuthenticationTypes=Basic" && \
  dotnet restore "src/Dhole.Content.Workers/Dhole.Content.Workers.csproj" --configfile /src/NuGet.Config

COPY . .

WORKDIR "/src/src/Dhole.Content.Workers"
RUN dotnet build "Dhole.Content.Workers.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

# Publish image
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/src/Dhole.Content.Workers

RUN dotnet publish "Dhole.Content.Workers.csproj" \
  -c $BUILD_CONFIGURATION \
  -o /app/publish \
  --no-restore \
  /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN chmod 755 /app

ENTRYPOINT ["dotnet", "Dhole.Content.Workers.dll"]
