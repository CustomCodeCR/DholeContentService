FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5210

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["src/Dhole.Content.Api/Dhole.Content.Api.csproj", "src/Dhole.Content.Api/"]
COPY ["src/Dhole.Content.Application/Dhole.Content.Application.csproj", "src/Dhole.Content.Application/"]
COPY ["src/Dhole.Content.Contracts/Dhole.Content.Contracts.csproj", "src/Dhole.Content.Contracts/"]
COPY ["src/Dhole.Content.Domain/Dhole.Content.Domain.csproj", "src/Dhole.Content.Domain/"]
COPY ["src/Dhole.Content.Infrastructure/Dhole.Content.Infrastructure.csproj", "src/Dhole.Content.Infrastructure/"]
COPY ["src/Dhole.Content.Persistence/Dhole.Content.Persistence.csproj", "src/Dhole.Content.Persistence/"]
RUN dotnet restore "src/Dhole.Content.Api/Dhole.Content.Api.csproj"
COPY . .
RUN dotnet publish "src/Dhole.Content.Api/Dhole.Content.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Dhole.Content.Api.dll"]
