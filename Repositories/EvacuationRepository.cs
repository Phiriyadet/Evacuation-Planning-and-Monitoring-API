using Evacuation_Planning_and_Monitoring_API.Data;
using Evacuation_Planning_and_Monitoring_API.Helpers;
using Evacuation_Planning_and_Monitoring_API.Interfaces;
using Evacuation_Planning_and_Monitoring_API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Text.Json;

namespace Evacuation_Planning_and_Monitoring_API.Repositories
{
    public class EvacuationRepository : IEvacuationRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly IEvacuationZoneRepository _zoneRepository;
        private readonly IVehicleRepository _vehicleRepository;
        //private readonly ILogger<EvacuationRepository> _logger;
        private readonly IDistributedCache _cache;
        private const string cacheKey = "EvacuationStatus";
        public EvacuationRepository(ApplicationDBContext context, IEvacuationZoneRepository zoneRepository, IVehicleRepository vehicleRepository, IDistributedCache cache)
        {
            _context = context;
            _zoneRepository = zoneRepository;
            _vehicleRepository = vehicleRepository;
            _cache = cache;
        }

        public async Task<IEnumerable<EvacuationPlan>> EvacationPlanAsync()
        {
            var zones = await _zoneRepository.GetAllEvacuationZonesAsync();
            var vehicles = await _vehicleRepository.GetAllVehiclesAsync();
            var sortedZones = zones.OrderByDescending(z => z.UrgencyLevel).ToList(); //เรียงลำดับโซนตามระดับความเร่งด่วนมากไปน้อย  
            var evacuationPlans = new List<EvacuationPlan>();

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
                    var evacuationPlan = new EvacuationPlan
                    {
                        ZoneID = zone.ZoneID,
                        VehicleID = v.VehicleID,
                        NumberOfPeople = canTake,
                        ETA = eta
                    };

                    evacuationPlans.Add(evacuationPlan); //เพิ่มแผนการอพยพลงในรายการแผนการอพยพ
                    
                    if (remainingPeople <= 0)
                    {
                        Console.WriteLine($"All people in zone {zone.ZoneID} have been assigned to vehicles.");
                       
                    }

                }
            }
            await _context.EvacuationPlans.AddRangeAsync(evacuationPlans);
            await _context.SaveChangesAsync(); // บันทึกแผนการอพยพลงฐานข้อมูล
            return evacuationPlans; // ส่งคืนแผนการอพยพที่ถูกสร้างขึ้น
        }

        public async Task EvacuationClearAsync()
        {
            
            await _cache.RemoveAsync(cacheKey); // Clear the evacuation status cache
            var deleteP = await _context.EvacuationPlans.ExecuteDeleteAsync(); // Clear all evacuation plans from the database
            var deleteS = await _context.EvacuationStatuses.ExecuteDeleteAsync(); // Clear all evacuation statuses from the database
            await _context.SaveChangesAsync();
            return;

        }

        public async Task<IEnumerable<EvacuationStatus>> EvacuationStatusAsync()
        {
            var cachedStatus = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedStatus))
            {
                Console.WriteLine("Returning cached evacuation status.");
                return JsonSerializer.Deserialize<IEnumerable<EvacuationStatus>>(cachedStatus) ?? new List<EvacuationStatus>();
            }
            else { 
                var evaStatus = await _context.EvacuationStatuses.ToListAsync();
                var status = JsonSerializer.Serialize(evaStatus);

                await _cache.SetStringAsync(cacheKey, status);
                return evaStatus;

            }
        

        }

        public async Task<IEnumerable<EvacuationStatus>> EvacuationUpdateAsync()
        {
            var evacuationPlans = await _context.EvacuationPlans.ToListAsync();
            var zones = await _zoneRepository.GetAllEvacuationZonesAsync();
            var vehicles = await _vehicleRepository.GetAllVehiclesAsync();
            
            var evacuationStatusList = new List<EvacuationStatus>();
            foreach (var zone in zones)
            {
                var zonePlans = evacuationPlans.Where(ep => ep.ZoneID == zone.ZoneID).ToList();
                if (zonePlans.Any())
                {
                    Console.WriteLine($"Evacuation plans for zone {zone.ZoneID}:");

                    int totalPeopleEvacuated = zonePlans.Sum(ep => ep.NumberOfPeople);
                    int remainingPeople = Math.Max(0, zone.NumberOfPeople - totalPeopleEvacuated);
                    var lastPlan = zonePlans.LastOrDefault();
                    string lastVehicleID = lastPlan?.VehicleID ?? "N/A";

                    var evacuationStatus = new EvacuationStatus
                    {
                        ZoneID = zone.ZoneID,
                        TotalEvacuated = totalPeopleEvacuated,
                        RemainingPeople = remainingPeople,
                        LastVehicleIDUsed = lastVehicleID

                    };
                    evacuationStatusList.Add(evacuationStatus);

                }
                else
                {
                    Console.WriteLine($"No evacuation plans for zone {zone.ZoneID}.");
                }
                

            }
            var status = JsonSerializer.Serialize(evacuationStatusList);

            await _cache.SetStringAsync(cacheKey, status);
            return evacuationStatusList; // ส่งคืนสถานะการอพยพที่ถูกสร้างขึ้น

        }
    }
}
