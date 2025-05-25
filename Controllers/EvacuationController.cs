using Evacuation_Planning_and_Monitoring_API.Interfaces;
using Evacuation_Planning_and_Monitoring_API.Models;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<EvacuationPlan>> Plan()
        {
            try
            {
                await _evacuationRepository.EvacationPlanAsync();
                return Ok("Evacuation plan created successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        ////GET: /api/evacuations/status
        //[HttpGet("status")]
        ////PUT: /api/evacuations/update
        //[HttpPut("update")]
        ////DELETE: /api/evacuations/clear
        //[HttpDelete("clear")]

    }
}
