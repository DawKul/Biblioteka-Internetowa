# Sprint_3 - Rejestrowanie wypożyczeń i zwrotów. Historia wypożyczeń

**Temat:** Sprint_3 - Rejestrowanie wypożyczeń i zwrotów. Historia wypożyczeń  
**Symbol:** P4  
**Nazwisko i imię:** Dawid Doktor, Dawid Kulig  
**Data ćwiczenia:** [Data do uzupełnienia]

---

## Cel Sprintu_3

**"Zaimplementować system wypożyczeń i zwrotów książek oraz historię wypożyczeń, umożliwiając użytkownikom wypożyczanie książek i śledzenie swoich wypożyczeń, a administratorom zarządzanie zwrotami."**

## Backlog Sprintu_3

Z Product Backlogu wybrano następujące zadania do realizacji w Sprint_3:

| ID | Zadanie | Priorytet | Story Points | Status |
|----|---------|-----------|--------------|--------|
| 1 | **Model i relacje wypożyczeń** | Wysoki | 5 | ✅ Ukończone |
|    | - Utworzenie modelu Borrowing | | | |
|    | - Relacja Borrowing -> Book (Many-to-One) | | | |
|    | - Relacja Borrowing -> ApplicationUser (Many-to-One) | | | |
|    | - Pola: BorrowDate, DueDate, ReturnDate, IsReturned | | | |
| 2 | **Funkcjonalność wypożyczania** | Wysoki | 8 | ✅ Ukończone |
|    | - Akcja Borrow w BorrowingsController | | | |
|    | - Sprawdzanie dostępności książki | | | |
|    | - Zmniejszanie AvailableCopies przy wypożyczeniu | | | |
|    | - Ustawienie terminu zwrotu (30 dni) | | | |
|    | - Przycisk "Wypożycz" w widoku Details książki | | | |
|    | - Autoryzacja - tylko zalogowani użytkownicy | | | |
| 3 | **Funkcjonalność zwrotów** | Wysoki | 5 | ✅ Ukończone |
|    | - Akcja Return w BorrowingsController | | | |
|    | - Oznaczanie wypożyczenia jako zwróconego | | | |
|    | - Zwiększanie AvailableCopies przy zwrocie | | | |
|    | - Autoryzacja - tylko Admin może zwracać | | | |
| 4 | **Historia wypożyczeń użytkownika** | Średni | 5 | ✅ Ukończone |
|    | - Widok Index w BorrowingsController | | | |
|    | - Wyświetlanie tylko wypożyczeń zalogowanego użytkownika | | | |
|    | - Tabela z informacjami o wypożyczeniach | | | |
|    | - Status wypożyczenia (Wypożyczona/Zwrócona/Przeterminowana) | | | |
|    | - Sortowanie po dacie wypożyczenia (najnowsze pierwsze) | | | |
| 5 | **Integracja z widokiem książki** | Średni | 3 | ✅ Ukończone |
|    | - Przycisk wypożyczenia w Details.cshtml | | | |
|    | - Komunikaty sukcesu/błędu (TempData) | | | |
|    | - Sprawdzanie dostępności przed wyświetleniem przycisku | | | |

**Suma Story Points:** 26

---

## Opis zrealizowanych funkcjonalności

### 1. Model Borrowing

**Kod modelu:**
```csharp
public class Borrowing
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    public DateTime BorrowDate { get; set; } = DateTime.Now;
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public bool IsReturned { get; set; } = false;
}
```

**Relacje:**
- `Borrowing` -> `Book` (Many-to-One) - jedno wypożyczenie dotyczy jednej książki
- `Borrowing` -> `ApplicationUser` (Many-to-One) - jedno wypożyczenie należy do jednego użytkownika
- `ApplicationUser` -> `Borrowing` (One-to-Many) - jeden użytkownik może mieć wiele wypożyczeń

### 2. Kontroler BorrowingsController

**Główne akcje:**

#### Index() - Historia wypożyczeń
```csharp
[Authorize]
public async Task<IActionResult> Index()
{
    var user = await _userManager.GetUserAsync(User);
    var borrowings = await _context.Borrowings
        .Include(b => b.Book)
        .Where(b => b.UserId == user.Id)
        .OrderByDescending(b => b.BorrowDate)
        .ToListAsync();
    return View(borrowings);
}
```

#### Borrow(int bookId) - Wypożyczenie książki
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Borrow(int bookId)
{
    var user = await _userManager.GetUserAsync(User);
    var book = await _context.Books.FindAsync(bookId);
    
    if (book == null || book.AvailableCopies <= 0)
    {
        TempData["Error"] = "Książka nie jest dostępna do wypożyczenia.";
        return RedirectToAction("Details", "Books", new { id = bookId });
    }

    var borrowing = new Borrowing
    {
        UserId = user.Id,
        BookId = bookId,
        BorrowDate = DateTime.Now,
        DueDate = DateTime.Now.AddDays(30),
        IsReturned = false
    };

    book.AvailableCopies--;
    _context.Borrowings.Add(borrowing);
    await _context.SaveChangesAsync();

    TempData["Success"] = "Książka została wypożyczona pomyślnie!";
    return RedirectToAction("Index");
}
```

#### Return(int id) - Zwrot książki
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> Return(int id)
{
    var borrowing = await _context.Borrowings
        .Include(b => b.Book)
        .FirstOrDefaultAsync(b => b.Id == id);

    if (borrowing == null || borrowing.IsReturned)
    {
        return NotFound();
    }

    borrowing.IsReturned = true;
    borrowing.ReturnDate = DateTime.Now;
    borrowing.Book.AvailableCopies++;

    await _context.SaveChangesAsync();

    TempData["Success"] = "Książka została zwrócona.";
    return RedirectToAction("Index");
}
```

