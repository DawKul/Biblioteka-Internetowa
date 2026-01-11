namespace BibliotekaInternetowa.Models
{
    public class DeleteUserViewModel
    {
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public List<string> Roles { get; set; } = new();
        public int ActiveBorrowingsCount { get; set; }
        public int TotalBorrowingsCount { get; set; }
    }
}

