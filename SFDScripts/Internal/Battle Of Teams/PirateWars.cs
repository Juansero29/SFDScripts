using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SFDGameScriptInterface;

namespace SFDScripts.Internal
{
    class PirateWars : GameScriptInterface
    {
        public PirateWars() : base(null) { }

        #region Script To Copy
        private string[] _Classes = new string[] { "s", "d", "f", "g", "h" };
        private Random rnd;
        int TimeCDT = 5 * 60;
        bool TextF = false;

        public void TimeRemain(TriggerArgs args)
        {
            IObjectText TimeText = (IObjectText)Game.GetSingleObjectByCustomId("TextCD");
            if (TimeText != null)
            {
                if (TextF == false)
                {
                    TimeCDT--;
                    TimeText.SetText("Time Remaining: " + TimeCDT.ToString());
                }

                if (TimeCDT == 0)
                {
                    WhoWins(args);
                }
            }
        }


        public void ClearPopup(TriggerArgs args)
        {
            Game.HidePopupMessage();
        }

        int vRedKills = 0;

        int vBluKills = 0;

        public void WhoWins(TriggerArgs args = null)
        {
            IObjectText TimeText = (IObjectText)Game.GetSingleObjectByCustomId("TextCD");

            if (vRedKills > vBluKills)
            {
                Game.SetGameOver("The Red wins");
                TextF = true;
                TimeText.SetText("Finished, space for the next one" + "\n             <-- " + vBluKills + " X " + vRedKills + " -->");
            }

            if (vBluKills > vRedKills)
            {
                Game.SetGameOver("The Blue wins");  // if limit is exceeded, restart round
                TextF = true;
                TimeText.SetText("Finished, space for the next one" + "\n             <-- " + vBluKills + " X " + vRedKills + " -->");
            }

            if (vBluKills == vRedKills)
            {
                Game.SetGameOver("Draw!");  // if limit is exceeded, restart round
                TextF = true;
                TimeText.SetText("Finished, space for the next one" + "\n             <-- " + vBluKills + " X " + vRedKills + " -->");
            }
        }


        public void OnEnter(TriggerArgs args)
        {

            var player = args.Sender as IPlayer;
            if (player == null) return;
            try
            {
                player.SetTeam(PlayerTeam.Team3);

                Game.CreateDialogue(player.Name + " entered the game", player.GetWorldPosition());

                // If it's not a bot, we don't need to do anything else
                if (!player.IsBot)
                {
                    Game.CreateDialogue(player.Name + " is not a bot", player.GetWorldPosition());
                    return;
                }

                Game.CreateDialogue(player.Name + " is a bot", player.GetWorldPosition());
                var caller = Game.GetSingleObjectByCustomID("RedBase");
                var args2 = new TriggerArgs(caller, player, false);
                MovetoBase(args2);

                // If now, we're not part of green team, the bot has spawned successfully
                if (player.GetTeam() != PlayerTeam.Team3)
                {
                    return;
                }

                caller = Game.GetSingleObjectByCustomID("BlueBase");
                var args3 = new TriggerArgs(caller, player, false);
                MovetoBase(args3);

                rnd = new Random();
                var i = rnd.Next(0, _Classes.Length - 1);

                var o = Game.GetFirstObject(_Classes[i]) as IObjectActivateTrigger;
                if (o == null) return;
                ClassChooser(new TriggerArgs(o, player, false));
            }
            catch (Exception e)
            {
                Game.CreateDialogue("OnEntered failed for " + player.Name, player);
                Game.CreateDialogue(e.Message, player);
                Game.CreateDialogue(e.StackTrace, player);
            }

        }

        public void CheckEnter(TriggerArgs args)
        {
            if ((args.Sender != null) && (args.Sender is IPlayer))
            {
            }
        }

        Vector2 here;
        IObject wee;
        bool didit = false;

        public void crateOpen(TriggerArgs args)
        {
            if (wee.GetHealth() == 0 && didit == false)
            {
                Game.SpawnWeaponItem(WeaponItem.FLAMETHROWER, here, true, 10000);
                didit = true;
                Game.GetSingleObjectByCustomId("IT").Destroy();
            }
            else { here = wee.GetWorldPosition(); }
        }

