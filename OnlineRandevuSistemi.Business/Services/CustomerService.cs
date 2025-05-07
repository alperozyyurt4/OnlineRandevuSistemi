using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Business.Interfaces;
using OnlineRandevuSistemi.Core.Entities;
using OnlineRandevuSistemi.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Business.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IRepository<Customer> _customerRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CustomerService(
            IRepository<Customer> customerRepository,
            UserManager<AppUser> userManager,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _customerRepository = customerRepository;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            var customers = await _customerRepository.TableNoTracking
                .Where(c => !c.IsDeleted)
                .Include(c => c.User)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CustomerDto>>(customers);
        }

        public async Task<CustomerDto> GetCustomerByIdAsync(int id)
        {
            var customer = await _customerRepository.TableNoTracking
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            return _mapper.Map<CustomerDto>(customer);
        }

        public async Task<CustomerDto> GetCustomerByUserIdAsync(string userId)
        {
            var customer = await _customerRepository.TableNoTracking
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return _mapper.Map<CustomerDto>(customer);
        }

        public async Task<CustomerDto> CreateCustomerAsync(CustomerDto customerDto)
        {
            var customer = _mapper.Map<Customer>(customerDto);
            await _customerRepository.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<CustomerDto>(customer);
        }
        public async Task<CustomerDto> UpdateCustomerAsync(CustomerDto customerDto)
        {
            // Müşteriyi al
            var customer = await _customerRepository.GetByIdAsync(customerDto.Id);
            if (customer == null)
                throw new Exception("Customer not found");

            // Kullanıcıyı al
            var user = await _userManager.FindByIdAsync(customer.UserId);
            if (user == null)
                throw new Exception("Associated user not found");

            // AppUser (kullanıcı) bilgilerini güncelle
            user.FirstName = customerDto.FirstName;
            user.LastName = customerDto.LastName;
            user.Email = customerDto.Email;
            user.PhoneNumber = customerDto.PhoneNumber;
            user.UpdatedDate = DateTime.Now;
            user.Address = customerDto.Address;
            user.DateOfBirth = customerDto.DateOfBirth;

            var userUpdateResult = await _userManager.UpdateAsync(user);
            if (!userUpdateResult.Succeeded)
            {
                throw new Exception("User update failed: " + string.Join(", ", userUpdateResult.Errors.Select(e => e.Description)));
            }

            // Customer tablosunu güncelle
            customer.notes = customerDto.Notes;
            customer.UpdatedDate = DateTime.Now;

            await _customerRepository.UpdateAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CustomerDto>(customer);
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
                return false;

            await _customerRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
