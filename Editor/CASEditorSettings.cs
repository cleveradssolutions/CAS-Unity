//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2023 CleverAdsSolutions. All rights reserved.
//

using System;
using UnityEngine;

namespace CAS.UEditor
{
    [Serializable]
    public class CASEditorSettings : ScriptableObject
    {
        public bool autoCheckForUpdatesEnabled = true;
        public bool delayAppMeasurementGADInit = true;
        public bool buildPreprocessEnabled = true;

        /// <summary>
        /// ISO2 such as US, RU ...
        /// </summary>
        public string mostPopularCountryOfUsers = "BR";

        public Permission permissionAdId = Permission.Auto;

        public bool generateIOSDeepLinksForPromo = true;
        public string attributionReportEndpoint = null;

        /// <summary>
        /// {key}.lproj Language code (ISO-639)
        /// NSUserTrackingUsageDescription = {value}
        /// 
        /// https://www.ibabbleon.com/iOS-Language-Codes-ISO-639.html
        /// </summary>
        public KeyValuePair[] userTrackingUsageDescription = new KeyValuePair[0];


#if UNITY_2022_2_OR_NEWER
        // Unity 2022.2 migrate to valid gradle 7.1.2
        public bool updateGradlePluginVersion { get { return false; } }
#else
        public bool updateGradlePluginVersion = true;
#endif

#if MULTIDEX_ENABLED || !UNITY_2020_1_OR_NEWER
        public bool multiDexEnabled = true;
#else
        // MultiDEX enable by default for API 21+
        public bool multiDexEnabled { get { return false; } }
#endif

        [Obsolete("No longer required to work Cross Promo")]
        public bool generateAndroidQuerriesForPromo { get { return false; } }
        [Obsolete("Exo player used in any case")]
        public bool exoPlayerIncluded { get { return true; } }
        [Obsolete("Starting with Xcode 14, bitcode is no longer required")]
        public bool bitcodeIOSDisabled { get { return true; } }

        public static CASEditorSettings Load(bool createAsset = false)
        {
            var asset = CASEditorUtils.GetSettingsAsset("CASEditorSettings",
                CASEditorUtils.editorFolderPath, typeof(CASEditorSettings), createAsset, null);
            if (asset)
                return (CASEditorSettings)asset;
            return CreateInstance<CASEditorSettings>();
        }

        public bool isUseAdvertiserIdLimited(Audience audience)
        {
            var permission = permissionAdId;
            if (permission == Permission.Auto)
                return audience == Audience.Children;
            return permission == Permission.Removed;
        }

        public enum Permission
        {
            Auto = 0,
            Required = 1,
            Removed = 2
        }
    }
}
