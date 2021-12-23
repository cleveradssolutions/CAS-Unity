#if UNITY_IOS || (CASDeveloper && UNITY_EDITOR)
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CAS.iOS
{
    internal class CASMediationManager : CASViewFactory, IMediationManager
    {
        private IntPtr _managerRef;
        private IntPtr _managerClient;
        private InitCompleteAction _initCompleteAction;
        private LastPageAdContent _lastPageAdContent = null;

        public string managerID { get; }
        public bool isTestAdMode { get; }

        #region Ad Events
        public event CASTypedEvent OnLoadedAd;
        public event CASTypedEventWithError OnFailedToLoadAd;

        public event Action OnInterstitialAdShown;
        public event CASEventWithMeta OnInterstitialAdOpening;
        public event CASEventWithError OnInterstitialAdFailedToShow;
        public event Action OnInterstitialAdClicked;
        public event Action OnInterstitialAdClosed;
        public event Action OnRewardedAdShown;
        public event CASEventWithMeta OnRewardedAdOpening;
        public event CASEventWithError OnRewardedAdFailedToShow;
        public event Action OnRewardedAdClicked;
        public event Action OnRewardedAdCompleted;
        public event Action OnRewardedAdClosed;
        public event Action OnAppReturnAdShown;
        public event CASEventWithMeta OnAppReturnAdOpening;
        public event CASEventWithError OnAppReturnAdFailedToShow;
        public event Action OnAppReturnAdClicked;
        public event Action OnAppReturnAdClosed;
        #endregion

        public CASMediationManager( CASInitSettings initData )
        {
            managerID = initData.targetId;
            isTestAdMode = initData.IsTestAdMode();
        }

        ~CASMediationManager()
        {
            try
            {
                CASExterns.CASUFreeManager( _managerRef );
                _managerRef = IntPtr.Zero;
                ( ( GCHandle )_managerClient ).Free();
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        public void CreateManager( CASInitSettings initData )
        {
            _initCompleteAction = initData.initListener;

            CASExterns.CASUSetUnityVersion( Application.unityVersion );

            _managerClient = ( IntPtr )GCHandle.Alloc( this );

            if (initData.extras != null && initData.extras.Count != 0)
            {
                var extrasKeys = new string[initData.extras.Count];
                var extrasValues = new string[initData.extras.Count];
                int extraI = 0;
                foreach (var extra in initData.extras)
                {
                    extrasKeys[extraI] = extra.Key;
                    extrasValues[extraI] = extra.Value;
                    extraI++;
                }
                _managerRef = CASExterns.CASUCreateManagerWithExtras(
                    _managerClient,
                    InitializationCompleteCallback,
                    managerID,
                    ( int )initData.allowedAdFlags,
                    isTestAdMode,
                    extrasKeys,
                    extrasValues,
                    initData.extras.Count
                );
            }
            else
            {
                _managerRef = CASExterns.CASUCreateManager(
                    _managerClient,
                    InitializationCompleteCallback,
                    managerID,
                    ( int )initData.allowedAdFlags,
                    isTestAdMode
                );
            }

            CASExterns.CASUSetInterstitialDelegate( _managerRef,
                InterstitialLoadedAdCallback,
                InterstitialFailedAdCallback,
                InterstitialOpeningWithMetaCallback,
                InterstitialDidShowAdFailedWithErrorCallback,
                InterstitialDidClickedAdCallback,
                InterstitialDidClosedAdCallback );

            CASExterns.CASUSetRewardedDelegate( _managerRef,
                RewardedLoadedAdCallback,
                RewardedFailedAdCallback,
                RewardedOpeningWithAdCallbackAndMeta,
                RewardedDidShowAdFailedWithErrorCallback,
                RewardedDidClickedAdCallback,
                RewardedDidCompletedAdCallback,
                RewardedDidClosedAdCallback );

            CASExterns.CASUSetAppReturnDelegate( _managerRef,
                ReturnAdOpeningWithAdCallback,
                ReturnAdDidShowAdFailedWithErrorCallback,
                ReturnAdDidClickedAdCallback,
                ReturnAdDidClosedAdCallback );
        }

        public LastPageAdContent lastPageAdContent
        {
            get { return _lastPageAdContent; }
            set
            {
                if (_lastPageAdContent != value)
                {
                    _lastPageAdContent = value;
                    string json = value == null ? "" : JsonUtility.ToJson( value );
                    CASExterns.CASUSetLastPageAdContent( _managerRef, json );
                }
            }
        }

        public string GetLastActiveMediation( AdType adType )
        {
            return CASExterns.CASUGetLastActiveMediationWithType( _managerRef, ( int )adType );
        }

        public bool IsEnabledAd( AdType adType )
        {
            return CASExterns.CASUIsAdEnabledType( _managerRef, ( int )adType );
        }

        public bool IsReadyAd( AdType adType )
        {
            switch (adType)
            {
                case AdType.Banner:
                    return IsGlobalViewReady();
                case AdType.Interstitial:
                    return CASExterns.CASUIsInterstitialReady( _managerRef );
                case AdType.Rewarded:
                    return CASExterns.CASUIsRewardedReady( _managerRef );
                default:
                    return false;
            }
        }

        public void LoadAd( AdType adType )
        {
            switch (adType)
            {
                case AdType.Banner:
                    GetOrCreateGlobalView().Load();
                    break;
                case AdType.Interstitial:
                    CASExterns.CASULoadInterstitial( _managerRef );
                    break;
                case AdType.Rewarded:
                    CASExterns.CASULoadReward( _managerRef );
                    break;
            }
        }

        public void SetEnableAd( AdType adType, bool enabled )
        {
            CASExterns.CASUEnableAdType( _managerRef, ( int )adType, enabled );
        }

        public void ShowAd( AdType adType )
        {
            switch (adType)
            {
                case AdType.Banner:
                    ShowBanner();
                    break;
                case AdType.Interstitial:
                    CASExterns.CASUPresentInterstitial( _managerRef );
                    break;
                case AdType.Rewarded:
                    CASExterns.CASUPresentRewarded( _managerRef );
                    break;
            }
        }

        protected override IAdView CreateAdView( AdSize size )
        {
            var view = new CASView( this, size );
            var viewClient = ( IntPtr )GCHandle.Alloc( view );
            view.Attach( CASExterns.CASUCreateAdView( _managerRef, viewClient, ( int )size ), viewClient );
            return view;
        }

        public override void CallbackOnDestroy( IAdView view )
        {
            base.CallbackOnDestroy( view );
            CASExterns.CASUDestroyAdView( ( ( CASView )view )._viewRef, _managerRef, ( int )view.size );
        }

        public bool TryOpenDebugger()
        {
            CASExterns.CASUOpenDebugger( _managerRef );
            return true;
        }

        public void SetAppReturnAdsEnabled( bool enable )
        {
            if (enable)
                CASExterns.CASUEnableAppReturnAds( _managerRef );
            else
                CASExterns.CASUDisableAppReturnAds( _managerRef );
        }

        public void SkipNextAppReturnAds()
        {
            CASExterns.CASUSkipNextAppReturnAds( _managerRef );
        }

        public override void OnLoadedCallback( AdType type )
        {
            if (OnLoadedAd != null)
                OnLoadedAd( type );
        }

        public override void OnFailedCallback( AdType type, AdError error )
        {
            if (OnFailedToLoadAd != null)
                OnFailedToLoadAd( type, error.GetMessage() );
        }

        private void OnInterstitialLoadedCallback()
        {
            OnLoadedCallback( AdType.Interstitial );
        }

        private void OnRewardedLoadedCallback()
        {
            OnLoadedCallback( AdType.Rewarded );
        }

        #region Callback methods
        private static CASMediationManager IntPtrToManagerClient( IntPtr managerClient )
        {
            GCHandle handle = ( GCHandle )managerClient;
            return handle.Target as CASMediationManager;
        }

        private static CASView IntPtrToAdViewClient( IntPtr managerClient )
        {
            GCHandle handle = ( GCHandle )managerClient;
            return handle.Target as CASView;
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUInitializationCompleteCallback ) )]
        private static void InitializationCompleteCallback( IntPtr client, bool success, string error )
        {
            if (MobileAds.settings.isDebugMode)
                Debug.Log( "[CleverAdsSolutions] InitializationComplete " + success );
            var instance = IntPtrToManagerClient( client );
            if (instance != null && instance._initCompleteAction != null)
            {
                CASFactory.ExecuteEvent( () =>
                {
                    if (instance._initCompleteAction != null)
                        instance._initCompleteAction( success, error );
                } );
            }
        }

        #region Interstitial Callback
        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidLoadedAdCallback ) )]
        private static void InterstitialLoadedAdCallback( IntPtr manager )
        {
            try
            {
                if (MobileAds.settings.isDebugMode)
                    Debug.Log( "[CleverAdsSolutions] Interstitial Loaded" );
                var instance = IntPtrToManagerClient( manager );
                if (instance != null)
                    CASFactory.ExecuteEvent( instance.OnInterstitialLoadedCallback );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidFailedAdCallback ) )]
        private static void InterstitialFailedAdCallback( IntPtr manager, int error )
        {
            try
            {
                var adError = ( AdError )error;
                if (MobileAds.settings.isDebugMode)
                    Debug.Log( "[CleverAdsSolutions] Interstitial Failed with error: " + adError.ToString() );
                var instance = IntPtrToManagerClient( manager );
                if (instance != null)
                    CASFactory.ExecuteEvent( () => instance.OnFailedCallback( AdType.Interstitial, adError ) );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUWillOpeningWithMetaCallback ) )]
        private static void InterstitialOpeningWithMetaCallback( IntPtr manager, int net, double cpm, int accuracy )
        {
            try
            {
                if (MobileAds.settings.isDebugMode)
                    Debug.Log( "[CleverAdsSolutions] Interstitial Opening " + net + " cpm:" + cpm + " accuracy:" + accuracy );
                var instance = IntPtrToManagerClient( manager );
                if (instance == null)
                    return;
                if (instance.OnInterstitialAdShown != null)
                    instance.OnInterstitialAdShown();

                if (instance.OnInterstitialAdOpening != null)
                    instance.OnInterstitialAdOpening(
                        new AdMetaData( AdType.Interstitial, ( AdNetwork )net, cpm, ( PriceAccuracy )accuracy ) );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidShowAdFailedWithErrorCallback ) )]
        private static void InterstitialDidShowAdFailedWithErrorCallback( IntPtr manager, string error )
        {
            try
            {
                if (MobileAds.settings.isDebugMode)
                    Debug.Log( "[CleverAdsSolutions] Interstitial Show Ad Failed with error: " + error );
                var instance = IntPtrToManagerClient( manager );
                if (instance != null && instance.OnInterstitialAdFailedToShow != null)
                    instance.OnInterstitialAdFailedToShow( error );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidClickedAdCallback ) )]
        private static void InterstitialDidClickedAdCallback( IntPtr manager )
        {
            try
            {
                if (MobileAds.settings.isDebugMode)
                    Debug.Log( "[CleverAdsSolutions] Interstitial Clicked" );
                var instance = IntPtrToManagerClient( manager );
                if (instance != null && instance.OnInterstitialAdClicked != null)
                    instance.OnInterstitialAdClicked();
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidClosedAdCallback ) )]
        private static void InterstitialDidClosedAdCallback( IntPtr manager )
        {
            try
            {
                if (MobileAds.settings.isDebugMode)
                    Debug.Log( "[CleverAdsSolutions] Interstitial Closed" );
                var instance = IntPtrToManagerClient( manager );
                if (instance != null && instance.OnInterstitialAdClosed != null)
                    instance.OnInterstitialAdClosed();
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }
        #endregion

        #region Rewarded Callback
        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidLoadedAdCallback ) )]
        private static void RewardedLoadedAdCallback( IntPtr manager )
        {
            try
            {
                if (MobileAds.settings.isDebugMode)
                    Debug.Log( "[CleverAdsSolutions] Rewarded Loaded" );
                var instance = IntPtrToManagerClient( manager );
                if (instance != null)
                    CASFactory.ExecuteEvent( instance.OnRewardedLoadedCallback );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidFailedAdCallback ) )]
        private static void RewardedFailedAdCallback( IntPtr manager, int error )
        {
            try
            {
                var adError = ( AdError )error;
                if (MobileAds.settings.isDebugMode)
                    Debug.Log( "[CleverAdsSolutions] Rewarded Failed with error: " + adError.ToString() );
                var instance = IntPtrToManagerClient( manager );
                if (instance != null)
                    CASFactory.ExecuteEvent( () => instance.OnFailedCallback( AdType.Rewarded, adError ) );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUWillOpeningWithMetaCallback ) )]
        private static void RewardedOpeningWithAdCallbackAndMeta( IntPtr manager, int net, double cpm, int accuracy )
        {
            try
            {
                if (MobileAds.settings.isDebugMode)
                    Debug.Log( "[CleverAdsSolutions] Rewarded Opening " + net + " cpm:" + cpm + " accuracy:" + accuracy );
                var instance = IntPtrToManagerClient( manager );
                if (instance == null)
                    return;
                if (instance.OnRewardedAdShown != null)
                    instance.OnRewardedAdShown();
                if (instance.OnRewardedAdOpening != null)
                    instance.OnRewardedAdOpening(
                        new AdMetaData( AdType.Rewarded, ( AdNetwork )net, cpm, ( PriceAccuracy )accuracy ) );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidShowAdFailedWithErrorCallback ) )]
        private static void RewardedDidShowAdFailedWithErrorCallback( IntPtr manager, string error )
        {
            try
            {
                if (MobileAds.settings.isDebugMode)
                    Debug.Log( "[CleverAdsSolutions] Rewarded  Show Ad Failed with error: " + error );
                var instance = IntPtrToManagerClient( manager );
                if (instance != null && instance.OnRewardedAdFailedToShow != null)
                    instance.OnRewardedAdFailedToShow( error );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidClickedAdCallback ) )]
        private static void RewardedDidClickedAdCallback( IntPtr manager )
        {
            try
            {
                if (MobileAds.settings.isDebugMode)
                    Debug.Log( "[CleverAdsSolutions] Rewarded Clicked" );
                var instance = IntPtrToManagerClient( manager );
                if (instance != null && instance.OnRewardedAdClicked != null)
                    instance.OnRewardedAdClicked();
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidCompletedAdCallback ) )]
        private static void RewardedDidCompletedAdCallback( IntPtr manager )
        {
            try
            {
                if (MobileAds.settings.isDebugMode)
                    Debug.Log( "[CleverAdsSolutions] Rewarded Completed" );
                var instance = IntPtrToManagerClient( manager );
                if (instance != null && instance.OnRewardedAdCompleted != null)
                    instance.OnRewardedAdCompleted();
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidClosedAdCallback ) )]
        private static void RewardedDidClosedAdCallback( IntPtr manager )
        {
            try
            {
                if (MobileAds.settings.isDebugMode)
                    Debug.Log( "[CleverAdsSolutions] Rewarded Closed" );
                var instance = IntPtrToManagerClient( manager );
                if (instance != null && instance.OnRewardedAdClosed != null)
                    instance.OnRewardedAdClosed();
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }
        #endregion

        #region App Return Ads Callback
        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUWillOpeningWithMetaCallback ) )]
        private static void ReturnAdOpeningWithAdCallback( IntPtr manager, int net, double cpm, int accuracy )
        {
            try
            {
                if (MobileAds.settings.isDebugMode)
                    Debug.Log( "[CleverAdsSolutions] Return Ad Opening " + net + " cpm: " + cpm + " accuracy: " + accuracy );
                var instance = IntPtrToManagerClient( manager );
                if (instance == null)
                    return;
                if (instance.OnAppReturnAdShown != null)
                    instance.OnAppReturnAdShown();
                if (instance.OnAppReturnAdOpening != null)
                    instance.OnAppReturnAdOpening(
                        new AdMetaData( AdType.Interstitial, ( AdNetwork )net, cpm, ( PriceAccuracy )accuracy ) );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidShowAdFailedWithErrorCallback ) )]
        private static void ReturnAdDidShowAdFailedWithErrorCallback( IntPtr manager, string error )
        {
            try
            {
                if (MobileAds.settings.isDebugMode)
                    Debug.Log( "[CleverAdsSolutions] Return Ad Show Failed " + error );
                var instance = IntPtrToManagerClient( manager );
                if (instance != null && instance.OnAppReturnAdFailedToShow != null)
                    instance.OnAppReturnAdFailedToShow( error );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidClickedAdCallback ) )]
        private static void ReturnAdDidClickedAdCallback( IntPtr manager )
        {
            try
            {
                if (MobileAds.settings.isDebugMode)
                    Debug.Log( "[CleverAdsSolutions] Return Ad Clicked" );
                var instance = IntPtrToManagerClient( manager );
                if (instance != null && instance.OnAppReturnAdClicked != null)
                    instance.OnAppReturnAdClicked();
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidClosedAdCallback ) )]
        private static void ReturnAdDidClosedAdCallback( IntPtr manager )
        {
            try
            {
                if (MobileAds.settings.isDebugMode)
                    Debug.Log( "[CleverAdsSolutions] Return Ad Closed" );
                var instance = IntPtrToManagerClient( manager );
                if (instance != null && instance.OnAppReturnAdClosed != null)
                    instance.OnAppReturnAdClosed();
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }
        #endregion
        #endregion
    }
}
#endif