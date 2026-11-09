using System;
using UnityEngine;

namespace EuroHaul.Monetization
{
    public enum RewardKind
    {
        SoftCurrency,
        FuelVoucher,
        CosmeticToken
    }

    public readonly struct RewardAdResult
    {
        public readonly bool Completed;
        public readonly bool RewardVerified;
        public readonly string TransactionId;

        public RewardAdResult(bool completed, bool rewardVerified, string transactionId)
        {
            Completed = completed;
            RewardVerified = rewardVerified;
            TransactionId = transactionId;
        }
    }

    /// <summary>
    /// Implement this adapter with a supported ad SDK. The adapter must only set
    /// RewardVerified after the SDK's reward callback / server-side verification succeeds.
    /// </summary>
    public interface IRewardedAdProvider
    {
        bool IsReady { get; }
        void ShowRewarded(Action<RewardAdResult> onFinished);
    }

    /// <summary>
    /// SDK-independent policy gate. It never fabricates impressions or rewards.
    /// Attach a real provider adapter after selecting and configuring an ad network.
    /// </summary>
    public sealed class RewardedAdService : MonoBehaviour
    {
        [SerializeField] private AdPolicy policy;
        [SerializeField] private bool consentGranted;
        [SerializeField] private bool gameplayIsDriving;

        private IRewardedAdProvider provider;
        private int rewardedAdsThisSession;
        private float lastRewardedAdTime = -999999f;
        private string lastGrantedTransactionId;

        public event Action<RewardKind, int> RewardGranted;
        public event Action<string> StatusChanged;

        public void ConfigureProvider(IRewardedAdProvider adProvider)
        {
            provider = adProvider;
        }

        public void SetConsent(bool granted)
        {
            consentGranted = granted;
            StatusChanged?.Invoke(granted ? "Ad consent granted" : "Ad consent declined");
        }

        public void SetGameplayDrivingState(bool isDriving)
        {
            gameplayIsDriving = isDriving;
        }

        public bool CanShowRewarded(out string reason)
        {
            if (policy == null) { reason = "Ad policy is not configured"; return false; }
            if (!policy.RewardedAdsEnabled) { reason = "Rewarded ads are disabled"; return false; }
            if (policy.RequireExplicitConsent && !consentGranted) { reason = "Player consent is required"; return false; }
            if (policy.BlockAdsDuringDriving && gameplayIsDriving) { reason = "Ads are paused during driving"; return false; }
            if (rewardedAdsThisSession >= policy.MaxRewardedAdsPerSession) { reason = "Session ad limit reached"; return false; }
            if (Time.unscaledTime - lastRewardedAdTime < policy.MinimumSecondsBetweenRewardedAds)
            { reason = "Please wait before requesting another ad"; return false; }
            if (provider == null || !provider.IsReady) { reason = "No rewarded ad is currently ready"; return false; }
            reason = string.Empty;
            return true;
        }

        /// <summary>Player must explicitly request an ad. No automatic playback.</summary>
        public bool TryShowRewarded(RewardKind rewardKind, int rewardAmount)
        {
            if (rewardAmount <= 0 || !CanShowRewarded(out string reason))
            {
                StatusChanged?.Invoke(string.IsNullOrEmpty(reason) ? "Invalid reward amount" : reason);
                return false;
            }

            lastRewardedAdTime = Time.unscaledTime;
            rewardedAdsThisSession++;
            provider.ShowRewarded(result =>
            {
                if (!result.Completed || !result.RewardVerified ||
                    string.IsNullOrWhiteSpace(result.TransactionId))
                {
                    StatusChanged?.Invoke("Ad not completed or reward not verified");
                    return;
                }

                if (string.Equals(lastGrantedTransactionId, result.TransactionId, StringComparison.Ordinal))
                {
                    StatusChanged?.Invoke("Duplicate reward callback ignored");
                    return;
                }

                lastGrantedTransactionId = result.TransactionId;
                RewardGranted?.Invoke(rewardKind, rewardAmount);
                StatusChanged?.Invoke("Reward granted");
            });
            return true;
        }
    }
}