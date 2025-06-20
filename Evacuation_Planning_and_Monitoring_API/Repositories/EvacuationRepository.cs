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
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1); // ใช้ SemaphoreSlim เพื่อจัดการการเข้าถึงพร้อมกัน
        private readonly ApplicationDBContext _context;
        private readonly IEvacuationZoneRepository _zoneRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ILogger<EvacuationRepository> _logger;
        private readonly IRedisRepository _cache;

        public EvacuationRepository(ApplicationDBContext context, IEvacuationZoneRepository zoneRepository, IVehicleRepository vehicleRepository, ILogger<EvacuationRepository> logger, IRedisRepository cache)
        {
            _context = context;
            _zoneRepository = zoneRepository;
            _vehicleRepository = vehicleRepository;
            _logger = logger;
            _cache = cache;
        }

        public async Task<IEnumerable<EvacuationPlan>> EvacationPlanAsync(double distanceKm)
        {
            await _semaphore.WaitAsync(); // รอจนกว่าจะสามารถเข้าถึงได้
            try
            {

                var zones = await _zoneRepository.GetAllEvacuationZonesAsync();
                var vehicles = await _vehicleRepository.GetAllVehiclesAsync();
                var sortedZones = zones.OrderByDescending(z => z.UrgencyLevel).ThenByDescending(z => z.NumberOfPeople).ToList(); //เรียงลำดับโซนตามระดับความเร่งด่วนมากไปน้อย  
                var evacuationPlanList = new List<EvacuationPlan>();
                var vehiclesList = new List<Vehicle>(vehicles);

                //await _cache.ClearEvacuationPlansCache(); // ลบแคชสำหรับแผนการอพยพก่อนเริ่มใหม่

                //ส่งรถไปยังโซนที่มีความเร่งด่วนสูงสุดก่อน และใช้รถที่มีความจุน้อยที่สุดก่อน  
                foreach (var zone in sortedZones)
                {
                    int remainingPeople = 0;

                    _logger.LogInformation($"Planning evacuation for zone {zone.ZoneID} with urgency level {zone.UrgencyLevel} and {zone.NumberOfPeople} people.");
                    remainingPeople = await GetRemainingPeopleFromCacheOrZone(zone); //ดึงจำนวนคนที่เหลืออยู่ในโซนจากแคชหรือฐานข้อมูล

                    while (remainingPeople > 0 && vehiclesList.Count > 0)
                    {
                        _logger.LogInformation($"Remaining people in zone {zone.ZoneID}: {remainingPeople}");

                        //หารถที่มีความจุที่สามารถรับคนได้ในโซนนี้
                        var vSuit = FindSuitableVehicle(zone, remainingPeople, vehiclesList, distanceKm);
                        if (vSuit == null)
                        {
                            _logger.LogWarning($"No suitable vehicle found for zone {zone.ZoneID} with remaining people {remainingPeople}. Skipping this zone.");
                            break;
                        }

                        vSuit.IsAvailable = false; // ทำเครื่องหมายว่ารถถูกใช้งานแล้ว
                        await _vehicleRepository.UpdateVehicleAsync(vSuit); // อัพเดทรถในฐานข้อมูล
                        vehiclesList.Remove(vSuit); //ลบรถที่ถูกใช้ไปแล้วออกจากรายการรถที่ว่างอยู่
                        _logger.LogInformation($"Vehicle {vSuit.VehicleID} is assigned to zone {zone.ZoneID} with capacity {vSuit.Capacity}.");

                        int canTake = Math.Min(vSuit.Capacity, remainingPeople); // จำนวนคนที่รถสามารถรับได้  
                        var evacuationPlan = CreateEvacuationPlan(zone, vSuit, canTake); // สร้างแผนการอพยพสำหรับโซนนี้และรถที่เลือก

                        var planJson = JsonSerializer.Serialize(evacuationPlan);
                        await _cache.SetEvacuationPlansCache(vSuit.VehicleID, planJson); // เก็บแผนการอพยพลงในแคช
                        await _context.EvacuationPlans.AddAsync(evacuationPlan);
                        await _context.SaveChangesAsync(); // บันทึกแผนการอพยพลงฐานข้อมูล

                        evacuationPlanList.Add(evacuationPlan); //เพิ่มแผนการอพยพลงในรายการแผนการอพยพ

                        remainingPeople -= canTake; // ลดจำนวนคนที่เหลืออยู่ในโซน
                        _logger.LogInformation($"Vehicle {vSuit.VehicleID} will evacuate {canTake} people from zone {zone.ZoneID}. Remaining people: {remainingPeople}.");

                        if (remainingPeople <= 0)
                        {
                            _logger.LogInformation($"All people in zone {zone.ZoneID} have been assigned to vehicles.");

                        }

                        
                    }
                }
              
                return evacuationPlanList; // ส่งคืนแผนการอพยพที่ถูกสร้างขึ้น
            }
            finally
            {
                _semaphore.Release();
            }
        }
        private async Task<int> GetRemainingPeopleFromCacheOrZone(EvacuationZone zone)
        {
            int remainingPeople = 0;

            var statusJson = await _cache.GetEvacuationStatusCache(zone.ZoneID);
            if (!string.IsNullOrEmpty(statusJson))
            {
                var cachedStatus = JsonSerializer.Deserialize<EvacuationStatus>(statusJson);
                if (cachedStatus != null)
                {
                    remainingPeople = cachedStatus.RemainingPeople;
                    _logger.LogInformation($"Retrieved cached evacuation status for zone {zone.ZoneID}. Remaining people: {remainingPeople}");
                }
            }
            else
            {
                var evaPlans = await _context.EvacuationPlans.ToListAsync();
                var totalEvacuated = evaPlans.Where(ep => ep.ZoneID == zone.ZoneID).Sum(ep => ep.NumberOfPeople);
                remainingPeople = zone.NumberOfPeople - totalEvacuated; //ใช้จำนวนคนที่เหลืออยู่ในโซน
                if (totalEvacuated > 0)
                {
                    
                    _logger.LogInformation($"No cached status found for zone {zone.ZoneID}. Total evacuated in plan: {totalEvacuated}, Remaining people: {remainingPeople}");
                }
                else
                {
                    
                    _logger.LogInformation($"No cached status found for zone {zone.ZoneID}. Using initial number of people: {remainingPeople}");
                }
            }
            return remainingPeople;
        }
        private EvacuationPlan CreateEvacuationPlan(EvacuationZone zone, Vehicle v, int canTake)
        {
            var distance = CalculationHelper.CalculateDistance(
                zone.LocationCoordinates.Latitude, zone.LocationCoordinates.Longitude, v.LocationCoordinates.Latitude, v.LocationCoordinates.Longitude);
            var eta = CalculationHelper.CalculateETA(distance, v.Speed);

            return new EvacuationPlan
            {
                ZoneID = zone.ZoneID,
                VehicleID = v.VehicleID,
                NumberOfPeople = canTake,
                ETA = eta
            };
        }
        private Vehicle? FindSuitableVehicle(EvacuationZone zone, int remainingPeople, List<Vehicle> vehicles, double distanceKm)
        {
            double maxDistance = distanceKm; // กำหนดระยะทางสูงสุดที่รถสามารถเดินทางได้ (กิโลเมตร) 
            // ค้นหารถที่มีความจุที่สามารถรับคนได้ในโซนนี้
            _logger.LogInformation($"Finding suitable vehicle for zone {zone.ZoneID} with remaining people {remainingPeople} and max distance {maxDistance} km.");
            // ค้นหารถที่ว่างและอยู่ในระยะทางที่กำหนด เรียงตามระยะใกล้
            var candidateVehicles = vehicles
         .Where(v => v.IsAvailable)
         .Select(v => new
         {
             Vehicle = v,
             Distance = CalculationHelper.CalculateDistance(
                 zone.LocationCoordinates.Latitude, zone.LocationCoordinates.Longitude,
                 v.LocationCoordinates.Latitude, v.LocationCoordinates.Longitude)
         })
         .Where(v => v.Distance <= maxDistance)
         .OrderBy(v => v.Distance)
         .ToList();

            var suitableVehicle = candidateVehicles
        .Where(v => v.Vehicle.Capacity >= remainingPeople)
        .OrderBy(v => v.Distance)
        .ThenBy(v => v.Vehicle.Capacity)
        .FirstOrDefault();
            if (suitableVehicle != null)
                return suitableVehicle.Vehicle; // หากพบรถที่สามารถรับคนได้ในโซนนี้ ให้ส่งคืนรถนั้น

            suitableVehicle = candidateVehicles
         .OrderBy(v => v.Distance)
         .ThenByDescending(v => v.Vehicle.Capacity)
         .FirstOrDefault();
            if (suitableVehicle == null)
                return null; // หากไม่พบรถที่เหมาะสม ให้ส่งคืน null


            return suitableVehicle.Vehicle;
        }

        public async Task EvacuationClearAsync()
        {
            await _semaphore.WaitAsync(); // รอจนกว่าจะสามารถเข้าถึงได้
            try
            {

                var zoneIDs = await _zoneRepository.GetAllZoneIDAsync(); // ดึงรายการ ZoneID ทั้งหมดจากฐานข้อมูล
                //var vehiles = await _vehicleRepository.GetAllVehiclesAsync();
                var vehicleIDs = await _vehicleRepository.GetAllVehicleIDAsync();// ดึงรายการรถทั้งหมดจากฐานข้อมูล
                foreach (var zoneID in zoneIDs)
                {
                    await _cache.ClearEvacuationStatusCache(zoneID); // ลบแคชสำหรับโซนนี้
                    _logger.LogInformation($"Removed cache for zone {zoneID}.");
                }
                foreach (var vehicleID in vehicleIDs)
                {
                    await _cache.ClearEvacuationPlansCache(vehicleID); // ลบแคชสำหรับแผนการอพยพของรถนี้
                    _logger.LogInformation($"Removed cache for evacuation plans of vehicle {vehicleID}.");
                }

                var deleteP = await _context.EvacuationPlans.ExecuteDeleteAsync(); // Clear all evacuation plans from the database
                var deleteS = await _context.EvacuationStatuses.ExecuteDeleteAsync(); // Clear all evacuation statuses from the database
                //await _context.SaveChangesAsync();
                _logger.LogInformation($"Cleared evacuation plans and statuses. Deleted {deleteP} plans and {deleteS} statuses from the database.");
                //foreach (var vehicle in vehiles)
                //{
                //    vehicle.IsAvailable = true; // ทำเครื่องหมายว่ารถทุกคันกลับมาใช้งานได้อีกครั้ง
                //    await _vehicleRepository.UpdateVehicleAsync(vehicle); // อัพเดทรถในฐานข้อมูล
                //    _logger.LogInformation($"Vehicle {vehicle.VehicleID} is now available again.");
                //}
                var vehicleUpdateCount = await _vehicleRepository.UpdateIsAvailableToTrue(); // ทำเครื่องหมายว่ารถทุกคันกลับมาใช้งานได้อีกครั้ง
                _logger.LogInformation($"Marked all vehicles as available again.");

            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<IEnumerable<EvacuationStatus>> EvacuationStatusAsync()
        {
           
            var zoneIDs = await _zoneRepository.GetAllZoneIDAsync(); // ดึงรายการ ZoneID ทั้งหมดจากฐานข้อมูล
            var evacuationStatusList = new List<EvacuationStatus>();
            foreach (var zoneID in zoneIDs)
            {

                var cachedStatusJson = await _cache.GetEvacuationStatusCache(zoneID);
                if (!string.IsNullOrEmpty(cachedStatusJson))
                {
                    var status = JsonSerializer.Deserialize<EvacuationStatus>(cachedStatusJson);
                    if (status != null)
                    {
                        evacuationStatusList.Add(status);
                        _logger.LogInformation($"Retrieved cached evacuation status for zone {zoneID}.");
                    }
                }
                else
                {
                    var evaStatus = await _context.EvacuationStatuses.FindAsync(zoneID);
                    if (evaStatus != null)
                    {
                        var statusJson = JsonSerializer.Serialize(evaStatus);

                        await _cache.SetEvacuationStatusCache(zoneID, statusJson); // เก็บสถานะการอพยพลงในแคช
                        evacuationStatusList.Add(evaStatus);
                        _logger.LogInformation($"No cached status found for zone {zoneID}. Fetched from database and cached it.");
                    }
                    else
                    {
                        _logger.LogWarning($"No evacuation status found for zone {zoneID} in the database.");
                    }
                }
            }
            return evacuationStatusList; // ส่งคืนสถานะการอพยพที่ถูกสร้างขึ้น
        }


        public async Task<IEnumerable<EvacuationStatus>> EvacuationUpdateAsync()
        {
            await _semaphore.WaitAsync(); // รอจนกว่าจะสามารถเข้าถึงได้
            try
            {
                var vehicleIDs = await _vehicleRepository.GetAllVehicleIDAsync(); // ดึงรายการรถทั้งหมดจากฐานข้อมูล

                var evacuationPlanList = new List<EvacuationPlan>();
                foreach ( var vehicleID in vehicleIDs ) {
                    var planJson = await _cache.GetEvacuationPlansCache(vehicleID);
                    if(!string.IsNullOrEmpty(planJson))
                    {
                        var plan = JsonSerializer.Deserialize<EvacuationPlan>(planJson);
                        if (plan != null)
                        {
                            evacuationPlanList.Add(plan);
                            _logger.LogInformation($"Retrieved cached evacuation plan for vehicle {vehicleID}.");
                        }
                        else
                        {
                            var plans = await _context.EvacuationPlans.Where(ep => ep.VehicleID == vehicleID).ToListAsync();
                            evacuationPlanList.AddRange(plans);
                            _logger.LogInformation($"Cached plan for vehicle {vehicleID} is invalid. Fetched from database.");
                        }
                    }
                    else
                    {
                        var plans = await _context.EvacuationPlans.Where(ep => ep.VehicleID == vehicleID).ToListAsync();
                        evacuationPlanList.AddRange(plans);
                        _logger.LogInformation($"No cached plans found for vehicle {vehicleID}. Fetched from database.");
                    }


                }

                var zones = await _zoneRepository.GetAllEvacuationZonesAsync();
                
                var evacuationStatusList = new List<EvacuationStatus>();

                foreach (var zone in zones)
                {
                    var zonePlans = evacuationPlanList.Where(ep => ep.ZoneID == zone.ZoneID).ToList();
                    if (zonePlans.Any())
                    {
                        _logger.LogInformation($"Updating evacuation status for zone {zone.ZoneID} with {zonePlans.Count} plans.");

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

                        var status = JsonSerializer.Serialize(evacuationStatus);
                        await _cache.SetEvacuationStatusCache(zone.ZoneID, status); // เก็บสถานะการอพยพลงในแคช
                        _logger.LogInformation($"Cached evacuation status for zone {zone.ZoneID}.");

                        var existingStatuses = await _context.EvacuationStatuses.Where(es => es.ZoneID == zone.ZoneID).FirstOrDefaultAsync();
                        if (existingStatuses != null) {
                            existingStatuses.TotalEvacuated = evacuationStatus.TotalEvacuated;
                            existingStatuses.RemainingPeople = evacuationStatus.RemainingPeople;
                            existingStatuses.LastVehicleIDUsed = evacuationStatus.LastVehicleIDUsed;

                            _logger.LogInformation($"Updated existing evacuation status for zone {zone.ZoneID}.");
                        }
                        else
                        {
                            await _context.EvacuationStatuses.AddAsync(evacuationStatus);
                            _logger.LogInformation($"Created new evacuation status for zone {zone.ZoneID}.");
                        }

                        evacuationStatusList.Add(evacuationStatus);

                    }
                    else
                    {
                        _logger.LogWarning($"No evacuation plans found for zone {zone.ZoneID}. Skipping status update.");

                    }

                }
                await _context.SaveChangesAsync(); // บันทึกสถานะการอพยพลงฐานข้อมูล
                return evacuationStatusList; // ส่งคืนสถานะการอพยพที่ถูกสร้างขึ้น
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
