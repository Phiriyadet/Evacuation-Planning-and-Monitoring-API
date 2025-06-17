using Evacuation_Planning_and_Monitoring_API.Models;

namespace Evacuation_Planning_and_Monitoring_API.Interfaces
{
    public interface IEvacuationZoneRepository
    {
        Task<EvacuationZone?> GetEvacuationZoneByIdAsync(string id);
        Task<IEnumerable<EvacuationZone>> GetAllEvacuationZonesAsync();
        Task<IEnumerable<string>> GetAllZoneIDAsync();
        Task<EvacuationZone> AddEvacuationZoneAsync(EvacuationZone zone);
        Task<EvacuationZone?> UpdateEvacuationZoneAsync(EvacuationZone zone);
        Task<EvacuationZone?> DeleteEvacuationZoneAsync(string id);

    }
}
