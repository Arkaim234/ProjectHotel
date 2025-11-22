# Базовый образ с рантаймом .NET
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копируем csproj
COPY ["MiniHttpServer/MiniHttpServer.csproj", "MiniHttpServer/"]
COPY ["MiniHttpServer.Repository/MiniHttpServer.Repository.csproj", "MiniHttpServer.Repository/"]
COPY ["MyORMLibrary/MyORMLibrary.csproj", "MyORMLibrary/"]
COPY ["MiniHttpServer.Frimework/MiniHttpServer.Frimework.csproj", "MiniHttpServer.Frimework/"]
COPY ["MiniTemplateEngine/MiniTemplateEngine.csproj", "MiniTemplateEngine/"]

# Восстанавливаем зависимости
RUN dotnet restore "./MiniHttpServer/MiniHttpServer.csproj"

# Копируем весь остальной код
COPY . .
WORKDIR "/src/MiniHttpServer"

# Сборка
RUN dotnet build "./MiniHttpServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Публикация
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MiniHttpServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Финальный образ
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MiniHttpServer.dll"]
