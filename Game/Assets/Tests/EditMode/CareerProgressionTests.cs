using NUnit.Framework;
using EuroHaul.Career;

namespace EuroHaul.Tests
{
    public sealed class CareerProgressionTests
    {
        [Test]
        public void ExperienceLevelsUpAndCarriesRemainder()
        {
            var data = new CareerSaveData { driverLevel = 1, experience = 0 };
            int levels = CareerProgression.ApplyExperience(data, 125);
            Assert.That(levels, Is.EqualTo(1));
            Assert.That(data.driverLevel, Is.EqualTo(2));
            Assert.That(data.experience, Is.EqualTo(25));
        }

        [Test]
        public void DeliveryRewardNeverAddsNegativeCash()
        {
            var data = new CareerSaveData { gameCash = 1000, driverLevel = 1 };
            CareerProgression.ApplyDeliveryReward(data, -500, 0);
            Assert.That(data.gameCash, Is.EqualTo(1000));
            Assert.That(data.completedDeliveries, Is.EqualTo(1));
        }

        [Test]
        public void LevelRequirementIncreasesWithLevel()
        {
            Assert.That(CareerProgression.ExperienceRequiredForNextLevel(3),
                Is.GreaterThan(CareerProgression.ExperienceRequiredForNextLevel(1)));
        }
    }
}