using Evacuation_Planning_and_Monitoring_API.Data;
using Evacuation_Planning_and_Monitoring_API.Helpers;
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
            var sortedZones = zones.OrderByDescending(z => z.UrgencyLevel).ToList(); //เรียงลำดับโซนตามระดับความเร่งด่วนมากไปน้อย
            var sortedVehicles = vehicles.OrderBy(v => v.Capacity).ToList(); //เรียงลำดับรถตามความจุจากน้อยไปมาก

            //ส่งรถไปยังโซนที่มีความเร่งด่วนสูงสุดก่อน และใช้รถที่มีความจุน้อยที่สุดก่อน
            foreach (var zone in sortedZones)
            {
                
                    Console.WriteLine($"Planning evacuation for zone {zone.ZoneID} with urgency level {zone.UrgencyLevel}");
                    int remainingPeople = zone.NumberOfPeople;
                foreach (var v in sortedVehicles)
                {
                    if (remainingPeople <= 0)
                    {
                        Console.WriteLine($"All people evacuated from zone {zone.ZoneID}");
                        break;
                    }
                    if (v.Capacity <= remainingPeople) // ตรวจสอบว่ารถมีความจุเพียงพอสำหรับคนที่เหลืออยู่ในโซนหรือไม่
                    {
                        Console.WriteLine($"Assigning vehicle {v.VehicleID} with capacity {v.Capacity} to zone {zone.ZoneID}");

                        var distance = CalculationHelper.CalculateDistance(
                            zone.LocationCoordinates.Latitude, zone.LocationCoordinates.Longitude, v.LocationCoordinates.Latitude, v.LocationCoordinates.Longitude);
                        var eta = CalculationHelper.CalculateETA(distance, v.Speed);

                        remainingPeople -= v.Capacity;
                        if (remainingPeople < 0)
                        {
                            remainingPeople = 0; // Ensure we don't go below zero
                        }
                        Console.WriteLine($"Vehicle {v.VehicleID} will take {eta} " +
                            $"to evacuate people from zone {zone.ZoneID}. Remaining people: {remainingPeople}");
                    }


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
