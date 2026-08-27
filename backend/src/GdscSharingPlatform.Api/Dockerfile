FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["global.json", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["src/GdscSharingPlatform.Api/GdscSharingPlatform.Api.csproj", "src/GdscSharingPlatform.Api/"]
COPY ["src/GdscSharingPlatform.Application/GdscSharingPlatform.Application.csproj", "src/GdscSharingPlatform.Application/"]
COPY ["src/GdscSharingPlatform.Infrastructure/GdscSharingPlatform.Infrastructure.csproj", "src/GdscSharingPlatform.Infrastructure/"]
COPY ["src/GdscSharingPlatform.Domain/GdscSharingPlatform.Domain.csproj", "src/GdscSharingPlatform.Domain/"]

RUN dotnet restore "src/GdscSharingPlatform.Api/GdscSharingPlatform.Api.csproj"

COPY . .

RUN dotnet publish "src/GdscSharingPlatform.Api/GdscSharingPlatform.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "GdscSharingPlatform.Api.dll"]
