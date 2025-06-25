//  Copyright © 2025 CAS.AI. All rights reserved.

#if UNITY_ANDROID || (CASDeveloper && UNITY_EDITOR)
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CAS.Android
{
    // Attention: AndroidJavaObject cannot be used for inheritance.
    internal static class CASJavaBridge
    {
        #region Clever Ads Solutions SDK class names
        internal const string PluginPackage = "com.cleveradssolutions.plugin.unity";
        internal const string BridgeBuilderClass = PluginPackage + ".CASBridgeBuilder";
        internal const string SettingsClass = PluginPackage + ".CASBridgeSettings";
        internal const string AdCallbackClass = PluginPackage + ".CASCallback";
        internal const string AdViewClass = PluginPackage + ".CASView";
        internal const string ConsentFlowClass = PluginPackage + ".CASConsentFlow";
        internal const string SimpleCallbackClass = PluginPackage + ".CASSimpleCallback";
        internal const string AppStateEventNotifierClass = PluginPackage + ".AppStateEventNotifier";
        #endregion

        internal static volatile bool executeEventsOnUnityThread = true;

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
    }

    internal class CASCallback : AndroidJavaProxy
    {
        internal interface Handler
        {
            void HandleCallback(int action, int type, int error, string errorMessage, object impression);
        }

        private readonly Handler _client;
        internal CASCallback(Handler client) : base(CASJavaBridge.AdCallbackClass)
        {
            _client = client;
        }

        public override AndroidJavaObject Invoke(string methodName, object[] args)
        {
            int action = (int)args[0];

            switch (action)
            {
                case AdActionCode.INIT:
                    string initError;
                    string countryCode;
                    bool isConsentRequired;
                    bool isTestMode;
                    int consentFlowStatus;
                    try
                    {
                        initError = args[1] as string;
                        countryCode = args[2] as string;
                        isConsentRequired = (bool)args[3];
                        isTestMode = (bool)args[4];
                        consentFlowStatus = (int)args[5];
                    }
                    catch (Exception e)
                    {
                        throw new ArgumentException("[CAS.AI] Initialization", e);
                    }
                    CASJavaBridge.ExecuteEvent(() =>
                    {
                        ((CASManagerClient)_client).OnInitialized(initError, countryCode, isConsentRequired, isTestMode, consentFlowStatus);
                    });
                    break;
                case AdActionCode.VIEW_RECT:
                    try
                    {
                        ((CASViewClient)_client).rectInPixels =
                            new Rect((int)args[1], (int)args[2], (int)args[3], (int)args[4]);
                    }
                    catch (Exception e)
                    {
                        throw new ArgumentException("[CAS.AI] View rect", e);
                    }
                    break;
                default:
                    try
                    {
                        int type = (int)args[1];
                        int error = (int)args[2];
                        string errorMessage = args[3] as string;
                        AndroidJavaObject impression = args[4] as AndroidJavaObject;
                        CASJavaBridge.ExecuteEvent(() => _client.HandleCallback(action, type, error, errorMessage, impression));
                    }
                    catch (Exception e)
                    {
                        throw new ArgumentException("[CAS.AI] Action " + action, e);
                    }
                    break;
            }
            return null;
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

        public override AndroidJavaObject Invoke(string methodName, object[] args)
        {
            int status = (int)args[0];
            CASFactory.UnityLog("Callback App State " + status);
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
            return null;
        }
    }

    internal class CASConsentFlowCallback : AndroidJavaProxy
    {
        private Action<ConsentFlow.Status> OnResult;

        internal CASConsentFlowCallback(Action<ConsentFlow.Status> OnResult) : base(CASJavaBridge.SimpleCallbackClass)
        {
            this.OnResult = OnResult;
        }

        public override AndroidJavaObject Invoke(string methodName, object[] args)
        {
            var status = (ConsentFlow.Status)(int)args[0];
            CASFactory.HandleConsentFlow(null, status);
            if (OnResult != null)
            {
                var callback = OnResult; // Use local variable for lambda
                OnResult = null;

                if (CASJavaBridge.executeEventsOnUnityThread)
                {
                    EventExecutor.Add(() => callback(status));
                }
                else
                {
                    try
                    {
                        callback(status);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
            return null;
        }
    }

    internal class CASConsentFlowClient : IDisposable
    {
        public AndroidJavaObject obj;
        internal CASConsentFlowClient(ConsentFlow flow, bool forceTesting)
        {
            obj = new AndroidJavaObject(CASJavaBridge.ConsentFlowClass, (int)flow.debugGeography);
            if (!flow.isEnabled)
                obj.Call("disable");
            if (forceTesting)
                obj.Call("forceTesting");
            if (!string.IsNullOrEmpty(flow.privacyPolicyUrl))
                obj.Call("withPrivacyPolicy", flow.privacyPolicyUrl);
            if (flow.OnResult != null)
                obj.Call("withDismissListener", new CASConsentFlowCallback(flow.OnResult));
        }

        internal void Show(bool ifRequired)
        {
            EventExecutor.Initialize(); // Required for callbacks
            obj.Call("show", ifRequired);
        }

        public void Dispose()
        {
            obj.Dispose();
        }
    }
}
#endif