        String VBlueRnd;

        public void BlueRnd(TriggerArgs args = null)
        {
            int whereB = RandNumber(0, 4);
            string[] GetposB = new string[] { "BluBase", "BluBase2", "BluBase3", "BluBase4", "BluBase5" };
            VBlueRnd = (GetposB[whereB]);
        }

        String VRedRnd;

        public void RedRnd(TriggerArgs args = null)
        {
            int whereR = RandNumber(0, 4);
            String[] GetposR = new string[] { "RediBase", "RedBase2", "RedBase3", "RedBase4", "RedBase5" };
            VRedRnd = (GetposR[whereR]);
        }


        public void refil(TriggerArgs args)
        {
            IPlayer helped = (IPlayer)args.Sender;
            helped.GiveWeaponItem(WeaponItem.PILLS);
            helped.GiveWeaponItem(helped.CurrentSecondaryWeapon.WeaponItem);
            if (helped.CurrentPrimaryWeapon.WeaponItem != WeaponItem.MAGNUM)
            {
                helped.GiveWeaponItem(helped.CurrentPrimaryWeapon.WeaponItem);
            }
        }

        public void ClassChooser(TriggerArgs args)
        {
            IPlayer sender = (IPlayer)args.Sender;
            if (sender == null)
            {
                Game.CreateDialogue("Class chooser sender is null", Vector2.Zero);
            }
            if ((sender != null) && (sender is IPlayer) && (!sender.IsDiving))
            {
                if (sender.GetTeam() == PlayerTeam.Team3) return;
                IObject Clas = (IObject)args.Caller;

                Game.CreateDialogue("Class chosen for " + sender.Name + " is " + Clas.CustomId, Vector2.Zero);
                switch (Clas.CustomId)
                {

                    case "i":
                        sender = (IPlayer)args.Sender;
                        removeWeapons(sender);
                        sender.GiveWeaponItem(WeaponItem.CARBINE);
                        sender.GiveWeaponItem(WeaponItem.GRENADES);
                        sender.GiveWeaponItem(WeaponItem.BAT);
                        sender.GiveWeaponItem(WeaponItem.SLOWMO_5);
                        if (sender.GetTeam() == PlayerTeam.Team1) { getAway(VBlueRnd, sender); }
                        else { getAway(VRedRnd, sender); }

                        break;




                    case "a":
                        sender = (IPlayer)args.Sender;
                        removeWeapons(sender);
                        sender.GiveWeaponItem(WeaponItem.CARBINE);
                        sender.GiveWeaponItem(WeaponItem.UZI);
                        sender.GiveWeaponItem(WeaponItem.ASSAULT);
                        sender.GiveWeaponItem(WeaponItem.UZI);
                        sender.GiveWeaponItem(WeaponItem.ASSAULT);
                        sender.GiveWeaponItem(WeaponItem.UZI);
                        sender.GiveWeaponItem(WeaponItem.SLOWMO_5);
                        if (sender.GetTeam() == PlayerTeam.Team1) { getAway(VBlueRnd, sender); }
                        else { getAway(VRedRnd, sender); }

                        break;


                    case "s":
                        sender = (IPlayer)args.Sender;
                        removeWeapons(sender);
                        sender.GiveWeaponItem(WeaponItem.MACHETE);
                        sender.GiveWeaponItem(WeaponItem.MAGNUM);
                        if (sender.GetTeam() == PlayerTeam.Team1) { getAway(VBlueRnd, sender); }
                        else { getAway(VRedRnd, sender); }

                        break;


                    case "d":
                        sender = (IPlayer)args.Sender;
                        removeWeapons(sender);
                        sender.GiveWeaponItem(WeaponItem.CARBINE);
                        sender.GiveWeaponItem(WeaponItem.BAT);
                        sender.GiveWeaponItem(WeaponItem.REVOLVER);
                        if (sender.GetTeam() == PlayerTeam.Team1) { getAway(VBlueRnd, sender); }
                        else { getAway(VRedRnd, sender); }

                        break;


                    case "f":
                        sender = (IPlayer)args.Sender;
                        removeWeapons(sender);
                        sender.GiveWeaponItem(WeaponItem.SNIPER);
                        sender.GiveWeaponItem(WeaponItem.PISTOL);
                        if (sender.GetTeam() == PlayerTeam.Team1) { getAway(VBlueRnd, sender); }
                        else { getAway(VRedRnd, sender); }

                        break;


                    case "g":
                        sender = (IPlayer)args.Sender;
                        removeWeapons(sender);
                        sender.GiveWeaponItem(WeaponItem.PIPE);
                        sender.GiveWeaponItem(WeaponItem.SHOTGUN);
                        sender.GiveWeaponItem(WeaponItem.REVOLVER);
                        if (sender.GetTeam() == PlayerTeam.Team1) { getAway(VBlueRnd, sender); }
                        else { getAway(VRedRnd, sender); }

                        break;


                    case "h":
                        sender = (IPlayer)args.Sender;
                        removeWeapons(sender);
                        sender.GiveWeaponItem(WeaponItem.AXE);
                        sender.GiveWeaponItem(WeaponItem.SUB_MACHINEGUN);
                        sender.GiveWeaponItem(WeaponItem.PISTOL);
                        if (sender.GetTeam() == PlayerTeam.Team1) { getAway(VBlueRnd, sender); }
                        else { getAway(VRedRnd, sender); }

                        break;


                    default:
                        Game.ShowPopupMessage("Something is wrong, Check the ClassChooser void");
                        break;
                }
            }
        }


