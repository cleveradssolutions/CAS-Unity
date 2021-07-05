using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Networking;

namespace CAS.UEditor
{
    public static class CASEditorUtils
    {
        #region Constants
        public const string packageName = "com.cleversolutions.ads.unity";
        public const string androidAdmobSampleAppID = "ca-app-pub-3940256099942544~3347511713";
        public const string iosAdmobSampleAppID = "ca-app-pub-3940256099942544~1458002511";

        public const string rootCASFolderPath = "Assets/CleverAdsSolutions";
        public const string editorFolderPath = rootCASFolderPath + "/Editor";
        public const string androidLibFolderPath = "Assets/Plugins/Android/CASPlugin.androidlib";
        public const string androidResSettingsPath = androidLibFolderPath + "/res/raw/cas_settings";
        public const string androidLibManifestPath = androidLibFolderPath + "/AndroidManifest.xml";
        public const string androidLibPropertiesPath = androidLibFolderPath + "/project.properties";

        public const string iosResSettingsPath = editorFolderPath + "/ios_cas_settings";

        public const string promoDependency = "CrossPromotion";

        public const string mainGradlePath = "Assets/Plugins/Android/mainTemplate.gradle";
        public const string launcherGradlePath = "Assets/Plugins/Android/launcherTemplate.gradle";
        public const string projectGradlePath = "Assets/Plugins/Android/baseProjectTemplate.gradle";
        public const string packageManifestPath = "Packages/manifest.json";

        public const string gitRootURL = "https://github.com/cleveradssolutions/";
        public const string websiteURL = "https://cleveradssolutions.com";
        #endregion

        #region Internal Constants
        internal const string gitUnityRepo = "CAS-Unity";
        internal const string gitUnityRepoURL = gitRootURL + gitUnityRepo;
        internal const string supportURL = gitUnityRepoURL + "#support";
        internal const string gitAppAdsTxtRepoUrl = gitRootURL + "App-ads.txt";

        internal const string generalDeprecateDependency = "General";
        internal const string teenDeprecateDependency = "Teen";
        internal const string promoDeprecateDependency = "Promo";
        internal const string dependenciesExtension = "Dependencies.xml";

        internal const string logTag = "[CleverAdsSolutions] ";
        internal const string editorRuntimeActiveAdPrefs = "typesadsavailable";
        internal const string editorLatestVersionPrefs = "cas_last_ver_";
        internal const string editorLatestVersionTimestampPrefs = "cas_last_ver_time_";
        internal const string editorIgnoreMultidexPrefs = "cas_ignore_multidex";
        internal const string androidLibManifestTemplateFile = "CASManifest.xml";
        internal const string androidLibPropTemplateFile = "CASLibProperties.txt";
        internal const string iosSKAdNetworksTemplateFile = "CASSKAdNetworks.txt";

        internal const string preferredCountry = "BR"; // ISO2: US, RU ...
        #endregion

        #region Internal structures
        [Serializable]
        internal class AdmobAppIdData
        {
            public string admob_app_id = null;
            public int[] Banner = null;
            public int[] Interstitial = null;
            public int[] Rewarded = null;

            public bool IsDisabled()
            {
                return ( Interstitial == null || Interstitial.Length < 5 )
                    && ( Banner == null || Banner.Length < 5 )
                    && ( Rewarded == null || Rewarded.Length < 5 );
            }
        }

        [Serializable]
        internal class GitVersionInfo
        {
            public string tag_name = null;
        }
        #endregion

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

        #region Public API
        public static string GetName( this AdNetwork network )
        {
            if (network == Dependency.adBase)
                return Dependency.adBaseName;
            return network.ToString();
        }

        public static string GetNativeSettingsPath( BuildTarget platform, string managerId )
        {
            if (string.IsNullOrEmpty( managerId ))
                return "";

            string root = platform == BuildTarget.Android ? androidResSettingsPath : iosResSettingsPath;
            return root + managerId.Length.ToString() + managerId[managerId.Length - 1] + ".json";
        }

