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

        private DependencyManager dependencyManager;

        private ReorderableList managerIdsList;
        private BuildTarget platform;
        private bool allowedPackageUpdate;
        private string newCASVersion;
        private bool deprecateDependenciesExist;

        private int editorRuntimeActiveAdFlags;
        private GUIStyle boxScopeStyle = null;
        private GUIStyle wordWrapTextAred = null;

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
            newCASVersion = Utils.GetNewVersionOrNull( Utils.gitUnityRepo, MobileAds.wrapperVersion, false );

            dependencyManager = DependencyManager.Create( platform, ( Audience )audienceTaggedProp.enumValueIndex, true );
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

            if (wordWrapTextAred == null)
            {
                wordWrapTextAred = new GUIStyle( EditorStyles.textArea );
                wordWrapTextAred.wordWrap = true;
            }

            LinksToolbarGUI();

            BeginBoxScope();
            EditorGUILayout.PropertyField( testAdModeProp );
            EditorGUI.BeginDisabledGroup( testAdModeProp.boolValue );
            managerIdsList.DoLayoutList();
            OnManagerIDVerificationGUI();
            EditorGUI.EndDisabledGroup();
            allowedAdFlagsProp.intValue = Convert.ToInt32(
               EditorGUILayout.EnumFlagsField( "Allowed ads in app", ( AdFlags )allowedAdFlagsProp.intValue ) );

            GUILayout.Label( "These settings are required for initialization with: CAS.MobileAds.InitializeFromResources(0)",
                EditorStyles.wordWrappedMiniLabel, GUILayout.ExpandHeight( false ) );
            EndBoxScope();

            DrawSeparator();
            OnAudienceGUI();
            OnIOSLocationUsageDescriptionGUI();

            DrawSeparator();
            OnBannerSizeGUI();
            bannerRefreshProp.intValue = Mathf.Clamp(
                 EditorGUILayout.IntField( "Banner refresh rate(sec):", bannerRefreshProp.intValue ), 10, short.MaxValue );

            DrawSeparator();
            interstitialIntervalProp.intValue = Math.Max( 0,
                EditorGUILayout.IntField( "Interstitial impression interval(sec):", interstitialIntervalProp.intValue ) );

            DrawSeparator();
            OnLoadingModeGUI();

            EditorGUILayout.PropertyField( debugModeProp );
            OnEditroRuntimeActiveAdGUI();

            DrawSeparator();
            DeprecatedDependenciesGUI();
            OnCASAboutGUI();

            if (dependencyManager == null)
            {
                EditorGUILayout.HelpBox( "The integrity of CAS Unity package is broken. " +
                    "Please try to reimport the package or contact support.", MessageType.Error );
            }
            else
            {
                BeginBoxScope();
                dependencyManager.OnGUI( platform );
                EndBoxScope();
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
                EditorGUILayout.TextArea( trackingUsageDescriptionProp.stringValue, wordWrapTextAred );
            EditorGUILayout.HelpBox( "NSUserTrackingUsageDescription key with a custom message describing your usage. Can be empty.", MessageType.None );

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

        private void OnCASAboutGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label( "CAS Unity wrapper version: " + MobileAds.wrapperVersion );
            if (GUILayout.Button( "Check for Updates", EditorStyles.miniButton, GUILayout.ExpandWidth( false ) ))
            {
                newCASVersion = Utils.GetNewVersionOrNull( Utils.gitUnityRepo, MobileAds.wrapperVersion, true );
                string message = string.IsNullOrEmpty( newCASVersion ) ? "You are using the latest version."
                    : "There is a new version " + newCASVersion + " of the CAS Unity available for update.";
                EditorUtility.DisplayDialog( "Check for Updates", message, "OK" );
            }
            EditorGUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty( newCASVersion ))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox( "There is a new version " + newCASVersion + " of the CAS Unity available for update.", MessageType.Warning );
                var layoutParams = new[] { GUILayout.Height( 38 ), GUILayout.ExpandWidth( false ) };
#if UNITY_2018_4_OR_NEWER
                if (allowedPackageUpdate && GUILayout.Button( "Update", layoutParams ))
                {
                    var request = UnityEditor.PackageManager.Client.Add( Utils.gitUnityRepoURL + ".git#" + newCASVersion );
                    try
                    {
                        while (!request.IsCompleted)
                        {
                            if (EditorUtility.DisplayCancelableProgressBar(
                                "Update Package Manager dependency", "Clever Ads Solutions " + newCASVersion, 0.5f ))
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
                    Application.OpenURL( Utils.gitUnityRepoURL + "/releases" );
                EditorGUILayout.EndHorizontal();
            }

        }

        private void LinksToolbarGUI()
        {
            EditorGUILayout.BeginHorizontal( EditorStyles.toolbar );
            if (GUILayout.Button( "Support", EditorStyles.toolbarButton, GUILayout.ExpandWidth( false ) ))
                Application.OpenURL( Utils.supportURL );
            if (GUILayout.Button( "GitHub", EditorStyles.toolbarButton, GUILayout.ExpandWidth( false ) ))
                Application.OpenURL( Utils.gitUnityRepoURL );
            if (GUILayout.Button( "CleverAdsSolutions.com", EditorStyles.toolbarButton ))
                Application.OpenURL( Utils.websiteURL );
            EditorGUILayout.EndHorizontal();
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

        private void BeginBoxScope()
        {
            if (boxScopeStyle == null)
            {
                boxScopeStyle = new GUIStyle( EditorStyles.helpBox );
                var p = boxScopeStyle.padding;
                p.right += 3;
                p.left += 3;
            }
            EditorGUILayout.BeginVertical( boxScopeStyle );
        }

        private void EndBoxScope()
        {
            EditorGUILayout.EndVertical();
        }
    }
}
#pragma warning restore 649