using System;
using UnityEngine;

namespace CAS
{
    [Serializable]
    public class CASInitSettings : ScriptableObject
    {
        public bool testAdMode = false;
        public string[] managerIds;
        public AdFlags allowedAdFlags = AdFlags.Everything;
        public Audience audienceTagged = Audience.Children;
        public AdSize bannerSize = AdSize.Banner;
        public int bannerRefresh = 30;
        public int interstitialInterval = 30;
        public LoadingManagerMode loadingMode = LoadingManagerMode.Optimal;
        public bool debugMode;
        public string trackingUsageDescription;
        public bool trackLocationEnabled;
    }
}