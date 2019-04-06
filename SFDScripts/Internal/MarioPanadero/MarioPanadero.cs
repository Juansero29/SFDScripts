using System;
using System.Collections.Generic;
using SFDGameScriptInterface;


namespace SFDScripts
{
    class MarioPanadero : GameScriptInterface
    {

        public MarioPanadero() : base(null) { }


        #region Script To Copy
        #region Constants

        #region ObjectIds
        private const string TIME_REMAINING_LABEL_ID = "TimeRemainingLabel";

        private const string CRATE_OBJECT_ID = "Crate";
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

        #region Settings

        /// <summary>
        /// The user respawn rate delay in miliseconds
        /// </summary>
        private const int USER_RESPAWN_DELAY_MS = 10000;

        /// <summary>
        /// Whether we want to gib the corpses or not
        /// </summary>
        private const bool GIB_CORPSES = false;

        /// <summary>
        /// Total duration of the game declared in seconds
        /// </summary>
        private const int TOTAL_GAME_TIME_IN_SECONDS = 5 * 60;

        /// <summary>
        /// Delay to connect a new player to the game
        /// </summary>
        private const int DELAY_TO_CONNECT_A_NEW_PLAYER = 15;
        #endregion

        #region Utilities
        /// <summary>
        /// The random generator for this map
        /// </summary>
        private readonly Random rnd = new Random();
        #endregion

        #endregion

        #region Private Fields

        /// <summary>
        /// The delay before connecting a new user to the game
        /// </summary>
        private int mConnectPlayerDelay = DELAY_TO_CONNECT_A_NEW_PLAYER;

        /// <summary>
        /// The total time remaining for this game in seconds
        /// </summary>
        private int mRemainingGameTimeInSeconds = TOTAL_GAME_TIME_IN_SECONDS;

        /// <summary>
        /// The crate object. When destroyed it will drop a weapon.
        /// </summary>
        private IObject mCrate;

        /// <summary>
        /// The point in the map where the grenade launcher is going to spawn
        /// </summary>
        private Vector2 mCratePosition;

        /// <summary>
        /// Boolean indicating whether the crate has been destroyed or not
        /// </summary>
        private bool mHasCrateBeenDestroyed = false;

        /// <summary>
        /// The text object showing the number of kills the blue team has made
        /// </summary>
        private IObjectText mTimeRemainingText;

        /// <summary>
        /// The list of all the players marked as marios in this game
        /// </summary>
        private List<IPlayer> mMarios = new List<IPlayer>();

        /// <summary>
        /// The number of kills the red team has made
        /// </summary>
        private int mRedKillsCount = 0;

        /// <summary>
        /// The number of kills the blue team has made
        /// </summary>
        private int mBlueKillsCount = 0;

        /// <summary>
        /// The position at which players should spawn
        /// </summary>
        private Vector2 mPlayerSpawnPosition;

        /// <summary>
        /// Boolean indicating whether the time has already elapsed or not
        /// </summary>
        private bool mHasGameTimeFinished = false;

        /// <summary>
        /// Indicates the winner team
        /// </summary>
        private PlayerTeam winner = PlayerTeam.Team3;

        /// <summary>
        /// The number of blue team players
        /// </summary>
        private int mNumberOfBlueTeamPlayers = 0;

        /// <summary>
        /// The number of red team players
        /// </summary>
        private int mNumberOfRedTeamPlayers = 0;

        /// <summary>
        /// The list of dead players in the game
        /// </summary>
        private List<DeadPlayer> mDeadPlayers = new List<DeadPlayer>();

        /// <summary>
        /// The number of dead players in the game
        /// </summary>
        private int mDeadPlayersCount = 0;

        /// <summary>
        /// The number of users connected to the game
        /// </summary>
        private int mUsersConnectedCount = 0;
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
            /// The player
            /// </summary>
            public IPlayer Player { get; set; }

            /// <summary>
            /// The team of this dead player
            /// </summary>
            public PlayerTeam Team { get; set; }

        }
        #endregion

