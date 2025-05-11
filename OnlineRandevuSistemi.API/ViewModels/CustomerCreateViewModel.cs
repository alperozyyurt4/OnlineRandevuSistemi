using System.ComponentModel.DataAnnotations;

namespace OnlineRandevuSistemi.Api.ViewModels
{
    public class CustomerCreateViewModel
    {
        [Required]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Telefon")]
        public string PhoneNumber { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Parola")]
        public string Password { get; set; }

        [Display(Name = "Adres")]
        public string Address { get; set; }

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public string? ProfilePicture { get; set; }
    }
}