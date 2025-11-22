using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BibliotekaInternetowa.Data;
using BibliotekaInternetowa.Models;

namespace BibliotekaInternetowa.Controllers
{
    [Authorize]
    public class BorrowingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BorrowingsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Borrowings
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var borrowings = await _context.Borrowings
                .Include(b => b.Book)
                .Where(b => b.UserId == user.Id)
                .OrderByDescending(b => b.BorrowDate)
                .ToListAsync();

            return View(borrowings);
        }

        // POST: Borrowings/Borrow
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrow(int bookId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

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
                DueDate = DateTime.Now.AddDays(30), // 30 dni na zwrot
                IsReturned = false
            };

            book.AvailableCopies--;
            _context.Borrowings.Add(borrowing);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Książka została wypożyczona pomyślnie!";
            return RedirectToAction("Index");
        }

        // POST: Borrowings/Return/5
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
    }
}

