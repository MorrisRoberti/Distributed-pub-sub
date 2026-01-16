# Fase di Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# 1. Copia il file nuget.config e la cartella dei pacchetti locali
# (Assumiamo che il contesto del build sia la root del progetto)
COPY ["nuget.config", "./"]
COPY ["LocalNuGet/", "./LocalNuGet/"]

# 2. Copia i file di progetto e ripristina le dipendenze
COPY ["src/IdentityService/Identity.Api/Identity.Api.csproj", "src/IdentityService/Identity.Api/"]
COPY ["src/IdentityService/Identity.Shared/Identity.Shared.csproj", "src/IdentityService/Identity.Shared/"]
# Se è il RegistryService, aggiungi qui anche i path dei suoi progetti Business e Repository

RUN dotnet restore "src/IdentityService/Identity.Api/Identity.Api.csproj"

# 3. Copia tutto il resto e compila
COPY . .
WORKDIR "/app/src/IdentityService/Identity.Api"
RUN dotnet build "Identity.Api.csproj" -c Release -o /app/build

# Fase di Publish
FROM build AS publish
RUN dotnet publish "Identity.Api.csproj" -c Release -o /app/publish

# Fase Finale (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Identity.Api.dll"]