using System.Collections.Generic;

namespace Evacuation_Planning_and_Monitoring_API.Models
{
    public class EvacuationStatus
    {
        public string ZoneID { get; set; } = string.Empty; 
        public int TotalEvacuated { get; set; } 
        public int RemainingPeople { get; set; }
        public Vehicle LastVehicleUsed { get; set; } = new Vehicle(); // The last vehicle used for evacuation.

    }
}


