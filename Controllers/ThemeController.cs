using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BibliotekaInternetowa.Models;

namespace BibliotekaInternetowa.Controllers
{
    [Authorize]
    public class ThemeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ThemeController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // POST: Theme/Toggle
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Toggle()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            user.IsDarkMode = !user.IsDarkMode;
            await _userManager.UpdateAsync(user);

            return Ok(new { isDarkMode = user.IsDarkMode });
        }

        // GET: Theme/GetTheme
        [HttpGet]
        public async Task<IActionResult> GetTheme()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Ok(new { isDarkMode = false });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Ok(new { isDarkMode = false });
            }

            return Ok(new { isDarkMode = user.IsDarkMode });
        }
    }
}
