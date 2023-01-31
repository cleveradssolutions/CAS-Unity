//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2023 CleverAdsSolutions. All rights reserved.
//

#if UNITY_ANDROID || (CASDeveloper && UNITY_EDITOR)
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CAS.Android
{
    internal static class CASJavaBridge
    {
        #region Clever Ads Solutions SDK class names
        internal const string bridgeBuilderClass = "com.cleversolutions.ads.unity.CASBridgeBuilder";
        internal const string settingsClass = "com.cleversolutions.ads.unity.CASBridgeSettings";
        internal const string adCallbackClass = "com.cleversolutions.ads.unity.CASCallback";
        internal const string initCallbackClass = "com.cleversolutions.ads.unity.CASInitCallback";
        #endregion

        internal static void RepeatCall(string method, AndroidJavaObject target, Dictionary<string, string> args, bool staticCall)
        {
            if (args == null || args.Count == 0)
                return;

            var tempArgs = new String[] { "String", "String" };
            var methodID = AndroidJNIHelper.GetMethodID(target.GetRawClass(),
                        "addExtras", tempArgs, staticCall);

            foreach (var item in args)
            {
                tempArgs[0] = item.Key;
                tempArgs[1] = item.Value;
                var nativeArgs = AndroidJNIHelper.CreateJNIArgArray(tempArgs);
                try
                {
                    if (staticCall)
                        AndroidJNI.CallStaticVoidMethod(target.GetRawClass(), methodID, nativeArgs);
                    else
                        AndroidJNI.CallVoidMethod(target.GetRawObject(), methodID, nativeArgs);
                }
                finally
                {
                    AndroidJNIHelper.DeleteJNIArgArray(tempArgs, nativeArgs);
                }
            }
        }


        internal static void ImpressionEvent(CASEventWithMeta onEvent, AdType adType, AndroidJavaObject impression)
        {
            if (onEvent != null)
            {
                CASFactory.ExecuteEvent(() =>
                {
                    if (onEvent != null)
                        onEvent(new CASImpressionClient(adType, impression));
                });
            }
        }
    }

    internal class InitCallbackProxy : AndroidJavaProxy
    {
        private readonly CASManagerClient manager;

        public InitCallbackProxy(CASManagerClient manager)
            : base(CASJavaBridge.initCallbackClass)
        {
            this.manager = manager;
        }

        public void onCASInitialized(string error, bool isTestMode)
        {
            manager.OnInitializationCallback(error, isTestMode);
        }
    }

    internal class AdEventsProxy : AndroidJavaProxy
    {
        private readonly AdType adType;
        private readonly string adTypeName;
        public Action OnAdLoaded;
        public CASEventWithAdError OnAdFailed;
        public Action OnAdShown;
        public CASEventWithMeta OnAdOpening;
        public CASEventWithMeta OnAdImpression;
        public CASEventWithError OnAdFailedToShow;
        public Action OnAdClicked;
        public Action OnAdCompleted;
        public Action OnAdClosed;
        public Action<Rect> OnAdRect;

        public AdEventsProxy(AdType adType) : base(CASJavaBridge.adCallbackClass)
        {
            this.adType = adType;
            adTypeName = adType.ToString();
        }

        public void onLoaded()
        {
            CASFactory.UnityLog("Callback Loaded " + adTypeName);
            CASFactory.ExecuteEvent(OnAdLoaded);
        }

        public void onFailed(int error)
        {
            CASFactory.UnityLog("Callback Failed " + adTypeName + " error: " + error);
            if (OnAdFailed != null)
            {
                CASFactory.ExecuteEvent(() =>
                {
                    if (OnAdFailed != null)
                        OnAdFailed((AdError)error);
                });
            }
        }

        public void onOpening(AndroidJavaObject impression)
        {
            CASFactory.UnityLog("Callback Presented " + adTypeName);
            CASFactory.ExecuteEvent(OnAdShown);
            CASJavaBridge.ImpressionEvent(OnAdOpening, adType, impression);
        }

        public void onImpression(AndroidJavaObject impression)
        {
            CASFactory.UnityLog("Callback Impression " + adTypeName);
            CASJavaBridge.ImpressionEvent(OnAdImpression, adType, impression);
        }

        public void onShowFailed(int error)
        {
            CASFactory.UnityLog("Callback Failed to show " + adTypeName + " with error: " + error);
            if (OnAdFailedToShow != null)
            {
                CASFactory.ExecuteEvent(() =>
                {
                    if (OnAdFailedToShow != null)
                    {
                        var adError = (AdError)error;
                        OnAdFailedToShow(adError.GetMessage());
                    }
                });
            }
        }

        public void onClicked()
        {
            CASFactory.UnityLog("Callback Clicked " + adTypeName);
            CASFactory.ExecuteEvent(OnAdClicked);
        }

        public void onComplete()
        {
            CASFactory.UnityLog("Callback Complete " + adTypeName);
            CASFactory.ExecuteEvent(OnAdCompleted);
        }

        public void onClosed()
        {
            CASFactory.UnityLog("Callback Closed " + adTypeName);
            CASFactory.ExecuteEvent(OnAdClosed);
        }

        public void onRect(int x, int y, int width, int height)
        {
            if (OnAdRect != null)
            {
                var rect = new Rect(x, y, width, height);
                CASFactory.ExecuteEvent(() =>
                {
                    if (OnAdRect != null)
                        OnAdRect(rect);
                });
            }
        }
    }
}
#endif