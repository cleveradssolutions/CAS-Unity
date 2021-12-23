#if UNITY_IOS || ( CASDeveloper && UNITY_EDITOR )
#define NATIVE_REQUEST
#endif

using System;
using UnityEngine;

namespace CAS
{
    /// <summary>
    /// A class that provides a tracking authorization request and the tracking authorization status of the app.
    /// </summary>
    public static class ATTrackingStatus
    {
        /// <summary>
        /// The status values for app tracking authorization.
        /// <a href="https://developer.apple.com/documentation/apptrackingtransparency/attrackingmanager/authorizationstatus">ATTrackingManager.AuthorizationStatus</a>
        /// </summary>
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
        /// This method allows you to <a href="https://developer.apple.com/documentation/apptrackingtransparency/attrackingmanager/3547037-requesttrackingauthorization">request the user permission dialogue</a>.
        /// </summary>
        public static void Request()
        {
            Request( null );
        }

        /// <summary>
        /// The request for user authorization to access app-related data.
        /// This method allows you to <a href="https://developer.apple.com/documentation/apptrackingtransparency/attrackingmanager/3547037-requesttrackingauthorization">request the user permission dialogue</a>.
        /// </summary>
        /// <exception cref="InvalidOperationException">App tracking transparency request is already triggered and awaiting completion</exception>
        public static void Request( CompleteHandler callback )
        {
            if (_completeCallback != null)
                throw new InvalidOperationException( "App tracking transparency request is already triggered and awaiting completion" );

            _completeCallback = callback;

#if NATIVE_REQUEST
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                CAS.iOS.CASExterns.CASURequestATT( ATTRequestCompleted );
                return;
            }
#endif
            ATTRequestCompleted( ( int )AuthorizationStatus.NotDetermined );
        }

        /// <summary>
        /// The authorization status that is current for the calling application.
        /// This method allows you to check the app tracking transparency (ATT) <a href="https://developer.apple.com/documentation/apptrackingtransparency/attrackingmanager/3547038-trackingauthorizationstatus">authorization status</a>.
        /// </summary>
        public static AuthorizationStatus GetStatus()
        {
#if NATIVE_REQUEST
            if (Application.platform == RuntimePlatform.IPhonePlayer)
                return ( AuthorizationStatus )CAS.iOS.CASExterns.CASUGetATTStatus();
#endif
            return AuthorizationStatus.NotDetermined;
        }


        #region Implementation

        private static CompleteHandler _completeCallback = null;

#if NATIVE_REQUEST
        [AOT.MonoPInvokeCallback( typeof( CAS.iOS.CASExterns.CASUATTCompletion ) )]
#endif
        private static void ATTRequestCompleted( int status )
        {
            try
            {
                if (_completeCallback != null)
                {
                    _completeCallback( ( AuthorizationStatus )status );
                    _completeCallback = null;
                }
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }
        #endregion
    }
}