        public static CASInitSettings GetSettingsAsset( BuildTarget platform, bool create = true )
        {
            if (!AssetDatabase.IsValidFolder( "Assets/Resources" ))
                AssetDatabase.CreateFolder( "Assets", "Resources" );
            var assetPath = "Assets/Resources/CASSettings" + platform.ToString() + ".asset";
            var asset = AssetDatabase.LoadAssetAtPath<CASInitSettings>( assetPath );
            if (create && !asset)
            {
                asset = ScriptableObject.CreateInstance<CASInitSettings>();
                AssetDatabase.CreateAsset( asset, assetPath );
            }
            return asset;
        }

        public static bool IsDependencyExists( string dependency, BuildTarget target )
        {
            return AssetDatabase.FindAssets( GetDependencyName( dependency, target ) ).Length > 0;
        }

        public static string GetDependencyName( string dependency, BuildTarget target )
        {
            return "CAS" + target.ToString() + dependency + "Dependencies";
        }

        public static string GetDependencyPath( string name, BuildTarget platform )
        {
            return editorFolderPath + "/CAS" + platform.ToString() + name + dependenciesExtension;
        }

        public static bool IsAndroidDependenciesResolverExist()
        {
            try
            {
                var resolverType = Type.GetType( "GooglePlayServices.PlayServicesResolver, Google.JarResolver", false );
                return resolverType != null;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryResolveAndroidDependencies( bool force = true )
        {
            bool success = true;
            try
            {
                var resolverType = Type.GetType( "GooglePlayServices.PlayServicesResolver, Google.JarResolver", false );
                if (resolverType != null)
                {
                    bool autoResolve;
                    if (force)
                        autoResolve = false;
                    else
                        autoResolve = ( bool )resolverType.GetProperty( "AutomaticResolutionEnabled",
                            BindingFlags.Public | BindingFlags.Static )
                            .GetValue( null );
                    if (!autoResolve)
                    {
                        try
                        {
                            EditorUtility.DisplayProgressBar( "Hold on.", "Resolve Android dependencies", 0.6f );
                            success = ( bool )resolverType.GetMethod( "ResolveSync", BindingFlags.Public | BindingFlags.Static, null,
                                new[] { typeof( bool ) }, null )
                                .Invoke( null, new object[] { true } );
                        }
                        finally
                        {
                            EditorUtility.ClearProgressBar();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning( logTag + "GooglePlayServices.PlayServicesResolver error: " + e.Message );
            }
            return success;
        }

        public static string GetNewVersionOrNull( string repo, string currVersion, bool force )
        {
            try
            {
                var newVerStr = GetLatestVersion( repo, force, currVersion );
                if (newVerStr != null && newVerStr != currVersion)
                {
                    var currVer = new System.Version( currVersion );
                    var newVer = new System.Version( newVerStr );
                    if (currVer < newVer)
                        return newVerStr;
                }
            }
            catch (Exception e)
            {
                Debug.LogException( e );
            }
            return null;
        }

        public static bool IsPackageExist( string package )
        {
            return File.Exists( packageManifestPath ) &&
                File.ReadAllText( packageManifestPath ).Contains( package );
        }

        public static void LinksToolbarGUI( string gitRepoName )
        {
            EditorGUILayout.BeginHorizontal( EditorStyles.toolbar );
            if (GUILayout.Button( "Support", EditorStyles.toolbarButton, GUILayout.ExpandWidth( false ) ))
                Application.OpenURL( gitRootURL + gitRepoName + "#support" );
            if (GUILayout.Button( "Wiki", EditorStyles.toolbarButton, GUILayout.ExpandWidth( false ) ))
                Application.OpenURL( gitRootURL + gitRepoName + "/wiki" );
            if (GUILayout.Button( "CleverAdsSolutions.com", EditorStyles.toolbarButton ))
                Application.OpenURL( websiteURL );
            EditorGUILayout.EndHorizontal();
        }

        public static void OnHeaderGUI( string gitRepoName, bool allowedPackageUpdate, string currVersion, ref string newCASVersion )
        {
            EditorGUILayout.BeginHorizontal( EditorStyles.toolbar );
            if (GUILayout.Button( HelpStyles.helpIconContent, EditorStyles.toolbarButton, GUILayout.ExpandWidth( false ) ))
                Application.OpenURL( gitRootURL + gitRepoName + "/wiki" );

            if (GUILayout.Button( gitRepoName + " " + currVersion, EditorStyles.toolbarButton ))
                Application.OpenURL( gitRootURL + gitRepoName + "/releases" );
            if (GUILayout.Button( "Check for Updates", EditorStyles.toolbarButton, GUILayout.ExpandWidth( false ) ))
            {
                newCASVersion = GetNewVersionOrNull( gitRepoName, currVersion, true );
                string message = string.IsNullOrEmpty( newCASVersion ) ? "You are using the latest version."
                    : "There is a new version " + newCASVersion + " of the " + gitRepoName + " available for update.";
                EditorUtility.DisplayDialog( "Check for Updates", message, "OK" );
            }

            if (GUILayout.Button( "Support", EditorStyles.toolbarButton, GUILayout.ExpandWidth( false ) ))
                Application.OpenURL( gitRootURL + gitRepoName + "#support" );
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty( newCASVersion ))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox( "There is a new version " + newCASVersion + " of the " + gitRepoName + " available for update.", MessageType.Warning );

#if UNITY_2018_4_OR_NEWER
                if (allowedPackageUpdate && GUILayout.Button( "Update", GUILayout.ExpandHeight( true ), GUILayout.ExpandWidth( false ) ))
                {
                    var request = UnityEditor.PackageManager.Client.Add( gitRootURL + gitRepoName + ".git#" + newCASVersion );
                    try
                    {
                        while (!request.IsCompleted)
                        {
                            if (EditorUtility.DisplayCancelableProgressBar(
                                "Update Package Manager dependency", gitRepoName + " " + newCASVersion, 0.5f ))
                                break;
                        }
                        if (request.Status == UnityEditor.PackageManager.StatusCode.Success)
                            Debug.Log( "Package Manager: Update " + request.Result.displayName );
                        else if (request.Status >= UnityEditor.PackageManager.StatusCode.Failure)
                            Debug.LogError( request.Error.message );
                    }
                    finally
                    {
                        EditorUtility.ClearProgressBar();
                    }
                }
#endif
                EditorGUILayout.EndHorizontal();
            }
        }

        public static void AboutRepoGUI( string gitRepoName, bool allowedPackageUpdate, string currVersion, ref string newCASVersion )
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label( gitRepoName, EditorStyles.boldLabel, GUILayout.ExpandWidth( false ) );
            GUILayout.Label( currVersion );
            if (GUILayout.Button( "Check for Updates", EditorStyles.miniButton, GUILayout.ExpandWidth( false ) ))
            {
                newCASVersion = GetNewVersionOrNull( gitRepoName, currVersion, true );
                string message = string.IsNullOrEmpty( newCASVersion ) ? "You are using the latest version."
                    : "There is a new version " + newCASVersion + " of the " + gitRepoName + " available for update.";
                EditorUtility.DisplayDialog( "Check for Updates", message, "OK" );
            }
            EditorGUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty( newCASVersion ))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox( "There is a new version " + newCASVersion + " of the " + gitRepoName + " available for update.", MessageType.Warning );
                var layoutParams = new[] { GUILayout.Height( 38 ), GUILayout.ExpandWidth( false ) };
#if UNITY_2018_4_OR_NEWER
                if (allowedPackageUpdate && GUILayout.Button( "Update", layoutParams ))
                {
                    var request = UnityEditor.PackageManager.Client.Add( gitRootURL + gitRepoName + ".git#" + newCASVersion );
                    try
                    {
                        while (!request.IsCompleted)
                        {
                            if (EditorUtility.DisplayCancelableProgressBar(
                                "Update Package Manager dependency", gitRepoName + " " + newCASVersion, 0.5f ))
                                break;
                        }
                        if (request.Status == UnityEditor.PackageManager.StatusCode.Success)
                            Debug.Log( "Package Manager: Update " + request.Result.displayName );
                        else if (request.Status >= UnityEditor.PackageManager.StatusCode.Failure)
                            Debug.LogError( request.Error.message );
                    }
                    finally
                    {
                        EditorUtility.ClearProgressBar();
                    }
                }
#endif
                if (GUILayout.Button( "Releases", layoutParams ))
                    Application.OpenURL( gitRootURL + gitRepoName + "/releases" );
                EditorGUILayout.EndHorizontal();
            }
        }
        #endregion

