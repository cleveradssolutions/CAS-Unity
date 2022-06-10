//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2021 CleverAdsSolutions. All rights reserved.
//

#if UNITY_EDITOR || TARGET_OS_SIMULATOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CAS.Unity
{
    [AddComponentMenu( "" )]
    internal class CASMediationManager : MonoBehaviour, IMediationManager
    {
        public AdFlags enabledTypes;

        internal readonly AdMetaData bannerMetaData = CreateAdMetaData( AdType.Banner );

        internal CASViewFactoryImpl viewFactory;
        [SerializeField]
        private CASFullscreenView _interstitial;
        [SerializeField]
        private CASFullscreenView _rewarded;

        private List<Action> _eventsQueue = new List<Action>();
        private InitCompleteAction _initCompleteAction;
        private LastPageAdContent _lastPageAdContent;


        private GUIStyle _btnStyle = null;
        internal CASSettings _settings;

        public string managerID { get; private set; }
        public bool isTestAdMode { get { return true; } }

        public LastPageAdContent lastPageAdContent
        {
            get { return _lastPageAdContent; }
            set
            {
                _lastPageAdContent = value;
                if (value == null)
                    Log( "CAS Last Page Ad content cleared" );
                else
                    Log( "CAS Last Page Ad apply content:" +
                        "\n- Headline:" + value.Headline +
                        "\n- DestinationURL:" + value.DestinationURL +
                        "\n- ImageURL:" + value.ImageURL +
                        "\n- IconURL:" + value.IconURL +
                        "\n- AdText:" + value.AdText );
            }
        }

#pragma warning disable 67
        #region Ad load Events
        public event CASTypedEvent OnLoadedAd
        {
            add { viewFactory.OnLoadedAd += value; }
            remove { viewFactory.OnLoadedAd -= value; }
        }
        public event CASTypedEventWithError OnFailedToLoadAd
        {
            add { viewFactory.OnFailedToLoadAd += value; }
            remove { viewFactory.OnFailedToLoadAd -= value; }
        }
        #endregion

        #region Banner ad Events
        public event Action OnBannerAdShown
        {
            add { viewFactory.OnBannerAdShown += value; }
            remove { viewFactory.OnBannerAdShown -= value; }
        }
        public event CASEventWithMeta OnBannerAdOpening
        {
            add { viewFactory.OnBannerAdOpening += value; }
            remove { viewFactory.OnBannerAdOpening -= value; }
        }
        public event CASEventWithError OnBannerAdFailedToShow
        {
            add { viewFactory.OnBannerAdFailedToShow += value; }
            remove { viewFactory.OnBannerAdFailedToShow -= value; }
        }
        public event Action OnBannerAdClicked
        {
            add { viewFactory.OnBannerAdClicked += value; }
            remove { viewFactory.OnBannerAdClicked -= value; }
        }
        public event Action OnBannerAdHidden
        {
            add { viewFactory.OnBannerAdHidden += value; }
            remove { viewFactory.OnBannerAdHidden -= value; }
        }
        #endregion

        #region Interstitial ad Events
        public event Action OnInterstitialAdShown
        {
            add { _interstitial.OnAdShown += value; }
            remove { _interstitial.OnAdShown -= value; }
        }
        public event CASEventWithMeta OnInterstitialAdOpening
        {
            add { _interstitial.OnAdOpening += value; }
            remove { _interstitial.OnAdOpening -= value; }
        }
        public event CASEventWithError OnInterstitialAdFailedToShow
        {
            add { _interstitial.OnAdFailedToShow += value; }
            remove { _interstitial.OnAdFailedToShow -= value; }
        }
        public event Action OnInterstitialAdClicked
        {
            add { _interstitial.OnAdClicked += value; }
            remove { _interstitial.OnAdClicked -= value; }
        }
        public event Action OnInterstitialAdClosed
        {
            add { _interstitial.OnAdClosed += value; }
            remove { _interstitial.OnAdClosed -= value; }
        }
        #endregion

        #region Rewarded Ad Events
        public event Action OnRewardedAdShown
        {
            add { _rewarded.OnAdShown += value; }
            remove { _rewarded.OnAdShown -= value; }
        }
        public event CASEventWithMeta OnRewardedAdOpening
        {
            add { _rewarded.OnAdOpening += value; }
            remove { _rewarded.OnAdOpening -= value; }
        }
        public event CASEventWithError OnRewardedAdFailedToShow
        {
            add { _rewarded.OnAdFailedToShow += value; }
            remove { _rewarded.OnAdFailedToShow -= value; }
        }
        public event Action OnRewardedAdClicked
        {
            add { _rewarded.OnAdClicked += value; }
            remove { _rewarded.OnAdClicked -= value; }
        }
        public event Action OnRewardedAdCompleted
        {
            add { _rewarded.OnAdCompleted += value; }
            remove { _rewarded.OnAdCompleted -= value; }
        }
        public event Action OnRewardedAdClosed
        {
            add { _rewarded.OnAdClosed += value; }
            remove { _rewarded.OnAdClosed -= value; }
        }
        #endregion

        #region Return to app not supported for Editor
        public event Action OnAppReturnAdShown;
        public event CASEventWithMeta OnAppReturnAdOpening;
        public event CASEventWithError OnAppReturnAdFailedToShow;
        public event Action OnAppReturnAdClicked;
        public event Action OnAppReturnAdClosed;
        #endregion
#pragma warning restore 67

        public static IMediationManager CreateManager( CASInitSettings initSettings )
        {
            var obj = new GameObject( "CASMediationManager" );
            //obj.hideFlags = HideFlags.HideInHierarchy;
            DontDestroyOnLoad( obj );
            var manager = obj.AddComponent<CASMediationManager>();
            // Set Settings before any other calls.
            manager._settings = CAS.MobileAds.settings as CASSettings;

            manager.Log( "Initialized manager with id: " + initSettings.targetId );
            manager.managerID = initSettings.targetId;
            manager.enabledTypes = initSettings.allowedAdFlags;
            manager._initCompleteAction = initSettings.initListener;
            manager.viewFactory = new CASViewFactoryImpl( manager );
            manager._interstitial = new CASFullscreenView( manager, AdType.Interstitial );
            manager._rewarded = new CASFullscreenView( manager, AdType.Rewarded );
            return manager;
        }

        internal static AdMetaData CreateAdMetaData( AdType type )
        {
            return new AdMetaData( type, new Dictionary<string, string>()
            {
                { "network", ((int)AdNetwork.CrossPromotion).ToString() }
            } );
        }

        #region IMediationManager implementation

        public string GetLastActiveMediation( AdType adType )
        {
            return AdNetwork.CrossPromotion.ToString();
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
            if (adType == AdType.Banner)
                return viewFactory.IsGlobalViewReady();
            if (adType == AdType.Interstitial)
                return _interstitial.loaded
                    && _settings.lastInterImpressionTimestamp + MobileAds.settings.interstitialInterval < Time.time;
            return _rewarded.loaded;
        }

        public void LoadAd( AdType adType )
        {
            switch (adType)
            {
                case AdType.Banner:
                    viewFactory.GetOrCreateGlobalView().Load();
                    break;
                case AdType.Interstitial:
                    _interstitial.Load();
                    break;
                case AdType.Rewarded:
                    _rewarded.Load();
                    break;
            }
        }

        public void SetEnableAd( AdType adType, bool enabled )
        {
            if (enabled)
            {
                enabledTypes |= GetFlag( adType );
                if (adType == AdType.Banner)
                    viewFactory.LoadCreatedViews();
                else
                    LoadAd( adType );
                return;
            }
            enabledTypes &= ~GetFlag( adType );
        }

        public void ShowAd( AdType adType )
        {
            switch (adType)
            {
                case AdType.Banner:
                    viewFactory.ShowBanner();
                    break;
                case AdType.Interstitial:
                    Post( _interstitial.Show );
                    break;
                case AdType.Rewarded:
                    Post( _rewarded.Show );
                    break;
            }
        }

        public bool TryOpenDebugger()
        {
            // Not supported for editor
            return false;
        }

        public void SetAppReturnAdsEnabled( bool enable )
        {
            Log( "App return ads " + ( enable ? "enabled" : "disabled" ) );
        }

        public void SkipNextAppReturnAds()
        {
            Log( "The next time user return to the app, no ads will appear." );
        }
        #endregion


        #region MonoBehaviour implementation
        private void Start()
        {
            if (isAutolod)
            {
                LoadAd( AdType.Interstitial );
                LoadAd( AdType.Rewarded );
            }
            Post( CallInitComplete );
        }
        private void CallInitComplete()
        {
            if (_initCompleteAction != null)
                _initCompleteAction( true, null );
        }

        public void Update()
        {
            if (_eventsQueue.Count == 0)
                return;
            for (int i = 0; i < _eventsQueue.Count; i++)
            {
                try
                {
                    var action = _eventsQueue[i];
                    if (action != null)
                        action.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException( e );
                }
            }
            _eventsQueue.Clear();
        }

        public void OnGUI()
        {
            if (_btnStyle == null)
                _btnStyle = new GUIStyle( "Button" );
            _btnStyle.fontSize = ( int )( Math.Min( Screen.width, Screen.height ) * 0.035f );

            viewFactory.OnGUIAd( _btnStyle );
            _interstitial.OnGUIAd( _btnStyle );
            _rewarded.OnGUIAd( _btnStyle );
        }

        #endregion

        #region Utils
        public void Post( Action action )
        {
            if (action == null)
                return;
            //Log( "Event " + action.Target.GetType().FullName + "." + action.Method.Name );
            _eventsQueue.Add( action );
        }

        public void Post( Action action, float delay )
        {
            if (action == null)
                return;
            //Log( "Event " + action.Target.GetType().FullName + "." + action.Method.Name );
            StartCoroutine( DelayAction( action, delay ) );
        }

        internal void Log( string message )
        {
            if (_settings.isDebugMode)
                Debug.Log( "[CleverAdsSolutions] " + message );
        }

        public bool isFullscreenAdVisible
        {
            get { return _rewarded.active || _interstitial.active; }
        }
        public bool isAutolod
        {
            get { return _settings.loadingMode != LoadingManagerMode.Manual; }
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
#pragma warning disable CS0618 // Type or member is obsolete
                case AdType.Native:
                    return AdFlags.Native;
#pragma warning restore CS0618 // Type or member is obsolete
                case AdType.None:
                    return AdFlags.None;
                default:
                    throw new NotImplementedException( "Unknown adType " + adType.ToString() );
            }
        }

        private IEnumerator DelayAction( Action action, float delay )
        {
            yield return new WaitForSecondsRealtime( delay );
            action();
        }
        #endregion

        #region Obsolete banner API support
        public float GetBannerHeightInPixels()
        {
            return viewFactory.GetBannerHeightInPixels();
        }

        public float GetBannerWidthInPixels()
        {
            return viewFactory.GetBannerWidthInPixels();
        }

        public void HideBanner()
        {
            viewFactory.HideBanner();
        }

        public AdSize bannerSize
        {
            get { return viewFactory.bannerSize; }
            set { viewFactory.bannerSize = value; }
        }

        public AdPosition bannerPosition
        {
            get { return viewFactory.bannerPosition; }
            set { viewFactory.bannerPosition = value; }
        }

        public IAdView GetAdView( AdSize size )
        {
            return viewFactory.GetAdView( size );
        }

        #endregion
    }
}
#endif