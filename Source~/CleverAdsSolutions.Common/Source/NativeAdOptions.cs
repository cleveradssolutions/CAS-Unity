using System;
using UnityEngine;

namespace CAS
{
    /// <summary>
    /// This determines where the AdChoices icon is displayed within the ad content.
    /// </summary>
    [Serializable]
    public enum AdChoicesPlacement
    {
        TopLeft = 0,
        TopRight = 1,
        BottomRight = 2,
        BottomLeft = 3,
    }

    /// <summary>
    /// Manages the loading of native ads with specific configurations.
    /// </summary>
    [Serializable]
    public class NativeAdOptions
    {
        /// <summary>
        /// This determines where the AdChoices icon is displayed within the ad content.
        /// Use this property to configure the placement according to your app's design and ad requirements.
        /// The icon placement is applied only before ad load.
        /// </summary>
        public AdChoicesPlacement adChoicesPlacement = AdChoicesPlacement.TopRight;

        /// <summary>
        /// Sets the initial mute state of the video.
        /// By default the video will start with the audio muted.
        /// The mute state is applied only before ad load.
        /// </summary>
        public bool isStartVideoMuted = true;

        /// <summary>
        /// An optional placement name for the ad instance that helps categorize
        /// and track statistics across different ad placements.
        /// The placement name is applied only before ad load.
        /// Maximum 100 characters allowed for the placement name.
        /// </summary>
        public string placement = null;
    }
}