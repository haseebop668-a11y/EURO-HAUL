using UnityEngine;
using UnityEngine.Events;

namespace EuroHaul.Driving
{
    /// <summary>Driver advisory only: warns about hazards but never silently takes steering control.</summary>
    public sealed class TruckSafetyAdvisor : MonoBehaviour
    {
        [SerializeField] private TruckController truck;
        [SerializeField] private TruckTrafficSensors sensors;
        [SerializeField, Min(0.5f)] private float reactionTimeSeconds = 1.8f;
        [SerializeField, Min(1f)] private float assumedDecelerationMps2 = 3.5f;
        [SerializeField, Min(0f)] private float stoppingSafetyMarginMeters = 8f;
        [SerializeField, Min(0f)] private float curveRadiusMeters;
        [SerializeField, Range(0.5f, 4f)] private float safeLateralAcceleration = 1.5f;
        [SerializeField] private UnityEvent onFrontCollisionWarning;
        [SerializeField] private UnityEvent onBlindSpotWarning;
        [SerializeField] private UnityEvent onUnsafeCurveWarning;
        [SerializeField] private UnityEvent onFastRearApproachWarning;

        private bool frontWarned;
        private bool blindWarned;
        private bool curveWarned;
        private bool rearWarned;

        public float RequiredStoppingDistanceMeters { get; private set; }
        public float RecommendedCurveSpeedKmh { get; private set; }
        public bool FrontCollisionRisk { get; private set; }
        public bool BlindSpotRisk { get; private set; }
        public bool CurveSpeedRisk { get; private set; }
        public bool FastRearApproachRisk { get; private set; }
        public bool SteeringLimitRisk { get; private set; }
        public bool CanOvertakeLeft => sensors != null && sensors.CanSafelyChangeLeft;
        public bool CanOvertakeRight => sensors != null && sensors.CanSafelyChangeRight;

        private void Awake()
        {
            if (truck == null) truck = GetComponentInParent<TruckController>();
            if (sensors == null) sensors = GetComponentInParent<TruckTrafficSensors>();
        }

        private void Update()
        {
            if (truck == null) return;
            SteeringLimitRisk = truck.SteeringLimitActive;
            float speedMps = VehicleSafetyMath.KmhToMps(truck.SpeedKmh);
            RequiredStoppingDistanceMeters = VehicleSafetyMath.BrakingDistance(
                speedMps, assumedDecelerationMps2, reactionTimeSeconds) + stoppingSafetyMarginMeters;

            FrontCollisionRisk = sensors != null && sensors.FrontGapMeters < RequiredStoppingDistanceMeters;
            if (FrontCollisionRisk && !frontWarned) onFrontCollisionWarning?.Invoke();
            frontWarned = FrontCollisionRisk;

            BlindSpotRisk = sensors != null &&
                ((sensors.LeftBlindSpotOccupied && !sensors.CanSafelyChangeLeft) ||
                 (sensors.RightBlindSpotOccupied && !sensors.CanSafelyChangeRight));
            if (BlindSpotRisk && !blindWarned) onBlindSpotWarning?.Invoke();
            blindWarned = BlindSpotRisk;

            RecommendedCurveSpeedKmh = curveRadiusMeters > 0f
                ? VehicleSafetyMath.SafeCurveSpeedKmh(curveRadiusMeters, safeLateralAcceleration)
                : float.PositiveInfinity;
            CurveSpeedRisk = !float.IsPositiveInfinity(RecommendedCurveSpeedKmh) &&
                             truck.SpeedKmh > RecommendedCurveSpeedKmh;
            if (CurveSpeedRisk && !curveWarned) onUnsafeCurveWarning?.Invoke();
            curveWarned = CurveSpeedRisk;

            FastRearApproachRisk = sensors != null && sensors.RearGapMeters < 35f &&
                VehicleSafetyMath.TimeToCollision(sensors.RearClosingSpeedMps, sensors.RearGapMeters) < 4f;
            if (FastRearApproachRisk && !rearWarned) onFastRearApproachWarning?.Invoke();
            rearWarned = FastRearApproachRisk;
        }

        public void SetCurveRadius(float radiusMeters)
        {
            curveRadiusMeters = Mathf.Max(0f, radiusMeters);
        }

        public void SetCurveRadius(float radiusMeters, float lateralAccelerationMps2)
        {
            curveRadiusMeters = Mathf.Max(0f, radiusMeters);
            safeLateralAcceleration = Mathf.Clamp(lateralAccelerationMps2, 0.5f, 4f);
        }
    }
}