        public void getAway(String id, IPlayer movingPlayer)
        {
            Game.CreateDialogue(movingPlayer.Name + " is being teleported to " + id, movingPlayer);
            if (movingPlayer == null || id == null) return;
            IObject FinalDestination = Game.GetSingleObjectByCustomId(id);
            Vector2 wee = FinalDestination.GetWorldPosition();
            movingPlayer.SetWorldPosition(wee);
        }

        private IPlayer Staffplayer = null;
        int bluTeam = 0;
        int redTeam = 0;


        public void MovetoBase(TriggerArgs args)
        {
            IPlayer sender = (IPlayer)args.Sender;
            if ((sender != null) && (sender is IPlayer) && (!sender.IsDiving))
            {
                IObject Where = (IObject)args.Caller;
                IPlayer player = (IPlayer)args.Sender;
                IUser user = player.GetUser();

                switch (Where.CustomId)
                {
                    case "RedBase":
                        if (bluTeam >= redTeam)
                        {
                            player.SetTeam(PlayerTeam.Team2);
                            redTeam++;
                            if (player.IsBot) break;
                            getAway("Red", player);
                        }
                        else
                        {
                            Game.ShowPopupMessage("Choose another team");
                        }
                        break;

                    case "BlueBase":
                        if (redTeam >= bluTeam)
                        {
                            player.SetTeam(PlayerTeam.Team1);
                            bluTeam++;
                            if (player.IsBot) break;
                            getAway("Blue", player);
                        }
                        else
                        {
                            Game.ShowPopupMessage("Choose another team");
                        }

                        break;

                    default:
                        Game.ShowPopupMessage("Something is wrong, Check the MovetoBase void");
                        break;
                }

                if (!player.IsBot || player.GetTeam() == PlayerTeam.Team3) return;
                rnd = new Random();
                var i = rnd.Next(0, _Classes.Length - 1);

                var o = Game.GetFirstObject(_Classes[i]) as IObjectActivateTrigger;
                if (o == null) return;
                ClassChooser(new TriggerArgs(o, player, false));
            }
        }

        public void CheckTeams(PlayerTeam team)
        {
            if (team == PlayerTeam.Team1)
            {
                bluTeam--;
            }

            if (team == PlayerTeam.Team2)
            {
                redTeam--;
            }
        }


        public static int RandNumber(int Low, int High)
        {
            Random rndNum = new Random();

            int rnd = rndNum.Next(Low, High);

            return rnd;
        }

        public void XstroyGun(TriggerArgs args)
        {
            if (args.Sender is IObjectWeaponItem)
            {
                IObject destroyingGun = (IObject)args.Sender;
                destroyingGun.Remove();

            }
        }

