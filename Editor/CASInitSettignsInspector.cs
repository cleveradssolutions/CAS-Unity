//#define UserDataPrivacySettings
using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Utils = CAS.UEditor.CASEditorUtils;

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
        private SerializedProperty locationUsageDescriptionProp;

        private ReorderableList managerIdsList;
        private BuildTarget platform;
        private bool promoDependencyExist;
        private bool teenDependencyExist;
        private bool generalDependencyExist;
        private bool reimportDependencyOnBuild;
        private bool allowedPackageUpdate;
        private string newCASVersion;

        private int editorRuntimeActiveAdFlags;
        private Vector2 mediationNetworkScroll;

        private readonly PartnerNetwork[] mediationPartners = new PartnerNetwork[]
        {
            new PartnerNetwork("SuperAwesome", "https://www.superawesome.com", Audience.Children, Audience.Mixed),
            new PartnerNetwork("FacebookAds", "https://www.facebook.com/business/marketing/audience-network", Audience.NotChildren, Audience.Mixed),
            new PartnerNetwork("YandexAds", "https://yandex.ru/dev/mobile-ads/", Audience.NotChildren, Audience.Mixed),
            new PartnerNetwork("GoogleAds", "https://admob.google.com/home/", Audience.Mixed, Audience.Mixed),
            new PartnerNetwork("AppLovin", "https://www.applovin.com", Audience.Mixed, Audience.Mixed),
            new PartnerNetwork("UnityAds", "https://unity.com/solutions/unity-ads", Audience.Mixed, Audience.Mixed),
            new PartnerNetwork("Vungle", "https://vungle.com", Audience.Mixed, Audience.Mixed),
            new PartnerNetwork("AdColony", "https://www.adcolony.com", Audience.Mixed, Audience.Mixed),
            new PartnerNetwork("IronSource", "https://www.ironsrc.com", Audience.Mixed, Audience.Mixed),
            new PartnerNetwork("Kidoz", "https://kidoz.net", Audience.Mixed, Audience.Mixed),
            new PartnerNetwork("InMobi", "https://www.inmobi.com", Audience.Mixed, Audience.Mixed),
            new PartnerNetwork("Chartboost", "https://www.chartboost.com", Audience.Mixed, Audience.Mixed),
            new PartnerNetwork("StartApp", "https://www.startapp.com", Audience.Mixed, Audience.Mixed)
        };

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
            locationUsageDescriptionProp = props.FindProperty( "locationUsageDescription" );

            editorRuntimeActiveAdFlags = PlayerPrefs.GetInt( Utils.editorRuntimeActiveAdPrefs, -1 );
            reimportDependencyOnBuild = PlayerPrefs.GetInt( Utils.editorReimportDepsOnBuildPrefs, 1 ) == 1;

            string assetName = target.name;
            if (assetName.EndsWith( BuildTarget.Android.ToString() ))
                platform = BuildTarget.Android;
            else if (assetName.EndsWith( BuildTarget.iOS.ToString() ))
                platform = BuildTarget.iOS;
            else
                platform = BuildTarget.NoTarget;

            generalDependencyExist = Utils.IsDependencyFileExists( Utils.generalTemplateDependency, platform );
            teenDependencyExist = Utils.IsDependencyFileExists( Utils.teenTemplateDependency, platform );
            promoDependencyExist = Utils.IsDependencyFileExists( Utils.promoTemplateDependency, platform );

            managerIdsList = new ReorderableList( props, managerIdsProp, true, true, true, true )
            {
                drawHeaderCallback = DrawListHeader,
                drawElementCallback = DrawListElement
            };

            allowedPackageUpdate = Utils.IsPackageExist( Utils.packageName );
            newCASVersion = Utils.GetNewVersionOrNull( Utils.gitUnityRepo, MobileAds.wrapperVersion, false );
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
            EditorGUILayout.PropertyField( testAdModeProp );
            EditorGUI.BeginDisabledGroup( testAdModeProp.boolValue );
            DrawSeparator();
            managerIdsList.DoLayoutList();
            OnManagerIDVerificationGUI();
            EditorGUI.EndDisabledGroup();

            allowedAdFlagsProp.intValue = Convert.ToInt32(
               EditorGUILayout.EnumFlagsField( "Allowed ads in app", ( AdFlags )allowedAdFlagsProp.intValue ) );

            DrawSeparator();
            bool isChildrenAudience = OnAudienceGUIActiveChildren();
            EditorGUI.BeginDisabledGroup( isChildrenAudience );
