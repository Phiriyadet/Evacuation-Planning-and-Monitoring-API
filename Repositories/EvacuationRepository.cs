using Evacuation_Planning_and_Monitoring_API.Data;
using Evacuation_Planning_and_Monitoring_API.Interfaces;

namespace Evacuation_Planning_and_Monitoring_API.Repositories
{
    public class EvacuationRepository : IEvacuationRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly IEvacuationZoneRepository _zoneRepository;
        private readonly IVehicleRepository _vehicleRepository;
        public EvacuationRepository(ApplicationDBContext context, IEvacuationZoneRepository zoneRepository, IVehicleRepository vehicleRepository)
        {
            _context = context;
            _zoneRepository = zoneRepository;
            _vehicleRepository = vehicleRepository;
        }

        public async Task EvacationPlanAsync()
        {
            var zones = await _zoneRepository.GetAllEvacuationZonesAsync();
            var vehicles = await _vehicleRepository.GetAllVehiclesAsync();

            foreach (var zone in zones)
            {
                if (zone.UrgencyLevel == 5)
                {
                    Console.WriteLine($"Planning evacuation for zone {zone.ZoneID} with urgency level {zone.UrgencyLevel}");
                }
            }
        }

        public Task EvacuationClearAsync()
        {
            throw new NotImplementedException();
        }

        public Task EvacuationStatusAsync()
        {
            throw new NotImplementedException();
        }

        public Task EvacuationUpdateAsync()
        {
            throw new NotImplementedException();
        }
    }
}
