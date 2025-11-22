namespace BibliotekaInternetowa.Models
{
    public class StatisticsViewModel
    {
        public Dictionary<string, int> BorrowingsByMonth { get; set; } = new();
        public Dictionary<string, int> BooksByCategory { get; set; } = new();
        public Dictionary<string, int> BorrowingsByStatus { get; set; } = new();
        public List<TopUserViewModel> TopUsers { get; set; } = new();
        public List<TopBookViewModel> TopBooks { get; set; } = new();
    }

    public class TopUserViewModel
    {
        public string UserName { get; set; } = null!;
        public string? FullName { get; set; }
        public int BorrowingsCount { get; set; }
    }

    public class TopBookViewModel
    {
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public int BorrowingsCount { get; set; }
    }
}

