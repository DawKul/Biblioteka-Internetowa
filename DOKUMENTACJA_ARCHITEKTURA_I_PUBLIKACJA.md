# Dokumentacja: Architektura Przechowywania Danych i Plan Publikacji

## 📊 CZĘŚĆ 1: ARCHITEKTURA PRZECHOWYWANIA DANYCH

### 1.1. Typ bazy danych

**Aplikacja wykorzystuje relacyjną bazę danych SQL Server** do przechowywania wszystkich danych.

#### Szczegóły techniczne:
- **System zarządzania bazą danych**: Microsoft SQL Server (lub SQL Server LocalDB w środowisku deweloperskim)
- **ORM (Object-Relational Mapping)**: Entity Framework Core 8.0
- **Dostawca danych**: `Microsoft.EntityFrameworkCore.SqlServer`
- **Migracje**: Automatyczne zarządzanie schematem bazy danych przez Entity Framework Migrations

### 1.2. Struktura bazy danych

#### Tabele w bazie danych:

##### 1. **AspNetUsers** (Tabela użytkowników - rozszerzona przez Identity)
```
- Id (string, PK) - Unikalny identyfikator użytkownika
- UserName (string) - Nazwa użytkownika
- Email (string) - Adres email
- EmailConfirmed (bool) - Potwierdzenie email
- PasswordHash (string) - Zaszyfrowane hasło
- FullName (string, nullable) - Imię i nazwisko użytkownika
- IsDarkMode (bool) - Preferencja trybu ciemnego
- [Pozostałe pola Identity: SecurityStamp, ConcurrencyStamp, LockoutEnabled, etc.]
```

##### 2. **AspNetRoles** (Role użytkowników)
```
- Id (string, PK)
- Name (string) - Nazwa roli (np. "Admin", "Czytelnik")
- NormalizedName (string) - Znormalizowana nazwa roli
```

##### 3. **AspNetUserRoles** (Tabela łącząca użytkowników z rolami)
```
- UserId (string, FK → AspNetUsers)
- RoleId (string, FK → AspNetRoles)
```

##### 4. **Books** (Tabela książek)
```
- Id (int, PK, Identity) - Unikalny identyfikator książki
- Title (string, required) - Tytuł książki
- Author (string, required) - Autor
- ISBN (string, required) - Numer ISBN
- PublicationYear (int) - Rok wydania
- Category (string, required) - Kategoria
- Description (string, nullable) - Opis książki
- CoverImageUrl (string, nullable) - URL do okładki
- TotalCopies (int) - Całkowita liczba egzemplarzy
- AvailableCopies (int) - Dostępne egzemplarze
```

##### 5. **Borrowings** (Tabela wypożyczeń)
```
- Id (int, PK, Identity) - Unikalny identyfikator wypożyczenia
- UserId (string, FK → AspNetUsers) - Użytkownik wypożyczający
- BookId (int, FK → Books) - Wypożyczona książka
- BorrowDate (DateTime) - Data wypożyczenia
- DueDate (DateTime) - Termin zwrotu
- ReturnDate (DateTime?, nullable) - Data zwrotu (null jeśli nie zwrócono)
- IsReturned (bool) - Czy książka została zwrócona
```

##### 6. **Pozostałe tabele Identity** (automatycznie generowane przez ASP.NET Core Identity):
- `AspNetUserClaims` - Oświadczenia użytkowników
- `AspNetUserLogins` - Zewnętrzne logowania
- `AspNetUserTokens` - Tokeny użytkowników
- `AspNetRoleClaims` - Oświadczenia ról

### 1.3. Relacje między tabelami

```
AspNetUsers (1) ────< (wiele) Borrowings
Books (1) ────< (wiele) Borrowings
AspNetUsers (wiele) ────< (wiele) AspNetRoles (przez AspNetUserRoles)
```

**Relacje:**
- **Użytkownik → Wypożyczenia**: Jeden użytkownik może mieć wiele wypożyczeń (1:N)
- **Książka → Wypożyczenia**: Jedna książka może być wypożyczona wiele razy (1:N)
- **Użytkownik → Role**: Jeden użytkownik może mieć wiele ról, jedna rola może być przypisana wielu użytkownikom (N:M)

### 1.4. Entity Framework Core - Konfiguracja

#### ApplicationDbContext
```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Book> Books { get; set; }
    public DbSet<Borrowing> Borrowings { get; set; }
}
```

**Dziedziczenie po `IdentityDbContext<ApplicationUser>`** zapewnia:
- Automatyczne zarządzanie tabelami użytkowników i autoryzacji
- Integrację z ASP.NET Core Identity
- Gotowe tabele dla logowania, ról, tokenów

