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

        #region Timer Methods
        public void OnBeginTimer(TriggerArgs args)
        {
            if (TimeToStart > 0)
            {
                TimeToStart--;
            }
            if (GameState == 0)
            {
                int readyPlayers = 0;
                for (int i = 0; i < PlayerMenuList.Count; i++)
                {
                    if (PlayerMenuList[i].Ready)
                    {
                        readyPlayers++;
                    }
                }
                if (TimeToStart > 15 && (float)readyPlayers / (float)PlayerList.Count >= 2.0 / 3.0)
                {
                    TimeToStart = 15;
                }
                if (readyPlayers == PlayerList.Count)
                {
                    TimeToStart = 0;
                }
                if (TimeToStart <= 10)
                {
                    BeginTimer.SetTextColor(Color.Red);
                    GlobalGame.PlaySound("TimerTick", new Vector2(0, 600), 1.0f);
                }
                BeginTimer.SetText("Choose your equipment: " + TimeToStart.ToString());
                if (TimeToStart == 0)
                {
                    GameState = 1;
                    Game.RunCommand("MSG BEST OF " + RoundsPerMapPart + " WINS!");
                    BeginTimer.SetText("");
                }
            }

            else if (GameState == 3)
            {
                for (int i = 0; i < PlayerList.Count; i++)
                {
                    PlayerList[i].ReloadEquipment();
                }
                for (int i = 0; i < TeamJamming.Length; i++)
                {
                    if (TeamJamming[i] > 0)
                    {
                        TeamJamming[i]--;
                        if (TeamJamming[i] == 0)
                        {
                            if (i == 0) Game.RunCommand("MSG RED TEAM DISABLE SUPPLY JAMMER");
                            else Game.RunCommand("MSG BLUE TEAM DISABLE SUPPLY JAMMER");
                        }
                    }
                }
                for (int i = 0; i < TeamHacking.Length; i++)
                {
                    if (TeamHacking[i] > 0)
                    {
                        TeamHacking[i]--;
                        if (TeamHacking[i] == 0)
                        {
                            if (i == 0) Game.RunCommand("MSG RED TEAM DISABLE SUPPLY HACKING");
                            else Game.RunCommand("MSG BLUE TEAM DISABLE SUPPLY HACKING");
                        }
                    }
                }
            }
            if (GameState != 0)
            {
                if (GameState == 1)
                {

                    if (TimeToStart > 0 && TimeToStart < 5)
                    {
                        GlobalGame.PlaySound("TimerTick", new Vector2(0, 600), 1.0f);
                        Game.ShowPopupMessage("Starting in: " + TimeToStart.ToString(), Color.Red);
                    }
                    else if (TimeToStart == 0)
                    {
                        GlobalGame.PlaySound("TimerTick", new Vector2(0, 600), 1.0f);
                        Game.ShowPopupMessage("Starting now!", Color.Red);
                    }
                    else
                    {
                        Game.ShowPopupMessage("Starting in: " + TimeToStart.ToString(), Color.Yellow);
                    }
                }
                else if (!IsDebug)
                {
                    Game.ShowPopupMessage("Time: " + TimeToStart.ToString(), Color.White);
                    if (TimeToStart == 0)
                    {
                        Game.HidePopupMessage();
                    }
                }
            }
        }
        #endregion

        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
