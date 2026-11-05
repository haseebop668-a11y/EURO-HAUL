using UnityEngine;

namespace EuroHaul.Driving
{
    /// <summary>First-pass truck controller for Truck Lab. Assign four WheelColliders in the Inspector.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class TruckController : MonoBehaviour
    {
        [Header("Wheel Colliders")]
        [SerializeField] private WheelCollider frontLeft;
        [SerializeField] private WheelCollider frontRight;
        [SerializeField] private WheelCollider rearLeft;
        [SerializeField] private WheelCollider rearRight;

        [Header("Vehicle Tuning")]
        [SerializeField, Min(100f)] private float motorTorque = 1800f;
        [SerializeField, Range(5f, 35f)] private float maxSteerAngle = 28f;
        [SerializeField, Min(100f)] private float serviceBrakeTorque = 4500f;
        [SerializeField, Min(5f)] private float maxForwardSpeedKmh = 90f;
        [SerializeField, Min(5f)] private float maxReverseSpeedKmh = 15f;
        [SerializeField] private Vector3 centerOfMassOffset = new Vector3(0f, -0.65f, 0f);

        private Rigidbody body;
        private float steering;
        private float throttle;
        private float brake;

        public float SpeedKmh => body != null ? body.linearVelocity.magnitude * 3.6f : 0f;
        public float SignedSpeedKmh => body != null
            ? Vector3.Dot(body.linearVelocity, transform.forward) * 3.6f
            : 0f;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.centerOfMass = centerOfMassOffset;
            body.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public void SetInput(float steeringInput, float throttleInput, float brakeInput)
        {
            steering = Mathf.Clamp(steeringInput, -1f, 1f);
            throttle = Mathf.Clamp(throttleInput, -1f, 1f);
            brake = Mathf.Clamp01(brakeInput);
        }

        private void FixedUpdate()
        {
            if (!WheelsAssigned()) return;
            frontLeft.steerAngle = steering * maxSteerAngle;
            frontRight.steerAngle = steering * maxSteerAngle;

            float requestedThrottle = throttle;
            float signedSpeed = SignedSpeedKmh;
            if (requestedThrottle > 0f && signedSpeed >= maxForwardSpeedKmh) requestedThrottle = 0f;
            if (requestedThrottle < 0f && signedSpeed <= -maxReverseSpeedKmh) requestedThrottle = 0f;

            rearLeft.motorTorque = requestedThrottle * motorTorque;
            rearRight.motorTorque = requestedThrottle * motorTorque;

            float appliedBrake = brake * serviceBrakeTorque;
            frontLeft.brakeTorque = appliedBrake;
            frontRight.brakeTorque = appliedBrake;
            rearLeft.brakeTorque = appliedBrake;
            rearRight.brakeTorque = appliedBrake;
        }

        private bool WheelsAssigned()
        {
            return frontLeft != null && frontRight != null && rearLeft != null && rearRight != null;
        }

        private void OnDisable()
        {
            steering = 0f;
            throttle = 0f;
            brake = 1f;
            if (rearLeft != null) rearLeft.motorTorque = 0f;
            if (rearRight != null) rearRight.motorTorque = 0f;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            motorTorque = Mathf.Max(100f, motorTorque);
            serviceBrakeTorque = Mathf.Max(100f, serviceBrakeTorque);
            maxForwardSpeedKmh = Mathf.Max(5f, maxForwardSpeedKmh);
            maxReverseSpeedKmh = Mathf.Max(5f, maxReverseSpeedKmh);
        }
#endif
    }
}