using System.ComponentModel.DataAnnotations;

namespace BibliotekaInternetowa.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tytuł jest wymagany")]
        [Display(Name = "Tytuł")]
        public string Title { get; set; } = null!;

        [Required] public string Author { get; set; } = null!;
        [Required] public string ISBN { get; set; } = null!;
        public int PublicationYear { get; set; }
        [Required] public string Category { get; set; } = null!;
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }

        public int TotalCopies { get; set; } = 1;
        public int AvailableCopies { get; set; } = 1;
    }
}