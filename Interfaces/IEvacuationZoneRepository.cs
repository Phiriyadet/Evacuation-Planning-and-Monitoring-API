using Evacuation_Planning_and_Monitoring_API.Models;

namespace Evacuation_Planning_and_Monitoring_API.Interfaces
{
    public interface IEvacuationZoneRepository
    {
        Task<EvacuationZone?> GetEvacuationZoneByIdAsync(string id);
        Task<IEnumerable<EvacuationZone>> GetAllEvacuationZonesAsync();
        Task<EvacuationZone> AddEvacuationZoneAsync(EvacuationZone evacuationZone);
        Task<EvacuationZone?> UpdateEvacuationZoneAsync(EvacuationZone evacuationZone);
        Task<EvacuationZone?> DeleteEvacuationZoneAsync(string id);

    }
}
