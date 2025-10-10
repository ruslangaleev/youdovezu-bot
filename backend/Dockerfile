FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/YouDovezu.API/YouDovezu.API.csproj", "src/YouDovezu.API/"]
COPY ["src/YouDovezu.Application/YouDovezu.Application.csproj", "src/YouDovezu.Application/"]
COPY ["src/YouDovezu.Domain/YouDovezu.Domain.csproj", "src/YouDovezu.Domain/"]
COPY ["src/YouDovezu.Infrastructure/YouDovezu.Infrastructure.csproj", "src/YouDovezu.Infrastructure/"]
RUN dotnet restore "src/YouDovezu.API/YouDovezu.API.csproj"
COPY . .
WORKDIR "/src/src/YouDovezu.API"
RUN dotnet build "YouDovezu.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "YouDovezu.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YouDovezu.API.dll"]
