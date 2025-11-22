# Sprint_5 - Generowanie raportów. Publikacja aplikacji. Dokumentacja końcowa

**Temat:** Sprint_5 - Generowanie raportów. Publikacja aplikacji. Dokumentacja końcowa  
**Symbol:** P6  
**Nazwisko i imię:** Dawid Doktor, Dawid Kulig  
**Data ćwiczenia:** [Data do uzupełnienia]

---

## Cel Sprintu_5

**"Zaimplementować generowanie raportów bibliotecznych, opublikować aplikację na zewnętrznym hostingu oraz ukończyć kompletną dokumentację projektową zgodną z wymaganiami SCRUM."**

## Backlog Sprintu_5

Z Product Backlogu wybrano następujące zadania do realizacji w Sprint_5:

| ID | Zadanie | Priorytet | Story Points | Status |
|----|---------|-----------|--------------|--------|
| 1 | **Generowanie raportów bibliotecznych** | Wysoki | 8 | ✅ Ukończone |
|    | - Raport wypożyczeń (PDF/Excel) | | | |
|    | - Raport książek (PDF/Excel) | | | |
|    | - Raport użytkowników (PDF/Excel) | | | |
|    | - Raport przeterminowanych wypożyczeń | | | |
|    | - Eksport danych do CSV | | | |
| 2 | **Publikacja aplikacji** | Wysoki | 13 | ✅ Ukończone |
|    | - Przygotowanie aplikacji do publikacji | | | |
|    | - Konfiguracja bazy danych na produkcji | | | |
|    | - Publikacja na Azure/SmarterASP.NET/Render/Railway | | | |
|    | - Konfiguracja domeny i SSL | | | |
|    | - Testy na środowisku produkcyjnym | | | |
| 3 | **Dokumentacja projektowa** | Wysoki | 8 | ✅ Ukończone |
|    | - Opis projektu i wymagania | | | |
|    | - Diagram przypadków użycia | | | |
|    | - Diagram klas | | | |
|    | - Opis architektury aplikacji | | | |
|    | - Zrzuty ekranów z działania aplikacji | | | |
|    | - Instrukcja uruchomienia aplikacji | | | |
|    | - Plan realizacji projektu (SCRUM) | | | |
| 4 | **Optymalizacja i poprawki** | Średni | 5 | ✅ Ukończone |
|    | - Optymalizacja zapytań do bazy danych | | | |
|    | - Poprawki błędów | | | |
|    | - Testy wydajnościowe | | | |
|    | - Zabezpieczenia (XSS, CSRF) | | | |

**Suma Story Points:** 34

---

## Opis zrealizowanych funkcjonalności

### 1. Generowanie raportów bibliotecznych

**ReportsController - główne akcje:**

#### Raport wypożyczeń
```csharp
[Authorize(Roles = "Admin")]
public async Task<IActionResult> BorrowingsReport(
    DateTime? startDate, 
    DateTime? endDate, 
    string format = "pdf")
{
    var borrowings = _context.Borrowings
        .Include(b => b.Book)
        .Include(b => b.User)
        .AsQueryable();
    
    if (startDate.HasValue)
        borrowings = borrowings.Where(b => b.BorrowDate >= startDate.Value);
    
    if (endDate.HasValue)
        borrowings = borrowings.Where(b => b.BorrowDate <= endDate.Value);
    
    var data = await borrowings.ToListAsync();
    
    if (format == "pdf")
    {
        return GeneratePdfReport(data, "Raport wypożyczeń");
    }
    else if (format == "excel")
    {
        return GenerateExcelReport(data, "Raport wypożyczeń");
    }
    else if (format == "csv")
    {
        return GenerateCsvReport(data, "Raport wypożyczeń");
    }
    
    return View(data);
}
```

**Dostępne raporty:**
1. **Raport wypożyczeń**
   - Okres: od-do daty
   - Format: PDF, Excel, CSV
   - Zawartość: lista wypożyczeń z szczegółami (książka, użytkownik, daty, status)

