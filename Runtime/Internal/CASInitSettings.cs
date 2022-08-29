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
    /// <summary>
    /// Please use <see cref="IManagerBuilder"/> instead.
    /// CASInitSettings may not be available in future.
    /// </summary>
    [Serializable]
    public class CASInitSettings : ScriptableObject, IManagerBuilder
    {
        #region Fields
#pragma warning disable 649 // is never assigned to, and will always have its default value null
        [Obsolete( "Please use IsTestAdMode() instead to get real runtime state." )]
        public bool testAdMode = false;
        [SerializeField]
        private string[] managerIds = { "demo" };
        public AdFlags allowedAdFlags = AdFlags.None;
        [SerializeField]
        private Audience audienceTagged = Audience.Children;
        [Obsolete( "Used only from obsolete Banner Size API. Better use new IAdView interface instead." )]
        public AdSize bannerSize = 0;

        [SerializeField]
        private int bannerRefresh = 30;
        [SerializeField]
        private int interstitialInterval = 30;
        [SerializeField]
        private LoadingManagerMode loadingMode = LoadingManagerMode.Optimal;
        [SerializeField]
        private bool debugMode;
        [SerializeField]
        private bool trackLocationEnabled;
        [SerializeField]
        private bool analyticsCollectionEnabled;
        [SerializeField]
        private bool interWhenNoRewardedAd;

        internal string targetId;
        internal string userID;
        internal Dictionary<string, string> extras;
        internal InitCompleteAction initListener;
#pragma warning restore 649
        #endregion

        /// <summary>
        /// Initialize new <see cref="IMediationManager"/> and save to <see cref="MobileAds.manager"/> field.
        /// Can be called for different identifiers to create different managers.
        /// </summary>
        /// <exception cref="NotSupportedException">Not supported platform. Allowed Android, iOS and Editor only</exception>
        /// <exception cref="ArgumentNullException">Manager ID are not found</exception>
        public IMediationManager Initialize()
        {
            if (string.IsNullOrEmpty( targetId ))
                WithManagerIdAtIndex( 0 );
            return CASFactory.CreateManager( this );
        }

        #region Initialization options
        /// <summary>
        /// An manager ID is a unique ID number assigned to each of your ad placements when they're created in CAS.
        /// <para>Using Manager Id at Index from list <b>`Assets/CleverAdsSolutions/Settings`</b> menu.</para>
        /// <para>Index 0 by default when the method is not called.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException">Manager ID is empty</exception>
        public IManagerBuilder WithManagerIdAtIndex( int index )
        {
            if (index < 0 || managerIds == null || managerIds.Length - 1 < index
                || string.IsNullOrEmpty( managerIds[index] ))
                throw new ArgumentNullException( "index", "Manager ID is empty. " +
                    "Please use Assets/CleverAdsSolutions/Settings menu and set manager ID." );
            targetId = managerIds[index];
            return this;
        }

        /// <summary>
        /// Set listener to initialization complete result callback.
        /// </summary>
        public IManagerBuilder WithInitListener( InitCompleteAction listener )
        {
            initListener = listener;
            return this;
        }

        /// <summary>
        /// An Enabled Ad types is option to increase application performance by initializing only those ad types that will be used.
        /// <para>For example: .withEnabledAdTypes(AdFlags.Banner, AdFlags.Interstitial)</para>
        /// <para>Changes in current session only.</para>
        /// <para>Ad types can be enabled manually after initialize by <see cref="IMediationManager.SetEnableAd(AdType, bool)"/></para>
        /// </summary>
        public IManagerBuilder WithEnabledAdTypes( params AdFlags[] adTypes )
        {
            allowedAdFlags = AdFlags.None;
            for (int i = 0; i < adTypes.Length; i++)
                allowedAdFlags |= adTypes[i];
            return this;
        }

        public IManagerBuilder WithUserID(string userID )
        {
            this.userID = userID;
            return this;
        }

        /// <summary>
        /// Additional mediation settings.
        /// Use constant key from <see cref="MediationExtras"/> with values of "1" or "0".
        /// </summary>
        public IManagerBuilder WithMediationExtras( string key, string value )
        {
            if (extras == null)
                extras = new Dictionary<string, string>();
            extras[key] = value;
            return this;
        }

        /// <summary>
        /// Clear additional mediation settings.
        /// </summary>
        public IManagerBuilder ClearMediationExtras()
        {
            if (extras != null)
                extras.Clear();
            return this;
        }

        /// <summary>
        /// An manager ID is a unique ID number assigned to each of your ad placements when they're created in CAS.
        /// <para>Typically, Manager ID for Adnroid equals the App Bundle ID, and for iOS the iTunes ID.</para>
        /// <para>The manager ID is added to your app's code and used to identify ad requests.</para>
        /// <para><b>Attention</b> The identifier is different for each platforms.
        /// You need to define different identifiers depending on the current platform.</para>
        /// <para>You can use a generic way to get the ID by ordinal index <see cref="WithManagerIdAtIndex(int)"/> for current platform.</para>
        /// <para>Please set all used Manager IDs in `Assets > CleverAdsSolutions > Settings` menu to setup the project correctly.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException">Manager ID is empty</exception>
        public IManagerBuilder WithManagerId( string managerId )
        {
            if (string.IsNullOrEmpty( managerId ))
                throw new ArgumentNullException( "managerId", "Manager ID is empty" );
            targetId = managerId;
            return this;
        }

        /// <summary>
        /// Option to enable Test ad mode that will always request test ads.
        /// <para>If you're just looking to experiment with the SDK in a Hello World app, though, you can use the true with any manager ID string.</para>
        /// <para><b>Don't forget to set False test ad mode to release application.</b></para>
        /// </summary>
        [Obsolete( "Please set Test Ad Mode in `Assets>CleverAdsSolutions>Settings` menu to get true Test Ad, " +
            "also Development build work same." )]
        public IManagerBuilder WithTestAdMode( bool test )
        {
            testAdMode = test;
            return this;
        }
        #endregion

        #region Settings getters
        public AdFlags defaultAllowedFormats { get { return allowedAdFlags; } }
        public Audience defaultAudienceTagged { get { return audienceTagged; } }
        public int defaultBannerRefresh { get { return bannerRefresh; } }
        public int defaultInterstitialInterval { get { return interstitialInterval; } }
        public LoadingManagerMode defaultLoadingMode { get { return loadingMode; } }
        public bool defaultDebugModeEnabled { get { return debugMode; } }
        public bool defaultIOSTrackLocationEnabled { get { return trackLocationEnabled; } }
        public bool defaultAnalyticsCollectionEnabled { get { return analyticsCollectionEnabled; } }
        public bool defaultInterstitialWhenNoRewardedAd { get { return interWhenNoRewardedAd; } }

        public int managersCount { get { return managerIds == null ? 0 : managerIds.Length; } }
        public string GetManagerId( int index )
        {
            return managerIds[index];
        }
        public int IndexOfManagerId( string id )
        {
            if (managerIds == null || id == null)
                return -1;
            return Array.IndexOf( managerIds, id );
        }
        public bool IsTestAdMode()
        {
#pragma warning disable CS0618 // Type or member is obsolete
#if UNITY_EDITOR
            return testAdMode || UnityEditor.EditorUserBuildSettings.development;
#else
            return testAdMode || Debug.isDebugBuild;
#endif
#pragma warning restore CS0618 // Type or member is obsolete
        }
        #endregion
    }
}