### 3. Widok historii wypożyczeń (Index.cshtml)

**Funkcjonalności:**
- Wyświetlanie tabeli z wypożyczeniami użytkownika
- Kolumny: Tytuł książki, Autor, Data wypożyczenia, Termin zwrotu, Data zwrotu, Status
- Statusy:
  - **Zwrócona** (zielony badge) - gdy IsReturned = true
  - **Przeterminowana** (czerwony badge) - gdy DueDate < DateTime.Now i IsReturned = false
  - **Wypożyczona** (żółty badge) - gdy IsReturned = false i DueDate >= DateTime.Now
- Komunikaty sukcesu/błędu (TempData)
- Link powrotu do katalogu książek

### 4. Integracja z widokiem Details książki

**Zmiany w Details.cshtml:**
- Przycisk "Wypożycz książkę" dla zalogowanych użytkowników (gdy AvailableCopies > 0)
- Przycisk "Niedostępna" (disabled) gdy brak dostępnych kopii
- Link do logowania dla niezalogowanych użytkowników
- Formularz POST do akcji Borrow z AntiForgeryToken

---

## Diagram przepływu wypożyczenia

```
[Użytkownik] 
    ↓
[Przegląda katalog książek]
    ↓
[Kliknij "Szczegóły" na książce]
    ↓
[Widok Details książki]
    ↓
{Sprawdź: Czy zalogowany?}
    ├─ NIE → [Link do logowania]
    └─ TAK → {Sprawdź: Czy dostępna?}
              ├─ NIE → [Przycisk "Niedostępna" (disabled)]
              └─ TAK → [Przycisk "Wypożycz książkę"]
                        ↓
                   [POST /Borrowings/Borrow]
                        ↓
                   {Sprawdź dostępność w bazie}
                        ├─ Niedostępna → [TempData Error] → [Powrót do Details]
                        └─ Dostępna → [Utwórz Borrowing]
                                      [Zmniejsz AvailableCopies]
                                      [Zapisz do bazy]
                                      [TempData Success]
                                      ↓
                                   [Przekieruj do /Borrowings/Index]
```

---

## Zrzuty ekranów z aplikacji

*[Miejsce na wklejenie zrzutów ekranów:*
- *Widok szczegółów książki z przyciskiem "Wypożycz"*
- *Formularz wypożyczenia (komunikat sukcesu)*
- *Historia wypożyczeń użytkownika (tabela)*
- *Statusy wypożyczeń (Wypożyczona/Zwrócona/Przeterminowana)*
- *Widok zwrotu książki (dla Admin)*
*]*

---

## Testy funkcjonalności

### Test 1: Wypożyczenie dostępnej książki
1. Zaloguj się jako użytkownik
2. Przejdź do katalogu książek
3. Kliknij "Szczegóły" na dostępnej książce
4. Kliknij "Wypożycz książkę"
5. **Oczekiwany wynik:** Komunikat sukcesu, przekierowanie do historii wypożyczeń, zmniejszenie AvailableCopies

### Test 2: Próba wypożyczenia niedostępnej książki
1. Zaloguj się jako użytkownik
2. Przejdź do książki z AvailableCopies = 0
3. Kliknij "Wypożycz książkę"
4. **Oczekiwany wynik:** Komunikat błędu, brak możliwości wypożyczenia

### Test 3: Historia wypożyczeń
1. Zaloguj się jako użytkownik z wypożyczeniami
2. Przejdź do "Moje wypożyczenia"
3. **Oczekiwany wynik:** Wyświetlenie tabeli z wszystkimi wypożyczeniami użytkownika, poprawne statusy

### Test 4: Zwrot książki (Admin)
1. Zaloguj się jako Admin
2. Przejdź do historii wypożyczeń
3. Zwróć książkę
4. **Oczekiwany wynik:** Oznaczenie jako zwrócona, zwiększenie AvailableCopies

---

## Podsumowanie Sprintu_3

**Cel Sprintu osiągnięty:** ✅ TAK

Wszystkie zaplanowane zadania zostały zrealizowane. Aplikacja posiada:
- Pełną funkcjonalność wypożyczania książek
- System zwrotów (dla administratorów)
- Historię wypożyczeń użytkownika
- Integrację z widokiem szczegółów książki
- Automatyczne zarządzanie dostępnością książek

**Kod programu:** Zawarty w repozytorium projektu BibliotekaInternetowa

---

## Migracje bazy danych

Dodano migrację dla tabeli Borrowings:
- Tabela `Borrowings` z polami: Id, UserId, BookId, BorrowDate, DueDate, ReturnDate, IsReturned
- Klucze obce: UserId -> AspNetUsers, BookId -> Books
- Indeksy dla optymalizacji zapytań

