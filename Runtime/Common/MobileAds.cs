//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2022 CleverAdsSolutions. All rights reserved.
//

using System;
using System.IO;
using UnityEngine;

namespace CAS
{
    [WikiPage( "https://github.com/cleveradssolutions/CAS-Unity/wiki/Initialize-SDK" )]
    public static class MobileAds
    {
        /// <summary>
        /// CAS Unity wrapper version
        /// </summary>
        public const string wrapperVersion = "2.9.9";

        /// <summary>
        /// Get singleton instance for configure all mediation managers.
        /// </summary>
        public static IAdsSettings settings
        {
            get { return CASFactory.GetAdsSettings(); }
        }

        /// <summary>
        /// Get first initialized <see cref="IMediationManager"/>
        /// <para>May be NULL before the first initialization in the session.</para>
        /// <para>We recommend using the <see cref="BuildManager()"/>.WithManagerIdAtIndex().Initialize() method to get a specific manager.</para>
        /// </summary>
        public static IMediationManager manager
        {
            get { return CASFactory.GetMainManagerOrNull(); }
        }

        /// <summary>
        /// You can now easily tailor the way you serve your ads to fit a specific audience!
        /// <para>You will need to inform our servers of the users’ details
        /// so the SDK will know to serve ads according to the segment the user belongs to.</para>
        /// <para>**Attention:** Must be set before initializing the SDK.</para>
        /// </summary>
        public static ITargetingOptions targetingOptions
        {
            get { return CASFactory.GetTargetingOptions(); }
        }

        /// <summary>
        /// Return Native SDK version else <see cref="wrapperVersion"/> for Unity Editor.
        /// </summary>
        public static string GetSDKVersion()
        {
            return CASFactory.GetSDKVersion();
        }

        /// <summary>
        /// Create <see cref="IMediationManager"/> builder.
        /// <para>Don't forget to call the <see cref="IManagerBuilder.Initialize"/> method to create manager instance.</para>
        /// </summary>
        public static CASInitSettings BuildManager()
        {
            return CASFactory.LoadDefaultBuiderFromResources();
        }

        /// <summary>
        /// The CAS SDK provides an easy way to verify that you’ve successfully integrated any additional adapters;
        /// it also makes sure all required dependencies and frameworks were added for the various mediated ad networks.
        /// <para>After you have finished your integration, call the following static method
        /// and confirm that all networks you have implemented are marked as <b>VERIFIED</b>.</para>
        ///
        /// <para>Find log information by tag: <b>CASIntegrationHelper</b></para>
        ///
        /// Once you’ve successfully verified your integration,
        /// please remember to remove the integration helper from your code.
        /// </summary>
        public static void ValidateIntegration()
        {
            CASFactory.ValidateIntegration();
        }

        /// <summary>
        /// Get array of active mediation networks in build.
        /// </summary>
        public static AdNetwork[] GetActiveNetworks()
        {
            return CASFactory.GetActiveNetworks();
        }

        public static bool IsActiveNetwork( AdNetwork network )
        {
            return CASFactory.IsActiveNetwork( network );
        }

        #region Obsolete API
        /// <summary>
        /// Initialize new <see cref="IMediationManager"/> and save to <see cref="manager"/> field.
        /// Can be called for different identifiers to create different managers.
        /// After initialization, advertising content of <paramref name="enableAd"/> is loading automatically.
        /// </summary>
        /// <param name="managerID">CAS manager (Placement) identifier</param>
        /// <param name="enableAd">Enabled Ad types processing.
        /// Ad types can be enabled manually after initialize by IMediationManager.SetEnableAd method.</param>
        /// <param name="testAdMode">Enable demo mode that will always request test ads</param>
        /// <param name="initCompleteAction">Initialization complete action</param>
        /// <exception cref="ArgumentNullException">Manager ID is empty.</exception>
        /// <exception cref="NotSupportedException">Not supported runtime platform</exception>
        [Obsolete( "We recommend using the builder for initialization new manager." )]
        public static IMediationManager Initialize(
            string managerID,
            AdFlags enableAd = AdFlags.Everything,
            bool testAdMode = false,
            InitCompleteAction initCompleteAction = null )
        {
            return BuildManager()
                .WithManagerId( managerID )
                .WithEnabledAdTypes( enableAd )
                .WithTestAdMode( testAdMode )
                .WithInitListener( initCompleteAction )
                .Initialize();
        }

        /// <summary>
        /// Initialize new <see cref="IMediationManager"/> and save to <see cref="manager"/> field.
        /// Initialize settings will be used from Resources asset.
        /// Use menu Assets/CleverAdsSolutions/Settings for change settings.
        /// Initialize can be called for different identifiers to create different managers.
        /// After initialization, advertising content of <paramref name="enableAd"/> is loading automatically.
        /// </summary>
        /// <param name="managerIndex">CAS manager (Placement) index in Initialize settings array.</param>
        /// <param name="enableAd">Enabled Ad types processing.
        /// The selected flags will be limited to the settings of the allowedAdFlags from the resources.
        /// Ad types can be enabled manually after initialize by IMediationManager.SetEnableAd method.</param>
        /// <param name="initCompleteAction">Initialization complete action</param>
        /// <exception cref="ArgumentNullException">Manager ID is empty</exception>
        /// <exception cref="NotSupportedException">Not supported runtime platform</exception>
        /// <exception cref="FileNotFoundException">No settings found in resources</exception>
        [Obsolete( "Use the new extended parameters of BuildManager() instead" )]
        public static IMediationManager InitializeFromResources(
            int managerIndex = 0,
            AdFlags enableAd = AdFlags.Everything,
            InitCompleteAction initCompleteAction = null )
        {
            var builder = BuildManager();
            builder.WithInitListener( initCompleteAction );
            builder.WithEnabledAdTypes( enableAd & builder.defaultAllowedFormats );
            if (managerIndex > 0)
                builder.WithManagerIdAtIndex( managerIndex );
            return builder.Initialize();
        }

        /// <summary>
        /// Mediation pattern string with format '1' - active and '0' - not active.
        /// Char index of string pattern equals enum index of <see cref="AdNetwork"/>
        /// </summary>
        [Obsolete( "Use GetActiveNetworks() instead." )]
        public static string GetActiveMediationPattern()
        {
            return CASFactory.GetActiveMediationPattern();
        }
        #endregion
    }
}
