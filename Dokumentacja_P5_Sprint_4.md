# Sprint_4 - Panel administratora. Zarządzanie użytkownikami

**Temat:** Sprint_4 - Panel administratora. Zarządzanie użytkownikami  
**Symbol:** P5  
**Nazwisko i imię:** Dawid Doktor, Dawid Kulig  
**Data ćwiczenia:** [Data do uzupełnienia]

---

## Cel Sprintu_4

**"Rozszerzyć funkcjonalności panelu administratora o zarządzanie użytkownikami (CRUD), przeglądanie wszystkich wypożyczeń oraz zaawansowane statystyki biblioteczne, umożliwiając administratorowi pełną kontrolę nad systemem bibliotecznym."**

## Backlog Sprintu_4

Z Product Backlogu wybrano następujące zadania do realizacji w Sprint_4:

| ID | Zadanie | Priorytet | Story Points | Status |
|----|---------|-----------|--------------|--------|
| 1 | **Panel administratora - Dashboard** | Wysoki | 5 | ✅ Ukończone |
|    | - Strona główna panelu admina | | | |
|    | - Statystyki: liczba książek, użytkowników, wypożyczeń | | | |
|    - Statystyki: książki najczęściej wypożyczane | | | |
|    | - Statystyki: przeterminowane wypożyczenia | | | |
|    | - Szybkie linki do zarządzania | | | |
| 2 | **Zarządzanie użytkownikami (CRUD)** | Wysoki | 8 | ✅ Ukończone |
|    | - Kontroler UsersController | | | |
|    | - Lista wszystkich użytkowników | | | |
|    | - Szczegóły użytkownika z historią wypożyczeń | | | |
|    | - Edycja danych użytkownika | | | |
|    | - Przypisywanie/odbieranie ról | | | |
|    | - Blokowanie/odblokowywanie kont | | | |
| 3 | **Przeglądanie wszystkich wypożyczeń** | Średni | 5 | ✅ Ukończone |
|    | - Widok wszystkich wypożyczeń (dla Admin) | | | |
|    | - Filtrowanie po statusie (wypożyczone/zwrócone/przeterminowane) | | | |
|    | - Filtrowanie po użytkowniku | | | |
|    | - Filtrowanie po książce | | | |
|    | - Sortowanie po dacie | | | |
| 4 | **Zaawansowane statystyki** | Średni | 5 | ✅ Ukończone |
|    | - Statystyki wypożyczeń w czasie (wykres) | | | |
|    | - Najpopularniejsze kategorie książek | | | |
|    | - Średni czas wypożyczenia | | | |
|    | - Statystyki użytkowników (najaktywniejszych) | | | |
| 5 | **Ulepszenia interfejsu administratora** | Niski | 3 | ✅ Ukończone |
|    | - Menu nawigacyjne dla Admin | | | |
|    | - Oznaczenia wizualne dla ról | | | |
|    | - Komunikaty i powiadomienia | | | |

**Suma Story Points:** 26

---

## Opis zrealizowanych funkcjonalności

### 1. Panel administratora - Dashboard

**Funkcjonalności:**
- Strona główna panelu administratora (`/Admin/Dashboard`)
- Statystyki ogólne:
  - Liczba książek w bibliotece
  - Liczba zarejestrowanych użytkowników
  - Liczba aktywnych wypożyczeń
  - Liczba przeterminowanych wypożyczeń
- Statystyki szczegółowe:
  - Top 5 najczęściej wypożyczanych książek
  - Top 5 najaktywniejszych użytkowników
  - Wykres wypożyczeń w ostatnich 30 dniach
- Szybkie linki:
  - Zarządzanie książkami
  - Zarządzanie użytkownikami
  - Wszystkie wypożyczenia
  - Statystyki szczegółowe

**Przykładowy kod kontrolera:**
```csharp
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    
    public async Task<IActionResult> Dashboard()
    {
        var stats = new AdminDashboardViewModel
        {
            TotalBooks = await _context.Books.CountAsync(),
            TotalUsers = await _context.Users.CountAsync(),
            ActiveBorrowings = await _context.Borrowings
                .CountAsync(b => !b.IsReturned),
            OverdueBorrowings = await _context.Borrowings
                .CountAsync(b => !b.IsReturned && b.DueDate < DateTime.Now),
            TopBooks = await _context.Borrowings
                .GroupBy(b => b.Book)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new { Book = g.Key, Count = g.Count() })
                .ToListAsync()
        };
        
        return View(stats);
    }
}
```

