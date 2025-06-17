using Evacuation_Planning_and_Monitoring_API.Data;
using Evacuation_Planning_and_Monitoring_API.Interfaces;
using Evacuation_Planning_and_Monitoring_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Evacuation_Planning_and_Monitoring_API.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly ApplicationDBContext _context;
        public VehicleRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<Vehicle> AddVehicleAsync(Vehicle vehicle)
        {
            vehicle.VehicleID = vehicle.VehicleID.ToUpper(); // Ensure VehicleID is in uppercase
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            return vehicle;
        }

        public async  Task<Vehicle?> DeleteVehicleAsync(string id)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleID.ToUpper() == id.ToUpper());
            if (vehicle != null)
            {
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
                return vehicle;
            }
            return null;
        }

        public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync()
        {
            return await _context.Vehicles.ToListAsync();
        }

        public async Task<Vehicle?> GetVehiclesByIdAsync(string id)
        {
            return await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleID.ToUpper() == id.ToUpper());
        }

        public async Task<int> UpdateIsAvailableToTrue()
        {
            return await _context.Vehicles.Where(v => v.IsAvailable == false)
                    .ExecuteUpdateAsync(v => v.SetProperty(x => x.IsAvailable, true));
        }

        public async Task<Vehicle?> UpdateVehicleAsync(Vehicle vehicle)
        {
            var existingVehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleID.ToUpper() == vehicle.VehicleID.ToUpper());
            if (existingVehicle != null)
            {
                existingVehicle.Capacity = vehicle.Capacity;
                existingVehicle.Type = vehicle.Type;
                existingVehicle.LocationCoordinates = vehicle.LocationCoordinates;
                existingVehicle.Speed = vehicle.Speed;
                await _context.SaveChangesAsync();
                return existingVehicle;
            }
            return null;
        }
    }
}