#### Migracje bazy danych

**Automatyczne stosowanie migracji:**
- Migracje są automatycznie stosowane przy starcie aplikacji (w `Program.cs`)
- Wszystkie migracje znajdują się w folderze `Data/Migrations/`
- Historia migracji:
  1. `00000000000000_CreateIdentitySchema` - Utworzenie schematu Identity
  2. `20251118172523_BibliotekaFinal` - Dodanie tabel Books i Borrowings, pole FullName
  3. `20250120000000_AddDarkModeToUser` - Dodanie pola IsDarkMode

### 1.5. Zarządzanie danymi

#### Operacje CRUD:
- **Create**: Dodawanie nowych rekordów przez `context.Set<T>().Add(entity)`
- **Read**: Odczytywanie danych przez LINQ queries (`context.Books.Where(...)`)
- **Update**: Aktualizacja przez `context.Set<T>().Update(entity)`
- **Delete**: Usuwanie przez `context.Set<T>().Remove(entity)`

#### Zapytania LINQ:
```csharp
// Przykład: Pobranie wszystkich dostępnych książek
var availableBooks = await _context.Books
    .Where(b => b.AvailableCopies > 0)
    .ToListAsync();
```

#### Wykorzystanie Include dla relacji:
```csharp
// Przykład: Pobranie wypożyczeń z danymi użytkownika i książki
var borrowings = await _context.Borrowings
    .Include(b => b.User)
    .Include(b => b.Book)
    .ToListAsync();
```

### 1.6. Bezpieczeństwo danych

- **Hasła**: Przechowywane jako hash (bcrypt) przez ASP.NET Core Identity
- **Connection String**: Przechowywany w `appsettings.json` (nie commitujemy `appsettings.Production.json` z prawdziwymi danymi)
- **SQL Injection**: Zabezpieczone przez parametryzowane zapytania Entity Framework
- **Autoryzacja**: Kontrola dostępu przez role (Admin, Czytelnik)

---

## 🚀 CZĘŚĆ 2: PLAN PUBLIKACJI NA ZEWNĘTRZNYM HOSTINGU

### 2.1. Wymagania przed publikacją

#### Wymagane komponenty:
1. **.NET 8.0 Runtime** - Musi być zainstalowany na serwerze
2. **SQL Server** - Baza danych (może być Azure SQL Database, SQL Server na VPS, lub inna opcja)
3. **HTTPS** - Certyfikat SSL (wymagany dla produkcji)
4. **Domena** (opcjonalnie) - Dla profesjonalnego wyglądu

#### Pliki do przygotowania:
- ✅ Skompilowana aplikacja (Release build)
- ✅ Connection string do bazy danych produkcyjnej
- ✅ Konfiguracja `appsettings.Production.json`
- ✅ Migracje bazy danych

### 2.2. Opcje hostingu

#### OPCJA A: Azure App Service (Rekomendowane) ⭐

**Zalety:**
- Pełna integracja z ekosystemem Microsoft
- Automatyczne skalowanie
- Wbudowane SSL
- Łatwa integracja z Azure SQL Database
- Automatyczne wdrożenia z GitHub

**Kroki publikacji:**

1. **Przygotowanie w Azure Portal:**
   ```
   a. Zaloguj się na https://portal.azure.com
   b. Utwórz nową "App Service" (Web App)
   c. Wybierz:
      - Runtime stack: .NET 8.0
      - Operating System: Windows (lub Linux)
      - Region: West Europe (lub inna bliska)
   ```

2. **Utworzenie Azure SQL Database:**
   ```
   a. W Azure Portal → Utwórz "SQL Database"
   b. Wybierz serwer SQL (lub utwórz nowy)
   c. Wybierz warstwę cenową (np. Basic - najtańsza)
   d. Skopiuj connection string
   ```

3. **Konfiguracja Connection String w App Service:**
   ```
   a. W App Service → Configuration → Connection strings
   b. Dodaj nowy connection string:
      - Name: DefaultConnection
      - Value: [Twój connection string z Azure SQL]
      - Type: SQLAzure
   ```

