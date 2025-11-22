using System.ComponentModel.DataAnnotations;

namespace BibliotekaInternetowa.Models
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = null!;

        [Required(ErrorMessage = "Nazwa użytkownika jest wymagana")]
        [Display(Name = "Nazwa użytkownika")]
        public string UserName { get; set; } = null!;

        [EmailAddress(ErrorMessage = "Nieprawidłowy adres email")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Imię i nazwisko")]
        public string? FullName { get; set; }

        [Display(Name = "Role")]
        public List<string> SelectedRoles { get; set; } = new();

        public List<string> AvailableRoles { get; set; } = new();
    }
}

