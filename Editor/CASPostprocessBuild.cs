//  Copyright © 2024 CAS.AI. All rights reserved.

// Bitcode is deprecated in Xcode 14.
// Unity 2020.3.44f still use Bitcode.
#define DisableBitcode

// EDM4U will add `Unity-iPhone` target to Podfile by default
//#define AddMainTargetToPodfile

#if UNITY_2019_3_OR_NEWER
#define EmbedDynamicFrameworks

// Avoid XCFrameworks issue with: Invalid Signature - A sealed resource is missing or invalid
//#define CAS_UNPACK_XCFRAMEWORKS

// Yandex Ads not support to place the Resources bundle in UnityFramework, 
// then we embed the bundle to App target
#define EmbedYandexAdsResourcesBundle
#endif


#if UNITY_IOS || CASDeveloper
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

#if UNITY_2019_3_OR_NEWER
using UnityEditor.iOS.Xcode.Extensions;
#endif

namespace CAS.UEditor
{
    public static class CASPostprocessBuild
    {
        private const string unityProjectName = "Unity-iPhone";
        private const string trackingUsageDescriptionKey = "NSUserTrackingUsageDescription";

        // Must be between 40 and 50 to ensure that it's not overriden by Podfile generation (40)
        // and that it's added before "pod install" (50)
        [PostProcessBuild(47)]
        public static void MainPostprocess(BuildTarget target, string buildPath)
        {
            if (target != BuildTarget.iOS)
                return;

            // Init Settings can be null
            var initSettings = CASEditorUtils.GetSettingsAsset(BuildTarget.iOS, false);
            var editorSettings = CASEditorSettings.Load();
            var depManager = DependencyManager.Create(BuildTarget.iOS, Audience.Mixed, true);

            EditXCProject(buildPath, unityProjectName, (project) =>
            {
                var appTargetGuid = project.GetAppGUID();
                var frameworkTargetGuid = project.GetFrameworkGUID();
                project.EnableSwiftLibraries(appTargetGuid, frameworkTargetGuid);
                project.FixLibrariesExecutablePath(appTargetGuid, depManager);
                project.FixGenerationInfoPlist(frameworkTargetGuid);
                project.FixGenerationInfoPlist(appTargetGuid);
                project.AddCASConfigResources(buildPath, appTargetGuid, initSettings);

                if (editorSettings.userTrackingUsageDescription.Length > 0)
                {
                    project.AddFrameworkToProject(frameworkTargetGuid, "AdSupport.framework", false);
                    project.AddFrameworkToProject(frameworkTargetGuid, "AppTrackingTransparency.framework", false);
                }
            });

            if (editorSettings.generateIOSDeepLinksForPromo && initSettings)
                ApplyCrosspromoDynamicLinks(buildPath, initSettings, depManager);

#if AddMainTargetToPodfile
            UpdatePodfileForUnity2019(buildPath);
#endif
            CASEditorUtils.Log("Postrocess Build done: " + MobileAds.wrapperVersion);
        }

        [PostProcessBuild(int.MaxValue - 10)]
        public static void OnCocoaPodsReady(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget != BuildTarget.iOS)
                return;

            // Init Settings can be null
            var initSettings = CASEditorUtils.GetSettingsAsset(BuildTarget.iOS, false);
            var editorSettings = CASEditorSettings.Load();
            var depManager = DependencyManager.Create(BuildTarget.iOS, Audience.Mixed, true);

            EditPList(buildPath, (plist) =>
            {
                plist.SetGADAppIdForCAS(initSettings, depManager);
                plist.SetSDKInitializationDelay();
                plist.SetAppTransportSecuritySettings();
                plist.SetAttributionReportEndpoint(editorSettings.attributionReportEndpoint);
                plist.SetDefaultUserTrackingDescription(editorSettings.userTrackingUsageDescription);
                plist.AddSKAdNetworkItemsForCAS();
            });