4. **Publikacja aplikacji:**
   
   **Metoda 1: Visual Studio (Najłatwiejsza)**
   ```
   a. Kliknij prawym na projekt → Publish
   b. Wybierz "Azure" → "Azure App Service"
   c. Zaloguj się i wybierz utworzoną App Service
   d. Kliknij "Publish"
   ```

   **Metoda 2: Azure CLI**
   ```bash
   # Zainstaluj Azure CLI: https://docs.microsoft.com/cli/azure/install-azure-cli
   az login
   az webapp deployment source config-zip --resource-group [RESOURCE_GROUP] --name [APP_NAME] --src [ZIP_FILE]
   ```

   **Metoda 3: GitHub Actions (Automatyczne)**
   ```yaml
   # Utwórz plik .github/workflows/deploy.yml
   # Skonfiguruj automatyczne wdrożenia z GitHub
   ```

5. **Zastosowanie migracji:**
   ```
   Migracje są automatycznie stosowane przy starcie aplikacji (dzięki kodowi w Program.cs).
   Alternatywnie, możesz użyć Azure Cloud Shell:
   ```
   ```bash
   dotnet ef database update --connection "[CONNECTION_STRING]"
   ```

**Koszty:**
- App Service (Basic B1): ~$13/miesiąc
- Azure SQL Database (Basic): ~$5/miesiąc
- **Łącznie: ~$18/miesiąc**

---

#### OPCJA B: Render.com (Proste i tanie) ⭐⭐

**Zalety:**
- Darmowy plan dla małych projektów
- Automatyczne wdrożenia z GitHub
- Wbudowane SSL
- Łatwa konfiguracja

**Kroki publikacji:**

1. **Przygotowanie repozytorium GitHub:**
   ```
   a. Upewnij się, że kod jest na GitHub
   b. Sprawdź, czy appsettings.Production.json jest w .gitignore
   ```

2. **Utworzenie bazy danych na Render:**
   ```
   a. Zaloguj się na https://render.com
   b. Kliknij "New" → "PostgreSQL" (lub użyj zewnętrznego SQL Server)
   c. Skopiuj connection string
   ```

3. **Utworzenie Web Service:**
   ```
   a. Kliknij "New" → "Web Service"
   b. Połącz z repozytorium GitHub
   c. Konfiguracja:
      - Name: biblioteka-internetowa
      - Environment: .NET
      - Build Command: dotnet publish -c Release -o ./publish
      - Start Command: dotnet BibliotekaInternetowa.dll
   ```

4. **Konfiguracja zmiennych środowiskowych:**
   ```
   W sekcji "Environment Variables":
   - ASPNETCORE_ENVIRONMENT: Production
   - ConnectionStrings__DefaultConnection: [Twój connection string]
   ```

5. **Deploy:**
   ```
   Render automatycznie zbuduje i wdroży aplikację.
   Migracje zostaną zastosowane automatycznie przy pierwszym uruchomieniu.
   ```

**Koszty:**
- Web Service (Free tier): $0/miesiąc (z limitami)
- PostgreSQL (Free tier): $0/miesiąc (z limitami)
- **Łącznie: $0/miesiąc (dla małych projektów)**

---

#### OPCJA C: Railway.app (Proste, dobre dla .NET)

**Zalety:**
- Łatwa konfiguracja
- Automatyczne wdrożenia
- Darmowy plan z $5 kredytów miesięcznie

**Kroki:**

1. **Utworzenie projektu:**
   ```
   a. Zaloguj się na https://railway.app
   b. "New Project" → "Deploy from GitHub repo"
   c. Wybierz repozytorium
   ```

2. **Konfiguracja:**
   ```
   Railway automatycznie wykryje .NET projekt.
   Dodaj zmienne środowiskowe:
   - ASPNETCORE_ENVIRONMENT=Production
   - ConnectionStrings__DefaultConnection=[connection string]
   ```

3. **Baza danych:**
   ```
   Railway oferuje PostgreSQL, ale możesz użyć zewnętrznego SQL Server.
   ```

---

#### OPCJA D: VPS (Virtual Private Server) - Najbardziej elastyczne

**Zalety:**
- Pełna kontrola
- Możliwość użycia własnego SQL Server
- Najlepsze dla większych projektów

**Kroki:**

1. **Wybór dostawcy VPS:**
   - DigitalOcean, Vultr, Hetzner, OVH
   - Minimalne wymagania: 2GB RAM, 1 CPU, 20GB storage

2. **Instalacja na serwerze:**
   ```bash
   # Połącz się z serwerem przez SSH
   ssh user@your-server-ip
   
   # Zainstaluj .NET 8.0 Runtime
   wget https://dot.net/v1/dotnet-install.sh
   chmod +x dotnet-install.sh
   ./dotnet-install.sh --channel 8.0 --runtime aspnetcore
   
   # Zainstaluj SQL Server (lub użyj zewnętrznego)
   # Zainstaluj Nginx jako reverse proxy
   ```

