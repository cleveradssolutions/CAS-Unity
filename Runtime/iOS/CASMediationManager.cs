﻿#if UNITY_IOS || CASDeveloper
// Vungle Network crashes if ad orientation is not allowed
//#define CAS_EXPIREMENTAL_ORIENTATION
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CAS.iOS
{
    internal class CASMediationManager : IMediationManager
    {
        private IntPtr _managerPtr;
        private IntPtr _managerClientPtr;
        private InitCompleteAction _initCompleteAction;
        private AdSize _bannerSize = AdSize.Banner;
        private AdPosition _bannerPosition = AdPosition.BottomCenter;
        private LastPageAdContent _lastPageAdContent = null;
#if CAS_EXPIREMENTAL_ORIENTATION
        private bool[] autorotateOrientation = new bool[4];
#endif

        public string managerID { get; }
        public bool isTestAdMode { get; }

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

        public CASMediationManager( string managerID, bool isDemoAdMode )
        {
            this.managerID = managerID;
            this.isTestAdMode = isDemoAdMode;
        }

        ~CASMediationManager()
        {
            try
            {
                CASExterns.CASUFreeManager( _managerPtr );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        public void CreateManager( AdFlags enableAd, InitCompleteAction initCompleteAction )
        {
            _initCompleteAction = initCompleteAction;

            CASExterns.CASUSetPluginPlatformWithName( "Unity", Application.unityVersion );

            _managerClientPtr = ( IntPtr )GCHandle.Alloc( this );
            _managerPtr = CASExterns.CASUCreateManager(
                _managerClientPtr,
                InitializationCompleteCallback,
                managerID,
                ( int )enableAd,
                isTestAdMode
            );

            CASExterns.CASUSetLoadAdDelegate( _managerPtr,
                DidAdLoadedCallback,
                DidAdFailedToLoadCallback );

            CASExterns.CASUSetBannerDelegate( _managerPtr,
                BannerWillShownWithAdCallback,
                BannerDidShowAdFailedWithErrorCallback,
                BannerDidClickedAdCallback,
                BannerDidClosedAdCallback );

            CASExterns.CASUSetInterstitialDelegate( _managerPtr,
                InterstitialWillShownWithAdCallback,
                InterstitialDidShowAdFailedWithErrorCallback,
                InterstitialDidClickedAdCallback,
                InterstitialDidClosedAdCallback );

            CASExterns.CASUSetRewardedDelegate( _managerPtr,
                RewardedWillShownWithAdCallback,
                RewardedDidShowAdFailedWithErrorCallback,
                RewardedDidClickedAdCallback,
                RewardedDidCompletedAdCallback,
                RewardedDidClosedAdCallback );
        }

        public AdSize bannerSize
        {
            get { return _bannerSize; }
            set
            {
                if (_bannerSize != value && _bannerSize != 0)
                {
                    _bannerSize = value;
                    CASExterns.CASUSetBannerSize( _managerPtr, ( int )value );
                }
            }
        }

        public AdPosition bannerPosition
        {
            get { return _bannerPosition; }
            set
            {
                if (_bannerPosition != value && value != AdPosition.Undefined)
                {
                    _bannerPosition = value;
                    CASExterns.CASUSetBannerPosition( _managerPtr, ( int )value );
                }
            }
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
                    CASExterns.CASUSetLastPageAdContent( _managerPtr, json );
                }
            }
        }

        public string GetLastActiveMediation( AdType adType )
        {
            return CASExterns.CASUGetLastActiveMediationWithType( _managerPtr, ( int )adType );
        }

        public void HideBanner()
        {
            CASExterns.CASUHideBanner( _managerPtr );
        }

        public bool IsEnabledAd( AdType adType )
        {
            return CASExterns.CASUIsAdEnabledType( _managerPtr, ( int )adType );
        }

        public bool IsReadyAd( AdType adType )
        {
            return CASExterns.CASUIsAdReadyWithType( _managerPtr, ( int )adType );
        }

        public void LoadAd( AdType adType )
        {
            CASExterns.CASULoadAdWithType( _managerPtr, ( int )adType );
        }

        public void SetEnableAd( AdType adType, bool enabled )
        {
            CASExterns.CASUEnableAdType( _managerPtr, ( int )adType, enabled );
        }

        public void ShowAd( AdType adType )
        {
#if CAS_EXPIREMENTAL_ORIENTATION
            if (adType == AdType.Interstitial || adType == AdType.Rewarded)
                PrepareScreenOrientation();
#endif
            CASExterns.CASUShowAdWithType( _managerPtr, ( int )adType );
        }

        public float GetBannerHeightInPixels()
        {
            return CASExterns.CASUGetBannerHeightInPixels( _managerPtr );
        }

        public float GetBannerWidthInPixels()
        {
            return CASExterns.CASUGetBannerWidthInPixels( _managerPtr );
        }

#if CAS_EXPIREMENTAL_ORIENTATION
        private void PrepareScreenOrientation()
        {
            autorotateOrientation[0] = Screen.autorotateToPortrait;
            autorotateOrientation[1] = Screen.autorotateToPortraitUpsideDown;
            autorotateOrientation[2] = Screen.autorotateToLandscapeLeft;
            autorotateOrientation[3] = Screen.autorotateToLandscapeRight;

            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
        }

        private void RestoreScreenOrientation()
        {
            Screen.autorotateToPortrait = autorotateOrientation[0];
            Screen.autorotateToPortraitUpsideDown = autorotateOrientation[1];
            Screen.autorotateToLandscapeLeft = autorotateOrientation[2];
            Screen.autorotateToLandscapeRight = autorotateOrientation[3];
        }
#endif

        #region Callback methods
        private static CASMediationManager IntPtrToManagerClient( IntPtr managerClient )
        {
            GCHandle handle = ( GCHandle )managerClient;
            return handle.Target as CASMediationManager;
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUInitializationCompleteCallback ) )]
        private static void InitializationCompleteCallback( IntPtr client, bool success, string error )
        {
            var instance = IntPtrToManagerClient( client );
            if (instance != null)
                CASFactory.ExecuteEvent( instance._initCompleteAction, success, error );
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidAdLoadedCallback ) )]
        private static void DidAdLoadedCallback( IntPtr client, int adType )
        {
            var instance = IntPtrToManagerClient( client );
            if (instance != null)
                CASFactory.ExecuteEvent( instance.OnLoadedAd, adType );
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidAdFailedToLoadCallback ) )]
        private static void DidAdFailedToLoadCallback( IntPtr client, int adType, string error )
        {
            var instance = IntPtrToManagerClient( client );
            if (instance != null)
                CASFactory.ExecuteEvent( instance.OnFailedToLoadAd, adType, error );
        }

        #region Banner Callback
        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUWillShownWithAdCallback ) )]
        private static void BannerWillShownWithAdCallback( IntPtr client )
        {
            try
            {
                var instance = IntPtrToManagerClient( client );
                if (instance != null && instance.OnBannerAdShown != null)
                    instance.OnBannerAdShown();
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidShowAdFailedWithErrorCallback ) )]
        private static void BannerDidShowAdFailedWithErrorCallback( IntPtr client, string error )
        {
            try
            {
                var instance = IntPtrToManagerClient( client );
                if (instance != null && instance.OnBannerAdFailedToShow != null)
                    instance.OnBannerAdFailedToShow( error );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidClickedAdCallback ) )]
        private static void BannerDidClickedAdCallback( IntPtr client )
        {
            try
            {
                var instance = IntPtrToManagerClient( client );
                if (instance != null && instance.OnBannerAdClicked != null)
                    instance.OnBannerAdClicked();
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidClosedAdCallback ) )]
        private static void BannerDidClosedAdCallback( IntPtr client )
        {
            try
            {
                var instance = IntPtrToManagerClient( client );
                if (instance != null && instance.OnBannerAdHidden != null)
                    instance.OnBannerAdHidden();
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }
        #endregion

        #region Interstitial Callback
        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUWillShownWithAdCallback ) )]
        private static void InterstitialWillShownWithAdCallback( IntPtr client )
        {
            try
            {
                var instance = IntPtrToManagerClient( client );
                if (instance != null && instance.OnInterstitialAdShown != null)
                    instance.OnInterstitialAdShown();
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidShowAdFailedWithErrorCallback ) )]
        private static void InterstitialDidShowAdFailedWithErrorCallback( IntPtr client, string error )
        {
            try
            {
                var instance = IntPtrToManagerClient( client );
#if CAS_EXPIREMENTAL_ORIENTATION
                EventExecutor.Add( instance.RestoreScreenOrientation );
#endif
                if (instance != null && instance.OnInterstitialAdFailedToShow != null)
                    instance.OnInterstitialAdFailedToShow( error );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidClickedAdCallback ) )]
        private static void InterstitialDidClickedAdCallback( IntPtr client )
        {
            try
            {
                var instance = IntPtrToManagerClient( client );
                if (instance != null && instance.OnInterstitialAdClicked != null)
                    instance.OnInterstitialAdClicked();
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidClosedAdCallback ) )]
        private static void InterstitialDidClosedAdCallback( IntPtr client )
        {
            try
            {
                var instance = IntPtrToManagerClient( client );
#if CAS_EXPIREMENTAL_ORIENTATION
                EventExecutor.Add( instance.RestoreScreenOrientation );
#endif
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
        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUWillShownWithAdCallback ) )]
        private static void RewardedWillShownWithAdCallback( IntPtr client )
        {
            try
            {
                var instance = IntPtrToManagerClient( client );
                if (instance != null && instance.OnRewardedAdShown != null)
                    instance.OnRewardedAdShown();
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidShowAdFailedWithErrorCallback ) )]
        private static void RewardedDidShowAdFailedWithErrorCallback( IntPtr client, string error )
        {
            try
            {
                var instance = IntPtrToManagerClient( client );

#if CAS_EXPIREMENTAL_ORIENTATION
                EventExecutor.Add( instance.RestoreScreenOrientation );
#endif
                if (instance != null && instance.OnRewardedAdFailedToShow != null)
                    instance.OnRewardedAdFailedToShow( error );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidClickedAdCallback ) )]
        private static void RewardedDidClickedAdCallback( IntPtr client )
        {
            try
            {
                var instance = IntPtrToManagerClient( client );
                if (instance != null && instance.OnRewardedAdClicked != null)
                    instance.OnRewardedAdClicked();
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidCompletedAdCallback ) )]
        private static void RewardedDidCompletedAdCallback( IntPtr client )
        {
            try
            {
                var instance = IntPtrToManagerClient( client );
                if (instance != null && instance.OnRewardedAdCompleted != null)
                    instance.OnRewardedAdCompleted();
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        [AOT.MonoPInvokeCallback( typeof( CASExterns.CASUDidClosedAdCallback ) )]
        private static void RewardedDidClosedAdCallback( IntPtr client )
        {
            try
            {
                var instance = IntPtrToManagerClient( client );
#if CAS_EXPIREMENTAL_ORIENTATION
            EventExecutor.Add( instance.RestoreScreenOrientation );
#endif
                if (instance != null && instance.OnRewardedAdClosed != null)
                    instance.OnRewardedAdClosed();
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