using System.ComponentModel.DataAnnotations;

namespace OnlineRandevuSistemi.Web.ViewModels
{
    public class CustomerEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "E-Posta")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Telefon")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Adres")]
        public string Address { get; set; }

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Notlar")]
        public string Notes { get; set; }
    }
}