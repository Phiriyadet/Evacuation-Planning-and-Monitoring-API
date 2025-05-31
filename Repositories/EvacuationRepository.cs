using Evacuation_Planning_and_Monitoring_API.Data;
using Evacuation_Planning_and_Monitoring_API.Helpers;
using Evacuation_Planning_and_Monitoring_API.Interfaces;
using Evacuation_Planning_and_Monitoring_API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
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
        public EvacuationRepository(ApplicationDBContext context, IEvacuationZoneRepository zoneRepository, IVehicleRepository vehicleRepository, IDistributedCache cache)
        {
            _context = context;
            _zoneRepository = zoneRepository;
            _vehicleRepository = vehicleRepository;
            _cache = cache;
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
                    var evacuationPlan = new EvacuationPlan
                    {
                        ZoneID = zone.ZoneID,
                        VehicleID = v.VehicleID,
                        NumberOfPeople = canTake,
                        ETA = eta
                    };

                    await _context.EvacuationPlans.AddAsync(evacuationPlan);
                    await _context.SaveChangesAsync(); // บันทึกแผนการอพยพลงฐานข้อมูล
                    if (remainingPeople <= 0)
                    {
                        Console.WriteLine($"All people in zone {zone.ZoneID} have been assigned to vehicles.");
                       
                    }

                }
            }
        }

        public async Task EvacuationClearAsync()
        {
           await _cache.RemoveAsync("EvacuationStatus"); // Clear the evacuation status cache
        }

        public async Task EvacuationStatusAsync()
        {
            var statuses = await _cache.GetAsync("EvacuationStatus"); // This is just a placeholder to show how to use the cache
            if (statuses != null)
            {
                var evacuationStatus = JsonSerializer.Deserialize<List<EvacuationStatus>>(statuses);
                if (evacuationStatus != null && evacuationStatus.Any())
                {
                    foreach (var status in evacuationStatus)
                    {
                        Console.WriteLine($"Zone: {status.ZoneID}, Total Evacuated: {status.TotalEvacuated}, Remaining People: {status.RemainingPeople}, Last Vehicle Used: {status.LastVehicleUsed?.VehicleID}");
                    }
                }
                else
                {
                    Console.WriteLine("No evacuation status found in cache.");
                }
            }
            else
            {
                Console.WriteLine("No evacuation status found in cache.");
            }
        }

        public async Task EvacuationUpdateAsync()
        {
            var evacuationPlans = await _context.EvacuationPlans.ToListAsync();
            var zones = await _zoneRepository.GetAllEvacuationZonesAsync();
            var vehicles = await _vehicleRepository.GetAllVehiclesAsync();
            if (evacuationPlans == null || !evacuationPlans.Any())
            {
                Console.WriteLine("No evacuation plans found.");
                return;
            }
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
                    var lastVehicle = vehicles.FirstOrDefault(v => v.VehicleID == lastPlan?.VehicleID);

                    var evacuationStatus = new EvacuationStatus
                    {
                        ZoneID = zone.ZoneID,
                        TotalEvacuated = totalPeopleEvacuated,
                        RemainingPeople = remainingPeople,
                        LastVehicleUsed = lastVehicle

                    };
                    evacuationStatusList.Add(evacuationStatus);

                }
                else
                {
                    Console.WriteLine($"No evacuation plans for zone {zone.ZoneID}.");
                }
                var status = JsonSerializer.Serialize(evacuationStatusList);

                await _cache.SetStringAsync("EvacuationStatus", status);
            }
        }
    }
}
