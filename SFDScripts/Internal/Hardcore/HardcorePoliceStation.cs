using System;
using System.Linq;
using System.Collections.Generic;
using SFDGameScriptInterface;
using System.IO.IsolatedStorage;


namespace SFDScripts
{
    public class HardcorePoliceStation
    {
        #region Map Dependant Data
        /// <summary>
        /// The number of map parts in this map. For each map part, an startup process
        /// will be done in the 'OnStartup' method.
        /// </summary>
        public static int NumberOfMapParts = 1;

        /// <summary>
        /// Defines the number of wins required per map part to advance
        /// </summary>
        /// <remarks>
        /// Has to be an uneven number to avoid ties.
        /// </remarks>
        public static int RoundsPerMapPart = 3;

        /// <summary>
        /// Defines the map part that we should start on
        /// </summary>
        public static int CurrentMapPartIndex = 0;

        /// <summary>
        /// The world top position (where airstrikes get launched from)
        /// </summary>
        public static int WorldTop = 600;

        #region Drones
        /// <summary>
        /// Left bottom corner spot of the map. (x and  y coordinates)
        /// </summary>
        public static Vector2 DroneAreaBegin = new Vector2(0, 0);
        /// <summary>
        /// An area that covers each of the map parts (x = width, y = height)
        /// </summary>
        public static Vector2 DroneAreaSize = new Vector2(600, 550);
        #endregion

        public static int CameraWidth = 770;
        public static int CameraHeight = 550;
        #endregion
    }
}
