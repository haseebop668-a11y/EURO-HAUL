using UnityEngine;

namespace EuroHaul.Driving
{
    /// <summary>Prototype diesel engine, manual clutch and gearbox. Ratios are game tuning values.</summary>
    public sealed class TruckPowertrain : MonoBehaviour
    {
        [Header("Engine")]
        [SerializeField, Min(300f)] private float idleRpm = 650f;
        [SerializeField, Min(1000f)] private float redlineRpm = 2400f;
        [SerializeField, Min(100f)] private float maxTorqueNm = 1100f;
        [SerializeField, Min(0.1f)] private float engineResponse = 2.5f;
        [SerializeField, Min(0.1f)] private float engineBraking = 1.5f;

        [Header("Transmission")]
        [SerializeField] private float[] forwardGearRatios = { 7.2f, 4.8f, 3.2f, 2.2f, 1.55f, 1f, 0.78f };
        [SerializeField, Min(1f)] private float reverseRatio = 6.5f;
        [SerializeField, Min(1f)] private float finalDriveRatio = 3.7f;
        [SerializeField, Range(0.5f, 1f)] private float drivetrainEfficiency = 0.88f;
        [SerializeField, Range(0.05f, 2f)] private float clutchEngagementRate = 0.65f;
        [SerializeField, Range(0f, 1f)] private float clutchPedal;
        [SerializeField, Min(0f)] private float wheelRadiusMeters = 0.52f;
        [SerializeField, Min(0.1f)] private float shiftDelay = 0.25f;
        [SerializeField, Min(0f)] private float reverseEngagementMaxSpeedKmh = 3f;

        private float engineRpm;
        private float throttle;
        private float wheelSpeedKmh;
        private float shiftTimer;
        private float currentClutchEngagement = 1f;
        private int gearIndex = 2;

        public float EngineRpm => engineRpm;
        public float ClutchPedal => clutchPedal;
        public float ClutchEngagement => currentClutchEngagement;
        public int Gear => gearIndex == 0 ? -1 : gearIndex == 1 ? 0 : gearIndex - 1;
        public bool IsNeutral => gearIndex == 1;
        public bool IsShifting => shiftTimer > 0f;

        private void Awake()
        {
            engineRpm = idleRpm;
            gearIndex = Mathf.Clamp(gearIndex, 0, forwardGearRatios.Length + 1);
            currentClutchEngagement = 1f - clutchPedal;
        }

        public void SetThrottle(float value) => throttle = Mathf.Clamp01(value);
        public void SetClutchPedal(float value) => clutchPedal = Mathf.Clamp01(value);
        public void SetWheelSpeed(float signedSpeedKmh) => wheelSpeedKmh = signedSpeedKmh;

        public bool ShiftUp()
        {
            if (shiftTimer > 0f || clutchPedal < 0.65f) return false;
            if (gearIndex >= forwardGearRatios.Length + 1) return false;
            gearIndex++;
            shiftTimer = shiftDelay;
            return true;
        }

        public bool ShiftDown()
        {
            if (shiftTimer > 0f || clutchPedal < 0.65f || gearIndex <= 0) return false;
            if (gearIndex == 1 && Mathf.Abs(wheelSpeedKmh) > reverseEngagementMaxSpeedKmh) return false;
            gearIndex--;
            shiftTimer = shiftDelay;
            return true;
        }

        public bool SelectReverse()
        {
            if (clutchPedal < 0.65f || shiftTimer > 0f ||
                Mathf.Abs(wheelSpeedKmh) > reverseEngagementMaxSpeedKmh) return false;
            gearIndex = 0;
            shiftTimer = shiftDelay;
            return true;
        }

        public void SelectNeutral()
        {
            if (clutchPedal >= 0.65f && shiftTimer <= 0f)
            {
                gearIndex = 1;
                shiftTimer = shiftDelay;
            }
        }

        public float GetWheelTorque(float deltaTime)
        {
            shiftTimer = Mathf.Max(0f, shiftTimer - deltaTime);
            currentClutchEngagement = Mathf.MoveTowards(currentClutchEngagement,
                1f - clutchPedal, clutchEngagementRate * deltaTime);

            float ratio = CurrentRatio();
            float wheelRpm = Mathf.Abs(wheelSpeedKmh) / 3.6f /
                (2f * Mathf.PI * Mathf.Max(0.1f, wheelRadiusMeters)) * 60f;
            float coupledRpm = wheelRpm * Mathf.Abs(ratio * finalDriveRatio);
            float freeTargetRpm = Mathf.Lerp(idleRpm, redlineRpm, throttle);
            float targetRpm = IsNeutral || currentClutchEngagement < 0.02f || shiftTimer > 0f
                ? freeTargetRpm
                : Mathf.Clamp(Mathf.Max(idleRpm, coupledRpm), idleRpm, redlineRpm);

            engineRpm = Mathf.MoveTowards(engineRpm, targetRpm,
                (throttle > 0.02f ? engineResponse : engineBraking) * redlineRpm * deltaTime);
            engineRpm = Mathf.Clamp(engineRpm, idleRpm, redlineRpm);

            if (IsNeutral || shiftTimer > 0f || ratio == 0f) return 0f;
            float normalizedRpm = Mathf.InverseLerp(idleRpm, redlineRpm, engineRpm);
            float torqueFactor = normalizedRpm < 0.72f
                ? Mathf.Lerp(0.72f, 1f, normalizedRpm / 0.72f)
                : Mathf.Lerp(1f, 0.78f, (normalizedRpm - 0.72f) / 0.28f);
            float engineTorque = maxTorqueNm * torqueFactor * throttle;
            return engineTorque * ratio * finalDriveRatio * drivetrainEfficiency * currentClutchEngagement;
        }

        private float CurrentRatio()
        {
            if (gearIndex == 0) return -reverseRatio;
            if (gearIndex == 1) return 0f;
            int forwardIndex = gearIndex - 2;
            return forwardIndex >= 0 && forwardIndex < forwardGearRatios.Length
                ? forwardGearRatios[forwardIndex] : 0f;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            idleRpm = Mathf.Max(300f, idleRpm);
            redlineRpm = Mathf.Max(idleRpm + 500f, redlineRpm);
            wheelRadiusMeters = Mathf.Max(0.1f, wheelRadiusMeters);
            if (forwardGearRatios == null || forwardGearRatios.Length == 0)
                forwardGearRatios = new[] { 7.2f, 4.8f, 3.2f, 2.2f, 1.55f, 1f, 0.78f };
        }
#endif
    }
}