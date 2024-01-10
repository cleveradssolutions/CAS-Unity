//  Copyright © 2024 CAS.AI. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace CAS.AdObject
{
    [AddComponentMenu("CleverAdsSolutions/Initialize Manager Ad Object")]
    [DisallowMultipleComponent]
    [HelpURL("https://github.com/cleveradssolutions/CAS-Unity/wiki/Manager-Ad-object")]
    public class ManagerAdObject : MonoBehaviour, IInternalAdObject
    {
        public ManagerIndex managerId;
        [SerializeField]
        private bool initializeOnAwake = true;
        public CCPAStatus metaDataProcessing = CCPAStatus.Undefined;
        public ConsentStatus metaAdvertiserTracking = ConsentStatus.Undefined;

        public UnityEvent OnInitialized;
        public CASUEventWithError OnInitializationFailed;

        public void Initialize()
        {
            CASFactory.UnsubscribeReadyManagerAsync(this, managerId.index);

            var builder = MobileAds.BuildManager()
               .WithManagerIdAtIndex(managerId.index)
               .WithCompletionListener(OnManagerReady);

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

            builder.Initialize();
        }


        #region MonoBehaviour
        private void Start()
        {
            if (initializeOnAwake)
                Initialize();
            else
                CASFactory.TryGetManagerByIndexAsync(this, managerId.index);
        }

        private void OnDestroy()
        {
            CASFactory.UnsubscribeReadyManagerAsync(this, managerId.index);
        }

        public void OnManagerReady(InitialConfiguration config)
        {
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
