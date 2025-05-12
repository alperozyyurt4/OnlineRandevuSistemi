

/* Redis 

using OnlineRandevuSistemi.Business.DTOs;
using OnlineRandevuSistemi.Business.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace OnlineRandevuSistemi.Business.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _redisDb;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redisDb = redis.GetDatabase();
        }

        public async Task<IEnumerable<ServiceDto>> GetOrSetServicesAsync(Func<Task<IEnumerable<ServiceDto>>> factory)
        {
            var key = "services-all";
            try
            {
                var cached = await _redisDb.StringGetAsync(key);
                if (cached.HasValue)
                    return JsonSerializer.Deserialize<IEnumerable<ServiceDto>>(cached);

                var data = await factory();
                var json = JsonSerializer.Serialize(data);
                await _redisDb.StringSetAsync(key, json, TimeSpan.FromMinutes(30));
                return data;
            }
            catch
            {
                return await factory(); // Redis yoksa doğrudan getir
            }
        }

        public async Task<IEnumerable<CustomerDto>> GetOrSetCustomersAsync(Func<Task<IEnumerable<CustomerDto>>> factory)
        {
            var key = "customers-all";
            try
            {
                var cached = await _redisDb.StringGetAsync(key);
                if (cached.HasValue)
                    return JsonSerializer.Deserialize<IEnumerable<CustomerDto>>(cached);

                var data = await factory();
                var json = JsonSerializer.Serialize(data);
                await _redisDb.StringSetAsync(key, json, TimeSpan.FromMinutes(30));
                return data;
            }
            catch
            {
                return await factory();
            }
        }

        public async Task<IEnumerable<EmployeeDto>> GetOrSetEmployeesAsync(Func<Task<IEnumerable<EmployeeDto>>> factory)
        {
            var key = "employees-all";
            try
            {
                var cached = await _redisDb.StringGetAsync(key);
                if (cached.HasValue)
                    return JsonSerializer.Deserialize<IEnumerable<EmployeeDto>>(cached);

                var data = await factory();
                var json = JsonSerializer.Serialize(data);
                await _redisDb.StringSetAsync(key, json, TimeSpan.FromMinutes(30));
                return data;
            }
            catch
            {
                return await factory();
            }
        }

        public async Task<IEnumerable<AppointmentDto>> GetOrSetAppointmentsAsync(Func<Task<IEnumerable<AppointmentDto>>> factory)
        {
            var key = "appointments-all";
            try
            {
                var cached = await _redisDb.StringGetAsync(key);
                if (cached.HasValue)
                    return JsonSerializer.Deserialize<IEnumerable<AppointmentDto>>(cached);

                var data = await factory();
                var json = JsonSerializer.Serialize(data);
                await _redisDb.StringSetAsync(key, json, TimeSpan.FromMinutes(30));
                return data;
            }
            catch
            {
                return await factory();
            }
        }

        public async Task ClearCacheAsync(string key)
        {
            try
            {
                await _redisDb.KeyDeleteAsync(key);
            }
            catch
            {
                // Redis yoksa hata verme, sessiz geç
            }
        }
    }
}

*/