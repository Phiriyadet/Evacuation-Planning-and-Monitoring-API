using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Evacuation_Planning_and_Monitoring_API.Models
{
    [Table("EvacuationZones")]
    public class EvacuationZone
    {
        [Key]
        [MaxLength(10, ErrorMessage = "Zone ID cannot exceed 10 characters.")]
        public string ZoneID { get; set; } = string.Empty; //Z1, Z2, Z3

        public LocationCoordinates LocationCoordinates { get; set; } = new LocationCoordinates();

        [Range(0, int.MaxValue, ErrorMessage = "Number of people must be a non-negative integer.")]
        public int NumberOfPeople { get; set; } 

        [Range(1, 5, ErrorMessage = "Urgency level must be between 1 and 5.")]
        public int UrgencyLevel { get; set; }   //(1 = low urgency, 5 = high urgency).
    }
}
