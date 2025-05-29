using Evacuation_Planning_and_Monitoring_API.Data;
using Evacuation_Planning_and_Monitoring_API.Helpers;
using Evacuation_Planning_and_Monitoring_API.Interfaces;
using Evacuation_Planning_and_Monitoring_API.Models;

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
            
            //ส่งรถไปยังโซนที่มีความเร่งด่วนสูงสุดก่อน และใช้รถที่มีความจุน้อยที่สุดก่อน  
            foreach (var zone in sortedZones)
            {
                var availableVehicles = new List<Vehicle>(vehicles);
                Console.WriteLine($"Planning evacuation for zone {zone.ZoneID} with urgency level {zone.UrgencyLevel} and {zone.NumberOfPeople} people.");
                int remainingPeople = zone.NumberOfPeople;
                 //เรียงลำดับรถตามความจุจากน้อยไปมากที่สามารถรับคนได้ในโซนนี้
                while (remainingPeople > 0)
                {

                    //หารถที่มีความจุที่สามารถรับคนได้ในโซนนี้
                    var v = availableVehicles.Where(v => v.Capacity <= remainingPeople).OrderByDescending(v => v.Capacity).FirstOrDefault();
                    if (v == null)
                    {
                        v = availableVehicles.Where(v => v.Capacity > remainingPeople).OrderBy(v => v.Capacity).FirstOrDefault(); //ใช้รถที่มีความจุมากที่สุดหากไม่มีรถที่สามารถรับคนได้ในโซนนี้
                        if (v == null)
                        {
                            Console.WriteLine($"No available vehicles for zone {zone.ZoneID}. Remaining people: {remainingPeople}");
                            break; //ไม่มีรถว่างให้ใช้
                        }
                    }
                    availableVehicles.Remove(v); //ลบรถที่ถูกใช้ไปแล้วออกจากรายการรถที่ว่างอยู่
                    
                    

                    Console.WriteLine($"Assigning vehicle {v.VehicleID} with capacity {v.Capacity} to zone {zone.ZoneID}");
                        int canTake = Math.Min(v.Capacity, remainingPeople); // จำนวนคนที่รถสามารถรับได้  
                        var distance = CalculationHelper.CalculateDistance(
                            zone.LocationCoordinates.Latitude, zone.LocationCoordinates.Longitude, v.LocationCoordinates.Latitude, v.LocationCoordinates.Longitude);
                        var eta = CalculationHelper.CalculateETA(distance, v.Speed);

                        remainingPeople -= canTake; // ลดจำนวนคนที่เหลืออยู่ในโซน
                        Console.WriteLine($"Vehicle {v.VehicleID} can take {canTake} people. Remaining people in zone {zone.ZoneID}: {remainingPeople}. ETA: {eta}");
                    if (remainingPeople <= 0)
                    {
                        Console.WriteLine($"All people in zone {zone.ZoneID} have been assigned to vehicles.");
                       
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
