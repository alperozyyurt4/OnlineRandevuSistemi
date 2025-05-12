using Microsoft.Extensions.Logging;
using OnlineRandevuSistemi.Business.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Business.Services
{
    public class DummyEmailService : IEmailService
    {
        private readonly ILogger<DummyEmailService> _logger;
        public DummyEmailService(ILogger<DummyEmailService> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string toEmail, string subject, string message)
        {
            _logger.LogInformation($"[EMAIL] To: {toEmail}, Subject: {subject}, Body: {message}");
            return Task.CompletedTask;
        }
    }
}
