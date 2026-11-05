namespace EuroHaul.Driving
{
    /// <summary>Read-only dashboard data source. UI can bind to these values without owning vehicle logic.</summary>
    public sealed class TruckDashboardTelemetry
    {
        public float SpeedKmh { get; internal set; }
        public float EngineRpm { get; internal set; }
        public int Gear { get; internal set; }
        public float ClutchPercent { get; internal set; }
        public float ThrottlePercent { get; internal set; }
        public float BrakePercent { get; internal set; }
        public float SteeringPercent { get; internal set; }
        public bool EngineRunning { get; internal set; }

        public string GearLabel => Gear < 0 ? "R" : Gear == 0 ? "N" : Gear.ToString();
    }
}