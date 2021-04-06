using System;
using UnityEngine;
using UnityEngine.Events;

namespace CAS.AdObject
{
    [AddComponentMenu( "CleverAdsSolutions/Initialize Manager Ad Object" )]
    [DisallowMultipleComponent]
    [HelpURL( "https://github.com/cleveradssolutions/CAS-Unity/wiki/Manager-Ad-object" )]
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
