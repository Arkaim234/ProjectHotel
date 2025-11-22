# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем весь solution
COPY . .

# Восстанавливаем зависимости
RUN dotnet restore "MiniHttpServer/MiniHttpServer.csproj"

# Сборка
RUN dotnet build "MiniHttpServer/MiniHttpServer.csproj" -c Release -o /app/build

# Публикация
FROM build AS publish
RUN dotnet publish "MiniHttpServer/MiniHttpServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Финальный образ (рантайм)
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MiniHttpServer.dll"]
