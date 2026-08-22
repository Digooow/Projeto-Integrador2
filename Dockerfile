
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY ["Projeto-integrador2.csproj", "."]
RUN dotnet restore "Projeto-integrador2.csproj"


COPY . .
RUN dotnet build "Projeto-integrador2.csproj" -c Release -o /app/build


FROM build AS publish
RUN dotnet publish "Projeto-integrador2.csproj" -c Release -o /app/publish


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Projeto-integrador2.dll"]