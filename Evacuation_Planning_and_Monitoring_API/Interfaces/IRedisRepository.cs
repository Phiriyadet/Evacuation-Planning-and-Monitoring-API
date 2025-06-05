namespace Evacuation_Planning_and_Monitoring_API.Interfaces
{
    public interface IRedisRepository
    {
        Task SetEvacuationStatusCache(string zoneId, string status);
        Task<string?> GetEvacuationStatusCache(string zoneId);
        Task ClearEvacuationStatusCache(string zoneId);
    }
}
