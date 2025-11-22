namespace BibliotekaInternetowa.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalBooks { get; set; }
        public int TotalUsers { get; set; }
        public int TotalBorrowings { get; set; }
        public int ActiveBorrowings { get; set; }
        public int OverdueBorrowings { get; set; }
        public int AvailableBooks { get; set; }
        public List<RecentBorrowingViewModel> RecentBorrowings { get; set; } = new();
        public List<PopularBookViewModel> PopularBooks { get; set; } = new();
    }

    public class RecentBorrowingViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public string BookTitle { get; set; } = null!;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsReturned { get; set; }
    }

    public class PopularBookViewModel
    {
        public int BookId { get; set; }
        public string Title { get; set; } = null!;
        public int BorrowCount { get; set; }
    }
}

