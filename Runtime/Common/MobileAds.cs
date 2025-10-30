/*
 * Copyright 2025 CleverAdsSolutions LTD, CAS.AI
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using UnityEngine;

namespace CAS
{
    [WikiPage("https://docs.page/cleveradssolutions/docs/Unity/Initialize-SDK")]
    public static class MobileAds
    {
        /// <summary>
        /// The CAS Unity plugin wrapper version.
        /// </summary>
        public const string wrapperVersion = "4.4.1";

        /// <summary>
        /// Get singleton instance for configure all mediation managers.
        /// </summary>
        public static IAdsSettings settings
        {
            get { return CASFactory.GetAdsSettings(); }
        }

        /// <summary>
        /// Get first initialized <see cref="IMediationManager"/>
        /// <para>May be NULL before the first initialization in the session.</para>
        /// <para>We recommend using the <see cref="BuildManager()"/>.WithManagerIdAtIndex().Build() method to get a specific manager.</para>
        /// </summary>
        public static IMediationManager manager
        {
            get { return CASFactory.GetMainManagerOrNull(); }
        }

        /// <summary>
        /// You can now easily tailor the way you serve your ads to fit a specific audience!
        /// <para>You will need to inform our servers of the users’ details
        /// so the SDK will know to serve ads according to the segment the user belongs to.</para>
        /// <para>**Attention:** Must be set before initializing the SDK.</para>
        /// </summary>
        public static ITargetingOptions targetingOptions
        {
            get { return CASFactory.GetTargetingOptions(); }
        }

        /// <summary>
        /// Raised when the application enters the background.
        /// 
        /// <para>Note that the Android runtime triggers events not from the main Unity thread. 
        /// However, you can call CAS plugin methods without leaving the current thread.</para>
        /// </summary>
        public static event Action OnApplicationBackground
        {
            add { CASFactory.GetAppStateEventClient().OnApplicationBackground += value; }
            remove { CASFactory.GetAppStateEventClient().OnApplicationBackground -= value; }
        }

        /// <summary>
        /// Raised when the application enters the foreground.
        /// It's important to use this event when you want to show ads on user returning to the game.
        /// 
        /// <para>Note that the Android runtime triggers events not from the main Unity thread. 
        /// However, you can call CAS plugin methods without leaving the current thread.</para>
        /// </summary>
        public static event Action OnApplicationForeground
        {
            add { CASFactory.GetAppStateEventClient().OnApplicationForeground += value; }
            remove { CASFactory.GetAppStateEventClient().OnApplicationForeground -= value; }
        }

        /// <summary>
        /// The Native SDK version else <see cref="wrapperVersion"/> for Unity Editor.
        /// </summary>
        public static string GetSDKVersion()
        {
            return CASFactory.GetSDKVersion();
        }

        /// <summary>
        /// The scale of the device screen in density independent pixels.
        /// </summary>
        public static float GetDeviceScreenScale()
        {
            return CASFactory.GetDeviceScreenScale();
        }

        /// <summary>
        /// Create <see cref="IMediationManager"/> builder.
        /// <para>Don't forget to call the <see cref="IManagerBuilder.Build"/> method to create manager instance.</para>
        /// </summary>
        public static IManagerBuilder BuildManager()
        {
            return CASFactory.LoadDefaultBuiderFromResources();
        }

        /// <summary>
        /// The CAS SDK provides an easy way to verify that you’ve successfully integrated any additional adapters;
        /// it also makes sure all required dependencies and frameworks were added for the various mediated ad networks.
        /// <para>After you have finished your integration, call the following static method
        /// and confirm that all networks you have implemented are marked as <b>VERIFIED</b>.</para>
        ///
        /// <para>Find log information by tag: <b>CASIntegrationHelper</b></para>
        ///
        /// Once you’ve successfully verified your integration,
        /// please remember to remove the integration helper from your code.
        /// </summary>
        public static void ValidateIntegration()
        {
            CASFactory.ValidateIntegration();
        }

        /// <summary>
        /// Get array of active mediation networks in build.
        /// </summary>
        public static AdNetwork[] GetActiveNetworks()
        {
            return CASFactory.GetActiveNetworks();
        }

        public static bool IsActiveNetwork(AdNetwork network)
        {
            return CASFactory.IsActiveNetwork(network);
        }

        /// <summary>
        /// For internal purposes only.
        /// </summary>
        public static void ReportCustomRevenue(string json)
        {
            CASFactory.ReportCustomRevenue(json);
        }

        [Obsolete("Use ShowIfRequired() or Show() methods for ConsentFlow insntance.")]
        public static void ShowConsentFlow(ConsentFlow flow)
        {
            flow.Show();
        }
    }
}

public static class CASImplementationExtensions
{
    /// <summary>
    /// Shows the consent form only if it is required and the user has not responded previously.
    /// If the consent status is required, the SDK loads a form and immediately presents it.
    /// </summary>
    public static void ShowIfRequired(this CAS.ConsentFlow flow)
    {
        CAS.CASFactory.ShowConsentFlow(flow, true);
    }

    /// <summary>
    /// Force shows the form to modify user  consent at any time.
    /// When a user interacts with your UI element, call function to show the form 
    /// so the user can update their privacy options at any time. 
    /// </summary>
    public static void Show(this CAS.ConsentFlow flow)
    {
        CAS.CASFactory.ShowConsentFlow(flow, false);
    }
}
