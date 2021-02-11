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
        private SerializedProperty trackingUsageDescriptionProp;
        private SerializedProperty trackLocationEnabledProp;
        private SerializedProperty analyticsCollectionEnabledProp;
        private SerializedProperty interWhenNoRewardedAdProp;

        private DependencyManager dependencyManager;

        private ReorderableList managerIdsList;
        private BuildTarget platform;
        private bool allowedPackageUpdate;
        private string newCASVersion = null;
        private bool deprecateDependenciesExist;
        //private bool usingMultidexOnBuild;

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
            trackingUsageDescriptionProp = props.FindProperty( "trackingUsageDescription" );
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

            deprecateDependenciesExist = Utils.IsDeprecateDependencyExists( Utils.generalDeprecateDependency, platform )
                || Utils.IsDeprecateDependencyExists( Utils.teenDeprecateDependency, platform )
                || Utils.IsDeprecateDependencyExists( Utils.promoDeprecateDependency, platform );

            managerIdsList = new ReorderableList( props, managerIdsProp, true, true, true, true )
            {
                drawHeaderCallback = DrawListHeader,
                drawElementCallback = DrawListElement
            };

            allowedPackageUpdate = Utils.IsPackageExist( Utils.packageName );
            if (managerIdsProp.arraySize == 0)
            {
                if (platform == BuildTarget.Android)
                {
                    managerIdsProp.arraySize = 1;
                    managerIdsProp.GetArrayElementAtIndex( 0 )
                                  .stringValue = PlayerSettings.GetApplicationIdentifier( BuildTargetGroup.Android );
                }
                else if (platform == BuildTarget.iOS)
                {
                    managerIdsProp.arraySize = 1;
                    interstitialIntervalProp.intValue = 90;
                }
            }

            //usingMultidexOnBuild = PlayerPrefs.GetInt( Utils.editorIgnoreMultidexPrefs, 0 ) == 0;

            dependencyManager = DependencyManager.Create( platform, ( Audience )audienceTaggedProp.enumValueIndex, true );

            EditorApplication.delayCall += () => newCASVersion = Utils.GetNewVersionOrNull( Utils.gitUnityRepo, MobileAds.wrapperVersion, false );
            props.ApplyModifiedProperties();
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

        public override void OnInspectorGUI()
        {
            var obj = serializedObject;
            obj.UpdateIfRequiredOrScript();

            Utils.LinksToolbarGUI( Utils.gitUnityRepo );

            HelpStyles.BeginBoxScope();
            EditorGUILayout.PropertyField( testAdModeProp );
            EditorGUI.BeginDisabledGroup( testAdModeProp.boolValue );
            managerIdsList.DoLayoutList();
            OnManagerIDVerificationGUI();
            EditorGUI.EndDisabledGroup();
            allowedAdFlagsProp.intValue = Convert.ToInt32(
               EditorGUILayout.EnumFlagsField( "Allowed ads in app", ( AdFlags )allowedAdFlagsProp.intValue ) );

            GUILayout.Label( "These settings are required for initialization with: CAS.MobileAds.InitializeFromResources(0)",
                EditorStyles.wordWrappedMiniLabel, GUILayout.ExpandHeight( false ) );
            HelpStyles.EndBoxScope();

            DrawSeparator();
            OnAudienceGUI();
            OnIOSLocationUsageDescriptionGUI();

            DrawSeparator();
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

            EditorGUILayout.PropertyField( debugModeProp );
            EditorGUILayout.PropertyField( analyticsCollectionEnabledProp );
            OnEditroRuntimeActiveAdGUI();
            //if(usingMultidexOnBuild != EditorGUILayout.Toggle("Use MultiDex", usingMultidexOnBuild )){
            //    usingMultidexOnBuild = !usingMultidexOnBuild;
            //    if (usingMultidexOnBuild)
            //        PlayerPrefs.DeleteKey( Utils.editorIgnoreMultidexPrefs );
            //    else
            //        PlayerPrefs.SetInt( Utils.editorIgnoreMultidexPrefs, 1 );
            //}

            DrawSeparator();
            DeprecatedDependenciesGUI();
            Utils.AboutRepoGUI( Utils.gitUnityRepo, allowedPackageUpdate, MobileAds.wrapperVersion, ref newCASVersion );

            if (dependencyManager == null)
            {
                EditorGUILayout.HelpBox( "The integrity of CAS Unity package is broken. " +
                    "Please try to reimport the package or contact support.", MessageType.Error );
            }
            else
            {
                dependencyManager.OnGUI( platform );
            }

            obj.ApplyModifiedProperties();
        }

        private void DrawSeparator()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        private void OnManagerIDVerificationGUI()
        {
            if (testAdModeProp.boolValue)
                return;

            if (managerIdsProp.arraySize == 0)
                EditorGUILayout.HelpBox( "Build is not supported without a manager ID.", MessageType.Error );
            else if (string.IsNullOrEmpty( managerIdsProp.GetArrayElementAtIndex( 0 ).stringValue ))
                EditorGUILayout.HelpBox( "Manager ID cannot be empty.", MessageType.Error );
            else
                return;
            EditorGUILayout.HelpBox( "If you haven't created an CAS account and registered an manager yet, " +
                "now's a great time to do so at cleveradssolutions.com. " +
                "If you're just looking to experiment with the SDK, though, you can use the Test Ad Mode above.", MessageType.Info );
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
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField( "Tracking Usage Description:" );
            if (GUILayout.Button( "Default", EditorStyles.miniButton, GUILayout.ExpandWidth( false ) ))
                trackingUsageDescriptionProp.stringValue = Utils.locationUsageDefaultDescription;
            if (GUILayout.Button( "Info", EditorStyles.miniButton, GUILayout.ExpandWidth( false ) ))
                Application.OpenURL( Utils.configuringPrivacyURL );
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel++;
            trackingUsageDescriptionProp.stringValue =
                EditorGUILayout.TextArea( trackingUsageDescriptionProp.stringValue, HelpStyles.wordWrapTextAred );
            EditorGUILayout.HelpBox( "NSUserTrackingUsageDescription key with a custom message describing your usage location tracking to AppTrackingTransparency.Request(). Can be empty if not using location tracking", MessageType.None );

            EditorGUI.indentLevel--;
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
                    var generalpath = Utils.GetDeprecatedDependencyPath( Utils.generalDeprecateDependency, platform );
                    if (File.Exists( generalpath ))
                        AssetDatabase.MoveAssetToTrash( generalpath );
                    var teenPath = Utils.GetDeprecatedDependencyPath( Utils.teenDeprecateDependency, platform );
                    if (File.Exists( teenPath ))
                        AssetDatabase.MoveAssetToTrash( teenPath );
                    var promoPath = Utils.GetDeprecatedDependencyPath( Utils.promoDeprecateDependency, platform );
                    if (File.Exists( promoPath ))
                        AssetDatabase.MoveAssetToTrash( promoPath );
                    deprecateDependenciesExist = false;
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
#pragma warning restore 649