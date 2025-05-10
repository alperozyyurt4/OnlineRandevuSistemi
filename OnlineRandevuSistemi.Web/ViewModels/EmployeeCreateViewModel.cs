using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OnlineRandevuSistemi.Web.ViewModels
{
    public class EmployeeCreateViewModel
    {
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

        [DataType(DataType.Password)]
        [Display(Name = "Parola")]
        public string? Password { get; set; }

        [Display(Name = "Profil Resmi (URL)")]
        public string? ProfilePicture { get; set; }

        [Display(Name = "Adres")]
        public string Address { get; set; }



        [Display(Name ="Biyografi")]
        public string? Biography { get; set; }




        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Hizmetler")]
        public List<int> SelectedServiceIds { get; set; } = new();
        public List<SelectListItem> Services { get; set; } = new();
    }
}