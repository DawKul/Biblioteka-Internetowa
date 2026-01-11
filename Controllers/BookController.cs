using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliotekaInternetowa.Data;
using BibliotekaInternetowa.Models;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;

namespace BibliotekaInternetowa.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;

        public BooksController(ApplicationDbContext context, IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _env = env;
        }

        // GET: Books - dostępne dla wszystkich
        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchTerm, string category, string availability, int? minYear, int? maxYear)
        {
            var books = _context.Books.AsQueryable();

            // Wyszukiwanie po tytule, autorze lub ISBN (case-insensitive)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                books = books.Where(b => 
                    EF.Functions.Like(b.Title, $"%{searchTerm}%") || 
                    EF.Functions.Like(b.Author, $"%{searchTerm}%") || 
                    EF.Functions.Like(b.ISBN, $"%{searchTerm}%"));
            }

            // Filtrowanie po kategorii
            if (!string.IsNullOrWhiteSpace(category))
            {
                books = books.Where(b => b.Category == category);
            }

            // Filtrowanie po dostępności
            if (!string.IsNullOrWhiteSpace(availability))
            {
                if (availability == "available")
                {
                    books = books.Where(b => b.AvailableCopies > 0);
                }
                else if (availability == "unavailable")
                {
                    books = books.Where(b => b.AvailableCopies == 0);
                }
            }

            // Filtrowanie po roku wydania
            if (minYear.HasValue)
            {
                books = books.Where(b => b.PublicationYear >= minYear.Value);
            }

            if (maxYear.HasValue)
            {
                books = books.Where(b => b.PublicationYear <= maxYear.Value);
            }

            var bookList = await books.OrderBy(b => b.Title).ToListAsync();
            var categories = await _context.Books
                .Select(b => b.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            var viewModel = new BookSearchViewModel
            {
                SearchTerm = searchTerm,
                Category = category,
                Availability = availability ?? "all",
                MinYear = minYear,
                MaxYear = maxYear,
                Books = bookList,
                Categories = categories
            };

            return View(viewModel);
        }

        // GET: Books/Details/5 - dostępne dla wszystkich
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var book = await _context.Books.FirstOrDefaultAsync(m => m.Id == id);
            if (book == null) return NotFound();
            return View(book);
        }

        // GET: Books/Create - tylko dla Admin
        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        // POST: Books/Create - tylko dla Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Book book)
        {
            if (ModelState.IsValid)
            {
                book.AvailableCopies = book.TotalCopies;
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Edit/5 - tylko dla Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }

        // POST: Books/Edit/5 - tylko dla Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Book book)
        {
            if (id != book.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Books.Any(e => e.Id == book.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Delete/5 - tylko dla Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var book = await _context.Books.FirstOrDefaultAsync(m => m.Id == id);
            if (book == null) return NotFound();
            return View(book);
        }

        // POST: Books/Delete/5 - tylko dla Admin
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null) _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Books/Cover/{isbn}?size=L
        [AllowAnonymous]
        [HttpGet("Books/Cover/{isbn}")]
        public async Task<IActionResult> Cover(string isbn, string size = "L")
        {
            if (string.IsNullOrWhiteSpace(isbn)) return NotFound();

            var isbn13 = NormalizeIsbn(isbn);
            if (string.IsNullOrWhiteSpace(isbn13)) return NotFound();

            size = size?.ToUpperInvariant() switch
            {
                "S" => "S",
                "M" => "M",
                _ => "L"
            };

            var coversRoot = Path.Combine(_env.WebRootPath, "covers");
            Directory.CreateDirectory(coversRoot);

            // Mapowanie ISBN-13 na ISBN-10 dla plików LibraryThing/Amazon
            var isbn10 = ConvertIsbn13To10(isbn13);

            // 1. Spróbuj standardowego formatu: {isbn13}-{size}.jpg
            var standardFileName = $"{isbn13}-{size}.jpg";
            var standardFilePath = Path.Combine(coversRoot, standardFileName);
            if (System.IO.File.Exists(standardFilePath))
            {
                return PhysicalFile(standardFilePath, "image/jpeg");
            }

            // 2. Spróbuj formatu LibraryThing/Amazon: {isbn10}.01._SX900_SY1270_SCLZZZZZZZ_.jpg
            if (!string.IsNullOrWhiteSpace(isbn10))
            {
                var libraryThingFileName = $"{isbn10}.01._SX900_SY1270_SCLZZZZZZZ_.jpg";
                var libraryThingFilePath = Path.Combine(coversRoot, libraryThingFileName);
                if (System.IO.File.Exists(libraryThingFilePath))
                {
                    return PhysicalFile(libraryThingFilePath, "image/jpeg");
                }

                // 3. Spróbuj znaleźć plik zaczynający się od ISBN-10 (różne rozszerzenia)
                var extensions = new[] { "*.jpg", "*.jpeg", "*.png" };
                foreach (var ext in extensions)
                {
                    var files = Directory.GetFiles(coversRoot, $"{isbn10}*{ext.Replace("*", "")}");
                    if (files.Length > 0)
                    {
                        return PhysicalFile(files[0], "image/jpeg");
                    }
                }

                // 4. Spróbuj znaleźć plik zawierający ISBN-10 w nazwie (dla plików z dodatkowymi znakami)
                var allFiles = Directory.GetFiles(coversRoot, "*.*")
                    .Where(f => Path.GetExtension(f).Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                Path.GetExtension(f).Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                Path.GetExtension(f).Equals(".png", StringComparison.OrdinalIgnoreCase))
                    .Where(f => Path.GetFileNameWithoutExtension(f).Contains(isbn10))
                    .ToList();
                
                if (allFiles.Count > 0)
                {
                    return PhysicalFile(allFiles[0], "image/jpeg");
                }
            }

            // 5. Spróbuj znaleźć plik zaczynający się od ISBN-13 (różne rozszerzenia)
            var extensionsIsbn13 = new[] { "*.jpg", "*.jpeg", "*.png" };
            foreach (var ext in extensionsIsbn13)
            {
                var filesByIsbn13 = Directory.GetFiles(coversRoot, $"{isbn13}*{ext.Replace("*", "")}");
                if (filesByIsbn13.Length > 0)
                {
                    return PhysicalFile(filesByIsbn13[0], "image/jpeg");
                }
            }

            // 6. Spróbuj znaleźć plik zawierający ISBN-13 w nazwie
            var allFilesIsbn13 = Directory.GetFiles(coversRoot, "*.*")
                .Where(f => Path.GetExtension(f).Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                            Path.GetExtension(f).Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                            Path.GetExtension(f).Equals(".png", StringComparison.OrdinalIgnoreCase))
                .Where(f => Path.GetFileNameWithoutExtension(f).Contains(isbn13))
                .ToList();
            
            if (allFilesIsbn13.Count > 0)
            {
                return PhysicalFile(allFilesIsbn13[0], "image/jpeg");
            }

            // 7. Spróbuj pobrać z Open Library i zapisać lokalnie
            var sizesToTry = new[] { size, "M", "S" };
            var client = _httpClientFactory.CreateClient();

            foreach (var s in sizesToTry.Distinct())
            {
                var url = $"https://covers.openlibrary.org/b/isbn/{isbn13}-{s}.jpg?default=false";
                try
                {
                    using var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        await using var stream = await response.Content.ReadAsStreamAsync();
                        await using var fs = System.IO.File.Create(standardFilePath);
                        await stream.CopyToAsync(fs);
                        return PhysicalFile(standardFilePath, "image/jpeg");
                    }
                }
                catch
                {
                    // ignoruj i próbuj dalej
                }
            }

            return NotFound();
        }

        private static string ConvertIsbn13To10(string isbn13)
        {
            // Mapowanie znanych ISBN-13 na ISBN-10 dla plików LibraryThing/Amazon
            var mapping = new Dictionary<string, string>
            {
                { "9788308060191", "0060883286" }, // Sto lat samotności
                { "9788308053810", "0156012197" }, // Mały Książę
                { "9788371808911", "0345339703" }, // Drużyna Pierścienia
                { "9788308060214", "0446675539" }, // Przeminęło z wiatrem
                { "9788307033020", "0451524934" }, // 1984
                { "9788308060207", "0553213105" }, // Duma i uprzedzenie
                { "9788374800012", "0590353403" }, // Harry Potter i Kamień Filozoficzny
                { "9788371808904", "0618260307" }, // Hobbit
                { "9788308060153", "0679734503" }, // Zbrodnia i kara
                { "9788308060184", "0679760806" }, // Mistrz i Małgorzata
                { "9780141439471", "0141439475" }  // Frankenstein
            };

            return mapping.TryGetValue(isbn13, out var isbn10) ? isbn10 : string.Empty;
        }

        private static string NormalizeIsbn(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            var chars = raw.Where(c => char.IsDigit(c) || c == 'X' || c == 'x').ToArray();
            return new string(chars);
        }
    }
}