using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Business.DTOs
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Position { get; set; }
        public string Biography { get; set; }
        public decimal HourlyDate { get; set; }
        public string Address { get; set; }
        public DateTime? DateOfBirth{ get; set; }
        public string? ProfilePicture { get; set; }
        public bool IsAvailable { get; set; }
        public List<int> ServiceIds { get; set; } = new List<int>();

    }
}
