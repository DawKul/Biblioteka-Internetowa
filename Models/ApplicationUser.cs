using Microsoft.AspNetCore.Identity;

namespace BibliotekaInternetowa.Models
{
    // Dziedziczymy po oryginalnym IdentityUser z Microsoft
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }

        // Relacja 1-do-wielu z wypożyczeniami
        public virtual ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();
    }
}