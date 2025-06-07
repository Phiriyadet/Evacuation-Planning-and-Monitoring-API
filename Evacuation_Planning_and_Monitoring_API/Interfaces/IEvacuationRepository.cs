using Evacuation_Planning_and_Monitoring_API.Models;

namespace Evacuation_Planning_and_Monitoring_API.Interfaces
{
    public interface IEvacuationRepository
    {
        Task<IEnumerable<EvacuationPlan>> EvacationPlanAsync(double distanceKm);
        Task<IEnumerable<EvacuationStatus>> EvacuationStatusAsync();
        Task<IEnumerable<EvacuationStatus>> EvacuationUpdateAsync();
        Task EvacuationClearAsync();
    }
}
