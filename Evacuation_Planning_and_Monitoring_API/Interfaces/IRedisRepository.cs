namespace Evacuation_Planning_and_Monitoring_API.Interfaces
{
    public interface IRedisRepository
    {
        Task SetEvacuationStatusCache(string zoneId, string statusJson);
        Task<string?> GetEvacuationStatusCache(string zoneId);
        Task ClearEvacuationStatusCache(string zoneId);
        Task SetEvacuationPlansCache(string vehicleId, string plansJson);
        Task<string?> GetEvacuationPlansCache(string vehicleId);
        Task ClearEvacuationPlansCache(string vehicleId);
    }
}