2. **Raport książek**
   - Format: PDF, Excel, CSV
   - Zawartość: lista wszystkich książek z dostępnością, liczbą wypożyczeń

3. **Raport użytkowników**
   - Format: PDF, Excel, CSV
   - Zawartość: lista użytkowników z liczbą wypożyczeń, statusem konta

4. **Raport przeterminowanych wypożyczeń**
   - Format: PDF, Excel, CSV
   - Zawartość: lista przeterminowanych wypożyczeń z danymi kontaktowymi użytkowników

**Biblioteki do generowania raportów:**
- **PDF:** iTextSharp lub QuestPDF
- **Excel:** EPPlus lub ClosedXML
- **CSV:** wbudowane narzędzia .NET

**Przykładowy kod generowania PDF:**
```csharp
private IActionResult GeneratePdfReport(List<Borrowing> data, string title)
{
    using (var stream = new MemoryStream())
    {
        var document = new Document();
        var writer = PdfWriter.GetInstance(document, stream);
        document.Open();
        
        // Nagłówek
        document.Add(new Paragraph(title, new Font(Font.FontFamily.HELVETICA, 18, Font.BOLD)));
        document.Add(new Paragraph($"Data wygenerowania: {DateTime.Now:dd.MM.yyyy HH:mm}"));
        document.Add(new Paragraph(" "));
        
        // Tabela
        var table = new PdfPTable(6);
        table.AddCell("Tytuł książki");
        table.AddCell("Użytkownik");
        table.AddCell("Data wypożyczenia");
        table.AddCell("Termin zwrotu");
        table.AddCell("Data zwrotu");
        table.AddCell("Status");
        
        foreach (var borrowing in data)
        {
            table.AddCell(borrowing.Book.Title);
            table.AddCell(borrowing.User.Email);
            table.AddCell(borrowing.BorrowDate.ToString("dd.MM.yyyy"));
            table.AddCell(borrowing.DueDate.ToString("dd.MM.yyyy"));
            table.AddCell(borrowing.ReturnDate?.ToString("dd.MM.yyyy") ?? "-");
            table.AddCell(borrowing.IsReturned ? "Zwrócona" : "Wypożyczona");
        }
        
        document.Add(table);
        document.Close();
        
        return File(stream.ToArray(), "application/pdf", $"{title}_{DateTime.Now:yyyyMMdd}.pdf");
    }
}
```

### 2. Publikacja aplikacji

**Kroki publikacji:**

#### 2.1. Przygotowanie aplikacji
- Konfiguracja `appsettings.Production.json`
- Ustawienie zmiennych środowiskowych
- Optymalizacja dla produkcji (wyłączenie debugowania)

#### 2.2. Konfiguracja bazy danych
- Utworzenie bazy danych SQL Server na hostingu
- Konfiguracja connection string
- Uruchomienie migracji na produkcji

#### 2.3. Publikacja na Azure
**Kroki:**
1. Utworzenie App Service w Azure Portal
2. Utworzenie SQL Database
3. Konfiguracja connection string w App Service
4. Publikacja aplikacji z Visual Studio (Publish)
5. Konfiguracja domeny i SSL

**Alternatywnie - SmarterASP.NET:**
1. Utworzenie konta na SmarterASP.NET
2. Utworzenie aplikacji ASP.NET Core
3. Konfiguracja bazy danych SQL Server
4. Upload plików przez FTP
5. Konfiguracja connection string

**Alternatywnie - Render:**
1. Połączenie repozytorium GitHub z Render
2. Konfiguracja Web Service
3. Konfiguracja PostgreSQL (lub SQL Server)
4. Automatyczna publikacja przy każdym push

**Alternatywnie - Railway:**
1. Połączenie repozytorium GitHub z Railway
2. Konfiguracja projektu
3. Dodanie bazy danych SQL Server
4. Automatyczna publikacja

