//  Copyright © 2024 CAS.AI. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CAS
{
    /// <summary>
    /// Please use <see cref="IManagerBuilder"/> instead.
    /// CASInitSettings may not be available in future.
    /// </summary>
    [Serializable]
    public class CASInitSettings : ScriptableObject, IManagerBuilder
    {
        #region Fields
#pragma warning disable 649 // is never assigned to, and will always have its default value null
        [SerializeField]
        private bool testAdMode = false;
        [SerializeField]
        private string[] managerIds = { "demo" };
        public AdFlags allowedAdFlags = AdFlags.None;
        [SerializeField]
        private Audience audienceTagged = Audience.Children;
        [Obsolete("No longer supported")]
        public AdSize bannerSize = 0;

        [SerializeField]
        private int bannerRefresh = 30;
        [SerializeField]
        private int interstitialInterval = 30;
        [SerializeField]
        private LoadingManagerMode loadingMode = LoadingManagerMode.Optimal;
        [SerializeField]
        private bool debugMode;
        [SerializeField]
        private bool trackLocationEnabled;
        [SerializeField]
        private bool interWhenNoRewardedAd = true;

        internal string targetId;
        internal string userID;
        internal Dictionary<string, string> extras;
        internal InitCompleteAction initListenerDeprecated;
        internal CASInitCompleteEvent initListener;
        internal ConsentFlow consentFlow;
#pragma warning restore 649
        #endregion

        public IMediationManager Initialize()
        {
            return Build();
        }


        public IMediationManager Build()
        {
            if (string.IsNullOrEmpty(targetId))
                WithCASId(0);
            return CASFactory.CreateManager(this);
        }

        #region Initialization options
        public IManagerBuilder WithManagerIdAtIndex(int index)
        {
            return WithCASId(index);
        }

        public IManagerBuilder WithCASId(int index)
        {
            if (index < 0 || managerIds == null || managerIds.Length - 1 < index
                || string.IsNullOrEmpty(managerIds[index]))
                throw new ArgumentNullException("index", "Manager ID is empty. " +
                    "Please use Assets/CleverAdsSolutions/Settings menu and set manager ID.");
            targetId = managerIds[index];
            return this;
        }

        public IManagerBuilder WithInitListener(InitCompleteAction listener)
        {
            initListenerDeprecated = listener;
            return this;
        }
        
        public IManagerBuilder WithCompletionListener(CASInitCompleteEvent listener)
        {
            initListener = listener;
            return this;
        }

        public IManagerBuilder WithEnabledAdTypes(params AdFlags[] adTypes)
        {
            allowedAdFlags = AdFlags.None;
            for (int i = 0; i < adTypes.Length; i++)
                allowedAdFlags |= adTypes[i];
            return this;
        }

        public IManagerBuilder WithUserID(string userID)
        {
            this.userID = userID;
            return this;
        }

        public IManagerBuilder WithConsentFlow(ConsentFlow flow)
        {
            consentFlow = flow;
            return this;
        }

        public IManagerBuilder WithMediationExtras(string key, string value)
        {
            if (extras == null)
                extras = new Dictionary<string, string>();
            extras[key] = value;
            return this;
        }

        public IManagerBuilder ClearMediationExtras()
        {
            if (extras != null)
                extras.Clear();
            return this;
        }

        public IManagerBuilder WithManagerId(string managerId)
        {
            return WithCASId(managerId);
        }

        public IManagerBuilder WithCASId(string casId)
        {
            if (string.IsNullOrEmpty(casId))
                throw new ArgumentNullException("managerId", "Manager ID is empty");
            targetId = casId;
            return this;
        }

        [Obsolete("Please set Test Ad Mode in `Assets>CleverAdsSolutions>Settings` menu to get true Test Ad, " +
            "also Development build work same.")]
        public IManagerBuilder WithTestAdMode(bool test)
        {
            testAdMode = test;
            return this;
        }
        #endregion

        #region Settings getters
        public AdFlags defaultAllowedFormats { get { return allowedAdFlags; } }
        public Audience defaultAudienceTagged { get { return audienceTagged; } }
        public int defaultBannerRefresh { get { return bannerRefresh; } }
        public int defaultInterstitialInterval { get { return interstitialInterval; } }
        public LoadingManagerMode defaultLoadingMode { get { return loadingMode; } }
        public bool defaultDebugModeEnabled { get { return debugMode; } }
        public bool defaultIOSTrackLocationEnabled { get { return trackLocationEnabled; } }
        public bool defaultInterstitialWhenNoRewardedAd { get { return interWhenNoRewardedAd; } }

        public int managersCount { get { return managerIds == null ? 0 : managerIds.Length; } }

        public string GetManagerId(int index)
        {
            return managerIds[index];
        }

        public int IndexOfManagerId(string id)
        {
            if (managerIds == null || id == null)
                return -1;
            return Array.IndexOf(managerIds, id);
        }

        public bool IsTestAdMode()
        {
#if UNITY_EDITOR
            return testAdMode || UnityEditor.EditorUserBuildSettings.development;
#else
            return testAdMode || Debug.isDebugBuild;
#endif
        }
        #endregion
    }
}