FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Locator.API/Locator.API.csproj", "Locator.API/"]
COPY ["Locator.Application/Locator.Application.csproj", "Locator.Application/"]
RUN dotnet restore "Locator.API/Locator.API.csproj"
COPY . .
WORKDIR "/src/Locator.API"
RUN dotnet build "./Locator.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Locator.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Locator.API.dll"]

COPY Locator.Application/Sources ./Sources
