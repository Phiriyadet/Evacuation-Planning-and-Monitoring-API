using Evacuation_Planning_and_Monitoring_API.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Evacuation_Planning_and_Monitoring_API.Repositories
{
    public class RedisRepository : IRedisRepository
    {
        private IDistributedCache _cache;
        private const string statusCacheKey = "Evacuation:Status:"; 
        private const string planCacheKey = "Evacuation:Plan:";
        public RedisRepository(IDistributedCache cache)
        {
            _cache = cache;
        }

        // Implement the methods from IRedisRepository here
        public async Task SetEvacuationStatusCache(string zoneId, string statusJson)
        {
            await _cache.SetStringAsync($"{statusCacheKey}{zoneId}", statusJson);
        }
        public async Task<string?> GetEvacuationStatusCache(string zoneId)
        {
            return await _cache.GetStringAsync($"{statusCacheKey}{zoneId}");
            
        }
        public async Task ClearEvacuationStatusCache(string zoneId)
        {
           await _cache.RemoveAsync($"{statusCacheKey}{zoneId}");
        }

        public async Task SetEvacuationPlansCache(string vehicleId, string plansJson)
        {
            await _cache.SetStringAsync($"{planCacheKey}{vehicleId}", plansJson);
        }

        public async Task<string?> GetEvacuationPlansCache(string vehicleId)
        {
            return await _cache.GetStringAsync($"{planCacheKey}{vehicleId}");
        }

        public async Task ClearEvacuationPlansCache(string vehicleId)
        {
            await _cache.RemoveAsync($"{planCacheKey}{vehicleId}");
        }
    }
    
    }

