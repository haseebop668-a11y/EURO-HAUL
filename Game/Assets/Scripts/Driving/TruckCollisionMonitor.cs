using UnityEngine;
using UnityEngine.Events;

namespace EuroHaul.Driving
{
    public enum ImpactSeverity { None, Minor, Moderate, Severe, Critical }

    /// <summary>Records impacts and applies short emergency braking. Damage is a gameplay estimate.</summary>
    public sealed class TruckCollisionMonitor : MonoBehaviour
    {
        [SerializeField] private TruckController truck;
        [SerializeField] private TruckSignals signals;
        [SerializeField, Min(1f)] private float minimumRecordedImpactKmh = 5f;
        [SerializeField, Min(1f)] private float hazardActivationImpactKmh = 20f;
        [SerializeField, Min(1f)] private float emergencyBrakeImpactKmh = 30f;
        [SerializeField, Min(0.1f)] private float postImpactBrakeSeconds = 2f;
        [SerializeField] private UnityEvent onImpact;
        [SerializeField] private UnityEvent onSevereImpact;

        private float emergencyBrakeUntil;

        public float LastImpactSpeedKmh { get; private set; }
        public float DamagePercent { get; private set; }
        public ImpactSeverity LastImpactSeverity { get; private set; }
        public bool IsVehicleDisabled => DamagePercent >= 100f;

        private void Awake()
        {
            if (truck == null) truck = GetComponent<TruckController>();
            if (signals == null) signals = GetComponent<TruckSignals>();
        }

        private void Update()
        {
            if (truck != null && Time.time < emergencyBrakeUntil)
                truck.SetInput(0f, 0f, 1f);
        }

        private void OnCollisionEnter(Collision collision)
        {
            float impactSpeed = collision.relativeVelocity.magnitude * 3.6f;
            if (impactSpeed < minimumRecordedImpactKmh) return;

            LastImpactSpeedKmh = impactSpeed;
            LastImpactSeverity = Classify(impactSpeed);
            DamagePercent = Mathf.Clamp(DamagePercent + Mathf.InverseLerp(5f, 70f, impactSpeed) * 38f, 0f, 100f);
            onImpact?.Invoke();

            if (impactSpeed >= hazardActivationImpactKmh && signals != null)
                signals.SetState(TurnSignalState.Hazard);

            if (impactSpeed >= emergencyBrakeImpactKmh && truck != null)
            {
                emergencyBrakeUntil = Time.time + postImpactBrakeSeconds;
                truck.SetInput(0f, 0f, 1f);
                if (impactSpeed >= 50f) truck.SetCrashLock(true);
                onSevereImpact?.Invoke();
            }
        }

        public void RepairAtService()
        {
            DamagePercent = 0f;
            LastImpactSpeedKmh = 0f;
            LastImpactSeverity = ImpactSeverity.None;
            emergencyBrakeUntil = 0f;
            if (truck != null) truck.ResetCrashLock();
            if (signals != null) signals.CancelSignal();
        }

        private static ImpactSeverity Classify(float speedKmh)
        {
            if (speedKmh >= 50f) return ImpactSeverity.Critical;
            if (speedKmh >= 30f) return ImpactSeverity.Severe;
            if (speedKmh >= 15f) return ImpactSeverity.Moderate;
            return ImpactSeverity.Minor;
        }
    }
}