using System.ComponentModel.DataAnnotations;

namespace Evacuation_Planning_and_Monitoring_API.Models
{
    public class LocationCoordinates
    {
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90 degrees.")]
        public double Latitude { get; set; }
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180 degrees.")]
        public double Longitude { get; set; } 
    }
}
