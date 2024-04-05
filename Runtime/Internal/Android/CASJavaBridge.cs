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
            void HandleCallback(int action, int type, int error, object impression);
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
                    try
                    {
                        initError = args[1] as string;
                        countryCode = args[2] as string;
                        isConsentRequired = (bool)args[3];
                        isTestMode = (bool)args[4];
                    }
                    catch (Exception e)
                    {
                        throw new ArgumentException("[CAS.AI] Initialization", e);
                    }
                    CASJavaBridge.ExecuteEvent(() =>
                    {
                        ((CASManagerClient)_client).OnInitialized(initError, countryCode, isConsentRequired, isTestMode);
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
                    int type;
                    int error;
                    AndroidJavaObject impression;
                    try
                    {
                        action = (int)args[0];
                        type = (int)args[1];
                        error = (int)args[2];
                        impression = args[3] as AndroidJavaObject;
                    }
                    catch (Exception e)
                    {
                        throw new ArgumentException("[CAS.AI] Action " + action, e);
                    }
                    CASJavaBridge.ExecuteEvent(() => _client.HandleCallback(action, type, error, impression));
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
        private Action OnCompleted;
        private Action<ConsentFlow.Status> OnResult;

        internal CASConsentFlowCallback(Action<ConsentFlow.Status> OnResult, Action OnCompleted) : base(CASJavaBridge.SimpleCallbackClass)
        {
            EventExecutor.Initialize();
            this.OnCompleted = OnCompleted;
            this.OnResult = OnResult;
        }

        public override AndroidJavaObject Invoke(string methodName, object[] args)
        {
            int status = (int)args[0];
            CASFactory.UnityLog("Callback Consent Flow status " + status);
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
            return null;
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
            Call("show", ifRequired);
        }
    }
}
#endif