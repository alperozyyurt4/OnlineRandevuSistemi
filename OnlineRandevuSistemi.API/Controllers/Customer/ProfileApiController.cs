using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Entities;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.API.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/profile")]
    [Authorize(Roles = "Customer")]
    public class ProfileApiController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly UserManager<AppUser> _userManager;

        public ProfileApiController(ICustomerService customerService, UserManager<AppUser> userManager)
        {
            _customerService = customerService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _customerService.GetCustomerByUserIdAsync(userId);

            if (customer == null)
                return NotFound();

            var profile = new
            {
                customer.Id,
                customer.FirstName,
                customer.LastName,
                customer.Email,
                customer.PhoneNumber,
                customer.Address,
                customer.DateOfBirth,
                customer.Notes
            };

            return Ok(profile);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] CustomerProfileUpdateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var dto = new CustomerDto
            {
                Id = model.Id,
                UserId = userId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                DateOfBirth = model.DateOfBirth ?? default,
                Notes = model.Notes
            };

            await _customerService.UpdateCustomerAsync(dto);
            return Ok(new { message = "Profil güncellendi." });
        }
    }

    public class CustomerProfileUpdateDto
    {
        public int Id { get; set; }  // DTO içinde ID de taşınıyor
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Notes { get; set; }
    }
}