### 2. Zarządzanie użytkownikami

**UsersController - główne akcje:**

#### Index() - Lista użytkowników
```csharp
[Authorize(Roles = "Admin")]
public async Task<IActionResult> Index(string searchTerm)
{
    var users = _context.Users.AsQueryable();
    
    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        users = users.Where(u => 
            u.Email.Contains(searchTerm) || 
            u.FullName.Contains(searchTerm));
    }
    
    var userList = await users.ToListAsync();
    var userViewModels = new List<UserViewModel>();
    
    foreach (var user in userList)
    {
        var roles = await _userManager.GetRolesAsync(user);
        userViewModels.Add(new UserViewModel
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Roles = roles,
            BorrowingsCount = await _context.Borrowings
                .CountAsync(b => b.UserId == user.Id)
        });
    }
    
    return View(userViewModels);
}
```

#### Details(string id) - Szczegóły użytkownika
- Pełne dane użytkownika
- Lista ról
- Historia wypożyczeń użytkownika
- Statystyki wypożyczeń

#### Edit(string id) - Edycja użytkownika
- Zmiana danych użytkownika (Email, FullName)
- Przypisywanie/odbieranie ról (Admin, Czytelnik)
- Blokowanie/odblokowywanie konta

#### Delete(string id) - Usuwanie użytkownika
- Usuwanie użytkownika (z weryfikacją)
- Sprawdzanie aktywnych wypożyczeń przed usunięciem

### 3. Przeglądanie wszystkich wypożyczeń

**BorrowingsController - akcja AllBorrowings (dla Admin):**
```csharp
[Authorize(Roles = "Admin")]
public async Task<IActionResult> AllBorrowings(
    string status, 
    string userId, 
    int? bookId)
{
    var borrowings = _context.Borrowings
        .Include(b => b.Book)
        .Include(b => b.User)
        .AsQueryable();
    
    // Filtrowanie po statusie
    if (!string.IsNullOrWhiteSpace(status))
    {
        switch (status)
        {
            case "active":
                borrowings = borrowings.Where(b => !b.IsReturned);
                break;
            case "returned":
                borrowings = borrowings.Where(b => b.IsReturned);
                break;
            case "overdue":
                borrowings = borrowings.Where(b => 
                    !b.IsReturned && b.DueDate < DateTime.Now);
                break;
        }
    }
    
    // Filtrowanie po użytkowniku
    if (!string.IsNullOrWhiteSpace(userId))
    {
        borrowings = borrowings.Where(b => b.UserId == userId);
    }
    
    // Filtrowanie po książce
    if (bookId.HasValue)
    {
        borrowings = borrowings.Where(b => b.BookId == bookId.Value);
    }
    
    var result = await borrowings
        .OrderByDescending(b => b.BorrowDate)
        .ToListAsync();
    
    return View(result);
}
```

**Funkcjonalności widoku:**
- Tabela z wszystkimi wypożyczeniami
- Filtry: status, użytkownik, książka
- Sortowanie po dacie wypożyczenia
- Akcja zwrotu książki (przycisk dla Admin)
- Eksport do CSV/Excel (opcjonalnie)

### 4. Zaawansowane statystyki

**StatystykiController:**
```csharp
[Authorize(Roles = "Admin")]
public class StatisticsController : Controller
{
    public async Task<IActionResult> Index()
    {
        var stats = new StatisticsViewModel
        {
            // Wypożyczenia w ostatnich 30 dniach
            BorrowingsLast30Days = await GetBorrowingsByDateRange(30),
            
            // Najpopularniejsze kategorie
            PopularCategories = await _context.Borrowings
                .Include(b => b.Book)
                .GroupBy(b => b.Book.Category)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToListAsync(),
            
            // Średni czas wypożyczenia
            AverageBorrowingDays = await _context.Borrowings
                .Where(b => b.IsReturned && b.ReturnDate.HasValue)
                .Select(b => (b.ReturnDate.Value - b.BorrowDate).Days)
                .AverageAsync(),
            
            // Najaktywniejsi użytkownicy
            TopUsers = await _context.Borrowings
                .GroupBy(b => b.User)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new { User = g.Key, Count = g.Count() })
                .ToListAsync()
        };
        
        return View(stats);
    }
}
```

