using System;
using System.Collections.Generic;
using UnityEngine;

namespace CAS
{
    [Serializable]
    public class CASInitSettings : ScriptableObject
    {
        public bool testAdMode = false;
        public string[] managerIds;
        public AdFlags allowedAdFlags = AdFlags.Everything;
        public Audience audienceTagged = Audience.Children;
        public AdSize bannerSize = AdSize.Banner;
        public int bannerRefresh = 30;
        public int interstitialInterval = 30;
        public LoadingManagerMode loadingMode = LoadingManagerMode.Optimal;
        public bool debugMode;
        public string trackingUsageDescription;
        public bool trackLocationEnabled;

        internal List<string> extrasKeys;
        internal List<string> extrasValues;
        internal string targetId;
        internal InitCompleteAction initListener;

        /// <summary>
        /// An manager ID is a unique ID number assigned to each of your ad placements when they're created in CAS.
        /// The manager ID is added to your app's code and used to identify ad requests.
        /// If you haven't created an CAS account and registered an app yet, now's a great time to do so at <see cref="https://cleveradssolutions.com"/>.
        /// In a real app, it is important that you use your actual CAS manager ID.
        /// </summary>
        /// <exception cref="ArgumentNullException">Manager ID is empty</exception>
        public CASInitSettings WithManagerId( string managerId )
        {
            if (string.IsNullOrEmpty( managerId ))
                throw new ArgumentNullException( "managerID", "Manager ID is empty" );
            targetId = managerId;
            return this;
        }

        /// <summary>
        /// An manager ID is a unique ID number assigned to each of your ad placements when they're created in CAS.
        /// The manager ID is added to your app's code and used to identify ad requests.
        /// If you haven't created an CAS account and registered an app yet, now's a great time to do so at <see cref="https://cleveradssolutions.com"/>.
        /// In a real app, it is important that you use your actual CAS manager ID.
        ///
        /// Use Manager Id at Index from list `Assets/CleverAdsSolutions/Settings` menu
        /// </summary>
        /// <exception cref="ArgumentNullException">Manager ID is empty</exception>
        public CASInitSettings WithManagerIdAtIndex( int index )
        {
            if (index < 0 || managerIds == null || managerIds.Length - 1 < index
                || string.IsNullOrEmpty( managerIds[index] ))
                throw new ArgumentNullException( "index", "Manager ID is empty. " +
                    "Please use Assets/CleverAdsSolutions/Settings menu and set manager ID." );
            targetId = managerIds[index];
            return this;
        }

        /// <summary>
        /// Initialization complete result callback
        /// </summary>
        public CASInitSettings WithInitListener( InitCompleteAction listener )
        {
            initListener = listener;
            return this;
        }

        /// <summary>
        /// An demoAdMode is optional to enable Test ad mode that will always request test ads.
        /// If you're just looking to experiment with the SDK in a Hello World app, though, you can use the <see cref="true"/> with any manager ID string.
        /// <b>Please remember to set False demo ad mode after tests done.</b>
        /// </summary>
        public CASInitSettings WithTestAdMode( bool test )
        {
            testAdMode = test;
            return this;
        }

        /// <summary>
        /// An Enabled Ad types is option to increase application performance by initializing only those ad types that will be used.
        /// Changes in current session only.
        /// Ad types can be enabled manually after initialize by <see cref="IMediationManager.SetEnableAd(AdType, bool)"/>
        ///
        /// For example: .withEnabledAdTypes(AdFlags.Banner | AdFlags.Interstitial)
        /// </summary>
        public CASInitSettings WithEnabledAdTypes( AdFlags adTypes )
        {
            allowedAdFlags = adTypes;
            return this;
        }

        /// <summary>
        /// Additional mediation settings.
        /// Use constant key from <see cref="MediationExtras"/>
        /// </summary>
        public CASInitSettings WithMediationExtras( string key, string value )
        {
            if (extrasKeys == null || extrasValues == null)
            {
                extrasKeys = new List<string>();
                extrasValues = new List<string>();
            }
            extrasKeys.Add( key );
            extrasValues.Add( value );
            return this;
        }

        /// <summary>
        /// Clear additional mediation settings.
        /// </summary>
        public CASInitSettings ClearMediationExtras()
        {
            if (extrasKeys != null)
                extrasKeys.Clear();
            if (extrasValues != null)
                extrasValues.Clear();
            return this;
        }

        /// <summary>
        /// Initialize new <see cref="IMediationManager"/> and save to <see cref="MobileAds.manager"/> field.
        /// Can be called for different identifiers to create different managers.
        /// </summary>
        /// <exception cref="NotSupportedException">Not supported platform. Allowed Android, iOS and Editor only</exception>
        /// <exception cref="ArgumentNullException">Manager ID is empty</exception>
        public IMediationManager Initialize()
        {
            if (string.IsNullOrEmpty( targetId ))
            {
                if (Application.isEditor || testAdMode)
                    targetId = "demo";
                else
                    WithManagerIdAtIndex( 0 );
            }
            var manager = CASFactory.CreateManager( this );
            manager.bannerSize = bannerSize;
            return manager;
        }
    }
}