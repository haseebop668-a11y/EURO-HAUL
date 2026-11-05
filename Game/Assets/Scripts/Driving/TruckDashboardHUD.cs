using UnityEngine;

namespace EuroHaul.Driving
{
    public sealed class TruckDashboardHUD : MonoBehaviour
    {
        [SerializeField] private TruckController truck;
        [SerializeField] private TruckTrafficSensors sensors;
        [SerializeField] private TruckSafetyAdvisor safety;
        [SerializeField] private TruckSignals signals;
        [SerializeField] private TruckCollisionMonitor collision;
        [SerializeField] private bool showHud = true;

        private void Awake()
        {
            if (truck == null) truck = GetComponentInParent<TruckController>();
            if (sensors == null) sensors = GetComponentInParent<TruckTrafficSensors>();
            if (safety == null) safety = GetComponentInParent<TruckSafetyAdvisor>();
            if (signals == null) signals = GetComponentInParent<TruckSignals>();
            if (collision == null) collision = GetComponentInParent<TruckCollisionMonitor>();
        }

        private void OnGUI()
        {
            if (!showHud || truck == null) return;
            TruckDashboardTelemetry data = truck.Telemetry;
            GUI.Box(new Rect(12f, 12f, 230f, 170f), "EURO HAUL | INSTRUMENTS");
            GUI.Label(new Rect(24f, 36f, 210f, 20f), $"SPEED  {data.SpeedKmh:0} km/h");
            GUI.Label(new Rect(24f, 56f, 210f, 20f), $"ENGINE {data.EngineRpm:0} rpm");
            GUI.Label(new Rect(24f, 76f, 210f, 20f), $"GEAR   {data.GearLabel}");
            GUI.Label(new Rect(24f, 96f, 210f, 20f), $"CLUTCH {data.ClutchPercent:0}%");
            GUI.Label(new Rect(24f, 116f, 210f, 20f), $"SIGNAL {signals != null ? signals.State.ToString() : "N/A"}");
            GUI.Label(new Rect(24f, 136f, 210f, 20f), $"HORN   {(signals != null && signals.IsHornHeld ? "ON" : "OFF")}");
            GUI.Label(new Rect(24f, 156f, 210f, 20f), $"DAMAGE {(collision != null ? collision.DamagePercent : 0f):0}%");

            if (sensors == null && safety == null) return;
            GUI.Box(new Rect(12f, 190f, 285f, 250f), "TRAFFIC | MIRRORS | SAFETY");
            GUI.Label(new Rect(24f, 214f, 270f, 20f), $"FRONT GAP {GapText(sensors != null ? sensors.FrontGapMeters : float.PositiveInfinity)}");
            GUI.Label(new Rect(24f, 234f, 270f, 20f), $"REAR GAP  {GapText(sensors != null ? sensors.RearGapMeters : float.PositiveInfinity)}");
            GUI.Label(new Rect(24f, 254f, 270f, 20f), $"LEFT BLIND  {(sensors != null && sensors.LeftBlindSpotOccupied ? "OCCUPIED" : "CLEAR")}");
            GUI.Label(new Rect(24f, 274f, 270f, 20f), $"RIGHT BLIND {(sensors != null && sensors.RightBlindSpotOccupied ? "OCCUPIED" : "CLEAR")}");
            GUI.Label(new Rect(24f, 294f, 270f, 20f), $"LEFT LANE  {(safety != null && safety.CanOvertakeLeft ? "GAP CLEAR" : "WAIT / CHECK")}");
            GUI.Label(new Rect(24f, 314f, 270f, 20f), $"RIGHT LANE {(safety != null && safety.CanOvertakeRight ? "GAP CLEAR" : "WAIT / CHECK")}");
            GUI.Label(new Rect(24f, 334f, 270f, 20f), $"STOP DIST {(safety != null ? safety.RequiredStoppingDistanceMeters : 0f):0} m");
            GUI.Label(new Rect(24f, 354f, 270f, 20f), SafetyText());
            GUI.Label(new Rect(24f, 374f, 270f, 20f), $"IMPACT {(collision != null ? collision.LastImpactSeverity.ToString() : "N/A")} {(collision != null ? collision.LastImpactSpeedKmh : 0f):0} km/h");
            GUI.Label(new Rect(24f, 394f, 270f, 20f), "ADVISORY ONLY - DRIVER CONTROLS TRUCK");
        }

        private string SafetyText()
        {
            if (collision != null && collision.IsVehicleDisabled) return "CRITICAL DAMAGE: SERVICE REQUIRED";
            if (safety == null) return "ADVISORY NOT CONNECTED";
            if (safety.FrontCollisionRisk) return "BRAKE WARNING: FRONT GAP";
            if (safety.SteeringLimitRisk) return "HIGH SPEED: STEERING LIMITED";
            if (safety.CurveSpeedRisk) return $"SLOW FOR CURVE: {safety.RecommendedCurveSpeedKmh:0} km/h";
            if (safety.FastRearApproachRisk) return "FAST VEHICLE APPROACHING REAR";
            if (safety.BlindSpotRisk) return "BLIND SPOT OCCUPIED";
            return "ROAD AWARENESS: CLEAR";
        }

        private static string GapText(float gap) =>
            float.IsPositiveInfinity(gap) ? "--" : $"{gap:0.0} m";
    }
}