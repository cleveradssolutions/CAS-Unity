#if UNITY_EDITOR || TARGET_OS_SIMULATOR
using System;
using UnityEngine;

namespace CAS.Unity
{
    internal class CASMediationManager : MonoBehaviour, IMediationManager
    {
        public AdFlags enabledFlags;
        public AdFlags loadedFlags = AdFlags.None;
        public AdFlags visibleTypes = AdFlags.None;
        private bool emulateTabletScreen = false;
        private bool bannerRequired = false;
        private GUIStyle btnStyle = null;
        private CASSettings settings;
        private InitCompleteAction initCompleteAction;
        private AdPosition _bannerPosition = AdPosition.Undefined;
        private AdSize _bannerSize = AdSize.Banner;

        public string managerID { get { return "Dummy"; } }
        public bool isTestAdMode { get { return true; } }

        public AdSize bannerSize
        {
            get { return _bannerSize; }
            set
            {
                if (value != 0)
                    _bannerSize = value;
            }
        }
        public AdPosition bannerPosition
        {
            get { return _bannerPosition; }
            set
            {
                if (value != AdPosition.Undefined)
                    _bannerPosition = value;
            }
        }

        #region Ad Events
        public event CASTypedEvent OnLoadedAd;
        public event CASTypedEventWithError OnFailedToLoadAd;
        public event Action OnBannerAdShown;
        public event CASEventWithError OnBannerAdFailedToShow;
        public event Action OnBannerAdClicked;
        public event Action OnBannerAdHidden;
        public event Action OnInterstitialAdShown;
        public event CASEventWithError OnInterstitialAdFailedToShow;
        public event Action OnInterstitialAdClicked;
        public event Action OnInterstitialAdClosed;
        public event Action OnRewardedAdShown;
        public event CASEventWithError OnRewardedAdFailedToShow;
        public event Action OnRewardedAdClicked;
        public event Action OnRewardedAdCompleted;
        public event Action OnRewardedAdClosed;
        #endregion

        public static IMediationManager CreateManager( AdFlags enableAd, InitCompleteAction initCompleteAction )
        {
            var obj = new GameObject( "CASMediationManager" );
            DontDestroyOnLoad( obj );
            var manager = obj.AddComponent<CASMediationManager>();
            manager.enabledFlags = enableAd;
            manager.initCompleteAction = initCompleteAction;
            manager.bannerSize = AdSize.Banner;
            manager.settings = CAS.MobileAds.settings as CASSettings;
            return manager;
        }

        #region IMediationManager implementation

        public string GetLastActiveMediation( AdType adType )
        {
            return "Dummy";
        }

        public void HideBanner()
        {
            bannerRequired = false;
            EventExecutor.Add( CallHideBanner );
        }

        public bool IsEnabledAd( AdType adType )
        {
            var flag = GetFlag( adType );
            return ( enabledFlags & flag ) == flag;
        }

        public bool IsReadyAd( AdType adType )
        {
            var flag = GetFlag( adType );
            return ( loadedFlags & flag ) == flag;
        }

        public void LoadAd( AdType adType )
        {
            switch (adType)
            {
                case AdType.Banner:
                    if (( loadedFlags & AdFlags.Banner ) != AdFlags.Banner)
                        Invoke( "DidBannerLoaded", 1.0f );
                    break;
                case AdType.Interstitial:
                    if (( loadedFlags & AdFlags.Interstitial ) != AdFlags.Interstitial)
                        Invoke( "DidInterstitialLoaded", 1.0f );
                    break;
                case AdType.Rewarded:
                    if (( loadedFlags & AdFlags.Rewarded ) != AdFlags.Rewarded)
                        Invoke( "DidRewardedLoaded", 1.0f );
                    break;
            }
        }

        public void SetEnableAd( AdType adType, bool enabled )
        {
            if (adType == AdType.Banner)
            {
                if (enabled)
                {
                    enabledFlags |= AdFlags.Banner;
                    if (bannerRequired)
                        EventExecutor.Add( CallShowBanner );
                }
                else
                {
                    EventExecutor.Add( CallHideBanner );
                    enabledFlags &= ~AdFlags.Banner;
                }
            }
            else
            {
                if (enabled)
                    enabledFlags |= GetFlag( adType );
                else
                    enabledFlags &= ~GetFlag( adType );
            }
        }

        public void ShowAd( AdType adType )
        {
            switch (adType)
            {
                case AdType.Banner:
                    EventExecutor.Add( CallShowBanner );
                    break;
                case AdType.Interstitial:
                    EventExecutor.Add( CallShowInterstitial );
                    break;
                case AdType.Rewarded:
                    EventExecutor.Add( CallShowRewarded );
                    break;
            }
        }
        #endregion

