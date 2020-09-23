using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Networking;

namespace CAS.UEditor
{
    internal static class CASEditorUtils
    {
        #region Constants
        public const string logTag = "[CleverAdsSolutions] ";
        public const string packageName = "com.cleversolutions.ads.unity";
        public const string androidAdmobSampleAppID = "ca-app-pub-3940256099942544~3347511713";
        public const string iosAdmobSampleAppID = "ca-app-pub-3940256099942544~1458002511";

        public const string editorRuntomeActiveAdPrefs = "typesadsavailable";
        public const string rootCASFolderPath = "Assets/CleverAdsSolutions";
        public const string editorFolderPath = rootCASFolderPath + "/Editor";
        public const string androidLibFolderPath = "Assets/Plugins/Android/CASPlugin.androidlib";
        public const string androidResSettingsPath = androidLibFolderPath + "/res/raw/cas_settings.json";
        public const string androidLibManifestPath = androidLibFolderPath + "/AndroidManifest.xml";
        public const string androidLibPropertiesPath = androidLibFolderPath + "/project.properties";

        public const string iosResSettingsPath = "Temp/UnityTempFile-cassettings";

        public const string generalTemplateDependency = "CASGeneral";
        public const string teenTemplateDependency = "CASTeen";
        public const string promoTemplateDependency = "CASPromo";
        public const string dependenciesExtension = "Dependencies.xml";

        public const string androidLibManifestTemplateFile = "CASManifest.xml";
        public const string androidLibPropTemplateFile = "CASLibProperties.txt";
        public const string iosSKAdNetworksTemplateFile = "CASSKAdNetworks.txt";

        public const string githubURL = "https://github.com/cleveradssolutions/CAS-Unity";
        public const string supportURL = "https://github.com/cleveradssolutions/CAS-Unity#support";
        public const string websiteURL = "https://cleveradssolutions.com";
        public const string configuringPrivacyURL = "https://github.com/cleveradssolutions/CAS-iOS#step-5-configuring-privacy-controls";

        public const string mainGradlePath = "Assets/Plugins/Android/mainTemplate.gradle";

        private const string locationUsageDefaultDescription = "Your data will be used to provide you a better and personalized ad experience.";
        #endregion

        [Serializable]
        internal class AdmobAppIdData
        {
            public string admob_app_id = null;
        }

        #region Menu items
        [MenuItem( "Assets/CleverAdsSolutions/Android Settings..." )]
        public static void OpenAndroidSettingsWindow()
        {
            OpenSettingsWindow( BuildTarget.Android );
        }

        [MenuItem( "Assets/CleverAdsSolutions/iOS Settings..." )]
        public static void OpenIOSSettingsWindow()
        {
            OpenSettingsWindow( BuildTarget.iOS );
        }
        #endregion

        public static bool IsFirebaseServiceExist( string service )
        {
            //analytics
            if (AssetDatabase.FindAssets( "Firebase." + service ).Length > 0)
                return true;

            const string packageManifest = "Packages/manifest.json";
            return File.Exists( packageManifest ) && File.ReadAllText( packageManifest )
                .Contains( "com.google.firebase." + service );
        }

        public static void OpenSettingsWindow( BuildTarget target )
        {
            var asset = GetSettingsAsset( target );
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject( asset );
        }

        public static string GetAdmobAppIdFromJson( string json )
        {
            return JsonUtility.FromJson<AdmobAppIdData>( json ).admob_app_id;
        }

        public static CASInitSettings GetSettingsAsset( BuildTarget platform )
        {
            if (!AssetDatabase.IsValidFolder( "Assets/Resources" ))
                AssetDatabase.CreateFolder( "Assets", "Resources" );
            var assetPath = "Assets/Resources/CASSettings" + platform.ToString() + ".asset";
            var asset = AssetDatabase.LoadAssetAtPath<CASInitSettings>( assetPath );
            if (!asset)
            {
                asset = ScriptableObject.CreateInstance<CASInitSettings>();
                if (platform == BuildTarget.Android)
                {
                    asset.managerIds = new string[] {
                        PlayerSettings.GetApplicationIdentifier( BuildTargetGroup.Android )
                    };
                }
                else if (platform == BuildTarget.iOS)
                {
                    asset.locationUsageDescription = locationUsageDefaultDescription;
                    asset.interstitialInterval = 90;
                }
                AssetDatabase.CreateAsset( asset, assetPath );
            }
            return asset;
        }

