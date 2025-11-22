using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BibliotekaInternetowa.Data;
using BibliotekaInternetowa.Models;
using BibliotekaInternetowa.Services;

namespace BibliotekaInternetowa.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly PdfReportService _pdfReportService;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            PdfReportService pdfReportService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _pdfReportService = pdfReportService;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var totalBooks = await _context.Books.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalBorrowings = await _context.Borrowings.CountAsync();
            var activeBorrowings = await _context.Borrowings
                .Where(b => !b.IsReturned)
                .CountAsync();
            var overdueBorrowings = await _context.Borrowings
                .Where(b => !b.IsReturned && b.DueDate < DateTime.Now)
                .CountAsync();
            var availableBooks = await _context.Books
                .Where(b => b.AvailableCopies > 0)
                .CountAsync();

            var recentBorrowings = await _context.Borrowings
                .Include(b => b.User)
                .Include(b => b.Book)
                .OrderByDescending(b => b.BorrowDate)
                .Take(10)
                .Select(b => new RecentBorrowingViewModel
                {
                    Id = b.Id,
                    UserName = b.User.UserName ?? "",
                    BookTitle = b.Book.Title,
                    BorrowDate = b.BorrowDate,
                    DueDate = b.DueDate,
                    IsReturned = b.IsReturned
                })
                .ToListAsync();

            var popularBooks = await _context.Borrowings
                .GroupBy(b => new { b.BookId, b.Book.Title })
                .Select(g => new PopularBookViewModel
                {
                    BookId = g.Key.BookId,
                    Title = g.Key.Title,
                    BorrowCount = g.Count()
                })
                .OrderByDescending(b => b.BorrowCount)
                .Take(5)
                .ToListAsync();

            var viewModel = new AdminDashboardViewModel
            {
                TotalBooks = totalBooks,
                TotalUsers = totalUsers,
                TotalBorrowings = totalBorrowings,
                ActiveBorrowings = activeBorrowings,
                OverdueBorrowings = overdueBorrowings,
                AvailableBooks = availableBooks,
                RecentBorrowings = recentBorrowings,
                PopularBooks = popularBooks
            };

            return View(viewModel);
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            var userViewModels = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var borrowingsCount = await _context.Borrowings
                    .Where(b => b.UserId == user.Id)
                    .CountAsync();
                var activeBorrowingsCount = await _context.Borrowings
                    .Where(b => b.UserId == user.Id && !b.IsReturned)
                    .CountAsync();

                userViewModels.Add(new UserListViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "",
                    Email = user.Email,
                    FullName = user.FullName,
                    Roles = roles.ToList(),
                    BorrowingsCount = borrowingsCount,
                    ActiveBorrowingsCount = activeBorrowingsCount
                });
            }

            return View(userViewModels);
        }

        // GET: Admin/UserDetails/5
        public async Task<IActionResult> UserDetails(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var borrowings = await _context.Borrowings
                .Include(b => b.Book)
                .Where(b => b.UserId == user.Id)
                .OrderByDescending(b => b.BorrowDate)
                .Select(b => new BorrowingViewModel
                {
                    Id = b.Id,
                    BookTitle = b.Book.Title,
                    BookAuthor = b.Book.Author,
                    BorrowDate = b.BorrowDate,
                    DueDate = b.DueDate,
                    ReturnDate = b.ReturnDate,
                    IsReturned = b.IsReturned,
                    IsOverdue = !b.IsReturned && b.DueDate < DateTime.Now
                })
                .ToListAsync();

            var viewModel = new UserDetailsViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email,
                FullName = user.FullName,
                Roles = roles.ToList(),
                Borrowings = borrowings
            };

            return View(viewModel);
        }

        // GET: Admin/EditUser/5
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();

            var viewModel = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email,
                FullName = user.FullName,
                SelectedRoles = roles.ToList(),
                AvailableRoles = allRoles
            };

            return View(viewModel);
        }

        // POST: Admin/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.FullName = model.FullName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                model.AvailableRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                return View(model);
            }

            // Aktualizuj role
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (model.SelectedRoles.Any())
            {
                await _userManager.AddToRolesAsync(user, model.SelectedRoles);
            }

            TempData["Success"] = "Użytkownik został zaktualizowany pomyślnie.";
            return RedirectToAction(nameof(UserDetails), new { id = user.Id });
        }

        // GET: Admin/AllBorrowings
        public async Task<IActionResult> AllBorrowings(string? searchTerm, string? status, string? userId)
        {
            var borrowings = _context.Borrowings
                .Include(b => b.User)
                .Include(b => b.Book)
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
                        borrowings = borrowings.Where(b => !b.IsReturned && b.DueDate < DateTime.Now);
                        break;
                }
            }

            // Filtrowanie po użytkowniku
            if (!string.IsNullOrWhiteSpace(userId))
            {
                borrowings = borrowings.Where(b => b.UserId == userId);
            }

            // Wyszukiwanie
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                borrowings = borrowings.Where(b =>
                    EF.Functions.Like(b.User.UserName ?? "", $"%{searchTerm}%") ||
                    EF.Functions.Like(b.Book.Title, $"%{searchTerm}%") ||
                    EF.Functions.Like(b.Book.Author, $"%{searchTerm}%"));
            }

            var borrowingList = await borrowings
                .OrderByDescending(b => b.BorrowDate)
                .Select(b => new BorrowingDetailViewModel
                {
                    Id = b.Id,
                    UserName = b.User.UserName ?? "",
                    UserFullName = b.User.FullName,
                    BookTitle = b.Book.Title,
                    BookAuthor = b.Book.Author,
                    BorrowDate = b.BorrowDate,
                    DueDate = b.DueDate,
                    ReturnDate = b.ReturnDate,
                    IsReturned = b.IsReturned,
                    IsOverdue = !b.IsReturned && b.DueDate < DateTime.Now
                })
                .ToListAsync();

            var users = await _context.Users
                .Select(u => new UserSelectItem
                {
                    Id = u.Id,
                    UserName = u.UserName ?? ""
                })
                .OrderBy(u => u.UserName)
                .ToListAsync();

            var viewModel = new AllBorrowingsViewModel
            {
                SearchTerm = searchTerm,
                Status = status ?? "all",
                UserId = userId,
                Borrowings = borrowingList,
                Users = users
            };

            return View(viewModel);
        }

        // GET: Admin/Statistics
        public async Task<IActionResult> Statistics()
        {
            var last12Months = Enumerable.Range(0, 12)
                .Select(i => DateTime.Now.AddMonths(-i))
                .Reverse()
                .ToList();

            var borrowingsByMonth = await _context.Borrowings
                .Where(b => b.BorrowDate >= last12Months.First())
                .GroupBy(b => new { b.BorrowDate.Year, b.BorrowDate.Month })
                .Select(g => new
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.Month, x => x.Count);

            var booksByCategory = await _context.Books
                .GroupBy(b => b.Category)
                .Select(g => new { Category = g.Key ?? "Brak", Count = g.Count() })
                .ToDictionaryAsync(x => x.Category, x => x.Count);

            var borrowingsByStatus = new Dictionary<string, int>
            {
                ["Aktywne"] = await _context.Borrowings.Where(b => !b.IsReturned).CountAsync(),
                ["Zwrócone"] = await _context.Borrowings.Where(b => b.IsReturned).CountAsync(),
                ["Przeterminowane"] = await _context.Borrowings
                    .Where(b => !b.IsReturned && b.DueDate < DateTime.Now)
                    .CountAsync()
            };

            var topUsers = await _context.Borrowings
                .Include(b => b.User)
                .GroupBy(b => new { b.UserId, b.User.UserName, b.User.FullName })
                .Select(g => new TopUserViewModel
                {
                    UserName = g.Key.UserName ?? "",
                    FullName = g.Key.FullName,
                    BorrowingsCount = g.Count()
                })
                .OrderByDescending(u => u.BorrowingsCount)
                .Take(10)
                .ToListAsync();

            var topBooks = await _context.Borrowings
                .Include(b => b.Book)
                .GroupBy(b => new { b.BookId, b.Book.Title, b.Book.Author })
                .Select(g => new TopBookViewModel
                {
                    Title = g.Key.Title,
                    Author = g.Key.Author,
                    BorrowingsCount = g.Count()
                })
                .OrderByDescending(b => b.BorrowingsCount)
                .Take(10)
                .ToListAsync();

            var viewModel = new StatisticsViewModel
            {
                BorrowingsByMonth = borrowingsByMonth,
                BooksByCategory = booksByCategory,
                BorrowingsByStatus = borrowingsByStatus,
                TopUsers = topUsers,
                TopBooks = topBooks
            };

            return View(viewModel);
        }

        // GET: Admin/GenerateBorrowingsReport
        public IActionResult GenerateBorrowingsReport(string? status, string? userId)
        {
            var pdfBytes = _pdfReportService.GenerateBorrowingsReport(status, userId);
            var fileName = $"raport_wypozyczen_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        // GET: Admin/GenerateStatisticsReport
        public IActionResult GenerateStatisticsReport()
        {
            var pdfBytes = _pdfReportService.GenerateStatisticsReport();
            var fileName = $"raport_statystyk_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}

