using Evacuation_Planning_and_Monitoring_API.Interfaces;
using Evacuation_Planning_and_Monitoring_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace Evacuation_Planning_and_Monitoring_API.Controllers
{
    [Route("api/evacuations")]
    [ApiController]
    public class EvacuationController : Controller
    {
        private readonly IEvacuationRepository _evacuationRepository;
       
        public EvacuationController(IEvacuationRepository evacuationRepository)
        {
            _evacuationRepository = evacuationRepository;
           
        }

        //POST: /api/evacuations/plan
        [HttpPost("plan")]
        public async Task<ActionResult<EvacuationPlan>> Plan([FromQuery] double distanceKm=10.0)
        {
            try
            {
                var result = await _evacuationRepository.EvacationPlanAsync(distanceKm);
                if (result == null || !result.Any())
                {
                    return NotFound("No evacuation plans found for the specified distance.");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //GET: /api/evacuations/status
        [HttpGet("status")]
        public async Task<ActionResult<IEnumerable<EvacuationStatus>>> GetStatus()
        {
            try
            {
                var status = await _evacuationRepository.EvacuationStatusAsync();
                if (status == null || !status.Any())
                {
                    return NotFound("No evacuation status found.");
                }
                return Ok(status);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        //PUT: /api/evacuations/update
        [HttpPut("update")]
        public async Task<ActionResult<IEnumerable<EvacuationStatus>>> UpdateStatus()
        {
            try
            {
                var updatedStatus = await _evacuationRepository.EvacuationUpdateAsync();
                return Ok(updatedStatus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        //DELETE: /api/evacuations/clear
        [HttpDelete("clear")]
        public async Task<ActionResult> ClearEvacuations()
        {
            try
            {
                await _evacuationRepository.EvacuationClearAsync();
                return Ok("Evacuation data cleared successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
