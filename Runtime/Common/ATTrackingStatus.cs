//  Copyright © 2024 CAS.AI. All rights reserved.

#if UNITY_IOS || ( CASDeveloper && UNITY_EDITOR )
#define NATIVE_REQUEST
#endif

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CAS
{
    /// <summary>
    /// A class that provides a tracking authorization request and the tracking authorization status of the app.
    /// </summary>
    [WikiPage("https://github.com/cleveradssolutions/CAS-Unity/wiki/App-Tracking-Transparency")]
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