        #region Triggers
        public void Start(TriggerArgs args)
        {
            CreateTrigger(300, 0, "ShootFireball", "");

            ConnectedPlayersTick(args);

            RefreshKillCounter(mBlueKillsCount, Teams.Blue);
            RefreshKillCounter(mRedKillsCount, Teams.Red);

            mCrate = Game.GetSingleObjectByCustomId(CRATE_OBJECT_ID);
            mTimeRemainingText = Game.GetSingleObjectByCustomId(TIME_REMAINING_LABEL_ID) as IObjectText;

            SpawnPeach();
        }
        public void Tick(TriggerArgs args)
        {
            RespawnTick(args);
            ConnectedPlayersTick(args);
        }
        public void TimeRemaining(TriggerArgs args)
        {
            if (mTimeRemainingText != null && mHasGameTimeFinished == false)
            {
                mRemainingGameTimeInSeconds--;
                mTimeRemainingText.SetText("Time Remaining: " + PrintMinutes(mRemainingGameTimeInSeconds));
            }
            if (mRemainingGameTimeInSeconds < 1 && !mHasGameTimeFinished)
            {
                mHasGameTimeFinished = true;
                WhoWins(args);
            }
        }
        public void PlayerEnteredTheGame(TriggerArgs args)
        {
            var senderPlayer = (IPlayer)args.Sender;
            senderPlayer.SetTeam(PlayerTeam.Team3);
        }
        public void ClearPopup(TriggerArgs args)
        {
            Game.HidePopupMessage();
        }
        public void WhoWins(TriggerArgs args)
        {
            if (mRedKillsCount > mBlueKillsCount)
            {
                Game.SetGameOver("The Red wins");
                winner = PlayerTeam.Team2;
                Game.WriteToConsole(winner.ToString());
                foreach (IPlayer p in Game.GetPlayers())
                {
                    if (p.GetTeam() == PlayerTeam.Team2)
                    {
                        p.SetWorldPosition(Game.GetSingleObjectByCustomID("PlayerPeach").GetWorldPosition());
                    }
                }
            }
            else if (mRedKillsCount < mBlueKillsCount)
            {
                Game.SetGameOver("The Blue wins");
                winner = PlayerTeam.Team1;

                foreach (IPlayer p in Game.GetPlayers())
                {
                    if (p.GetTeam() == PlayerTeam.Team1)
                    {
                        p.SetWorldPosition(Game.GetSingleObjectByCustomID("PlayerPeach").GetWorldPosition());
                    }
                }
            }
            else
            {
                Game.SetGameOver("Draw!");
                ExplodePeach();
            }
        }
        public void CheckIfCrateIsOpen(TriggerArgs args)
        {
            if (mCrate.GetHealth() == 0 && mHasCrateBeenDestroyed == false)
            {
                mHasCrateBeenDestroyed = true;
                Game.SpawnWeaponItem(WeaponItem.GRENADE_LAUNCHER, mCratePosition, true, 10000);
                Game.GetSingleObjectByCustomId("CrateTrigger").Destroy();
            }
            else
            {
                mCratePosition = mCrate.GetWorldPosition();
            }
        }
        public void MoveToBase(TriggerArgs args)
        {
            var senderPlayer = (IPlayer)args.Sender;
            var caller = (IObject)args.Caller;

            if ((senderPlayer != null) && (senderPlayer is IPlayer) && (!senderPlayer.IsDiving))
            {
                switch (caller.CustomId)
                {
                    case "SelectedRedButton":
                        if (mNumberOfBlueTeamPlayers >= mNumberOfRedTeamPlayers)
                        {
                            SendToPosition(GetRedRandomPosition(), senderPlayer);
                            senderPlayer.SetTeam(PlayerTeam.Team2);
                            mNumberOfRedTeamPlayers++;
                            SetPlayerToMarioOne(senderPlayer);
                        }
                        else
                        {
                            Game.ShowPopupMessage("Choose another team");
                        }
                        break;
                    case "SelectedBlueButton":
                        if (mNumberOfRedTeamPlayers >= mNumberOfBlueTeamPlayers)
                        {
                            SendToPosition(GetBlueRandomPosition(), senderPlayer);
                            senderPlayer.SetTeam(PlayerTeam.Team1);
                            mNumberOfBlueTeamPlayers++;
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
        public void Death(TriggerArgs args)
        {
            var senderPlayer = (IPlayer)args.Sender;
            if ((args.Sender != null) && (args.Sender is IPlayer))
            {
                mDeadPlayersCount++;

                if (senderPlayer.GetTeam() == PlayerTeam.Team1)
                {
                    mRedKillsCount++;
                    RefreshKillCounter(mRedKillsCount, Teams.Red);
                }
                if (senderPlayer.GetTeam() == PlayerTeam.Team2)
                {
                    mBlueKillsCount++;
                    RefreshKillCounter(mBlueKillsCount, Teams.Blue);
                }
                var user = senderPlayer.GetUser();
                if (user != null)
                {
                    mDeadPlayers.Add(
                        new DeadPlayer()
                        {
                            DeathTimeStamp = Game.TotalElapsedGameTime,
                            User = user,
                            Player = senderPlayer,
                            Team = senderPlayer.GetTeam()
                        });
                }
            }
        }
        public void RespawnTick(TriggerArgs args)
        {
            if (mDeadPlayers.Count > 0)
            {
                for (int i = mDeadPlayers.Count - 1; i >= 0; i--)
                {
                    var deadPlayer = mDeadPlayers[i];
                    if (deadPlayer.DeathTimeStamp + USER_RESPAWN_DELAY_MS < Game.TotalElapsedGameTime)
                    {

                        mDeadPlayers.RemoveAt(i);
                        if (deadPlayer.Player != null)
                        {
                            deadPlayer.Player.SetWorldPosition(Game.GetSingleObjectByCustomID("ThePit").GetWorldPosition());
                        }
                        var player = deadPlayer.User.GetPlayer();
                        if (((player == null) || (player.IsDead)))
                        {
                            Respawn(deadPlayer.User, deadPlayer.Team);
                        }
                    }
                }
            }
        }
        public void ConnectedPlayersTick(TriggerArgs args)
        {
            if (mConnectPlayerDelay > 0)
            {
                mConnectPlayerDelay--;
                if (mConnectPlayerDelay <= 0)
                {
                    var allUsers = Game.GetActiveUsers();
                    if (mUsersConnectedCount == 0)
                    {
                        mUsersConnectedCount = allUsers.Length;
                    }
                    else if (allUsers.Length > mUsersConnectedCount)
                    {
                        for (int i = 0; i < allUsers.Length; i++)
                        {
                            var player = allUsers[i].GetPlayer();
                            if ((player == null)) FirstSpawn(allUsers[i]);
                        }
                    }
                    mUsersConnectedCount = allUsers.Length;
                    var usersListText = (IObjectText)Game.GetSingleObjectByCustomId(USERS_COUNT_TEXT_ID);
                    usersListText.SetText(mUsersConnectedCount.ToString());
                    string playerList = "";
                    for (int i = 0; i < allUsers.Length; i++)
                        playerList += allUsers[i].GetProfile().Name + "\n";
                    if (allUsers.Length < 8)
                        for (int i = allUsers.Length - 1; i < 8; i++)
                            playerList += " \n";
                    usersListText = (IObjectText)Game.GetSingleObjectByCustomId(USERS_LIST_TEXT_ID);
                    usersListText.SetText(playerList);
                    mConnectPlayerDelay = 15;
                }
            }
        }
        public void ShootFireball(TriggerArgs args)
        {
            foreach (IPlayer ply in mMarios)
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

        public void BalloonsParty(TriggerArgs args)
        {
            IObjectGroupMarker groupOne = (IObjectGroupMarker)Game.GetSingleObjectByCustomID("BalloonsGroupOne");
            IObjectGroupMarker groupTwo = (IObjectGroupMarker)Game.GetSingleObjectByCustomID("BalloonsGroupTwo");
            IObjectGroupMarker groupThree = (IObjectGroupMarker)Game.GetSingleObjectByCustomID("BalloonsGroupThree");

            groupOne.Trigger();
            groupTwo.Trigger();
            groupThree.Trigger();
        }
        #endregion

        #region Utility Methods

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
                mNumberOfBlueTeamPlayers--;
            }
            if (team == PlayerTeam.Team2)
            {
                mNumberOfRedTeamPlayers--;
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
            return rnd.Next(low, high);
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
        private void Respawn(IUser user, PlayerTeam team)
        {
            if (CheckUserStillActive(user) && !user.IsSpectator)
            {
                var player = Game.CreatePlayer(mPlayerSpawnPosition);
                player.SetProfile(user.GetProfile());
                if (team == PlayerTeam.Team1)
                {
                    player.SetTeam(PlayerTeam.Team1);
                    mPlayerSpawnPosition = Game.GetSingleObjectByCustomId(GetBlueRandomPosition()).GetWorldPosition();

                    if (winner == PlayerTeam.Team1)
                    {
                        mPlayerSpawnPosition = Game.GetSingleObjectByCustomID("PlayerPeach").GetWorldPosition();
                    }
                    SetPlayerToMarioTwo(player);
                }
                if (team == PlayerTeam.Team2)
                {
                    player.SetTeam(PlayerTeam.Team2);
                    mPlayerSpawnPosition = Game.GetSingleObjectByCustomId(GetRedRandomPosition()).GetWorldPosition();
                    if (winner == PlayerTeam.Team2)
                    {
                        mPlayerSpawnPosition = Game.GetSingleObjectByCustomID("PlayerPeach").GetWorldPosition();
                    }
                    SetPlayerToMarioOne(player);
                }
                if (team.Equals(PlayerTeam.Team3))
                {
                    player.SetTeam(PlayerTeam.Team3);
                    mPlayerSpawnPosition = Game.GetSingleObjectByCustomID(PLAYER_MIDDLE_SPAWN_BLOCK_ID).GetWorldPosition();
                }
                player.SetUser(user);
                player.SetWorldPosition(mPlayerSpawnPosition);
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
            if (CheckUserStillActive(user) && !user.IsSpectator)
            {
                mPlayerSpawnPosition = Game.GetSingleObjectByCustomId(PLAYER_MIDDLE_SPAWN_BLOCK_ID).GetWorldPosition();
                var player = Game.CreatePlayer(mPlayerSpawnPosition);
                player.SetUser(user);
                player.SetProfile(user.GetProfile());
                player.SetWorldPosition(mPlayerSpawnPosition);
                player.SetTeam(PlayerTeam.Team3);
            }
        }

        /// <summary>
        /// Checks whether the user is still active or not
        /// </summary>
        /// <param name="user">The user</param>
        /// <returns> Whether the user is still active or not</returns>
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

        /// <summary>
        /// Refreshes the kill counter for the specified team
        /// </summary>
        /// <param name="killCount">The number of kills of the team</param>
        /// <param name="team">The team</param>
        private void RefreshKillCounter(int killCount, Teams team)
        {
            var blueKillCounterText = (IObjectText)Game.GetSingleObjectByCustomId(BLUE_KILLS_COUNT_TEXT_ID);
            var redKillCounterTet = (IObjectText)Game.GetSingleObjectByCustomId(RED_KILLS_COUNT_TEXT_ID);

            if (mHasGameTimeFinished != true)
            {
                if (blueKillCounterText != null && team == Teams.Blue)
                    blueKillCounterText.SetText("Blue Score: " + killCount.ToString());
                if (redKillCounterTet != null && team == Teams.Red)
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
            mMarios.Add(player);
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
            player.SetProfile(marioProfile);
            mMarios.Add(player);
        }

        /// <summary>
        /// Creates a new trigger
        /// </summary>
        /// <param name="interval">The interval at which the trigger will run</param>
        /// <param name="count">How many times the trigger will round (0 for infinite times)</param>
        /// <param name="method">The name of the method that is going to be called when the trigger executes</param>
        /// <param name="id">The id for the trigger</param>
        private void CreateTrigger(int interval, int count, string method, string id)
        {
            IObjectTimerTrigger timerTrigger = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
            timerTrigger.SetIntervalTime(interval);
            timerTrigger.SetRepeatCount(count);
            timerTrigger.SetScriptMethod(method);
            timerTrigger.CustomId = id;
            timerTrigger.Trigger();
            timerTrigger.SetWorldPosition(Game.GetSingleObjectByCustomId("TriggersPosition").GetWorldPosition());
        }
        private string PrintMinutes(int toConvertSeconds)
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
        public void ExplodePeach()
        {
            IObjectExplosionTrigger explode = (IObjectExplosionTrigger)Game.GetSingleObjectByCustomID("Explode");
            explode.Trigger();
            explode.Trigger();
            explode.Trigger();
            explode.Trigger();
        }
        #endregion
        #endregion
    }
}
