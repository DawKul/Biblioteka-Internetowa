# Instrukcje publikacji projektu

## 1. Publikacja na GitHub

### Krok 1: Zainstaluj Git (jeśli nie masz)
Pobierz i zainstaluj Git z: https://git-scm.com/download/win

### Krok 2: Zainicjalizuj repozytorium Git

Otwórz terminal w folderze projektu i wykonaj:

```bash
# Przejdź do głównego folderu projektu (gdzie jest BibliotekaInternetowa.csproj)
cd C:\Users\dawid\source\repos\BibliotekaInternetowa\BibliotekaInternetowa

# Zainicjalizuj repozytorium Git
git init

# Dodaj zdalne repozytorium
git remote add origin https://github.com/DawKul/Biblioteka-Internetowa.git

# Dodaj wszystkie pliki
git add .

# Utwórz pierwszy commit
git commit -m "Initial commit - Biblioteka Internetowa"

# Prześlij na GitHub (jeśli repozytorium jest puste)
git branch -M main
git push -u origin main
```

### Krok 3: Jeśli repozytorium już ma pliki

Jeśli repozytorium na GitHub już ma pliki (np. README), wykonaj:

```bash
# Pobierz zmiany z GitHub
git pull origin main --allow-unrelated-histories

# Rozwiąż konflikty jeśli występują, następnie:
git add .
git commit -m "Merge with remote repository"
git push -u origin main
```

## 2. Publikacja aplikacji na hostingu

### Opcja A: Azure App Service (Rekomendowane)

1. **Utwórz aplikację w Azure Portal:**
   - Zaloguj się na https://portal.azure.com
   - Utwórz nową "App Service"
   - Wybierz .NET 8.0 jako runtime stack

2. **Skonfiguruj bazę danych:**
   - Utwórz SQL Database w Azure
   - Skopiuj connection string

3. **Skonfiguruj Connection String:**
   - W Azure Portal → Twoja aplikacja → Configuration
   - Dodaj Connection String:
     - Name: `DefaultConnection`
     - Value: Twój connection string z Azure SQL

4. **Opublikuj aplikację:**
   ```bash
   # Zainstaluj Azure CLI jeśli nie masz
   # https://docs.microsoft.com/cli/azure/install-azure-cli

   # Zaloguj się
   az login

   # Opublikuj
   dotnet publish -c Release
   cd bin/Release/net8.0/publish
   zip -r deploy.zip .

   # Lub użyj Visual Studio: Publish → Azure App Service
   ```

### Opcja B: Inny hosting (np. VPS, Shared Hosting)

1. **Skonfiguruj appsettings.Production.json:**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=TWÓJ_SERWER;Database=BibliotekaInternetowa;User Id=TWÓJ_USER;Password=HASŁO;TrustServerCertificate=True;"
     }
   }
   ```

2. **Opublikuj aplikację:**
   ```bash
   dotnet publish -c Release -o ./publish
   ```

3. **Prześlij pliki na serwer:**
   - Skopiuj zawartość folderu `publish` na serwer
   - Upewnij się, że .NET 8.0 Runtime jest zainstalowany na serwerze

4. **Skonfiguruj serwer:**
   - Ustaw zmienną środowiskową: `ASPNETCORE_ENVIRONMENT=Production`
   - Skonfiguruj reverse proxy (IIS, Nginx, Apache)
   - Uruchom aplikację

### Opcja C: Docker (Opcjonalne)

Utwórz plik `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BibliotekaInternetowa.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BibliotekaInternetowa.dll"]
```

Następnie:
```bash
docker build -t biblioteka-internetowa .
docker run -p 8080:80 biblioteka-internetowa
```

## 3. Migracje bazy danych na produkcji

Po opublikowaniu aplikacji, wykonaj migracje:

```bash
# Na serwerze lub przez Azure Cloud Shell
dotnet ef database update --connection "TWÓJ_CONNECTION_STRING"
```

Lub skonfiguruj automatyczne migracje w `Program.cs` (już jest skonfigurowane).

## 4. Bezpieczeństwo

⚠️ **WAŻNE:**
- Nigdy nie commituj `appsettings.Production.json` z prawdziwymi hasłami
- Używaj User Secrets dla developmentu
- Używaj Azure Key Vault lub zmiennych środowiskowych dla produkcji
- Włącz HTTPS na produkcji

## 5. Aktualizacje

Aby zaktualizować aplikację na GitHub:

```bash
git add .
git commit -m "Opis zmian"
git push origin main
```

Aby zaktualizować na hostingu, opublikuj ponownie aplikację.

