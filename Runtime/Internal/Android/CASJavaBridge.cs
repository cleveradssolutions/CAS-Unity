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
        internal const string pluginPackage = "com.cleveradssolutions.plugin.unity";
        internal const string bridgeBuilderClass = pluginPackage + ".CASBridgeBuilder";
        internal const string settingsClass = pluginPackage + ".CASBridgeSettings";
        internal const string adCallbackClass = pluginPackage + ".CASCallback";
        internal const string adViewClass = pluginPackage + ".CASView";
        internal const string adViewCallbackClass = pluginPackage + ".CASViewCallback";
        internal const string consentFlowClass = pluginPackage + ".CASConsentFlow";
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

    internal class CASConsentFlowClient : IDisposable
    {
        internal readonly AndroidJavaObject obj;

        internal CASConsentFlowClient(ConsentFlow flow)
        {
            obj = new AndroidJavaObject(CASJavaBridge.consentFlowClass);
            if (!flow.isEnabled)
                obj.Call("disable");
            if (flow.privacyPolicyUrl != null)
                obj.Call("withPrivacyPolicy", flow.privacyPolicyUrl);
            if (flow.OnCompleted != null)
                obj.Call("withDismissListener", new AndroidJavaRunnable(flow.OnCompleted));
        }

        internal void show()
        {
            obj.Call("show");
        }

        public void Dispose()
        {
            obj.Dispose();
        }
    }
}
#endif