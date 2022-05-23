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

        #region Debug Logger Class

        public static class DebugLogger
        {


            public static void DebugOnlyDialogLog(string message)
            {
                if (!ShowDebugMessages) return;
                Game.CreateDialogue(message, new Vector2(CameraPosition.X + 500, CameraPosition.Y + 500), "LOG");
            }


            public static void DebugOnlyDialogLog(string message, Vector2 position)
            {
                if (!ShowDebugMessages) return;
                Game.CreateDialogue(message, position, "LOG");
            }

            public static void DialogLog(string message)
            {
                Game.CreateDialogue(message, new Vector2(CameraPosition.X + 500, CameraPosition.Y + 500), "LOG");
            }

            public static void DialogLog(string message, Vector2 position)
            {
                Game.CreateDialogue(message, position, "LOG");
            }
        }

        #endregion

        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
