﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Business.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string message);
    }
}