        #region Skip frame on call SDK
        private void CallHideBanner()
        {
            if (( visibleTypes & AdFlags.Banner ) == AdFlags.Banner)
            {
                visibleTypes &= ~AdFlags.Banner;
                CASFactory.ExecuteEvent( OnBannerAdHidden );
            }
        }

        private void CallShowBanner()
        {
            bannerRequired = true;
            if (( enabledFlags & AdFlags.Banner ) != AdFlags.Banner)
            {
                CASFactory.ExecuteEvent( OnBannerAdFailedToShow, "Manager is disabled!" );
            }
            else if (( loadedFlags & AdFlags.Banner ) != AdFlags.Banner)
            {
                CASFactory.ExecuteEvent( OnBannerAdFailedToShow, "NoFill" );
            }
            else
            {
                visibleTypes |= AdFlags.Banner;
                CASFactory.ExecuteEvent( OnBannerAdShown );
            }
        }

        private void CallShowInterstitial()
        {
            if (isFullscreenAdVisible)
                CASFactory.ExecuteEvent( OnInterstitialAdFailedToShow, "Ad already displayed." );
            else if (( enabledFlags & AdFlags.Interstitial ) != AdFlags.Interstitial)
                CASFactory.ExecuteEvent( OnInterstitialAdFailedToShow, "Manager is disabled!" );
            else if (settings.lastInterImpressionTimestamp + MobileAds.settings.interstitialInterval > Time.time)
                CASFactory.ExecuteEvent( OnInterstitialAdFailedToShow,
                    "The interval between impressions Ad has not yet passed." );
            else if (( loadedFlags & AdFlags.Interstitial ) != AdFlags.Interstitial)
                CASFactory.ExecuteEvent( OnInterstitialAdFailedToShow, "NoFill" );
            else
            {
                visibleTypes |= AdFlags.Interstitial;
                CASFactory.ExecuteEvent( OnInterstitialAdShown );
            }
        }

        private void CallShowRewarded()
        {
            if (isFullscreenAdVisible)
                CASFactory.ExecuteEvent( OnRewardedAdFailedToShow, "Ad already displayed." );
            else if (( enabledFlags & AdFlags.Rewarded ) != AdFlags.Rewarded)
                CASFactory.ExecuteEvent( OnRewardedAdFailedToShow, "Manager is disabled!" );
            else
            {
                visibleTypes |= AdFlags.Rewarded;
                CASFactory.ExecuteEvent( OnRewardedAdShown );
            }
        }
        #endregion

        #region MonoBehaviour implementation
        private void Start()
        {
            if (settings.loadingMode != LoadingManagerMode.Manual)
            {
                LoadAd( AdType.Banner );
                LoadAd( AdType.Interstitial );
                LoadAd( AdType.Rewarded );
            }
            CASFactory.ExecuteEvent( initCompleteAction, true, null );
        }

        public void OnGUI()
        {
            if (btnStyle == null)
                btnStyle = new GUIStyle( "Button" );
            btnStyle.fontSize = ( int )( Math.Min( Screen.width, Screen.height ) * 0.035f );

            OnGUIBannerAd();
            OnGUIInterstitialAd();
            OnGUIRewardedAd();
        }

        private void OnGUIBannerAd()
        {
            if (( visibleTypes & AdFlags.Banner ) == AdFlags.Banner)
            {
                Vector2 sizePX = CalculateAdSizeOnScreen( bannerSize );
                Vector2 position = CalculateAdPositionOnScreen( bannerPosition, sizePX );
                var rect = new Rect( position, sizePX );

                rect.height *= 0.65f;
                if (GUI.Button( rect, "CAS " + bannerSize.ToString() + " Ad", btnStyle ))
                    CASFactory.ExecuteEvent( OnBannerAdClicked );

                rect.y += rect.height;
                rect.height = sizePX.y * 0.35f;

                emulateTabletScreen = GUI.Toggle( rect, emulateTabletScreen,
                    emulateTabletScreen ? "Switch to phone" : "Switch to tablet", btnStyle );
            }
        }

        public Vector2 CalculateAdSizeOnScreen( AdSize adSize )
        {
            const float phoneScale = 640;
            const float tabletScale = 1024;
            float scale = Mathf.Max( Screen.width, Screen.height ) / ( emulateTabletScreen ? tabletScale : phoneScale );

            switch (adSize)
            {
                case AdSize.AdaptiveBanner:
                    return new Vector2( Screen.width, 50 * scale );
                case AdSize.SmartBanner:
                    if (emulateTabletScreen)
                        return new Vector2( 728.0f * scale, 90.0f * scale );
                    else
                        return new Vector2( 320.0f * scale, 50.0f * scale );
                case AdSize.Leaderboard:
                    return new Vector2( 728.0f * scale, 90.0f * scale );
                case AdSize.MediumRectangle:
                    return new Vector2( 300.0f * scale, 250.0f * scale );
                default:
                    return new Vector2( 320.0f * scale, 50.0f * scale );
            }
        }

