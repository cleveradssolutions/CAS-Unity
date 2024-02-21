namespace CAS
{
    public static class InitializationError
    {
        /// <summary>
        /// Indicates that device network connection is not stable enough.
        /// Your listener is stored in memory and will be called when initialization is successful.
        /// </summary>
        public const string NoConnection = "Connection failed";

        /// <summary>
        /// Indicates that the CAS ID is not registered in system.
        /// Contact support to clarify the reasons.
        /// </summary>
        public const string NotRegisteredID = "Not registered ID";

        /// <summary>
        /// Indicates that the SDK version is no longer compatible.
        /// Please update to the latest SDK.
        /// </summary>
        public const string VerificationFailed = "Verification failed";

        /// <summary>
        /// Indicates a temporary problem with the server.
        /// If the error could be 100% replicated, please give feedback to us.
        /// </summary>
        public const string ServerError = "Server error";
    }
}

