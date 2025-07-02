using Evacuation_Planning_and_Monitoring_API.Data;
using Evacuation_Planning_and_Monitoring_API.Interfaces;
using Evacuation_Planning_and_Monitoring_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Evacuation_Planning_and_Monitoring_API.Repositories
{
    public class EvacuationZoneRepository : IEvacuationZoneRepository
    {
        private readonly ApplicationDBContext _context;
        public EvacuationZoneRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<EvacuationZone> AddEvacuationZoneAsync(EvacuationZone zone)
        {
            zone.ZoneID = zone.ZoneID.ToUpper(); // Ensure ZoneID is in uppercase
            await _context.EvacuationZones.AddAsync(zone);
            await _context.SaveChangesAsync();
            return zone;
        }

        public async Task<EvacuationZone?> DeleteEvacuationZoneAsync(string id)
        {
            var zone = await _context.EvacuationZones.FirstOrDefaultAsync(e => e.ZoneID.ToUpper() == id.ToUpper());
            if (zone != null)
            {
                _context.EvacuationZones.Remove(zone);
                await _context.SaveChangesAsync();
                return zone;
            }
            return null;

        }

        public async Task<IEnumerable<EvacuationZone>> GetAllEvacuationZonesAsync()
        {
            return await _context.EvacuationZones.ToListAsync();

        }

        public async Task<IEnumerable<string>> GetAllZoneIDAsync()
        {
            return await _context.EvacuationZones.Select(e => e.ZoneID).ToListAsync();
        }

        public async Task<EvacuationZone?> GetEvacuationZoneByIdAsync(string id)
        {
            return await _context.EvacuationZones.FirstOrDefaultAsync(e => e.ZoneID.ToUpper() == id.ToUpper());

        }

        public async Task<EvacuationZone?> UpdateEvacuationZoneAsync(EvacuationZone zone)
        {
            var existingEvacuationZone = await _context.EvacuationZones.FirstOrDefaultAsync(e => e.ZoneID.ToUpper() == zone.ZoneID.ToUpper());
            if (existingEvacuationZone != null)
            {
                existingEvacuationZone.LocationCoordinates = zone.LocationCoordinates;
                existingEvacuationZone.NumberOfPeople = zone.NumberOfPeople;
                existingEvacuationZone.UrgencyLevel = zone.UrgencyLevel;

                await _context.SaveChangesAsync();
                return existingEvacuationZone;
            }
            return null;

        }
    }
}
