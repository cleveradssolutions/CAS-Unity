#pragma warning disable 649
using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Utils = CAS.UEditor.CASEditorUtils;
using System.IO;
using System.Reflection;
using System.Text;

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

        private SerializedObject editorSettingsObj;
        private SerializedProperty autoCheckForUpdatesEnabledProp;
        private SerializedProperty delayAppMeasurementGADInitProp;
        private SerializedProperty multiDexEnabledProp;
        private SerializedProperty permissionAdIdRemovedProp;
        private SerializedProperty mostPopularCountryOfUsersProp;
        private SerializedProperty attributionReportEndpointProp;
        private SerializedProperty userTrackingUsageDescriptionProp;

        private DependencyManager dependencyManager;

        private ReorderableList managerIdsList;
        private ReorderableList userTrackingList;
        private BuildTarget platform;
        private bool allowedPackageUpdate;
        private string newCASVersion = null;
        private bool deprecateDependenciesExist;
        private Version edmVersion;
        private PropertyInfo edmIOSStaticLinkProp = null;
        private string environmentDetails;

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
                onCanRemoveCallback = DisabledRemoveLastItemFromList,
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

            // Remove deprecated CAS settings raw data
            if (File.Exists( Utils.androidResSettingsPath + ".json" ))
                AssetDatabase.MoveAssetToTrash( Utils.androidResSettingsPath + ".json" );

            edmVersion = Utils.GetEDM4UVersion( platform );
            if (edmVersion != null && edmVersion < Utils.minEDM4UVersion)
                edmVersion = null;
            try
            {
                if (platform == BuildTarget.iOS)
                    edmIOSStaticLinkProp = Type.GetType( "Google.IOSResolver, Google.IOSResolver", true )
                        .GetProperty( "PodfileStaticLinkFrameworks", BindingFlags.Public | BindingFlags.Static );
            }
            catch
            {
                edmIOSStaticLinkProp = null;
            }


            var environmentBuilder = new StringBuilder( "Environment Details: " )
                .Append( "Unity - " ).Append( Application.unityVersion ).Append( "; " )
                .Append( "Platform - " ).Append( Application.platform ).Append( "; " );
            if (edmVersion != null)
                environmentBuilder.Append( "EDM4U - " ).Append( edmVersion ).Append( "; " );
            environmentDetails = environmentBuilder.ToString();


            editorSettingsObj = new SerializedObject( CASEditorSettings.Load( true ) );
            autoCheckForUpdatesEnabledProp = editorSettingsObj.FindProperty( "autoCheckForUpdatesEnabled" );
            delayAppMeasurementGADInitProp = editorSettingsObj.FindProperty( "delayAppMeasurementGADInit" );
            multiDexEnabledProp = editorSettingsObj.FindProperty( "multiDexEnabled" );
            permissionAdIdRemovedProp = editorSettingsObj.FindProperty( "permissionAdIdRemoved" );

            mostPopularCountryOfUsersProp = editorSettingsObj.FindProperty( "mostPopularCountryOfUsers" );
            attributionReportEndpointProp = editorSettingsObj.FindProperty( "attributionReportEndpoint" );

            userTrackingUsageDescriptionProp = editorSettingsObj.FindProperty( "userTrackingUsageDescription" );

            userTrackingList = new ReorderableList( editorSettingsObj, userTrackingUsageDescriptionProp, true, true, true, true )
            {
                drawHeaderCallback = DrawNSTrackingListHeader,
                drawElementCallback = DrawNSTrackingListElement,
                onCanRemoveCallback = DisabledRemoveLastItemFromList,
            };
        }

        private void DrawListHeader( Rect rect )
        {
            EditorGUI.LabelField( rect, "Manager ID's " + ( platform == BuildTarget.iOS ? "(iTunes ID)" : "(Bundle ID)" ) );
        }

        private void DrawListElement( Rect rect, int index, bool isActive, bool isFocused )
        {
            var item = managerIdsProp.GetArrayElementAtIndex( index );
            rect.yMin += 1;
            rect.yMax -= 1;
            item.stringValue = EditorGUI.TextField( rect, item.stringValue );
        }

        private bool DisabledRemoveLastItemFromList( ReorderableList list )
        {
            return list.count > 1;
        }

        private void DrawNSTrackingListHeader( Rect rect )
        {
            EditorGUI.LabelField( rect, "(ISO-639) : NSUserTrackingUsageDescription" );
        }

        private void DrawNSTrackingListElement( Rect rect, int index, bool isActive, bool isFocused )
        {
            var item = userTrackingUsageDescriptionProp.GetArrayElementAtIndex( index );
            rect.yMin += 1;
            rect.yMax -= 1;
            var maxX = rect.xMax;
            rect.xMax = rect.xMin + 40;

            item.Next( true );
            var langCode = item.stringValue;
            EditorGUI.BeginChangeCheck();
            langCode = EditorGUI.TextField( rect, langCode );
            if (EditorGUI.EndChangeCheck())
            {
                if (langCode.Length > 2)
                    langCode = langCode.Substring( 0, 2 );
                item.stringValue = langCode.ToLower();
            }
            rect.xMin = rect.xMax + 5;
            rect.xMax = maxX;
            item.Next( false );
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
            serializedObject.Update();
            editorSettingsObj.Update();

            if (managerIdsList.count > 1)
            {
                var appId = managerIdsProp.GetArrayElementAtIndex( 0 ).stringValue;
                if (!string.IsNullOrEmpty( appId ))
                    EditorGUILayout.LabelField( "Application id in CAS:", appId );
            }
            managerIdsList.DoLayoutList();
            OnManagerIDVerificationGUI();
            DrawTestAdMode();
            DrawSeparator();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var mediumFlag = ( int )AdFlags.MediumRectangle;
            EditorGUI.BeginDisabledGroup( ( allowedAdFlagsProp.intValue & mediumFlag ) == mediumFlag );
            var banner = DrawAdFlagToggle( AdFlags.Banner );
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup( !banner );
            DrawAdFlagToggle( AdFlags.MediumRectangle );
            EditorGUI.EndDisabledGroup();
            var inter = DrawAdFlagToggle( AdFlags.Interstitial );
            var reward = DrawAdFlagToggle( AdFlags.Rewarded );
            DrawAdFlagToggle( AdFlags.Native );
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            if (banner)
                DrawBannerScope();
            if (inter)
                DrawInterstitialScope();
            if (reward)
                DrawRewardedScope( inter );

            IsAdFormatsNotUsed();
            DrawSeparator();
            OnEditroRuntimeActiveAdGUI();
            OnLoadingModeGUI();
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
            DrawSeparator();

            OnUserTrackingGUI();
            OnIOSLocationUsageDescriptionGUI();
            OnEditorEnvirementGUI();
            editorSettingsObj.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnUserTrackingGUI()
        {
            if (platform != BuildTarget.iOS)
                return;
            var enabled = userTrackingUsageDescriptionProp.arraySize > 0;
            EditorGUILayout.BeginHorizontal();
            if (enabled != EditorGUILayout.ToggleLeft( "Set User Tracking Usage description", enabled ))
            {
                enabled = !enabled;
                if (enabled)
                {
                    var defDescr = Utils.DefaultUserTrackingUsageDescription();
                    userTrackingUsageDescriptionProp.arraySize = defDescr.Length;
                    for (int i = 0; i < defDescr.Length; i++)
                    {
                        var pair = userTrackingUsageDescriptionProp.GetArrayElementAtIndex( i );
                        pair.Next( true );
                        pair.stringValue = defDescr[i].key;
                        pair.Next( false );
                        pair.stringValue = defDescr[i].value;
                    }
                }
                else
                {
                    userTrackingUsageDescriptionProp.ClearArray();
                }
            }
            HelpStyles.HelpButton( Utils.gitUnityRepoURL + "/wiki/App-Tracking-Transparency" );
            EditorGUILayout.EndHorizontal();
            if (enabled)
                userTrackingList.DoLayoutList();
        }

        private void OnEditorEnvirementGUI()
        {
            analyticsCollectionEnabledProp.boolValue = EditorGUILayout.ToggleLeft( HelpStyles.GetContent( "Impression Analytics collection (Firebase)", null,
                "If your application uses Google Analytics(Firebase) then CAS collects ad impressions and states to analytic.\n" +
                "Disabling analytics collection may save internet traffic and improve application performance.\n" +
                "The Analytics collection has no effect on ad revenue." ), analyticsCollectionEnabledProp.boolValue );
            debugModeProp.boolValue = EditorGUILayout.ToggleLeft( HelpStyles.GetContent( "Verbose Debug logging", null,
                "The enabled Debug Mode will display a lot of useful information for debugging about the states of the sdk with tag CAS. " +
                "Disabling the Debug Mode may improve application performance." ), debugModeProp.boolValue );

            if (platform == BuildTarget.Android)
            {
                permissionAdIdRemovedProp.boolValue = EditorGUILayout.ToggleLeft(
                    "Remove permission to use Advertising ID (AD_ID)",
                    permissionAdIdRemovedProp.boolValue );

                EditorGUILayout.BeginHorizontal();
                multiDexEnabledProp.boolValue = EditorGUILayout.ToggleLeft(
                    "Multi DEX enabled",
                    multiDexEnabledProp.boolValue );
                HelpStyles.HelpButton( Utils.gitUnityRepoURL + "/wiki/Include-Android#enable-multidex" );
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                var reportEndpointEnabled = attributionReportEndpointProp.stringValue.Length > 0;
                if (reportEndpointEnabled != EditorGUILayout.ToggleLeft(
                    "Set Attribution Report endpoint", reportEndpointEnabled ))
                {
                    reportEndpointEnabled = !reportEndpointEnabled;
                    if (reportEndpointEnabled)
                        attributionReportEndpointProp.stringValue = Utils.attributionReportEndPoint;
                    else
                        attributionReportEndpointProp.stringValue = string.Empty;
                }
                HelpStyles.HelpButton( Utils.gitUnityRepoURL + "/wiki/Include-iOS#ios-15-global-skadnetwork-reporting" );
                EditorGUILayout.EndHorizontal();

                if (reportEndpointEnabled)
                {
                    EditorGUI.indentLevel++;
                    attributionReportEndpointProp.stringValue = EditorGUILayout.TextField(
                        attributionReportEndpointProp.stringValue );
                    EditorGUI.indentLevel--;
                }
            }
            delayAppMeasurementGADInitProp.boolValue = EditorGUILayout.ToggleLeft(
                    "Delay measurement of the Google SDK initialization",
                    delayAppMeasurementGADInitProp.boolValue );
            autoCheckForUpdatesEnabledProp.boolValue = EditorGUILayout.ToggleLeft(
                "Auto check for CAS updates enabled",
                autoCheckForUpdatesEnabledProp.boolValue );

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label( "Most popular country of users (ISO2)", GUILayout.ExpandWidth( false ) );
            EditorGUI.BeginChangeCheck();
            var countryCode = mostPopularCountryOfUsersProp.stringValue;
            countryCode = EditorGUILayout.TextField( countryCode, GUILayout.Width( 25.0f ) );
            if (EditorGUI.EndChangeCheck())
            {
                if (countryCode.Length > 2)
                    countryCode = countryCode.Substring( 0, 2 );
                mostPopularCountryOfUsersProp.stringValue = countryCode.ToUpper();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox( environmentDetails, MessageType.None );
        }

        private bool IsAdFormatsNotUsed()
        {
            if (allowedAdFlagsProp.intValue == 0 || allowedAdFlagsProp.intValue == ( int )AdFlags.Native)
            {
                EditorGUILayout.HelpBox( "Please activate the ad formats that you want to use in your game.", MessageType.Error );
                return true;
            }
            return false;
        }

        private void DrawTestAdMode()
        {
            EditorGUILayout.PropertyField( testAdModeProp );
            EditorGUI.indentLevel++;
            if (testAdModeProp.boolValue)
                EditorGUILayout.HelpBox( "Make sure you disable test ad mode and replace test manager ID with your own ad manager ID before publishing your app!", MessageType.Warning );
            else if (EditorUserBuildSettings.development)
                EditorGUILayout.HelpBox( "Development build enabled, only test ads are allowed. " +
                    "\nMake sure you disable Development build and use real ad manager ID before publishing your app!", MessageType.Warning );
            else
                EditorGUILayout.HelpBox( "When testing your app, make sure you use Test Ads mode or Development Build. " +
                "Failure to do so can lead to suspension of your account.", MessageType.None );
            EditorGUI.indentLevel--;
        }

        private void DrawInterstitialScope()
        {
            EditorGUILayout.LabelField( "Interstitial ads:" );
            EditorGUI.indentLevel++;
            interstitialIntervalProp.intValue = Math.Max( 0,
            EditorGUILayout.IntField( "Impression interval(sec)", interstitialIntervalProp.intValue ) );
            EditorGUI.indentLevel--;
        }

        private void DrawRewardedScope( bool allowInter )
        {
            EditorGUILayout.LabelField( "Rewarded ads:" );
            EditorGUI.indentLevel++;
            EditorGUI.BeginDisabledGroup( !allowInter );
            interWhenNoRewardedAdProp.boolValue = EditorGUILayout.ToggleLeft(
               HelpStyles.GetContent( "Increase filling by Interstitial ads", null,
               "Sometimes a situation occurs when filling Rewarded ads is not enough, " +
               "in this case, you can allow the display of Interstitial ads to receiving a reward in any case." ),
                allowInter && interWhenNoRewardedAdProp.boolValue );
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;
        }

        private void DrawBannerScope()
        {
            EditorGUILayout.LabelField( "Ad View:" );
            EditorGUI.indentLevel++;
            bannerRefreshProp.intValue = Mathf.Clamp(
                  EditorGUILayout.IntField( "Refresh rate(sec)", bannerRefreshProp.intValue ), 0, short.MaxValue );

            var obsoleteAPI = bannerSizeProp.intValue > 0;
            if (obsoleteAPI != EditorGUILayout.Toggle( "Use Single Banner(Obsolete)", obsoleteAPI ))
            {
                obsoleteAPI = !obsoleteAPI;
                bannerSizeProp.intValue = obsoleteAPI ? 1 : 0;
            }
            if (obsoleteAPI)
            {
                EditorGUILayout.PropertyField( bannerSizeProp, HelpStyles.GetContent( "Single Ad size", null ) );
                //if (settings.bannerSize == AdSize.Banner && Utils.IsPortraitOrientation())
                //{
                //    DialogOrCancel( "For portrait applications, we recommend using the adaptive banner size." +
                //            "This will allow you to get more expensive advertising.", target );
                //}
                EditorGUI.indentLevel++;
                switch (( AdSize )bannerSizeProp.intValue)
                {
                    case AdSize.Banner:
                        EditorGUILayout.HelpBox( "Size in DPI: 320:50", MessageType.None );
                        break;
                    case AdSize.AdaptiveBanner:
                        EditorGUILayout.HelpBox( "Pick the best ad size in full width screen for improved performance.", MessageType.None );
                        break;
                    case AdSize.SmartBanner:
                        EditorGUILayout.HelpBox( "Typically, Smart Banners on phones have a Banner size. " +
                            "Or on tablets a Leaderboard size", MessageType.None );
                        break;
                    case AdSize.Leaderboard:
                        EditorGUILayout.HelpBox( "Size in DPI: 728:90", MessageType.None );
                        break;
                    case AdSize.MediumRectangle:
                        EditorGUILayout.HelpBox( "Size in DPI: 300:250", MessageType.None );
                        var enableMrec = allowedAdFlagsProp.intValue | ( int )AdFlags.MediumRectangle;
                        if (enableMrec != allowedAdFlagsProp.intValue)
                            allowedAdFlagsProp.intValue = enableMrec;
                        break;
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }

        private bool DrawAdFlagToggle( AdFlags flag )
        {
            var flagInt = ( int )flag;
            var enabled = ( allowedAdFlagsProp.intValue & flagInt ) == flagInt;
            var icon = HelpStyles.GetFormatIcon( flag, enabled );
            var content = HelpStyles.GetContent( "", icon, "Use " + flag.ToString() + " placement" );

            EditorGUI.BeginDisabledGroup( flag == AdFlags.Native );
            if (flag == AdFlags.Native)
            {
                if (enabled)
                {
                    allowedAdFlagsProp.intValue = allowedAdFlagsProp.intValue ^ flagInt;
                    enabled = false;
                }
                content.tooltip = "Native ads coming soon";
            }
            if (icon == null)
            {
                content.text = content.tooltip;
                content.tooltip = "";
            }
            if (enabled != GUILayout.Toggle( enabled, content, "button", GUILayout.ExpandWidth( false ), GUILayout.MinWidth( 45 ) ))
            {
                enabled = !enabled;
                if (enabled)
                    allowedAdFlagsProp.intValue = allowedAdFlagsProp.intValue | flagInt;
                else
                    allowedAdFlagsProp.intValue = allowedAdFlagsProp.intValue ^ flagInt;
            }
            EditorGUI.EndDisabledGroup();
            return enabled;
        }

        private void DrawSeparator()
        {
            EditorGUILayout.Space();
        }

        private void OnAppAdsTxtGUI()
        {
            EditorGUILayout.Space();
            var content = HelpStyles.GetContent( " Don’t forget to implement app-ads.txt", HelpStyles.helpIconContent.image );
            if (GUILayout.Button( content, EditorStyles.label, GUILayout.ExpandWidth( false ) ))
                Application.OpenURL( Utils.gitAppAdsTxtRepoUrl );
        }

        private void OnEDMAreaGUI()
        {
            if (edmVersion == null)
            {
                HelpStyles.BeginBoxScope();
                EditorGUILayout.HelpBox( "In order to properly include third party dependencies in your project, " +
                    "an External Dependency Manager is required.", MessageType.Error );
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label( "1. Download latest EDM4U.unitypackage", GUILayout.ExpandWidth( false ) );
                if (GUILayout.Button( "here", EditorStyles.miniButton, GUILayout.ExpandWidth( false ) ))
                    Application.OpenURL( "https://github.com/googlesamples/unity-jar-resolver/tags" );
                EditorGUILayout.EndHorizontal();
                GUILayout.Label( "2. Import the EDM4U.unitypackage into your project." );
                HelpStyles.EndBoxScope();
                return;
            }

            if (platform == BuildTarget.Android)
            {
#if UNITY_2019_3_OR_NEWER
                if (!File.Exists( Utils.projectGradlePath ))
                {
                    OnWarningGUI( "Custom Base Gradle Template disabled",
                        "Please enable 'Custom Base Gradle Template' found under " +
                        "'Player Settings -> Settings for Android -> Publishing Settings' menu " +
                        "to allow CAS update Grdale plugin version.",
                        MessageType.Error );
                }

                if (!File.Exists( Utils.launcherGradlePath ))
                {
                    OnWarningGUI( "Custom Launcher Gradle Template disabled",
                        "Please enable 'Custom Launcher Gradle Template' found under " +
                        "'Player Settings -> Settings for Android -> Publishing Settings' menu " +
                        "to allow CAS use MultiDEX.",
                        MessageType.Error );
                }

                if (!File.Exists( Utils.propertiesGradlePath ))
                {
                    OnWarningGUI( "Custom Gradle Properties Template disabled",
                        "Please enable 'Custom Gradle Properties Template' found under " +
                        "'Player Settings > Settings for Android -> Publishing Settings' menu " +
                        "to allow CAS use Jetifier.",
                        MessageType.Error );
                }
#else
                if (!File.Exists( Utils.mainGradlePath ))
                {
                    OnWarningGUI( "Custom Gradle Template disabled",
                        "Please enable 'Custom Gradle Template' found under " +
                        "'Player Settings -> Settings for Android -> Publishing Settings' menu " +
                        "to allow CAS update Grdale plugin version and enable MultiDEX.",
                        MessageType.Error );
                }
#endif

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox( "Changing dependencies will change the project settings. " +
                    "Please use Android Resolver after the change complete.", MessageType.Info );
                if (GUILayout.Button( "Resolve", GUILayout.ExpandWidth( false ), GUILayout.Height( 38 ) ))
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
                return;
            }
            if (platform == BuildTarget.iOS)
            {
                if (edmIOSStaticLinkProp != null && !( bool )edmIOSStaticLinkProp.GetValue( null, null ))
                {
                    OnWarningGUI( "Link frameworks statically disabled",
                        "Please enable 'Add use_frameworks!' and 'Link frameworks statically' found under " +
                        "'Assets -> External Dependency Manager -> iOS Resolver -> Settings' menu.\n" +
                        "Failing to do this step may result in undefined behavior of the plugin and doubled import of frameworks.",
                        MessageType.Warning );
                }

                if (PlayerSettings.muteOtherAudioSources)
                {
                    OnWarningGUI( "Mute Other AudioSources enabled in PlayerSettings",
                        "Known issue with muted all sounds in Unity Game after closing interstitial ads. " +
                        "We recommend not using 'Mute Other AudioSources'.",
                        MessageType.Warning );
                }
            }
        }

        private void OnWarningGUI( string title, string message, MessageType type )
        {
            HelpStyles.BeginBoxScope();
            EditorGUILayout.HelpBox( title, type );
            EditorGUILayout.LabelField( message, EditorStyles.wordWrappedLabel );
            HelpStyles.EndBoxScope();
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
                    EditorGUILayout.HelpBox( "The app is intended for audiences of all ages and complying with COPPA.",
                        MessageType.None );
                    break;
                case Audience.Children:
                    EditorGUILayout.HelpBox( "Children restrictions and Google Families Ads Program participation apply to this app. Audience under 12 years old.",
                        MessageType.None );
                    if (platform == BuildTarget.Android && !permissionAdIdRemovedProp.boolValue)
                    {
                        EditorGUILayout.HelpBox( "Families Policy require that apps not use the Ad ID. " +
                            "We recommend that you remove the permission using the checkbox below.",
                            MessageType.Warning );
                    }
                    break;
                case Audience.NotChildren:
                    EditorGUILayout.HelpBox( "Audience over 12 years old only. There are no restrictions on ad filling.", MessageType.None );
                    break;
            }
            EditorGUI.indentLevel--;
        }

        private void OnIOSLocationUsageDescriptionGUI()
        {
            if (platform != BuildTarget.iOS)
                return;

            trackLocationEnabledProp.boolValue = EditorGUILayout.ToggleLeft(
                "Location collection when user allowed", trackLocationEnabledProp.boolValue );
        }

        private void OnLoadingModeGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField( loadingModeProp, HelpStyles.GetContent( "Loading mode", null ) );
            HelpStyles.HelpButton( Utils.gitUnityRepoURL + "/wiki/Configuring-SDK#loading-mode" );
            EditorGUILayout.EndHorizontal();
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