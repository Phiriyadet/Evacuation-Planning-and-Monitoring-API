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
        public async Task<EvacuationZone> AddEvacuationZoneAsync(EvacuationZone evacuationZone)
        {
            await _context.EvacuationZones.AddAsync(evacuationZone);
            await _context.SaveChangesAsync();
            return evacuationZone;

        }

        public async Task<EvacuationZone?> DeleteEvacuationZoneAsync(string id)
        {
            var evacuationZone = await _context.EvacuationZones.FirstOrDefaultAsync(e => e.ZoneID == id);
            if (evacuationZone != null)
            {
                _context.EvacuationZones.Remove(evacuationZone);
                await _context.SaveChangesAsync();
                return evacuationZone;
            }
            return null;

        }

        public async Task<IEnumerable<EvacuationZone>> GetAllEvacuationZonesAsync()
        {
            return await _context.EvacuationZones.ToListAsync();

        }

        public async Task<EvacuationZone?> GetEvacuationZoneByIdAsync(string id)
        {
            return await _context.EvacuationZones.FirstOrDefaultAsync(e => e.ZoneID == id);

        }

        public async Task<EvacuationZone?> UpdateEvacuationZoneAsync(EvacuationZone evacuationZone)
        {
            var existingEvacuationZone = await _context.EvacuationZones.FirstOrDefaultAsync(e => e.ZoneID == evacuationZone.ZoneID);
            if (existingEvacuationZone != null)
            {
                existingEvacuationZone.LocationCoordinates = evacuationZone.LocationCoordinates;
                existingEvacuationZone.NumberOfPeople = evacuationZone.NumberOfPeople;
                existingEvacuationZone.UrgencyLevel = evacuationZone.UrgencyLevel;

                await _context.SaveChangesAsync();
                return existingEvacuationZone;
            }
            return null;

        }
    }
}
