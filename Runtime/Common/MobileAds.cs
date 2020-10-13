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
        public const string wrapperVersion = "1.6.1";

        private static IAdsSettings _settings;

        /// <summary>
        /// Get singleton instance for configure all mediation managers.
        /// </summary>
        public static IAdsSettings settings
        {
            get
            {
                if (_settings == null)
                    _settings = CASFactory.CreateSettigns();
                return _settings;
            }
        }

        /// <summary>
        /// Get last initialized <see cref="IMediationManager"/>
        /// May be NULL before the first initialization in the session.
        /// </summary>
        public static IMediationManager manager { get; private set; }

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
            if (string.IsNullOrEmpty( managerID ))
                throw new ArgumentNullException( "managerID", "Manager ID is empty" );

            if (manager != null && manager.managerID == managerID)
            {
                if (initCompleteAction != null)
                    initCompleteAction( true, null );
                return manager;
            }
            EventExecutor.Initialize();
            var instance = CASFactory.CreateManager( managerID, enableAd, testAdMode, initCompleteAction );
            manager = instance;
            return instance;
        }

        /// <summary>
        /// Initialize new <see cref="IMediationManager"/> and save to <see cref="manager"/> field.
        /// Initialize settings will be used from Resources asset.
        /// Use menu Assets/CleverAdsSolutions/Settings for change settings.
        /// Initialize can be called for different identifiers to create different managers.
        /// After initialization, advertising content of <paramref name="enableAd"/> is loading automatically.
        /// </summary>
        /// <param name="managerIndex">CAS manager (Placement) index in Initialize settings array.</param>
        /// <param name="initCompleteAction">Initialization complete action</param>
        /// <exception cref="ArgumentNullException">Manager ID is empty</exception>
        /// <exception cref="NotSupportedException">Not supported runtime platform</exception>
        /// <exception cref="FileNotFoundException">No settings found in resources</exception>
        public static IMediationManager InitializeFromResources(
            int managerIndex = 0, InitCompleteAction initCompleteAction = null )
        {
            return InitializeFromResources( managerIndex, AdFlags.Everything, initCompleteAction );
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
            int managerIndex, AdFlags enableAd, InitCompleteAction initCompleteAction = null )
        {
            var initSettings = CASFactory.LoadInitSettingsFromResources();

            if (!initSettings)
                throw new FileNotFoundException( "No settings found in resources. " +
                    "Please use Assets/CleverAdsSolutions/Settings menu for create settings asset." );
            string managerID;
            if (Application.isEditor || initSettings.testAdMode)
            {
                managerID = "demo";
            }
            else
            {
                if (initSettings.managerIds.Length - 1 < managerIndex || string.IsNullOrEmpty( initSettings.managerIds[managerIndex] ))
                    throw new ArgumentNullException( "managerIds", "Manager ID is empty. " +
                        "Please use Assets/CleverAdsSolutions/Settings menu and set manager ID." );
                managerID = initSettings.managerIds[managerIndex];
            }
            var manager = Initialize( managerID,
                                      initSettings.allowedAdFlags & enableAd,
                                      initSettings.testAdMode,
                                      initCompleteAction );

            manager.bannerSize = initSettings.bannerSize;
            return manager;
        }
    }
}