        public Vector2 CalculateAdPositionOnScreen( AdPosition position, Vector2 pxSize )
        {
            Vector2 res = Vector2.zero;
            switch (position)
            {
                case AdPosition.BottomCenter:
                    res.y = Screen.height - pxSize.y;
                    res.x = Screen.width * 0.5f - pxSize.x * 0.5f;
                    break;
                case AdPosition.TopCenter:
                    res.x = Screen.width * 0.5f - pxSize.x * 0.5f;
                    break;
                case AdPosition.BottomLeft:
                    res.y = Screen.height - pxSize.y;
                    break;
                case AdPosition.BottomRight:
                    res.y = Screen.height - pxSize.y;
                    res.x = Screen.width - pxSize.x;
                    break;
                case AdPosition.TopRight:
                    res.x = Screen.width - pxSize.x;
                    break;
            }
            return res;
        }

        private void OnGUIInterstitialAd()
        {
            if (( visibleTypes & AdFlags.Interstitial ) == AdFlags.Interstitial)
            {
                if (GUI.Button( new Rect( 0, 0, Screen.width, Screen.height ), "Close\n\nCAS Interstitial Ad", btnStyle ))
                {
                    CASFactory.ExecuteEvent( OnInterstitialAdClicked );
                    settings.lastInterImpressionTimestamp = Time.time;
                    visibleTypes &= ~AdFlags.Interstitial;
                    loadedFlags &= ~AdFlags.Interstitial;
                    CASFactory.ExecuteEvent( OnFailedToLoadAd, ( int )AdType.Interstitial, "Please Load new Ad" );
                    if (settings.loadingMode != LoadingManagerMode.Manual)
                        LoadAd( AdType.Interstitial );
                    CASFactory.ExecuteEvent( OnInterstitialAdClosed );
                }
            }
        }

        private void OnGUIRewardedAd()
        {
            if (( visibleTypes & AdFlags.Rewarded ) == AdFlags.Rewarded)
            {
                float width = Screen.width;
                float height = Screen.height;
                if (GUI.Button( new Rect( 0, 0, width, height * 0.5f ), "Close\nCAS Rewarded Video Ad", btnStyle ))
                {
                    visibleTypes &= ~AdFlags.Rewarded;
                    loadedFlags &= ~AdFlags.Rewarded;
                    CASFactory.ExecuteEvent( OnFailedToLoadAd, ( int )AdType.Rewarded, "Please Load new Ad" );
                    if (settings.loadingMode != LoadingManagerMode.Manual)
                        LoadAd( AdType.Rewarded );
                    CASFactory.ExecuteEvent( OnRewardedAdClosed );
                }
                if (GUI.Button( new Rect( 0, height * 0.5f, width, height * 0.5f ), "Complete\nCAS Rewarded Video Ad", btnStyle ))
                {
                    CASFactory.ExecuteEvent( OnRewardedAdClicked );
                    CASFactory.ExecuteEvent( OnRewardedAdCompleted );
                    visibleTypes &= ~AdFlags.Rewarded;
                    loadedFlags &= ~AdFlags.Rewarded;
                    CASFactory.ExecuteEvent( OnFailedToLoadAd, ( int )AdType.Rewarded, "Please Load new Ad" );
                    if (settings.loadingMode != LoadingManagerMode.Manual)
                        LoadAd( AdType.Rewarded );
                    CASFactory.ExecuteEvent( OnRewardedAdClosed );
                }
            }
        }
        #endregion

        #region Private implementation
        private bool isFullscreenAdVisible
        {
            get
            {
                return ( visibleTypes & ( AdFlags.Interstitial | AdFlags.Rewarded ) ) != AdFlags.None;
            }
        }

        private void DidBannerLoaded()
        {
            loadedFlags |= AdFlags.Banner;
            CASFactory.ExecuteEvent( OnLoadedAd, ( int )AdType.Banner );
        }

        private void DidInterstitialLoaded()
        {
            loadedFlags |= AdFlags.Interstitial;
            CASFactory.ExecuteEvent( OnLoadedAd, ( int )AdType.Interstitial );
        }

        private void DidRewardedLoaded()
        {
            loadedFlags |= AdFlags.Rewarded;
            CASFactory.ExecuteEvent( OnLoadedAd, ( int )AdType.Rewarded );
        }

        private AdFlags GetFlag( AdType adType )
        {
            switch (adType)
            {
                case AdType.Banner:
                    return AdFlags.Banner;
                case AdType.Interstitial:
                    return AdFlags.Interstitial;
                case AdType.Rewarded:
                    return AdFlags.Rewarded;
                case AdType.Native:
                    return AdFlags.Native;
                case AdType.None:
                    return AdFlags.None;
                default:
                    throw new NotImplementedException( "Unknown adType " + adType.ToString() );
            }
        }
        #endregion
    }
}
#endif