#if UserDataPrivacySettings
            OnUserConsentGUI();
            OnCCPAStatusGUI();
#endif
            EditorGUI.EndDisabledGroup();
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
            OnPromoDependenciesGUI();

            DrawSeparator();
            OnAllowedAdNetworksGUI();

            DrawSeparator();
            OnBuildSettingsGUI();

            DrawSeparator();
            OnCASAboutGUI();
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

#if UserDataPrivacySettings
        private void OnUserConsentGUI()
        {
            var consentStatus = ( ConsentStatus )EditorGUILayout.EnumPopup( "User Consent",
                    ( ConsentStatus )consentStatusProp.enumValueIndex );
            consentStatusProp.enumValueIndex = ( int )consentStatus;
            EditorGUI.indentLevel++;
            switch (consentStatus)
            {
                case ConsentStatus.Undefined:
                    EditorGUILayout.HelpBox( "Mediation ads network behavior.", MessageType.None );
                    break;
                case ConsentStatus.Accepted:
                    EditorGUILayout.HelpBox( "User consents to behavioral targeting in compliance with GDPR.", MessageType.None );
                    break;
                case ConsentStatus.Denied:
                    EditorGUILayout.HelpBox( "User does not consent to behavioral targeting in compliance with GDPR.", MessageType.None );
                    break;
            }
            EditorGUI.indentLevel--;
        }
#endif

        private bool OnAudienceGUIActiveChildren()
        {
            var targetAudience = ( Audience )EditorGUILayout.EnumPopup( "Audience Tagged",
                ( Audience )audienceTaggedProp.enumValueIndex );
            audienceTaggedProp.enumValueIndex = ( int )targetAudience;
            if (targetAudience == Audience.Children)
            {
#if UserDataPrivacySettings
                consentStatusProp.enumValueIndex = ( int )ConsentStatus.Denied;
                ccpaStatusProp.enumValueIndex = ( int )CCPAStatus.OptOutSale;
#endif
                EditorGUI.indentLevel++;
                if (!generalDependencyExist)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox( "Apps tagged for kids require General CAS Dependencies to be activated. " +
                        "Please use Android Resolver after the change.",
                        MessageType.Error );
                    if (GUILayout.Button( "Activate", GUILayout.Height( 38 ) ))
                        if (Utils.TryActivateDependencies( Utils.generalTemplateDependency, platform ))
                            generalDependencyExist = true;
                    EditorGUILayout.EndHorizontal();
                }
                if (teenDependencyExist)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox( "Teen CAS Dependencies not supported and should be deactivated. " +
                        "Please use Android Resolver after the change.",
                        MessageType.Error );
                    if (GUILayout.Button( "Deactivate", GUILayout.Height( 38 ) ))
                        if (DeactivateDependencies( Utils.teenTemplateDependency ))
                            teenDependencyExist = false;
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.HelpBox( "The children's audience tagged does not allow (GDPR, CCPA) requests " +
                    "for consent to the use of personal data. Also Families Ads Program allows using only certified ad networks.", MessageType.Info );
                EditorGUI.indentLevel--;
                return true;
            }
            else
            {
                if (generalDependencyExist)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox( "General CAS Dependencies not supported and should be deactivated. " +
                        "Please use Android Resolver after the change.",
                        MessageType.Error );
                    if (GUILayout.Button( "Deactivate", GUILayout.Height( 38 ) ))
                        if (DeactivateDependencies( Utils.generalTemplateDependency ))
                            generalDependencyExist = false;
                    EditorGUILayout.EndHorizontal();
                }
                if (!teenDependencyExist)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox( "Apps tagged for " + targetAudience.ToString() +
                        " require Teen CAS Dependencies to be activated. " +
                        "Please use Android Resolver after the change.", MessageType.Error );
                    if (GUILayout.Button( "Activate", GUILayout.Height( 38 ) ))
                        if (Utils.TryActivateDependencies( Utils.teenTemplateDependency, platform ))
                            teenDependencyExist = true;
                    EditorGUILayout.EndHorizontal();
                }
            }
            return false;
        }

