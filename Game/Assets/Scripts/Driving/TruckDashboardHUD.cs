using UnityEngine;

namespace EuroHaul.Driving
{
    /// <summary>Minimal built-in prototype HUD; replace with a designed Canvas UI in the presentation milestone.</summary>
    public sealed class TruckDashboardHUD : MonoBehaviour
    {
        [SerializeField] private TruckController truck;
        [SerializeField] private bool showHud = true;

        private void Awake()
        {
            if (truck == null) truck = GetComponentInParent<TruckController>();
        }

        private void OnGUI()
        {
            if (!showHud || truck == null) return;
            TruckDashboardTelemetry data = truck.Telemetry;
            const int width = 210;
            const int height = 112;
            GUI.Box(new Rect(12f, 12f, width, height), "EURO HAUL  |  TRUCK");
            GUI.Label(new Rect(24f, 36f, width - 24, 20f), $"SPEED   {data.SpeedKmh:0} km/h");
            GUI.Label(new Rect(24f, 56f, width - 24, 20f), $"ENGINE  {data.EngineRpm:0} rpm");
            GUI.Label(new Rect(24f, 76f, width - 24, 20f), $"GEAR    {data.GearLabel}");
            GUI.Label(new Rect(24f, 96f, width - 24, 20f), $"CLUTCH  {data.ClutchPercent:0}%");
        }
    }
}