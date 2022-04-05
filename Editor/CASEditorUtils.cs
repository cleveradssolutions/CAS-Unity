using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public const string iosResSettingsPath = "ProjectSettings/ios_cas_settings";

        public const string promoDependency = "CrossPromotion";

        public const string androidPluginsPath = "Assets/Plugins/Android/";
        public const string mainGradlePath = androidPluginsPath + "mainTemplate.gradle";
        public const string launcherGradlePath = androidPluginsPath + "launcherTemplate.gradle";
        public const string projectGradlePath = androidPluginsPath + "baseProjectTemplate.gradle";
        public const string propertiesGradlePath = androidPluginsPath + "gradleTemplate.properties";
        public const string packageManifestPath = "Packages/manifest.json";

        public const string swiftEnableFileInXCode = "Libraries/com.cleversolutions.ads.unity/Plugins/iOS/CASUEnable.swift";

        public const string gitRootURL = "https://github.com/cleveradssolutions/";
        public const string websiteURL = "https://cleveradssolutions.com";

        public static System.Version minEDM4UVersion
        {
            get { return new System.Version( 1, 2, 169 ); }
        }
        #endregion

        #region Internal Constants
        internal const string gitUnityRepo = "CAS-Unity";
        internal const string gitUnityRepoURL = gitRootURL + gitUnityRepo;
        internal const string supportURL = gitUnityRepoURL + "#support";
        internal const string gitAppAdsTxtRepoUrl = gitRootURL + "App-ads.txt";
        internal const string attributionReportEndPoint = "https://postbacks-app.com"; // MAX

        internal const string generalDeprecateDependency = "General";
        internal const string teenDeprecateDependency = "Teen";
        internal const string promoDeprecateDependency = "Promo";

        internal const string logTag = "[CleverAdsSolutions] ";
        internal const string editorRuntimeActiveAdPrefs = "typesadsavailable";
        internal const string editorLatestVersionPrefs = "cas_last_ver_";
        internal const string editorLatestVersionTimestampPrefs = "cas_last_ver_time_";
        internal const string editorIgnoreMultidexPrefs = "cas_ignore_multidex";
        internal const string iosSKAdNetworksTemplateFile = "CASSKAdNetworks.txt";

        internal const string editorIconNamePrefix = "cas_editoricon_";
        #endregion

        #region Internal structures
        [Serializable]
        internal class AdmobAppIdData
        {
            public string admob_app_id = null;
#if false
            public int[] Banner = null;
            public int[] Interstitial = null;
            public int[] Rewarded = null;

            public bool IsDisabled()
            {
                return ( Interstitial == null || Interstitial.Length < 5 )
                    && ( Banner == null || Banner.Length < 5 )
                    && ( Rewarded == null || Rewarded.Length < 5 );
            }
#endif
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

        [MenuItem( "Assets/CleverAdsSolutions/Documentation..." )]
        public static void OpenDocumentationMenu()
        {
            OpenDocumentation( gitUnityRepo );
        }

        [MenuItem( "Assets/CleverAdsSolutions/Report Issue..." )]
        public static void ReportIssueMenu()
        {
            Application.OpenURL( gitRootURL + gitUnityRepo + "/issues" );
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
            string suffixChar = char.ToLower( managerId[managerId.Length - 1] ).ToString();
            return root + managerId.Length.ToString() + suffixChar + ".json";
        }

        public static CASInitSettings GetSettingsAsset( BuildTarget platform, bool create = true )
        {
            return ( CASInitSettings )GetSettingsAsset(
                "CASSettings" + platform.ToString(), "Assets/Resources", typeof( CASInitSettings ), create, "Resources" );
        }

        public static UnityEngine.Object GetSettingsAsset( string name, string location, Type type, bool create, string requireDir )
        {
            var found = AssetDatabase.FindAssets( name, new string[] { "Assets" } );
            for (int i = 0; i < found.Length; i++)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath( found[i] );
                if (assetPath.StartsWith( "Assets/" ) && assetPath.EndsWith( ".asset" ))
                {
                    if (string.IsNullOrEmpty( requireDir ) || Path.GetDirectoryName( assetPath ).EndsWith( requireDir ))
                    {
                        try
                        {
                            var asset = AssetDatabase.LoadAssetAtPath( assetPath, type );
                            if (asset)
                                return asset;
                            AssetDatabase.DeleteAsset( assetPath );
                        }
                        catch { }
                    }
                }
            }
            if (!create)
                return null;
            var result = ScriptableObject.CreateInstance( type );
            if (!AssetDatabase.IsValidFolder( location ))
            {
                Directory.CreateDirectory( location );
                AssetDatabase.ImportAsset( location );
            }
            AssetDatabase.CreateAsset( result, location + "/" + name + ".asset" );
            return result;
        }

        public static bool IsDependencyExists( string name, BuildTarget platform )
        {
            return AssetDatabase.FindAssets( GetDependencyName( name, platform ) ).Length > 0;
        }

        public static string GetDependencyName( string name, BuildTarget platform )
        {
            return "CAS" + platform.ToString() + name + "Dependencies";
        }

        public static string GetDependencyPath( string name, BuildTarget platform )
        {
            var depFileName = GetDependencyName( name, platform );
            var foundDepFiles = AssetDatabase.FindAssets( depFileName );
            for (int i = 0; i < foundDepFiles.Length; i++)
            {
                var pathToFile = AssetDatabase.GUIDToAssetPath( foundDepFiles[i] );
                if (pathToFile.EndsWith( ".xml" ))
                    return pathToFile;
            }
            return null;
        }

        public static string GetDependencyPathOrDefault( string name, BuildTarget platform )
        {
            var foundPath = GetDependencyPath( name, platform );
            if (foundPath != null)
                return foundPath;

            return editorFolderPath + "/" + GetDependencyName( name, platform ) + ".xml";
        }

        public static KeyValuePair[] DefaultUserTrackingUsageDescription()
        {
            return new KeyValuePair[]{
                new KeyValuePair( "en", "Get ads that are more interesting and support keeping this game free by allowing tracking." ),
                new KeyValuePair( "de", "\\\"Erlauben\\\" drücken benutzt Gerätinformationen für relevantere Werbeinhalte" ),
                new KeyValuePair( "ru", "Получайте более интересную рекламу и помогайте этой игре быть бесплатной, поделившись информацией о устройстве." ),
                new KeyValuePair( "es", "Presionando \\\"Permitir\\\", se usa la información del dispositivo para obtener contenido publicitario más relevante" ),
                new KeyValuePair( "fr", "\\\"Autoriser\\\" permet d'utiliser les infos du téléphone pour afficher des contenus publicitaires plus pertinents" ),
                new KeyValuePair( "ja", "\\\"許可\\\"をクリックすることで、デバイス情報を元により最適な広告を表示することができます" ),
                new KeyValuePair( "ko", "\\\"허용\\\"을 누르면 더 관련성 높은 광고 콘텐츠를 제공하기 위해 기기 정보가 사용됩니다" ),
                new KeyValuePair( "zh-Hans", "点击\\\"允许\\\"以使用设备信息获得更加相关的广告内容" ),
                new KeyValuePair( "zh-Hant", "點擊\\\"允許\\\"以使用設備信息獲得更加相關的廣告內容" )
            };
        }

        #region Deprecated Dependencies paths

        internal static bool IsDeprecateDependencyExists( string dependency, BuildTarget target )
        {
            return AssetDatabase.FindAssets( GetDeprecateDependencyName( dependency, target ) ).Length > 0;
        }

        internal static string GetDeprecateDependencyName( string dependency, BuildTarget target )
        {
            return "CAS" + dependency + target.ToString() + "Dependencies";
        }

        internal static string GetDeprecatedDependencyPath( string name, BuildTarget platform )
        {
            return editorFolderPath + "/CAS" + name + platform.ToString() + "Dependencies.xml";
        }
        #endregion

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

        public static System.Version GetEDM4UVersion( BuildTarget platform )
        {
            try
            {
                Type resolverType = null;
                if (platform == BuildTarget.Android)
                    resolverType = Type.GetType( "Google.AndroidResolverVersionNumber, Google.JarResolver", false );
                else if (platform == BuildTarget.iOS)
                    resolverType = Type.GetType( "Google.IOSResolverVersionNumber, Google.IOSResolver", false );
                if (resolverType == null)
                    return null;
                return resolverType.GetProperty( "Value" ).GetValue( null, null ) as System.Version;
            }
            catch
            {
                return null;
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
                            .GetValue( null, null );
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
                if (newVerStr != null && newVerStr != currVersion && !currVersion.Contains( "-RC" ))
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
                OpenDocumentation( gitRepoName );
            if (GUILayout.Button( "CleverAdsSolutions.com", EditorStyles.toolbarButton ))
                Application.OpenURL( websiteURL );
            EditorGUILayout.EndHorizontal();
        }

        public static void OnHeaderGUI( string gitRepoName, bool allowedPackageUpdate, string currVersion, ref string newCASVersion )
        {
            EditorGUILayout.BeginHorizontal( EditorStyles.toolbar );
            HelpStyles.HelpButton( gitRootURL + gitRepoName + "/wiki" );

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

        [Obsolete( "Please migrate to new implementation OnHeaderGUI()" )]
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

        internal static void OpenDocumentation( string gitRepoName )
        {
            Application.OpenURL( gitRootURL + gitRepoName + "/wiki" );
        }

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

        internal static string DownloadRemoteSettings( string managerID, BuildTarget platform, CASInitSettings settings, DependencyManager deps )
        {
            const string title = "Update CAS remote settings";

            var editorSettings = CASEditorSettings.Load();
            string casV = "";
            #region Find CAS Version
            if (deps != null)
            {
                var casDep = deps.Find( Dependency.adOptimalName );
                if (casDep != null)
                    casV = casDep.installedVersion;
                if (string.IsNullOrEmpty( casV ) && platform == BuildTarget.Android)
                {
                    casDep = deps.Find( Dependency.adFamiliesName );
                    if (casDep != null)
                        casV = casDep.installedVersion;
                }
                if (string.IsNullOrEmpty( casV ))
                {
                    casDep = deps.Find( Dependency.adBaseName );
                    if (casDep != null)
                        casV = casDep.version;
                }
            }

            if (casV.Length > 0)
            {
                try
                {
                    var parsesV = new System.Version( casV );
                    casV = parsesV.Major.ToString() + parsesV.Minor.ToString() + parsesV.Build.ToString( "D2" );
                }
                catch
                {
                    casV = "";
                }
            }
            #endregion

            #region Create request URL
            #region Hash
            var managerIdBytes = new UTF8Encoding().GetBytes( managerID );
            var suffix = new byte[] { 48, 77, 101, 68, 105, 65, 116, 73, 111, 78, 104, 65, 115, 72 };
            if (platform == BuildTarget.iOS)
                suffix[0] = 49;
            var sourceBytes = new byte[managerID.Length + suffix.Length];
            Array.Copy( managerIdBytes, 0, sourceBytes, 0, managerIdBytes.Length );
            Array.Copy( suffix, 0, sourceBytes, managerIdBytes.Length, suffix.Length );

            var hashBytes = new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash( sourceBytes );
            StringBuilder hashBuilder = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
                hashBuilder.Append( Convert.ToString( hashBytes[i], 16 ).PadLeft( 2, '0' ) );
            var hash = hashBuilder.ToString().PadLeft( 32, '0' );
            #endregion

            var urlBuilder = new StringBuilder( "https://psvpromo.psvgamestudio.com/Scr/cas.php?platform=" )
                .Append( platform == BuildTarget.Android ? 0 : 1 )
                .Append( "&bundle=" ).Append( UnityWebRequest.EscapeURL( managerID ) )
                .Append( "&hash=" ).Append( hash )
                .Append( "&lang=" ).Append( SystemLanguage.English )
                .Append( "&appDev=2" )
                .Append( "&appV=" ).Append( PlayerSettings.bundleVersion )
                .Append( "&coppa=" ).Append( ( int )settings.defaultAudienceTagged )
                .Append( "&adTypes=" ).Append( ( int )settings.allowedAdFlags )
                .Append( "&sdk=" ).Append( casV )
                .Append( "&nets=" ).Append( DependencyManager.GetActiveMediationPattern( deps ) )
                .Append( "&orient=" ).Append( GetOrientationId() )
                .Append( "&framework=Unity_" ).Append( Application.unityVersion );
            if (string.IsNullOrEmpty( editorSettings.mostPopularCountryOfUsers ))
                urlBuilder.Append( "&country=" ).Append( "US" );
            else
                urlBuilder.Append( "&country=" ).Append( editorSettings.mostPopularCountryOfUsers );
            if (platform == BuildTarget.Android)
                urlBuilder.Append( "&appVC=" ).Append( PlayerSettings.Android.bundleVersionCode );

            #endregion

            using (var loader = UnityWebRequest.Get( urlBuilder.ToString() ))
            {
                try
                {
                    loader.SendWebRequest();
                    while (!loader.isDone)
                    {
                        if (EditorUtility.DisplayCancelableProgressBar( title, managerID,
                            Mathf.Repeat( ( float )EditorApplication.timeSinceStartup, 1.0f ) ))
                        {
                            loader.Dispose();
                            throw new Exception( "Update CAS Settings canceled" );
                        }
                    }
                    if (string.IsNullOrEmpty( loader.error ))
                    {
                        var content = loader.downloadHandler.text.Trim();
                        if (string.IsNullOrEmpty( content ))
                            throw new Exception( "ManagerID [" + managerID + "] is not registered in CAS." );

                        EditorUtility.DisplayProgressBar( title, "Write CAS settings", 0.7f );
                        var data = JsonUtility.FromJson<AdmobAppIdData>( content );
                        WriteSettingsForPlatform( content, managerID, platform );
                        return data.admob_app_id;
                    }
                    throw new Exception( "Server response " + loader.responseCode + ": " + loader.error );
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        public static string ReadAppIdFromCache( string managerID, BuildTarget platform, string error )
        {
            const string title = "Update CAS remote settings";
            var cachePath = GetNativeSettingsPath( platform, managerID );
            int dialogResponse = 0;


            if (!Application.isBatchMode)
                dialogResponse = EditorUtility.DisplayDialogComplex( title,
                    error +
                    "\nPlease try using a real identifier in the first place else contact support." +
                    "\n- Warning! -" +
                    "\n1. Continue build the app for release with current settings can reduce monetization revenue." +
                    "\n2. When build to testing your app, make sure you use Test Ads mode rather than live ads. " +
                    "Failure to do so can lead to suspension of your account.",
                    "Continue", "Cancel Build", "Select settings file" );

            if (dialogResponse == 0)
            {
                if (File.Exists( cachePath ))
                    return GetAdmobAppIdFromJson( File.ReadAllText( cachePath ) );
                return null;
            }
            if (dialogResponse == 1)
            {
                StopBuildWithMessage( "Build canceled", BuildTarget.NoTarget );
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
            catch (Exception e)
            {
                Debug.LogException( e );
            }
            StopBuildWithMessage( "Selected wrong settings file: " + filePath, BuildTarget.NoTarget );
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
            if (!force)
            {
                var editorSettings = CASEditorSettings.Load();
                if (!editorSettings.autoCheckForUpdatesEnabled
                    || !HasTimePassed( editorLatestVersionTimestampPrefs + repo, 1, false ))
                {
                    var last = EditorPrefs.GetString( editorLatestVersionPrefs + repo );
                    if (!string.IsNullOrEmpty( last ))
                        return last;
                }
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

        private static int GetOrientationId()
        {
            var orientation = Screen.orientation;
            if (orientation == ScreenOrientation.Portrait || orientation == ScreenOrientation.PortraitUpsideDown)
                return 1;
            if (orientation == ScreenOrientation.Landscape || orientation == ScreenOrientation.LandscapeRight)
                return 2;
            var supportPortrait = Screen.autorotateToPortrait || Screen.autorotateToPortraitUpsideDown;
            var supportLandscape = Screen.autorotateToLandscapeLeft || Screen.autorotateToLandscapeRight;
            if (supportPortrait && supportLandscape)
                return 0;
            if (supportPortrait)
                return 1;
            if (supportLandscape)
                return 2;
            return 0;
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

    [Serializable]
    public class KeyValuePair
    {
        public string key;
        public string value;

        public KeyValuePair( string key, string value )
        {
            this.key = key;
            this.value = value;
        }
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

        public static Texture[] formatIcons;

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

            formatIcons = new Texture[5 * 2];
            var editorIcons = AssetDatabase.FindAssets( CASEditorUtils.editorIconNamePrefix );
            for (int i = 0; i < editorIcons.Length; i++)
            {
                var iconPath = AssetDatabase.GUIDToAssetPath( editorIcons[i] );
                var asset = AssetDatabase.LoadAssetAtPath<Texture2D>( iconPath );
                var suffix = iconPath.Substring( iconPath.LastIndexOf( '_' ) + 1 );
                var index = -1;
                switch (suffix)
                {
                    case "banner.png":
                        index = 0;
                        break;
                    case "inter.png":
                        index = 1;
                        break;
                    case "reward.png":
                        index = 2;
                        break;
                    case "native.png":
                        index = 3;
                        break;
                    case "mrec.png":
                        index = 4;
                        break;
                }
                if (index > -1)
                    formatIcons[index + ( iconPath.Contains( "_dark_" ) ? 5 : 0 )] = asset;
            }
        }

        public static Texture GetFormatIcon( AdFlags format, bool active )
        {
            int iconIndex;
            switch (format)
            {
                case AdFlags.Banner: iconIndex = 0; break;
                case AdFlags.Interstitial: iconIndex = 1; break;
                case AdFlags.Rewarded: iconIndex = 2; break;
                case AdFlags.Native: iconIndex = 3; break;
                case AdFlags.MediumRectangle: iconIndex = 4; break;
                default: return null;
            }
            return formatIcons[active ? iconIndex : iconIndex + 5];
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

        public static void Devider()
        {
            var dividerRect = EditorGUILayout.GetControlRect( GUILayout.Height( 1 ) );
            if (Event.current.type == EventType.Repaint) //draw the divider
                GUI.skin.box.Draw( dividerRect, GUIContent.none, 0 );
        }

        public static void HelpButton( string url )
        {
            if (GUILayout.Button( helpIconContent, EditorStyles.label, GUILayout.ExpandWidth( false ) ))
                Application.OpenURL( url );

        }
    }
}