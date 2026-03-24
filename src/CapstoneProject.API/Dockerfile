# Base runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# QUAN TRỌNG: listen đúng port Koyeb
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/CapstoneProject.API/CapstoneProject.API.csproj", "src/CapstoneProject.API/"]
COPY ["src/CapstoneProject.Application/CapstoneProject.Application.csproj", "src/CapstoneProject.Application/"]
COPY ["src/CapstoneProject.Domain/CapstoneProject.Domain.csproj", "src/CapstoneProject.Domain/"]
COPY ["src/CapstoneProject.Infrastructure/CapstoneProject.Infrastructure.csproj", "src/CapstoneProject.Infrastructure/"]

RUN dotnet restore "./src/CapstoneProject.API/CapstoneProject.API.csproj"

COPY . .
WORKDIR "/src/src/CapstoneProject.API"

RUN dotnet publish "./CapstoneProject.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CapstoneProject.API.dll"]