using Evacuation_Planning_and_Monitoring_API.Interfaces;
using Evacuation_Planning_and_Monitoring_API.Models;
using Evacuation_Planning_and_Monitoring_API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Evacuation_Planning_and_Monitoring_API.Controllers
{
    [Route("api/evcuation-zones")]
    [ApiController]
    public class EvacuationZoneController : ControllerBase
    {
        private readonly IEvacuationZoneRepository _evacuationZoneRepository;
        public EvacuationZoneController(IEvacuationZoneRepository evacuationZoneRepository)
        {
            _evacuationZoneRepository = evacuationZoneRepository;
        }

        // GET: api/evacuation-zones
        [HttpGet]
        public async Task<ActionResult<EvacuationZone>> GetAll()
        {
            try
            {
                var evacuationZones = await _evacuationZoneRepository.GetAllEvacuationZonesAsync();
                return Ok(evacuationZones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET api/evacuation-zones/z5
        [HttpGet("{id}")]
        public async Task<ActionResult<EvacuationZone>> GetByID(string id)
        {
            try
            {
                var evacuationZone = await _evacuationZoneRepository.GetEvacuationZoneByIdAsync(id);
                if (evacuationZone == null)
                {
                    return NotFound($"EvacuationZone with ID {id} not found.");
                }
                return Ok(evacuationZone);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        // POST api/evacuation-zones
        [HttpPost]
        public async Task<ActionResult<EvacuationZone>> Create([FromBody] EvacuationZone evacuationZone)
        {
            if (evacuationZone == null)
            {
                return BadRequest("EvacuationZone object is null.");
            }
            try
            {
                var createdEvacuationZone = await _evacuationZoneRepository.AddEvacuationZoneAsync(evacuationZone);
                return CreatedAtAction(nameof(GetByID), new { id = createdEvacuationZone.ZoneID }, createdEvacuationZone);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        // PUT api/evacuation-zones
        [HttpPut]
        public async Task<ActionResult<EvacuationZone>> Update([FromBody] EvacuationZone evacuationZone)
        {
            if (evacuationZone == null)
            {
                return BadRequest("EvacuationZone object is null.");
            }
            try
            {
                var updatedEvacuationZone = await _evacuationZoneRepository.UpdateEvacuationZoneAsync(evacuationZone);
                if (updatedEvacuationZone == null)
                {
                    return NotFound($"EvacuationZone with ID {evacuationZone.ZoneID} not found.");
                }
                return Ok(updatedEvacuationZone);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        // DELETE api/evacuation-zones/z5
        [HttpDelete("{id}")]
        public ActionResult Delete(string id)
        {
            try
            {
                var deletedEvacuationZone = _evacuationZoneRepository.DeleteEvacuationZoneAsync(id);
                if (deletedEvacuationZone == null)
                {
                    return NotFound($"EvacuationZone with ID {id} not found.");
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
