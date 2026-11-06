FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Projeto-Integrador2.csproj", "./"]
RUN dotnet restore "Projeto-Integrador2.csproj"

COPY . .
RUN dotnet publish "Projeto-Integrador2.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build --chown=$APP_UID:$APP_UID /app/publish .
ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false \
    DOTNET_USE_POLLING_FILE_WATCHER=true
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Projeto-Integrador2.dll"]