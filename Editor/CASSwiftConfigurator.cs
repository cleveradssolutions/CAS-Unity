//  Copyright © 2025 CAS.AI. All rights reserved.

#if UNITY_IOS || CASDeveloper
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CAS.UEditor
{
    using Utils = CASEditorUtils;

    internal static class CASSwiftConfigurator
    {
        private const string binaryName = "cas-configure";

        [Serializable]
        private class Config
        {
            public string gadApplicationIdentifier;
            public string userTrackingUsageDescription;
            public string attributionReportEndpoint;
            public bool delayAppMeasurementInit;
            public string skAdNetworkIdsFile;
            public string adAttributionKitIdsFile;
        }

        internal static bool Configure(string buildPath, CASInitSettings initSettings,
            CASEditorSettings editorSettings, DependencyManager depManager)
        {
            var binaryPath = Utils.GetPluginComponentPath(binaryName);
            if (string.IsNullOrEmpty(binaryPath))
            {
                Debug.LogError(Utils.logTag + "cas-configure binary not found in the plugin.");
                return false;
            }

            var config = new Config
            {
                gadApplicationIdentifier = AdRemoteConfig.FindGADAppId(initSettings, depManager),
                userTrackingUsageDescription = editorSettings.userTrackingUsageDescription.Length > 0
                    ? editorSettings.userTrackingUsageDescription[0].value : null,
                attributionReportEndpoint = editorSettings.attributionReportEndpoint,
                delayAppMeasurementInit = true,
                skAdNetworkIdsFile = Utils.GetPluginComponentPath("CASSKAdNetworks.txt"),
                adAttributionKitIdsFile = Utils.GetPluginComponentPath("CASAttAdNetworks.txt"),
            };

            var configPath = Path.Combine(buildPath, "cas-configure.json");
            File.WriteAllText(configPath, JsonUtility.ToJson(config));
            var plistPath = Path.Combine(buildPath, "Info.plist");

            try
            {
                MakeExecutable(binaryPath);
                var exitCode = Run(binaryPath,
                    "--plist \"" + plistPath + "\" --config \"" + configPath + "\"");
                return exitCode == 0;
            }
            catch (Exception e)
            {
                Debug.LogError(Utils.logTag + "cas-configure failed: " + e.Message);
                return false;
            }
            finally
            {
                if (File.Exists(configPath))
                    File.Delete(configPath);
            }
        }

        private static void MakeExecutable(string path)
        {
            // Unity may drop the executable bit on imported assets.
            Run("/bin/chmod", "+x \"" + path + "\"");
        }

        private static int Run(string fileName, string arguments)
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (!string.IsNullOrEmpty(output))
                Utils.Log(output.Trim());
            if (process.ExitCode != 0 && !string.IsNullOrEmpty(error))
                Debug.LogError(Utils.logTag + error.Trim());
            return process.ExitCode;
        }
    }
}
#endif
