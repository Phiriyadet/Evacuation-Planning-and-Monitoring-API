using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Evacuation_Planning_and_Monitoring_API.Models
{
   
    public class EvacuationZone
    {
        [Key]
        public string ZoneID { get; set; } = string.Empty; //Z1, Z2, Z3
        public LocationCoordinates LocationCoordinates { get; set; } = new LocationCoordinates();
        public int NumberOfPeople { get; set; }
        [Range(1,5)]
        public int UrgencyLevel { get; set; }//(1 = low urgency, 5 = high urgency).
    }
}
