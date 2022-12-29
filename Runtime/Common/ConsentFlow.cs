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
    ///          .Initialize();
    /// </code>
    /// By default, the consent flow will be shown to users who are protected by laws.
    /// You can prevent us from showing the consent dialog to the user ussing followed lines:
    /// <code>
    /// MobileAds.BuildManager()
    ///          .WithConsentFlow(new ConsentFlow(isEnabled: false))
    ///          .Initialize();
    /// </code>
    /// </summary>
    public class ConsentFlow
    {
        public bool isEnabled;
        public string privacyPolicyUrl = null;

        /// <summary>
        /// Create Conset flow configuration
        /// </summary>
        /// <param name="isEnabled">If enabled then the consent flow will be shown to users who are protected by laws.</param>
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
    }
}

