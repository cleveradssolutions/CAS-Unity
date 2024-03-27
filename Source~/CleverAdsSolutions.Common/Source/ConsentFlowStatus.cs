//  Copyright © 2024 CAS.AI. All rights reserved.

namespace CAS
{
    public enum ConsentFlowStatus
    {
        /// <summary>
        /// User consent obtained. Personalized vs non-personalized undefined.
        /// </summary>
        Obtained = 3,

        /// <summary>
        /// User consent not required.
        /// </summary>
        NotRequired = 4,

        /// <summary>
        /// User consent unavailable.
        /// </summary>
        Unavailable = 5,

        /// <summary>
        /// There was an internal error.
        /// </summary>
        InternalError = 10,

        /// <summary>
        /// There was an error loading data from the network.
        /// </summary>
        InternetError = 11,

        /// <summary>
        /// There was an error with the UI context is passed in.
        /// </summary>
        ContextInvalid = 12,

        /// <summary>
        /// There was an error with another form is still being displayed.
        /// </summary>
        FlowStillShowing = 13,
    }
}