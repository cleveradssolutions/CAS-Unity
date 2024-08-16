//  Copyright © 2024 CAS.AI. All rights reserved.

#if UNITY_IOS || (CASDeveloper && UNITY_EDITOR)
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CAS.iOS
{
    internal sealed class CASManagerClient : CASManagerBase
    {
        private IntPtr _managerRef;
        private IntPtr _managerClient;

        ~CASManagerClient()
        {
            try
            {
                _managerRef = IntPtr.Zero;
                ((GCHandle)_managerClient).Free();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        internal override void Init(CASInitSettings initSettings)
        {
            base.Init(initSettings);
            _managerClient = (IntPtr)GCHandle.Alloc(this);

            if (initSettings.userID == null)
                initSettings.userID = string.Empty; // Null string not supported

            CASExterns.CASUCreateBuilder(
                (int)initSettings.allowedAdFlags,
                isTestAdMode,
                Application.unityVersion,
                initSettings.userID
            );

            var consentFlow = initSettings.consentFlow;
            if (consentFlow != null)
            {
                if (consentFlow.isEnabled)
                {
                    CASExternCallbacks.ConsentFlow(consentFlow);
                    CASExterns.CASUSetConsentFlow(
                        consentFlow.isEnabled,
                        (int)consentFlow.debugGeography,
                        consentFlow.privacyPolicyUrl,
                        CASExternCallbacks.ConsentFlowCompletion);
                }
                else
                {
                    CASExterns.CASUDisableConsentFlow();
                }
            }

            if (initSettings.extras != null && initSettings.extras.Count != 0)
            {
                var extrasKeys = new string[initSettings.extras.Count];
                var extrasValues = new string[initSettings.extras.Count];
                int extraI = 0;
                foreach (var extra in initSettings.extras)
                {
                    extrasKeys[extraI] = extra.Key;
                    extrasValues[extraI] = extra.Value;
                    extraI++;
                }
                CASExterns.CASUSetMediationExtras(extrasKeys, extrasValues, extrasKeys.Length);
            }

            _managerRef = CASExterns.CASUBuildManager(_managerClient, InitializationCompleteCallback, managerID);

            CASExterns.CASUSetDelegates(_managerRef, AdActionCallback, AdImpressionCallback);
        }

        protected override void SetLastPageAdContentNative(string json)
        {
            CASExterns.CASUSetLastPageAdContent(_managerRef, json);
        }

        public override void EnableAd(AdType adType)
        {
            CASExterns.CASUEnableAdType(_managerRef, (int)adType, true);
        }

        public override void DisposeAd(AdType adType)
        {
            CASExterns.CASUEnableAdType(_managerRef, (int)adType, false);
        }

        public override bool IsReadyAd(AdType adType)
        {
            return CASExterns.CASUIsAdReady(_managerRef, (int)adType);
        }

        protected override void LoadAdNetive(AdType adType)
        {
            CASExterns.CASULoadAd(_managerRef, (int)adType);
        }

        public override void ShowAd(AdType adType)
        {
            CASExterns.CASUShowAd(_managerRef, (int)adType);
        }

        [UnityEngine.Scripting.Preserve]
        public bool TryOpenDebugger()
        {
            CASExterns.CASUOpenDebugger(_managerRef);
            return true;
        }

        public override void SetAppReturnAdsEnabled(bool enable)
        {
            CASExterns.CASUSetAutoShowAdOnAppReturn(_managerRef, enable);
        }

        public override void SkipNextAppReturnAds()
        {
            CASExterns.CASUSkipNextAppReturnAds(_managerRef);
        }

        protected override CASViewBase CreateAdView(AdSize size)
        {
            var view = new CASViewClient(this, size);
            var viewClient = (IntPtr)GCHandle.Alloc(view);
            view.Attach(CASExterns.CASUCreateAdView(_managerRef, viewClient, (int)size), viewClient);
            return view;
        }

        public override AdMetaData WrapImpression(AdType adType, object impression)
        {
            return new CASImpressionClient(adType, (IntPtr)impression);
        }

        #region Callback methods
        private static CASManagerClient GetClient(IntPtr managerClient)
        {
            GCHandle handle = (GCHandle)managerClient;
            return handle.Target as CASManagerClient;
        }


        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUInitializationCompleteCallback))]
        private static void InitializationCompleteCallback(IntPtr manager, string error, string countryCode, bool withConsent, bool isTestMode)
        {
            try
            {
                GetClient(manager).OnInitialized(error, countryCode, withConsent, isTestMode);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUActionCallback))]
        private static void AdActionCallback(IntPtr manager, int action, int type, int error)
        {
            try
            {
                GetClient(manager).HandleCallback(action, type, error, null, null);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CASExterns.CASUImpressionCallback))]
        private static void AdImpressionCallback(IntPtr manager, int action, int type, IntPtr impression)
        {
            try
            {
                GetClient(manager).HandleCallback(action, type, 0, null, impression);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        #endregion
    }
}
#endif