            EditXCProject(buildPath, unityProjectName, (project) =>
            {
                var appTargetGuid = project.GetAppGUID();
#if DisableBitcode
                project.SetBitcodeEnabled(appTargetGuid, false);
                var frameworkGuid = project.GetFrameworkGUID();
                if (appTargetGuid != frameworkGuid)
                    project.SetBitcodeEnabled(frameworkGuid, false);
#endif
                project.LocalizeUserTrackingDescription(buildPath, appTargetGuid, editorSettings.userTrackingUsageDescription);

#if EmbedDynamicFrameworks
                if (IsStaticPodInstallUsed(buildPath))
                {
                    var depManager = DependencyManager.Create(BuildTarget.iOS, Audience.Mixed, true);
                    project.AddEmbeddablePaths(appTargetGuid, depManager);

#if EmbedYandexAdsResourcesBundle
                    var yandexDep = depManager.Find(AdNetwork.YandexAds);
                    if (yandexDep != null && yandexDep.IsInstalled())
                    {
                        const string yandexBundlePath = "Pods/YandexMobileAds/static/YandexMobileAds.xcframework/MobileAdsBundle.bundle";
                        project.AddEmbeddableResources(appTargetGuid, yandexBundlePath);
                    }
#endif
                }
#endif
            });

#if DisableBitcode
            EditXCProject(Path.Combine(buildPath, "Pods"), "Pods", (project) =>
            {
                project.SetBitcodeEnabled(project.ProjectGuid(), false);
            });
#endif
        }

