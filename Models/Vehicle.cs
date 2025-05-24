using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.Xml;
using System.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Evacuation_Planning_and_Monitoring_API.Models
{
 
    public class Vehicle
    {
        
        public string VehicleID { get; set; } = string.Empty; //V1, V2
   
        public int Capacity { get; set; } 
      
        public string Type { get; set; } = string.Empty; //Type of vehicle (e.g., bus, van, boat).
    
        public LocationCoordinates LocationCoordinates { get; set; } = new LocationCoordinates(); //Latitude and longitude of the vehicle’s current location.
 
        public int Speed { get; set; } //Average speed of the vehicle in km/h.

    }
}
