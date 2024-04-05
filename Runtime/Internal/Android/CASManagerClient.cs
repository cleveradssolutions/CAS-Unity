//  Copyright © 2024 CAS.AI. All rights reserved.

#if UNITY_ANDROID || (CASDeveloper && UNITY_EDITOR)
using System;
using UnityEngine;

namespace CAS.Android
{
    internal sealed class CASManagerClient : CASManagerBase, CASCallback.Handler
    {
        private AndroidJavaObject _managerBridge;
        private CASCallback _managerCallback;

        internal override void Init(CASInitSettings initSettings)
        {
            base.Init(initSettings);

            EventExecutor.Initialize();

            _managerCallback = new CASCallback(this);

            using (var builder = new AndroidJavaObject(CASJavaBridge.BridgeBuilderClass))
            {
                if (isTestAdMode)
                    builder.Call("enableTestMode");

                if (!string.IsNullOrEmpty(initSettings.userID))
                    builder.Call("setUserId", initSettings.userID);

                if (initSettings.consentFlow != null)
                {
                    if (!initSettings.consentFlow.isEnabled)
                        builder.Call("disableConsentFlow");
                    else
                        builder.Call("withConsentFlow", new CASConsentFlowClient(initSettings.consentFlow, false));
                }
                if (initSettings.extras != null)
                {
                    foreach (var extra in initSettings.extras)
                    {
                        builder.Call("addExtras", extra.Key, extra.Value);
                    }
                }

                _managerBridge = builder.Call<AndroidJavaObject>("build",
                    initSettings.targetId, Application.unityVersion, _managerCallback, (int)initSettings.allowedAdFlags);
            }
        }

        protected override void SetLastPageAdContentNative(string json)
        {
            _managerBridge.Call("setLastPageAdContent", json);
        }

        public override bool IsEnabledAd(AdType adType)
        {
            return _managerBridge.Call<bool>("isEnabled", (int)adType);
        }

        public override void SetEnableAd(AdType adType, bool enabled)
        {
            _managerBridge.Call("enableAd", (int)adType, enabled);
        }

        public override bool IsReadyAd(AdType adType)
        {
            return _managerBridge.Call<bool>("isAdReady", (int)adType);
        }

        public override void LoadAd(AdType adType)
        {
            _managerBridge.Call("loadAd", (int)adType);
        }

        public override void ShowAd(AdType adType)
        {
            _managerBridge.Call("showAd", (int)adType);
        }

        [UnityEngine.Scripting.Preserve]
        public bool TryOpenDebugger()
        {
            return _managerBridge.Call<bool>("tryOpenDebugger");
        }

        public override void SetAppReturnAdsEnabled(bool enable)
        {
            _managerBridge.Call("setAutoShowAdOnAppReturn", enable);
        }

        public override void SkipNextAppReturnAds()
        {
            _managerBridge.Call("skipNextReturnAds");
        }

        protected override IAdView CreateAdView(AdSize size)
        {
            return new CASViewClient(this, size, _managerBridge);
        }

        public override AdMetaData WrapImpression(AdType adType, object impression)
        {
            return new CASImpressionClient(adType, (AndroidJavaObject)impression);
        }
    }
}
#endif