#if UserDataPrivacySettings
        private void OnCCPAStatusGUI()
        {
            var ccpaStatus = ( CCPAStatus )EditorGUILayout.EnumPopup( "CCPA Status",
                    ( CCPAStatus )ccpaStatusProp.enumValueIndex );
            ccpaStatusProp.enumValueIndex = ( int )ccpaStatus;
            EditorGUI.indentLevel++;
            switch (ccpaStatus)
            {
                case CCPAStatus.Undefined:
                    EditorGUILayout.HelpBox( "Mediation ads network behavior.", MessageType.None );
                    break;
                case CCPAStatus.OptInSale:
                    EditorGUILayout.HelpBox( "User consents to the sale of his or her personal information in compliance with CCPA.", MessageType.None );
                    break;
                case CCPAStatus.OptOutSale:
                    EditorGUILayout.HelpBox( "User does not consent to the sale of his or her personal information in compliance with CCPA.", MessageType.None );
                    break;
            }
            EditorGUI.indentLevel--;
        }
#endif

        private void OnIOSLocationUsageDescriptionGUI()
        {
            if (platform != BuildTarget.iOS)
                return;
            EditorGUILayout.PropertyField( locationUsageDescriptionProp );
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox( "NSUserTrackingUsageDescription key with a custom message describing your usage. Can be empty.", MessageType.None );
            if (GUILayout.Button( "Info", EditorStyles.miniButton, GUILayout.ExpandWidth( false ) ))
                Application.OpenURL( Utils.configuringPrivacyURL );
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
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

            float requestsVal = 0.5f;
            float performVal = 0.5f;
            switch (loadingMode)
            {
                case LoadingManagerMode.FastestRequests:
                    requestsVal = 1.0f;
                    performVal = 0.1f;
                    break;
                case LoadingManagerMode.FastRequests:
                    requestsVal = 0.75f;
                    performVal = 0.4f;
                    break;
                case LoadingManagerMode.HighePerformance:
                    requestsVal = 0.25f;
                    performVal = 0.7f;
                    break;
                case LoadingManagerMode.HighestPerformance:
                case LoadingManagerMode.Manual:
                    requestsVal = 0.0f;
                    performVal = 1.0f;
                    break;
            }
            if (debugModeProp.boolValue)
                performVal -= 0.1f;

            EditorGUI.BeginDisabledGroup( true );
            DrawEffectiveSlider( "Requests:", requestsVal );
            DrawEffectiveSlider( "Performance:", performVal );
            EditorGUI.EndDisabledGroup();
        }

        private void DrawEffectiveSlider( string label, float performVal )
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space( 15.0f );
            GUILayout.Label( label, GUILayout.Width( 80.0f ) );
            GUILayout.Label( "slow", EditorStyles.miniLabel, GUILayout.ExpandWidth( false ) );
            GUILayout.HorizontalSlider( performVal, 0, 1 );
            GUILayout.Label( "fast", EditorStyles.miniLabel, GUILayout.ExpandWidth( false ) );
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

        private void OnCASAboutGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField( "CAS Unity wrapper version: " + MobileAds.wrapperVersion );
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
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button( "GitHub", EditorStyles.miniButton, GUILayout.ExpandWidth( false ) ))
                Application.OpenURL( Utils.gitUnityRepoURL );
            if (GUILayout.Button( "Support", EditorStyles.miniButton, GUILayout.ExpandWidth( false ) ))
                Application.OpenURL( Utils.supportURL );
            if (GUILayout.Button( "cleveradssolutions.com", EditorStyles.miniButton, GUILayout.ExpandWidth( false ) ))
                Application.OpenURL( Utils.websiteURL );
            EditorGUILayout.EndHorizontal();
        }

        private void OnPromoDependenciesGUI()
        {
            if (promoDependencyExist != EditorGUILayout.Toggle( "Cross Promotion enabled", promoDependencyExist ))
            {
                if (promoDependencyExist)
                {
                    if (DeactivateDependencies( Utils.promoTemplateDependency ))
                        promoDependencyExist = false;
                }
                else
                {
                    if (Utils.TryActivateDependencies( Utils.promoTemplateDependency, platform ))
                        promoDependencyExist = true;
                }
            }
            if (platform == BuildTarget.Android)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox( "Changing this flag will change the project dependencies. " +
                    "Please use Android Resolver after the change.", MessageType.None );
                EditorGUI.indentLevel--;
            }
        }

        private void OnAllowedAdNetworksGUI()
        {
            var audience = ( Audience )audienceTaggedProp.enumValueIndex;
            mediationNetworkScroll = EditorGUILayout.BeginScrollView( mediationNetworkScroll,
                GUILayout.ExpandHeight( false ), GUILayout.MinHeight( EditorGUIUtility.singleLineHeight * 5.5f ) );

            EditorGUILayout.LabelField( "Allowed partners networks for " + audience.ToString() + " audience" );
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < mediationPartners.Length; i++)
            {
                var partner = mediationPartners[i];
                var partnerTagged = partner.GetAudience( platform );
                if (platform == BuildTarget.iOS || ( audience == Audience.Mixed && partnerTagged == Audience.NotChildren )
                    || partnerTagged == audience || partnerTagged == Audience.Mixed)
                {
                    if (GUILayout.Button( partner.name, EditorStyles.miniButton, GUILayout.ExpandWidth( false ) ))
                        Application.OpenURL( partner.url );
                }
            }
            EditorGUILayout.EndHorizontal();
            if (platform == BuildTarget.Android)
            {
                EditorGUILayout.LabelField( "Not allowed partners networks for " + audience.ToString() + " audience" );
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < mediationPartners.Length; i++)
                {
                    var partner = mediationPartners[i];
                    var partnerTagged = partner.GetAudience( platform );
                    if (partnerTagged != audience && partnerTagged != Audience.Mixed
                        && !( audience == Audience.Mixed && partnerTagged == Audience.NotChildren ))
                    {
                        if (GUILayout.Button( partner.name, EditorStyles.miniButton, GUILayout.ExpandWidth( false ) ))
                            Application.OpenURL( partner.url );
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private void OnBuildSettingsGUI()
        {
            if (reimportDependencyOnBuild != EditorGUILayout.ToggleLeft( "Force reimport CAS dependencies on build.", reimportDependencyOnBuild ))
            {
                reimportDependencyOnBuild = !reimportDependencyOnBuild;
                PlayerPrefs.SetInt( Utils.editorReimportDepsOnBuildPrefs, reimportDependencyOnBuild ? 1 : 0 );
            }
        }

        private bool DeactivateDependencies( string template )
        {
            return AssetDatabase.DeleteAsset( Utils.editorFolderPath + "/"
                + template + platform.ToString() + Utils.dependenciesExtension );
        }

        private struct PartnerNetwork
        {
            public readonly string name;
            public readonly string url;
            public readonly Audience androidAudience;
            public readonly Audience iOSAudience;
            public PartnerNetwork( string name, string url, Audience androidAudience, Audience iOSAudience )
            {
                this.name = name;
                this.url = url;
                this.androidAudience = androidAudience;
                this.iOSAudience = iOSAudience;
            }
            public Audience GetAudience( BuildTarget platform )
            {
                if (platform == BuildTarget.Android)
                    return androidAudience;
                return iOSAudience;
            }
        }
    }
}