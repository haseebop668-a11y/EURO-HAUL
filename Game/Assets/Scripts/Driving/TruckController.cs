using UnityEngine;

namespace EuroHaul.Driving
{
    /// <summary>Truck dynamics adapter. Assign four WheelColliders and a TruckPowertrain.</summary>
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
        [SerializeField, Min(1f)] private float wheelbaseMeters = 4.2f;
        [SerializeField, Min(0.5f)] private float maxLateralAccelerationMps2 = 2.2f;
        [SerializeField] private Vector3 centerOfMassOffset = new Vector3(0f, -0.65f, 0f);
        [SerializeField] private TruckPowertrain powertrain;

        private Rigidbody body;
        private float steering;
        private float throttle;
        private float brake;
        private float clutchPedal;
        private bool crashLocked;

        public TruckDashboardTelemetry Telemetry { get; } = new TruckDashboardTelemetry();
        public float SpeedKmh => body != null ? body.linearVelocity.magnitude * 3.6f : 0f;
        public int CurrentGear => powertrain != null ? powertrain.Gear : 0;
        public bool SteeringLimitActive { get; private set; }
        public float AppliedSteeringAngle { get; private set; }
        public float SignedSpeedKmh => body != null
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
        public void SetCrashLock(bool locked) => crashLocked = locked;
        public void ResetCrashLock() => crashLocked = false;

        private void FixedUpdate()
        {
            if (!WheelsAssigned()) return;
            if (crashLocked)
            {
                frontLeft.steerAngle = 0f;
                frontRight.steerAngle = 0f;
                if (powertrain != null) powertrain.SetThrottle(0f);
                rearLeft.motorTorque = 0f;
                rearRight.motorTorque = 0f;
                frontLeft.brakeTorque = serviceBrakeTorque;
                frontRight.brakeTorque = serviceBrakeTorque;
                rearLeft.brakeTorque = serviceBrakeTorque;
                rearRight.brakeTorque = serviceBrakeTorque;
                return;
            }
            float speedMpsForSteering = Mathf.Abs(SignedSpeedKmh) / 3.6f;
            float requestedSteerAngle = steering * maxSteerAngle;
            float safeSteerAngle = VehicleSafetyMath.SafeSteeringAngleDegrees(
                speedMpsForSteering, wheelbaseMeters, maxLateralAccelerationMps2, maxSteerAngle);
            AppliedSteeringAngle = Mathf.Clamp(requestedSteerAngle, -safeSteerAngle, safeSteerAngle);
            SteeringLimitActive = Mathf.Abs(requestedSteerAngle) > safeSteerAngle + 0.1f;
            frontLeft.steerAngle = AppliedSteeringAngle;
            frontRight.steerAngle = AppliedSteeringAngle;

            float driveTorque = 0f;
            if (powertrain != null)
            {
                float selectedThrottle = Mathf.Clamp01(throttle);
                if (powertrain.Gear > 0 && SignedSpeedKmh >= maxForwardSpeedKmh) selectedThrottle = 0f;
                if (powertrain.Gear < 0 && SignedSpeedKmh <= -maxReverseSpeedKmh) selectedThrottle = 0f;
                if (powertrain.IsNeutral) selectedThrottle = Mathf.Clamp01(throttle);
                powertrain.SetThrottle(selectedThrottle);
                powertrain.SetWheelSpeed(SignedSpeedKmh);
                driveTorque = powertrain.GetWheelTorque(Time.fixedDeltaTime);
            }

            // Powertrain output is total axle torque; split it across the two driven rear wheels.
            rearLeft.motorTorque = driveTorque * 0.5f;
            rearRight.motorTorque = driveTorque * 0.5f;

            float appliedBrake = brake * serviceBrakeTorque;
            frontLeft.brakeTorque = appliedBrake;
            frontRight.brakeTorque = appliedBrake;
            rearLeft.brakeTorque = appliedBrake;
            rearRight.brakeTorque = appliedBrake;

            Telemetry.SpeedKmh = SpeedKmh;
            Telemetry.EngineRpm = powertrain != null ? powertrain.EngineRpm : 0f;
            Telemetry.Gear = powertrain != null ? powertrain.Gear : 0;
            Telemetry.ClutchPercent = clutchPedal * 100f;
            Telemetry.ThrottlePercent = throttle * 100f;
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