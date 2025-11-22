namespace BibliotekaInternetowa.Models
{
    public class BookSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public string? Category { get; set; }
        public string? Availability { get; set; } // "all", "available", "unavailable"
        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }
        public List<Book> Books { get; set; } = new List<Book>();
        public List<string> Categories { get; set; } = new List<string>();
    }
}

