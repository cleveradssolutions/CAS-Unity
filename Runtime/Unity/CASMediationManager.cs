#if UNITY_EDITOR || TARGET_OS_SIMULATOR
using System;
using UnityEngine;

namespace CAS.Unity
{
    internal class CASMediationManager : MonoBehaviour, IMediationManager
    {
        public AdFlags enabledTypes;
        public AdFlags loadedTypes = AdFlags.None;
        public AdFlags visibleTypes = AdFlags.None;

        private bool _emulateTabletScreen = false;
        private bool _bannerRequired = false;
        private GUIStyle _btnStyle = null;
        private CASSettings _settings;
        private InitCompleteAction _initCompleteAction;
        private AdPosition _bannerPosition = AdPosition.BottomCenter;
        private AdSize _bannerSize = AdSize.Banner;
        private LastPageAdContent _lastPageAdContent;

        public string managerID { get { return "Dummy"; } }
        public bool isTestAdMode { get { return true; } }
        public LastPageAdContent lastPageAdContent
        {
            get { return _lastPageAdContent; }
            set
            {
                _lastPageAdContent = value;
                if (value == null)
                    Debug.Log( "CAS Last Page Ad content cleared" );
                else
                    Debug.Log( "CAS Last Page Ad apply content:" +
                        "\n- Headline:" + value.Headline +
                        "\n- DestinationURL:" + value.DestinationURL +
                        "\n- ImageURL:" + value.ImageURL +
                        "\n- IconURL:" + value.IconURL +
                        "\n- AdText:" + value.AdText );
            }
        }
        public AdSize bannerSize
        {
            get { return _bannerSize; }
            set
            {
                if (value == 0)
                    return;
                if (value <= AdSize.MediumRectangle)
                    _bannerSize = value;
                else
                    Debug.Log( "CAS change Banner size to unsupported value: " + value );
            }
        }
        public AdPosition bannerPosition
        {
            get { return _bannerPosition; }
            set
            {
                if (value < AdPosition.Undefined)
                    _bannerPosition = value;
                else if (value != AdPosition.Undefined)
                    Debug.Log( "CAS change Banner position to unsupported value: " + value );
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

        public static IMediationManager CreateManager( CASInitSettings initSettings )
        {
            var obj = new GameObject( "CASMediationManager" );
            DontDestroyOnLoad( obj );
            var manager = obj.AddComponent<CASMediationManager>();
            manager.enabledTypes = initSettings.allowedAdFlags;
            manager._initCompleteAction = initSettings.initListener;
            manager.bannerSize = AdSize.Banner;
            manager._settings = CAS.MobileAds.settings as CASSettings;
            return manager;
        }

        #region IMediationManager implementation

        public string GetLastActiveMediation( AdType adType )
        {
            return "Dummy";
        }

        public void HideBanner()
        {
            _bannerRequired = false;
            EventExecutor.Add( CallHideBanner );
        }

        public bool IsEnabledAd( AdType adType )
        {
            var flag = GetFlag( adType );
            return ( enabledTypes & flag ) == flag;
        }

        public bool IsReadyAd( AdType adType )
        {
            if (!IsEnabledAd( adType ))
                return false;
            if (adType == AdType.Interstitial
                && _settings.lastInterImpressionTimestamp + MobileAds.settings.interstitialInterval > Time.time)
                return false;
            var flag = GetFlag( adType );
            return ( loadedTypes & flag ) == flag;
        }

        public void LoadAd( AdType adType )
        {
            switch (adType)
            {
                case AdType.Banner:
                    if (( loadedTypes & AdFlags.Banner ) != AdFlags.Banner)
                        Invoke( "DidBannerLoaded", 1.0f );
                    break;
                case AdType.Interstitial:
                    if (( loadedTypes & AdFlags.Interstitial ) != AdFlags.Interstitial)
                        Invoke( "DidInterstitialLoaded", 1.0f );
                    break;
                case AdType.Rewarded:
                    if (( loadedTypes & AdFlags.Rewarded ) != AdFlags.Rewarded)
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
                    enabledTypes |= AdFlags.Banner;
                    if (_bannerRequired)
                        EventExecutor.Add( CallShowBanner );
                }
                else
                {
                    EventExecutor.Add( CallHideBanner );
                    enabledTypes &= ~AdFlags.Banner;
                }
            }
            else
            {
                if (enabled)
                    enabledTypes |= GetFlag( adType );
                else
                    enabledTypes &= ~GetFlag( adType );
            }
        }

        public void ShowAd( AdType adType )
        {
            switch (adType)
            {
                case AdType.Banner:
                    _bannerRequired = true;
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

        public float GetBannerHeightInPixels()
        {
            return CalculateAdSizeOnScreen( _bannerSize ).y;
        }

        public float GetBannerWidthInPixels()
        {
            return CalculateAdSizeOnScreen( _bannerSize ).x;
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
            if (( enabledTypes & AdFlags.Banner ) != AdFlags.Banner)
            {
                CASFactory.ExecuteEvent( OnBannerAdFailedToShow, "Manager is disabled!" );
            }
            else if (( loadedTypes & AdFlags.Banner ) != AdFlags.Banner)
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
            else if (( enabledTypes & AdFlags.Interstitial ) != AdFlags.Interstitial)
                CASFactory.ExecuteEvent( OnInterstitialAdFailedToShow, "Manager is disabled!" );
            else if (_settings.lastInterImpressionTimestamp + MobileAds.settings.interstitialInterval > Time.time)
                CASFactory.ExecuteEvent( OnInterstitialAdFailedToShow,
                    "The interval between impressions Ad has not yet passed." );
            else if (( loadedTypes & AdFlags.Interstitial ) != AdFlags.Interstitial)
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
            else if (( enabledTypes & AdFlags.Rewarded ) != AdFlags.Rewarded)
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
            if (_settings.loadingMode != LoadingManagerMode.Manual)
            {
                LoadAd( AdType.Banner );
                LoadAd( AdType.Interstitial );
                LoadAd( AdType.Rewarded );
            }
            CASFactory.ExecuteEvent( _initCompleteAction, true, null );
        }

        public void OnGUI()
        {
            if (_btnStyle == null)
                _btnStyle = new GUIStyle( "Button" );
            _btnStyle.fontSize = ( int )( Math.Min( Screen.width, Screen.height ) * 0.035f );

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
                if (GUI.Button( rect, "CAS " + bannerSize.ToString() + " Ad", _btnStyle ))
                    CASFactory.ExecuteEvent( OnBannerAdClicked );

                rect.y += rect.height;
                rect.height = sizePX.y * 0.35f;

                _emulateTabletScreen = GUI.Toggle( rect, _emulateTabletScreen,
                    _emulateTabletScreen ? "Switch to phone" : "Switch to tablet", _btnStyle );
            }
        }

        public Vector2 CalculateAdSizeOnScreen( AdSize adSize )
        {
            const float phoneScale = 640;
            const float tabletScale = 1024;
            float scale = Mathf.Max( Screen.width, Screen.height ) / ( _emulateTabletScreen ? tabletScale : phoneScale );

            switch (adSize)
            {
                case AdSize.AdaptiveBanner:
                    return new Vector2( Screen.width, 50 * scale );
                case AdSize.SmartBanner:
                    if (_emulateTabletScreen)
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
            switch (position)
            {
                case AdPosition.TopLeft:
                    return Vector2.zero;
                case AdPosition.TopCenter:
                    return new Vector2( Screen.width * 0.5f - pxSize.x * 0.5f, 0f );
                case AdPosition.TopRight:
                    return new Vector2( Screen.width - pxSize.x, 0f );
                case AdPosition.BottomLeft:
                    return new Vector2( 0f, Screen.height - pxSize.y );
                case AdPosition.BottomRight:
                    return new Vector2( Screen.width - pxSize.x, Screen.height - pxSize.y );
                default:
                    return new Vector2( Screen.width * 0.5f - pxSize.x * 0.5f, Screen.height - pxSize.y );
            }
        }

        private void OnGUIInterstitialAd()
        {
            if (( visibleTypes & AdFlags.Interstitial ) == AdFlags.Interstitial)
            {
                if (GUI.Button( new Rect( 0, 0, Screen.width, Screen.height ), "Close\n\nCAS Interstitial Ad", _btnStyle ))
                {
                    CASFactory.ExecuteEvent( OnInterstitialAdClicked );
                    _settings.lastInterImpressionTimestamp = Time.time;
                    visibleTypes &= ~AdFlags.Interstitial;
                    loadedTypes &= ~AdFlags.Interstitial;
                    CASFactory.ExecuteEvent( OnFailedToLoadAd, ( int )AdType.Interstitial, "Please Load new Ad" );
                    if (_settings.loadingMode != LoadingManagerMode.Manual)
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
                if (GUI.Button( new Rect( 0, 0, width, height * 0.5f ), "Close\nCAS Rewarded Video Ad", _btnStyle ))
                {
                    visibleTypes &= ~AdFlags.Rewarded;
                    loadedTypes &= ~AdFlags.Rewarded;
                    CASFactory.ExecuteEvent( OnFailedToLoadAd, ( int )AdType.Rewarded, "Please Load new Ad" );
                    if (_settings.loadingMode != LoadingManagerMode.Manual)
                        LoadAd( AdType.Rewarded );
                    CASFactory.ExecuteEvent( OnRewardedAdClosed );
                }
                if (GUI.Button( new Rect( 0, height * 0.5f, width, height * 0.5f ), "Complete\nCAS Rewarded Video Ad", _btnStyle ))
                {
                    CASFactory.ExecuteEvent( OnRewardedAdClicked );
                    CASFactory.ExecuteEvent( OnRewardedAdCompleted );
                    visibleTypes &= ~AdFlags.Rewarded;
                    loadedTypes &= ~AdFlags.Rewarded;
                    CASFactory.ExecuteEvent( OnFailedToLoadAd, ( int )AdType.Rewarded, "Please Load new Ad" );
                    if (_settings.loadingMode != LoadingManagerMode.Manual)
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
            loadedTypes |= AdFlags.Banner;
            if (_bannerRequired)
                EventExecutor.Add( CallShowBanner );
            CASFactory.ExecuteEvent( OnLoadedAd, ( int )AdType.Banner );
        }

        private void DidInterstitialLoaded()
        {
            loadedTypes |= AdFlags.Interstitial;
            CASFactory.ExecuteEvent( OnLoadedAd, ( int )AdType.Interstitial );
        }

        private void DidRewardedLoaded()
        {
            loadedTypes |= AdFlags.Rewarded;
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