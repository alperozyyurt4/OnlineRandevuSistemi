using Microsoft.Extensions.Logging;
using OnlineRandevuSistemi.Business.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Business.Services
{
    public class DummySmsService : ISmsService
    {
        private readonly ILogger<DummySmsService> _logger;
        public DummySmsService(ILogger<DummySmsService> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string phoneNumber, string message)
        {
            _logger.LogInformation($"[SMS] To: {phoneNumber}, Message: {message}");
            return Task.CompletedTask;
        }
    }
}
