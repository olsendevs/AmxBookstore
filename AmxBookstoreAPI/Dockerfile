#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AmxBookstoreAPI/AmxBookstoreAPI.csproj", "AmxBookstoreAPI/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["DependencyInjection/DependencyInjection.csproj", "DependencyInjection/"]
COPY ["EventStore/EventStore.csproj", "EventStore/"]
COPY ["Identity/Identity.csproj", "Identity/"]
COPY ["Logging/Logging.csproj", "Logging/"]
COPY ["Persistence/Persistence.csproj", "Persistence/"]
COPY ["Repositories/Repositories.csproj", "Repositories/"]
RUN dotnet restore "./AmxBookstoreAPI/AmxBookstoreAPI.csproj"
COPY . .
WORKDIR "/src/AmxBookstoreAPI"
RUN dotnet build "./AmxBookstoreAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AmxBookstoreAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AmxBookstoreAPI.dll"]