        public static void CreateFolderInAssets( string folderName )
        {
            if (!AssetDatabase.IsValidFolder( rootCASFolderPath ))
                AssetDatabase.CreateFolder( "Assets", "CleverAdsSolutions" );
            if (!AssetDatabase.IsValidFolder( rootCASFolderPath + "/" + folderName ))
                AssetDatabase.CreateFolder( rootCASFolderPath, folderName );
        }

        public static bool IsDependencyFileExists( string dependency, BuildTarget platform )
        {
            return AssetDatabase.FindAssets( dependency + platform.ToString() + "Dependencies" ).Length > 0;
        }

        public static string GetTemplatePath( string templateFile )
        {
            string templateFolder = "/Templates/" + templateFile;
            string path = "Packages/" + packageName + templateFolder;
            if (!File.Exists( path ))
            {
                path = rootCASFolderPath + templateFolder;
                if (!File.Exists( path ))
                {
                    Debug.LogError( logTag + "Template " + templateFile + " file not found. Try reimport CAS package." );
                    return null;
                }
            }
            return path;
        }

        public static bool TryCopyFile( string source, string dest )
        {
            try
            {
                AssetDatabase.DeleteAsset( dest );
                File.Copy( source, dest );
                AssetDatabase.ImportAsset( dest );
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException( e );
                return false;
            }
        }

        public static void WriteToFile( string data, string path )
        {
            try
            {
                var directoryPath = Path.GetDirectoryName( path );
                if (!Directory.Exists( directoryPath ))
                    Directory.CreateDirectory( directoryPath );
                File.WriteAllText( path, data );
                AssetDatabase.ImportAsset( path );
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
        }

        public static bool IsPortraitOrientation()
        {
            var orientation = PlayerSettings.defaultInterfaceOrientation;
            if (orientation == UIOrientation.Portrait || orientation == UIOrientation.PortraitUpsideDown)
            {
                return true;
            }
            else if (orientation == UIOrientation.AutoRotation)
            {
                if (PlayerSettings.allowedAutorotateToPortrait
                    && !PlayerSettings.allowedAutorotateToLandscapeRight
                    && !PlayerSettings.allowedAutorotateToLandscapeLeft)
                    return true;
            }
            return false;
        }

        public static void StopBuildWithMessage( string message, BuildTarget target )
        {
            EditorUtility.ClearProgressBar();
            if (target != BuildTarget.NoTarget
                    && EditorUtility.DisplayDialog( "CAS Stop Build", message, "Open Settings", "Close" ))
                OpenSettingsWindow( target );

#if UNITY_2018_1_OR_NEWER
            throw new BuildFailedException( logTag + message );
#elif UNITY_2017_1_OR_NEWER
            throw new BuildPlayerWindow.BuildMethodException( logTag + message );
#else
            throw new OperationCanceledException(logTag + message);
#endif
        }

        public static string BuildRemoteUrl( string managerID, string country, BuildTarget platform )
        {
            string platformCode;
            switch (platform)
            {
                case BuildTarget.Android:
                    platformCode = "0";
                    break;
                case BuildTarget.iOS:
                    platformCode = "1";
                    break;
                default:
                    platformCode = "9";
                    Debug.LogError( "Not supported platform for CAS " + platform.ToString() );
                    break;
            }

            var result = new StringBuilder( "https://psvpromo.psvgamestudio.com/Scr/cas.php?platform=" )
                .Append( platformCode )
                .Append( "&bundle=" ).Append( UnityWebRequest.EscapeURL( managerID ) )
                .Append( "&hash=" ).Append( Md5Sum( managerID + platformCode ) )
                .Append( "&lang=" ).Append( SystemLanguage.English );

            if (!string.IsNullOrEmpty( country ))
                result.Append( "&country=" ).Append( country );
            return result.ToString();
        }

        private static string Md5Sum( string strToEncrypt )
        {
            UTF8Encoding ue = new UTF8Encoding();
            byte[] bytes = ue.GetBytes( strToEncrypt + "MeDiAtIoNhAsH" );
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hashBytes = md5.ComputeHash( bytes );
            StringBuilder hashString = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
                hashString.Append( Convert.ToString( hashBytes[i], 16 ).PadLeft( 2, '0' ) );
            return hashString.ToString().PadLeft( 32, '0' );
        }
    }
}