//  Copyright © 2024 CAS.AI. All rights reserved.

// Used for iOS and Editor runtime
#if UNITY_EDITOR || UNITY_IOS

using System;
using UnityEngine;

namespace CAS.Unity
{
    [AddComponentMenu("")]
    internal class AppStateEventClient : MonoBehaviour, IAppStateEventClient
    {
        public event Action OnApplicationPaused;
        public event Action OnApplicationResumed;

        internal static IAppStateEventClient Create()
        {
            GameObject obj = new GameObject("CASAppStateEventClient");
            obj.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(obj);
            return obj.AddComponent<AppStateEventClient>();
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
            {
                if (OnApplicationPaused != null)
                    OnApplicationPaused();
            }
            else
            {
                if (OnApplicationResumed != null)
                    OnApplicationResumed();
            }
        }
    }

}

#endif