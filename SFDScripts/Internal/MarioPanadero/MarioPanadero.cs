using System;
using System.Collections.Generic;
using SFDGameScriptInterface;


namespace SFDScripts
{
    class MarioPanadero : GameScriptInterface
    {

        public MarioPanadero() : base(null) { }


        #region Script To Copy

        #region Settings

        /// <summary>
        /// The user respawn rate delay in miliseconds
        /// </summary>
        private const int USER_RESPAWN_DELAY_MS = 5000;

        /// <summary>
        /// Total duration of the game declared in seconds
        /// </summary>
        private const int TOTAL_GAME_TIME_IN_SECONDS = 4 * 60;
        #endregion

        #region Declarations
        private enum Teams
        {
            Blue,
            Red
        }

        private class DeadPlayer
        {
            /// <summary>
            /// The death time of this dead player
            /// </summary>
            public float DeathTimeStamp { get; set; }

            /// <summary>
            /// The user tied to this dead player
            /// </summary>
            public IUser User { get; set; }
            /// <summary>
            /// The player's body
            /// </summary>
            public IPlayer DeadBody { get; set; }

            /// <summary>
            /// The team of this dead player
            /// </summary>
            public PlayerTeam Team { get; set; }

            /// <summary>
            /// Wether it is a bot or not
            /// </summary>
            public bool IsBot { get; set; }

        }
        #endregion

        #region Constants

        #region ObjectIds
        private const string TIME_REMAINING_LABEL_ID = "TimeRemainingLabel";

        private const string PLAYER_MIDDLE_SPAWN_BLOCK_ID = "PlayerSpawnBlock";

        private const string PEACH_POSITION_BLOCK_ID = "PeachPositionBlock";

        private const string BLUE_KILLS_COUNT_TEXT_ID = "BlueKillsCountText";
        private const string RED_KILLS_COUNT_TEXT_ID = "RedKillsCountText";
        private const string USERS_COUNT_TEXT_ID = "UsersCountText";
        private const string USERS_LIST_TEXT_ID = "UsersListText";

        #endregion

        #region Team Positions
        /// <summary>
        /// The name's of the different positions for red spawning points
        /// </summary>
        private readonly string[] RedSpawnPositions = new string[]
        {
                "RedBase",
                "RedBase2",
                "RedBase3",
                "RedBase4",
                "RedBase5"
        };

        /// <summary>
        /// The name's of the different positions for blue spawning points
        /// </summary>
        private readonly string[] BlueSpawnPositions = new string[]
        {
                    "BlueBase",
                    "BlueBase2",
                    "BlueBase3",
                    "BlueBase4",
                    "BlueBase5"
        };
        #endregion



        #region Utilities
        /// <summary>
        /// The random generator for this map
        /// </summary>
        private readonly Random _Rnd = new Random();
        #endregion

        #endregion

        #region Private Fields

        /// <summary>
        /// The total time remaining for this game in seconds
        /// </summary>
        private int _RemainingGameTimeInSeconds = TOTAL_GAME_TIME_IN_SECONDS;

        /// <summary>
        /// The text object showing the time remaining
        /// </summary>
        private IObjectText _TimeRemainingText;

        /// <summary>
        /// The list of all the players marked as marios in this game
        /// </summary>
        private List<IPlayer> _Marios = new List<IPlayer>();

        /// <summary>
        /// The number of kills the red team has made
        /// </summary>
        private int _RedKillsCount = 0;

        /// <summary>
        /// The number of kills the blue team has made
        /// </summary>
        private int _BlueKillsCount = 0;

        /// <summary>
        /// The position at which players should spawn
        /// </summary>
        private Vector2 _PlayerSpawnPosition;

        /// <summary>
        /// Boolean indicating whether the time has already elapsed or not
        /// </summary>
        private bool _HasGameTimeFinished = false;

        /// <summary>
        /// Indicates the winner team
        /// </summary>
        private PlayerTeam winner = PlayerTeam.Team3;

        /// <summary>
        /// The number of blue team players
        /// </summary>
        private int _NumberOfBlueTeamPlayers = 0;

