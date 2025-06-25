//  Copyright © 2025 CAS.AI. All rights reserved.

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
        TopCenter = 0,
        TopLeft = 1,
        TopRight = 2,
        BottomCenter = 3,
        BottomLeft = 4,
        BottomRight = 5,
        MiddleCenter = 6,
        MiddleLeft = 7,
        MiddleRight = 8,

        /// <summary>
        /// Service value to continue use previously Ad Position.
        /// </summary>
        Undefined,
    }
}
