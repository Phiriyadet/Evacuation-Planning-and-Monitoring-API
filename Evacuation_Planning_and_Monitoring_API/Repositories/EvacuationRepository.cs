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
        private readonly ILogger<EvacuationRepository> _logger;
        private readonly IDistributedCache _cache;
        private const string cacheKey = "Evacuation:Status:";
        public EvacuationRepository(ApplicationDBContext context, IEvacuationZoneRepository zoneRepository, IVehicleRepository vehicleRepository, ILogger<EvacuationRepository> logger, IDistributedCache cache)
        {
            _context = context;
            _zoneRepository = zoneRepository;
            _vehicleRepository = vehicleRepository;
            _logger = logger;
            _cache = cache;
        }

        public async Task<IEnumerable<EvacuationPlan>> EvacationPlanAsync()
        {
            var zones = await _zoneRepository.GetAllEvacuationZonesAsync();
            var vehicles = await _vehicleRepository.GetAllVehiclesAsync();
            var sortedZones = zones.OrderByDescending(z => z.UrgencyLevel).ToList(); //เรียงลำดับโซนตามระดับความเร่งด่วนมากไปน้อย  
            var evacuationPlans = new List<EvacuationPlan>();
            var availableVehicles = new List<Vehicle>(vehicles);
            var cacheStatusList = new List<EvacuationStatus>();
            int remainingPeople = 0;

           
            //ส่งรถไปยังโซนที่มีความเร่งด่วนสูงสุดก่อน และใช้รถที่มีความจุน้อยที่สุดก่อน  
            foreach (var zone in sortedZones)
            {
                string zoneCacheKey = $"{cacheKey}{zone.ZoneID}";
                Console.WriteLine($"Planning evacuation for zone {zone.ZoneID} with urgency level {zone.UrgencyLevel} and {zone.NumberOfPeople} people.");
               var statusJson = await _cache.GetStringAsync(zoneCacheKey);
                if (!string.IsNullOrEmpty(statusJson))
                {
                    var cachedStatus = JsonSerializer.Deserialize<EvacuationStatus>(statusJson);
                    if (cachedStatus != null)
                    {
                        remainingPeople = cachedStatus.RemainingPeople;
                        Console.WriteLine($"Retrieved cached evacuation status for zone {zone.ZoneID}. Remaining people: {remainingPeople}");
                    }
                }
                else
                {
                    remainingPeople = zone.NumberOfPeople; //ใช้จำนวนคนที่เหลืออยู่ในโซน
                    Console.WriteLine($"No cached status found for zone {zone.ZoneID}. Using initial number of people: {remainingPeople}");
                }

                


                //หารถที่มีความจุที่สามารถรับคนได้ในโซนนี้
                var v = availableVehicles.Where(v => v.Capacity <= remainingPeople).OrderByDescending(v => v.Capacity).FirstOrDefault();
                if (v == null)
                {
                    v = availableVehicles.Where(v => v.Capacity > remainingPeople).OrderBy(v => v.Capacity).FirstOrDefault(); //ใช้รถที่มีความจุมากที่สุดหากไม่มีรถที่สามารถรับคนได้ในโซนนี้
                    if (v == null)
                    {
                        Console.WriteLine($"No available vehicles for zone {zone.ZoneID}. Remaining people: {remainingPeople}");
                        continue; //หากไม่มีรถที่สามารถรับคนได้ในโซนนี้ ให้ข้ามไปยังโซนถัดไป
                    }
                }
                availableVehicles.Remove(v); //ลบรถที่ถูกใช้ไปแล้วออกจากรายการรถที่ว่างอยู่

                Console.WriteLine($"Assigning vehicle {v.VehicleID} with capacity {v.Capacity} to zone {zone.ZoneID}");
                int canTake = Math.Min(v.Capacity, remainingPeople); // จำนวนคนที่รถสามารถรับได้  
                var distance = CalculationHelper.CalculateDistance(
                    zone.LocationCoordinates.Latitude, zone.LocationCoordinates.Longitude, v.LocationCoordinates.Latitude, v.LocationCoordinates.Longitude);
                var eta = CalculationHelper.CalculateETA(distance, v.Speed);

                remainingPeople -= canTake; // ลดจำนวนคนที่เหลืออยู่ในโซน
                //zone.NumberOfPeople = remainingPeople; // อัพเดตจำนวนคนที่เหลืออยู่ในโซน
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
            var zones = await _zoneRepository.GetAllEvacuationZonesAsync();
            var evacuationStatusList = new List<EvacuationStatus>();
            foreach (var zone in zones)
            {
                string zoneCacheKey = $"{cacheKey}{zone.ZoneID}";
                var cachedStatusJson = await _cache.GetStringAsync(zoneCacheKey);
                if (!string.IsNullOrEmpty(cachedStatusJson))
                {
                    var status = JsonSerializer.Deserialize<EvacuationStatus>(cachedStatusJson);
                    if (status != null)
                    {
                        evacuationStatusList.Add(status);
                        Console.WriteLine($"Retrieved cached evacuation status for zone {zone.ZoneID}.");
                    }
                }
                else
                {
                    var evaStatus = await _context.EvacuationStatuses.FindAsync(zone.ZoneID);
                    if (evaStatus != null)
                    {
                        var statusJson = JsonSerializer.Serialize(evaStatus);

                        await _cache.SetStringAsync(zoneCacheKey, statusJson);
                        evacuationStatusList.Add(evaStatus);
                        Console.WriteLine($"No cached status found for zone {zone.ZoneID}. Fetched from database and cached it.");
                    }
                    else
                    {
                        Console.WriteLine($"No evacuation status found for zone {zone.ZoneID} in the database.");
                    }
                }
            }return evacuationStatusList; // ส่งคืนสถานะการอพยพที่ถูกสร้างขึ้น
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
                    string zoneCacheKey = $"{cacheKey}{zone.ZoneID}";
                    var status = JsonSerializer.Serialize(evacuationStatus);

                    await _cache.SetStringAsync(zoneCacheKey, status);

                 
                    evacuationStatusList.Add(evacuationStatus);


                }
                else
                {
                    Console.WriteLine($"No evacuation plans for zone {zone.ZoneID}.");
                }
                

            }
            return evacuationStatusList; // ส่งคืนสถานะการอพยพที่ถูกสร้างขึ้น

        }
    }
}
