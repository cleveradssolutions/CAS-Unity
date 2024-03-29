//  Copyright © 2024 CAS.AI. All rights reserved.

// Used for Editor and iOS runtime
#if UNITY_EDITOR || UNITY_IOS

using System;
using UnityEngine;

namespace CAS.Unity
{
    [AddComponentMenu("")]
    internal class CASAppStateEventClient : MonoBehaviour, IAppStateEventClient
    {
        public event Action OnApplicationBackground;
        public event Action OnApplicationForeground;

        internal static IAppStateEventClient Create()
        {
            GameObject obj = new GameObject("CASAppStateEventClient");
            obj.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(obj);
            return obj.AddComponent<CASAppStateEventClient>();
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
            {
                if (OnApplicationBackground != null)
                    OnApplicationBackground();
            }
            else
            {
                if (OnApplicationForeground != null)
                    OnApplicationForeground();
            }
        }
    }

}

#endif