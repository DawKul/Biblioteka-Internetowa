# Sprint_2 - ASP.NET MVC Core. Daily StandUp. Backlog Sprintu_2 oraz Cel Sprintu_2

**Temat:** Sprint_2 ASP.NET MVC Core. Daily StandUp. Backlog Sprintu_2 oraz Cel Sprintu_2  
**Symbol:** P3  
**Nazwisko i imię:** Dawid Doktor, Dawid Kulig  
**Data ćwiczenia:** [Data do uzupełnienia]

---

## Zadanie_1: Daily StandUp

### Opis Daily StandUp

Daily StandUp (Daily Scrum) to codzienne spotkanie zespołu deweloperskiego, które odbywa się o stałej porze i trwa maksymalnie 15 minut. Celem jest synchronizacja pracy zespołu i identyfikacja przeszkód.

### Format Daily StandUp

Każdy członek zespołu odpowiada na trzy pytania:
1. **Co zrobiłem wczoraj?** (What did I do yesterday?)
2. **Co zrobię dzisiaj?** (What will I do today?)
3. **Czy napotkałem jakieś przeszkody?** (Are there any impediments?)

### Przykładowy zapis Daily StandUp dla Sprintu_2

**Data:** [Data spotkania]  
**Uczestnicy:** Dawid Doktor, Dawid Kulig

#### Dawid Doktor:
- **Wczoraj:** 
  - Skonfigurowałem strukturę projektu ASP.NET Core MVC
  - Utworzyłem modele danych (Book, Borrowing, ApplicationUser)
  - Skonfigurowałem Entity Framework Core z SQL Server
  - Utworzyłem migracje bazy danych
  
- **Dzisiaj:**
  - Zaimplementuję kontrolery (BooksController, BorrowingsController)
  - Utworzę widoki dla operacji CRUD na książkach
  - Dodam walidację formularzy
  
- **Przeszkody:** Brak

#### Dawid Kulig:
- **Wczoraj:**
  - Zaprojektowałem strukturę bazy danych
  - Przygotowałem diagram klas
  - Skonfigurowałem Identity dla autoryzacji użytkowników
  
- **Dzisiaj:**
  - Zaimplementuję widoki wyszukiwania i filtrowania książek
  - Dodam responsywny interfejs użytkownika z Bootstrap
  - Utworzę widoki dla wypożyczeń
  
- **Przeszkody:** Brak

### Zrzut ekranu Daily StandUp

*[Miejsce na wklejenie zrzutu ekranu z narzędzia do zarządzania projektem (np. Trello, Jira, Azure DevOps) lub z dokumentu Word/Excel z zapisem spotkania]*

---

## Zadanie_2: Backlog Sprintu_2 oraz Cel Sprintu_2

### Cel Sprintu_2

**"Zaimplementować podstawową funkcjonalność zarządzania książkami (CRUD) oraz wyszukiwania i filtrowania książek w katalogu bibliotecznym, umożliwiając użytkownikom przeglądanie zbiorów bibliotecznych."**

### Backlog Sprintu_2

Z Product Backlogu wybrano następujące zadania do realizacji w Sprint_2:

| ID | Zadanie | Priorytet | Story Points | Status |
|----|---------|-----------|--------------|--------|
| 1 | **Zarządzanie książkami (CRUD)** | Wysoki | 8 | ✅ Ukończone |
|    | - Utworzenie modelu Book z walidacją | | | |
|    | - Implementacja BooksController z akcjami CRUD | | | |
|    | - Utworzenie widoków: Index, Create, Edit, Delete, Details | | | |
|    | - Autoryzacja - tylko Admin może dodawać/edytować/usuwac | | | |
| 2 | **Wyszukiwanie i filtrowanie książek** | Wysoki | 5 | ✅ Ukończone |
|    | - Formularz wyszukiwania po tytule, autorze, ISBN | | | |
|    | - Filtrowanie po kategorii | | | |
|    | - Filtrowanie po dostępności (dostępne/niedostępne) | | | |
|    | - Filtrowanie po roku wydania (min/max) | | | |
|    | - Utworzenie BookSearchViewModel | | | |
| 3 | **Szczegóły książki** | Średni | 3 | ✅ Ukończone |
|    | - Widok szczegółów z pełnymi informacjami o książce | | | |
|    | - Wyświetlanie dostępności (dostępne kopie) | | | |
|    | - Przycisk wypożyczenia (dla zalogowanych użytkowników) | | | |
| 4 | **Walidacja danych w formularzach** | Średni | 3 | ✅ Ukończone |
|    | - Walidacja po stronie serwera (DataAnnotations) | | | |
|    - Walidacja po stronie klienta (jQuery Validation) | | | |
|    | - Komunikaty błędów walidacji | | | |
| 5 | **Responsywny interfejs użytkownika** | Średni | 5 | ✅ Ukończone |
|    | - Responsywny layout z Bootstrap 5 | | | |
|    | - Karty książek z okładkami | | | |
|    | - Responsywne formularze wyszukiwania | | | |
|    | - Nawigacja mobilna | | | |

