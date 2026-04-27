/*
 * Copyright 2025 CleverAdsSolutions LTD, CAS.AI
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#if UNITY_IOS || ( CASDeveloper && UNITY_EDITOR )
#define NATIVE_REQUEST
#endif

using UnityEngine;

namespace CAS
{
    /// <summary>
    /// A class that provides a tracking authorization request and the tracking authorization status of the app.
    /// </summary>
    [WikiPage("https://docs.page/cleveradssolutions/docs/Unity/App-Tracking-Transparency")]
    public static class ATTrackingStatus
    {
        /// <summary>
        /// The status values for app tracking authorization.
        /// </summary>
        [WikiPage("https://developer.apple.com/documentation/apptrackingtransparency/attrackingmanager/authorizationstatus")]
        public enum AuthorizationStatus
        {
            /// <summary>
            /// The value that returns when the app can’t determine the user’s authorization status
            /// for access to app-related data for tracking the user or the device.
            /// </summary>
            NotDetermined = 0,
            /// <summary>
            /// The value that returns if authorization to access app-related data
            /// for tracking the user or the device has a restricted status.
            /// </summary>
            Restricted,
            /// <summary>
            /// The value that returns if the user denies authorization to access app-related data
            /// for tracking the user or the device.
            /// </summary>
            Denied,
            /// <summary>
            /// The value that returns if the user authorizes access to app-related data
            /// for tracking the user or the device.
            /// </summary>
            Authorized
        }

        public delegate void CompleteHandler(AuthorizationStatus status);

        /// <summary>
        /// The request for user authorization to access app-related data.
        /// This method allows you to request the user permission dialogue.
        /// </summary>
        [WikiPage("https://developer.apple.com/documentation/apptrackingtransparency/attrackingmanager/3547037-requesttrackingauthorization")]
        public static void Request()
        {
            Request(null);
        }

        /// <summary>
        /// The request for user authorization to access app-related data.
        /// This method allows you to request the user permission dialogue.
        /// </summary>
        [WikiPage("https://developer.apple.com/documentation/apptrackingtransparency/attrackingmanager/3547037-requesttrackingauthorization")]
        public static void Request(CompleteHandler callback)
        {
#if NATIVE_REQUEST
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                CAS.iOS.CASExternCallbacks.ATTRequest(callback);
                return;
            }
#endif
            if (callback != null)
                callback(AuthorizationStatus.NotDetermined);
        }

        /// <summary>
        /// The authorization status that is current for the calling application.
        /// This method allows you to check the app tracking transparency (ATT) authorization status.
        /// </summary>
        [WikiPage("https://developer.apple.com/documentation/apptrackingtransparency/attrackingmanager/3547038-trackingauthorizationstatus")]
        public static AuthorizationStatus GetStatus()
        {
#if NATIVE_REQUEST
            if (Application.platform == RuntimePlatform.IPhonePlayer)
                return (AuthorizationStatus)CAS.iOS.CASExterns.CASUGetATTStatus();
#endif
            return AuthorizationStatus.NotDetermined;
        }

    }
}