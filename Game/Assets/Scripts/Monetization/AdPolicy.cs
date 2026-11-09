using UnityEngine;

namespace EuroHaul.Monetization
{
    /// <summary>
    /// Runtime-safe ad policy defaults. Network-specific category controls must also be
    /// configured in the chosen ad network dashboard; this asset cannot guarantee ad content.
    /// </summary>
    [CreateAssetMenu(menuName = "Euro Haul/Monetization/Ad Policy", fileName = "AdPolicy")]
    public sealed class AdPolicy : ScriptableObject
    {
        [Header("Player experience")]
        [SerializeField] private bool rewardedAdsEnabled = true;
        [SerializeField] private bool interstitialAdsEnabled = false;
        [SerializeField, Min(0)] private int maxRewardedAdsPerSession = 5;
        [SerializeField, Min(0f)] private float minimumSecondsBetweenRewardedAds = 90f;
        [SerializeField, Min(0f)] private float interstitialCooldownSeconds = 600f;
        [SerializeField] private bool blockAdsDuringDriving = true;
        [SerializeField] private bool requireExplicitConsent = true;

        [Header("Content controls (also configure in the ad network dashboard)")]
        [SerializeField] private bool requestFamilySafeInventory = true;
        [SerializeField] private bool blockAlcohol = true;
        [SerializeField] private bool blockGambling = true;
        [SerializeField] private bool blockAdult = true;
        [SerializeField] private bool blockTobacco = true;
        [SerializeField] private bool blockDating = true;

        public bool RewardedAdsEnabled => rewardedAdsEnabled;
        public bool InterstitialAdsEnabled => interstitialAdsEnabled;
        public int MaxRewardedAdsPerSession => maxRewardedAdsPerSession;
        public float MinimumSecondsBetweenRewardedAds => minimumSecondsBetweenRewardedAds;
        public float InterstitialCooldownSeconds => interstitialCooldownSeconds;
        public bool BlockAdsDuringDriving => blockAdsDuringDriving;
        public bool RequireExplicitConsent => requireExplicitConsent;
        public bool RequestFamilySafeInventory => requestFamilySafeInventory;
        public bool BlockAlcohol => blockAlcohol;
        public bool BlockGambling => blockGambling;
        public bool BlockAdult => blockAdult;
        public bool BlockTobacco => blockTobacco;
        public bool BlockDating => blockDating;
    }
}