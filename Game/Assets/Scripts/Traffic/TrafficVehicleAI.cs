using UnityEngine;
namespace EuroHaul.Traffic
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class TrafficVehicleAI : MonoBehaviour
    {
        [SerializeField] private TrafficRoute route;
        [SerializeField] private WheelCollider frontLeft, frontRight, rearLeft, rearRight;
        [SerializeField] private float targetSpeedKmh = 60f;
        [SerializeField] private float maxSteerAngle = 25f;
        [SerializeField] private float motorTorque = 1200f;
        [SerializeField] private float brakeTorque = 4500f;
        [SerializeField] private float waypointDistance = 8f;\n        [SerializeField] private LayerMask vehicleMask = ~0;\n        [SerializeField] private float sensorRange = 55f;\n        [SerializeField] private float standstillGap = 6f;\n        [SerializeField] private float timeHeadway = 2f;
        private Rigidbody body;
        private int index;
        public float SpeedKmh => body == null ? 0f : body.linearVelocity.magnitude * 3.6f;
        private void Awake() { body = GetComponent<Rigidbody>(); body.interpolation = RigidbodyInterpolation.Interpolate; }
        private void FixedUpdate()
        {
            if (route == null || route.Count == 0 || frontLeft == null || frontRight == null || rearLeft == null || rearRight == null) { Drive(0f, 1f, 0f); return; }
            Transform point = route.GetWaypoint(index);
            if (point == null) return;
            if (Vector3.Distance(transform.position, point.position) < waypointDistance) index = route.Loop ? (index + 1) % route.Count : Mathf.Min(index + 1, route.Count - 1);
            point = route.GetWaypoint(index);
            Vector3 local = transform.InverseTransformPoint(point.position);
            float steer = Mathf.Clamp(local.x / Mathf.Max(4f, local.magnitude), -1f, 1f);
            float targetSpeed = targetSpeedKmh;\n            Vector3 sensorOrigin = transform.position + transform.forward * 2f + Vector3.up;\n            if (Physics.SphereCast(sensorOrigin, 0.85f, transform.forward, out RaycastHit hit, sensorRange, vehicleMask, QueryTriggerInteraction.Ignore) && hit.rigidbody != body)\n            {\n                float safeGap = standstillGap + (SpeedKmh / 3.6f) * timeHeadway;\n                if (hit.distance < safeGap) targetSpeed = Mathf.Min(targetSpeed, Mathf.Max(0f, SpeedKmh - (safeGap - hit.distance) * 3f));\n            }\n            float speedError = targetSpeed - SpeedKmh;
            float throttle = Mathf.Clamp01(speedError * 0.08f);
            float brake = Mathf.Clamp01(-speedError * 0.12f);
            Drive(throttle, brake, steer);
        }
        private void Drive(float throttle, float brake, float steer)
        {
            if (frontLeft == null || frontRight == null || rearLeft == null || rearRight == null) return;
            frontLeft.steerAngle = steer * maxSteerAngle; frontRight.steerAngle = steer * maxSteerAngle;
            rearLeft.motorTorque = Mathf.Clamp01(throttle) * motorTorque; rearRight.motorTorque = Mathf.Clamp01(throttle) * motorTorque;
            float b = Mathf.Clamp01(brake) * brakeTorque;
            frontLeft.brakeTorque = b; frontRight.brakeTorque = b; rearLeft.brakeTorque = b; rearRight.brakeTorque = b;
        }
        private void OnDisable() { Drive(0f, 1f, 0f); }
    }
}