3. **Publikacja aplikacji:**
   ```bash
   # Na lokalnym komputerze:
   dotnet publish -c Release -o ./publish
   
   # Prześlij pliki na serwer (SCP, FTP, lub Git)
   scp -r ./publish/* user@server:/var/www/biblioteka/
   ```

4. **Konfiguracja Nginx:**
   ```nginx
   server {
       listen 80;
       server_name twoja-domena.pl;
       
       location / {
           proxy_pass http://localhost:5000;
           proxy_http_version 1.1;
           proxy_set_header Upgrade $http_upgrade;
           proxy_set_header Connection keep-alive;
           proxy_set_header Host $host;
       }
   }
   ```

5. **Uruchomienie jako usługa:**
   ```bash
   # Utwórz plik /etc/systemd/system/biblioteka.service
   # Uruchom: systemctl start biblioteka
   ```

---

### 2.3. Konfiguracja appsettings.Production.json

**WAŻNE:** Ten plik NIE powinien być commitowany do Git z prawdziwymi danymi!

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TWÓJ_SERWER;Database=BibliotekaInternetowa;User Id=TWÓJ_USER;Password=HASŁO;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**Dla Azure SQL Database:**
```
Server=tcp:twoj-serwer.database.windows.net,1433;Initial Catalog=BibliotekaInternetowa;Persist Security Info=False;User ID=twoj-user;Password=twoje-haslo;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

### 2.4. Checklist przed publikacją

- [ ] Kod jest w repozytorium Git (GitHub/GitLab)
- [ ] `appsettings.Production.json` jest w `.gitignore`
- [ ] Aplikacja kompiluje się bez błędów (`dotnet build -c Release`)
- [ ] Wszystkie migracje są utworzone
- [ ] Connection string do bazy produkcyjnej jest przygotowany
- [ ] Testy lokalne przeszły pomyślnie
- [ ] HTTPS jest skonfigurowany
- [ ] Backup bazy danych jest skonfigurowany (dla produkcji)

### 2.5. Po publikacji - Weryfikacja

1. **Sprawdź działanie aplikacji:**
   - Otwórz URL aplikacji w przeglądarce
   - Sprawdź, czy strona główna się ładuje
   - Zaloguj się jako administrator

2. **Sprawdź bazę danych:**
   - Sprawdź, czy migracje zostały zastosowane
   - Sprawdź, czy dane seed są obecne

3. **Sprawdź logi:**
   - W Azure: App Service → Log stream
   - W Render: Dashboard → Logs
   - W VPS: `journalctl -u biblioteka -f`

4. **Test funkcjonalności:**
   - Rejestracja nowego użytkownika
   - Logowanie
   - Przeglądanie książek
   - Wypożyczanie książek
   - Panel administratora

### 2.6. Backup i bezpieczeństwo

#### Backup bazy danych:
- **Azure**: Automatyczne backupy (włącz w Azure Portal)
- **VPS**: Skonfiguruj cron job do regularnych backupów
- **Render/Railway**: Użyj zewnętrznego narzędzia do backupów

#### Bezpieczeństwo:
- ✅ Używaj silnych haseł dla bazy danych
- ✅ Włącz HTTPS (wymuszony w produkcji)
- ✅ Regularnie aktualizuj zależności NuGet
- ✅ Monitoruj logi pod kątem podejrzanych aktywności
- ✅ Używaj zmiennych środowiskowych dla wrażliwych danych

---

## 📝 PODSUMOWANIE

### Architektura danych:
- ✅ **Relacyjna baza danych SQL Server**
- ✅ **Entity Framework Core 8.0** jako ORM
- ✅ **Automatyczne migracje** przy starcie aplikacji
- ✅ **ASP.NET Core Identity** dla zarządzania użytkownikami
- ✅ **Relacje 1:N i N:M** między encjami

### Publikacja:
- **Rekomendowane**: Azure App Service (najlepsza integracja z .NET)
- **Dla małych projektów**: Render.com (darmowy plan)
- **Dla większej kontroli**: VPS z własnym serwerem

### Następne kroki:
1. Wybierz platformę hostingu
2. Przygotuj bazę danych produkcyjną
3. Skonfiguruj connection string
4. Opublikuj aplikację
5. Zweryfikuj działanie

---

**Data utworzenia dokumentacji:** 2025-01-20  
**Wersja aplikacji:** 1.0  
**Technologie:** ASP.NET Core MVC 8.0, Entity Framework Core 8.0, SQL Server