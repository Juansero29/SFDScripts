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

        #region Map Part Class
        public class TMapPart
        {
            public List<TCapturePoint> PointList = new List<TCapturePoint>();
            public List<IObject> RedSpawnPosition = new List<IObject>();
            public List<IObject> BlueSpawnPosition = new List<IObject>();
            public Vector2 MapPosition = new Vector2(0, 0);
            public int CurrentRound = 1;
            public int BlueRoundsWon = 0;
            public int RedRoundsWon = 0;
            public int CapturedBy = 0;
            private bool _PlayersHaveSpawned = false;
            //functions
            public void Start()
            {
                CapturedBy = 0;
                CameraPosition = MapPosition;

                for (int i = 0; i < PointList.Count; i++)
                {
                    PointList[i].Start();
                }

                if (GlobalRandom.Next(0, 100) <= 20)
                {
                    WeatherType weather = GlobalGame.GetWeatherType();
                    if (weather == WeatherType.None)
                    {
                        GlobalGame.SetWeatherType(WeatherType.Rain);
                        return;
                    }
                    if (weather == WeatherType.Rain)
                    {
                        GlobalGame.SetWeatherType(WeatherType.Snow);
                        return;
                    }
                    if (weather == WeatherType.Snow)
                    {
                        GlobalGame.SetWeatherType(WeatherType.None);
                        return;
                    }
                }
            }
            public int Update()
            {
                int blue = 0, red = 0;
                if (GlobalGame.GetCameraArea().Left == CameraPosition.X && GlobalGame.GetCameraArea().Top == CameraPosition.Y && !_PlayersHaveSpawned)
                {

                    for (int i = 0; i < PlayerList.Count; i++)
                    {

                        var player = PlayerList[i];
                        var menu = PlayerMenuList.Where(m => m.Player != null && m.Player.Name != null && m.Player.Name.Equals(player.Name)).FirstOrDefault();
                        if (menu == null)
                        {
                            DebugLogger.DebugOnlyDialogLog("PLAYER " + menu.Player.Name + " CAN'T BE SPAWNED BECAUSE HE HAS NO MENU ASSIGNED");
                            continue;
                        }
                        if (player.Team == PlayerTeam.Team1)
                        {

                            menu.SpawnPlayer(BlueSpawnPosition[blue].GetWorldPosition());
                            DebugLogger.DebugOnlyDialogLog("PLAYER " + menu.Player.Name + " SPAWNED AT BLUE POSITION NUMBER " + blue);
                            blue++;
                        }
                        else
                        {
                            menu.SpawnPlayer(RedSpawnPosition[red].GetWorldPosition());
                            DebugLogger.DebugOnlyDialogLog("PLAYER " + menu.Player.Name + " SPAWNED AT RED POSITION NUMBER " + blue);
                            red++;
                        }
                    }


                    for (int i = 0; i < PlayerList.Count; i++)
                    {
                        PlayerList[i].Start();
                    }

                    _PlayersHaveSpawned = true;
                }

                //if (_PlayersHaveSpawned)
                //{
                //    var activeUsers = Game.GetActiveUsers();
                //    for (int i = 0; i < activeUsers.Length; i++)
                //    {

                //        var u = activeUsers[i];
                //        if (u != null && u.GetPlayer() != null)
                //        {
                //            var body = u.GetPlayer();
                //            if (body.GetWorldPosition().Y > WorldTop)
                //            {
                //                DebugLogger.DebugOnlyDialogLog("PLAYER " + u.Name + " DIDN'T SPAWNED CORRECTLY. TELEPORTING THEM");
                //                var menu = PlayerMenuList.Where(m => m.Player != null && m.Player.Name != null && m.Player.Name.Equals(u.Name)).FirstOrDefault();
                //                var player = PlayerList.Where(p => p.Name != null && p.Name.Equals(u.Name)).FirstOrDefault();
                //                if (player != null)
                //                {
                //                    DebugLogger.DebugOnlyDialogLog("PLAYER " + player.Name + " EXISTS IN PlayerList");
                //                }
                //                else
                //                {
                //                    DebugLogger.DebugOnlyDialogLog("PLAYER " + player.Name + " DOESN'T EXIST IN PlayerList");
                //                }

                //                DebugLogger.DebugOnlyDialogLog("PLAYER " + u.Name + " DIDN'T SPAWNED CORRECTLY. TELEPORTING THEM");

                //                if (menu == null)
                //                {
                //                    DebugLogger.DebugOnlyDialogLog("PLAYER " + player.Name + " DOESN'T HAVE AN ASSIGNED MENU. HIS PROGRESS WILL BE LOST");

                //                    switch (body.GetTeam())
                //                    {
                //                        case PlayerTeam.Team1:
                //                            body.SetWorldPosition(BlueSpawnPosition[0].GetWorldPosition());
                //                            break;
                //                        case PlayerTeam.Team2:
                //                            body.SetWorldPosition(RedSpawnPosition[0].GetWorldPosition());
                //                            break;
                //                        case PlayerTeam.Team3:
                //                            DebugLogger.DebugOnlyDialogLog("BODY WAS FROM GREEN TEAM.");
                //                            break;
                //                    }
                //                }
                //                else
                //                {
                //                    DebugLogger.DebugOnlyDialogLog("PLAYER " + player.Name + " HAS AN ASSIGNED MENU. USING MENU TO SPAWN HIM");


                //                    switch (body.GetTeam())
                //                    {
                //                        case PlayerTeam.Team1:
                //                            DebugLogger.DebugOnlyDialogLog("BODY WAS FROM BLUE TEAM.");
                //                            menu.SpawnPlayer(BlueSpawnPosition[0].GetWorldPosition());
                //                            break;
                //                        case PlayerTeam.Team2:
                //                            DebugLogger.DebugOnlyDialogLog("BODY WAS FROM RED TEAM.");
                //                            menu.SpawnPlayer(RedSpawnPosition[0].GetWorldPosition());
                //                            break;
                //                        case PlayerTeam.Team3:
                //                            DebugLogger.DebugOnlyDialogLog("BODY WAS FROM GREEN TEAM.");
                //                            break;
                //                    }
                //                }


                //            }
                //        }
                //    }
                //}


                if (CapturedBy != 0) return 0;
                bool blueWin = true;
                bool redWin = true;
                for (int i = 0; i < PointList.Count; i++)
                {
                    PointList[i].Update();
                    if (PointList[i].CaptureProgress != MaxCapturePointProgress)
                    {
                        blueWin = false;
                    }
                    if (PointList[i].CaptureProgress != -MaxCapturePointProgress)
                    {
                        redWin = false;
                    }
                }
                if (blueWin)
                {
                    CapturedBy = 1;
                    return 1;
                }
                else if (redWin)
                {
                    CapturedBy = 2;
                    return 2;
                }
                return 0;
            }

            public void Restart()
            {
                for (int i = 0; i < PlayerList.Count; i++)
                {
                    var p = PlayerList[i];
                    if (p.User == null || p.User.GetPlayer() == null) continue;
                    PlayerList[i].User.GetPlayer().Remove();
                }
                _PlayersHaveSpawned = false;
            }
        }

        #endregion

        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
