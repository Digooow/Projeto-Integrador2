
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app          
EXPOSE 80             


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Projeto-Integrador2.csproj", "."]  
RUN dotnet restore "./Projeto-Integrador2.csproj" 
COPY . .             
RUN dotnet build ... 


FROM build AS publish
RUN dotnet publish ... -o /app/publish # Gera os arquivos finais na pasta "publish"


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish . 
ENTRYPOINT ["dotnet", "Projeto-Integrador2.dll"] 