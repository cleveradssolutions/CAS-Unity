﻿#if UNITY_IOS || (CASDeveloper && UNITY_EDITOR)
using System;
using System.Runtime.InteropServices;

namespace CAS.iOS
{
    // Externs used by the iOS component.
    internal class CASExterns
    {
        #region CAS Settings
        [DllImport( "__Internal" )]
        internal static extern void CASUSetAnalyticsCollectionWithEnabled( bool enabled );

        [DllImport( "__Internal" )]
        internal static extern void CASUSetPluginPlatformWithName( string name, string version );

        [DllImport( "__Internal" )]
        internal static extern void CASUSetTestDeviceWithIds( string[] testDeviceIDs, int testDeviceIDLength );

        [DllImport( "__Internal" )]
        internal static extern void CASUSetBannerRefreshWithInterval( int interval );

        [DllImport( "__Internal" )]
        internal static extern void CASUSetInterstitialWithInterval( int interval );

        [DllImport( "__Internal" )]
        internal static extern void CASURestartInterstitialInterval();

        [DllImport( "__Internal" )]
        internal static extern void CASUUpdateUserConsent( int consent );

        [DllImport( "__Internal" )]
        internal static extern void CASUUpdateCCPAWithStatus( int status );

        [DllImport( "__Internal" )]
        internal static extern void CASUSetTaggedWithAudience( int audience );

        [DllImport( "__Internal" )]
        internal static extern void CASUSetDebugMode( bool mode );

        [DllImport( "__Internal" )]
        internal static extern void CASUSetMuteAdSoundsTo( bool muted );

        [DllImport( "__Internal" )]
        internal static extern void CASUSetLoadingWithMode( int mode );

        [DllImport( "__Internal" )]
        internal static extern void CASUSetInterstitialAdsWhenVideoCostAreLower( bool allow );

        [DllImport( "__Internal" )]
        internal static extern void CASUSetTrackLocationEnabled( bool enabled );

        [DllImport( "__Internal" )]
        internal static extern void CASUSetiOSAppPauseOnBackground( bool pause );
        #endregion

        #region User Targeting options
        [DllImport( "__Internal" )]
        internal static extern void CASUSetUserGender( int gender );

        [DllImport( "__Internal" )]
        internal static extern void CASUSetUserAge( int age );
        #endregion

        #region Utils
        [DllImport( "__Internal" )]
        internal static extern string CASUGetSDKVersion();

        [DllImport( "__Internal" )]
        internal static extern string CASUValidateIntegration();

        [DllImport( "__Internal" )]
        internal static extern string CASUGetActiveMediationPattern();

        [DllImport( "__Internal" )]
        internal static extern bool CASUIsActiveMediationNetwork( int net );

        [DllImport( "__Internal" )]
        internal static extern void CASUOpenDebugger( IntPtr manager );
        #endregion

        #region CAS Callback types
        // Field Client is C# manager client ptr
        internal delegate void CASUWillOpeningWithAdCallbackAndMeta( IntPtr client, int net, double cpm, int accuracy );
        internal delegate void CASUDidShowAdFailedWithErrorCallback( IntPtr client, string error );
        internal delegate void CASUDidClickedAdCallback( IntPtr client );
        internal delegate void CASUDidCompletedAdCallback( IntPtr client );
        internal delegate void CASUDidClosedAdCallback( IntPtr client );

        internal delegate void CASUInitializationCompleteCallback( IntPtr client, bool success, string error );
        internal delegate void CASUDidAdLoadedCallback( IntPtr client, int adType );
        internal delegate void CASUDidAdFailedToLoadCallback( IntPtr client, int adType, string error );
        #endregion

        #region CAS Manager
        [DllImport( "__Internal" )]
        internal static extern IntPtr CASUCreateManager(
            IntPtr client, // C# manager client ptr
            CASUInitializationCompleteCallback onInit,
            string managerID,
            int enableAd,
            bool demoAd
        );

        [DllImport( "__Internal" )]
        internal static extern IntPtr CASUCreateManagerWithExtras(
            IntPtr client, // C# manager client ptr
            CASUInitializationCompleteCallback onInit,
            string managerID,
            int enableAd,
            bool demoAd,
            string[] extraKeys,
            string[] extraValues,
            int extrasCount
        );

