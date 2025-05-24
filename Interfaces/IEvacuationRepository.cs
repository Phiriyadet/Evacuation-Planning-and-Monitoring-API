using Evacuation_Planning_and_Monitoring_API.Models;

namespace Evacuation_Planning_and_Monitoring_API.Interfaces
{
    public interface IEvacuationRepository
    {
        Task EvacationPlanAsync();
        Task EvacuationStatusAsync();
        Task EvacuationUpdateAsync();
        Task EvacuationClearAsync();
    }
}
