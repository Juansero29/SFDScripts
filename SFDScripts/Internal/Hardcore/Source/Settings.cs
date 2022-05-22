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

        #region Settings
        /// <summary>
        /// Time per round in seconds
        /// </summary>
        public static int AreaTime = 30 * 3 + 5;

        /// <summary>
        /// Time in the menus
        /// </summary>
        public static int TimeToStart = 60;

        /// <summary>
        /// Defines wether we want to allow players to keep their skins or not
        /// <remarks>
        /// If false, players will use the dafult red and blue skins in the map
        /// </remarks>
        /// </summary>
        public static bool KeepPlayersSkins = false;

        #endregion

        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
