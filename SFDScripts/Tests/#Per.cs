using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SFDGameScriptInterface;

namespace SFDScripts
{

    class Persistance : GameScriptInterface
    {
        public Persistance() : base(null) { }

        #region CodeForScript
        private int GamesPlayed = 0;

        public void OnStartup()
        {
            bool success = Int32.TryParse(Game.Data, out GamesPlayed);
            if (!success)
            {
                GamesPlayed = 0;
            }
            else
            {
                GamesPlayed++;
            }
            Game.RunCommand("/MSG This script has been running for " + GamesPlayed + " maps");

            IObjectTimerTrigger Timer = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
            Timer.SetIntervalTime(1000);
            Timer.SetRepeatCount(0);
            Timer.SetScriptMethod("ShowGameDotData");
            Timer.Trigger();
        }

        public void ShowGameDotData(TriggerArgs args)
        {
            Game.RunCommand("/MSG Game.Data.ToString() = " + Game.Data.ToString());
            foreach (char c in Game.Data)
            {
                 Game.RunCommand("/MSG " + c.ToString());
            }
            
        }

        public void OnShutdown()
        {
            Game.Data = GamesPlayed.ToString();
        }

        #endregion CodeForScript

    }
}