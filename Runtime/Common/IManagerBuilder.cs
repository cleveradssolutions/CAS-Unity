//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2022 CleverAdsSolutions. All rights reserved.
//

using System;
using System.Reflection;
using UnityScript.Steps;

namespace CAS
{
    public delegate void InitCompleteAction(bool success, string error);

    [WikiPage("https://github.com/cleveradssolutions/CAS-Unity/wiki/Initialize-SDK")]
    public interface IManagerBuilder : IAdsPreset
    {
        /// <summary>
        /// Initialize new <see cref="IMediationManager"/> and save to <see cref="MobileAds.manager"/> field.
        /// Can be called for different identifiers to create different managers.
        /// <para>Attention! Do not initialize mediated advertising SDKs (CAS does that for you).
        /// Not following this step will result in noticeable integration issues.</para>
        /// </summary>
        /// <exception cref="NotSupportedException">Not supported platform. Allowed Android, iOS and Editor only</exception>
        /// <exception cref="ArgumentNullException">CAS ID are not found</exception>
        IMediationManager Build();

        /// <summary>
        /// Same as <see cref="Build()"/> method.
        /// </summary>
        /// <exception cref="NotSupportedException">Not supported platform. Allowed Android, iOS and Editor only</exception>
        /// <exception cref="ArgumentNullException">CAS ID are not found</exception>
        IMediationManager Initialize();

        /// <summary>
        /// An CAS manager ID is a unique ID number assigned to each of your ad placements when they're created in CAS.
        /// <para>Using Manager Id at Index from list <b>`Assets/CleverAdsSolutions/Settings`</b> menu.</para>
        /// <para>Index 0 by default when the method is not called.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException">CAS ID are not found with index</exception>
        IManagerBuilder WithCASId(int index);

        /// <summary>
        /// An CAS manager ID is a unique ID number assigned to each of your ad placements when they're created in CAS.
        /// <para>Typically, Manager ID for Adnroid equals the App Bundle ID, and for iOS the iTunes ID.</para>
        /// <para>The manager ID is added to your app's code and used to identify ad requests.</para>
        /// <para><b>Attention</b> The identifier is different for each platforms.
        /// You need to define different identifiers depending on the current platform.</para>
        /// <para>You can use a generic way to get the ID by ordinal index <see cref="WithManagerIdAtIndex(int)"/> for current platform.</para>
        /// <para>Please set all used Manager IDs in `Assets > CleverAdsSolutions > Settings` menu to setup the project correctly.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException">CAS ID are is empty</exception>
        IManagerBuilder WithCASId(string casId);

        /// <summary>
        /// Same as <see cref="WithCASId(int)"/> method. 
        /// </summary>
        /// <exception cref="ArgumentNullException">CAS ID are not found with index</exception>
        IManagerBuilder WithManagerIdAtIndex(int index);

        /// <summary>
        /// Same as <see cref="WithCASId(string)"/> method. 
        /// </summary>
        /// <exception cref="ArgumentNullException">CAS ID is empty</exception>
        IManagerBuilder WithManagerId(string managerId);

        /// <summary>
        /// Set listener to initialization complete result callback.
        /// </summary>
        IManagerBuilder WithInitListener(InitCompleteAction listener);

        /// <summary>
        /// An Enabled Ad types is option to increase application performance by initializing only those ad types that will be used.
        /// <code>
        /// .withEnabledAdTypes(AdFlags.Banner, AdFlags.Interstitial)
        /// </code>
        /// <para>Changes in current session only.</para>
        /// <para>Ad types can be enabled manually after initialize by <see cref="IMediationManager.SetEnableAd(AdType, bool)"/></para>
        /// </summary>
        IManagerBuilder WithEnabledAdTypes(params AdFlags[] adTypes);

        /// <summary>
        /// The userID is a unique identifier supplied by your application and must be static for each user across sessions.
        /// Your userID should not contain any personally identifiable information such as
        /// an email address, screen name, Android ID(AID), or Google Advertising ID(GAID).
        /// </summary>
        IManagerBuilder WithUserID(string userID);

        /// <summary>
        /// Create and attach the Conset flow configuration for initialization.
        /// <code>
        /// .withConsentFlow(
        ///    new ConsentFlow()
        ///       .withPrivacyPolicy("https://url_to_privacy_policy")
        /// )
        /// </code>
        /// By default, the consent flow will be shown to users who are protected by laws.
        /// You can prevent us from showing the consent dialog to the user ussing followed lines:
        /// <code>
        /// .withConsentFlow(new ConsentFlow(isEnabled: false))
        /// </code>
        /// </summary>
        IManagerBuilder WithConsentFlow(ConsentFlow flow);

        /// <summary>
        /// Additional mediation settings.
        /// Use constant key from <see cref="MediationExtras"/> with values of "1" or "0".
        /// </summary>
        IManagerBuilder WithMediationExtras(string key, string value);

        /// <summary>
        /// Clear additional mediation settings.
        /// </summary>
        IManagerBuilder ClearMediationExtras();

        /// <summary>
        /// Option to enable Test ad mode that will always request test ads.
        /// <para>If you're just looking to experiment with the SDK in a Hello World app, though, you can use the true with any manager ID string.</para>
        /// <para><b>Don't forget to set False test ad mode to release application.</b></para>
        /// </summary>
        [Obsolete("Please set Test Ad Mode in `Assets>CleverAdsSolutions>Settings` menu to get true Test Ad, " +
            "also Development build work same.")]
        IManagerBuilder WithTestAdMode(bool test);
    }
}