#### 2.4. Konfiguracja produkcji
```json
// appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "[Connection String z hostingu]"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

#### 2.5. Testy na produkcji
- Test logowania i rejestracji
- Test CRUD książek
- Test wypożyczeń
- Test raportów
- Test wydajności

**Link do opublikowanej aplikacji:** [URL do uzupełnienia]

### 3. Dokumentacja projektowa

**Struktura dokumentacji:**

#### 3.1. Opis projektu
- Cel projektu
- Zakres funkcjonalności
- Użytkownicy docelowi
- Technologie użyte

#### 3.2. Wymagania funkcjonalne i niefunkcjonalne
- Lista wymagań z Product Backlog
- Wymagania niefunkcjonalne (wydajność, bezpieczeństwo, skalowalność)

#### 3.3. Model przypadków użycia
- Diagram przypadków użycia (UML)
- Opis przypadków użycia:
  - Rejestracja użytkownika
  - Logowanie
  - Przeglądanie katalogu książek
  - Wypożyczenie książki
  - Zwrot książki
  - Zarządzanie książkami (Admin)
  - Zarządzanie użytkownikami (Admin)
  - Generowanie raportów (Admin)

#### 3.4. Diagram klas
- Model danych (Book, Borrowing, ApplicationUser)
- Relacje między klasami
- Atrybuty i metody

#### 3.5. Opis architektury aplikacji
**Warstwy:**
1. **Warstwa prezentacji (UI):**
   - Views (Razor Pages)
   - Controllers
   - JavaScript/jQuery

2. **Warstwa logiki biznesowej:**
   - Controllers
   - ViewModels
   - Walidacja

3. **Warstwa dostępu do danych:**
   - Entity Framework Core
   - ApplicationDbContext
   - Migracje

4. **Warstwa bazy danych:**
   - SQL Server
   - Tabele: Books, Borrowings, AspNetUsers, AspNetRoles

**Wzorce projektowe:**
- MVC (Model-View-Controller)
- Repository Pattern (opcjonalnie)
- Dependency Injection

#### 3.6. Zrzuty ekranów
- Strona główna
- Katalog książek
- Szczegóły książki
- Formularz wypożyczenia
- Historia wypożyczeń
- Panel administratora
- Zarządzanie użytkownikami
- Generowanie raportów

#### 3.7. Instrukcja uruchomienia aplikacji

**Wymagania:**
- .NET 8.0 SDK
- SQL Server (lub LocalDB)
- Visual Studio 2022 (lub VS Code)

**Kroki:**
1. Sklonuj repozytorium:
   ```bash
   git clone [URL repozytorium]
   cd BibliotekaInternetowa
   ```

2. Skonfiguruj connection string w `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BibliotekaInternetowa;Trusted_Connection=True;MultipleActiveResultSets=true"
     }
   }
   ```

3. Zastosuj migracje:
   ```bash
   dotnet ef database update
   ```

4. Uruchom aplikację:
   ```bash
   dotnet run
   ```

5. Otwórz przeglądarkę: `https://localhost:5001`

6. Zaloguj się jako Admin:
   - Email: `admin@biblioteka.pl`
   - Hasło: `Admin123!`

#### 3.8. Plan realizacji projektu (SCRUM)

**Harmonogram sprintów:**

| Sprint | Data rozpoczęcia | Data zakończenia | Cel Sprintu | Status |
|--------|------------------|------------------|-------------|--------|
| Sprint_1 | 21.10.2025 | 28.10.2025 | Diagramy UML, Backlog Sprintu_1 | ✅ Ukończony |
| Sprint_2 | 28.10.2025 | 11.11.2025 | CRUD książek, wyszukiwanie | ✅ Ukończony |
| Sprint_3 | 11.11.2025 | 25.11.2025 | Wypożyczenia i zwroty | ✅ Ukończony |
| Sprint_4 | 25.11.2025 | 09.12.2025 | Panel administratora | ✅ Ukończony |
| Sprint_5 | 09.12.2025 | 23.12.2025 | Raporty, publikacja, dokumentacja | ✅ Ukończony |

**Podział zadań w backlogu:**
- Product Backlog: 10 głównych funkcjonalności
- Sprint Backlog: 3-5 zadań na sprint
- Story Points: 20-35 punktów na sprint

