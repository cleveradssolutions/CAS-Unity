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
            if (target == null)
                return "";

            var networks = Enum.GetValues( typeof( AdNetwork ) );
            var result = new char[networks.Length];
            for (int i = 0; i < networks.Length; i++)
            {
                var dependency = target.Find( ( ( AdNetwork )networks.GetValue( i ) ).ToString() );
                result[i] = ( dependency != null && dependency.isInstalled() ) ? '1' : '0';
            }
            return new string( result );
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
            return Find( Utils.promoDependency );
        }
    }



    [Serializable]
    public partial class Dependency
    {
        [Flags]
        public enum Label
        {
            None = 0,
            Banner = 1,
            Inter = 2,
            Reward = 4,
            Native = 8,
            Beta = 16,
        }

        public string name;
        public string version;
        public string require;
        public string url;
        public int filter;
        public string[] dependencies;
        public string[] contains;
        public string[] source;
        public Label labels = Label.Banner | Label.Inter | Label.Reward;

        public string installedVersion { get; set; }
        public bool isNewer { get; set; }
        public bool isRequired { get; set; }
        public bool locked { get; set; }
        public bool inBan { get; set; }

        public Dependency() { }

        public Dependency( string name, string url, string version, int filter, string require, params string[] dependencies )
        {
            this.name = name;
            this.dependencies = dependencies;
            this.version = version;
            this.url = url;
            this.filter = filter;
            this.require = require;
        }

        public bool isInstalled()
        {
            return isRequired || locked || !string.IsNullOrEmpty( installedVersion );
        }
    }
}