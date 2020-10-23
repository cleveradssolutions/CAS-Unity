#if UNITY_ANDROID || CASDeveloper
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
        internal const string AdCallbackClassName = "com.cleversolutions.ads.AdCallback";
        internal const string AdLoadCallbackClassName = "com.cleversolutions.ads.AdLoadCallback";
        internal const string OnInitializationListenerClassName = "com.cleversolutions.ads.OnInitializationListener";

        internal const string UnityActivityClassName = "com.unity3d.player.UnityPlayer";
        #endregion

        internal static AndroidJavaObject GetJavaListObject( List<String> csTypeList )
        {
            AndroidJavaObject javaTypeArrayList = new AndroidJavaObject( "java.util.ArrayList" );
            foreach (string itemList in csTypeList)
            {
                javaTypeArrayList.Call<bool>( "add", itemList );
            }
            return javaTypeArrayList;
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

        public void onInitialization( bool success, string error )
        {
            manager.initializationListener = null;
            CASFactory.ExecuteEvent( initCompleteAction, success, error );
            initCompleteAction = null;
        }
    }

    internal class AdCallbackProxy : AndroidJavaProxy
    {
        public event Action OnAdShown;
        public event CASEventWithError OnAdFailedToShow;
        public event Action OnAdClicked;
        public event Action OnAdCompleted;
        public event Action OnAdClosed;

        public AdCallbackProxy() : base( CASJavaProxy.AdCallbackClassName )
        {
        }

        public void onShown( AndroidJavaObject ad )
        {
            CASFactory.ExecuteEvent( OnAdShown );
        }

        public void onShowFailed( string message )
        {
            CASFactory.ExecuteEvent( OnAdFailedToShow, message );
        }

        public void onClicked()
        {
            CASFactory.ExecuteEvent( OnAdClicked );
        }

        public void onComplete()
        {
            CASFactory.ExecuteEvent( OnAdCompleted );
        }

        public void onClosed()
        {
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
            CASFactory.ExecuteEvent( OnLoadedAd, typeId );
        }

        public void onAdFailedToLoad( AndroidJavaObject type, string error )
        {
            if (OnFailedToLoadAd == null)
                return;
            int typeId = type.Call<int>( "ordinal" );
            CASFactory.ExecuteEvent( OnFailedToLoadAd, typeId, error );
        }
    }
}
#endif