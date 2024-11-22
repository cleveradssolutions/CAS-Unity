//  Copyright © 2024 CAS.AI. All rights reserved.

using System;
using UnityEngine;

namespace CAS
{
    public struct AdError
    {
        public const int Internal = 0;
        /// <summary>
        /// Ad are not ready to show. 
        /// You need to call Load ads or use one of the automatic cache mode. 
        /// If you are already using automatic cache mode then just wait a little longer.
        /// </summary>
        public const int NotReady = 1;
        /// <summary>
        /// Indicates that the device is rejected for services.
        /// <para>Services may not be available for some devices that do not meet the requirements.
        /// For example, the country or version of the OS.</para>
        /// </summary>
        public const int Rejected = 2;
        /// <summary>
        /// This means we are not able to serve ads to this person.
        /// 
        /// <para>Note that if you can see ads while you are testing with enabled <see cref="IMediationManager.isTestAdMode"/>,
        /// your implementation works correctly and people will be able to see ads in your app once it's live.</para>
        /// </summary>
        public const int NoFill = 3;
        /// <summary>
        /// Ad creative has reached its daily cap for user.
        /// <para>The reason is for cross promo only.</para>
        /// </summary>
        public const int ReachedCap = 6;
        /// <summary>
        /// The CAS is not initialized at the moment.
        /// </summary>
        public const int NotInitialized = 7;
        /// <summary>
        /// The Timeout error occurs when an advertising source fails to respond within a specified timeframe.
        /// Following this error, the system continues to await a response from the source,
        /// potentially resulting in delayed ad loading or triggering a genuine loading error.
        /// </summary>
        public const int Timeout = 8;
        /// <summary>
        /// Loading ads cannot be successful without an internet connection.
        /// </summary>
        public const int NoConnection = 9;

        /// <summary>
        /// A configuration error has been detected in one of the mediation ad sources.
        /// Please report error message to your manager support.
        /// </summary>
        public const int Configuration = 10;
        /// <summary>
        /// The interval between impressions of Interstitial Ad has not yet passed.
        /// <para>To change the interval, use <see cref="IAdsSettings.interstitialInterval"/> property.</para>
        /// </summary>
        public const int NotPassedInterval = 11;
        /// <summary>
        /// You can not show ads because another fullscreen ad is being displayed at the moment.
        /// <para>Please check your ad call logic to eliminate duplicate impressions.</para>
        /// </summary>
        public const int AlreadyDisplayed = 12;
        /// <summary>
        /// Ads cannot be shown as the application is currently not visible to the user.
        /// </summary>
        public const int NotForeground = 13;

        /// <summary>
        /// The ad format you want to use is not active at the moment.
        /// 
        /// <para>To change the state of the manager, use <see cref="IMediationManager.SetEnableAd(AdType, bool)"/> method.</para>
        /// </summary>
        public const int ManagerIsDisabled = 15;


        private readonly int code;
        private readonly string message;

        public AdError(int code, string message)
        {
            this.code = code;
            this.message = message;
        }

        public int GetCode()
        {
            return code;
        }

        public string GetMessage()
        {
            return ToString();
        }

        public override string ToString()
        {
            if (message != null) return message;
            switch (code)
            {
                case AdError.NoConnection: return "No internet connection";
                case AdError.NoFill: return "No Fill";
                case AdError.Configuration: return "Invalid configuration";
                case AdError.NotReady: return "Ad are not ready";
                case AdError.Timeout: return "Timeout";
                case AdError.NotInitialized: return "Not initialized";
                case AdError.ManagerIsDisabled: return "Ad format is disabled";
                case AdError.ReachedCap: return "Reached cap for user";
                case AdError.NotPassedInterval: return "Interval has not yet passed";
                case AdError.AlreadyDisplayed: return "Ad already displayed";
                case AdError.NotForeground: return "App not foreground";
                default: return "Internal error";
            }
        }

        public static implicit operator int(AdError error) { return error.code; }
        public static explicit operator AdError(int code) { return new AdError(code, null); }

        [Obsolete("Renamed to NotPassedInterval")]
        public const int IntervalNotYetPassed = NotPassedInterval;
        [Obsolete("Renamed to NotForeground")]
        public const int AppIsPaused = NotForeground;
    }
}