        /// <summary>
        /// The number of red team players
        /// </summary>
        private int _NumberOfRedTeamPlayers = 0;

        /// <summary>
        /// The list of dead players in the game
        /// </summary>
        private List<DeadPlayer> _DeadPlayers = new List<DeadPlayer>();

        /// <summary>
        /// The number of dead players in the game
        /// </summary>
        private int _DeadPlayersCount = 0;

        /// <summary>
        /// The number of users connected to the game
        /// </summary>
        private int _UsersConnectedCount = 0;
        #endregion

        #region Game Lifecycle


        /// <summary>
        /// The game will always call the following method "public void OnStartup()" during a map start (or script activates). 
        /// </summary>
        /// <remarks>
        /// No triggers required. This is run before triggers that activate on startup (and before OnStartup triggers).
        /// </remarks>
        public void OnStartup()
        {
            // This is the recommended place to hook up any events. To register an update loop:
            Events.UpdateCallback.Start(OnUpdate, 200);
            Events.PlayerDeathCallback.Start(Death);


            // can't use nameof(Method)
            CreateTrigger(1000, 0, "TimeRemaining");
            CreateTrigger(300, 0, "ShootFireball");
            CreateTrigger(5000, 0, "ClearPopup");

            RefreshKillCounter(_BlueKillsCount, Teams.Blue);
            RefreshKillCounter(_RedKillsCount, Teams.Red);
            _TimeRemainingText = Game.GetSingleObjectByCustomId(TIME_REMAINING_LABEL_ID) as IObjectText;

            SpawnPeach();
        }

        /// <summary>
        /// Update loop (must be enabled in the OnStartup() function or AfterStartup() function).
        /// </summary>
        /// <param name="elapsed">Time elapsed</param>
        public void OnUpdate(float elapsed)
        {
            RespawnTick();
            CheckConnectedPlayersTick();
        }

        /// <summary>
        /// The game will always call the following method "public void AfterStartup()" after a map start (or script activates). 
        /// </summary>
        /// <remarks>
        /// No triggers required. This is run after triggers that activate on startup (and after OnStartup triggers).
        /// </remarks>
        public void AfterStartup()
        {

        }

        /// <summary>
        /// The game will always call the following method "public void OnShutdown()" before a map restart (or script deactivates). 
        /// </summary>
        /// <remarks>
        /// Perform some cleanup here or store some final information to Game.Data/Game.LocalStorage/Game.SessionStorage if needed.
        /// </remarks>
        public void OnShutdown()
        {

        }
        #endregion

        #region Triggers

        #region Player Lifecycle Triggers

        /// <summary>
        /// Checks if there are any new active users that haven't been spawned
        /// </summary>
        /// <param name="args"></param>
        public void CheckConnectedPlayersTick()
        {
            var allUsers = Game.GetActiveUsers();

            for (int i = 0; i < allUsers.Length; i++)
            {
                var player = allUsers[i].GetPlayer();
                if (player == null)
                {
                    FirstSpawn(allUsers[i]);
                }
            }
            _UsersConnectedCount = allUsers.Length;

            UpdateUsersTextList(allUsers);
        }

        /// <summary>
        /// Tick called when a player dies
        /// </summary>
        /// <param name="args"></param>
        public void Death(IPlayer deadPlayer, PlayerDeathArgs args)
        {

            Game.WriteToConsole("Death() deadPlayer: ", deadPlayer.Name);
            Game.WriteToConsole("Death() deadPlayer.IsRemoved: ", deadPlayer.IsRemoved);
            Game.WriteToConsole("Death() args.IsRemoved: ", args.Removed);

            if (deadPlayer == null || deadPlayer.IsRemoved || args.Removed)
            {
                return;
            }

            _DeadPlayersCount++;

            if (deadPlayer.GetTeam() == PlayerTeam.Team1)
            {
                Game.WriteToConsole("Blue player dead");
                _RedKillsCount++;
                RefreshKillCounter(_RedKillsCount, Teams.Red);
            }
            if (deadPlayer.GetTeam() == PlayerTeam.Team2)
            {
                Game.WriteToConsole("Red player dead");
                _BlueKillsCount++;
                RefreshKillCounter(_BlueKillsCount, Teams.Blue);
            }
            var user = deadPlayer.GetUser();
            if (user != null)
            {
                _DeadPlayers.Add(
                new DeadPlayer()
                {
                    DeathTimeStamp = Game.TotalElapsedGameTime,
                    User = user,
                    DeadBody = deadPlayer,
                    Team = deadPlayer.GetTeam(),
                    IsBot = deadPlayer.IsBot
                });
                Game.WriteToConsole("Death() added senderPlayer to _DeadPlayers: ", deadPlayer.Name);
            }
        }