        #region Internal API
        internal static bool IsFirebaseServiceExist( string service )
        {
            if (AssetDatabase.FindAssets( "Firebase." + service ).Length > 0)
                return true;

            return IsPackageExist( "com.google.firebase." + service );
        }

        internal static void OpenSettingsWindow( BuildTarget target )
        {
            var asset = GetSettingsAsset( target );
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject( asset );
        }

        internal static string GetAdmobAppIdFromJson( string json )
        {
            return JsonUtility.FromJson<AdmobAppIdData>( json ).admob_app_id;
        }

        internal static void CreateFolderInAssets( string folderName )
        {
            if (!AssetDatabase.IsValidFolder( rootCASFolderPath ))
                AssetDatabase.CreateFolder( "Assets", "CleverAdsSolutions" );
            if (!AssetDatabase.IsValidFolder( rootCASFolderPath + "/" + folderName ))
                AssetDatabase.CreateFolder( rootCASFolderPath, folderName );
        }

        internal static bool IsDeprecateDependencyExists( string dependency, BuildTarget target )
        {
            return AssetDatabase.FindAssets( GetDeprecateDependencyName( dependency, target ) ).Length > 0;
        }

        internal static string GetDeprecateDependencyName( string dependency, BuildTarget target )
        {
            return "CAS" + dependency + target.ToString() + "Dependencies";
        }

