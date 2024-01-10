//  Copyright © 2024 CAS.AI. All rights reserved.

#if UNITY_IOS || (CASDeveloper && UNITY_EDITOR)
using System.Runtime.InteropServices;
using System;

// Type representing a CASManagerBuilder type
using CASManagerBuilderRef = System.IntPtr;

// Type representing a CASMediationManager type
using CASUManagerRef = System.IntPtr;

// Type representing a CASUView type
using CASUViewRef = System.IntPtr;

// Type representing a Unity AdView ad client.
using CASViewClientRef = System.IntPtr;

// Type representing a Unity MediationManager ad client.
using CASManagerClientRef = System.IntPtr;

// Type representing a NSObject<CASStatusHandler> type
using CASImpressionRef = System.IntPtr;

// Type representing a CASConsentFlow
using CASConsentFlowRef = System.IntPtr;
using UnityEngine;

namespace CAS.iOS
{
    // Externs used by the iOS component.
    internal class CASExterns
    {
        #region CAS Mediation Manager callbacks
        internal delegate void CASUInitializationCompleteCallback(CASUManagerRef manager, string error, string countryCode, bool withConsent, bool isTestMode);

        internal delegate void CASUDidLoadedAdCallback(CASUManagerRef manager);
        internal delegate void CASUDidFailedAdCallback(CASUManagerRef manager, int error);
        internal delegate void CASUWillPresentAdCallback(CASUManagerRef manager, CASImpressionRef impression);
        internal delegate void CASUDidShowAdFailedWithErrorCallback(CASUManagerRef manager, int error);
        internal delegate void CASUDidClickedAdCallback(CASUManagerRef manager);
        internal delegate void CASUDidCompletedAdCallback(CASUManagerRef manager);
        internal delegate void CASUDidClosedAdCallback(CASUManagerRef manager);
        #endregion

        #region CAS AdView callbacks
        internal delegate void CASUViewDidLoadCallback(CASUViewRef view);
        internal delegate void CASUViewDidFailedCallback(CASUViewRef view, int error);
        internal delegate void CASUViewWillPresentCallback(CASUViewRef view, CASImpressionRef impression);
        internal delegate void CASUViewDidClickedCallback(CASUViewRef view);
        internal delegate void CASUViewDidRectCallback(CASUViewRef view, float x, float y, float width, float height);
        #endregion

        internal delegate void CASUConsentFlowCompletion();
        internal delegate void CASUATTCompletion(int status);

        #region CAS Settings
        [DllImport("__Internal")]
        internal static extern void CASUSetTestDeviceWithIds(string[] testDeviceIDs, int testDeviceIDLength);

        [DllImport("__Internal")]
        internal static extern void CASUSetTrialAdFreeInterval(int interval);

        [DllImport("__Internal")]
        internal static extern int CASUGetTrialAdFreeInterval();

        [DllImport("__Internal")]
        internal static extern void CASUSetBannerRefreshRate(int interval);

        [DllImport("__Internal")]
        internal static extern int CASUGetBannerRefreshRate();

        [DllImport("__Internal")]
        internal static extern void CASUSetInterstitialInterval(int interval);

        [DllImport("__Internal")]
        internal static extern int CASUGetInterstitialInterval();

        [DllImport("__Internal")]
        internal static extern void CASURestartInterstitialInterval();

        [DllImport("__Internal")]
        internal static extern void CASUSetUserConsent(int consent);

        [DllImport("__Internal")]
        internal static extern int CASUGetUserConsent();

        [DllImport("__Internal")]
        internal static extern void CASUSetCCPAStatus(int status);

        [DllImport("__Internal")]
        internal static extern int CASUGetCCPAStatus();

        [DllImport("__Internal")]
        internal static extern void CASUSetAudienceTagged(int audience);

        [DllImport("__Internal")]
        internal static extern int CASUGetAudienceTagged();

        [DllImport("__Internal")]
        internal static extern void CASUSetDebugMode(bool mode);

        [DllImport("__Internal")]
        internal static extern bool CASUGetDebugMode();

        [DllImport("__Internal")]
        internal static extern void CASUSetMuteAdSounds(bool muted);

        [DllImport("__Internal")]
        internal static extern bool CASUGetMuteAdSounds();

        [DllImport("__Internal")]
        internal static extern void CASUSetLoadingWithMode(int mode);

        [DllImport("__Internal")]
        internal static extern int CASUGetLoadingMode();

        [DllImport("__Internal")]
        internal static extern void CASUSetInterstitialAdsWhenVideoCostAreLower(bool allow);

        [DllImport("__Internal")]
        internal static extern bool CASUGetInterstitialAdsWhenVideoCostAreLower();

        [DllImport("__Internal")]
        internal static extern void CASUSetTrackLocationEnabled(bool enabled);

        [DllImport("__Internal")]
        internal static extern bool CASUGetTrackLocationEnabled();

        [DllImport("__Internal")]
        internal static extern void CASUSetiOSAppPauseOnBackground(bool pause);

        [DllImport("__Internal")]
        internal static extern bool CASUGetiOSAppPauseOnBackground();
        #endregion

