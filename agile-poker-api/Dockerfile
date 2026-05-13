# 1: Base Runtime

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080


# 2: Build SDK 

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["AgilePoker.Api/AgilePoker.Api.csproj", "AgilePoker.Api/"]
RUN dotnet restore "./AgilePoker.Api/AgilePoker.Api.csproj"

COPY . .
WORKDIR "/src/AgilePoker.Api"
RUN dotnet build "./AgilePoker.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build


# 3: Publish 

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AgilePoker.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


# 4: Final Image 

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "AgilePoker.Api.dll"]