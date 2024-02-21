using UnityEngine;
using UnityEngine.UI;
using CAS;
using System.Collections.Generic;

public class CSharpSampleController : MonoBehaviour
{
    public Text versionText;
    public Text interstitialStatus;
    public Text rewardedStatus;

    public Text appReturnButtonText;

    private bool isAppReturnEnable = false;
    private bool userRewardEarned = false;

    private IMediationManager manager;
    private IAdView bannerView;

    public void Start()
    {
        versionText.text = MobileAds.wrapperVersion;

        // -- Create manager:
        manager = MobileAds.BuildManager().Build();

        CreateBanner(AdSize.Banner);

        // -- Subscribe to CAS events:
        manager.OnInterstitialAdLoaded += OnInterstitialAdLoaded;
        manager.OnInterstitialAdFailedToLoad += OnInterstitialAdFailedToLoad;

        manager.OnRewardedAdLoaded += OnRewardedAdLoaded;
        manager.OnRewardedAdFailedToLoad += OnRewardedAdFailedToLoad;
        manager.OnRewardedAdCompleted += OnRewardedAdCompleted;
        manager.OnRewardedAdClosed += OnRewardedAdClosed;
        // Any other events in IMediationManager
    }

    private void OnDestroy()
    {
        // -- Unsubscribe from CAS events:
        manager.OnInterstitialAdLoaded -= OnInterstitialAdLoaded;
        manager.OnInterstitialAdFailedToLoad -= OnInterstitialAdFailedToLoad;

        manager.OnRewardedAdLoaded -= OnRewardedAdLoaded;
        manager.OnRewardedAdFailedToLoad -= OnRewardedAdFailedToLoad;
        manager.OnRewardedAdCompleted -= OnRewardedAdCompleted;
        manager.OnRewardedAdClosed += OnRewardedAdClosed;
    }

    private void CreateBanner(AdSize size)
    {
        HideBanner();
        bannerView = manager.GetAdView(size);
        ShowBanner();
    }

    public void ShowBanner()
    {
        bannerView.SetActive(true);
    }

    public void HideBanner()
    {
        if (bannerView != null)
            bannerView.SetActive(false);
    }

    public void SetBannerSize(int sizeID)
    {
        CreateBanner((AdSize)sizeID);
    }

    public void SetBannerPosition(int positionEnum)
    {
        bannerView.position = (AdPosition)positionEnum;
    }

    public void ShowInterstitial()
    {
        manager.ShowAd(AdType.Interstitial);
    }

    public void ShowRewarded()
    {
        userRewardEarned = false;
        manager.ShowAd(AdType.Rewarded);
    }

    public void ChangeAppReturnState()
    {
        isAppReturnEnable = !isAppReturnEnable;
        appReturnButtonText.text = isAppReturnEnable ? "ENABLED" : "DISABLED";
        manager.SetAppReturnAdsEnabled(isAppReturnEnable);
    }


    #region Events

    private void OnRewardedAdCompleted()
    {
        userRewardEarned = true;
        rewardedStatus.text = "User reward earned";
    }

    private void OnRewardedAdClosed()
    {
        if (!userRewardEarned)
            rewardedStatus.text = "User are not rewarded";
    }

    private void OnRewardedAdLoaded()
    {
        rewardedStatus.text = "Ready";
    }

    private void OnRewardedAdFailedToLoad(AdError error)
    {
        rewardedStatus.text = error.GetMessage();
    }

    private void OnInterstitialAdFailedToLoad(AdError error)
    {
        interstitialStatus.text = error.GetMessage();
    }

    private void OnInterstitialAdLoaded()
    {
        interstitialStatus.text = "Ready";
    }
    #endregion
}
