//  Copyright © 2024 CAS.AI. All rights reserved.

namespace CAS
{
    public enum AdError
    {
        Internal = 0,
        /// <summary>
        /// Loading ads cannot be successful without an internet connection.
        /// </summary>
        NoConnection = 2,
        /// <summary>
        /// This means we are not able to serve ads to this person.
        /// 
        /// <para>Note that if you can see ads while you are testing with enabled <see cref="IMediationManager.isTestAdMode"/>,
        /// your implementation works correctly and people will be able to see ads in your app once it's live.</para>
        /// </summary>
        NoFill = 3,
        /// <summary>
        /// A configuration error has been detected in one of the mediation ad networks.
        /// Please report error message to your manager support.
        /// </summary>
        Configuration = 6,
        /// <summary>
        /// Ad are not ready to show.
        /// You need to call Load ads or use one of the automatic cache mode.
        /// 
        /// <para>If you are already using automatic cache mode then just wait a little longer.</para>
        /// <para>You can always check if ad is ready to show
        /// using <see cref="IMediationManager.IsReadyAd(AdType)"/> or <see cref="IAdView.isReady"/> methods.</para>
        /// </summary>
        NotReady = 1001,
        /// <summary>
        /// The manager you want to use is not active at the moment.
        /// 
        /// <para>To change the state of the manager, use <see cref="IMediationManager.SetEnableAd(AdType, bool)"/> method.</para>
        /// </summary>
        ManagerIsDisabled = 1002,
        /// <summary>
        /// Ad creative has reached its daily cap for user.
        /// <para>The reason is for cross promo only.</para>
        /// </summary>
        ReachedCap = 1004,
        [System.Obsolete("The error reason no longer used")]
        NotEnoughSpace = 1005,
        /// <summary>
        /// The interval between impressions of Interstitial Ad has not yet passed.
        /// <para>To change the interval, use <see cref="IAdsSettings.interstitialInterval"/> property.</para>
        /// </summary>
        IntervalNotYetPassed = 2001,
        /// <summary>
        /// You can not show ads because another fullscreen ad is being displayed at the moment.
        /// <para>Please check your ad call logic to eliminate duplicate impressions.</para>
        /// </summary>
        AlreadyDisplayed = 2002,
        /// <summary>
        /// Ads cannot be shown as the application is currently not visible to the user.
        /// </summary>
        AppIsPaused = 2003
    }

    public static class AdErrorExtension
    {
        public static string GetMessage( this AdError error )
        {
            switch (error)
            {
                case AdError.NoConnection: return "No internet connection detected";
                case AdError.NoFill: return "No Fill";
                case AdError.Configuration: return "Invalid configuration";
                case AdError.NotReady: return "Ad are not ready";
                case AdError.ManagerIsDisabled: return "Manager is disabled";
                case AdError.ReachedCap: return "Reached cap for user";
                case AdError.IntervalNotYetPassed: return "The interval between Ad impressions has not yet passed";
                case AdError.AlreadyDisplayed: return "Ad already displayed";
                case AdError.AppIsPaused: return "Application is paused";
                default: return "Internal error";
            }
        }
    }
}