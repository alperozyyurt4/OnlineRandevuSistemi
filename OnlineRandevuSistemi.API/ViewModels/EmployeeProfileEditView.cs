using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineRandevuSistemi.Api.ViewModels
{
    public class EmployeeProfileEditViewModel
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        [Required]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Telefon")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Adres")]
        public string Address { get; set; }

        [Display(Name = "Doğum Tarihi")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Pozisyon")]
        public string Position { get; set; }

        [Display(Name = "Biyografi")]
        public string Biography { get; set; }

        [Display(Name = "Saatlik Ücret")]
        public decimal HourlyDate { get; set; }
    }
}