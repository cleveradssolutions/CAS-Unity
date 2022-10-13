//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2022 CleverAdsSolutions. All rights reserved.
//

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
    [WikiPage( "https://github.com/cleveradssolutions/CAS-Unity/wiki/App-Tracking-Transparency" )]
    public static class ATTrackingStatus
    {
        /// <summary>
        /// The status values for app tracking authorization.
        /// </summary>
        [WikiPage( "https://developer.apple.com/documentation/apptrackingtransparency/attrackingmanager/authorizationstatus" )]
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

        public delegate void CompleteHandler( AuthorizationStatus status );

        /// <summary>
        /// The request for user authorization to access app-related data.
        /// This method allows you to request the user permission dialogue.
        /// </summary>
        [WikiPage( "https://developer.apple.com/documentation/apptrackingtransparency/attrackingmanager/3547037-requesttrackingauthorization" )]
        public static void Request()
        {
            Request( null );
        }

        /// <summary>
        /// The request for user authorization to access app-related data.
        /// This method allows you to request the user permission dialogue.
        /// </summary>
        [WikiPage( "https://developer.apple.com/documentation/apptrackingtransparency/attrackingmanager/3547037-requesttrackingauthorization" )]
        public static void Request( CompleteHandler callback = null )
        {
            if (_completeCallback != null)
            {
                if (callback != null)
                    _completeCallback += callback;
                return;
            }
            if (callback == null)
                _completeCallback = IgnoreResponsePlug;
            else
                _completeCallback = callback;

#if NATIVE_REQUEST
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                CASURequestATT( ATTRequestCompleted );
                return;
            }
#endif
            ATTRequestCompleted( ( int )AuthorizationStatus.NotDetermined );
        }

        /// <summary>
        /// The authorization status that is current for the calling application.
        /// This method allows you to check the app tracking transparency (ATT) authorization status.
        /// </summary>
        [WikiPage( "https://developer.apple.com/documentation/apptrackingtransparency/attrackingmanager/3547038-trackingauthorizationstatus" )]
        public static AuthorizationStatus GetStatus()
        {
#if NATIVE_REQUEST
            if (Application.platform == RuntimePlatform.IPhonePlayer)
                return ( AuthorizationStatus )CASUGetATTStatus();
#endif
            return AuthorizationStatus.NotDetermined;
        }


        #region Implementation
        private static CompleteHandler _completeCallback = null;

#if NATIVE_REQUEST
        private delegate void CASUATTCompletion( int status );

        [DllImport( "__Internal" )]
        private static extern void CASURequestATT( CASUATTCompletion callback );
        [DllImport( "__Internal" )]
        private static extern int CASUGetATTStatus();
#endif

#if NATIVE_REQUEST
        [AOT.MonoPInvokeCallback( typeof( CASUATTCompletion ) )]
#endif
        private static void ATTRequestCompleted( int status )
        {
            try
            {
                // Callback in UI Thread from native side
                if (_completeCallback != null)
                    _completeCallback( ( AuthorizationStatus )status );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
            _completeCallback = null;
        }

        private static void IgnoreResponsePlug( AuthorizationStatus status ) { }
        #endregion
    }
}