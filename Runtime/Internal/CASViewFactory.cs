//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2022 CleverAdsSolutions. All rights reserved.
//

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CAS
{
    internal abstract class CASViewFactory : ISingleBannerManager
    {
        protected List<IAdView> adViews = new List<IAdView>();
        public IAdView globalView { get; private set; }
        private bool isActiveGlobalView = false;

        public event Action OnBannerAdShown;
        public event CASEventWithMeta OnBannerAdOpening;
        public event CASEventWithError OnBannerAdFailedToShow;
        public event Action OnBannerAdClicked;
        public event Action OnBannerAdHidden;

        private bool isGlobalViewAllowed
        {
            get { return globalView != null; }
        }

        public AdSize bannerSize
        {
            get
            {
                if (globalView == null)
                    return 0;
                return globalView.size;
            }
            set
            {
                if (value != 0)
                    OnGlobalViewChanged( GetAdView( value ), globalView );
            }
        }

        public AdPosition bannerPosition
        {
            get
            {
                if (globalView == null)
                    return AdPosition.Undefined;
                return globalView.position;
            }
            set
            {
                if (globalView == null)
                {
                    Debug.LogError( "Single banner ad size not defined. " +
                        "Please set IMediationManager.bannerSize before change bannerPosition." );
                    return;
                }
                globalView.position = value;
            }
        }

        public IAdView GetAdView( AdSize size )
        {
            if (size < AdSize.Banner)
                throw new ArgumentException( "Invalid AdSize " + size.ToString() );
            for (int i = 0; i < adViews.Count; i++)
            {
                if (adViews[i].size == size)
                    return adViews[i];
            }
            var view = CreateAdView( size );
            adViews.Add( view );
            return view;
        }

        protected abstract IAdView CreateAdView( AdSize size );

        protected virtual void OnGlobalViewChanged( IAdView newView, IAdView lastView )
        {
            if (lastView == newView)
                return;

            if (lastView != null)
            {
                lastView.OnClicked -= CallbackAdViewClicked;
                lastView.OnPresented -= CallbackAdViewPresented;
                lastView.OnHidden -= CallbackAdViewHidden;
                lastView.OnLoaded -= CallbackAdViewLoaded;
                lastView.OnFailed -= CallbackAdViewFailed;

                lastView.SetActive( false );
                newView.position = lastView.position;
            }

            newView.OnClicked += CallbackAdViewClicked;
            newView.OnPresented += CallbackAdViewPresented;
            newView.OnHidden += CallbackAdViewHidden;
            newView.OnLoaded += CallbackAdViewLoaded;
            newView.OnFailed += CallbackAdViewFailed;

            newView.SetActive( isActiveGlobalView );

            globalView = newView;
        }

        public float GetBannerHeightInPixels()
        {
            if (globalView == null)
                return 0.0f;
            return globalView.rectInPixels.height;
        }

        public float GetBannerWidthInPixels()
        {
            if (globalView == null)
                return 0.0f;
            return globalView.rectInPixels.width;
        }

        internal void LoadGlobalBanner()
        {
            if (globalView == null)
            {
                Debug.LogError( "Single banner ad size not defined. " +
                    "Please set IMediationManager.bannerSize before call LoadAd." );
                return;
            }
            globalView.Load();
        }

        internal void ShowGlobalBanner()
        {
            isActiveGlobalView = true;
            if (globalView != null)
                globalView.SetActive( true );
        }

        public void HideBanner()
        {
            isActiveGlobalView = false;
            if (globalView != null)
                globalView.SetActive( false );
        }

        public void RemoveAdViewFromFactory( IAdView view )
        {
            if (globalView == adViews)
                globalView = null;
            adViews.Remove( view );
        }

        public abstract void OnLoadedCallback( AdType type );

        public abstract void OnFailedCallback( AdType type, AdError error );

        private void CallbackAdViewLoaded( IAdView view )
        {
            if (view == globalView)
                OnLoadedCallback( AdType.Banner );
        }

        private void CallbackAdViewFailed( IAdView view, AdError error )
        {
            if (view == globalView)
            {
                if (OnBannerAdFailedToShow != null)
                    OnBannerAdFailedToShow( error.GetMessage() );
                OnFailedCallback( AdType.Banner, error );
            }
        }

        private void CallbackAdViewPresented( IAdView view, AdMetaData data )
        {
            if (view != globalView)
                return;
            if (OnBannerAdShown != null)
                OnBannerAdShown();
            if (OnBannerAdOpening != null)
                OnBannerAdOpening( data );
        }

        private void CallbackAdViewClicked( IAdView view )
        {
            if (view == globalView && OnBannerAdClicked != null)
                OnBannerAdClicked();
        }

        private void CallbackAdViewHidden( IAdView view )
        {
            if (view == globalView && OnBannerAdHidden != null)
                OnBannerAdHidden();
        }

    }
}
