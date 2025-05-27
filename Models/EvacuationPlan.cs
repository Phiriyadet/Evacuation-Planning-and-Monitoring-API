namespace Evacuation_Planning_and_Monitoring_API.Models
{
    public class EvacuationPlan
    {
        public string ZoneID { get; set; } = string.Empty;
        public string VehicleID { get; set; } = string.Empty;
        public string ETA { get; set; } = string.Empty;
        public int NumberOfPeopleEvacuated { get; set; }
    }
}
