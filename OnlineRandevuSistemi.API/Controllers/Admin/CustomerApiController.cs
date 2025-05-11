using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Entities;

namespace OnlineRandevuSistemi.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/customers")]
    [Authorize(Roles = "Admin")]
    public class CustomerApiController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly UserManager<AppUser> _userManager;

        public CustomerApiController(ICustomerService customerService, UserManager<AppUser> userManager)
        {
            _customerService = customerService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
                return NotFound();

            return Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomerCreateDto model)
        {
            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                DateOfBirth = model.DateOfBirth ?? DateTime.Now,
                ProfilePicture = model.ProfilePicture ?? "/images/default-profile.jpg"
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "Customer");

            await _customerService.CreateCustomerAsync(new CustomerDto
            {
                UserId = user.Id,
                PreferredLanguage = "TR",
                Notes = "API üzerinden oluşturulan kayıt"
            });

            return Ok(new { message = "Müşteri başarıyla oluşturuldu." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CustomerDto model)
        {
            if (id != model.Id)
                return BadRequest("ID uyuşmuyor.");

            await _customerService.UpdateCustomerAsync(model);
            return Ok(new { message = "Müşteri güncellendi." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _customerService.DeleteCustomerAsync(id);
            return Ok(new { message = "Müşteri silindi." });
        }
    }

    // API'ye özel DTO
    public class CustomerCreateDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }  // API tarafında sadece ilk kayıtta kullanılır
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ProfilePicture { get; set; }
    }
}