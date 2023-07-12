//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2023 CleverAdsSolutions. All rights reserved.
//

#define DisableBitcode

// EDM4U will do this by default
//#define AddMainTargetToPodfile

// No longer required
//#define AddApplicationQueriesSchames

#if UNITY_2019_3_OR_NEWER
#define EmbedDynamicLibraries
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

            EditPList(buildPath, (plist) =>
            {
                if (initSettings)
                    plist.SetGADAppIdForCAS(initSettings, depManager);
                plist.SetSDKInitializationDelay(editorSettings.delayAppMeasurementGADInit);
                plist.SetAppTransportSecuritySettings();
                plist.SetAttributionReportEndpoint(editorSettings.attributionReportEndpoint);
                plist.SetDefaultUserTrackingDescription(editorSettings.userTrackingUsageDescription);
                plist.AddSKAdNetworkItemsForCAS();
#if AddApplicationQueriesSchames
                plist.AddApplicationQueriesSchamesForCAS();
#endif
            });

            EditXCProject(buildPath, unityProjectName, (project) =>
            {
                var appTargetGuid = project.GetAppGUID();
                project.EnableSwiftLibraries(appTargetGuid);
                project.FixLibrariesExecutablePath(appTargetGuid, depManager);
                if (initSettings)
                    project.CopyConfigCacheFileForCAS(buildPath, appTargetGuid, initSettings);
            });

            if (editorSettings.generateIOSDeepLinksForPromo && initSettings)
                ApplyCrosspromoDynamicLinks(buildPath, initSettings, depManager);

#if AddMainTargetToPodfile
            UpdatePodfileForUnity2019(buildPath);
#endif
            Debug.Log(CASEditorUtils.logTag + "Postrocess Build done.");
        }

        [PostProcessBuild(int.MaxValue - 2)]
        public static void OnCocoaPodsReady(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget != BuildTarget.iOS)
                return;

            var editorSettings = CASEditorSettings.Load();

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