        public void removeWeapons(IPlayer pleyrr)
        {
            pleyrr.RemoveWeaponItemType(WeaponItemType.Rifle);
            pleyrr.RemoveWeaponItemType(WeaponItemType.Handgun);
            pleyrr.RemoveWeaponItemType(WeaponItemType.Melee);
            pleyrr.RemoveWeaponItemType(WeaponItemType.Thrown);
            pleyrr.SetHealth(100);
        }


        //===========================================================//
        //=========<      DEATHMATCH GAMEMODE BY CHELOG      >=======//
        //=========<             MODIFIED BY GURT            >=======//
        //=========<                 V 0.2.2                 >=======//
        //===========================================================//
        //======================< DM - settings >====================//
        private const int DEATH_LIMIT = 10;               // DeathLimit after that round will restart (only integer)
        private const int USER_RESPAWN_DELAY_MS = 10000;        // Time in ms after a player will respawn
        private const bool GIB_CORPSES = false;               // if set to "true" - gib corpses; "false" - remove corpses
                                                              //===========================================================//

        public void Start(TriggerArgs args)
        {
            Events.PlayerDeathCallback.Start(Death);
            ConnectedPlayersTick(args);
            RefreshCounter(vBluKills, "BlueT");
            RefreshCounter(vRedKills, "RedT");
            BlueRnd(args);
            RedRnd(args);
            //Game.SetCurrentCameraMode(CameraMode.Dynamic);
            //Game.AddCameraFocus((IObjectText)Game.GetSingleObjectByCustomId("DeathCounterBl"));
            //Game.AddCameraFocus((IObjectText)Game.GetSingleObjectByCustomId("DeathCounterRe"));
            //Game.AddCameraFocus((IObject)Game.GetSingleObjectByCustomId("Middle"));
            wee = ((IObject)Game.GetSingleObjectByCustomId("crate"));
            here = wee.GetWorldPosition();
        }

        private class DeadPlayer
        {
            public float Timestamp = 0f;
            public IUser User = null;
            public IPlayer DeadBody = null;
            public PlayerTeam Team = PlayerTeam.Independent;
            public DeadPlayer(float timestamp, IUser user, IPlayer deadBody, PlayerTeam team)
            {
                this.Timestamp = timestamp;
                this.User = user;
                this.DeadBody = deadBody;
                this.Team = team;
            }
        }

        private List<DeadPlayer> m_deadPlayers = new List<DeadPlayer>();
        private int deathNum = 0;
        private int usersConnectedTickDelay = 15;

        // called each 200ms, makes a game tick
        public void Tick(TriggerArgs args)
        {
            Fly(args);
            RespawnTick(args);
            ConnectedPlayersTick(args);
            BlueRnd(args);
            RedRnd(args);
            if (Staffplayer != null)
                Staffplayer.SetHealth(100);
        }



        // called on player's death
        public void Death(IPlayer killedPlayer, PlayerDeathArgs args)
        {
            BlueRnd();
            RedRnd();

            // refresh death counter on the map
            if (killedPlayer != null && args.Removed == false)
            {
                deathNum++;
                if (killedPlayer.GetTeam() == PlayerTeam.Team1)
                {
                    vRedKills++;
                    RefreshCounter(vRedKills, "RedT");
                }
                if (killedPlayer.GetTeam() == PlayerTeam.Team2)
                {
                    vBluKills++;
                    RefreshCounter(vBluKills, "BlueT");
                }


                if (vRedKills < DEATH_LIMIT || vBluKills < DEATH_LIMIT)
                {
                    IUser user = killedPlayer.GetUser();
                    if (user == null)
                    {
                        Game.CreateDialogue(killedPlayer.Name + " not added to dead list because user is null.", killedPlayer);
                        return;
                    }

                    //store user to respawn and body to remove
                    m_deadPlayers.Add(new DeadPlayer(Game.TotalElapsedGameTime, user, killedPlayer, killedPlayer.GetTeam()));
                    Game.CreateDialogue(killedPlayer.Name + " added to dead list", killedPlayer);
                }
                else { WhoWins(); }
            }
        }


