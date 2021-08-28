#pragma warning disable 649
using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Utils = CAS.UEditor.CASEditorUtils;
using System.IO;

namespace CAS.UEditor
{
    [CustomEditor( typeof( CASInitSettings ) )]
    internal class CASInitSettignsInspector : Editor
    {
        private SerializedProperty testAdModeProp;
        private SerializedProperty managerIdsProp;
        private SerializedProperty allowedAdFlagsProp;
        private SerializedProperty audienceTaggedProp;
        private SerializedProperty debugModeProp;
        private SerializedProperty bannerRefreshProp;
        private SerializedProperty interstitialIntervalProp;
        private SerializedProperty loadingModeProp;
        private SerializedProperty bannerSizeProp;
        private SerializedProperty trackLocationEnabledProp;
        private SerializedProperty analyticsCollectionEnabledProp;
        private SerializedProperty interWhenNoRewardedAdProp;

        private DependencyManager dependencyManager;

        private ReorderableList managerIdsList;
        private BuildTarget platform;
        private bool allowedPackageUpdate;
        private string newCASVersion = null;
        private bool deprecateDependenciesExist;
        private bool edmExist;

        private string[] deprecatedAssets = null;

        private int editorRuntimeActiveAdFlags;

        private void OnEnable()
        {
            var props = serializedObject;
            testAdModeProp = props.FindProperty( "testAdMode" );
            managerIdsProp = props.FindProperty( "managerIds" );
            allowedAdFlagsProp = props.FindProperty( "allowedAdFlags" );
            audienceTaggedProp = props.FindProperty( "audienceTagged" );
            debugModeProp = props.FindProperty( "debugMode" );
            bannerRefreshProp = props.FindProperty( "bannerRefresh" );
            interstitialIntervalProp = props.FindProperty( "interstitialInterval" );
            loadingModeProp = props.FindProperty( "loadingMode" );
            bannerSizeProp = props.FindProperty( "bannerSize" );
            trackLocationEnabledProp = props.FindProperty( "trackLocationEnabled" );
            analyticsCollectionEnabledProp = props.FindProperty( "analyticsCollectionEnabled" );
            interWhenNoRewardedAdProp = props.FindProperty( "interWhenNoRewardedAd" );

            editorRuntimeActiveAdFlags = PlayerPrefs.GetInt( Utils.editorRuntimeActiveAdPrefs, -1 );

            string assetName = target.name;
            if (assetName.EndsWith( BuildTarget.Android.ToString() ))
                platform = BuildTarget.Android;
            else if (assetName.EndsWith( BuildTarget.iOS.ToString() ))
                platform = BuildTarget.iOS;
            else
                platform = BuildTarget.NoTarget;

            managerIdsList = new ReorderableList( props, managerIdsProp, true, true, true, true )
            {
                drawHeaderCallback = DrawListHeader,
                drawElementCallback = DrawListElement,
                onCanRemoveCallback = ( list ) => list.count > 1,
            };

            allowedPackageUpdate = Utils.IsPackageExist( Utils.packageName );

            //usingMultidexOnBuild = PlayerPrefs.GetInt( Utils.editorIgnoreMultidexPrefs, 0 ) == 0;

            dependencyManager = DependencyManager.Create( platform, ( Audience )audienceTaggedProp.enumValueIndex, true );

            EditorApplication.delayCall += () => newCASVersion = Utils.GetNewVersionOrNull( Utils.gitUnityRepo, MobileAds.wrapperVersion, false );
            props.ApplyModifiedProperties();

            deprecatedAssets = new string[]{
                Utils.GetDeprecateDependencyName( Utils.generalDeprecateDependency, platform ),
                Utils.GetDeprecateDependencyName( Utils.teenDeprecateDependency, platform ),
                Utils.GetDeprecateDependencyName( Utils.promoDeprecateDependency, platform ),
                Utils.GetDependencyName( "Additional", platform )
            };

            for (int i = 0; i < deprecatedAssets.Length; i++)
            {
                if (deprecateDependenciesExist |= AssetDatabase.FindAssets( deprecatedAssets[i] ).Length > 0)
                    break;
            }

            if (File.Exists( Utils.androidResSettingsPath + ".json" ))
                AssetDatabase.MoveAssetToTrash( Utils.androidResSettingsPath + ".json" );

            edmExist = Utils.IsAndroidDependenciesResolverExist();
        }