        [DllImport( "__Internal" )]
        internal static extern void CASUFreeManager( IntPtr manager );

        [DllImport( "__Internal" )]
        internal static extern void CASUSetLoadAdDelegate(
            IntPtr manager, // Manager Ptr from CASUCreateManager
            CASUDidAdLoadedCallback didLoaded,
            CASUDidAdFailedToLoadCallback didFailedToLoad
        );

        [DllImport( "__Internal" )]
        internal static extern void CASULoadAdWithType( IntPtr manager, int adType );

        [DllImport( "__Internal" )]
        internal static extern bool CASUIsAdReadyWithType( IntPtr manager, int adType );

        [DllImport( "__Internal" )]
        internal static extern void CASUShowAdWithType( IntPtr manager, int adType );

        [DllImport( "__Internal" )]
        internal static extern string CASUGetLastActiveMediationWithType( IntPtr manager, int adType );

        [DllImport( "__Internal" )]
        internal static extern bool CASUIsAdEnabledType( IntPtr manager, int adType );

        [DllImport( "__Internal" )]
        internal static extern void CASUEnableAdType( IntPtr manager, int adType, bool enable );

        [DllImport( "__Internal" )]
        internal static extern void CASUSetLastPageAdContent( IntPtr manager, string contentJson );
        #endregion

        #region Interstitial Ads
        [DllImport( "__Internal" )]
        internal static extern void CASUSetInterstitialDelegate(
            IntPtr manager, // Manager Ptr from CASUCreateManager
            CASUWillOpeningWithAdCallbackAndMeta willOpen,
            CASUDidShowAdFailedWithErrorCallback didShowWithError,
            CASUDidClickedAdCallback didClick,
            CASUDidClosedAdCallback didClosed
        );
        #endregion

        #region Rewarded Ads
        [DllImport( "__Internal" )]
        internal static extern void CASUSetRewardedDelegate(
            IntPtr manager, // Manager Ptr from CASUCreateManager
            CASUWillOpeningWithAdCallbackAndMeta willOpen,
            CASUDidShowAdFailedWithErrorCallback didShowWithError,
            CASUDidClickedAdCallback didClick,
            CASUDidCompletedAdCallback didComplete,
            CASUDidClosedAdCallback didClosed
        );
        #endregion

        #region Banner Ads
        [DllImport( "__Internal" )]
        internal static extern void CASUSetBannerDelegate(
            IntPtr manager, // Manager Ptr from CASUCreateManager
            CASUWillOpeningWithAdCallbackAndMeta willOpen,
            CASUDidShowAdFailedWithErrorCallback didShowWithError,
            CASUDidClickedAdCallback didClick,
            CASUDidClosedAdCallback didClosed
        );

        [DllImport( "__Internal" )]
        internal static extern void CASUHideBanner( IntPtr manager );

        [DllImport( "__Internal" )]
        internal static extern void CASUSetBannerSize( IntPtr manager, int bannerSize );

        [DllImport( "__Internal" )]
        internal static extern void CASUSetBannerPosition( IntPtr manager, int bannerPos );

        [DllImport( "__Internal" )]
        internal static extern float CASUGetBannerHeightInPixels( IntPtr manager );

        [DllImport( "__Internal" )]
        internal static extern float CASUGetBannerWidthInPixels( IntPtr manager );
        #endregion

        #region App Return Ads

        [DllImport( "__Internal" )]
        internal static extern void CASUSetAppReturnDelegate(
            IntPtr manager, // Manager Ptr from CASUCreateManager
            CASUWillOpeningWithAdCallbackAndMeta willOpen,
            CASUDidShowAdFailedWithErrorCallback didShowWithError,
            CASUDidClickedAdCallback didClick,
            CASUDidClosedAdCallback didClosed
        );

        [DllImport( "__Internal" )]
        internal static extern void CASUEnableAppReturnAds( IntPtr manager );

        [DllImport( "__Internal" )]
        internal static extern void CASUDisableAppReturnAds( IntPtr manager );

        [DllImport( "__Internal" )]
        internal static extern void CASUSkipNextAppReturnAds( IntPtr manager );
        #endregion
    }
}
#endif