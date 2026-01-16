FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["nuget.config", "./"]
COPY ["LocalNuGet/", "./LocalNuGet/"]

# Copia tutti i progetti della soluzione Registry
COPY ["src/RegistryService/Registry.Api/Registry.Api.csproj", "src/RegistryService/Registry.Api/"]
COPY ["src/RegistryService/Registry.Business/Registry.Business.csproj", "src/RegistryService/Registry.Business/"]
COPY ["src/RegistryService/Registry.Repository/Registry.Repository.csproj", "src/RegistryService/Registry.Repository/"]

RUN dotnet restore "src/RegistryService/Registry.Api/Registry.Api.csproj"

COPY . .
WORKDIR "/src/src/RegistryService/Registry.Api"
RUN dotnet build "Registry.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Registry.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Registry.Api.dll"]