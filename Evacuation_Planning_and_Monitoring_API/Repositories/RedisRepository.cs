using Evacuation_Planning_and_Monitoring_API.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Evacuation_Planning_and_Monitoring_API.Repositories
{
    public class RedisRepository : IRedisRepository
    {
        private IDistributedCache _cache;
        private const string cacheKey = "Evacuation:Status:"; 
        public RedisRepository(IDistributedCache cache)
        {
            _cache = cache;
        }

        // Implement the methods from IRedisRepository here
        public async Task SetEvacuationStatusCache(string zoneId, string status)
        {
            string zoneCacheKey = $"{cacheKey}{zoneId}";
            await _cache.SetStringAsync(zoneCacheKey, status);
        }
        public async Task<string?> GetEvacuationStatusCache(string zoneId)
        {
            return await _cache.GetStringAsync($"{cacheKey}{zoneId}");
            
        }
        public async Task ClearEvacuationStatusCache(string zoneId)
        {
           await _cache.RemoveAsync($"{cacheKey}{zoneId}");
        }
    }
    
    }

