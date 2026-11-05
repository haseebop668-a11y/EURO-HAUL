using UnityEngine;

namespace EuroHaul.Driving
{
    public enum TrafficAlertLevel { Clear, Caution, Warning, Critical }

    /// <summary>Front, rear, blind-spot and rear-quarter awareness sensors.</summary>
    public sealed class TruckTrafficSensors : MonoBehaviour
    {
        [SerializeField] private Transform forwardOrigin;
        [SerializeField] private Transform rearOrigin;
        [SerializeField] private Transform leftBlindSpotOrigin;
        [SerializeField] private Transform rightBlindSpotOrigin;
        [SerializeField] private Transform leftRearApproachOrigin;
        [SerializeField] private Transform leftFrontApproachOrigin;
        [SerializeField] private Transform rightRearApproachOrigin;
        [SerializeField] private Transform rightFrontApproachOrigin;
        [SerializeField] private LayerMask trafficMask = ~0;
        [SerializeField, Min(5f)] private float forwardRange = 100f;
        [SerializeField, Min(5f)] private float rearRange = 60f;
        [SerializeField, Min(1f)] private float blindSpotRange = 12f;
        [SerializeField, Min(0.1f)] private float sensorRadius = 1.2f;

        private readonly RaycastHit[] hitBuffer = new RaycastHit[24];

        public float FrontGapMeters { get; private set; } = float.PositiveInfinity;
        public float RearGapMeters { get; private set; } = float.PositiveInfinity;
        public float FrontClosingSpeedMps { get; private set; }
        public float RearClosingSpeedMps { get; private set; }
        public bool LeftBlindSpotOccupied { get; private set; }
        public bool RightBlindSpotOccupied { get; private set; }
        public float LeftRearGapMeters { get; private set; } = float.PositiveInfinity;
        public float RightRearGapMeters { get; private set; } = float.PositiveInfinity;
        public float LeftRearClosingSpeedMps { get; private set; }
        public float LeftFrontGapMeters { get; private set; } = float.PositiveInfinity;
        public float LeftFrontClosingSpeedMps { get; private set; }
        public float RightRearClosingSpeedMps { get; private set; }
        public float RightFrontGapMeters { get; private set; } = float.PositiveInfinity;
        public float RightFrontClosingSpeedMps { get; private set; }
        public TrafficAlertLevel FrontAlert { get; private set; }
        public TrafficAlertLevel RearAlert { get; private set; }

        public bool CanSafelyChangeLeft =>
            !LeftBlindSpotOccupied && LeftRearGapMeters > 18f && LeftFrontGapMeters > 45f &&
            VehicleSafetyMath.TimeToCollision(LeftRearClosingSpeedMps, LeftRearGapMeters) > 5f &&
            VehicleSafetyMath.TimeToCollision(LeftFrontClosingSpeedMps, LeftFrontGapMeters) > 8f;

        public bool CanSafelyChangeRight =>
            !RightBlindSpotOccupied && RightRearGapMeters > 18f && RightFrontGapMeters > 45f &&
            VehicleSafetyMath.TimeToCollision(RightRearClosingSpeedMps, RightRearGapMeters) > 5f &&
            VehicleSafetyMath.TimeToCollision(RightFrontClosingSpeedMps, RightFrontGapMeters) > 8f;

        private void FixedUpdate()
        {
            FrontGapMeters = SenseGap(forwardOrigin, transform.forward, forwardRange, out FrontClosingSpeedMps);
            RearGapMeters = SenseGap(rearOrigin, -transform.forward, rearRange, out RearClosingSpeedMps);
            LeftBlindSpotOccupied = SenseOccupied(leftBlindSpotOrigin, blindSpotRange);
            RightBlindSpotOccupied = SenseOccupied(rightBlindSpotOrigin, blindSpotRange);
            LeftRearGapMeters = SenseGap(leftRearApproachOrigin,
                leftRearApproachOrigin != null ? leftRearApproachOrigin.forward : -transform.forward,
                rearRange, out LeftRearClosingSpeedMps);
            RightRearGapMeters = SenseGap(rightRearApproachOrigin,
                rightRearApproachOrigin != null ? rightRearApproachOrigin.forward : -transform.forward,
                rearRange, out RightRearClosingSpeedMps);
            LeftFrontGapMeters = SenseGap(leftFrontApproachOrigin,
                leftFrontApproachOrigin != null ? leftFrontApproachOrigin.forward : transform.forward,
                forwardRange, out LeftFrontClosingSpeedMps);
            RightFrontGapMeters = SenseGap(rightFrontApproachOrigin,
                rightFrontApproachOrigin != null ? rightFrontApproachOrigin.forward : transform.forward,
                forwardRange, out RightFrontClosingSpeedMps);
            FrontAlert = Classify(FrontGapMeters, FrontClosingSpeedMps);
            RearAlert = Classify(RearGapMeters, RearClosingSpeedMps);
        }

        private float SenseGap(Transform origin, Vector3 direction, float range, out float closingSpeed)
        {
            closingSpeed = 0f;
            if (origin == null) return float.PositiveInfinity;
            int count = Physics.SphereCastNonAlloc(origin.position, sensorRadius, direction, hitBuffer,
                range, trafficMask, QueryTriggerInteraction.Ignore);
            Rigidbody own = GetComponentInParent<Rigidbody>();
            float nearest = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = hitBuffer[i];
                Rigidbody other = hit.rigidbody;
                if (other != null && other == own) continue;
                if (hit.distance >= nearest) continue;
                nearest = hit.distance;
                closingSpeed = 0f;
                if (other != null && own != null)
                {
                    Vector3 relativeVelocity = other.linearVelocity - own.linearVelocity;
                    closingSpeed = Vector3.Dot(-relativeVelocity, direction.normalized);
                }
            }
            return nearest;
        }

        private bool SenseOccupied(Transform origin, float range)
        {
            if (origin == null) return false;
            int count = Physics.SphereCastNonAlloc(origin.position, sensorRadius, origin.forward,
                hitBuffer, range, trafficMask, QueryTriggerInteraction.Ignore);
            Rigidbody own = GetComponentInParent<Rigidbody>();
            for (int i = 0; i < count; i++)
            {
                Rigidbody other = hitBuffer[i].rigidbody;
                if (other == null || other != own) return true;
            }
            return false;
        }

        private static TrafficAlertLevel Classify(float gap, float closingSpeed)
        {
            if (float.IsPositiveInfinity(gap)) return TrafficAlertLevel.Clear;
            float ttc = VehicleSafetyMath.TimeToCollision(closingSpeed, gap);
            if (ttc < 1.2f || gap < 5f) return TrafficAlertLevel.Critical;
            if (ttc < 2.5f || gap < 12f) return TrafficAlertLevel.Warning;
            if (ttc < 4f || gap < 25f) return TrafficAlertLevel.Caution;
            return TrafficAlertLevel.Clear;
        }
    }
}