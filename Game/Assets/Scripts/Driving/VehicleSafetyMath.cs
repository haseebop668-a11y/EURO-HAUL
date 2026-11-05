using UnityEngine;

namespace EuroHaul.Driving
{
    /// <summary>Deterministic safety calculations. Inputs use SI units unless explicitly named.</summary>
    public static class VehicleSafetyMath
    {
        public static float KmhToMps(float kmh) => Mathf.Max(0f, kmh) / 3.6f;

        public static float BrakingDistance(float speedMps, float decelerationMps2, float reactionSeconds)
        {
            float speed = Mathf.Max(0f, speedMps);
            float decel = Mathf.Max(0.1f, decelerationMps2);
            float reaction = Mathf.Max(0f, reactionSeconds);
            return speed * reaction + (speed * speed) / (2f * decel);
        }

        public static float TimeToCollision(float closingSpeedMps, float gapMeters)
        {
            if (closingSpeedMps <= 0.05f || gapMeters <= 0f) return float.PositiveInfinity;
            return gapMeters / closingSpeedMps;
        }

        public static float SafeCurveSpeedKmh(float radiusMeters, float lateralAccelerationMps2)
        {
            if (radiusMeters <= 0f || lateralAccelerationMps2 <= 0f) return 0f;
            return Mathf.Sqrt(radiusMeters * lateralAccelerationMps2) * 3.6f;
        }

        public static bool IsSafeGap(float gapMeters, float requiredStoppingDistanceMeters, float safetyMarginMeters)
        {
            return gapMeters >= Mathf.Max(0f, requiredStoppingDistanceMeters) + Mathf.Max(0f, safetyMarginMeters);
        }
    }
}