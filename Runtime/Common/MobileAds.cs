using System;
using System.IO;
using UnityEngine;

namespace CAS
{
    public delegate void InitCompleteAction( bool success, string error );

    public static class MobileAds
    {
        /// <summary>
        /// CAS Unity wrapper version
        /// </summary>
        public const string wrapperVersion = "1.8.2";

        /// <summary>
        /// Get singleton instance for configure all mediation managers.
        /// </summary>
        public static IAdsSettings settings
        {
            get { return CASFactory.GetAdsSettings(); }
        }

        /// <summary>
        /// Get last initialized <see cref="IMediationManager"/>
        /// May be NULL before the first initialization in the session.
        /// </summary>
        public static IMediationManager manager { get; private set; }

        /// <summary>
        /// You can now easily tailor the way you serve your ads to fit a specific audience!
        /// You’ll need to inform our servers of the users’ details
        /// so the SDK will know to serve ads according to the segment the user belongs to.
        /// **Attention:** Must be set before initializing the SDK.
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
        public static IMediationManager Initialize(
            string managerID,
            AdFlags enableAd = AdFlags.Everything,
            bool testAdMode = false,
            InitCompleteAction initCompleteAction = null )
        {
            return manager = CASFactory.Initialize( managerID, enableAd, testAdMode, initCompleteAction );
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
        public static IMediationManager InitializeFromResources(
            int managerIndex = 0,
            AdFlags enableAd = AdFlags.Everything,
            InitCompleteAction initCompleteAction = null )
        {
            return manager = CASFactory.InitializeFromResources( managerIndex, enableAd, initCompleteAction );
        }

        /// <summary>
        /// The CAS SDK provides an easy way to verify that you’ve successfully integrated any additional adapters;
        /// it also makes sure all required dependencies and frameworks were added for the various mediated ad networks.
        /// After you have finished your integration, call the following static method
        /// and confirm that all networks you have implemented are marked as VERIFIED.
        ///
        /// Find log information by tag: CASIntegrationHelper
        ///
        /// Once you’ve successfully verified your integration,
        /// please remember to remove the integration helper from your code.
        /// </summary>
        public static void ValidateIntegration()
        {
            CASFactory.ValidateIntegration();
        }
    }
}
