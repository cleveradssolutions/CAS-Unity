using System;
using UnityEngine;
using UnityEngine.Events;

namespace CAS.AdObject
{
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
                return -1;
#endif
            }
        }
    }

    [AddComponentMenu( "CleverAdsSolutions/Initialzie Manager Ad Object" )]
    [DisallowMultipleComponent]
    public class ManagerAdObject : MonoBehaviour
    {
        public ManagerIndex managerId;
        [SerializeField]
        private bool initializeOnAwake = true;

        public UnityEvent OnInitialized;

        public void Initialize()
        {
            MobileAds.BuildManager()
                .WithManagerIdAtIndex( managerId.index )
                .WithInitListener( OnInitializeResult )
                .Initialize();
        }

        private void Start()
        {
            if (initializeOnAwake)
                Initialize();
        }

        private void OnInitializeResult( bool success, string error )
        {
            OnInitialized.Invoke();
        }
    }
}
