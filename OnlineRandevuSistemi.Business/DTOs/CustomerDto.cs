﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Business.DTOs
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PreferredLanguage { get; set; }
        public string Notes { get; set; }
        public string Address { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}