        private static void UpdatePodfileForUnity2019(string buildPath)
        {
            var path = Path.Combine(buildPath, "Podfile");
            if (!File.Exists(path))
            {
                Debug.LogError(CASEditorUtils.logTag + "Podfile not found.\n" +
                   "Please add `target '" + unityProjectName + "' do end` to the Podfile in root folder " +
                   "of XCode project and call `pod install --no-repo-update`");
                return;
            }
            try
            {
                var content = File.ReadAllText(path);
                if (!content.Contains("'" + unityProjectName + "'"))
                {
                    content += "\ntarget '" + unityProjectName + "' do\nend\n";
                    File.WriteAllText(path, content);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        #region Info PList
        private static void EditPList(string root, Action<PlistDocument> action)
        {
            string path = Path.Combine(root, "Info.plist");
            if (!File.Exists(path))
            {
                Debug.LogError(CASEditorUtils.logTag + "XCode plist not found: " + path);
                return;
            }
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(path);
            action(plist);
            File.WriteAllText(path, plist.WriteToString());
        }

        private static void SetSDKInitializationDelay(this PlistDocument plist)
        {
            plist.root.SetBoolean("GADDelayAppMeasurementInit", true);
            plist.root.SetBoolean("MyTargetSDKAutoInitMode", !false);
        }

        private static void SetGADAppIdForCAS(this PlistDocument plist, CASInitSettings initSettings, DependencyManager deps)
        {
            string googleAppId = AdRemoteConfig.FindGADAppId(initSettings, deps);

            if (!string.IsNullOrEmpty(googleAppId))
                plist.root.SetString("GADApplicationIdentifier", googleAppId);
        }

        private static void AddSKAdNetworkItemsForCAS(this PlistDocument plist)
        {
            var templateFile = CASEditorUtils.GetPluginComponentPath(CASEditorUtils.iosSKAdNetworksTemplateFile);
            if (string.IsNullOrEmpty(templateFile))
            {
                Debug.LogError(CASEditorUtils.logTag + "Not found SKAdNetworkItems. Try reimport CAS package.");
                return;
            }
            var networksLines = File.ReadAllLines(templateFile);

            PlistElementArray adNetworkItems;
            var adNetworkItemsField = plist.root["SKAdNetworkItems"];
            if (adNetworkItemsField == null)
                adNetworkItems = plist.root.CreateArray("SKAdNetworkItems");
            else
                adNetworkItems = adNetworkItemsField.AsArray();

            for (int i = 0; i < networksLines.Length; i++)
            {
                if (!string.IsNullOrEmpty(networksLines[i]))
                {
                    var dict = adNetworkItems.AddDict();
                    dict.SetString("SKAdNetworkIdentifier", networksLines[i] + ".skadnetwork");
                }
            }
        }

        private static void SetAppTransportSecuritySettings(this PlistDocument plist)
        {
            PlistElement atsRoot;
            plist.root.values.TryGetValue("NSAppTransportSecurity", out atsRoot);

            if (atsRoot == null || atsRoot.GetType() != typeof(PlistElementDict))
            {
                // Add the missing App Transport Security settings for publishers if needed. 
                CASEditorUtils.Log("Adding App Transport Security settings");
                atsRoot = plist.root.CreateDict("NSAppTransportSecurity");
                atsRoot.AsDict().SetBoolean("NSAllowsArbitraryLoads", true);
                return;
            }
        }

        private static void SetAttributionReportEndpoint(this PlistDocument plist, string endpoint)
        {
            if (!string.IsNullOrEmpty(endpoint))
                plist.root.SetString("NSAdvertisingAttributionReportEndpoint", endpoint);
        }

        private static void SetDefaultUserTrackingDescription(this PlistDocument plist, KeyValuePair[] descriptions)
        {
            if (descriptions.Length == 0)
                return;
            var description = descriptions[0].value;
            if (string.IsNullOrEmpty(description))
                return;
            plist.root.SetString(trackingUsageDescriptionKey, description);
        }
        #endregion

        #region XCode project
        private static string GetXCodeProjectPath(string buildPath)
        {
            return Path.Combine(buildPath, "Unity-iPhone.xcodeproj/project.pbxproj");
        }

        private static void EditXCProject(string root, string projectName, Action<PBXProject> action)
        {
            var path = Path.Combine(root, projectName + ".xcodeproj") + "/project.pbxproj";
            if (!File.Exists(path))
            {
                Debug.LogWarning(CASEditorUtils.logTag + "XCode project not found: " + path);
                return;
            }
            var project = new PBXProject();
            project.ReadFromString(File.ReadAllText(path));
            action(project);
            File.WriteAllText(path, project.WriteToString());
        }

        private static string GetAppGUID(this PBXProject project)
        {
#if UNITY_2019_3_OR_NEWER
            return project.GetUnityMainTargetGuid();
#elif UNITY_2018_1_OR_NEWER
            return project.TargetGuidByName(PBXProject.GetUnityTargetName());
#else
            return project.TargetGuidByName( unityProjectName );
#endif
        }

        private static string GetFrameworkGUID(this PBXProject project)
        {
#if UNITY_2019_3_OR_NEWER
            return project.GetUnityFrameworkTargetGuid();
#else
            return GetAppGUID(project);
#endif
        }

        private static void AddCASConfigResources(this PBXProject project, string rootPath, string targetGuid, CASInitSettings casSettings)
        {
            if (!casSettings) return;
            var resourcesBuildPhase = project.GetResourcesBuildPhaseByTarget(targetGuid);
            for (int i = 0; i < casSettings.managersCount; i++)
            {
                string cachePath = AdRemoteConfig.GetCachePath(BuildTarget.iOS, casSettings.GetManagerId(i));
                if (File.Exists(cachePath))
                {
                    try
                    {
                        string fileName = AdRemoteConfig.GetResourcesFileName(casSettings.GetManagerId(i));
                        File.Copy(cachePath, Path.Combine(rootPath, fileName), true);
                        var fileGuid = project.AddFile(fileName, fileName);
                        project.AddFileToBuildSection(targetGuid, resourcesBuildPhase, fileGuid);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(CASEditorUtils.logTag + "Copy Config resources to XCode Project failed: " + e.ToString());
                    }
                }
                else
                {
                    CASEditorUtils.Log("Not found config file: " + cachePath);
                }
            }
        }

        private static void ApplyCrosspromoDynamicLinks(string buildPath, CASInitSettings casSettings, DependencyManager deps)
        {
            if (casSettings.IsTestAdMode() || casSettings.managersCount == 0
                || string.IsNullOrEmpty(casSettings.GetManagerId(0)))
                return;
            if (deps != null)
            {
                var crossPromoDependency = deps.FindCrossPromotion();
                if (crossPromoDependency != null && !crossPromoDependency.IsInstalled())
                    return;
            }
            try
            {
                var identifier = Application.identifier;
                var productName = identifier.Substring(identifier.LastIndexOf(".") + 1);
                var projectPath = GetXCodeProjectPath(buildPath);
                // Attention: Use string name of Unity target for any Unity Editor version to avoid deprecation warnings. 
                var entitlements = new ProjectCapabilityManager(
                    projectPath, productName + ".entitlements", unityProjectName);
                string link = "applinks:psvios" + casSettings.GetManagerId(0) + ".page.link";
                entitlements.AddAssociatedDomains(new[] { link });
                entitlements.WriteToFile();
                CASEditorUtils.Log("Apply application Associated Domain: " + link);
            }
            catch (Exception e)
            {
                Debug.LogError(CASEditorUtils.logTag + "Dynamic link creation fail: " + e.ToString());
            }
        }

        private static void SetBitcodeEnabled(this PBXProject project, string targetGuid, bool enabled)
        {
            project.SetBuildProperty(targetGuid, "ENABLE_BITCODE", enabled ? "YES" : "NO");
        }

        private static void EnableSwiftLibraries(this PBXProject project, string mainTargetGuid, string frameworkTargetGuid)
        {
            var swiftVersion = project.GetBuildPropertyForAnyConfig(frameworkTargetGuid, "SWIFT_VERSION");
            if (string.IsNullOrEmpty(swiftVersion))
                project.SetBuildProperty(frameworkTargetGuid, "SWIFT_VERSION", "5.0");
            project.SetBuildProperty(frameworkTargetGuid, "CLANG_ENABLE_MODULES", "YES");

            project.SetBuildProperty(mainTargetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
            if (frameworkTargetGuid != mainTargetGuid)
            {
                // Force disable for Framework target. Cause build error when enabled in both targets.
                project.SetBuildProperty(frameworkTargetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
            }
        }

        /// <summary>
        /// Known issue with failed validation by Apple:
        /// <para>
        /// The bundle 'Payload/Cubes World.app/Frameworks/UnityFramework.framework' is missing plist key. 
        /// The Info.plist file is missing the required key: CFBundleShortVersionString. 
        /// Please find more information about CFBundleShortVersionString
        /// </para>
        /// This happens when some pods want to enable GENERATE_INFOPLIST_FILE, but Unity is not support it.
        /// - Yandex Ads: https://github.com/CocoaPods/Specs/blob/master/Specs/a/c/5/YandexMobileAds/6.0.0/YandexMobileAds.podspec.json
        /// </summary>
        private static void FixGenerationInfoPlist(this PBXProject project, string targetGuid)
        {
            project.SetBuildProperty(targetGuid, "GENERATE_INFOPLIST_FILE", "NO");
        }

        private static void FixLibrariesExecutablePath(this PBXProject project, string targetGuid, DependencyManager deps)
        {
#if !UNITY_2019_3_OR_NEWER
            string runpathSearchPaths = project.GetBuildPropertyForAnyConfig(targetGuid, "LD_RUNPATH_SEARCH_PATHS");

            // Check if runtime search paths already contains the required search paths for dynamic libraries.
            const string pathToFrameworks = "@executable_path/Frameworks";
            if (!runpathSearchPaths.Contains(pathToFrameworks))
            {
                if (runpathSearchPaths == null)
                    runpathSearchPaths = pathToFrameworks;
                else
                    runpathSearchPaths += " " + pathToFrameworks;

                project.SetBuildProperty(targetGuid, "LD_RUNPATH_SEARCH_PATHS", runpathSearchPaths.Trim(' '));
            }
#endif
        }

        private static void LocalizeUserTrackingDescription(this PBXProject project, string buildPath, string targetGuid, KeyValuePair[] descriptions)
        {
            const string resourcesDirectoryName = "CASUResources";

            if (descriptions.Length < 2)
                return;

            var resourcesRootPath = Path.Combine(buildPath, resourcesDirectoryName);

            foreach (var keyValue in descriptions)
            {
                if (string.IsNullOrEmpty(keyValue.key))
                    continue;
                if (string.IsNullOrEmpty(keyValue.value))
                    continue;
                var localizedLine = "\"" + trackingUsageDescriptionKey + "\" = \"" + keyValue.value + "\";";
                var resDirectoryName = keyValue.key + ".lproj";
                var resDirectoryPath = Path.Combine(resourcesRootPath, resDirectoryName);
                var resInfoPlistPath = Path.Combine(resDirectoryPath, "InfoPlist.strings");
                var resPathInProject = Path.Combine(resourcesDirectoryName, resDirectoryName);

                if (!Directory.Exists(resDirectoryPath))
                    Directory.CreateDirectory(resDirectoryPath);

                if (File.Exists(resInfoPlistPath))
                {
                    var lines = File.ReadAllLines(resInfoPlistPath);
                    var needAppend = true;
                    using (StreamWriter writer = new StreamWriter(resInfoPlistPath, false))
                    {
                        for (int i = 0; i < lines.Length; i++)
                        {
                            if (lines[i].Contains(trackingUsageDescriptionKey))
                            {
                                writer.WriteLine(localizedLine);
                                needAppend = false;
                            }
                            else
                            {
                                writer.WriteLine(lines[i]);
                            }
                        }
                        if (needAppend)
                            writer.WriteLine(localizedLine);
                    }
                }
                else
                {
                    File.WriteAllText(resInfoPlistPath, "/* Generated by CAS Unity plugin */\n" + localizedLine);
                }

                var guid = project.AddFolderReference(resPathInProject, resPathInProject);
                project.AddFileToBuild(targetGuid, guid);
            }
        }

#if EmbedDynamicFrameworks
        private static bool IsStaticPodInstallUsed(string buildPath)
        {
            var podPath = Path.Combine(buildPath, "Podfile");
            if (!File.Exists(podPath))
            {
                Debug.LogWarning(CASEditorUtils.logTag + "IsStaticPodInstallUsed Not found path: " + podPath);
                return true;
            }
            try
            {
                var content = File.ReadAllText(podPath);
                // If Podfile have use_frameworks!
                // at last then DynamicFrameworks will embeded by Cocoapods

                // If Podifle have use_frameworks! :linkage => :static
                // at last then DynamicFrameworks should be embeded by CAS script
                var dynamicUsage = content.LastIndexOf("use_frameworks!");
                var staticUsage = content.LastIndexOf(":linkage => :static");
                return dynamicUsage < staticUsage;
            }
            catch (Exception e)
            {
                Debug.LogWarning(CASEditorUtils.logTag + "IsStaticPodInstallUsed: " + e.Message);
            }
            return true;
        }

        private static void AddEmbeddablePaths(this PBXProject project, string targetGuid, DependencyManager deps)
        {
            for (int i = 0; i < deps.networks.Length; i++)
            {
                if (deps.networks[i].embedPath.Length == 0 || !deps.networks[i].IsInstalled())
                    continue;

                foreach (var embedPath in deps.networks[i].embedPath)
                {
                    var path = Path.Combine("Pods", embedPath);
#if CAS_UNPACK_XCFRAMEWORKS
                    if (path.EndsWith(".xcframework"))
                    {
                        var frameworkName = Path.GetFileNameWithoutExtension(path) + ".framework";
                        var arch = Path.Combine(path, "ios-arm64");
                        if (PlayerSettings.iOS.sdkVersion == iOSSdkVersion.SimulatorSDK)
                            arch += "_x86_64-simulator";
                        path = Path.Combine(arch, frameworkName);
                    }
#endif
                    var pathInProject = Path.Combine("Frameworks", Path.GetFileName(path));
                    var fileGuid = project.AddFile(path, pathInProject);
                    project.AddFileToEmbedFrameworks(targetGuid, fileGuid);
                }
            }
        }
#endif

        private static void AddEmbeddableResources(this PBXProject project, string targetGuid, string bundlePath)
        {
            var pathInProject = Path.Combine("Frameworks", Path.GetFileName(bundlePath));
            var resourcesGuid = project.AddFolderReference(bundlePath, pathInProject, PBXSourceTree.Source);
            var buildPhase = project.GetResourcesBuildPhaseByTarget(targetGuid);
            project.AddFileToBuildSection(targetGuid, buildPhase, resourcesGuid);
        }
        #endregion
    }
}
#endif