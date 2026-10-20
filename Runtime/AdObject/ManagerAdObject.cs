//  Copyright © 2025 CAS.AI. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace CAS.AdObject
{
    /// <summary>
    /// A component that initializes the <see cref="IMediationManager"/> without code,
    /// using <see cref="UnityEvent"/> callbacks configured in the Inspector.
    /// <para>Other AdObjects with the same <see cref="managerId"/> wait for this initialization.</para>
    /// </summary>
    [AddComponentMenu("CleverAdsSolutions/Initialize Manager Ad Object")]
    [DisallowMultipleComponent]
    [HelpURL("https://docs.page/cleveradssolutions/docs/Unity/Manager-Ad-object")]
    public class ManagerAdObject : MonoBehaviour
    {
        /// <summary>
        /// The index of the manager to initialize.
        /// </summary>
        public ManagerIndex managerId;
        [Tooltip("Initialize the manager automatically on comnponent start")]
        [SerializeField]
        private bool initializeOnAwake = false;

        /// <summary>
        /// Show the user consent flow during initialization if it is required.
        /// <para>If <c>false</c>, the consent flow is disabled.</para>
        /// </summary>
        public bool consentFlowEnabled = true;
        /// <summary>
        /// An optional <see cref="ConsentFlowAdObject"/> with consent flow settings.
        /// <para>If set, the consent flow is shown during initialization
        /// instead of on the <see cref="ConsentFlowAdObject"/> Start.</para>
        /// </summary>
        public ConsentFlowAdObject consentFlow;

        /// <summary>
        /// The Limited Data Use status for Meta Audience Network.
        /// Not passed to Meta when <see cref="CCPAStatus.Undefined"/>.
        /// </summary>
        public CCPAStatus metaDataProcessing = CCPAStatus.Undefined;
        /// <summary>
        /// The Advertiser Tracking status for Meta Audience Network.
        /// Not passed to Meta when <see cref="ConsentStatus.Undefined"/>.
        /// </summary>
        public ConsentStatus metaAdvertiserTracking = ConsentStatus.Undefined;

        /// <summary>
        /// Invoked when the manager is initialized successfully.
        /// </summary>
        public UnityEvent OnInitialized;
        /// <summary>
        /// Invoked with the error message when the manager initialization fails.
        /// </summary>
        public CASUEventWithError OnInitializationFailed;

        /// <summary>
        /// Initializes the manager with the settings of this component.
        /// <para>Does nothing on platforms other than Android, iOS and the Unity Editor.</para>
        /// </summary>
        public void Initialize()
        {
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || CASDeveloper
            CreateBuilder().Build();
#endif
        }

        /// <summary>
        /// Creates a manager builder with the settings of this component, without initializing the manager.
        /// <para>Use it to add more settings in code before calling <c>Build()</c>.</para>
        /// </summary>
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

    /// <summary>
    /// A <see cref="UnityEvent"/> with an error message.
    /// </summary>
    [Serializable]
    public class CASUEventWithError : UnityEvent<string> { }

    /// <summary>
    /// A <see cref="UnityEvent"/> with the ad impression metadata.
    /// </summary>
    [Serializable]
    public class CASUEventWithMeta : UnityEvent<AdMetaData> { }

    /// <summary>
    /// A <see cref="UnityEvent"/> with a <c>bool</c> state.
    /// </summary>
    [Serializable]
    public class CASUToggleEvent : UnityEvent<bool> { }

    /// <summary>
    /// The index of a CAS manager, set separately for Android and iOS in the Inspector.
    /// </summary>
    [Serializable]
    public struct ManagerIndex
    {
#pragma warning disable 649
        [SerializeField]
        private int android;
        [SerializeField]
        private int ios;
#pragma warning restore 649

        /// <summary>
        /// The manager index for the current platform. Returns 0 on other platforms.
        /// </summary>
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
