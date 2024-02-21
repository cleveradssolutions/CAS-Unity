using System;

namespace CAS
{
    public class InitialConfiguration
    {
        /// <summary>
        /// Get the CAS manager initialization error message or NULL if initialization is successful.
        /// Check constants from <see cref="InitializationError"/> class.
        /// </summary>
        public readonly string error;
        /// <summary>
        /// Get the initialized CAS manager.
        /// </summary>
        public readonly IMediationManager manager;
        /// <summary>
        /// Get the user's ISO-2 country code, or NULL if the location fails.
        /// </summary>
        public readonly string countryCode;
        /// <summary>
        /// The consent must be requested from the user.
        /// </summary>
        public readonly bool isConsentRequired;

        public InitialConfiguration(string error, IMediationManager manager, string countryCode, bool isConsentRequired)
        {
            this.error = error;
            this.manager = manager;
            this.countryCode = countryCode;
            this.isConsentRequired = isConsentRequired;
        }
    }
}

