using System;
using UnityEngine;
using UnityEditor;

namespace CAS.UEditor
{
    [Serializable]
    internal class CASEditorSettings : ScriptableObject
    {
        public bool autoCheckForUpdatesEnabled = true;
        public bool delayAppMeasurementGADInit = true;
        public bool multiDexEnabled = true;

        /// <summary>
        /// ISO2 such as US, RU ...
        /// </summary>
        public string mostPopularCountryOfUsers = "BR";
        public string attributionReportEndpoint = CASEditorUtils.attributionReportEndPoint;

        /// <summary>
        /// {key}.lproj Language code (ISO-639)
        /// NSUserTrackingUsageDescription = {value}
        /// 
        /// https://www.ibabbleon.com/iOS-Language-Codes-ISO-639.html
        /// </summary>
        public KeyValuePair[] userTrackingUsageDescription = new KeyValuePair[0];


        public static CASEditorSettings Load( bool createAsset = false )
        {
            var asset = CASEditorUtils.GetSettingsAsset( "CASEditorSettings",
                CASEditorUtils.editorFolderPath, typeof( CASEditorSettings ), createAsset, null );
            if (asset)
                return ( CASEditorSettings )asset;
            return CreateInstance<CASEditorSettings>();
        }
    }
}