        public void RespawnTick(TriggerArgs args)
        {
            if (m_deadPlayers.Count > 0)
            {
                for (int i = m_deadPlayers.Count - 1; i >= 0; i--)
                { // traverse list backwards for easy removal of elements in list
                    DeadPlayer deadPlayer = m_deadPlayers[i];
                    if (deadPlayer.Timestamp + USER_RESPAWN_DELAY_MS < Game.TotalElapsedGameTime)
                    {
                        Game.CreateDialogue("Trying to respawn " + deadPlayer.User.Name, deadPlayer.DeadBody.GetWorldPosition());
                        // time to respawn this user
                        // remove entry from list over deadPlayers
                        m_deadPlayers.RemoveAt(i);
                        // remove old body (if any)
                        if (deadPlayer.DeadBody != null)
                        {
                            if (GIB_CORPSES) { deadPlayer.DeadBody.Gib(); } else { (deadPlayer.DeadBody).Remove(); }
                        }
                        // respawn user
                        IPlayer ply = deadPlayer.User.GetPlayer();
                        if (((ply == null) || (ply.IsDead)))
                        {
                            Game.CreateDialogue("Calling SpawnUser(" + deadPlayer.User.Name + ", " + deadPlayer.Team.ToString() + ")", deadPlayer.DeadBody);
                            SpawnUser(deadPlayer.User, deadPlayer.Team);
                        }
                    }
                }
            }
        }

        int usersConnected = 0;
        //bool justConnected = true;
        public void ConnectedPlayersTick(TriggerArgs args)
        {
            if (usersConnectedTickDelay > 0)
            {
                usersConnectedTickDelay--;
                if (usersConnectedTickDelay <= 0)
                {
                    IUser[] users = Game.GetActiveUsers(); // get all players
                    if (usersConnected == 0) { usersConnected = users.Length; } // update amount of users at start
                    else if (users.Length > usersConnected)
                    {
                        // check users list for non-spawned users
                        for (int i = 0; i < users.Length; i++)
                        {
                            IPlayer ply = users[i].GetPlayer();
                            if ((ply == null)) SpawnUser2(users[i]);
                        }
                    }
                    usersConnected = users.Length; // update number of connected users
                    IObjectText textUsersHere = (IObjectText)Game.GetSingleObjectByCustomId("UsersHere");
                    textUsersHere.SetText(usersConnected.ToString());
                    string playerList = "";
                    for (int i = 0; i < users.Length; i++)
                        playerList += users[i].GetProfile().Name + "\n";
                    if (users.Length < 8) for (int i = users.Length - 1; i < 8; i++)
                            playerList += " \n";
                    IObjectText textUsersList = (IObjectText)Game.GetSingleObjectByCustomId("UsersList");
                    textUsersList.SetText(playerList);
                    usersConnectedTickDelay = 15;
                }
            }
        }

        Vector2 spawnPos;
        private void SpawnUser(IUser user, PlayerTeam team)
        {
            if (CheckUserStillActive(user))
            {
                IPlayer ply = user.GetPlayer();
                IPlayer newPlayer = Game.CreatePlayer(spawnPos); // create a new blank player

                Game.CreateDialogue("Respawning " + ply.Name, ply);
                if (ply.IsBot)
                {
                    Game.CreateDialogue(ply.Name + " is a bot", ply);
                    var botBehavior = new BotBehavior(true, PredefinedAIType.BotD);
                    ply.SetBotBehavior(botBehavior);
                    newPlayer.SetBotBehavior(botBehavior);

                    rnd = new Random();
                    var i = rnd.Next(0, _Classes.Length - 1);

                    var o = Game.GetFirstObject(_Classes[i]) as IObjectActivateTrigger;
                    if (o == null)
                    {
                        Game.CreateDialogue(ply.Name + " not teleported because _Classes[" + i + "] is null.", ply);
                        return;
                    }

                    Game.CreateDialogue("Going to choose class for bot " + ply.Name, ply);
                    ClassChooser(new TriggerArgs(o, ply, false));
                    return;
                }

                if (team == PlayerTeam.Team1)
                {
                    newPlayer.SetTeam(PlayerTeam.Team1);
                    spawnPos = Game.GetSingleObjectByCustomId("Blue").GetWorldPosition();
                }
                if (team == PlayerTeam.Team2)
                {
                    newPlayer.SetTeam(PlayerTeam.Team2);
                    spawnPos = Game.GetSingleObjectByCustomId("Red").GetWorldPosition();
                }
                if (team.Equals(PlayerTeam.Team3))
                {
                    newPlayer.SetTeam(PlayerTeam.Team3);
                    spawnPos = Game.GetSingleObjectByCustomID("Middle").GetWorldPosition();
                }

                newPlayer.SetUser(user); // set user (this will make the user control the player instance)
                newPlayer.SetProfile(user.GetProfile()); // set user's profile
                newPlayer.SetWorldPosition(spawnPos);
            }
            else CheckTeams(team);
        }

