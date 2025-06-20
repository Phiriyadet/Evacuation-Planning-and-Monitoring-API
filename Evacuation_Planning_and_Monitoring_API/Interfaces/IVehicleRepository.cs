using Evacuation_Planning_and_Monitoring_API.Models;

namespace Evacuation_Planning_and_Monitoring_API.Interfaces
{
    public interface IVehicleRepository
    {
        Task<Vehicle?> GetVehiclesByIdAsync(string id);
        Task<IEnumerable<Vehicle>> GetAllVehiclesAsync();
        Task<IEnumerable<string>> GetAllVehicleIDAsync();
        Task<Vehicle> AddVehicleAsync(Vehicle vehicle);
        Task<Vehicle?> UpdateVehicleAsync(Vehicle vehicle);
        Task<int> UpdateIsAvailableToTrue();
        Task<Vehicle?> DeleteVehicleAsync(string id);
    }
}