**Suma Story Points:** 24

### Opis zrealizowanych funkcjonalności

#### 1. Zarządzanie książkami (CRUD)

**Model Book:**
```csharp
public class Book
{
    public int Id { get; set; }
    [Required] public string Title { get; set; }
    [Required] public string Author { get; set; }
    [Required] public string ISBN { get; set; }
    public int PublicationYear { get; set; }
    [Required] public string Category { get; set; }
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public int TotalCopies { get; set; } = 1;
    public int AvailableCopies { get; set; } = 1;
}
```

**Kontroler BooksController:**
- `Index()` - wyświetlanie listy książek z wyszukiwaniem i filtrowaniem
- `Details(int id)` - szczegóły książki
- `Create()` - formularz dodawania (tylko Admin)
- `Edit(int id)` - formularz edycji (tylko Admin)
- `Delete(int id)` - usuwanie książki (tylko Admin)

**Widoki:**
- `Index.cshtml` - lista książek z kartami, wyszukiwanie i filtry
- `Create.cshtml` - formularz dodawania książki
- `Edit.cshtml` - formularz edycji książki
- `Delete.cshtml` - potwierdzenie usunięcia
- `Details.cshtml` - szczegóły książki

#### 2. Wyszukiwanie i filtrowanie książek

**BookSearchViewModel:**
```csharp
public class BookSearchViewModel
{
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public string? Availability { get; set; }
    public int? MinYear { get; set; }
    public int? MaxYear { get; set; }
    public List<Book> Books { get; set; }
    public List<string> Categories { get; set; }
}
```

**Funkcjonalności:**
- Wyszukiwanie po tytule, autorze lub ISBN (case-insensitive)
- Filtrowanie po kategorii (dropdown z unikalnymi kategoriami)
- Filtrowanie po dostępności (wszystkie/dostępne/niedostępne)
- Filtrowanie po roku wydania (zakres min-max)
- Wyświetlanie liczby znalezionych książek

#### 3. Szczegóły książki

Widok `Details.cshtml` zawiera:
- Pełne informacje o książce (tytuł, autor, ISBN, rok, kategoria, opis)
- Okładka książki (jeśli dostępna)
- Status dostępności (liczba dostępnych kopii)
- Przycisk "Wypożycz" (dla zalogowanych użytkowników)
- Przyciski edycji/usunięcia (tylko dla Admin)

#### 4. Walidacja danych

**Walidacja po stronie serwera:**
- Atrybuty `[Required]` w modelu Book
- `ModelState.IsValid` w kontrolerach
- Komunikaty błędów w widokach

**Walidacja po stronie klienta:**
- jQuery Validation Unobtrusive
- Walidacja w czasie rzeczywistym podczas wypełniania formularza

#### 5. Responsywny interfejs użytkownika

- Bootstrap 5 dla responsywnego layoutu
- Karty książek z efektem hover
- Responsywne formularze (kolumny dostosowują się do rozmiaru ekranu)
- Mobilna nawigacja
- Gradientowe tło dla okładek książek

### Zrzuty ekranów z aplikacji

*[Miejsce na wklejenie zrzutów ekranów:*
- *Lista książek (Index)*
- *Formularz dodawania książki (Create)*
- *Formularz edycji książki (Edit)*
- *Szczegóły książki (Details)*
- *Wyszukiwanie i filtrowanie*
- *Widok mobilny*
*]*

### Podsumowanie Sprintu_2

**Cel Sprintu osiągnięty:** ✅ TAK

Wszystkie zaplanowane zadania zostały zrealizowane. Aplikacja posiada:
- Pełną funkcjonalność CRUD dla książek
- Zaawansowane wyszukiwanie i filtrowanie
- Responsywny interfejs użytkownika
- Walidację formularzy
- Autoryzację opartą na rolach

**Kod programu:** Zawarty w repozytorium projektu BibliotekaInternetowa

---

## Dodatkowe informacje

### Technologie użyte w Sprint_2:
- ASP.NET Core MVC 8.0
- Entity Framework Core 8.0
- SQL Server / LocalDB
- ASP.NET Core Identity
- Bootstrap 5
- jQuery Validation

### Struktura projektu:
```
BibliotekaInternetowa/
├── Controllers/
│   ├── BooksController.cs
│   └── BorrowingsController.cs
├── Models/
│   ├── Book.cs
│   ├── BookSearchViewModel.cs
│   ├── Borrowing.cs
│   └── ApplicationUser.cs
├── Views/
│   └── Books/
│       ├── Index.cshtml
│       ├── Create.cshtml
│       ├── Edit.cshtml
│       ├── Delete.cshtml
│       └── Details.cshtml
└── Data/
    └── ApplicationDbContext.cs
```