**Wykresy i wizualizacje:**
- Wykres liniowy: wypożyczenia w czasie (Chart.js lub podobne)
- Wykres kołowy: najpopularniejsze kategorie
- Wykres słupkowy: top użytkownicy

### 5. Ulepszenia interfejsu

**Menu nawigacyjne dla Admin:**
- Link "Panel Admin" w głównym menu (tylko dla Admin)
- Dropdown menu z opcjami:
  - Dashboard
  - Zarządzanie książkami
  - Zarządzanie użytkownikami
  - Wszystkie wypożyczenia
  - Statystyki

**Oznaczenia wizualne:**
- Badge "Admin" przy nazwie użytkownika
- Kolorowe ikony dla różnych sekcji
- Powiadomienia o przeterminowanych wypożyczeniach

---

## Diagram przepływu zarządzania użytkownikiem

```
[Admin]
    ↓
[Panel Admin → Zarządzanie użytkownikami]
    ↓
[Lista użytkowników]
    ↓
[Kliknij "Szczegóły" na użytkowniku]
    ↓
[Szczegóły użytkownika]
    ├─ [Historia wypożyczeń]
    ├─ [Edycja danych]
    ├─ [Zarządzanie rolami]
    │   ├─ [Dodaj rolę Admin]
    │   └─ [Usuń rolę]
    ├─ [Blokuj konto]
    └─ [Usuń użytkownika]
        ↓
    {Sprawdź: Czy ma aktywne wypożyczenia?}
        ├─ TAK → [Komunikat błędu]
        └─ NIE → [Potwierdzenie] → [Usuń]
```

---

## Zrzuty ekranów z aplikacji

*[Miejsce na wklejenie zrzutów ekranów:*
- *Dashboard administratora ze statystykami*
- *Lista użytkowników*
- *Szczegóły użytkownika z historią wypożyczeń*
- *Formularz edycji użytkownika*
- *Wszystkie wypożyczenia z filtrami*
- *Strona statystyk z wykresami*
*]*

---

## Testy funkcjonalności

### Test 1: Dostęp do panelu administratora
1. Zaloguj się jako Admin
2. Kliknij "Panel Admin" w menu
3. **Oczekiwany wynik:** Wyświetlenie dashboardu ze statystykami

### Test 2: Zarządzanie użytkownikami
1. Zaloguj się jako Admin
2. Przejdź do "Zarządzanie użytkownikami"
3. Kliknij "Szczegóły" na użytkowniku
4. **Oczekiwany wynik:** Wyświetlenie szczegółów użytkownika z historią wypożyczeń

### Test 3: Przypisanie roli Admin
1. Zaloguj się jako Admin
2. Edytuj użytkownika
3. Dodaj rolę "Admin"
4. **Oczekiwany wynik:** Użytkownik otrzymuje uprawnienia administratora

### Test 4: Filtrowanie wypożyczeń
1. Zaloguj się jako Admin
2. Przejdź do "Wszystkie wypożyczenia"
3. Użyj filtrów (status, użytkownik)
4. **Oczekiwany wynik:** Poprawne filtrowanie wyników

---

## Podsumowanie Sprintu_4

**Cel Sprintu osiągnięty:** ✅ TAK

Wszystkie zaplanowane zadania zostały zrealizowane. Aplikacja posiada:
- Pełny panel administratora z dashboardem
- Zarządzanie użytkownikami (CRUD)
- Przeglądanie wszystkich wypożyczeń z filtrami
- Zaawansowane statystyki biblioteczne
- Ulepszony interfejs dla administratora

**Kod programu:** Zawarty w repozytorium projektu BibliotekaInternetowa

---

## Modele widoków

**AdminDashboardViewModel:**
```csharp
public class AdminDashboardViewModel
{
    public int TotalBooks { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveBorrowings { get; set; }
    public int OverdueBorrowings { get; set; }
    public List<TopBookViewModel> TopBooks { get; set; }
    public List<TopUserViewModel> TopUsers { get; set; }
}
```

**UserViewModel:**
```csharp
public class UserViewModel
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string? FullName { get; set; }
    public IList<string> Roles { get; set; }
    public int BorrowingsCount { get; set; }
    public bool IsLockedOut { get; set; }
}
```

