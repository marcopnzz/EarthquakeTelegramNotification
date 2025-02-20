# 1. Usa l'immagine base con il .NET SDK (per costruire l'app)
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# 2. Imposta la cartella di lavoro dove il codice sarà copiato
WORKDIR /src

# 3. Copia il file .csproj del tuo progetto nella cartella di lavoro
COPY ["EarthquakeAdvisorTelegramBotBackend/EarthquakeAdvisorTelegramBotBackend.csproj", "EarthquakeAdvisorTelegramBotBackend/"]

# 4. Ripristina le dipendenze del progetto
RUN dotnet restore "EarthquakeAdvisorTelegramBotBackend/EarthquakeAdvisorTelegramBotBackend.csproj"

# 5. Copia tutto il resto del codice
COPY . .

# 6. Pubblica il progetto in modalità Release
RUN dotnet publish "EarthquakeAdvisorTelegramBotBackend/EarthquakeAdvisorTelegramBotBackend.csproj" -c Release -o /app/publish

# 7. Usa un'immagine più leggera per eseguire l'app
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

# 8. Imposta la cartella di lavoro per l'esecuzione
WORKDIR /app

# 9. Copia i file dal passo di costruzione
COPY --from=build /app/publish .

# 10. Espone la porta 80 per l'app (opzionale)
EXPOSE 80

# 11. Esegui l'app (sostituisci con il nome della tua DLL)
ENTRYPOINT ["dotnet", "EarthquakeAdvisorTelegramBotBackend.dll"]