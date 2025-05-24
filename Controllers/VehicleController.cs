using Evacuation_Planning_and_Monitoring_API.Interfaces;
using Evacuation_Planning_and_Monitoring_API.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Evacuation_Planning_and_Monitoring_API.Controllers
{
    [Route("api/vehicles")]
    [ApiController]
    public class VehicleController : ControllerBase
    { 
        private readonly IVehicleRepository _vehicleRepository;
        public VehicleController(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        // GET: api/vehicles
        [HttpGet]
        public async Task<ActionResult<Vehicle>> GetAll()
        {
            try
            {
                var vehicles = await _vehicleRepository.GetAllVehiclesAsync();
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET api/vehicles/V5
        [HttpGet("{id}")]
        public async Task<ActionResult<Vehicle>> GetByID(string id)
        {
            try
            {
                var vehicle = await _vehicleRepository.GetVehiclesByIdAsync(id);
                if (vehicle == null)
                {
                    return NotFound($"Vehicle with ID {id} not found.");
                }
                return Ok(vehicle);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        // POST api/vehicles
        [HttpPost]
        public async Task<ActionResult<Vehicle>> Create([FromBody] Vehicle vehicle)
        {
            if (vehicle == null)
            {
                return BadRequest("Vehicle object is null.");
            }
            try
            {
                var createdVehicle = await _vehicleRepository.AddVehicleAsync(vehicle);
                return CreatedAtAction(nameof(GetByID), new { id = createdVehicle.VehicleID }, createdVehicle);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        // PUT api/vehicles
        [HttpPut]
        public async Task<ActionResult<Vehicle>> Update([FromBody] Vehicle vehicle)
        {
            if (vehicle== null)
            {
                return BadRequest("Vehicle object is null.");
            }
            try
            {
                var updatedVehicle = await _vehicleRepository.UpdateVehicleAsync(vehicle);
                if (updatedVehicle == null)
                {
                    return NotFound($"Vehicle with ID {vehicle.VehicleID} not found.");
                }
                return Ok(updatedVehicle);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        // DELETE api/vehicles/V5
        [HttpDelete("{id}")]
        public ActionResult Delete(string id)
        {
            try
            {
                var deletedVehicle = _vehicleRepository.DeleteVehicleAsync(id);
                if (deletedVehicle == null)
                {
                    return NotFound($"Vehicle with ID {id} not found.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }
    }
}
