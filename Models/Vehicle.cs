using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.Xml;
using System.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Evacuation_Planning_and_Monitoring_API.Models
{
    [Table("Vehicles")]
    public class Vehicle
    {
        [Key]
        [MaxLength(10, ErrorMessage = "Vehicle ID cannot exceed 10 characters.")]
        public string VehicleID { get; set; } = string.Empty; //V1, V2

        [Range(0, int.MaxValue, ErrorMessage = "Capacity must be a non-negative integer.")]

        public int Capacity { get; set; } 

        [MaxLength(50, ErrorMessage = "Type Vehicle cannot exceed 50 characters.")]
        public string Type { get; set; } = string.Empty; //Type of vehicle (e.g., bus, van, boat).
    
        public LocationCoordinates LocationCoordinates { get; set; } = new LocationCoordinates();

        [Range(0, int.MaxValue, ErrorMessage = "Speed must be a non-negative integer.")]
        public int Speed { get; set; }  // km/h.

    }
}
