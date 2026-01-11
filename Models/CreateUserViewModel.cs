using System.ComponentModel.DataAnnotations;

namespace BibliotekaInternetowa.Models
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Nazwa użytkownika jest wymagana")]
        [Display(Name = "Nazwa użytkownika")]
        public string UserName { get; set; } = null!;

        [EmailAddress(ErrorMessage = "Nieprawidłowy adres email")]
        [Required(ErrorMessage = "Email jest wymagany")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Display(Name = "Imię i nazwisko")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Hasło jest wymagane")]
        [StringLength(100, ErrorMessage = "Hasło musi mieć co najmniej {2} znaków.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Hasło")]
        public string Password { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Potwierdź hasło")]
        [Compare("Password", ErrorMessage = "Hasła nie są zgodne.")]
        public string ConfirmPassword { get; set; } = null!;

        [Display(Name = "Role")]
        public List<string> SelectedRoles { get; set; } = new();

        public List<string> AvailableRoles { get; set; } = new();
    }
}
