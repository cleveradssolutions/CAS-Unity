//  Copyright © 2025 CAS.AI. All rights reserved.

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

        private bool isApplicationFocusChanged = true;

        internal static IAppStateEventClient Create()
        {
            GameObject obj = new GameObject("CASAppStateEventClient");
            obj.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(obj);
            return obj.AddComponent<CASAppStateEventClient>();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            isApplicationFocusChanged = true;
        }

        private void OnApplicationPause(bool isPaused)
        {
            // There is a known issue where Unity pauses the app while a full-screen ad is displayed, 
            // but the application's focus does not change. 
            // We expect that Unity on expanding the application, 
            // trigger the focus change first, and then resume the app.
            if (!isApplicationFocusChanged)
                return;
            isApplicationFocusChanged = false;
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