//  Copyright © 2024 CAS.AI. All rights reserved.

using System;

namespace CAS
{
    /// <summary>
    /// Use this object for configure Consent flow dialogs for GDPR.
    /// Create and attach the object to CAS initialization.
    /// <code>
    /// MobileAds.BuildManager()
    ///          .WithConsentFlow(new ConsentFlow()
    ///             .WithPrivacyPolicy("https://url_to_privacy_policy")
    ///          )
    ///          .Build();
    /// </code>
    /// By default, the consent flow will be shown to users who are protected by laws.
    /// You can prevent us from showing the consent dialog to the user ussing followed lines:
    /// <code>
    /// MobileAds.BuildManager()
    ///          .WithConsentFlow(new ConsentFlow(isEnabled: false))
    ///          .Build();
    /// </code>
    /// You can also display Consent Flow at any time using the show functions:
    /// <code>
    /// new ConsentFlow()
    ///    .WithCompletionListener((status) =>
    ///    {
    ///        Debug.Log("Consent flow completed with status: " + status.ToString());
    ///    })
    ///    .ShowIfRequired();
    /// </code>
    /// </summary>
    public class ConsentFlow
    {
        public bool isEnabled;
        public string privacyPolicyUrl = null;
        public Action OnCompleted = null;
        public Action<ConsentFlowStatus> OnResult = null;
        public DebugGeography debugGeography = DebugGeography.EEA;

        /// <summary>
        /// Create Conset flow configuration
        /// </summary>
        /// <param name="isEnabled">Is enabled auto display consent flow if required on Ads initialization.</param>
        public ConsentFlow(bool isEnabled = true)
        {
            this.isEnabled = isEnabled;
        }

        /// <summary>
        /// Override a link to the App's Privacy Policy in the consent dialog.
        /// </summary>
        public ConsentFlow WithPrivacyPolicy(string url)
        {
            privacyPolicyUrl = url;
            return this;
        }

        /// <summary>
        /// Set a listener to be invoked when the dialog is dismissed.
        /// The <see cref="ConsentFlowStatus"/> with which the dialog is dismissed will be passed to the listener function.
        /// </summary>
        public ConsentFlow WithCompletionListener(Action<ConsentFlowStatus> result)
        {
            OnResult += result;
            return this;
        }

        /// <summary>
        /// Set a listener to be invoked when the dialog is dismissed.
        /// </summary>
        public ConsentFlow WithCompletionListener(Action complete)
        {
            OnCompleted += complete;
            return this;
        }

        /// <summary>
        /// Sets the debug geography for testing purposes.
        /// Note that debug settings only work with Test Ad Mode enabled or for <see cref="IAdsSettings.SetTestDeviceIds"/>.
        /// Default value is <see cref="DebugGeography.EEA"/>
        /// </summary>
        public ConsentFlow WithDebugGeography(DebugGeography debugGeography)
        {
            this.debugGeography = debugGeography;
            return this;
        }

        public enum DebugGeography
        {
            /// <summary>
            /// Disable geography debugging.
            /// </summary>
            Disabled = 0,

            /// <summary>
            /// Geography appears as in European Economic Area for debug devices.
            /// </summary>
            EEA = 1,

            /// <summary>
            /// Geography appears as not in European Economic Area.
            /// </summary>
            NotEEA = 2
        }
    }
}

