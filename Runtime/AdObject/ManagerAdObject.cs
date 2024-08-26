//  Copyright © 2024 CAS.AI. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace CAS.AdObject
{
    [AddComponentMenu("CleverAdsSolutions/Initialize Manager Ad Object")]
    [DisallowMultipleComponent]
    [HelpURL("https://github.com/cleveradssolutions/CAS-Unity/wiki/Manager-Ad-object")]
    public class ManagerAdObject : MonoBehaviour
    {
        public ManagerIndex managerId;
        [SerializeField]
        private bool initializeOnAwake = false;

        public bool consentFlowEnabled = true;
        public ConsentFlowAdObject consentFlow;

        public CCPAStatus metaDataProcessing = CCPAStatus.Undefined;
        public ConsentStatus metaAdvertiserTracking = ConsentStatus.Undefined;

        public UnityEvent OnInitialized;
        public CASUEventWithError OnInitializationFailed;

        public void Initialize()
        {
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || CASDeveloper
            CreateBuilder().Build();
#endif
        }

        public IManagerBuilder CreateBuilder()
        {
            var builder = MobileAds.BuildManager()
                           .WithManagerIdAtIndex(managerId.index);

            if (!consentFlowEnabled)
                builder.WithConsentFlow(new ConsentFlow(false));
            else if (consentFlow)
                builder.WithConsentFlow(consentFlow.CreateFlow());

            if (metaDataProcessing != CCPAStatus.Undefined)
            {
                builder.WithMediationExtras(
                    MediationExtras.facebookDataProcessing,
                    metaDataProcessing == CCPAStatus.OptInSale ? "" : "LDU"
                );
            }
            if (metaAdvertiserTracking != ConsentStatus.Undefined)
            {
                builder.WithMediationExtras(
                    MediationExtras.facebookAdvertiserTracking,
                    metaAdvertiserTracking == ConsentStatus.Accepted ? "1" : "0"
                );
            }
            return builder;
        }


        #region MonoBehaviour
        private void Awake()
        {
            if (consentFlow)
                consentFlow.showOnAwakeIfRequired = false;
        }

        private void Start()
        {
            CASFactory.OnManagerStateChanged += OnManagerReady;
            if (initializeOnAwake)
                Initialize();
        }

        private void OnDestroy()
        {
            CASFactory.OnManagerStateChanged -= OnManagerReady;
        }

        private void OnManagerReady(int index, CASManagerBase manager)
        {
            if (!this || index != managerId.index) return;
            var config = manager.initialConfig;
            if (config == null) return;
            if (config.error == null)
                OnInitialized.Invoke();
            else
                OnInitializationFailed.Invoke(config.error);
        }
        #endregion
    }

    [Serializable]
    public class CASUEventWithError : UnityEvent<string> { }

    [Serializable]
    public class CASUEventWithMeta : UnityEvent<AdMetaData> { }

    [Serializable]
    public class CASUToggleEvent : UnityEvent<bool> { }

    [Serializable]
    public struct ManagerIndex
    {
#pragma warning disable 649
        [SerializeField]
        private int android;
        [SerializeField]
        private int ios;
#pragma warning restore 649

        public int index
        {
            get
            {
#if UNITY_ANDROID
                return android;
#elif UNITY_IOS
                return ios;
#else
                return 0;
#endif
            }
        }
    }
}
