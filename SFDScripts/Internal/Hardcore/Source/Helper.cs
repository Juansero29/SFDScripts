using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SFDGameScriptInterface;


namespace SFDScript
{

    public partial class GameScript : GameScriptInterface
    {

        /* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */

        #region Helper Class
        public static class CategoryBits
        {
            internal const ushort None = 0x0000;

            /// <summary>
            /// Static impassable objects (wall, ground, plate...)
            /// </summary>
            internal const ushort StaticGround = 0x0001;

            internal const ushort DynamicPlatform = 0x0002;

            internal const ushort Player = 0x0004;
            /// <summary>
            /// Dynamic objects that can collide with player without setting IObject.TrackAsMissle(true)
            /// Example: table, chair, couch, crate...
            /// </summary>
            internal const ushort DynamicG1 = 0x0008;
            /// <summary>
            /// Dynamic objects that cannot collide with player but can collide with other dynamic objects
            /// Set IObject.TrackAsMissle(true) to make them collide with players
            /// Example: glass, cup, bottle, weapons on map...
            /// </summary>
            internal const ushort DynamicG2 = 0x0010;
            internal const ushort Dynamic = DynamicG1 + DynamicG2;

            internal const ushort Items = 0x0020;
            internal const ushort Debris = 0x0010;
            internal const ushort DynamicsThrown = 0x8000;
        }
        #endregion

        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
