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
        public GameScript() : base(null) { }
        /* SCRIPT STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */
        #region Debug
        /// <summary>
        /// Is the debug mode enabled
        /// </summary>
        /// <remakrs>
        // If true:
        /// - Never finishes a matches by player's death
        /// - Doesn't show timer
        /// - Doesn't end the game when there isn't enough players
        /// - Shows some extra visual debugging info
        /// </remakrs>
        public static bool IsDebug = false;

        /// <summary>
        /// Defines wether we want to show the debug messages or not as logs in the chat
        /// </summary>
        public static bool ShowDebugMessages = false;

        /// <summary>
        // When true, we use a file called hardcoredebug.txt rather than hardcore.txt for loading and saving data
        /// </summary>
        public static bool UseDebugStorage = false;

        /// <summary>
        /// When true, there's no need to set players to ready, they will all be ready from the start;
        /// </summary>
        public static bool MakeAllPlayersReadyFromTheStart = false;

        #endregion
        /* SCRIPT ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */
    }
}
