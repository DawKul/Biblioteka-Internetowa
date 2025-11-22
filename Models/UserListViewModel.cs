using Microsoft.AspNetCore.Identity;

namespace BibliotekaInternetowa.Models
{
    public class UserListViewModel
    {
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public List<string> Roles { get; set; } = new();
        public int BorrowingsCount { get; set; }
        public int ActiveBorrowingsCount { get; set; }
    }
}

