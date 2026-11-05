using UnityEngine;

namespace EuroHaul.Driving
{
    /// <summary>Temporary IMGUI instrument and safety panel for the prototype.</summary>
    public sealed class TruckDashboardHUD : MonoBehaviour
    {
        [SerializeField] private TruckController truck;
        [SerializeField] private TruckTrafficSensors sensors;
        [SerializeField] private TruckSafetyAdvisor safety;
        [SerializeField] private TruckSignals signals;
        [SerializeField] private bool showHud = true;

        private void Awake()
        {
            if (truck == null) truck = GetComponentInParent<TruckController>();
            if (sensors == null) sensors = GetComponentInParent<TruckTrafficSensors>();
            if (safety == null) safety = GetComponentInParent<TruckSafetyAdvisor>();
            if (signals == null) signals = GetComponentInParent<TruckSignals>();
        }

        private void OnGUI()
        {
            if (!showHud || truck == null) return;
            TruckDashboardTelemetry data = truck.Telemetry;
            GUI.Box(new Rect(12f, 12f, 220f, 132f), "EURO HAUL | INSTRUMENTS");
            GUI.Label(new Rect(24f, 36f, 200f, 20f), $"SPEED  {data.SpeedKmh:0} km/h");
            GUI.Label(new Rect(24f, 56f, 200f, 20f), $"ENGINE {data.EngineRpm:0} rpm");
            GUI.Label(new Rect(24f, 76f, 200f, 20f), $"GEAR   {data.GearLabel}");
            GUI.Label(new Rect(24f, 96f, 200f, 20f), $"CLUTCH {data.ClutchPercent:0}%");
            GUI.Label(new Rect(24f, 116f, 200f, 20f), $"SIGNAL {signals != null ? signals.State.ToString() : "N/A"}");

            if (sensors == null && safety == null) return;
            GUI.Box(new Rect(12f, 152f, 250f, 150f), "TRAFFIC AWARENESS");
            GUI.Label(new Rect(24f, 176f, 230f, 20f), $"FRONT  {GapText(sensors != null ? sensors.FrontGapMeters : float.PositiveInfinity)}");
            GUI.Label(new Rect(24f, 196f, 230f, 20f), $"REAR   {GapText(sensors != null ? sensors.RearGapMeters : float.PositiveInfinity)}");
            GUI.Label(new Rect(24f, 216f, 230f, 20f), $"LEFT BLIND SPOT  {(sensors != null && sensors.LeftBlindSpotOccupied ? "OCCUPIED" : "CLEAR")}");
            GUI.Label(new Rect(24f, 236f, 230f, 20f), $"RIGHT BLIND SPOT {(sensors != null && sensors.RightBlindSpotOccupied ? "OCCUPIED" : "CLEAR")}");
            GUI.Label(new Rect(24f, 256f, 230f, 20f), $"STOP DIST { (safety != null ? safety.RequiredStoppingDistanceMeters : 0f):0} m");
            GUI.Label(new Rect(24f, 276f, 230f, 20f), SafetyText());
        }

        private string SafetyText()
        {
            if (safety == null) return "ADVISORY NOT CONNECTED";
            if (safety.FrontCollisionRisk) return "WARNING: BRAKE / COLLISION RISK";
            if (safety.CurveSpeedRisk) return $"SLOW DOWN: CURVE {safety.RecommendedCurveSpeedKmh:0} km/h";
            if (safety.FastRearApproachRisk) return "REAR VEHICLE APPROACHING FAST";
            if (safety.BlindSpotRisk) return "BLIND SPOT: DO NOT CHANGE LANE";
            return "ROAD AWARENESS: CLEAR";
        }

        private static string GapText(float gap) =>
            float.IsPositiveInfinity(gap) ? "--" : $"{gap:0.0} m";
    }
}