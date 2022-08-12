//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2022 CleverAdsSolutions. All rights reserved.
//

#if UNITY_ANDROID || (CASDeveloper && UNITY_EDITOR)
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CAS.Android
{
    internal static class CASJavaProxy
    {
        #region Clever Ads Solutions SDK class names
        internal const string NativeBridgeClassName = "com.cleversolutions.ads.unity.CASBridge";
        internal const string NativeSettingsClassName = "com.cleversolutions.ads.unity.CASBridgeSettings";
        //internal const string NativeViewClassName = "com.cleversolutions.ads.unity.CASViewWrapper";
        internal const string AdCallbackClassName = "com.cleversolutions.ads.unity.CASCallback";
        internal const string OnInitializationListenerClassName = "com.cleversolutions.ads.unity.CASInitCallback";

        internal const string UnityActivityClassName = "com.unity3d.player.UnityPlayer";
        internal const string JavaUtilArrayList = "java.util.ArrayList";
        #endregion

        internal static AndroidJavaObject GetJavaListObject( List<string> csTypeList )
        {
            AndroidJavaObject javaTypeArrayList = new AndroidJavaObject( JavaUtilArrayList );
            for (int i = 0; i < csTypeList.Count; i++)
                javaTypeArrayList.Call<bool>( "add", csTypeList[i] );
            return javaTypeArrayList;
        }

        internal static AndroidJavaObject GetUnityActivity()
        {
            AndroidJavaClass playerClass = new AndroidJavaClass( UnityActivityClassName );
            return playerClass.GetStatic<AndroidJavaObject>( "currentActivity" );
        }
    }

    internal class InitializationListenerProxy : AndroidJavaProxy
    {
        private readonly CASManagerClient manager;
        private InitCompleteAction initCompleteAction;

        public InitializationListenerProxy( CASManagerClient manager, InitCompleteAction initCompleteAction )
            : base( CASJavaProxy.OnInitializationListenerClassName )
        {
            this.manager = manager;
            this.initCompleteAction = initCompleteAction;
        }

        public void onCASInitialized( string error, bool isTestMode )
        {
            CASFactory.UnityLog( "OnInitialization " + error );
            manager.OnInitializationCallback( isTestMode );
            if (initCompleteAction != null)
            {
                CASFactory.ExecuteEvent( () =>
                {
                    if (string.IsNullOrEmpty( error ))
                        initCompleteAction( true, null );
                    else
                        initCompleteAction( false, error );

                    initCompleteAction = null;
                } );
            }
        }
    }

    internal class AdEventsProxy : AndroidJavaProxy
    {
        private readonly AdType adType;
        public event Action OnAdLoaded;
        public event CASEventWithAdError OnAdFailed;
        public event Action OnAdShown;
        public event CASEventWithMeta OnAdOpening;
        public event CASEventWithError OnAdFailedToShow;
        public event Action OnAdClicked;
        public event Action OnAdCompleted;
        public event Action OnAdClosed;
        public event Action<Rect> OnAdRect;

        public AdEventsProxy( AdType adType ) : base( CASJavaProxy.AdCallbackClassName )
        {
            this.adType = adType;
        }

        public void onLoaded()
        {
            CASFactory.UnityLog( "onLoaded " + adType.ToString() );
            CASFactory.ExecuteEvent( OnAdLoaded );
        }

        public void onFailed( int error )
        {
            CASFactory.UnityLog( "onFailed " + adType.ToString() + " error: " + Enum.GetName( typeof( AdError ), error ) );
            if (OnAdFailed != null)
            {
                CASFactory.ExecuteEvent( () =>
                {
                    if (OnAdFailed != null) OnAdFailed( ( AdError )error );
                } );
            }
        }

        public void onOpening( string parameters )
        {
            CASFactory.UnityLog( "onOpening " + adType.ToString() );
            CASFactory.ExecuteEvent( OnAdShown );
            if (OnAdOpening != null)
            {
                CASFactory.ExecuteEvent( () =>
                {
                    if (OnAdOpening != null)
                        OnAdOpening( new CASImpressionClient( adType, parameters ) );
                } );
            }
        }

        public void onShowFailed( string message )
        {
            if (CASFactory.isDebug)
                Debug.Log( "[CleverAdsSolutions] onShowFailed " + adType.ToString() + " with error: " + message );
            if (OnAdFailedToShow != null)
            {
                CASFactory.ExecuteEvent( () =>
                {
                    if (OnAdFailedToShow != null)
                        OnAdFailedToShow( message );
                } );
            }
        }

        public void onClicked()
        {
            if (CASFactory.isDebug)
                Debug.Log( "[CleverAdsSolutions] onClicked " + adType.ToString() );
            CASFactory.ExecuteEvent( OnAdClicked );
        }

        public void onComplete()
        {
            if (CASFactory.isDebug)
                Debug.Log( "[CleverAdsSolutions] onComplete " + adType.ToString() );
            CASFactory.ExecuteEvent( OnAdCompleted );
        }

        public void onClosed()
        {
            if (CASFactory.isDebug)
                Debug.Log( "[CleverAdsSolutions] onClosed " + adType.ToString() );
            CASFactory.ExecuteEvent( OnAdClosed );
        }

        public void onRect( int x, int y, int width, int height )
        {
            if (OnAdRect != null)
            {
                var rect = new Rect( x, y, width, height );
                CASFactory.ExecuteEvent( () =>
                {
                    if (OnAdRect != null)
                        OnAdRect( rect );
                } );
            }
        }
    }
}
#endif