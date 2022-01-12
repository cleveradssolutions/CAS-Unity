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
        internal const string OnInitializationListenerClassName = "com.cleversolutions.ads.OnInitializationListener";

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
        private readonly CASMediationManager manager;
        private InitCompleteAction initCompleteAction;

        public InitializationListenerProxy(
            CASMediationManager manager,
            InitCompleteAction initCompleteAction )
            : base( CASJavaProxy.OnInitializationListenerClassName )
        {
            this.manager = manager;
            this.initCompleteAction = initCompleteAction;
        }

        public void onInitialization( bool success, AndroidJavaObject error )
        {
            onInitialization( success, "" );
        }

        public void onInitialization( bool success, string error )
        {
            if (CASFactory.isDebug)
                Debug.Log( "[CleverAdsSolutions] onInitialization " + success );
            manager.initializationListener = null;
            if (initCompleteAction != null)
            {
                CASFactory.ExecuteEvent( () =>
                {
                    initCompleteAction( success, error );
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

        public AdEventsProxy( AdType adType ) : base( CASJavaProxy.AdCallbackClassName )
        {
            this.adType = adType;
        }

        public void onLoaded()
        {
            if (CASFactory.isDebug)
                Debug.Log( "[CleverAdsSolutions] onLoaded " + adType.ToString() );
            CASFactory.ExecuteEvent( OnAdLoaded );
        }

        public void onFailed( int error )
        {
            if (CASFactory.isDebug)
                Debug.Log( "[CleverAdsSolutions] onFailed " + adType.ToString() + " error: " + Enum.GetName( typeof( AdError ), error ) );
            if (OnAdFailed != null)
            {
                CASFactory.ExecuteEvent( () =>
                {
                    if (OnAdFailed != null) OnAdFailed( ( AdError )error );
                } );
            }
        }

        public void onOpening( int net, double cpm, int accuracy )
        {
            if (CASFactory.isDebug)
                Debug.Log( "[CleverAdsSolutions] onOpening " + adType.ToString() + " net: " + net + " cpm: " + cpm + " accuracy: " + accuracy );
            CASFactory.ExecuteEvent( OnAdShown );
            if (OnAdOpening != null)
            {
                CASFactory.ExecuteEvent( () =>
                {
                    if (OnAdOpening != null)
                        OnAdOpening( new AdMetaData( adType, ( AdNetwork )net, cpm, ( PriceAccuracy )accuracy ) );
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
    }
}
#endif