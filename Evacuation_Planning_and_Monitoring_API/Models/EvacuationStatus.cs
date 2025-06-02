using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Evacuation_Planning_and_Monitoring_API.Models
{
    public class EvacuationStatus
    {
        [Key]
        public string ZoneID { get; set; } = string.Empty; 
        public int TotalEvacuated { get; set; } 
        public int RemainingPeople { get; set; }
        public string? LastVehicleIDUsed { get; set; } = string.Empty;  // The last vehicle used for evacuation.

    }
}


