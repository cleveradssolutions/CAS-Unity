//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2021 CleverAdsSolutions. All rights reserved.
//

using System;

namespace CAS
{
    public interface IManagerBuilder : IAdsPreset
    {
        /// <summary>
        /// Initialize new <see cref="IMediationManager"/> and save to <see cref="MobileAds.manager"/> field.
        /// Can be called for different identifiers to create different managers.
        /// </summary>
        /// <exception cref="NotSupportedException">Not supported platform. Allowed Android, iOS and Editor only</exception>
        /// <exception cref="ArgumentNullException">Manager ID are not found</exception>
        IMediationManager Initialize();

        /// <summary>
        /// An manager ID is a unique ID number assigned to each of your ad placements when they're created in CAS.
        /// <para>Using Manager Id at Index from list <b>`Assets/CleverAdsSolutions/Settings`</b> menu.</para>
        /// <para>Index 0 by default when the method is not called.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException">Manager ID is empty</exception>
        CASInitSettings WithManagerIdAtIndex( int index );

        /// <summary>
        /// An manager ID is a unique ID number assigned to each of your ad placements when they're created in CAS.
        /// <para>Typically, Manager ID for Adnroid equals the App Bundle ID, and for iOS the iTunes ID.</para>
        /// <para>The manager ID is added to your app's code and used to identify ad requests.</para>
        /// <para><b>Attention</b> The identifier is different for each platforms.
        /// You need to define different identifiers depending on the current platform.</para>
        /// <para>You can use a generic way to get the ID by ordinal index <see cref="WithManagerIdAtIndex(int)"/> for current platform.</para>
        /// <para>Please set all used Manager IDs in `Assets > CleverAdsSolutions > Settings` menu to setup the project correctly.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException">Manager ID is empty</exception>
        CASInitSettings WithManagerId( string managerId );

        /// <summary>
        /// Set listener to initialization complete result callback.
        /// </summary>
        CASInitSettings WithInitListener( InitCompleteAction listener );

        /// <summary>
        /// An Enabled Ad types is option to increase application performance by initializing only those ad types that will be used.
        /// <para>For example: .withEnabledAdTypes(AdFlags.Banner, AdFlags.Interstitial)</para>
        /// <para>Changes in current session only.</para>
        /// <para>Ad types can be enabled manually after initialize by <see cref="IMediationManager.SetEnableAd(AdType, bool)"/></para>
        /// </summary>
        CASInitSettings WithEnabledAdTypes( params AdFlags[] adTypes );

        /// <summary>
        /// Additional mediation settings.
        /// Use constant key from <see cref="MediationExtras"/> with values of "1" or "0".
        /// </summary>
        CASInitSettings WithMediationExtras( string key, string value );

        /// <summary>
        /// Clear additional mediation settings.
        /// </summary>
        CASInitSettings ClearMediationExtras();

        /// <summary>
        /// Option to enable Test ad mode that will always request test ads.
        /// <para>If you're just looking to experiment with the SDK in a Hello World app, though, you can use the true with any manager ID string.</para>
        /// <para><b>Don't forget to set False test ad mode to release application.</b></para>
        /// </summary>
        [Obsolete( "Please set Test Ad Mode in `Assets>CleverAdsSolutions>Settings` menu to get true Test Ad, " +
            "also Development build work same." )]
        CASInitSettings WithTestAdMode( bool test );
    }
}