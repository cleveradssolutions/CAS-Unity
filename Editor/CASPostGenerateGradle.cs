//  Copyright © 2025 CAS.AI. All rights reserved.

#if UNITY_ANDROID || CASDeveloper

/*
Unity Version               | Gradle  | AGP
6000.0.45f1+                | 8.11    | 8.7.2
6000.0.1f1 - 6000.0.44f1    | 8.4     | 8.3.0
2023.1 - 2023.2             | 7.6     | 7.3.1
2022.3.38f1+                | 7.5.1   | 7.4.2
2022.2 - 2022.3.37f1        | 7.2     | 7.1.2
2021.3.41f1+                | 7.5.1   | 7.4.2
2021.3.37f1 - 2021.3.40f1   | 6.7.1   | 4.2.2
2021.2 - 2021.3.36f1        | 6.1.1   | 4.0.1
*/


// Known issue of META-INF duplication from kotlinx_coroutines_core and core-utils_release.
// Recommended workaround: pickFirst META-INF in packagingOptions.
// by cas_android_build_options.gradle file
#if !CAS_DISABLE_PACKAGING_OPTIONS
#define CAS_ADD_PACKAGING_OPTIONS
#endif

// Known issue: Crash on API 25 and below when using play-services-ads-identifier:18.2.0.
// Recommended workaround: explicitly force library version 18.1.0.
#define CAS_FORCE_AD_ID_VERSION

#if !UNITY_6000_0_OR_NEWER
// Known issue: Yandex Ads SDK 7.10.0 build fails with Android Gradle Plugin versions lower than 7.5.
// Recommended workaround: explicitly force SDK version 7.9.0.
#define CAS_DOWNGRADE_YANDEX_SDK
#endif

#if !UNITY_2021_3_OR_NEWER
// Known issue in older minor versions (before 4.2.2) of the Android Gradle Plugin to enable support 
// for <queries> elements in the manifest.
#define CAS_FIX_AGP_MINOR_VERSION
#endif

#if !UNITY_2022_2_OR_NEWER
// Known issue with jCenter repository where repository is not responding
// and gradle build stops with timeout error.
#define CAS_REMOVE_JCENTER
#endif

using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Android;
using UnityEditor;
using System.Text;
using System.Text.RegularExpressions;

namespace CAS.UEditor
{
    using Utils = CASEditorUtils;

    public class CASPostGenerateGradle : IPostGenerateGradleAndroidProject
    {
        private readonly XNamespace androidNamespace = "http://schemas.android.com/apk/res/android";
        private readonly Regex versionRegex = new Regex("\\b\\d+(?:\\.\\d+){1,2}\\b");

        public int callbackOrder { get { return 1000; } }

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            // Path is 'root/unityLibrary/' with 'build.gradle' for Unity 2019.3+
            // Path is 'root/' with 'build.gradle' for Unity 2018

            var initSettings = Utils.GetSettingsAsset(BuildTarget.Android, false);
            var editorSettings = CASEditorSettings.Load();
            var depManager = DependencyManager.Create(BuildTarget.Android, Audience.Mixed, true);

            UpdateGradleWrapper(path, editorSettings);
            UpdateRootGradleBuild(path, initSettings, editorSettings, depManager);
            UpdateGradleBuild(path);
            UpdateGradleProperties(path);
            UpdateAppManifest(path, initSettings, editorSettings, depManager);
            AddCASConfigResources(path, initSettings);
        }