        private void DrawListHeader( Rect rect )
        {
            EditorGUI.LabelField( rect, "Manager ID's" );
        }

        private void DrawListElement( Rect rect, int index, bool isActive, bool isFocused )
        {
            var item = managerIdsProp.GetArrayElementAtIndex( index );
            rect.yMin += 1;
            rect.yMax -= 1;
            item.stringValue = EditorGUI.TextField( rect, item.stringValue );
        }

        protected override void OnHeaderGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label( "CleverAdsSolutions", HelpStyles.largeTitleStyle );
            GUILayout.Label( platform.ToString(), HelpStyles.largeTitleStyle, GUILayout.ExpandWidth( false ) );
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            Utils.OnHeaderGUI( Utils.gitUnityRepo, allowedPackageUpdate, MobileAds.wrapperVersion, ref newCASVersion );
        }

        public override void OnInspectorGUI()
        {
            var obj = serializedObject;
            obj.UpdateIfRequiredOrScript();

            HelpStyles.BeginBoxScope();
            if (managerIdsList.count > 1)
            {
                var appId = managerIdsProp.GetArrayElementAtIndex( 0 ).stringValue;
                if (!string.IsNullOrEmpty( appId ))
                    EditorGUILayout.LabelField( "Application id:", appId );
            }
            managerIdsList.DoLayoutList();
            OnManagerIDVerificationGUI();
            allowedAdFlagsProp.intValue = Convert.ToInt32(
               EditorGUILayout.EnumFlagsField( "Allowed ads in app", ( AdFlags )allowedAdFlagsProp.intValue ) );


            EditorGUILayout.PropertyField( testAdModeProp );
            if (testAdModeProp.boolValue)
            {
                EditorGUILayout.HelpBox( "Make sure you disable test ad mode and replace test manager ID with your own ad manager ID before publishing your app!", MessageType.Warning );
            }
            else if (EditorUserBuildSettings.development)
            {
                EditorGUILayout.HelpBox( "Development build enabled, only test ads are allowed. " +
                    "\nMake sure you disable Development build and use real ad manager ID before publishing your app!", MessageType.Warning );
            }
            else
            {
                EditorGUILayout.LabelField( "When testing your app, make sure you use Test Ads mode rather than live ads. " +
                "Failure to do so can lead to suspension of your account.", EditorStyles.wordWrappedMiniLabel );
            }
            HelpStyles.EndBoxScope();

            OnBannerSizeGUI();
            bannerRefreshProp.intValue = Mathf.Clamp(
                 EditorGUILayout.IntField( "Banner refresh rate(sec)", bannerRefreshProp.intValue ), 10, short.MaxValue );

            DrawSeparator();
            interstitialIntervalProp.intValue = Math.Max( 0,
                EditorGUILayout.IntField( "Interstitial impression interval(sec)", interstitialIntervalProp.intValue ) );
            interWhenNoRewardedAdProp.boolValue = EditorGUILayout.ToggleLeft(
                "Allow Interstitial Ad when the cost of the Rewarded Ad is lower",
                interWhenNoRewardedAdProp.boolValue );
            DrawSeparator();
            OnLoadingModeGUI();
            OnIOSLocationUsageDescriptionGUI();
            EditorGUILayout.PropertyField( debugModeProp );
            EditorGUILayout.PropertyField( analyticsCollectionEnabledProp );
            OnEditroRuntimeActiveAdGUI();

            DrawSeparator();
            OnAudienceGUI();
            DeprecatedDependenciesGUI();

            if (dependencyManager == null)
            {
                EditorGUILayout.HelpBox( "The integrity of CAS Unity package is broken. " +
                    "Please try to reimport the package or contact support.", MessageType.Error );
            }
            else
            {
                dependencyManager.OnGUI( platform );
                OnEDMAreaGUI();
            }

            OnAppAdsTxtGUI();
            GUILayout.FlexibleSpace();
            obj.ApplyModifiedProperties();
        }

