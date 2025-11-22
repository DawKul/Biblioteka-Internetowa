using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliotekaInternetowa.Data;
using BibliotekaInternetowa.Models;

namespace BibliotekaInternetowa.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
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
    }
}