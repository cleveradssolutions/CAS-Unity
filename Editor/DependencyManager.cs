//  Copyright © 2024 CAS.AI. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CAS.UEditor
{
    [Serializable]
    public partial class DependencyManager
    {
        public Dependency[] solutions
        {
            get { return simple; }
        }

        public Dependency[] networks
        {
            get { return adapters; }
        }

        public static DependencyManager Create(BuildTarget platform, Audience audience, bool deepInit)
        {
            string listPath = CASEditorUtils.GetPluginComponentPath("CAS" + platform.ToString() + "Mediation.list");
            if (listPath == null)
                return null;

            var mediation = JsonUtility.FromJson<DependencyManager>(File.ReadAllText(listPath));
            mediation.Init(platform, deepInit);
            mediation.SetAudience(audience);
            return mediation;
        }

        public static DependencyManager Create(Dependency[] solutions, Dependency[] adapters)
        {
            return Create(solutions, adapters, new string[0]);
        }

        public static DependencyManager Create(Dependency[] solutions, Dependency[] adapters, string[] deprecated)
        {
            return new DependencyManager()
            {
                version = MobileAds.wrapperVersion,
                simple = solutions,
                adapters = adapters,
                deprecated = deprecated
            };
        }

        // Reflection target
        [UnityEngine.Scripting.Preserve]
        public static string GetActiveMediationPattern()
        {
            var target = Create(EditorUserBuildSettings.activeBuildTarget, Audience.Mixed, true);
            return GetActiveMediationPattern(target);
        }

        public static string GetActiveMediationPattern(DependencyManager manager, int size = 25)
        {
            if (manager == null)
                return "";

            var result = new char[size];
            for (int i = 0; i < size; i++)
            {
                var dependency = manager.Find((AdNetwork)i);
                result[i] = (dependency != null && dependency.IsInstalled()) ? '1' : '0';
            }
            return new string(result);
        }

        public Dependency Find(AdNetwork network)
        {
            if (network == Dependency.noNetwork)
                return null;
            if (network == Dependency.adBase)
            {
                var dep = new Dependency();
                dep.id = Dependency.adBase;
                dep.version = Dependency.FindInstalledVersion(Dependency.adBaseName, platform);
                dep.installedVersion = dep.version;
                return dep;
            }
            for (int i = 0; i < solutions.Length; i++)
            {
                if (solutions[i].id == network)
                    return solutions[i];
            }
            for (int i = 0; i < adapters.Length; i++)
            {
                if (adapters[i].id == network)
                    return adapters[i];
            }
            return null;
        }

        public void UpdateDependencies()
        {
            for (int i = 0; i < solutions.Length; i++)
            {
                if (solutions[i].isNewer)
                    solutions[i].ActivateDependencies(platform, this);
            }

            for (int i = 0; i < adapters.Length; i++)
            {
                if (adapters[i].isNewer)
                    adapters[i].ActivateDependencies(platform, this);
            }
        }

        [Obsolete("Use UpdateDependencies() without build target instead")]
        public void UpdateDependencies(BuildTarget platform)
        {
            UpdateDependencies();
        }

        public string GetInstalledVersion()
        {
            string version = "";
            var casDep = Find(Dependency.adBase);
            if (casDep != null)
                version = casDep.version;
            if (!string.IsNullOrEmpty(version))
                return version;

            casDep = Find(Dependency.adsOptimal);
            if (casDep != null)
                version = casDep.installedVersion;
            if (!string.IsNullOrEmpty(version))
                return version;

            casDep = Find(Dependency.adsFamilies);
            if (casDep != null)
                version = casDep.installedVersion;

            return version;
        }

        public int GetInstalledBuildCode()
        {
            var version = GetInstalledVersion();
            if (!string.IsNullOrEmpty(version))
            {
                try
                {
                    var parsesV = new System.Version(version);
                    return parsesV.Major * 1000 + parsesV.Minor * 100 + parsesV.Build;
                }
                catch { }
            }
            return 0;
        }

        public bool IsNewerVersionFound()
        {
            for (int i = 0; i < simple.Length; i++)
            {
                if (simple[i].isNewer)
                    return true;
            }
            for (int i = 0; i < adapters.Length; i++)
            {
                if (adapters[i].isNewer)
                    return true;
            }
            return false;
        }

        public Dependency FindCrossPromotion()
        {
            return Find(AdNetwork.CrossPromotion);
        }
    }

    [Serializable]
    public partial class Dependency
    {
        public const AdNetwork adsOptimal = (AdNetwork)62;
        public const AdNetwork adsFamilies = (AdNetwork)63;
        public const AdNetwork adBase = (AdNetwork)64;
        public const AdNetwork noNetwork = (AdNetwork)65;
        public const string adBaseName = "Base";
        public const string adsOptimalName = "OptimalAds";
        public const string adsFamiliesName = "FamiliesAds";

        [Flags]
        public enum Label
        {
            None = 0,
            Banner = 1,
            Inter = 2,
            Reward = 4,
            Native = 8,
            Beta = 16,
            AppOpen = 32
        }

        public enum Filter
        {
            None = -1,
            Adult = 0,
            Any = 1,
            Children = 2,
        }

        [Serializable]
        public class SDK
        {
            public string name;
            public string version;
            public bool embed;

            public SDK(string name, string version, bool addToAllTargets = false)
            {
                this.name = name;
                this.version = version;
                embed = addToAllTargets;
            }
        }

        public AdNetwork id;
        public string name;
        public string altName = string.Empty;
        public string version = string.Empty;
        public AdNetwork require = noNetwork;
        public Filter filter;
        public SDK[] libs = new SDK[0];
        public string[] embedPath = new string[0];
        public AdNetwork[] contains = new AdNetwork[0];
        public string source = string.Empty;
        public string comment;
        public Label labels = Label.Banner | Label.Inter | Label.Reward;

        public string installedVersion { get; set; }
        public bool isNewer { get; set; }
        /// <summary>
        /// Is required for another dependency
        /// </summary>
        public bool isRequired { get; set; }
        /// <summary>
        /// Has installed from another dependency
        /// </summary>
        public bool locked { get; set; }
        /// <summary>
        /// Not supported for current configuration
        /// </summary>
        public bool notSupported { get; set; }

        public Dependency() { }

        public bool IsInstalled()
        {
            return locked || !string.IsNullOrEmpty(installedVersion);
        }
    }
}