        /// <summary>
        /// Checks if there's any dead players to respawn
        /// </summary>
        /// <param name="args"></param>
        public void RespawnTick()
        {
            if (_DeadPlayers.Count <= 0) return;


            for (int i = _DeadPlayers.Count - 1; i >= 0; i--)
            {
                var deadPlayer = _DeadPlayers[i];

                if (deadPlayer == null) continue;
                if (deadPlayer.DeathTimeStamp + USER_RESPAWN_DELAY_MS < Game.TotalElapsedGameTime)
                {
                    _DeadPlayers.RemoveAt(i);

                    if (deadPlayer.DeadBody != null)
                    {
                        Game.WriteToConsole("RespawnTick() before deadPlayer.DeadBody.Remove()", deadPlayer.DeadBody.Name);
                        deadPlayer.DeadBody.Remove();
                        Game.WriteToConsole("RespawnTick() after deadPlayer.DeadBody.Remove()", deadPlayer.DeadBody.Name);
                    }
                    var player = deadPlayer.DeadBody;
                    if (player == null || player.IsDead)
                    {
                        Game.WriteToConsole("RespawnTick() before Respawn()", deadPlayer.DeadBody.Name);
                        Respawn(deadPlayer.User, deadPlayer.Team, deadPlayer.IsBot);
                        Game.WriteToConsole("RespawnTick() after Respawn()", deadPlayer.DeadBody.Name);
                    }
                }
            }
        }



        #endregion

        #region Utility Triggers


        /// <summary>
        /// Trigger called to update the time remaining
        /// </summary>
        /// <param name="args"></param>
        public void TimeRemaining(TriggerArgs args)
        {
            if (_TimeRemainingText != null && _HasGameTimeFinished == false)
            {
                _RemainingGameTimeInSeconds--;
                _TimeRemainingText.SetText("Time Remaining: " + GetRemainingMinutesAndSecondsString(_RemainingGameTimeInSeconds));
            }
            if (_RemainingGameTimeInSeconds < 1 && !_HasGameTimeFinished)
            {
                _HasGameTimeFinished = true;
                OnTimeFinished(args);
            }
        }

        /// <summary>
        /// Tick called to see if we need to spawn a fireball
        /// </summary>
        /// <param name="args"></param>
        public void ShootFireball(TriggerArgs args)
        {
            foreach (IPlayer ply in _Marios)
            {
                if (ply != null)
                {
                    if (ply.IsBlocking && ply.IsWalking)
                    {
                        Vector2 pos = ply.GetWorldPosition();
                        int dir = ply.FacingDirection;
                        for (int i = 1; i >= 1; i--)
                        {
                            Game.SpawnProjectile(ProjectileItem.FLAREGUN, pos + new Vector2(6f * dir, 9f), new Vector2(150f * dir, i));
                        }
                    }
                }
            }
        }



        /// <summary>
        /// Tick called when a player has entered the area with the two spawn buttons
        /// </summary>
        /// <param name="args"></param>
        public void PlayerEnteredTheGame(TriggerArgs args)
        {
            var player = (IPlayer)args.Sender;
            player.SetTeam(PlayerTeam.Team3);

            // If it's not a bot, we don't need to do anything else
            if (!player.IsBot)
            {
                return;
            }

            var caller = Game.GetSingleObjectByCustomID("SelectedBlueButton");
            var args2 = new TriggerArgs(caller, player, false);
            MoveToBase(args2);

            // If now, we're not part of green team, the bot has spawned successfully
            if (player.GetTeam() != PlayerTeam.Team3)
            {
                return;
            }

            caller = Game.GetSingleObjectByCustomID("SelectedRedButton");
            var args3 = new TriggerArgs(caller, player, false);
            MoveToBase(args3);

        }