        private void SpawnUser2(IUser user)
        {
            if (CheckUserStillActive(user))
            {
                spawnPos = Game.GetSingleObjectByCustomId("Middle").GetWorldPosition();

                IPlayer newPlayer = Game.CreatePlayer(spawnPos); // create a new blank player
                newPlayer.SetUser(user); // set user (this will make the user control the player instance)
                newPlayer.SetProfile(user.GetProfile()); // set user's profile
                newPlayer.SetWorldPosition(spawnPos);
                newPlayer.SetTeam(PlayerTeam.Team3);
            }
        }

        // Checks if the user is still present in the game
        private bool CheckUserStillActive(IUser user)
        {
            foreach (IUser activeUser in Game.GetActiveUsers())
            {
                if (activeUser.UserId == user.UserId)
                {
                    return true;
                }
            }
            return false;
        }


        private void RefreshCounter(int value, string team)
        {
            IObjectText DeathCounterB = (IObjectText)Game.GetSingleObjectByCustomId("DeathCounterBl");
            IObjectText DeathCounterR = (IObjectText)Game.GetSingleObjectByCustomId("DeathCounterRe");

            if (TextF != true)
            {
                if (DeathCounterB != null && team == "BlueT")
                    DeathCounterR.SetText("Blue score: " + value.ToString());

                if (DeathCounterR != null && team == "RedT")
                    DeathCounterB.SetText("Red score: " + value.ToString());
            }
        }



        int buttonPressCount = 0;
        public void Fly(TriggerArgs args)
        {
            buttonPressCount++;

            Vector2 worldPosition = Vector2.Zero;
            Vector2 flying = Game.GetSingleObjectByCustomId("fly").GetLinearVelocity();
            if (Staffplayer != null)
            {
                worldPosition = Staffplayer.GetWorldPosition() + new Vector2(0f, -8.15f);
                if (Staffplayer != null && buttonPressCount <= 10 && !(Staffplayer.IsCrouching) && (Staffplayer.IsInMidAir))
                {
                    worldPosition = Staffplayer.GetWorldPosition() + new Vector2(0f, -8.15f);
                    IObject createdObject = Game.CreateObject("InvisiblePlatform", worldPosition, 0f, flying, 0f);
                    createdObject.CustomId = "Fly";
                }
                else
                {
                    buttonPressCount = 0;
                    IObject[] flyobject = Game.GetObjectsByCustomId("Fly");
                    for (int j = 0; j < flyobject.Length; j++)
                    {
                        flyobject[j].Remove();
                    }
                    if (!(Staffplayer.IsCrouching) && !(Staffplayer.IsInMidAir) && !(Staffplayer.IsStaggering))
                    {
                        IObject createdObject = Game.CreateObject("InvisiblePlatform", worldPosition, 0f, flying, 0f);
                        createdObject.CustomId = "Flying";
                    }
                }
            }
        }

        public void Delete(TriggerArgs args)
        {
            IObject[] flyingobj = Game.GetObjectsByCustomId("Flying");
            for (int i = 0; i < flyingobj.Length; i++)
            {
                flyingobj[i].Remove();
            }
        }

        #endregion ScriptToCopy
    }
}
