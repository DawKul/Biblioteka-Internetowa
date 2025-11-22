namespace BibliotekaInternetowa.Models
{
    public class AllBorrowingsViewModel
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; } // "all", "active", "returned", "overdue"
        public string? UserId { get; set; }
        public List<BorrowingDetailViewModel> Borrowings { get; set; } = new();
        public List<UserSelectItem> Users { get; set; } = new();
    }

    public class UserSelectItem
    {
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = null!;
    }

    public class BorrowingDetailViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public string? UserFullName { get; set; }
        public string BookTitle { get; set; } = null!;
        public string BookAuthor { get; set; } = null!;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public bool IsReturned { get; set; }
        public bool IsOverdue { get; set; }
    }
}

