namespace BibliotekaInternetowa.Models
{
    public class UserDetailsViewModel
    {
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<BorrowingViewModel> Borrowings { get; set; } = new();
    }

    public class BorrowingViewModel
    {
        public int Id { get; set; }
        public string BookTitle { get; set; } = null!;
        public string BookAuthor { get; set; } = null!;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public bool IsReturned { get; set; }
        public bool IsOverdue { get; set; }
    }
}

