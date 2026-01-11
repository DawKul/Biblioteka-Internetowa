using System.Diagnostics;
using BibliotekaInternetowa.Models;
using BibliotekaInternetowa.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotekaInternetowa.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // GET: Home/Popular
        [AllowAnonymous]
        public async Task<IActionResult> Popular(string period = "month")
        {
            // Walidacja okresu
            if (period != "week" && period != "month" && period != "year")
            {
                period = "month";
            }

            // Oblicz datę początkową na podstawie wybranego okresu
            DateTime startDate = period switch
            {
                "week" => DateTime.Now.AddDays(-7),
                "month" => DateTime.Now.AddMonths(-1),
                "year" => DateTime.Now.AddYears(-1),
                _ => DateTime.Now.AddMonths(-1)
            };

            // Pobierz najpopularniejsze książki z wybranego okresu
            var popularBooks = await _context.Borrowings
                .Include(b => b.Book)
                .Where(b => b.BorrowDate >= startDate)
                .GroupBy(b => new
                {
                    b.BookId,
                    b.Book.Title,
                    b.Book.Author,
                    b.Book.ISBN,
                    b.Book.CoverImageUrl,
                    b.Book.Category,
                    b.Book.AvailableCopies,
                    b.Book.TotalCopies
                })
                .Select(g => new PopularBookItem
                {
                    BookId = g.Key.BookId,
                    Title = g.Key.Title,
                    Author = g.Key.Author,
                    ISBN = g.Key.ISBN,
                    CoverImageUrl = g.Key.CoverImageUrl,
                    Category = g.Key.Category,
                    BorrowingsCount = g.Count(),
                    AvailableCopies = g.Key.AvailableCopies,
                    TotalCopies = g.Key.TotalCopies
                })
                .OrderByDescending(b => b.BorrowingsCount)
                .Take(20) // Top 20 najpopularniejszych
                .ToListAsync();

            var viewModel = new PopularBooksViewModel
            {
                Period = period,
                Books = popularBooks
            };

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
