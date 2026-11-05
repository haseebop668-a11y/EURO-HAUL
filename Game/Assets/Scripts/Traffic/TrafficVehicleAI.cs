using UnityEngine;
using UnityEngine.Events;
using EuroHaul.Audio;

namespace EuroHaul.Traffic
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class TrafficVehicleAI : MonoBehaviour
    {
        [SerializeField] private TrafficRoute route;
        [SerializeField] private WheelCollider frontLeft, frontRight, rearLeft, rearRight;
        [SerializeField] private Transform frontSensorOrigin;
        [SerializeField] private LayerMask vehicleMask = ~0;
        [SerializeField] private float targetSpeedKmh = 60f;
        [SerializeField] private float maxSteerAngle = 25f;
        [SerializeField] private float wheelbaseMeters = 4.2f;
        [SerializeField] private float maxLateralAccelerationMps2 = 2.2f;
        [SerializeField] private float motorTorque = 1200f;
        [SerializeField] private float brakeTorque = 4500f;
        [SerializeField] private float waypointDistance = 8f;
        [SerializeField] private float sensorRange = 55f;
        [SerializeField] private float standstillGap = 6f;
        [SerializeField] private float timeHeadway = 2f;
        [SerializeField] private float hornGapMeters = 10f;
        [SerializeField] private float hornCooldownSeconds = 7f;
        [SerializeField] private AudioSource hornAudio;
        [SerializeField] private UnityEvent onHornWarning;

        private readonly RaycastHit[] hitBuffer = new RaycastHit[16];
        private Rigidbody body;
        private int index;
        private float nextHornTime;

        public float SpeedKmh => body == null ? 0f : body.linearVelocity.magnitude * 3.6f;
        public float ObstacleGapMeters { get; private set; } = float.PositiveInfinity;
        public bool IsFollowingSlowTraffic { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.interpolation = RigidbodyInterpolation.Interpolate;
            if (hornAudio == null) hornAudio = GetComponent<AudioSource>();
            if (hornAudio == null) hornAudio = gameObject.AddComponent<AudioSource>();
            if (hornAudio.clip == null) hornAudio.clip = PrototypeHornAudio.CreateClip("Traffic Courtesy Horn");
            hornAudio.spatialBlend = 1f;
            FindNearestWaypoint();
        }

        private void FixedUpdate()
        {
            if (route == null || route.Count == 0 || !WheelsReady())
            {
                Drive(0f, 1f, 0f);
                return;
            }

            AdvanceWaypoint();
            Transform point = route.GetWaypoint(index);
            if (point == null) return;

            Vector3 local = transform.InverseTransformPoint(point.position);
            float steer = Mathf.Clamp(local.x / Mathf.Max(4f, local.magnitude), -1f, 1f);
            float desiredSpeed = Mathf.Min(targetSpeedKmh, CurveSpeedLimit());
            SenseFrontVehicle(out float gap, out float obstacleSpeed);
            ObstacleGapMeters = gap;
            IsFollowingSlowTraffic = !float.IsPositiveInfinity(gap) && gap < standstillGap + SpeedKmh / 3.6f * timeHeadway;

            if (IsFollowingSlowTraffic)
            {
                float speedMps = SpeedKmh / 3.6f;
                float safeGap = standstillGap + speedMps * timeHeadway;
                float closingSpeed = Mathf.Max(0f, speedMps - obstacleSpeed);
                float dynamicGap = safeGap + speedMps * closingSpeed / 4f;
                if (gap < dynamicGap)
                    desiredSpeed = Mathf.Min(desiredSpeed,
                        Mathf.Max(0f, obstacleSpeed * 3.6f - (dynamicGap - gap) * 1.8f));
            }

            if (gap < hornGapMeters && SpeedKmh > 8f && Time.time >= nextHornTime)
            {
                nextHornTime = Time.time + hornCooldownSeconds;
                if (hornAudio != null) hornAudio.Play();
                onHornWarning?.Invoke();
            }

            float speedError = desiredSpeed - SpeedKmh;
            float throttle = Mathf.Clamp01(speedError * 0.08f);
            float brake = Mathf.Clamp01(-speedError * 0.12f);
            if (gap < standstillGap) { throttle = 0f; brake = 1f; }
            Drive(throttle, brake, steer);
        }

        private void FindNearestWaypoint()
        {
            if (route == null || route.Count == 0) return;
            float nearest = float.PositiveInfinity;
            for (int i = 0; i < route.Count; i++)
            {
                Transform point = route.GetWaypoint(i);
                if (point == null) continue;
                float distance = (transform.position - point.position).sqrMagnitude;
                if (distance < nearest) { nearest = distance; index = i; }
            }
        }

        private void AdvanceWaypoint()
        {
            Transform point = route.GetWaypoint(index);
            if (point == null || Vector3.Distance(transform.position, point.position) > waypointDistance) return;
            int next = index + 1;
            if (next >= route.Count) next = route.Loop ? 0 : route.Count - 1;
            index = next;
        }

        private float CurveSpeedLimit()
        {
            Transform current = route.GetWaypoint(index);
            int nextIndex = index + 1;
            if (nextIndex >= route.Count) nextIndex = route.Loop ? 0 : index;
            Transform next = route.GetWaypoint(nextIndex);
            if (current == null || next == null || next == current) return targetSpeedKmh;
            Vector3 a = (current.position - transform.position).normalized;
            Vector3 b = (next.position - current.position).normalized;
            float angle = Vector3.Angle(a, b);
            float chord = Mathf.Max(5f, Vector3.Distance(current.position, next.position));
            float radius = angle < 2f ? 10000f :
                chord / Mathf.Max(0.05f, 2f * Mathf.Sin(angle * Mathf.Deg2Rad * 0.5f));
            return Mathf.Min(targetSpeedKmh, Mathf.Sqrt(radius * 1.25f) * 3.6f);
        }

        private void SenseFrontVehicle(out float gap, out float speedMps)
        {
            gap = float.PositiveInfinity;
            speedMps = 0f;
            Vector3 origin = frontSensorOrigin != null
                ? frontSensorOrigin.position
                : transform.position + transform.forward * 4.5f + Vector3.up;
            int count = Physics.SphereCastNonAlloc(origin, 0.9f, transform.forward,
                hitBuffer, sensorRange, vehicleMask, QueryTriggerInteraction.Ignore);
            float nearest = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = hitBuffer[i];
                if (hit.rigidbody == body || hit.distance >= nearest) continue;
                nearest = hit.distance;
                speedMps = hit.rigidbody != null
                    ? Vector3.Dot(hit.rigidbody.linearVelocity, transform.forward)
                    : 0f;
            }
            gap = nearest;
        }

        private void Drive(float throttle, float brake, float steer)
        {
            if (!WheelsReady()) return;
            float speedMps = SpeedKmh / 3.6f;
            float safeAngle = maxSteerAngle;
            if (speedMps > 1f)
                safeAngle = Mathf.Min(maxSteerAngle,
                    Mathf.Atan(maxLateralAccelerationMps2 * wheelbaseMeters / (speedMps * speedMps)) * Mathf.Rad2Deg);
            float appliedSteer = Mathf.Clamp(steer * maxSteerAngle, -safeAngle, safeAngle);
            frontLeft.steerAngle = appliedSteer;
            frontRight.steerAngle = appliedSteer;
            rearLeft.motorTorque = Mathf.Clamp01(throttle) * motorTorque;
            rearRight.motorTorque = Mathf.Clamp01(throttle) * motorTorque;
            float appliedBrake = Mathf.Clamp01(brake) * brakeTorque;
            frontLeft.brakeTorque = appliedBrake;
            frontRight.brakeTorque = appliedBrake;
            rearLeft.brakeTorque = appliedBrake;
            rearRight.brakeTorque = appliedBrake;
        }

        private bool WheelsReady() =>
            frontLeft != null && frontRight != null && rearLeft != null && rearRight != null;

        private void OnDisable() => Drive(0f, 1f, 0f);
    }
}