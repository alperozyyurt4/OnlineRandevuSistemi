using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace OnlineRandevuSistemi.Api.ViewModels
{
    public class EmployeeEditViewModel
    {
        public int Id { get; set; }
      

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        public string Address { get; set; }
        public string Biography { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public string? ProfilePicture { get; set; }

        [Display(Name = "Hizmetler")]
        public List<int> SelectedServiceIds { get; set; } = new();
        public List<SelectListItem> Services{ get; set; } = new();

    }
}