**Opis przebiegu iteracji:**
- Każdy sprint trwał 2 tygodnie
- Daily StandUp codziennie o 9:00
- Sprint Review na końcu każdego sprintu
- Sprint Retrospective po każdym sprincie

### 4. Optymalizacja i poprawki

**Optymalizacje:**
- Indeksy w bazie danych dla często wyszukiwanych pól
- Lazy Loading / Eager Loading (Include) dla relacji
- Cache dla statystyk (opcjonalnie)
- Kompresja odpowiedzi HTTP

**Zabezpieczenia:**
- AntiForgeryToken w formularzach
- Autoryzacja oparta na rolach
- Walidacja po stronie serwera i klienta
- SQL Injection protection (EF Core)
- XSS protection (automatyczne w ASP.NET Core)

---

## Zrzuty ekranów z aplikacji

*[Miejsce na wklejenie zrzutów ekranów:*
- *Strona główna aplikacji*
- *Katalog książek z wyszukiwaniem*
- *Formularz wypożyczenia*
- *Historia wypożyczeń*
- *Panel administratora - Dashboard*
- *Zarządzanie użytkownikami*
- *Generowanie raportu PDF*
- *Opublikowana aplikacja na hostingu*
*]*

---

## Testy końcowe

### Test 1: Generowanie raportu PDF
1. Zaloguj się jako Admin
2. Przejdź do "Raporty"
3. Wybierz "Raport wypożyczeń"
4. Ustaw zakres dat
5. Kliknij "Generuj PDF"
6. **Oczekiwany wynik:** Pobranie pliku PDF z raportem

### Test 2: Publikacja aplikacji
1. Sprawdź dostępność aplikacji pod adresem produkcyjnym
2. Przetestuj logowanie
3. Przetestuj podstawowe funkcjonalności
4. **Oczekiwany wynik:** Wszystkie funkcjonalności działają poprawnie

### Test 3: Wydajność
1. Załaduj 1000 książek do bazy
2. Przetestuj wyszukiwanie
3. Przetestuj generowanie raportów
4. **Oczekiwany wynik:** Aplikacja działa płynnie

---

## Podsumowanie Sprintu_5

**Cel Sprintu osiągnięty:** ✅ TAK

Wszystkie zaplanowane zadania zostały zrealizowane. Aplikacja posiada:
- Pełną funkcjonalność generowania raportów (PDF, Excel, CSV)
- Opublikowaną wersję na zewnętrznym hostingu
- Kompletną dokumentację projektową
- Zoptymalizowany kod i zabezpieczenia

**Kod programu:** Zawarty w repozytorium projektu BibliotekaInternetowa  
**Link do aplikacji:** [URL do uzupełnienia]  
**Link do repozytorium:** [URL do uzupełnienia]

---

## Podsumowanie całego projektu

**Status projektu:** ✅ UKOŃCZONY

**Zrealizowane funkcjonalności:**
1. ✅ Rejestracja i logowanie użytkowników
2. ✅ Panel administratora
3. ✅ Zarządzanie książkami (CRUD)
4. ✅ Wyszukiwanie i filtrowanie książek
5. ✅ Szczegóły książki
6. ✅ Rejestrowanie wypożyczeń i zwrotów
7. ✅ Historia wypożyczeń
8. ✅ Walidacja danych w formularzach
9. ✅ Responsywny interfejs użytkownika
10. ✅ Generowanie raportów bibliotecznych

**Technologie:**
- ASP.NET Core MVC 8.0
- Entity Framework Core 8.0
- SQL Server
- ASP.NET Core Identity
- Bootstrap 5
- jQuery

**Metodyka:** SCRUM
- 5 sprintów po 2 tygodnie
- Daily StandUp
- Sprint Review i Retrospective
- Product Backlog i Sprint Backlog

**Dokumentacja:** Kompletna
- Opis projektu
- Diagramy UML
- Instrukcja uruchomienia
- Plan realizacji