        internal static string GetTemplatePath( string templateFile )
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

        internal static string GetDeprecatedDependencyPath( string name, BuildTarget platform )
        {
            return editorFolderPath + "/CAS" + name + platform.ToString() + dependenciesExtension;
        }

        internal static bool TryCopyFile( string source, string dest )
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

        internal static void WriteToFile( string data, string path )
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

        internal static bool IsPortraitOrientation()
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

        internal static void StopBuildWithMessage( string message, BuildTarget target )
        {
            EditorUtility.ClearProgressBar();
            if (target != BuildTarget.NoTarget
                && !Application.isBatchMode
                && EditorUtility.DisplayDialog( "CAS Stop Build", message, "Open Settings", "Close" ))
            {
                OpenSettingsWindow( target );
            }

#if UNITY_2018_1_OR_NEWER
            throw new BuildFailedException( logTag + message );
#elif UNITY_2017_1_OR_NEWER
            throw new BuildPlayerWindow.BuildMethodException( logTag + message );
#else
            throw new OperationCanceledException(logTag + message);
#endif
        }

        internal static string BuildRemoteUrl( string managerID, string country, BuildTarget platform )
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

        internal static string DownloadRemoteSettings( string managerID, string country, BuildTarget platform, bool main )
        {
            const string title = "Update CAS remote settings";
            string url = BuildRemoteUrl( managerID, country, platform );
            string message = null;

            AdmobAppIdData data = null;
            using (var loader = UnityWebRequest.Get( url ))
            {
                loader.SendWebRequest();
                while (!loader.isDone)
                {
                    if (EditorUtility.DisplayCancelableProgressBar( title, managerID,
                        Mathf.Repeat( ( float )EditorApplication.timeSinceStartup, 1.0f ) ))
                    {
                        loader.Dispose();
                        message = "Update CAS Settings canceled";
                        break;
                    }
                }
                EditorUtility.ClearProgressBar();
                if (message == null)
                {
                    if (string.IsNullOrEmpty( loader.error ))
                    {
                        EditorUtility.DisplayProgressBar( title, "Write CAS settings", 0.7f );
                        var content = loader.downloadHandler.text.Trim();
                        if (string.IsNullOrEmpty( content ))
                            StopBuildWithMessage( "Server have no settings for " + managerID +
                                " Please try using a different identifier in the first place or contact support." +
                                " To test build please use Test Ad Mode in settings.", platform );

                        try
                        {
                            data = JsonUtility.FromJson<AdmobAppIdData>( content );
                            if (main && data.IsDisabled())
                            {
                                message = "Ads are currently disabled for your application. You can use the cached settings or try later.";
                            }
                            else
                            {
                                WriteSettingsForPlatform( content, managerID, platform );
                                return data.admob_app_id;
                            }
                        }
                        catch (Exception e)
                        {
                            message = e.ToString();
                        }
                    }
                    else
                    {
                        message = "Server response " + loader.responseCode + ": " + loader.error;
                    }
                }
            }
            if (!main)
                return null;
            var cachePath = GetNativeSettingsPath( platform, managerID );
            if (Application.isBatchMode)
            {
                if (data != null)
                    // This content will be not save to cache
                    return data.admob_app_id;
                if (File.Exists( cachePath ))
                    return GetAdmobAppIdFromJson( File.ReadAllText( cachePath ) );
                StopBuildWithMessage( message, BuildTarget.NoTarget );
                return null;
            }

            if (data != null || File.Exists( cachePath ))
            {
                var dialogResponse = EditorUtility.DisplayDialogComplex( title, message, "Use cache", "Cancel Build", "Select settings file" );
                if (dialogResponse == 0)
                {
                    if (data == null)
                        return GetAdmobAppIdFromJson( File.ReadAllText( cachePath ) );
                    // This content will be not save to cache
                    return data.admob_app_id;
                }
                if (dialogResponse == 1)
                {
                    StopBuildWithMessage( message, BuildTarget.NoTarget );
                    return null;
                }
            }
            else if (!EditorUtility.DisplayDialog( title, message, "Select settings file", "Cancel Build" ))
            {
                StopBuildWithMessage( message, BuildTarget.NoTarget );
                return null;
            }

            var filePath = EditorUtility.OpenFilePanelWithFilters(
                "Select CAS Settings file for build", "", new[] { "Settings file", "json" } );
            try
            {
                if (!string.IsNullOrEmpty( filePath ))
                {
                    var content = File.ReadAllText( filePath );
                    WriteSettingsForPlatform( content, managerID, platform );
                    return GetAdmobAppIdFromJson( content );
                }
            }
            catch
            {
                StopBuildWithMessage( "Selected wrong file: " + filePath, BuildTarget.NoTarget );
                return null;
            }
            StopBuildWithMessage( message, BuildTarget.NoTarget );
            return null;
        }