        #region User Targeting options
        [DllImport("__Internal")]
        internal static extern void CASUSetUserGender(int gender);

        [DllImport("__Internal")]
        internal static extern int CASUGetUserGender();

        [DllImport("__Internal")]
        internal static extern void CASUSetUserAge(int age);

        [DllImport("__Internal")]
        internal static extern int CASUGetUserAge();
        
        [DllImport("__Internal")]
        internal static extern void CASUSetContentURL(string contentURL);

        [DllImport("__Internal")]
        internal static extern string CASUGetContentURL();

        [DllImport("__Internal")]
        internal static extern void CASUSetKeywords(string[] keywords, int keywordsLength);
        #endregion

        #region Utils
        [DllImport("__Internal")]
        internal static extern string CASUGetSDKVersion();

        [DllImport("__Internal")]
        internal static extern string CASUValidateIntegration();

        [DllImport("__Internal")]
        internal static extern string CASUGetActiveMediationPattern();

        [DllImport("__Internal")]
        internal static extern bool CASUIsActiveMediationNetwork(int net);

        [DllImport("__Internal")]
        internal static extern void CASUOpenDebugger(CASUManagerRef manager);
        #endregion

        #region CAS Manager
        [DllImport("__Internal")]
        internal static extern CASManagerBuilderRef CASUCreateBuilder(
            int enableAd,
            bool demoAd,
            string unityVersion,
            string userID
        );

        [DllImport("__Internal")]
        internal static extern void CASUSetConsentFlow(
            CASManagerBuilderRef builderRef,
            bool isEnabled,
            string privacyUrl,
            CASUConsentFlowCompletion completion
        );

        [DllImport("__Internal")]
        internal static extern void CASUDisableConsentFlow(CASManagerBuilderRef builderRef);

        /// <summary>
        /// Set Mediation Extras to manager Builder
        /// </summary>
        /// <param name="builderRef">Builder ref from CASUCreateBuilder()</param>
        /// <param name="extraKeys">Array of extra keys</param>
        /// <param name="extraValues">Array of extra values</param>
        /// <param name="extrasCount">Count of extras</param>
        [DllImport("__Internal")]
        internal static extern void CASUSetMediationExtras(
            CASManagerBuilderRef builderRef,
            string[] extraKeys,
            string[] extraValues,
            int extrasCount
        );

        /// <summary>
        /// Initialize Manager from builder
        /// </summary>
        /// <param name="builderRef">Builder ref from CASUCreateBuilder()</param>
        /// <param name="client">C# CASMediationManager client ref</param>
        /// <returns>CASUTypeManagerRef</returns>
        [DllImport("__Internal")]
        internal static extern CASUManagerRef CASUInitializeManager(
            CASManagerBuilderRef builderRef,
            CASManagerClientRef client,
            CASUInitializationCompleteCallback onInit,
            string identifier
        );

        [DllImport("__Internal")]
        internal static extern void CASUFreeManager(CASUManagerRef managerRef);
        #endregion

        #region General Ads functions
        [DllImport("__Internal")]
        internal static extern bool CASUIsAdEnabledType(CASUManagerRef managerRef, int adType);

        [DllImport("__Internal")]
        internal static extern void CASUEnableAdType(CASUManagerRef managerRef, int adType, bool enable);

        [DllImport("__Internal")]
        internal static extern void CASUSetLastPageAdContent(CASUManagerRef managerRef, string contentJson);
        #endregion

        #region Interstitial Ads
        [DllImport("__Internal")]
        internal static extern void CASUSetInterstitialDelegate(
            CASUManagerRef managerRef,
            CASUDidLoadedAdCallback didLoad,
            CASUDidFailedAdCallback didFaied,
            CASUWillPresentAdCallback willPresent,
            CASUWillPresentAdCallback didImpression,
            CASUDidShowAdFailedWithErrorCallback didShowWithError,
            CASUDidClickedAdCallback didClick,
            CASUDidClosedAdCallback didClosed
        );

        [DllImport("__Internal")]
        internal static extern void CASULoadInterstitial(CASUManagerRef managerRef);

        [DllImport("__Internal")]
        internal static extern bool CASUIsInterstitialReady(CASUManagerRef managerRef);

        [DllImport("__Internal")]
        internal static extern bool CASUPresentInterstitial(CASUManagerRef managerRef);
        #endregion

        #region Rewarded Ads
        [DllImport("__Internal")]
        internal static extern void CASUSetRewardedDelegate(
            CASUManagerRef managerRef,
            CASUDidLoadedAdCallback didLoad,
            CASUDidFailedAdCallback didFaied,
            CASUWillPresentAdCallback willPresent,
            CASUWillPresentAdCallback didImpression,
            CASUDidShowAdFailedWithErrorCallback didShowWithError,
            CASUDidClickedAdCallback didClick,
            CASUDidCompletedAdCallback didComplete,
            CASUDidClosedAdCallback didClosed
        );

        [DllImport("__Internal")]
        internal static extern void CASULoadReward(CASUManagerRef managerRef);

