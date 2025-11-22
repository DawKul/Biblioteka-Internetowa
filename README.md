<<<<<<< HEAD
# Biblioteka Internetowa

Aplikacja webowa do zarządzania biblioteką internetową z systemem wypożyczeń książek.

## Funkcjonalności

- 📚 **Katalog książek** - przeglądanie, wyszukiwanie i filtrowanie książek
- 📖 **System wypożyczeń** - wypożyczanie i zwracanie książek
- 👥 **Zarządzanie użytkownikami** - rejestracja, logowanie, role (Admin, Czytelnik)
- ⚙️ **Panel administratora**:
  - Dashboard ze statystykami
  - Zarządzanie użytkownikami
  - Przegląd wszystkich wypożyczeń
  - Statystyki z wykresami
  - Generowanie raportów PDF
- 📄 **Raporty PDF** - generowanie raportów wypożyczeń i statystyk

## Technologie

- **.NET 8.0** - ASP.NET Core MVC
- **Entity Framework Core** - ORM
- **SQL Server** - baza danych
- **ASP.NET Core Identity** - autoryzacja i uwierzytelnianie
- **QuestPDF** - generowanie raportów PDF
- **Chart.js** - wykresy statystyczne
- **Bootstrap 5** - responsywny interfejs

## Wymagania

- .NET 8.0 SDK
- SQL Server (LocalDB lub pełna wersja)
- Visual Studio 2022 lub VS Code

## Instalacja

1. Sklonuj repozytorium:
```bash
git clone https://github.com/DawKul/Biblioteka-Internetowa.git
cd Biblioteka-Internetowa/BibliotekaInternetowa
```

2. Przywróć pakiety NuGet:
```bash
dotnet restore
```

3. Zastosuj migracje bazy danych:
```bash
dotnet ef database update
```

4. Uruchom aplikację:
```bash
dotnet run
```

Aplikacja będzie dostępna pod adresem: `https://localhost:5001` lub `http://localhost:5000`

## Domyślne konto administratora

- **Email**: admin@biblioteka.pl
- **Hasło**: Admin123!

## Publikacja

### Publikacja na Azure App Service

1. Utwórz aplikację w Azure Portal
2. Skonfiguruj connection string w ustawieniach aplikacji
3. Opublikuj aplikację:
```bash
dotnet publish -c Release
```

### Publikacja na inny hosting

1. Skonfiguruj `appsettings.Production.json` z właściwym connection string
2. Opublikuj aplikację:
```bash
dotnet publish -c Release -o ./publish
```

## Struktura projektu

```
BibliotekaInternetowa/
├── Controllers/          # Kontrolery MVC
├── Models/              # Modele danych i ViewModele
├── Views/               # Widoki Razor
├── Services/           # Serwisy (np. PDF)
├── Data/               # Kontekst bazy danych
└── wwwroot/            # Pliki statyczne (CSS, JS)
```

## Licencja

Projekt edukacyjny.

=======
# Biblioteka-Internetowa
Biblioteka Internetowa Projekt
>>>>>>> 2217d8cc6ab3f4d6140813ca01453e98224cf92a