        private static void WriteSettingsForPlatform( string content, string managerId, BuildTarget target )
        {
            WriteToFile( content, GetNativeSettingsPath( target, managerId ) );
        }

        internal static void GetCrossPromoAlias( BuildTarget platform, string managerId, HashSet<string> result )
        {
            if (!IsDependencyExists( promoDependency, platform ))
                return;

            string pattern = "alias\\\": \\\""; //: "iOSBundle\\\": \\\"";
            string cachePath = GetNativeSettingsPath( platform, managerId );

            if (File.Exists( cachePath ))
            {
                try
                {
                    string content = File.ReadAllText( cachePath );
                    int beginIndex = content.IndexOf( pattern );
                    while (beginIndex > 0)
                    {
                        beginIndex += pattern.Length;
                        var endIndex = content.IndexOf( '\\', beginIndex );
                        result.Add( content.Substring( beginIndex, endIndex - beginIndex ) );
                        beginIndex = content.IndexOf( pattern, endIndex );
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException( e );
                }
            }
            return;
        }

        internal static string GetLatestVersion( string repo, bool force, string currVersion )
        {
            if (!force && !HasTimePassed( editorLatestVersionTimestampPrefs + repo, 1, false ))
            {
                var last = EditorPrefs.GetString( editorLatestVersionPrefs + repo );
                if (!string.IsNullOrEmpty( last ))
                    return last;
            }

            const string title = "Get latest CAS version info";
            string url = "https://api.github.com/repos/cleveradssolutions/" + repo + "/releases/latest";

            using (var loader = UnityWebRequest.Get( url ))
            {
                loader.timeout = 30;
                loader.SendWebRequest();
                try
                {
                    while (!loader.isDone)
                    {
                        if (EditorUtility.DisplayCancelableProgressBar( title, repo,
                            Mathf.Repeat( ( float )EditorApplication.timeSinceStartup, 1.0f ) ))
                        {
                            loader.Dispose();
                            SaveLatestRepoVersion( repo, currVersion );
                            return null;
                        }
                    }
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }

                if (string.IsNullOrEmpty( loader.error ))
                {
                    var content = loader.downloadHandler.text;
                    var versionInfo = JsonUtility.FromJson<GitVersionInfo>( content );
                    if (!string.IsNullOrEmpty( versionInfo.tag_name ))
                        SaveLatestRepoVersion( repo, versionInfo.tag_name );

                    return versionInfo.tag_name;
                }
                else
                {
                    Debug.LogError( logTag + "Check " + repo + " updates failed. Response " +
                        loader.responseCode + ": " + loader.error );
                    SaveLatestRepoVersion( repo, currVersion );
                }

            }

            return null;
        }

        private static void SaveLatestRepoVersion( string repo, string version )
        {
            EditorPrefs.SetString( editorLatestVersionPrefs + repo, version );
            EditorPrefs.SetString( editorLatestVersionTimestampPrefs + repo, DateTime.Now.ToBinary().ToString() );
        }

        internal static bool HasTimePassed( string prefKey, int days, bool projectOnly )
        {
            string pref;
            if (projectOnly)
                pref = PlayerPrefs.GetString( prefKey, string.Empty );
            else
                pref = EditorPrefs.GetString( prefKey, string.Empty );

            if (string.IsNullOrEmpty( pref ))
            {
                return true;
            }
            else
            {
                DateTime checkTime;
                try
                {
                    long binartDate = long.Parse( pref );
                    checkTime = DateTime.FromBinary( binartDate );
                }
                catch
                {
                    return true;
                }
                checkTime = checkTime.Add( TimeSpan.FromDays( days ) );
                return DateTime.Compare( DateTime.Now, checkTime ) > 0; // Now time is later than checkTime
            }
        }
        #endregion
    }

