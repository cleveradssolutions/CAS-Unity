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
        internal const string AdCallbackClassName = "com.cleversolutions.ads.unity.CASCallback";
        internal const string AdLoadCallbackClassName = "com.cleversolutions.ads.AdLoadCallback";
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
            if (MobileAds.settings.isDebugMode)
                Debug.Log( "[CleverAdsSolutions] onInitialization " + success );
            manager.initializationListener = null;
            CASFactory.ExecuteEvent( initCompleteAction, success, "" );
            initCompleteAction = null;
        }

        public void onInitialization( bool success, string error )
        {
            if (MobileAds.settings.isDebugMode)
                Debug.Log( "[CleverAdsSolutions] onInitialization " + success );
            manager.initializationListener = null;
            CASFactory.ExecuteEvent( initCompleteAction, success, error );
            initCompleteAction = null;
        }
    }

    internal class AdCallbackProxy : AndroidJavaProxy
    {
        private readonly AdType adType;
        public event Action OnAdShown;
        public event CASEventWithMeta OnAdOpening;
        public event CASEventWithError OnAdFailedToShow;
        public event Action OnAdClicked;
        public event Action OnAdCompleted;
        public event Action OnAdClosed;

        public AdCallbackProxy( AdType adType ) : base( CASJavaProxy.AdCallbackClassName )
        {
            this.adType = adType;
        }

        public void onOpening( int net, double cpm, int accuracy )
        {
            if (MobileAds.settings.isDebugMode)
                Debug.Log( "[CleverAdsSolutions] onOpening " + adType.ToString() + " net: " + net + " cpm: " + cpm + " accuracy: " + accuracy );
            CASFactory.ExecuteEvent( OnAdShown );
            if (OnAdOpening != null)
            {
                CASFactory.ExecuteEvent( OnAdOpening,
                    new AdMetaData( adType, ( AdNetwork )net, cpm, ( PriceAccuracy )accuracy ) );
            }
        }

        public void onShowFailed( string message )
        {
            if (MobileAds.settings.isDebugMode)
                Debug.Log( "[CleverAdsSolutions] onShowFailed " + adType.ToString() + " with error: " + message );
            CASFactory.ExecuteEvent( OnAdFailedToShow, message );
        }

        public void onClicked()
        {
            if (MobileAds.settings.isDebugMode)
                Debug.Log( "[CleverAdsSolutions] onClicked " + adType.ToString() );
            CASFactory.ExecuteEvent( OnAdClicked );
        }

        public void onComplete()
        {
            if (MobileAds.settings.isDebugMode)
                Debug.Log( "[CleverAdsSolutions] onComplete " + adType.ToString() );
            CASFactory.ExecuteEvent( OnAdCompleted );
        }

        public void onClosed()
        {
            if (MobileAds.settings.isDebugMode)
                Debug.Log( "[CleverAdsSolutions] onClosed " + adType.ToString() );
            CASFactory.ExecuteEvent( OnAdClosed );
        }
    }

    internal class AdLoadCallbackProxy : AndroidJavaProxy
    {
        public event CASTypedEvent OnLoadedAd;
        public event CASTypedEventWithError OnFailedToLoadAd;

        public AdLoadCallbackProxy() : base( CASJavaProxy.AdLoadCallbackClassName ) { }

        public void onAdLoaded( AndroidJavaObject type )
        {
            if (OnLoadedAd == null)
                return;
            int typeId = type.Call<int>( "ordinal" );
            if (MobileAds.settings.isDebugMode)
                Debug.Log( "[CleverAdsSolutions] onAdLoaded " + typeId );
            CASFactory.ExecuteEvent( OnLoadedAd, typeId );
        }

        public void onAdFailedToLoad( AndroidJavaObject type, string error )
        {
            if (OnFailedToLoadAd == null)
                return;
            int typeId = type.Call<int>( "ordinal" );
            if (MobileAds.settings.isDebugMode)
                Debug.Log( "[CleverAdsSolutions] onAdFailedToLoad " + typeId );
            CASFactory.ExecuteEvent( OnFailedToLoadAd, typeId, error );
        }
    }
}
#endif