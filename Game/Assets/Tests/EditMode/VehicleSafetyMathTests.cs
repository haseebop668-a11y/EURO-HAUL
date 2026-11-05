using NUnit.Framework;
using EuroHaul.Driving;

namespace EuroHaul.Tests
{
    public sealed class VehicleSafetyMathTests
    {
        [Test]
        public void BrakingDistanceIncludesReactionAndPhysicalBraking()
        {
            float distance = VehicleSafetyMath.BrakingDistance(20f, 5f, 1.5f);
            Assert.That(distance, Is.EqualTo(70f).Within(0.001f));
        }

        [Test]
        public void CollisionTimeUsesClosingSpeed()
        {
            Assert.That(VehicleSafetyMath.TimeToCollision(5f, 10f), Is.EqualTo(2f).Within(0.001f));
            Assert.That(float.IsPositiveInfinity(VehicleSafetyMath.TimeToCollision(0f, 10f)), Is.True);
        }

        [Test]
        public void CurveSpeedUsesLateralAccelerationAndRadius()
        {
            Assert.That(VehicleSafetyMath.SafeCurveSpeedKmh(100f, 1f), Is.EqualTo(36f).Within(0.01f));
        }

        [Test]
        public void SteeringAngleShrinksAsSpeedIncreases()
        {
            float slowAngle = VehicleSafetyMath.SafeSteeringAngleDegrees(5f, 4.2f, 2.2f, 28f);
            float fastAngle = VehicleSafetyMath.SafeSteeringAngleDegrees(25f, 4.2f, 2.2f, 28f);
            Assert.That(slowAngle, Is.GreaterThan(fastAngle));
            Assert.That(fastAngle, Is.LessThan(2f));
        }

        [Test]
        public void SafeGapIncludesStoppingDistanceAndMargin()
        {
            Assert.That(VehicleSafetyMath.IsSafeGap(55f, 45f, 10f), Is.True);
            Assert.That(VehicleSafetyMath.IsSafeGap(54.9f, 45f, 10f), Is.False);
        }

        [Test]
        public void NegativeInputsAreClampedToSafeValues()
        {
            Assert.That(VehicleSafetyMath.BrakingDistance(-10f, -2f, -1f), Is.EqualTo(0f));
            Assert.That(VehicleSafetyMath.KmhToMps(-20f), Is.EqualTo(0f));
        }
    }
}