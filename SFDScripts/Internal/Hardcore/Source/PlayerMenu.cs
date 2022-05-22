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

        #region Player Menu Class
        public class TPlayerMenu
        {
            public IObjectText Menu;
            public TPlayer Player = null;
            public int CurrentPoints = 0;
            public List<int> Equipment = new List<int>();
            public int CurrentGroup = 0;
            public int ActionTimer = 0;
            public bool Ready = false;
            public bool Change = true;
            public bool IsDescription = false;
            public int AccessLevel = 0;


            public void SetPlayer(TPlayer player)
            {
                if (player == null) return;
                Player = player;
                if (UserAccessList.ContainsKey(player.User.Name)) AccessLevel = UserAccessList[player.User.Name];

                if (Equipment.Count == 0)
                {
                    for (int i = 0; i < EquipmentList.Count; i++) Equipment.Add(0);
                }

                if (Player.User.IsBot)
                {
                    Ready = true;
                }

                Equipment[0] = 1;
                Equipment[2] = 3;
            }
            public string Save(bool saveOnlyIfActive = true)
            {
                if (Player == null) return "";
                if (saveOnlyIfActive)
                {
                    if (!Player.IsActive())
                    {
                        DebugLogger.DebugOnlyDialogLog("PLAYER " + Player.Name + " NOT BEING SAVED BECAUSE HE WASN'T ACTIVE");
                        return "";
                    }
                }
                if (Player.Name.Contains("Unnamed"))
                {
                    DebugLogger.DebugOnlyDialogLog("PLAYER NOT BEING SAVED BECAUSE HE IS CALLED 'Unnamed'");
                    return "";
                }
                string data = Player.Save();
                for (int i = 0; i < Equipment.Count; i++)
                {
                    data += Equipment[i].ToString();
                    if (i == Equipment.Count - 1)
                    {
                        data += ";";
                    }
                    else
                    {
                        data += ":";
                    }
                }
                return data;
            }
            public void ValidateEquipment()
            {
                if (Player == null) return;
                for (int i = 0; i < Equipment.Count; i++)
                {
                    if (EquipmentList[i].EquipmentList.Count <= Equipment[i]) Equipment[i] = 0;
                }
            }
            public void ShowExp()
            {
                if (Player == null) return;
                string text = "";
                text += Player.Name + "\n";
                //text += "Class: " + race.Name + "\n";
                text += "Level " + (Player.Level + 1).ToString() + " \"" + LevelList[Player.Level].Name + "\"";
                if (AccessLevel == 1) text += " [$]";
                text += "\n";
                if (Player.Level + 1 < LevelList.Count)
                {
                    int percent = (int)((float)Player.CurrentExp / (float)LevelList[Player.Level + 1].NeedExp * 10);
                    text += "[";
                    for (int i = 0; i < 10; i++)
                    {
                        if (i < percent)
                        {
                            text += "=";
                        }
                        else
                        {
                            text += "_";
                        }
                    }
                    text += "] " + ((int)Player.CurrentExp) + "/" + LevelList[Player.Level + 1].NeedExp + "\n";
                }
                text += "------------------------------\n";
                if (Player.ExpSource[0] > 0) text += "Kills: +" + ((int)Player.ExpSource[0]).ToString() + "\n";
                if (Player.ExpSource[5] > 0) text += "Reinforcements: +" + ((int)Player.ExpSource[5]).ToString() + "\n";
                if (Player.ExpSource[1] > 0) text += "Healing: +" + ((int)Player.ExpSource[1]).ToString() + "\n";
                if (Player.ExpSource[2] > 0) text += "Point Capture: +" + ((int)Player.ExpSource[2]).ToString() + "\n";
                if (Player.ExpSource[3] > 0) text += "Area Capture: +" + ((int)Player.ExpSource[3]).ToString() + "\n";
                if (Player.ExpSource[4] > 0) text += "Win: +" + ((int)Player.ExpSource[4]).ToString() + "\n";
                if (Player.IsNewLevel)
                {
                    text += "---------New equipment--------\n";
                    int lineLength = 30;
                    int lineLeft = lineLength;
                    for (int i = 0; i < EquipmentList.Count; i++)
                    {
                        for (int j = 0; j < EquipmentList[i].EquipmentList.Count; j++)
                        {
                            if (EquipmentList[i].EquipmentList[j].Level == Player.Level && EquipmentList[i].EquipmentList[j].AccessLevel <= AccessLevel)
                            {
                                string name = EquipmentList[i].EquipmentList[j].Name;
                                if (name.Length + 2 > lineLeft)
                                {
                                    text += "\n";
                                    lineLeft = lineLength;
                                }
                                lineLeft -= name.Length;
                                text += name + ", ";
                            }
                        }
                    }

                    if (Player.Level >= 1)
                    {
                        var previousLevelEquipmentPoints = LevelList[Player.Level - 1].AllowPoints;
                        var wonEquipmentPoints = LevelList[Player.Level].AllowPoints - previousLevelEquipmentPoints;
                        if (wonEquipmentPoints > 0)
                        {
                            text += "\n +" + wonEquipmentPoints + " Equipment Points";
                        }
                    }

                }
                Menu.SetTextColor(Color.White);
                Menu.SetText(text);
            }

            public void Dispose()
            {
                if (Menu == null) return;
                if (Player != null && Player.User != null)
                {
                    var p = Player.User.GetPlayer();
                    if (p != null)
                    {
                        p.Remove();
                    }
                }
                Player = null;
                Menu.SetText(String.Empty);
                Ready = false;
            }
            public void Update()
            {
                if (Menu == null || Player == null || Player.User == null) return;

                IPlayer pl = Player.User.GetPlayer();
                Player.UpdateActiveStatus();
                if (pl == null) return;
                if (MakeAllPlayersReadyFromTheStart)
                {
                    Ready = true;
                }
                if (!pl.IsDead && !Ready)
                {
                    if (ActionTimer <= 0)
                    {
                        if (IsDescription)
                        {
                            if (pl.IsBlocking)
                            {
                                if (EquipmentList[CurrentGroup].EquipmentList[Equipment[CurrentGroup]].Description != "")
                                {
                                    IsDescription = !IsDescription;
                                    Change = true;
                                    ActionTimer = 25;
                                }
                            }
                        }
                        else
                        {
                            if (pl.IsInMidAir)
                            {
                                CurrentGroup = Math.Max(CurrentGroup - 1, 0);
                                ActionTimer = 25;
                                Change = true;
                            }
                            else if (pl.IsMeleeAttacking && CurrentPoints <= LevelList[Player.Level].AllowPoints)
                            {
                                Ready = true;
                                CurrentGroup = -1;
                                Change = true;
                            }
                            else if (pl.IsCrouching)
                            {
                                CurrentGroup = Math.Min(CurrentGroup + 1, Equipment.Count - 1);
                                ActionTimer = 15;
                                Change = true;
                            }
                            else if (pl.IsRunning)
                            {
                                if (pl.FacingDirection == 1)
                                {
                                    for (int i = Equipment[CurrentGroup] + 1; i < EquipmentList[CurrentGroup].EquipmentList.Count; i++)
                                    {
                                        if (EquipmentList[CurrentGroup].EquipmentList[i].Level <= Player.Level && EquipmentList[CurrentGroup].EquipmentList[i].AccessLevel <= AccessLevel)
                                        {
                                            Equipment[CurrentGroup] = i;
                                            Change = true;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = Equipment[CurrentGroup] - 1; i >= 0; i--)
                                    {
                                        if (EquipmentList[CurrentGroup].EquipmentList[i].Level <= Player.Level && EquipmentList[CurrentGroup].EquipmentList[i].AccessLevel <= AccessLevel)
                                        {
                                            Equipment[CurrentGroup] = i;
                                            Change = true;
                                            break;
                                        }
                                    }
                                }
                                ActionTimer = 15;
                            }
                            else if (pl.IsBlocking)
                            {
                                if (EquipmentList[CurrentGroup].EquipmentList[Equipment[CurrentGroup]].Description != "")
                                {
                                    IsDescription = !IsDescription;
                                    Change = true;
                                    ActionTimer = 25;
                                }
                            }
                        }
                    }
                    else
                    {
                        ActionTimer--;
                    }
                }
                if (Change)
                {
                    int lineCount = 14;
                    string text = "";
                    text += Player.Name + "\n";
                    CurrentPoints = 0;
                    for (int i = 0; i < Equipment.Count; i++)
                    {
                        CurrentPoints += EquipmentList[i].EquipmentList[Equipment[i]].Cost;
                    }
                    if (LevelList.Count > 0)
                    {
                        text += "Level " + (Player.Level + 1).ToString() + " \"" + LevelList[Player.Level].Name + "\"";
                        if (AccessLevel == 1) text += " [$]";
                        text += "\n";
                        if (Player.Level + 1 < LevelList.Count)
                        {
                            int percent = (int)((float)Player.CurrentExp / (float)LevelList[Player.Level + 1].NeedExp * 10);
                            text += "[";
                            for (int i = 0; i < 10; i++)
                            {
                                if (i < percent)
                                {
                                    text += "=";
                                }
                                else
                                {
                                    text += "_";
                                }
                            }
                            text += "] " + Player.CurrentExp + "/" + LevelList[Player.Level + 1].NeedExp + "\n";
                        }
                        if (Ready)
                        {
                            Menu.SetTextColor(Color.Green);
                            text += "READY TO BATTLE\n";
                        }
                        else
                        {
                            text += "Equipment: " + CurrentPoints.ToString() + "/" + LevelList[Player.Level].AllowPoints + "\n\n";
                            if (IsDescription)
                            {
                                int maxLength = 30;
                                string[] words = (EquipmentList[CurrentGroup].EquipmentList[Equipment[CurrentGroup]].Name + ": " + EquipmentList[CurrentGroup].EquipmentList[Equipment[CurrentGroup]].Description).Split(' ');
                                int currentLen = 0;
                                for (int i = 0; i < words.Length; i++)
                                {
                                    if (currentLen + words[i].Length > maxLength)
                                    {
                                        text += "\n";
                                        currentLen = 0;
                                    }
                                    text += words[i] + " ";
                                    currentLen += words[i].Length + 1;
                                }
                            }
                            else
                            {
                                for (int i = 0; i < Equipment.Count; i++)
                                {
                                    if (EquipmentList[i].EquipmentList.Count <= Equipment[i]) Equipment[i] = 0;
                                    text += ((CurrentGroup == i) ? ">" : "") + EquipmentList[i].Name + ": ";
                                    switch (EquipmentList[i].EquipmentList[Equipment[i]].AccessLevel)
                                    {
                                        case 1: text += "[$]"; break;
                                        case 2: text += "[TEST]"; break;
                                    }
                                    text += EquipmentList[i].EquipmentList[Equipment[i]].Name + "[" + EquipmentList[i].EquipmentList[Equipment[i]].Cost.ToString() + "]\n";
                                }
                                if (CurrentPoints > LevelList[Player.Level].AllowPoints)
                                {
                                    Menu.SetTextColor(Color.Red);
                                }
                                else
                                {
                                    Menu.SetTextColor(Color.White);
                                }
                            }
                        }
                    }
                    else
                    {
                        Menu.SetTextColor(Color.Green);
                    }

                    int additionLines = lineCount - text.Split('\n').Length;
                    for (int i = 0; i < additionLines; i++)
                    {
                        text += "\n";
                    }
                    Menu.SetText(text);
                }
            }
            public void SpawnPlayer(Vector2 position)
            {
                if (Player == null) return;
                if (!Player.IsActive()) return;
                if (!Ready && CurrentPoints <= LevelList[Player.Level].AllowPoints) Ready = true;
                Player.Respawn();
                if (Player.User.GetPlayer() != null) Player.User.GetPlayer().Remove();
                IPlayer newPlayer = GlobalGame.CreatePlayer(position);
                newPlayer.SetUser(Player.User);
                newPlayer.SetTeam(Player.Team);
                //newPlayer.SetStatusBarsVisible(false);
                if (Ready)
                {
                    for (int i = 0; i < Equipment.Count; i++)
                    {
                        Player.AddEquipment(EquipmentList[i].EquipmentList[Equipment[i]].Id, i);
                    }
                }

                if (KeepPlayersSkins)
                {
                    newPlayer.SetProfile(Player.User.GetProfile());
                }
                else
                {
                    newPlayer.SetProfile(Player.GetSkin());
                }

                Player.OnPlayerCreated();
                newPlayer.SetInputEnabled(false);
            }
        }

        #endregion

        #region Player Menus Methods
        private void PreparePlayerMenus()
        {
            for (int i = 0; i <= 7; i++)
            {
                PlayerMenuList.Add(new TPlayerMenu()
                {
                    Menu = (IObjectText)Game.GetSingleObjectByCustomId("PlayerMenu" + (i).ToString()),
                });
            }
        }

        private void RefreshPlayerMenus()
        {

            IUser[] users = Game.GetActiveUsers();

            if (users == null || users.Length == 0) return;

            foreach (var u in users)
            {
                var menus = PlayerMenuList.Where(m => m.Player != null && m.Player.User != null && m.Player.User.Name == u.Name).ToList();
                if (menus.Count > 1)
                {
                    DebugLogger.DebugOnlyDialogLog("PLAYER " + u.Name + " HAD MORE THAN ONE MENU. DELETING THE OTHER MENUS");
                    menus[1].Player = null;
                    menus[1].Menu.SetText(string.Empty);
                    menus[1].Ready = false;
                }
            }

            if (PlayerList.Count == users.Length) return;
            try
            {
                int initialPlayercount = PlayerList.Count;



                if (PlayerList.Count < users.Length)
                {
                    var availableMenus = PlayerMenuList.Where(m => m.Player == null || m.Player.User == null).ToList();
                    for (int i = 0; i < users.Length; i++)
                    {
                        IUser currentUser = null;
                        currentUser = users[i];
                        if (currentUser == null) continue;
                        if (currentUser.IsSpectator) continue;

                        if (PlayerList.Any(p => p.User.Name.Equals(currentUser.Name)))
                        {
                            continue;
                        }

                        if (currentUser.GetPlayer() == null)
                        {
                            DebugLogger.DebugOnlyDialogLog("SPAWNING BODY FOR PLAYER " + currentUser.Name, CameraPosition);
                            var positionToSpawn = Game.GetSingleObjectByCustomId("StartSpawnPoint").GetWorldPosition();
                            var p = Game.CreatePlayer(positionToSpawn);
                            p.SetTeam(PlayerTeam.Team3);
                            p.SetUser(currentUser);
                            p.SetProfile(currentUser.GetProfile());
                            p.SetWorldPosition(positionToSpawn);
                        }

                        var player = new TPlayer(currentUser);



                        if (availableMenus.Count == 0)
                        {
                            DebugLogger.DebugOnlyDialogLog("NO AVAILABLE MENUS FOR PLAYER " + player.Name);
                            return;
                        }
                        TPlayerMenu menu = null;
                        menu = availableMenus.FirstOrDefault();
                        availableMenus.RemoveAt(availableMenus.IndexOf(menu));
                        if (menu == null)
                        {
                            return;
                        }


                        try
                        {
                            var playerFound = false;
                            if (!string.IsNullOrEmpty(OtherData))
                            {
                                // try to look for player in OtherData
                                var playerList = OtherData.Split(';');

                                for (int p = 0; p < playerList.Length; p++)
                                {
                                    string[] plData = playerList[p].Split(':');
                                    if (plData.Length == 11)
                                    {
                                        List<string> list = new List<string>(plData);
                                        list.RemoveAt(3);
                                        plData = list.ToArray();
                                    }

                                    string name = FormatName(player.Name);
                                    if (name == plData[0])
                                    {
                                        menu.SetPlayer(player);
                                        player.Level = Math.Min(Convert.ToInt32(plData[1]), LevelList.Count - 1);
                                        player.CurrentExp = Convert.ToInt32(plData[2]);
                                        for (int u = 0; u < menu.Equipment.Count; u++)
                                        {
                                            menu.Equipment[u] = Convert.ToInt32(plData[3 + u]);
                                        }
                                        menu.ValidateEquipment();
                                        menu.Change = true;
                                        menu.Update();
                                        playerFound = true;
                                        DebugLogger.DebugOnlyDialogLog("PLAYER FOUND IN MEMORY DATA. WELCOME BACK!" + player.Name);
                                        break;
                                    }

                                }
                            }

                            if (!playerFound)
                            {
                                DebugLogger.DebugOnlyDialogLog("ASSIGNING MENU TO PLAYER " + player.Name);
                                if (ShowDebugMessages && !string.IsNullOrEmpty(OtherData))
                                {
                                    DebugLogger.DebugOnlyDialogLog("THEY ARE A NEW PLAYER! WELCOME " + player.Name + "!");
                                }
                                menu.SetPlayer(player);
                                menu.Change = true;
                                menu.Update();
                            }
                        }
                        catch (Exception e)
                        {
                            DebugLogger.DebugOnlyDialogLog(e.Message);
                            DebugLogger.DebugOnlyDialogLog(e.StackTrace);
                            DebugLogger.DebugOnlyDialogLog("COULDN'T INSTANTIATE PLAYER " + player.Name);
                        }

                        PlayerList.Add(player);
                    }

                }


                if (PlayerList.Count > users.Length)
                {
                    DebugLogger.DebugOnlyDialogLog("More players in PlayersList than active users");
                    for (int i = PlayerMenuList.Count - 1; i >= 0; i--)
                    {
                        TPlayer player = null;
                        try
                        {

                            player = PlayerList.ElementAtOrDefault(i);
                        }
                        catch (Exception)
                        {
                            DebugLogger.DebugOnlyDialogLog("PlayerList.ElementAtOrDefault(i) was out of range, i:" + (i).ToString(), CameraPosition);
                        }

                        if (player == null)
                        {
                            continue;
                        }
                        TPlayerMenu menu = null;
                        try
                        {
                            menu = PlayerMenuList.Where(m => m.Player != null && m.Player.Name != null && m.Player.Name.Equals(player.Name)).FirstOrDefault();
                        }
                        catch (Exception e)
                        {
                            DebugLogger.DebugOnlyDialogLog("PlayerMenuList.Where(m => m.Player.Name.Equals(m.Player)).FirstOrDefault()" + (i).ToString(), CameraPosition);
                            DebugLogger.DebugOnlyDialogLog(e.Message + (i).ToString(), CameraPosition);
                            DebugLogger.DebugOnlyDialogLog(e.StackTrace + (i).ToString(), CameraPosition);
                        }


                        if (menu == null)
                        {
                            DebugLogger.DebugOnlyDialogLog("MENU FOR PLAYER " + player.Name + " WAS NULL. IF HE LEFT HIS PROGRESS WILL NOT BE SAVED!!");
                            continue;
                        }

                        if (!users.Any(u => u.Name != null && u.Name.Equals(player.User.Name)) && GameState != -3 && GameState != -4)
                        {
                            DebugLogger.DebugOnlyDialogLog("REMOVING MENU FOR: " + player.User.Name + ". LEVEL " + player.Level + ". USER HAS LEFT THE MATCH");
                            var playersInMemoryCountBefore = OtherData.Split(';').Length;
                            var leavingPlayerData = menu.Save(saveOnlyIfActive: false);
                            OtherData += leavingPlayerData;
                            var playersInMemoryCountAfter = OtherData.Split(';').Length;
                            menu.Dispose();
                            PlayerList.Remove(player);
                            if (player.Body != null)
                            {
                                player.Body.Remove();
                                DebugLogger.DebugOnlyDialogLog("REMOVED BODY FOR PLAYER " + player.User.Name);
                            }
                            else
                            {
                                DebugLogger.DebugOnlyDialogLog("COULDN'T REMOVE BODY FOR PLAYER " + player.User.Name);
                            }
                            DebugLogger.DebugOnlyDialogLog("REMOVED MENU FOR: " + player.User.Name + "SUCCESFULLY. DATA ADDED TO MEMORY PLAYERS");
                            DebugLogger.DebugOnlyDialogLog(playersInMemoryCountBefore + " LOADED IN MEMORY BEFORE AND " + playersInMemoryCountAfter + " AFTER. " + PlayerList.Count + " IN MATCH");
                            DebugLogger.DebugOnlyDialogLog("SAVED DATA FOR LEAVING PLAYER " + leavingPlayerData);
                        }
                    }
                }

                // ONLY LOAD DATA ONCE PER GAME
                if (OtherData == string.Empty)
                {
                    LoadData();
                }

            }
            catch (Exception e)
            {
                DebugLogger.DebugOnlyDialogLog(e.StackTrace, CameraPosition);
                DebugLogger.DebugOnlyDialogLog(e.Source, CameraPosition);
            }

        }

        #endregion

        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