    public static class HelpStyles
    {
        public static GUIStyle labelStyle;
        public static GUIStyle wordWrapTextAred;
        public static GUIStyle largeTitleStyle;
        private static GUIStyle boxScopeStyle;

        public static GUIContent helpIconContent;
        public static GUIContent errorIconContent;
        private static GUIContent tempContent;

        static HelpStyles()
        {
            labelStyle = new GUIStyle( "AssetLabel" );
            labelStyle.fontSize = 9;
            labelStyle.fixedHeight = 10;
            labelStyle.padding = new RectOffset( 4, 3, 0, 0 );
            boxScopeStyle = new GUIStyle( EditorStyles.helpBox );
            boxScopeStyle.padding = new RectOffset( 6, 6, 6, 6 );
            wordWrapTextAred = new GUIStyle( EditorStyles.textArea );
            wordWrapTextAred.wordWrap = true;
            errorIconContent = EditorGUIUtility.IconContent( "d_console.erroricon.sml" );
            errorIconContent = new GUIContent( string.Empty, errorIconContent.image,
                "Remove dependency.\nThe dependency cannot be used in current configuration." );
            helpIconContent = EditorGUIUtility.IconContent( "_Help" );
            helpIconContent = new GUIContent( string.Empty, helpIconContent.image,
                "Open home page of the mediation network." );
            tempContent = new GUIContent();
            largeTitleStyle = new GUIStyle( EditorStyles.boldLabel );
            largeTitleStyle.fontSize = 18;
        }

        public static void BeginBoxScope()
        {
            EditorGUILayout.BeginVertical( boxScopeStyle );
        }

        public static void EndBoxScope()
        {
            EditorGUILayout.EndVertical();
        }

        public static GUIContent GetContent( string text, Texture image, string tooltip = "" )
        {
            tempContent.text = text;
            tempContent.image = image;
            tempContent.tooltip = tooltip;
            return tempContent;
        }
    }
}