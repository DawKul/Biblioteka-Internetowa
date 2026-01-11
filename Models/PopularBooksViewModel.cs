namespace BibliotekaInternetowa.Models
{
    public class PopularBooksViewModel
    {
        public string Period { get; set; } = "month"; // "week", "month", "year"
        public List<PopularBookItem> Books { get; set; } = new List<PopularBookItem>();
    }

    public class PopularBookItem
    {
        public int BookId { get; set; }
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string ISBN { get; set; } = null!;
        public string? CoverImageUrl { get; set; }
        public string Category { get; set; } = null!;
        public int BorrowingsCount { get; set; }
        public int AvailableCopies { get; set; }
        public int TotalCopies { get; set; }
    }
}