        private void UpdateGradleWrapper(string path, CASEditorSettings editorSettings)
        {
            if (string.IsNullOrEmpty(editorSettings.overrideGradleWrapperVersion))
                return;

#if UNITY_2019_3_OR_NEWER
            path = Path.Combine(path, "..");
#endif
            var propertiesPath = Path.Combine(path, "gradle/wrapper/gradle-wrapper.properties");
            if (!File.Exists(propertiesPath))
            {
                Debug.LogError(Utils.logTag + "Required file is missied: " + propertiesPath);
                return;
            }
            var lines = File.ReadAllLines(propertiesPath);
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("distributionUrl="))
                {
                    if (editorSettings.overrideGradleWrapperVersion.StartsWith("http"))
                    {
                        lines[i] = editorSettings.overrideGradleWrapperVersion;
                    }
                    else
                    {
                        lines[i] = versionRegex.Replace(lines[i], editorSettings.overrideGradleWrapperVersion);
                    }
                    Utils.Log("Android Gradle Wrapper distribution url set to: " + lines[i] + "\nIn file: " + propertiesPath + Utils.logAutoFeature);
                }
            }
            File.WriteAllLines(propertiesPath, lines);
        }

        internal System.Version FindGradlePluginVersion()
        {
            if (File.Exists(Utils.projectGradlePath))
            {
                var lines = File.ReadAllLines(Utils.projectGradlePath);
                var version = FindGradlePluginVersion(lines);
                if (version != null)
                {
                    return version;
                }
            }
#if UNITY_6000_0_OR_NEWER
            return new Version(8, 3, 0);
#elif UNITY_2022_3 || UNITY_2021_3
            return new Version(7, 4, 2);
#elif UNIT_2022_2_OR_NEWER
            return new Version(7, 1, 2);
#else
            return new Version(4, 0, 1);
#endif
        }

        private System.Version FindGradlePluginVersion(string[] fileLines)
        {
            // Extracts an Android Gradle Plugin version number from the contents of a *.gradle file for
            // Unity 2022.2+ or 2023.1+.
            // Example:
            //   id 'com.android.application' version '7.1.2' apply false
            //   id 'com.android.library' version '7.1.2' apply false
            const string pluginVersionLine = "id 'com.android.application' version '";

            // Extracts an Android Gradle Plugin version number from the contents of a *.gradle file.
            // This should work for Unity 2022.1 and below.
            // Example:
            //   classpath 'com.android.tools.build:gradle:4.0.1'
            const string pluginVersionLineLegacy = "classpath 'com.android.tools.build:gradle:";

            for (int i = 0; i < fileLines.Length; i++)
            {
                if (fileLines[i].Contains(pluginVersionLine) || fileLines[i].Contains(pluginVersionLineLegacy))
                {
                    try
                    {
                        return new System.Version(versionRegex.Match(fileLines[i]).Value);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
            return null;
        }

        private void UpdateRootGradleBuild(string path, CASInitSettings initSettings, CASEditorSettings editorSettings, DependencyManager depManager)
        {
#if UNITY_2019_3_OR_NEWER
            path = Path.Combine(path, "..");
#endif
            var gradlePath = Path.Combine(path, "build.gradle");
            if (!File.Exists(gradlePath))
            {
                Debug.LogError(Utils.logTag + "Required file is missied: " + gradlePath);
                return;
            }

            var lines = File.ReadAllLines(gradlePath);
            var updated = false;

            var currVersion = FindGradlePluginVersion(lines);

            System.Version newVersion = null;
            if (!string.IsNullOrEmpty(editorSettings.overrideGradlePluginVersion))
            {
                try
                {
                    newVersion = new System.Version(editorSettings.overrideGradlePluginVersion);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            if (newVersion == null)
                newVersion = GetFixedGradlePluginVersion(currVersion);
            if (newVersion != null)
            {
                Utils.Log("Updated Android Gradle Plugin version to: " + newVersion.ToString() +
                                   " from: " + currVersion.ToString() + "\nIn file: " + gradlePath);
            }

            for (var i = 0; i < lines.Length; i++)
            {
                if (newVersion != null && lines[i].Contains(" 'com.android."))
                {
                    lines[i] = versionRegex.Replace(lines[i], newVersion.ToString());
                    updated = true;
                }
#if CAS_REMOVE_JCENTER
                else if (lines[i].Contains("jcenter()"))
                {
                    lines[i] = lines[i].Replace("jcenter()", "mavenCentral()");
                    updated = true;
                }
#endif
            }

            var linesList = new List<string>(lines);

#if CAS_FORCE_AD_ID_VERSION
            var useAdvertiserId = !editorSettings.isUseAdvertiserIdLimited(initSettings.defaultAudienceTagged);
            if ((int)PlayerSettings.Android.minSdkVersion < 26 && useAdvertiserId)
            {
                const string forceAdIdVersion = "force 'com.google.android.gms:play-services-ads-identifier:18.1.0'";
                if (AddResolutionStrategy(forceAdIdVersion, linesList))
                {
                    updated = true;
                    lines = linesList.ToArray();
                }
            }
#endif

#if CAS_DOWNGRADE_YANDEX_SDK
            var yandexDependency = depManager.Find(AdNetwork.YandexAds);
            if (yandexDependency.IsInstalled() && currVersion < new System.Version(7, 5, 0))
            {
                const string forceYandexAdsSDK = "force 'com.yandex.android:mobileads:7.9.0'";
                if (AddResolutionStrategy(forceYandexAdsSDK, linesList))
                {
                    updated = true;
                    lines = linesList.ToArray();
                }
            }
#endif

            if (updated)
                File.WriteAllLines(gradlePath, lines);
        }

        private void UpdateGradleBuild(string path)
        {
            var gradlePath = Path.Combine(path, "build.gradle");
            if (!File.Exists(gradlePath))
            {
                Debug.LogError(Utils.logTag + "Required file is missied: " + gradlePath);
                return;
            }
            var mainGradle = File.ReadAllText(gradlePath);
            var gradleChanged = false;

#if CAS_ADD_PACKAGING_OPTIONS
            const string buildOptionsFile = "cas_android_build_options.gradle";
            const string applyBuildOptions = "apply from: '" + buildOptionsFile + "'";
            if (!mainGradle.Contains(applyBuildOptions))
            {
                var optionsPath = Utils.GetPluginComponentPath(buildOptionsFile);
                if (optionsPath != null)
                {
                    File.Copy(optionsPath, Path.Combine(path, buildOptionsFile), true);
                    mainGradle += Environment.NewLine + applyBuildOptions + Environment.NewLine;
                    gradleChanged = true;
                }
            }
#endif

            if (gradleChanged)
                File.WriteAllText(gradlePath, mainGradle);
        }

        private void UpdateGradleProperties(string path)
        {
#if UNITY_2019_3_OR_NEWER
            path = Path.Combine(path, "..");
#endif
            var propertiesPath = Path.Combine(path, "gradle.properties");
            var properties = new Dictionary<string, string>();
            if (File.Exists(propertiesPath))
            {
                var file = File.ReadAllLines(propertiesPath);
                foreach (var line in file)
                {
                    var parts = line.Split('=');
                    if (parts.Length == 1)
                        properties[line] = "";
                    else
                        properties[parts[0]] = parts[1];
                }
            }

            properties["android.useAndroidX"] = "true";

            //properties["android.enableJetifier"] = "true";
#if !UNITY_2022_2_OR_NEWER
            // Unity 6 uses Gradle version 8.4, but enableDexingArtifactTransform was removed in version 8.2.
            // Enabled by default Dexing artifact transform causes issues for ExoPlayer with Gradle plugin 3.5.0+
            properties["android.enableDexingArtifactTransform"] = "false";
#endif

            var fileContent = new StringBuilder();
            foreach (var prop in properties)
            {
                fileContent.Append(prop.Key);
                if (prop.Value.Length > 0)
                    fileContent.Append('=');
                fileContent.AppendLine(prop.Value);
            }

            try
            {
                File.WriteAllText(propertiesPath, fileContent.ToString());
            }
            catch
            {
                Debug.LogError(Utils.logTag + "Failed to update file: " + propertiesPath);
            }
        }


        private void AddCASConfigResources(string path, CASInitSettings initSettings)
        {
            if (!initSettings) return;

            var rootPath = Path.Combine(path, "src/main/res/raw");
            if (!Directory.Exists(rootPath))
                Directory.CreateDirectory(rootPath);

            for (int i = 0; i < initSettings.managersCount; i++)
            {
                string cachePath = AdRemoteConfig.GetCachePath(BuildTarget.Android, initSettings.GetManagerId(i));
                if (File.Exists(cachePath))
                {
                    try
                    {
                        string fileName = AdRemoteConfig.GetResourcesFileName(initSettings.GetManagerId(i));
                        File.Copy(cachePath, Path.Combine(rootPath, fileName), true);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(Utils.logTag + "Copy Config resources to Android Project failed: " + e.ToString());
                    }
                }
                else if (initSettings.GetManagerId(i) != "demo")
                {
                    Utils.Log("Not found config file: " + cachePath);
                }
            }
        }

        private void UpdateAppManifest(string path, CASInitSettings initSettings, CASEditorSettings editorSettings, DependencyManager depManager)
        {
            string manifestPath = Path.Combine(path, "src/main/AndroidManifest.xml");
            if (!File.Exists(manifestPath))
            {
                Debug.LogError(Utils.logTag + "Application AndroidManifest.xml is missing in path: " + manifestPath);
                return;
            }

            XDocument manifest = XDocument.Load(manifestPath);
            var manifestElem = manifest.Element("manifest");
            if (manifestElem == null)
            {
                Debug.LogError(Utils.logTag + "Application AndroidManifest.xml is invalid, <manifest> tag not found.");
                return;
            }

            var appElem = manifestElem.Element("application");
            if (appElem == null)
            {
                Debug.LogError(Utils.logTag + "Application AndroidManifest.xml is invalid, <application> tag not found.");
                return;
            }

            // elemApplication.Add(new XElement("uses-library",
            //         new XAttribute(ns + "required", "false"),
            //         new XAttribute(nameAttribute, "org.apache.http.legacy")));


            const string permission_AD_ID = "com.google.android.gms.permission.AD_ID";
            XNamespace nsTools = "http://schemas.android.com/tools";
            XName toolsNode = nsTools + "node";
            var permissionAdIdElem = manifestElem.Descendants()
                .FirstOrDefault(element => element.Name.LocalName.Equals("uses-permission")
                    && IsElementName(element, permission_AD_ID));
            if (permissionAdIdElem == null)
            {
                permissionAdIdElem = new XElement("uses-permission",
                    new XAttribute(androidNamespace + "name", permission_AD_ID));
                manifestElem.Add(permissionAdIdElem);
            }

            if (editorSettings.isUseAdvertiserIdLimited(initSettings.defaultAudienceTagged))
            {
                manifestElem.SetAttributeValue(XNamespace.Xmlns + "tools", nsTools);
                permissionAdIdElem.SetAttributeValue(toolsNode, "remove");
            }
            else
            {
                permissionAdIdElem.SetAttributeValue(toolsNode, null);
            }

            var metaDataElems = appElem.Descendants()
                .Where(element => element.Name.LocalName.Equals("meta-data"));

            var googleAppId = AdRemoteConfig.FindGADAppId(initSettings, depManager);
            if (!string.IsNullOrEmpty(googleAppId))
            {
                UpdateMetaData(appElem, metaDataElems, "com.google.android.gms.ads.APPLICATION_ID", googleAppId);
            }
            manifest.Save(manifestPath);
        }

        private bool IsElementName(XElement element, string name)
        {
            var attribute = element.Attribute(androidNamespace + "name");
            return attribute != null && attribute.Value == name;
        }

        private void UpdateMetaData(XElement appElem, IEnumerable<XElement> metaDataElements, string name, string value)
        {
            foreach (var element in metaDataElements)
            {
                if (IsElementName(element, name))
                {
                    element.SetAttributeValue(androidNamespace + "value", value);
                    return;
                }
            }
            appElem.Add(
                new XElement("meta-data",
                    new XAttribute(androidNamespace + "name", name),
                    new XAttribute(androidNamespace + "value", value))
            );
        }

        private bool AddResolutionStrategy(string stategyLine, List<string> lines)
        {
            var lineNotFound = true;
            var blockLineNum = -1;
            for (var i = 0; lineNotFound && i < lines.Count; i++)
            {
                if (lines[i].Contains("resolutionStrategy"))
                    blockLineNum = i + 1;
                else if (lines[i].Contains(stategyLine))
                    lineNotFound = false;
            }
            if (lineNotFound)
            {
                if (blockLineNum > 0)
                {
                    lines.Insert(blockLineNum, "          " + stategyLine);
                }
                else
                {
                    lines.AddRange(new string[] {
                            "",
                            "// The CAS Unity Plugin adds the following resolution strategy",
                            "allprojects {",
                            "    configurations.all {",
                            "        resolutionStrategy {",
                            "          " + stategyLine,
                            "        }",
                            "    }",
                            "}"
                        });
                }
                return true;
            }
            return false;
        }

        private Version GetFixedGradlePluginVersion(Version current)
        {
#if CAS_FIX_AGP_MINOR_VERSION || CASDeveloper
            Version target = null;

            if (current.Major == 4)
            {
                switch (current.Minor)
                {
                    case 0:
                    case 2:
                        if (current.Build < 2)
                            target = new Version(4, current.Minor, 2);
                        break;
                    case 1:
                        if (current.Build < 3)
                            target = new Version(4, 1, 3);
                        break;
                }
            }
            else if (current.Major == 3)
            {
                switch (current.Minor)
                {
                    case 3:
                    case 4:
                        target = new Version(3, current.Minor, 3);
                        break;
                    case 5:
                    case 6:
                        target = new Version(3, current.Minor, 4);
                        break;
                }
            }

            if (target != null)
            {
                if (current.Major != target.Major
                    || current.Minor != target.Minor
                    || current.Build < target.Build)
                {
                    return target;
                }
            }
#endif
            return null;
        }
    }
}

#endif
