using UnityEngine;

namespace EuroHaul.Driving
{
    /// <summary>Thin UI bridge for signal and horn controls; attach beside TruckSignals.</summary>
    public sealed class TruckIndicatorInput : MonoBehaviour
    {
        [SerializeField] private TruckSignals signals;

        private void Awake()
        {
            if (signals == null) signals = GetComponent<TruckSignals>();
        }

        public void LeftIndicator() { if (signals != null) signals.ToggleLeft(); }
        public void RightIndicator() { if (signals != null) signals.ToggleRight(); }
        public void HazardLights() { if (signals != null) signals.ToggleHazard(); }
        public void SignalOff() { if (signals != null) signals.CancelSignal(); }
        public void HornDown() { if (signals != null) signals.HornDown(); }
        public void HornUp() { if (signals != null) signals.HornUp(); }
    }
}