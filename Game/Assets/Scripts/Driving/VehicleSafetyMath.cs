using UnityEngine;

namespace EuroHaul.Driving
{
    /// <summary>Deterministic vehicle safety calculations. SI units unless named otherwise.</summary>
    public static class VehicleSafetyMath
    {
        public static float KmhToMps(float kmh) => Mathf.Max(0f, kmh) / 3.6f;

        public static float BrakingDistance(float speedMps, float decelerationMps2, float reactionSeconds)
        {
            float speed = Mathf.Max(0f, speedMps);
            float decel = Mathf.Max(0.1f, decelerationMps2);
            float reaction = Mathf.Max(0f, reactionSeconds);
            return speed * reaction + speed * speed / (2f * decel);
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

        public static float SafeSteeringAngleDegrees(float speedMps, float wheelbaseMeters,
            float lateralAccelerationMps2, float maximumSteeringAngleDegrees)
        {
            float maximum = Mathf.Max(0f, maximumSteeringAngleDegrees);
            if (speedMps <= 1f) return maximum;
            float wheelbase = Mathf.Max(0.1f, wheelbaseMeters);
            float lateralLimit = Mathf.Max(0.1f, lateralAccelerationMps2);
            float angle = Mathf.Atan(lateralLimit * wheelbase / (speedMps * speedMps)) * Mathf.Rad2Deg;
            return Mathf.Min(maximum, angle);
        }

        public static bool IsSafeGap(float gapMeters, float requiredStoppingDistanceMeters, float safetyMarginMeters)
        {
            return gapMeters >= Mathf.Max(0f, requiredStoppingDistanceMeters) + Mathf.Max(0f, safetyMarginMeters);
        }
    }
}