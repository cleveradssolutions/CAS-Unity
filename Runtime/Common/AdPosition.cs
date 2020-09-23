using System;

namespace CAS
{
    public enum AdPosition
    {
        TopCenter,
        TopLeft,
        TopRight,
        BottomCenter,
        BottomLeft,
        BottomRight,
        //Center, Not supported
        Undefined = 7,

        [Obsolete( "Renamed to TopCenter")]
        Top_Centered = TopCenter,
        [Obsolete( "Renamed to TopLeft" )]
        Top_Left = TopLeft,
        [Obsolete( "Renamed to TopRight" )]
        Top_Right = TopRight,
        [Obsolete( "Renamed to BottomCenter" )]
        Bottom_Centered = BottomCenter,
        [Obsolete( "Renamed to BottomLeft" )]
        Bottom_Left = BottomLeft,
        [Obsolete( "Renamed to BottomRight" )]
        Bottom_Right = BottomRight,
    }
}
