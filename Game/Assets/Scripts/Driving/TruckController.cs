using UnityEngine;

namespace EuroHaul.Driving
{
    /// <summary>Truck dynamics adapter. Assign four WheelColliders and optionally TruckPowertrain.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class TruckController : MonoBehaviour
    {
        [Header("Wheel Colliders")]
        [SerializeField] private WheelCollider frontLeft;
        [SerializeField] private WheelCollider frontRight;
        [SerializeField] private WheelCollider rearLeft;
        [SerializeField] private WheelCollider rearRight;

        [Header("Vehicle Tuning")]
        [SerializeField, Min(5f)] private float maxSteerAngle = 28f;
        [SerializeField, Min(100f)] private float serviceBrakeTorque = 4500f;
        [SerializeField, Min(5f)] private float maxForwardSpeedKmh = 90f;
        [SerializeField, Min(5f)] private float maxReverseSpeedKmh = 15f;
        [SerializeField] private Vector3 centerOfMassOffset = new Vector3(0f, -0.65f, 0f);
        [SerializeField] private TruckPowertrain powertrain;

        private Rigidbody body;
        private float steering;
        private float throttle;
        private float brake;
        private float clutchPedal = 0f;

        public TruckDashboardTelemetry Telemetry { get; } = new TruckDashboardTelemetry();
        public float SpeedKmh => body != null ? body.linearVelocity.magnitude * 3.6f : 0f;
        public int CurrentGear => powertrain != null ? powertrain.Gear : 0;\n        public float SignedSpeedKmh => body != null
            ? Vector3.Dot(body.linearVelocity, transform.forward) * 3.6f : 0f;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            if (powertrain == null) powertrain = GetComponent<TruckPowertrain>();
            body.centerOfMass = centerOfMassOffset;
            body.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public void SetInput(float steeringInput, float throttleInput, float brakeInput)
        {
            steering = Mathf.Clamp(steeringInput, -1f, 1f);
            throttle = Mathf.Clamp(throttleInput, -1f, 1f);
            brake = Mathf.Clamp01(brakeInput);
        }

        public void SetClutchPedal(float value)
        {
            clutchPedal = Mathf.Clamp01(value);
            if (powertrain != null) powertrain.SetClutchPedal(clutchPedal);
        }

        public bool ShiftGearUp() => powertrain != null && powertrain.ShiftUp();
        public bool ShiftGearDown() => powertrain != null && powertrain.ShiftDown();
        public void SelectNeutral() { if (powertrain != null) powertrain.SelectNeutral(); }
        public bool SelectReverse() => powertrain != null && powertrain.SelectReverse();

        private void FixedUpdate()
        {
            if (!WheelsAssigned()) return;
            frontLeft.steerAngle = steering * maxSteerAngle;
            frontRight.steerAngle = steering * maxSteerAngle;

            float driveTorque;
            if (powertrain != null)
            {
                // Gear selection determines direction. A negative input is not used to force a gear change.
                float selectedThrottle = Mathf.Clamp01(Mathf.Abs(throttle));
                if (SignedSpeedKmh >= maxForwardSpeedKmh && !powertrain.IsNeutral && powertrain.Gear > 0)
                    selectedThrottle = 0f;
                if (SignedSpeedKmh <= -maxReverseSpeedKmh && powertrain.Gear < 0)
                    selectedThrottle = 0f;
                powertrain.SetThrottle(selectedThrottle);
                powertrain.SetWheelSpeed(SignedSpeedKmh);
                driveTorque = powertrain.GetWheelTorque(Time.fixedDeltaTime);
            }
            else
            {
                // Backward-compatible fallback until TruckPowertrain is added to the truck.
                float requested = throttle;
                if (requested > 0f && SignedSpeedKmh >= maxForwardSpeedKmh) requested = 0f;
                if (requested < 0f && SignedSpeedKmh <= -maxReverseSpeedKmh) requested = 0f;
                driveTorque = requested * 1800f;
            }

            rearLeft.motorTorque = driveTorque;
            rearRight.motorTorque = driveTorque;

            float appliedBrake = brake * serviceBrakeTorque;
            frontLeft.brakeTorque = appliedBrake;
            frontRight.brakeTorque = appliedBrake;
            rearLeft.brakeTorque = appliedBrake;
            rearRight.brakeTorque = appliedBrake;

            Telemetry.SpeedKmh = SpeedKmh;
            Telemetry.EngineRpm = powertrain != null ? powertrain.EngineRpm : 0f;
            Telemetry.Gear = powertrain != null ? powertrain.Gear : 0;
            Telemetry.ClutchPercent = clutchPedal * 100f;
            Telemetry.ThrottlePercent = Mathf.Abs(throttle) * 100f;
            Telemetry.BrakePercent = brake * 100f;
            Telemetry.SteeringPercent = steering * 100f;
            Telemetry.EngineRunning = powertrain != null;
        }

        private bool WheelsAssigned() =>
            frontLeft != null && frontRight != null && rearLeft != null && rearRight != null;

        private void OnDisable()
        {
            steering = throttle = 0f;
            brake = 1f;
            clutchPedal = 1f;
            if (powertrain != null)
            {
                powertrain.SetThrottle(0f);
                powertrain.SetClutchPedal(1f);
            }
            if (rearLeft != null) rearLeft.motorTorque = 0f;
            if (rearRight != null) rearRight.motorTorque = 0f;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            serviceBrakeTorque = Mathf.Max(100f, serviceBrakeTorque);
            maxForwardSpeedKmh = Mathf.Max(5f, maxForwardSpeedKmh);
            maxReverseSpeedKmh = Mathf.Max(5f, maxReverseSpeedKmh);
        }
#endif
    }
}