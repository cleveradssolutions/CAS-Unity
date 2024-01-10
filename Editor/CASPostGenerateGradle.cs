//  Copyright © 2024 CAS.AI. All rights reserved.

#if UNITY_ANDROID || CASDeveloper

//#define CAS_DISABLE_PACKAGING_OPTIONS
//#define CAS_DISABLE_VALIDATE_DEPS

using System;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEditor.Android;
using UnityEditor;
using Utils = CAS.UEditor.CASEditorUtils;

namespace CAS.UEditor
{
    public class CASPostGenerateGradle : IPostGenerateGradleAndroidProject
    {
        private const string applyFromPlugin = "apply from: 'CASPlugin.androidlib/";
        private const string applyPackagingOptions = applyFromPlugin + "packaging_options.gradle'";
        private const string applyValidateDependencies = applyFromPlugin + "validate_dependencies.gradle'";

        public int callbackOrder { get { return 0; } }

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            // Path is 'root/unityLibrary/' with 'build.gradle' for Unity 2019.3+
            // Path is 'root/' with 'build.gradle' for Unity 2018

            var pluginPath = Path.Combine(path, Utils.androidLibName);
            var initSettings = Utils.GetSettingsAsset(BuildTarget.Android, false);
            var editorSettings = CASEditorSettings.Load();
            var depManager = DependencyManager.Create(BuildTarget.Android, Audience.Mixed, true);

            var gradlePath = Path.Combine(path, "build.gradle");
            var mainGradle = File.ReadAllText(gradlePath);
            var gradleChanged = false;
#if !CAS_DISABLE_VALIDATE_DEPS
            if (IsNeedApplyGAMDependenciesValidation(mainGradle))
            {
                mainGradle += Environment.NewLine + "gradle.projectsEvaluated {" + Environment.NewLine;
                mainGradle += "    " + applyValidateDependencies + Environment.NewLine;
                mainGradle += "}" + Environment.NewLine;
                gradleChanged = true;
            }
#endif

#if !CAS_DISABLE_PACKAGING_OPTIONS
            if (!mainGradle.Contains(applyPackagingOptions))
            {
                mainGradle += Environment.NewLine + applyPackagingOptions + Environment.NewLine;
                gradleChanged = true;
            }
#endif

            if (gradleChanged)
                File.WriteAllText(gradlePath, mainGradle);


            AddCASConfigResources(pluginPath, initSettings);
            UpdatePluginManifest(pluginPath, initSettings, editorSettings, depManager);
        }

        /// <summary>
        /// Include `validate_dependencies.gradle` to build tasks 
        /// if Android Gradle Plugin version is lower then 4.2.2
        /// </summary>
        private bool IsNeedApplyGAMDependenciesValidation(string mainGradle)
        {
            var gradleVer = CASPreprocessGradle.GetAndroidGradlePluginVersion();
            var isSupportNewGradleTag = gradleVer.Major > 4
                    || (gradleVer.Major == 4 && gradleVer.Minor >= 2 && gradleVer.Build >= 2);

            return !isSupportNewGradleTag && !mainGradle.Contains(applyValidateDependencies);
        }

        private void AddCASConfigResources(string pluginPath, CASInitSettings initSettings)
        {
            if (!initSettings) return;

            var rootPath = Path.Combine(pluginPath, "res/raw");
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
                else
                {
                    Utils.Log("Not found config file: " + cachePath);
                }
            }
        }

        private static void UpdatePluginManifest(string pluginPath, CASInitSettings initSettings, CASEditorSettings editorSettings, DependencyManager depManager)
        {
            string manifestPath = Path.Combine(pluginPath, "AndroidManifest.xml");
            if (!File.Exists(manifestPath))
            {
                Debug.LogError(Utils.logTag + "Plugin AndroidManifest.xml is missing. Try re-importing the plugin.");
                return;
            }

            try
            {
                XNamespace ns = "http://schemas.android.com/apk/res/android";
                XNamespace nsTools = "http://schemas.android.com/tools";
                XName nameAttribute = ns + "name";
                XName valueAttribute = ns + "value";

                var document = new XDocument(
                    new XDeclaration("1.0", "utf-8", null),
                    new XComment("This file is automatically generated by CAS Unity plugin from `Assets > CleverAdsSolutions > Android Settings`"),
                    new XComment("Do not modify this file. YOUR CHANGES WILL BE ERASED!"));
                var elemManifest = new XElement("manifest",
                    new XAttribute(XNamespace.Xmlns + "android", ns),
                    new XAttribute(XNamespace.Xmlns + "tools", nsTools),
                    new XAttribute("package", Utils.packageName),
                    new XAttribute(ns + "versionName", MobileAds.wrapperVersion),
                    new XAttribute(ns + "versionCode", 1));
                document.Add(elemManifest);

                var elemApplication = new XElement("application");

                var googleAppId = AdRemoteConfig.FindGADAppId(initSettings, depManager);
                if (!string.IsNullOrEmpty(googleAppId))
                {
                    elemApplication.Add(new XElement("meta-data",
                            new XAttribute(nameAttribute, "com.google.android.gms.ads.APPLICATION_ID"),
                            new XAttribute(valueAttribute, googleAppId)));
                }

                if (editorSettings.delayAppMeasurementGADInit)
                {
                    elemApplication.Add(new XElement("meta-data",
                        new XAttribute(nameAttribute, "com.google.android.gms.ads.DELAY_APP_MEASUREMENT_INIT"),
                        new XAttribute(valueAttribute, "true")));
                }

                if (editorSettings.optimizeGADLoading)
                {
                    elemApplication.Add(new XElement("meta-data",
                        new XAttribute(nameAttribute, "com.google.android.gms.ads.flag.OPTIMIZE_INITIALIZATION"),
                        new XAttribute(valueAttribute, "true")));
                    elemApplication.Add(new XElement("meta-data",
                        new XAttribute(nameAttribute, "com.google.android.gms.ads.flag.OPTIMIZE_AD_LOADING"),
                        new XAttribute(valueAttribute, "true")));
                }

                elemApplication.Add(new XElement("uses-library",
                    new XAttribute(ns + "required", "false"),
                    new XAttribute(nameAttribute, "org.apache.http.legacy")));

                elemManifest.Add(elemApplication);

                var elemAdIDPermission = new XElement("uses-permission",
                    new XAttribute(nameAttribute, "com.google.android.gms.permission.AD_ID"));
                if (editorSettings.isUseAdvertiserIdLimited(initSettings.defaultAudienceTagged))
                    elemAdIDPermission.SetAttributeValue(nsTools + "node", "remove");
                elemManifest.Add(elemAdIDPermission);

                // XDocument required absolute path
                document.Save(manifestPath);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}

#endif