#if EmbedDynamicLibraries
                var depManager = DependencyManager.Create(BuildTarget.iOS, Audience.Mixed, true);
                project.EmbedDynamicLibrariesIfNeeded(buildPath, appTargetGuid, depManager);
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

        private static void SetSDKInitializationDelay(this PlistDocument plist, bool delayInit)
        {
            plist.root.SetBoolean("GADDelayAppMeasurementInit", delayInit);
            plist.root.SetBoolean("MyTargetSDKAutoInitMode", !delayInit);
        }

        private static void SetGADAppIdForCAS(this PlistDocument plist, CASInitSettings initSettings, DependencyManager deps)
        {
            #region Read Admob App ID from CAS Settings
            bool admobAppIdRequired = deps == null;
            if (deps != null)
            {
                var admobDep = deps.Find(AdNetwork.GoogleAds);
                if (admobDep != null)
                    admobAppIdRequired = admobDep.IsInstalled();
            }

            string admobAppId = null;
            if (initSettings.managersCount > 0)
            {
                string settingsPath = CASEditorUtils.GetNativeSettingsPath(BuildTarget.iOS, initSettings.GetManagerId(0));
                if (File.Exists(settingsPath))
                {
                    try
                    {
                        admobAppId = CASEditorUtils.GetAdmobAppIdFromJson(File.ReadAllText(settingsPath));
                    }
                    catch (Exception e)
                    {
                        if (!initSettings.IsTestAdMode() && admobAppIdRequired)
                            CASEditorUtils.StopBuildWithMessage(e.ToString(), BuildTarget.iOS);
                    }
                }
            }
            if (string.IsNullOrEmpty(admobAppId) && initSettings.IsTestAdMode())
            {
                admobAppId = CASEditorUtils.iosAdmobSampleAppID;
            }

            #endregion

            if (!string.IsNullOrEmpty(admobAppId))
                plist.root.SetString("GADApplicationIdentifier", admobAppId);
        }

        private static void AddSKAdNetworkItemsForCAS(this PlistDocument plist)
        {
            var templateFile = CASEditorUtils.GetTemplatePath(CASEditorUtils.iosSKAdNetworksTemplateFile);
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

        private static void AddApplicationQueriesSchamesForCAS(this PlistDocument plist)
        {
            PlistElementArray schemesList;
            var applicationQueriesSchemesField = plist.root["LSApplicationQueriesSchemes"];
            if (applicationQueriesSchemesField == null)
                schemesList = plist.root.CreateArray("LSApplicationQueriesSchemes");
            else
                schemesList = applicationQueriesSchemesField.AsArray();
            var schemes = new string[] { "fb", "instagram", "tumblr", "twitter" };
            for (int i = 0; i < schemes.Length; i++)
            {
                var scheme = schemes[i];
                if (string.IsNullOrEmpty(scheme))
                    continue;
                var exist = false;
                for (int findI = 0; findI < schemesList.values.Count; findI++)
                {
                    if (schemesList.values[findI].AsString() == scheme)
                    {
                        exist = true;
                        break;
                    }
                }
                if (!exist)
                    schemesList.AddString(scheme);
            }
        }

        private static void SetAppTransportSecuritySettings(this PlistDocument plist)
        {
            PlistElement atsRoot;
            plist.root.values.TryGetValue("NSAppTransportSecurity", out atsRoot);

            if (atsRoot == null || atsRoot.GetType() != typeof(PlistElementDict))
            {
                // Add the missing App Transport Security settings for publishers if needed. 
                Debug.Log(CASEditorUtils.logTag + "Adding App Transport Security settings...");
                atsRoot = plist.root.CreateDict("NSAppTransportSecurity");
                atsRoot.AsDict().SetBoolean("NSAllowsArbitraryLoads", true);
                return;
            }

            // Check if both NSAllowsArbitraryLoads and NSAllowsArbitraryLoadsInWebContent are present
            // and remove NSAllowsArbitraryLoadsInWebContent if both are present.
            var atsRootDict = atsRoot.AsDict().values;
            if (atsRootDict.ContainsKey("NSAllowsArbitraryLoads")
                && atsRootDict.ContainsKey("NSAllowsArbitraryLoadsInWebContent"))
            {
                Debug.Log(CASEditorUtils.logTag + "Removing NSAllowsArbitraryLoadsInWebContent");
                atsRootDict.Remove("NSAllowsArbitraryLoadsInWebContent");
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
            plist.root.SetString("NSUserTrackingUsageDescription", description);
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

        private static void CopyConfigCacheFileForCAS(this PBXProject project, string rootPath, string targetGuid, CASInitSettings casSettings)
        {
            var resourcesBuildPhase = project.GetResourcesBuildPhaseByTarget(targetGuid);
            for (int i = 0; i < casSettings.managersCount; i++)
            {
                string managerId = casSettings.GetManagerId(i);
                int managerIdLength = managerId.Length;
                string suffixChar = char.ToLower(managerId[managerIdLength - 1]).ToString();
                string fileName = "cas_settings" + managerIdLength.ToString() + suffixChar + ".json";
                string pathInAssets = CASEditorUtils.GetNativeSettingsPath(BuildTarget.iOS, managerId);
                if (File.Exists(pathInAssets))
                {
                    try
                    {
                        File.Copy(pathInAssets, Path.Combine(rootPath, fileName), true);
                        var fileGuid = project.AddFile(fileName, fileName, PBXSourceTree.Source);
                        project.AddFileToBuildSection(targetGuid, resourcesBuildPhase, fileGuid);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(CASEditorUtils.logTag + "Copy Raw File To XCode Project failed: " + e.ToString());
                    }
                }
                else
                {
                    Debug.Log(CASEditorUtils.logTag + "Not found Raw file: " + pathInAssets);
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
                Debug.Log(CASEditorUtils.logTag + "Apply application Associated Domain: " + link);
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

        private static void EnableSwiftLibraries(this PBXProject project, string mainTargetGuid)
        {
            var frameworkTargetGuid = project.GetFrameworkGUID();
            var swiftVersion = project.GetBuildPropertyForAnyConfig(frameworkTargetGuid, "SWIFT_VERSION");
            if (string.IsNullOrEmpty(swiftVersion))
                project.SetBuildProperty(frameworkTargetGuid, "SWIFT_VERSION", "5.0");
            project.SetBuildProperty(frameworkTargetGuid, "CLANG_ENABLE_MODULES", "YES");


            // For Swift 5+ code that uses the standard libraries,
            // the Swift Standard Libraries MUST be embedded for iOS < 12.2
            // 
            // Swift 5 introduced ABI stability, which allowed iOS to start bundling
            // the standard libraries in the OS starting with iOS 12.2
            // This needs to be added the main target.
            // App Store may reject builds if added to UnityFramework.
            string embedStandardLib = IsNeedEmbedSwiftStandardLibraries() ? "YES" : "NO";
            project.SetBuildProperty(mainTargetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", embedStandardLib);
            if (frameworkTargetGuid != mainTargetGuid)
            {
                // Force disable for Framework target. Cause build error when enabled in both targets.
                project.SetBuildProperty(frameworkTargetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
            }
        }

        private static void AddSwiftFile(this PBXProject project, string buildPath, string frameworkTargetGuid)
        {
            // Skip when Generated by External Dependency Manager for Unity
            if (File.Exists(Path.Combine(buildPath, "Dummy.swift")))
                return;

            var relativePath = "Classes/CASSwiftEnable.swift";
            var filePath = Path.Combine(buildPath, relativePath);
            if (File.Exists(filePath))
                return;

            try
            {
                File.WriteAllLines(filePath, new[]{
                    "// This file ensures the project includes Swift support.",
                    "// It is automatically generated by CAS Unity Plugin.",
                    "import Foundation"
                });
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            var swiftFileGuid = project.AddFile(relativePath, relativePath, PBXSourceTree.Source);
            project.AddFileToBuild(frameworkTargetGuid, swiftFileGuid);
        }

        private static void FixLibrariesExecutablePath(this PBXProject project, string targetGuid, DependencyManager deps)
        {
#if UNITY_2018_2_OR_NEWER
            string runpathSearchPaths = project.GetBuildPropertyForAnyConfig(targetGuid, "LD_RUNPATH_SEARCH_PATHS");
            if (runpathSearchPaths == null)
                runpathSearchPaths = "";
#else
            string runpathSearchPaths = "$(inherited)";          
#endif

#if !UNITY_2019_3_OR_NEWER
            // Check if runtime search paths already contains the required search paths for dynamic libraries.
            const string pathToFrameworks = "@executable_path/Frameworks";
            if (!runpathSearchPaths.Contains(pathToFrameworks))
                runpathSearchPaths += " " + pathToFrameworks;
#endif

            // If build for iOS 12.2.0 or earlier,
            // add /usr/lib/swift to Build Settings > Runpath Search Paths
            // to prevent any issues with libswiftCore.dylib.
            const string pathToSwiftLibs = "/usr/lib/swift";
            if (IsNeedEmbedSwiftStandardLibraries() && !runpathSearchPaths.Contains(pathToSwiftLibs))
                runpathSearchPaths += " " + pathToSwiftLibs;

            project.SetBuildProperty(targetGuid, "LD_RUNPATH_SEARCH_PATHS", runpathSearchPaths.Trim(' '));

            // Needed to build successfully on Xcode 12+,
            // as framework was build with latest Xcode but not as an xcframework
            //project.AddBuildProperty( targetGuid, "VALIDATE_WORKSPACE", "YES" );
        }

        private static bool IsNeedEmbedSwiftStandardLibraries()
        {
            try
            {
                return new Version(PlayerSettings.iOS.targetOSVersionString) <= new Version(12, 2, 0);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return false;
        }

        private static void LocalizeUserTrackingDescription(this PBXProject project, string buildPath, string targetGuid, KeyValuePair[] descriptions)
        {
            const string LegacyResourcesDirectoryName = "Resources";
            const string CASResourcesDirectoryName = "CASUResources";

            if (descriptions.Length < 2)
                return;

            // Use the legacy resources directory name if the build is being appended (the "Resources" directory already exists if it is an incremental build).
            var resourcesDirectoryName = Directory.Exists(Path.Combine(buildPath, LegacyResourcesDirectoryName))
                ? LegacyResourcesDirectoryName : CASResourcesDirectoryName;
            var resourcesDirectoryPath = Path.Combine(buildPath, resourcesDirectoryName);

            for (int i = 0; i < descriptions.Length; i++)
            {
                var keyValue = descriptions[i];
                var description = keyValue.value;
                var localeCode = keyValue.key;
                if (string.IsNullOrEmpty(localeCode))
                    continue;
                var localeSpecificDirectoryName = localeCode + ".lproj";
                var localeSpecificDirectoryPath = Path.Combine(resourcesDirectoryPath, localeSpecificDirectoryName);
                var infoPlistStringsFilePath = Path.Combine(localeSpecificDirectoryPath, "InfoPlist.strings");

                // Check if localization has been disabled between builds, and remove them as needed.
                if (string.IsNullOrEmpty(description))
                {
                    if (File.Exists(infoPlistStringsFilePath))
                        File.Delete(infoPlistStringsFilePath);
                    continue;
                }

                // Create intermediate directories as needed.
                if (!Directory.Exists(resourcesDirectoryPath))
                    Directory.CreateDirectory(resourcesDirectoryPath);
                if (!Directory.Exists(localeSpecificDirectoryPath))
                    Directory.CreateDirectory(localeSpecificDirectoryPath);


                var localizedDescriptionLine = "\"NSUserTrackingUsageDescription\" = \"" + description + "\";\n";
                // File already exists, update it in case the value changed between builds.
                if (File.Exists(infoPlistStringsFilePath))
                {
                    var output = new List<string>();
                    var lines = File.ReadAllLines(infoPlistStringsFilePath);
                    var keyUpdated = false;
                    foreach (var line in lines)
                    {
                        if (line.Contains("NSUserTrackingUsageDescription"))
                        {
                            output.Add(localizedDescriptionLine);
                            keyUpdated = true;
                        }
                        else
                        {
                            output.Add(line);
                        }
                    }

                    if (!keyUpdated)
                        output.Add(localizedDescriptionLine);

                    File.WriteAllText(infoPlistStringsFilePath, string.Join("\n", output.ToArray()) + "\n");
                }
                // File doesn't exist, create one.
                else
                {
                    File.WriteAllText(infoPlistStringsFilePath, "/* Localized versions of Info.plist keys - Generated by CAS plugin */\n" + localizedDescriptionLine);
                }

                var guid = project.AddFolderReference(localeSpecificDirectoryPath, Path.Combine(resourcesDirectoryName, localeSpecificDirectoryName));
                project.AddFileToBuild(targetGuid, guid);
            }
        }

        private static void EmbedDynamicLibrariesIfNeeded(this PBXProject project, string buildPath, string targetGuid, DependencyManager deps)
        {
            for (int i = 0; i < deps.networks.Length; i++)
            {
                var dynamicLibraryPath = deps.networks[i].embedFramework;
                if (string.IsNullOrEmpty(dynamicLibraryPath))
                    continue;
                dynamicLibraryPath = Path.Combine("Pods", dynamicLibraryPath);
                if (!Directory.Exists(Path.Combine(buildPath, dynamicLibraryPath)))
                    continue;

#if UNITY_2019_3_OR_NEWER
                var fileGuid = project.AddFile(dynamicLibraryPath, dynamicLibraryPath);
                project.AddFileToEmbedFrameworks(targetGuid, fileGuid);
#endif
            }
        }
        #endregion
    }
}
#endif