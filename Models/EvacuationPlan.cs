using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Evacuation_Planning_and_Monitoring_API.Models
{
    public class EvacuationPlan
    {
        [Key]
        [JsonIgnore]
        public int Id { get; set; }
        public string ZoneID { get; set; } = string.Empty;
        public string VehicleID { get; set; } = string.Empty;
        public string ETA { get; set; } = string.Empty;
        public int NumberOfPeople { get; set; }
    }
}
