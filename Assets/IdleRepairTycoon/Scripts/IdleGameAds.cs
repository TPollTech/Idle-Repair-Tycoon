using System;
using UnityEngine;

#if USE_ADMOB
using GoogleMobileAds.Api;
#endif

namespace IdleRepairTycoon
{
    public interface IIdleAdsService
    {
        bool RewardedReady { get; }
        void Initialize();
        void ShowRewarded(Action onRewardEarned, Action<string> onFailed = null);
    }

    public sealed class FakeAdsService : IIdleAdsService
    {
        public bool RewardedReady => true;

        public void Initialize()
        {
            Debug.Log("[IdleRepairTycoon] Fake ads enabled. Rewarded ads always succeed in editor/test mode.");
        }

        public void ShowRewarded(Action onRewardEarned, Action<string> onFailed = null)
        {
            Debug.Log("[IdleRepairTycoon] Fake rewarded ad watched.");
            onRewardEarned?.Invoke();
        }
    }

#if USE_ADMOB
    public sealed class GoogleMobileAdsService : IIdleAdsService
    {
#if UNITY_ANDROID
        // ID de teste oficial para rewarded no Android. Troque por seu ID real apenas na publicação.
        private const string RewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";
#else
        private const string RewardedAdUnitId = "unused";
#endif

        private RewardedAd rewardedAd;
        private bool initialized;

        public bool RewardedReady => rewardedAd != null && rewardedAd.CanShowAd();

        public void Initialize()
        {
            if (initialized) return;
            initialized = true;

            MobileAds.Initialize(_ =>
            {
                Debug.Log("[IdleRepairTycoon] Google Mobile Ads initialized.");
                LoadRewarded();
            });
        }

        private void LoadRewarded()
        {
            if (rewardedAd != null)
            {
                rewardedAd.Destroy();
                rewardedAd = null;
            }

            RewardedAd.Load(RewardedAdUnitId, new AdRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning("[IdleRepairTycoon] Rewarded ad failed to load: " + error);
                    return;
                }

                rewardedAd = ad;
                rewardedAd.OnAdFullScreenContentClosed += LoadRewarded;
                rewardedAd.OnAdFullScreenContentFailed += adError =>
                {
                    Debug.LogWarning("[IdleRepairTycoon] Rewarded ad failed to show: " + adError);
                    LoadRewarded();
                };
            });
        }

        public void ShowRewarded(Action onRewardEarned, Action<string> onFailed = null)
        {
            if (!RewardedReady)
            {
                onFailed?.Invoke("Anúncio ainda não carregou.");
                LoadRewarded();
                return;
            }

            rewardedAd.Show(_ => onRewardEarned?.Invoke());
        }
    }
#endif

    public static class IdleAdsFactory
    {
        public static IIdleAdsService Create()
        {
#if USE_ADMOB && !UNITY_EDITOR
            return new GoogleMobileAdsService();
#else
            return new FakeAdsService();
#endif
        }
    }
}
