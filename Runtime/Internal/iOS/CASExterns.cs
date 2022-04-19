//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2021 CleverAdsSolutions. All rights reserved.
//

#if UNITY_IOS || (CASDeveloper && UNITY_EDITOR)
using System;
using System.Runtime.InteropServices;

namespace CAS.iOS
{
    // Externs used by the iOS component.
    internal class CASExterns
    {
        #region CAS Callback types
        internal delegate void CASUDidLoadedAdCallback( IntPtr manager );
        internal delegate void CASUDidFailedAdCallback( IntPtr manager, int error );
        internal delegate void CASUWillOpeningWithMetaCallback( IntPtr manager, int net, double cpm, int accuracy );
        internal delegate void CASUDidShowAdFailedWithErrorCallback( IntPtr manager, string error );
        internal delegate void CASUDidClickedAdCallback( IntPtr manager );
        internal delegate void CASUDidCompletedAdCallback( IntPtr manager );
        internal delegate void CASUDidClosedAdCallback( IntPtr manager );

        internal delegate void CASUInitializationCompleteCallback( IntPtr manager, bool success, string error );

        internal delegate void CASUATTCompletion( int status );
        #endregion

        #region CAS Settings
        [DllImport( "__Internal" )]
        internal static extern void CASUSetAnalyticsCollectionWithEnabled( bool enabled );

        [DllImport( "__Internal" )]
        internal static extern void CASUSetUnityVersion( string version );

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
        internal static extern void CASUFreeManager( IntPtr managerRef );
        #endregion

        #region General Ads functions
        [DllImport( "__Internal" )]
        internal static extern string CASUGetLastActiveMediationWithType( IntPtr managerRef, int adType );

        [DllImport( "__Internal" )]
        internal static extern bool CASUIsAdEnabledType( IntPtr managerRef, int adType );

        [DllImport( "__Internal" )]
        internal static extern void CASUEnableAdType( IntPtr managerRef, int adType, bool enable );

        [DllImport( "__Internal" )]
        internal static extern void CASUSetLastPageAdContent( IntPtr managerRef, string contentJson );
        #endregion

        #region Interstitial Ads
        [DllImport( "__Internal" )]
        internal static extern void CASUSetInterstitialDelegate(
            IntPtr managerRef, // Manager Ptr from CASUCreateManager
            CASUDidLoadedAdCallback didLoad,
            CASUDidFailedAdCallback didFaied,
            CASUWillOpeningWithMetaCallback willOpen,
            CASUDidShowAdFailedWithErrorCallback didShowWithError,
            CASUDidClickedAdCallback didClick,
            CASUDidClosedAdCallback didClosed
        );

        [DllImport( "__Internal" )]
        internal static extern void CASULoadInterstitial( IntPtr managerRef );

        [DllImport( "__Internal" )]
        internal static extern bool CASUIsInterstitialReady( IntPtr managerRef );

        [DllImport( "__Internal" )]
        internal static extern bool CASUPresentInterstitial( IntPtr managerRef );
        #endregion

        #region Rewarded Ads
        [DllImport( "__Internal" )]
        internal static extern void CASUSetRewardedDelegate(
            IntPtr manager, // Manager Ptr from CASUCreateManager
            CASUDidLoadedAdCallback didLoad,
            CASUDidFailedAdCallback didFaied,
            CASUWillOpeningWithMetaCallback willOpen,
            CASUDidShowAdFailedWithErrorCallback didShowWithError,
            CASUDidClickedAdCallback didClick,
            CASUDidCompletedAdCallback didComplete,
            CASUDidClosedAdCallback didClosed
        );

        [DllImport( "__Internal" )]
        internal static extern void CASULoadReward( IntPtr managerRef );

        [DllImport( "__Internal" )]
        internal static extern bool CASUIsRewardedReady( IntPtr managerRef );

        [DllImport( "__Internal" )]
        internal static extern bool CASUPresentRewarded( IntPtr managerRef );
        #endregion

        #region AdView
        [DllImport( "__Internal" )]
        internal static extern IntPtr CASUCreateAdView(
            IntPtr managerRef, // Manager Ptr from CASUCreateManager
            IntPtr client, // C# manager client ptr
            int adSizeCode
        );

        [DllImport( "__Internal" )]
        internal static extern void CASUDestroyAdView( IntPtr viewRef, IntPtr managerRef, int adSizeCode );

        [DllImport( "__Internal" )]
        internal static extern void CASUAttachAdViewDelegate(
            IntPtr viewRef,
            CASUDidLoadedAdCallback didLoad,
            CASUDidFailedAdCallback didFailed,
            CASUWillOpeningWithMetaCallback willPresent,
            CASUDidClickedAdCallback didClicked );

        [DllImport( "__Internal" )]
        internal static extern void CASUPresentAdView( IntPtr viewRef );

        [DllImport( "__Internal" )]
        internal static extern void CASUHideAdView( IntPtr viewRef );

        [DllImport( "__Internal" )]
        internal static extern void CASUSetAdViewPosition( IntPtr viewRef, int posCode, int x, int y );

        [DllImport( "__Internal" )]
        internal static extern void CASUSetAdViewRefreshInterval( IntPtr viewRef, int interval );

        [DllImport( "__Internal" )]
        internal static extern void CASULoadAdView( IntPtr viewRef );

        [DllImport( "__Internal" )]
        internal static extern bool CASUIsAdViewReady( IntPtr viewRef );

        [DllImport( "__Internal" )]
        internal static extern int CASUGetAdViewHeightInPixels( IntPtr viewRef );

        [DllImport( "__Internal" )]
        internal static extern int CASUGetAdViewWidthInPixels( IntPtr viewRef );

        [DllImport( "__Internal" )]
        internal static extern int CASUGetAdViewXOffsetInPixels( IntPtr viewRef );

        [DllImport( "__Internal" )]
        internal static extern int CASUGetAdViewYOffsetInPixels( IntPtr viewRef );
        #endregion

        #region App Return Ads

        [DllImport( "__Internal" )]
        internal static extern void CASUSetAppReturnDelegate(
            IntPtr manager, // Manager Ptr from CASUCreateManager
            CASUWillOpeningWithMetaCallback willOpen,
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

        [DllImport( "__Internal" )]
        internal static extern void CASURequestATT( CASUATTCompletion callback );

        [DllImport( "__Internal" )]
        internal static extern int CASUGetATTStatus();
    }
}
#endif