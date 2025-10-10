# Use the official .NET 9 runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the official .NET 9 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/YouDovezu.API/YouDovezu.API.csproj", "src/YouDovezu.API/"]
COPY ["src/YouDovezu.Application/YouDovezu.Application.csproj", "src/YouDovezu.Application/"]
COPY ["src/YouDovezu.Infrastructure/YouDovezu.Infrastructure.csproj", "src/YouDovezu.Infrastructure/"]
COPY ["src/YouDovezu.Domain/YouDovezu.Domain.csproj", "src/YouDovezu.Domain/"]

# Restore dependencies
RUN dotnet restore "src/YouDovezu.API/YouDovezu.API.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR "/src/src/YouDovezu.API"
RUN dotnet build "YouDovezu.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "YouDovezu.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YouDovezu.API.dll"]