        private void DrawSeparator()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        private void OnAppAdsTxtGUI()
        {
            EditorGUILayout.Space();
            var content = HelpStyles.GetContent( " Don’t forget to implement app-ads.txt", HelpStyles.helpIconContent.image );
            if (GUILayout.Button( content, EditorStyles.label ))
                Application.OpenURL( Utils.gitAppAdsTxtRepoUrl );
        }

        private void OnEDMAreaGUI()
        {
            if (edmExist)
            {
                if (platform == BuildTarget.Android)
                {
#if UNITY_2019_3_OR_NEWER
                    if (!File.Exists( Utils.projectGradlePath ))
                        EditorGUILayout.HelpBox( "Please enable 'Custom Base Gradle Template' found under " +
                        "'Player Settings -> Settings for Android -> Publishing Settings' menu " +
                        "to allow CAS update Grdale plugin version.", MessageType.Error );

                    if (!File.Exists( Utils.launcherGradlePath ))
                        EditorGUILayout.HelpBox( "Please enable 'Custom Launcher Gradle Template' found under " +
                        "'Player Settings -> Settings for Android -> Publishing Settings' menu " +
                        "to allow CAS use MultiDEX.", MessageType.Warning );
#else
                    if (!File.Exists( Utils.mainGradlePath ))
                    {
                        EditorGUILayout.HelpBox( "Please enable 'Custom Gradle Template' found under " +
                        "'Player Settings -> Settings for Android -> Publishing Settings' menu " +
                        "to allow CAS update Grdale plugin version and enable MultiDEX.", MessageType.Error );
                    }
#endif

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox( "Changing dependencies will change the project settings. " +
                        "Please use Android Resolver after the change complete.", MessageType.Info );
                    if (GUILayout.Button( "Resolve", GUILayout.ExpandWidth( false ), GUILayout.ExpandHeight( true ) ))
                    {
#if UNITY_ANDROID
                        var succses = Utils.TryResolveAndroidDependencies();
                        EditorUtility.DisplayDialog( "Android Dependencies",
                            succses ? "Resolution Succeeded" : "Resolution Failed. See the log for details.",
                            "OK" );
#else
                        EditorUtility.DisplayDialog( "Android Dependencies",
                            "Android resolver not enabled. Unity Android platform target must be selected.",
                            "OK" );
#endif
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                HelpStyles.BeginBoxScope();
                EditorGUILayout.HelpBox( "In order to properly include third party dependencies in your project, " +
                    "an External Dependency Manager is required.", MessageType.Error );
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label( "1. Download latest EDM4U.unitypackage", GUILayout.ExpandWidth( false ) );
                if (GUILayout.Button( "here", EditorStyles.miniButton, GUILayout.ExpandWidth( false ) ))
                {
                    Application.OpenURL( "https://github.com/googlesamples/unity-jar-resolver/releases" );
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Label( "2. Import the EDM4U.unitypackage into your project." );
                HelpStyles.EndBoxScope();
            }
        }

        private void OnManagerIDVerificationGUI()
        {
            if (managerIdsProp.arraySize == 0)
                EditorGUILayout.HelpBox( "Build is not supported without a manager ID.", MessageType.Error );
            else if (string.IsNullOrEmpty( managerIdsProp.GetArrayElementAtIndex( 0 ).stringValue ))
                EditorGUILayout.HelpBox( "The ID of the first manager cannot be empty!", MessageType.Error );
            else
                return;
            EditorGUILayout.HelpBox( "If you haven't created an CAS account and registered an manager yet, " +
                "now's a great time to do so at cleveradssolutions.com. " +
                "If you're just looking to experiment with the SDK, though, you can use the Test Ad Mode below with any manager ID.", MessageType.Info );
        }

        private void OnAudienceGUI()
        {
            var targetAudience = ( Audience )EditorGUILayout.EnumPopup( "Audience Tagged",
                ( Audience )audienceTaggedProp.enumValueIndex );
            if (audienceTaggedProp.enumValueIndex != ( int )targetAudience)
            {
                if (dependencyManager != null)
                    dependencyManager.SetAudience( targetAudience );
                audienceTaggedProp.enumValueIndex = ( int )targetAudience;
            }

            EditorGUI.indentLevel++;
            switch (targetAudience)
            {
                case Audience.Mixed:
                    EditorGUILayout.HelpBox( "This app is aimed at audiences of all ages and will have some restrictions for children.",
                        MessageType.None );
                    break;
                case Audience.Children:
                    EditorGUILayout.HelpBox( "Children restrictions and Families Ads Program participation apply to this app. Audience under 12 years old.",
                        MessageType.None );
                    break;
                case Audience.NotChildren:
                    EditorGUILayout.HelpBox( "Audience over 12 years old only.", MessageType.None );
                    break;
            }
            EditorGUI.indentLevel--;
        }

        private void OnIOSLocationUsageDescriptionGUI()
        {
            if (platform != BuildTarget.iOS)
                return;

            EditorGUILayout.PropertyField( trackLocationEnabledProp );
            if (trackLocationEnabledProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox( "Location data will be collected if the user allowed the app to track the location.", MessageType.None );
                EditorGUI.indentLevel--;
            }
        }

        private void OnBannerSizeGUI()
        {
            EditorGUILayout.PropertyField( bannerSizeProp );
            EditorGUI.indentLevel++;
            switch (( AdSize )bannerSizeProp.intValue)
            {
                case AdSize.Banner:
                    EditorGUILayout.HelpBox( "Current size: 320:50", MessageType.None );
                    break;
                case AdSize.AdaptiveBanner:
                    EditorGUILayout.HelpBox( "Pick the best ad size in full width screen for improved performance.", MessageType.None );
                    break;
                case AdSize.SmartBanner:
                    EditorGUILayout.HelpBox( "Typically, Smart Banners on phones have a Banner size. " +
                        "Or on tablets a Leaderboard size", MessageType.None );
                    break;
                case AdSize.Leaderboard:
                    EditorGUILayout.HelpBox( "Current size: 728:90", MessageType.None );
                    break;
                case AdSize.MediumRectangle:
                    EditorGUILayout.HelpBox( "Current size: 300:250", MessageType.None );
                    break;
            }
            EditorGUI.indentLevel--;
        }

        private void OnLoadingModeGUI()
        {
            var loadingMode = ( LoadingManagerMode )EditorGUILayout.EnumPopup( "Loading mode",
                    ( LoadingManagerMode )loadingModeProp.enumValueIndex );
            loadingModeProp.enumValueIndex = ( int )loadingMode;
        }

        private void OnEditroRuntimeActiveAdGUI()
        {
            if (editorRuntimeActiveAdFlags > -1)
            {
                DrawSeparator();
                EditorGUI.BeginChangeCheck();
                editorRuntimeActiveAdFlags = Convert.ToInt32(
                    EditorGUILayout.EnumFlagsField( "Editor runtime Active ad", ( AdFlags )editorRuntimeActiveAdFlags ) );
                if (EditorGUI.EndChangeCheck())
                    PlayerPrefs.SetInt( Utils.editorRuntimeActiveAdPrefs, editorRuntimeActiveAdFlags );
            }
        }

        private void DeprecatedDependenciesGUI()
        {
            if (deprecateDependenciesExist)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox( "Deprecated dependencies found. " +
                    "Please remove them and use the new dependencies below.", MessageType.Error );
                if (GUILayout.Button( "Remove", GUILayout.ExpandWidth( false ), GUILayout.Height( 40 ) ))
                {
                    for (int i = 0; i < deprecatedAssets.Length; i++)
                    {
                        var assets = AssetDatabase.FindAssets( deprecatedAssets[i] );
                        for (int assetI = 0; assetI < assets.Length; assetI++)
                            AssetDatabase.MoveAssetToTrash( AssetDatabase.GUIDToAssetPath( assets[assetI] ) );
                    }
                    deprecateDependenciesExist = false;
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
#pragma warning restore 649