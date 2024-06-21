//  Copyright © 2024 CAS.AI. All rights reserved.

using System;

namespace CAS
{
    /// <summary>
    /// Ad Position on screen.
    ///         ___Left____Center____Right__
    /// Top     |   1    |   0    |    2   |
    /// Middle  |   7    |   6    |    8   |
    /// Bottom  |   4    |   3    |    5   |
    /// </summary>
    public enum AdPosition
    {
        TopCenter,
        TopLeft,
        TopRight,
        BottomCenter,
        BottomLeft,
        BottomRight,
        MiddleCenter,
        MiddleLeft,
        MiddleRight,

        /// <summary>
        /// Service value to continue use previously Ad Position.
        /// </summary>
        Undefined,
    }
}
