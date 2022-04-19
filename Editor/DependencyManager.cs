//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2021 CleverAdsSolutions. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Utils = CAS.UEditor.CASEditorUtils;

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
            get { return advanced; }
        }

        public static DependencyManager Create( BuildTarget platform, Audience audience, bool deepInit )
        {
            string listPath = Utils.GetTemplatePath( "CAS" + platform.ToString() + "Mediation.list" );
            if (listPath == null)
                return null;

            var mediation = JsonUtility.FromJson<DependencyManager>( File.ReadAllText( listPath ) );
            mediation.Init( platform, deepInit );
            mediation.SetAudience( audience );
            return mediation;
        }

        public static DependencyManager Create( Dependency[] simple, Dependency[] advanced )
        {
            return new DependencyManager()
            {
                simple = simple,
                advanced = advanced
            };
        }

        // Reflection target
        public static string GetActiveMediationPattern()
        {
            var target = Create( EditorUserBuildSettings.activeBuildTarget, Audience.Mixed, true );
            return GetActiveMediationPattern( target );
        }

        public static string GetActiveMediationPattern( DependencyManager manager )
        {
            if (manager == null)
                return "";

            var networks = Enum.GetValues( typeof( AdNetwork ) );
            var result = new char[networks.Length];
            for (int i = 0; i < networks.Length; i++)
            {
                var dependency = manager.Find( ( AdNetwork )networks.GetValue( i ) );
                result[i] = ( dependency != null && dependency.IsInstalled() ) ? dependency.mediation : '0';
            }
            return new string( result );
        }

        public Dependency Find( AdNetwork network )
        {
            if (network == Dependency.noNetwork)
                return null;
            if (network == AdNetwork.MAX)
                network = AdNetwork.AppLovin;
            return Find( network.GetName() );
        }

        public Dependency Find( string name )
        {
            for (int i = 0; i < simple.Length; i++)
            {
                if (simple[i].name == name)
                    return simple[i];
            }
            for (int i = 0; i < advanced.Length; i++)
            {
                if (advanced[i].name == name)
                    return advanced[i];
            }
            return null;
        }

        public Dependency FindCrossPromotion()
        {
            return Find( AdNetwork.CrossPromotion.GetName() );
        }
    }

    [Serializable]
    public partial class Dependency
    {
        public const AdNetwork adBase = ( AdNetwork )64;
        public const AdNetwork noNetwork = ( AdNetwork )65;
        public const string adBaseName = "Base";
        public const string adOptimalName = "OptimalAds";
        public const string adFamiliesName = "FamiliesAds";

        [Flags]
        public enum Label
        {
            None = 0,
            Banner = 1,
            Inter = 2,
            Reward = 4,
            Native = 8,
            Beta = 16,
            Obsolete = 32
        }

        [Serializable]
        public class SDK
        {
            public string name;
            public string version;
            public bool addToAllTargets;

            public SDK( string name, string version, bool addToAllTargets = false )
            {
                this.name = name;
                this.version = version;
                this.addToAllTargets = addToAllTargets;
            }
        }

        public string name;
        public string altName = string.Empty;
        public string version;
        public AdNetwork require = noNetwork;
        public string url;
        public int filter;
        public string[] dependencies = new string[0];
        public List<SDK> depsSDK = new List<SDK>();
        public AdNetwork[] contains = new AdNetwork[0];
        public string[] source;
        public string comment;
        public Label labels = Label.Banner | Label.Inter | Label.Reward;
        public string embedFramework;
        public char mediation = '1';

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

        [Obsolete( "Renamed to `notSupported`" )]
        public bool inBan { get; set; }

        public Dependency() { }

        public Dependency( AdNetwork network )
        {
            this.name = network.GetName();
        }

        public Dependency( AdNetwork network, string url, string version, int filter, AdNetwork? require, params string[] dependencies )
        {
            this.name = network.GetName();
            this.dependencies = dependencies;
            this.version = version;
            this.url = url;
            this.filter = filter;
            this.require = require ?? noNetwork;
        }

        public Dependency( string name, string url, string version, int filter, AdNetwork? require, params string[] dependencies )
        {
            this.name = name;
            this.dependencies = dependencies;
            this.version = version;
            this.url = url;
            this.filter = filter;
            this.require = require ?? noNetwork;
        }

        public bool IsInstalled()
        {
            return locked || !string.IsNullOrEmpty( installedVersion );
        }

        [Obsolete( "Renamed to IsInstalled()" )]
        public bool isInstalled()
        {
            return IsInstalled();
        }
    }
}