        /// <summary>
        /// Triggers the ballons group, making them go up
        /// </summary>
        /// <remarks>
        /// Tick called by an area trigger when players are moved to the peach area
        /// </remarks>
        /// <param name="args">The tick arguments</param>
        public void BalloonsParty(TriggerArgs args)
        {
            IObjectGroupMarker groupOne = (IObjectGroupMarker)Game.GetSingleObjectByCustomID("BalloonsGroupOne");
            groupOne.Trigger();
        }

        /// <summary>
        /// Tick called to clear the game popup message
        /// </summary>
        /// <param name="args"></param>
        public void ClearPopup(TriggerArgs args)
        {
            Game.HidePopupMessage();
        }
        #endregion
        #endregion

        #region Buttons

        /// <summary>
        /// Called when a player presses one of the buttons to spawn as mario
        /// </summary>
        /// <param name="args"></param>
        public void MoveToBase(TriggerArgs args)
        {
            var senderPlayer = (IPlayer)args.Sender;
            var caller = (IObject)args.Caller;

            if ((senderPlayer != null) && (senderPlayer is IPlayer) && (!senderPlayer.IsDiving))
            {
                switch (caller.CustomId)
                {
                    case "SelectedRedButton":
                        if (_NumberOfBlueTeamPlayers >= _NumberOfRedTeamPlayers)
                        {
                            SendToPosition(GetRedRandomPosition(), senderPlayer);
                            senderPlayer.SetTeam(PlayerTeam.Team2);
                            _NumberOfRedTeamPlayers++;
                            SetPlayerToMarioOne(senderPlayer);
                        }
                        else
                        {
                            Game.ShowPopupMessage("Choose another team");
                        }
                        break;
                    case "SelectedBlueButton":
                        if (_NumberOfRedTeamPlayers >= _NumberOfBlueTeamPlayers)
                        {
                            SendToPosition(GetBlueRandomPosition(), senderPlayer);
                            senderPlayer.SetTeam(PlayerTeam.Team1);
                            _NumberOfBlueTeamPlayers++;
                            SetPlayerToMarioTwo(senderPlayer);
                        }
                        else
                        {
                            Game.ShowPopupMessage("Choose another team");
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Called when a player presses a refill button
        /// </summary>
        /// <param name="args"></param>
        public void Refill(TriggerArgs args)
        {
            var senderPlayer = (IPlayer)args.Sender;
            senderPlayer.GiveWeaponItem(WeaponItem.PILLS);
            senderPlayer.GiveWeaponItem(senderPlayer.CurrentSecondaryWeapon.WeaponItem);
            if (senderPlayer.CurrentPrimaryWeapon.WeaponItem != WeaponItem.MAGNUM)
            {
                senderPlayer.GiveWeaponItem(senderPlayer.CurrentPrimaryWeapon.WeaponItem);
            }
        }

        #endregion

        #region Specific Utility Methods

        private void UpdateUsersTextList(IUser[] allUsers)
        {
            var usersListText = (IObjectText)Game.GetSingleObjectByCustomId(USERS_COUNT_TEXT_ID);
            usersListText.SetText(_UsersConnectedCount.ToString());
            string playerList = "";
            for (int i = 0; i < allUsers.Length; i++)
            {
                playerList += allUsers[i].GetProfile().Name + "\n";
            }

            if (allUsers.Length < 8)
            {
                for (int i = allUsers.Length - 1; i < 8; i++)
                {
                    playerList += " \n";
                }
            }
            usersListText = (IObjectText)Game.GetSingleObjectByCustomId(USERS_LIST_TEXT_ID);
            usersListText.SetText(playerList);
        }


        /// <summary>
        /// Called when the game time has elapsed
        /// </summary>
        /// <param name="args"></param>
        public void OnTimeFinished(TriggerArgs args)
        {
            if (_RedKillsCount > _BlueKillsCount)
            {
                Game.SetGameOver("Red wins");
                winner = PlayerTeam.Team2;
                Game.WriteToConsole(winner.ToString());
                foreach (IPlayer p in Game.GetPlayers())
                {
                    if (p.GetTeam() == PlayerTeam.Team2)
                    {
                        p.SetWorldPosition(Game.GetSingleObjectByCustomID("WinBlock").GetWorldPosition());
                    }
                }
            }
            else if (_RedKillsCount < _BlueKillsCount)
            {
                Game.SetGameOver("Blue wins");
                winner = PlayerTeam.Team1;

                foreach (IPlayer p in Game.GetPlayers())
                {
                    if (p.GetTeam() == PlayerTeam.Team1)
                    {
                        p.SetWorldPosition(Game.GetSingleObjectByCustomID("WinBlock").GetWorldPosition());
                    }
                }
            }
            else
            {
                Game.SetGameOver("Draw!");
                Explode();
            }

            var camera = Game.GetSingleObjectByCustomID("PeachCastleCamera") as IObjectCameraAreaTrigger;
            var music = Game.GetSingleObjectByCustomID("WinnerMusic") as IObjectMusicTrigger;
            camera.Trigger();
            music.Trigger();
        }


        /// <summary>
        /// Gets a random position for spawning a blue player
        /// </summary>
        /// <returns></returns>
        public string GetBlueRandomPosition()
        {
            var rand = RandNumber(0, 4);
            return (BlueSpawnPositions[rand]);
        }

        /// <summary>
        /// Gets a random position for spawning a red player
        /// </summary>
        /// <returns></returns>
        public string GetRedRandomPosition()
        {
            var rand = RandNumber(0, 4);
            return (RedSpawnPositions[rand]);
        }

        /// <summary>
        /// Refreshes the kill counter for the specified team
        /// </summary>
        /// <param name="killCount">The number of kills of the team</param>
        /// <param name="team">The team</param>
        private void RefreshKillCounter(int killCount, Teams team)
        {
            var blueKillCounterText = (IObjectText)Game.GetSingleObjectByCustomId(BLUE_KILLS_COUNT_TEXT_ID);
            var redKillCounterTet = (IObjectText)Game.GetSingleObjectByCustomId(RED_KILLS_COUNT_TEXT_ID);

            if (_HasGameTimeFinished) return;

            if (blueKillCounterText != null && team == Teams.Blue)
            {
                blueKillCounterText.SetText("Blue Score: " + killCount.ToString());
            }

            if (redKillCounterTet != null && team == Teams.Red)
            {
                redKillCounterTet.SetText("Red Score: " + killCount.ToString());
            }
        }

        /// <summary>
        /// Sets the player to a white mario
        /// </summary>
        /// <param name="player">The player</param>
        private void SetPlayerToMarioOne(IPlayer player)
        {
            IProfile marioProfile = new IProfile()
            {
                Gender = Gender.Male,
                Head = new IProfileClothingItem("Cap", "ClothingWhite"),
                ChestOver = new IProfileClothingItem("Suspenders", "ClothingRed", "ClothingYellow"),
                ChestUnder = new IProfileClothingItem("TrainingShirt", "ClothingWhite"),
                Hands = new IProfileClothingItem("Gloves", "ClothingRed"),
                Accesory = new IProfileClothingItem("Belt", "ClothingRed"),
                Legs = new IProfileClothingItem("Pants", "ClothingRed"),
                Feet = new IProfileClothingItem("Boots", "ClothingBrown")
            };
            player.SetProfile(marioProfile);
            player.GiveWeaponItem(WeaponItem.KNIFE);
            player.GiveWeaponItem(WeaponItem.PISTOL45);
            player.GiveWeaponItem(WeaponItem.STRENGTHBOOST);
            _Marios.Add(player);
        }

        /// <summary>
        /// Sets the player to a red mario
        /// </summary>
        /// <param name="player">The player</param>
        private void SetPlayerToMarioTwo(IPlayer player)
        {
            IProfile marioProfile = new IProfile()
            {
                Gender = Gender.Male,
                Head = new IProfileClothingItem("Cap", "ClothingRed"),
                ChestOver = new IProfileClothingItem("Suspenders", "ClothingYellow", "ClothingYellow"),
                ChestUnder = new IProfileClothingItem("TrainingShirt", "ClothingRed"),
                Hands = new IProfileClothingItem("Gloves", "ClothingWhite"),
                Accesory = new IProfileClothingItem("Belt", "ClothingLightGray", "ClothingLightGray"),
                Legs = new IProfileClothingItem("Pants", "ClothingWhite"),
                Feet = new IProfileClothingItem("Boots", "ClothingBrown")
            };
            player.GiveWeaponItem(WeaponItem.KNIFE);
            player.GiveWeaponItem(WeaponItem.PISTOL45);
            player.GiveWeaponItem(WeaponItem.STRENGTHBOOST);
            player.SetProfile(marioProfile);
            _Marios.Add(player);
        }
        private string GetRemainingMinutesAndSecondsString(int toConvertSeconds)
        {
            int minutes = toConvertSeconds / 60;
            int seconds = toConvertSeconds % 60;
            return minutes.ToString("00") + ":" + seconds.ToString("00");
        }

        private void SpawnPeach()
        {
            Vector2 pos = Game.GetSingleObjectByCustomID(PEACH_POSITION_BLOCK_ID).GetWorldPosition();

            IProfile peach = new IProfile()
            {
                Gender = Gender.Female,
                Head = new IProfileClothingItem("TopHat", "ClothingWhite", "ClothingRed"),
                ChestOver = new IProfileClothingItem("Robe", "ClothingWhite", "ClothingRed"),
                Hands = new IProfileClothingItem("FingerlessGloves", "ClothingRed"),
                Accesory = new IProfileClothingItem("Belt_fem", "ClothingRed"),
                Legs = new IProfileClothingItem("Skirt", "ClothingWhite"),
                Feet = new IProfileClothingItem("HighHeels", "ClothingRed")
            };
            peach.Skin = new IProfileClothingItem("Normal_fem", "Skin2");
            peach.Name = "Peach";

            IPlayer peachPlayer = Game.CreatePlayer(pos);
            peachPlayer.CustomID = "PlayerPeach";
            peachPlayer.SetProfile(peach);
            peachPlayer.SetNametagVisible(false);
        }
        public void Explode()
        {
            IObjectExplosionTrigger explode = (IObjectExplosionTrigger)Game.GetSingleObjectByCustomID("Explode");
            explode.Trigger();
            explode.Trigger();
            explode.Trigger();
            explode.Trigger();
        }
        #endregion

        #region Generic Utility Methods

        /// <summary>
        /// Creates a new trigger
        /// </summary>
        /// <param name="interval">The interval at which the trigger will run</param>
        /// <param name="count">How many times the trigger will round (0 for infinite times)</param>
        /// <param name="method">The name of the method that is going to be called when the trigger executes</param>
        /// <param name="id">The id for the trigger</param>
        private void CreateTrigger(int interval, int count, string method, string id = "")
        {
            IObjectTimerTrigger timerTrigger = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
            timerTrigger.SetIntervalTime(interval);
            timerTrigger.SetRepeatCount(count);
            timerTrigger.SetScriptMethod(method);
            timerTrigger.CustomId = id;
            timerTrigger.Trigger();
            timerTrigger.SetWorldPosition(Game.GetSingleObjectByCustomId("TriggersPosition").GetWorldPosition());
        }

        /// <summary>
        /// Checks whether the user is still active or not
        /// </summary>
        /// <param name="user">The user</param>
        /// <returns> Whether the user is still active or not</returns>
        private bool IsUserStillActive(IUser user)
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

        /// <summary>
        /// Sends the player to a specified position
        /// </summary>
        /// <param name="id">The id of the desired position</param>
        /// <param name="ply">The player</param>
        public void SendToPosition(string id, IPlayer ply)
        {
            var mCallerObject = Game.GetSingleObjectByCustomId(id);
            var position = mCallerObject.GetWorldPosition();
            ply.SetWorldPosition(position);
        }

        /// <summary>
        /// Substracts a player from the team 
        /// </summary>
        /// <param name="team">The team</param>
        public void SubstractPlayerFromThisTeam(PlayerTeam team)
        {
            if (team == PlayerTeam.Team1)
            {
                _NumberOfBlueTeamPlayers--;
            }
            if (team == PlayerTeam.Team2)
            {
                _NumberOfRedTeamPlayers--;
            }
        }

        /// <summary>
        /// Returns a random int from low bound to high bound
        /// </summary>
        /// <param name="low">the lower value (included)</param>
        /// <param name="high">the higher bound value (included)</param>
        /// <returns></returns>
        public int RandNumber(int low, int high)
        {
            return _Rnd.Next(low, high);
        }

        /// <summary>
        /// Removes all weapons to this player
        /// </summary>
        /// <param name="ply"></param>
        public void RemoveWeapons(IPlayer ply)
        {
            ply.RemoveWeaponItemType(WeaponItemType.Rifle);
            ply.RemoveWeaponItemType(WeaponItemType.Handgun);
            ply.RemoveWeaponItemType(WeaponItemType.Melee);
            ply.RemoveWeaponItemType(WeaponItemType.Thrown);
            ply.SetHealth(100);
        }

        /// <summary>
        /// Spawns a player that has already spawned before
        /// </summary>
        /// <param name="user">The user</param>
        /// <param name="team">The team for this user</param>
        /// <param name="isBot">Whether the player to respawn is a bot or not</param>
        private void Respawn(IUser user, PlayerTeam team, bool isBot)
        {
            if (IsUserStillActive(user) && !user.IsSpectator)
            {
                var player = Game.CreatePlayer(_PlayerSpawnPosition);
                if (isBot)
                {
                    var botBehavior = new BotBehavior(true, PredefinedAIType.BotB);
                    player.SetBotBehavior(botBehavior);
                }

                player.SetProfile(user.GetProfile());
                if (team == PlayerTeam.Team1)
                {
                    player.SetTeam(PlayerTeam.Team1);
                    _PlayerSpawnPosition = Game.GetSingleObjectByCustomId(GetBlueRandomPosition()).GetWorldPosition();

                    if (winner == PlayerTeam.Team1)
                    {
                        _PlayerSpawnPosition = Game.GetSingleObjectByCustomID("PlayerPeach").GetWorldPosition();
                    }
                    SetPlayerToMarioTwo(player);
                }
                if (team == PlayerTeam.Team2)
                {
                    player.SetTeam(PlayerTeam.Team2);
                    _PlayerSpawnPosition = Game.GetSingleObjectByCustomId(GetRedRandomPosition()).GetWorldPosition();
                    if (winner == PlayerTeam.Team2)
                    {
                        _PlayerSpawnPosition = Game.GetSingleObjectByCustomID("PlayerPeach").GetWorldPosition();
                    }
                    SetPlayerToMarioOne(player);
                }
                if (team.Equals(PlayerTeam.Team3))
                {
                    player.SetTeam(PlayerTeam.Team3);
                    _PlayerSpawnPosition = Game.GetSingleObjectByCustomID(PLAYER_MIDDLE_SPAWN_BLOCK_ID).GetWorldPosition();
                }
                player.SetUser(user);
                player.SetWorldPosition(_PlayerSpawnPosition);
            }
            else
            {
                SubstractPlayerFromThisTeam(team);
            }
        }

        /// <summary>
        /// Spawns the player in the middle of the map for it to choose a team
        /// </summary>
        /// <param name="user"></param>
        private void FirstSpawn(IUser user)
        {
            if (!IsUserStillActive(user) || user.IsSpectator) return;
            _PlayerSpawnPosition = Game.GetSingleObjectByCustomId(PLAYER_MIDDLE_SPAWN_BLOCK_ID).GetWorldPosition();
            var player = Game.CreatePlayer(_PlayerSpawnPosition);
            player.SetUser(user);
            player.SetProfile(user.GetProfile());
            player.SetWorldPosition(_PlayerSpawnPosition);
            player.SetTeam(PlayerTeam.Team3);
        }


        #endregion

        #endregion
    }
}
