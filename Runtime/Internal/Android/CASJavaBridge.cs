//  Copyright © 2024 CAS.AI. All rights reserved.

#if UNITY_ANDROID || (CASDeveloper && UNITY_EDITOR)
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CAS.Android
{
    internal static class CASJavaBridge
    {
        #region Clever Ads Solutions SDK class names
        internal const string PluginPackage = "com.cleveradssolutions.plugin.unity";
        internal const string BridgeBuilderClass = PluginPackage + ".CASBridgeBuilder";
        internal const string SettingsClass = PluginPackage + ".CASBridgeSettings";
        internal const string AdCallbackClass = PluginPackage + ".CASCallback";
        internal const string AdViewClass = PluginPackage + ".CASView";
        internal const string AdViewCallbackClass = PluginPackage + ".CASViewCallback";
        internal const string ConsentFlowClass = PluginPackage + ".CASConsentFlow";
        internal const string SimpleCallbackClass = PluginPackage + ".CASSimpleCallback";
        internal const string AppStateEventNotifierClass = PluginPackage + ".AppStateEventNotifier";
        #endregion

        internal static volatile bool executeEventsOnUnityThread = true;

        internal static void ExecuteEvent(CASEventWithMeta adEvent, AdType adType, AndroidJavaObject impression)
        {
            if (adEvent == null) return;
            if (executeEventsOnUnityThread)
            {
                EventExecutor.Add(() => adEvent(new CASImpressionClient(adType, impression)));
                return;
            }
            try
            {
                adEvent(new CASImpressionClient(adType, impression));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        internal static void ExecuteEvent(Action adEvent)
        {
            if (adEvent == null) return;
            if (executeEventsOnUnityThread)
            {
                EventExecutor.Add(adEvent);
                return;
            }
            try
            {
                adEvent();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        internal static void ExecuteEvent(CASEventWithAdError adEvent, int error)
        {
            if (adEvent == null) return;
            if (executeEventsOnUnityThread)
            {
                EventExecutor.Add(() => adEvent((AdError)error));
                return;
            }
            try
            {
                adEvent((AdError)error);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    internal class CASAppStateEventClient : AndroidJavaProxy, IAppStateEventClient
    {
        public event Action OnApplicationBackground;
        public event Action OnApplicationForeground;

#pragma warning disable IDE0052
        private readonly AndroidJavaObject _bridge;
#pragma warning restore IDE0052

        internal CASAppStateEventClient() : base(CASJavaBridge.SimpleCallbackClass)
        {
            _bridge = new AndroidJavaObject(CASJavaBridge.AppStateEventNotifierClass, this);
        }

        public void onNativeCallback(int status)
        {
            if (status == 1)
            {
                if (OnApplicationForeground != null)
                    OnApplicationForeground();
            }
            else
            {
                if (OnApplicationBackground != null)
                    OnApplicationBackground();
            }
        }
    }

    internal class CASConsentFlowCallback : AndroidJavaProxy
    {
        private Action OnCompleted;
        private Action<ConsentFlow.Status> OnResult;

        internal CASConsentFlowCallback(Action<ConsentFlow.Status> OnResult, Action OnCompleted) : base(CASJavaBridge.SimpleCallbackClass)
        {
            EventExecutor.Initialize();
            this.OnCompleted = OnCompleted;
            this.OnResult = OnResult;
        }

        public void onNativeCallback(int status)
        {
            if (OnResult != null)
            {
                var callback = OnResult; // Use local variable for lambda
                OnResult = null;

                if (CASJavaBridge.executeEventsOnUnityThread)
                {
                    EventExecutor.Add(() => callback((ConsentFlow.Status)status));
                }
                else
                {
                    try
                    {
                        callback((ConsentFlow.Status)status);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
            CASJavaBridge.ExecuteEvent(OnCompleted);
            OnCompleted = null;
        }
    }

    internal class CASConsentFlowClient : AndroidJavaObject
    {
        internal CASConsentFlowClient(ConsentFlow flow, bool forceTesting)
            : base(CASJavaBridge.ConsentFlowClass, (int)flow.debugGeography)
        {
            if (!flow.isEnabled)
                Call("disable");
            if (forceTesting)
                Call("forceTesting");
            if (!string.IsNullOrEmpty(flow.privacyPolicyUrl))
                Call("withPrivacyPolicy", flow.privacyPolicyUrl);
            if (flow.OnResult != null || flow.OnCompleted != null)
                Call("withDismissListener", new CASConsentFlowCallback(flow.OnResult, flow.OnCompleted));
        }

        internal void Show(bool ifRequired)
        {
            Call(ifRequired ? "showIfRequired" : "show");
        }
    }
}
#endif