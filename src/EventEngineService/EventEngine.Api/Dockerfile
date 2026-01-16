FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["nuget.config", "./"]
COPY ["LocalNuGet/", "./LocalNuGet/"]

COPY ["src/EventEngineService/EventEngine.Api/EventEngine.Api.csproj", "src/EventEngineService/EventEngine.Api/"]
# Se ha dipendenze NuGet locali, il restore le troverà grazie ai file sopra
RUN dotnet restore "src/EventEngineService/EventEngine.Api/EventEngine.Api.csproj"

COPY . .
WORKDIR "/src/src/EventEngineService/EventEngine.Api"
RUN dotnet build "EventEngine.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventEngine.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventEngine.Api.dll"]