        [DllImport("__Internal")]
        internal static extern bool CASUIsRewardedReady(CASUManagerRef managerRef);

        [DllImport("__Internal")]
        internal static extern bool CASUPresentRewarded(CASUManagerRef managerRef);
        #endregion

        #region AdView
        [DllImport("__Internal")]
        internal static extern CASUViewRef CASUCreateAdView(
            CASUManagerRef managerRef,
            CASViewClientRef client,
            int adSizeCode
        );

        [DllImport("__Internal")]
        internal static extern void CASUDestroyAdView(CASUViewRef viewRef, string key);

        [DllImport("__Internal")]
        internal static extern void CASUAttachAdViewDelegate(
            CASUViewRef viewRef,
            CASUViewDidLoadCallback didLoad,
            CASUViewDidFailedCallback didFailed,
            CASUViewWillPresentCallback willPresent,
            CASUViewDidClickedCallback didClicked,
            CASUViewDidRectCallback didRect);

        [DllImport("__Internal")]
        internal static extern void CASUPresentAdView(CASUViewRef viewRef);

        [DllImport("__Internal")]
        internal static extern void CASUHideAdView(CASUViewRef viewRef);

        [DllImport("__Internal")]
        internal static extern void CASUSetAdViewPosition(CASUViewRef viewRef, int posCode, int x, int y);

        [DllImport("__Internal")]
        internal static extern void CASUSetAdViewRefreshInterval(CASUViewRef viewRef, int interval);

        [DllImport("__Internal")]
        internal static extern void CASULoadAdView(CASUViewRef viewRef);

        [DllImport("__Internal")]
        internal static extern bool CASUIsAdViewReady(CASUViewRef viewRef);

        [DllImport("__Internal")]
        internal static extern int CASUGetAdViewRefreshInterval(CASUViewRef viewRef);
        #endregion

        #region App Return Ads

        [DllImport("__Internal")]
        internal static extern void CASUSetAppReturnDelegate(
            CASUManagerRef manager,
            CASUWillPresentAdCallback willOpen,
            CASUWillPresentAdCallback didImpression,
            CASUDidShowAdFailedWithErrorCallback didShowWithError,
            CASUDidClickedAdCallback didClick,
            CASUDidClosedAdCallback didClosed
        );

        [DllImport("__Internal")]
        internal static extern void CASUEnableAppReturnAds(CASUManagerRef manager);

        [DllImport("__Internal")]
        internal static extern void CASUDisableAppReturnAds(CASUManagerRef manager);

        [DllImport("__Internal")]
        internal static extern void CASUSkipNextAppReturnAds(CASUManagerRef manager);
        #endregion

        #region Ad Impression
        [DllImport("__Internal")]
        internal static extern int CASUGetImpressionNetwork(CASImpressionRef impression);

        [DllImport("__Internal")]
        internal static extern double CASUGetImpressionCPM(CASImpressionRef impression);

        [DllImport("__Internal")]
        internal static extern int CASUGetImpressionPrecission(CASImpressionRef impression);

        [DllImport("__Internal")]
        internal static extern string CASUGetImpressionCreativeId(CASImpressionRef impression);

        [DllImport("__Internal")]
        internal static extern string CASUGetImpressionIdentifier(CASImpressionRef impression);

        [DllImport("__Internal")]
        internal static extern int CASUGetImpressionDepth(CASImpressionRef impression);

        [DllImport("__Internal")]
        internal static extern double CASUGetImpressionLifetimeRevenue(CASImpressionRef impression);
        #endregion

        [DllImport("__Internal")]
        internal static extern void CASUShowConsentFlow(bool enabled, string policy, CASUConsentFlowCompletion completion);

        [DllImport("__Internal")]
        internal static extern void CASURequestATT(CASUATTCompletion callback);
        [DllImport("__Internal")]
        internal static extern int CASUGetATTStatus();
    }

    internal class CASExternCallbacks
    {
        internal static Action consentFlowComplete;
        private static ATTrackingStatus.CompleteHandler attTrackingComplete;

        internal static void ATTRequest(ATTrackingStatus.CompleteHandler callback)
        {
            if (attTrackingComplete != null)
            {
                if (callback != null)
                    attTrackingComplete += callback;
                return;
            }
            if (callback == null)
                attTrackingComplete = IgnoreResponsePlug;
            else
                attTrackingComplete = callback;

            CASExterns.CASURequestATT(ATTRequestCompleted);
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUConsentFlowCompletion))]
        internal static void OnConsentFlowCompletion()
        {
            try
            {
                // Callback in UI Thread from native side
                if (consentFlowComplete != null)
                    consentFlowComplete();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            consentFlowComplete = null;
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUATTCompletion))]
        internal static void ATTRequestCompleted(int status)
        {
            try
            {
                // Callback in UI Thread from native side
                if (attTrackingComplete != null)
                    attTrackingComplete((ATTrackingStatus.AuthorizationStatus)status);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            attTrackingComplete = null;
        }

        private static void IgnoreResponsePlug(ATTrackingStatus.AuthorizationStatus status) { }
    }
}
#endif