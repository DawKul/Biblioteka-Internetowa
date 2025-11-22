using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BibliotekaInternetowa.Models;

namespace BibliotekaInternetowa.Data
{
    // Używamy IdentityDbContext z ApplicationUser
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; } = null!;
        public DbSet<Borrowing> Borrowings { get; set; } = null!;
    }
}