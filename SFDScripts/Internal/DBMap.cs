using System;
using System.Collections.Generic;
using SFDGameScriptInterface;


namespace SFDScripts
{
    class DBMap : GameScriptInterface
    {

        public DBMap() : base(null) { }

        const string version = "ROTTEN CITY: v1.3";

        const int blockedAttack_DMG = 5;
        int available_time = 10;
        int time_game = 120;
        const float fire_DMG = 1.2f;
        const float projectile_DMG = 2.1f;
        const float explosion_DMG = 1.0f;
        const float fall_DMG = 2.3f;

        float fire_const_DMG = 2.5f,
        projectile_const_DMG = 1,
        explosion_const_DMG = 1,
        fall_const_DMG = 1;

        // ranks	
        string[] level = {
            "Lvl 0 - Newbie",
            "Lvl 1 - Rookie",
            "Lvl 2 - Soldier"
        };

        // exp for every rank
        // 1 match = +15
        int[] exp = {
            50,
            80,
            100
        };

        bool selectingState = true;

        #region equipment
        public int selection_MAX = 3;

        public int primary_MAX = 3;
        public string[] primary_weapons = {
            "None",
            "SMG",
            "Carbine",
        };

        public int secondary_MAX = 3;
        public string[] secondary_weapons = {
            "None",
            "Pistol",
            "Revolver"
        };

        public int special_MAX = 3;
        public string[] special_equipment = {
            "None",
            "Small medkit",
            "Airdrop"
        };

        public enum PlayerDirection
        {
            NONE = -1,
            UP, DOWN,
            LEFT, RIGHT
        };
        #endregion

        /*
         *	TO-DO:
         *		- Done: Improvements to the Game.Data system
         *		- player killed another player system, best name
         *		- Done: random exp: from 10 to 15
         *		- 3 teams (on v2.0)
         *
         *	FLAGS:
         * 		To Delete 
         *		To Adjust
         *		To Improve
         *
         *		int total_of_hours_wasted = 28;
         */

        #region initialization
        Random random = new Random();
        public enum State
        {
            None = (1 << 0),
            Bleeding = (1 << 1), // Random event after been hit by a projectile
            Blind = (1 << 2), // After an explosion
            Healing = (1 << 3)
        }

        public class Player
        {

            public string[] selection = {
    ">",
    "",
    ""
   };

            // ----------------------------------------------------------

            public IPlayer player;
            public IUser user;

            // States
            public float fire_ATK = 0,
             projectile_ATK = 0,
             explosion_ATK = 0,
             fall_ATK = 0,
             damage_ATK = 0;


            public int bleeding_hits = 2,
             blind_time = 5,
             blind_chance = 8; // 8/10 = 80%

            // To adjust
            // public bool			bleeding_state = false;

            // Labels
            public IObjectText LBL_owneq = null, // equipment owned
             LBL_equipment = null;

            // Equipment -- temporary (change to Dictionary<int, string>)
            public string current_item = "Small medkit";
            public int small_medkits = 1;

            // EXP & LVL
            public int level;
            public int EXP;

            // Temporary selection
            public int _selection = 0,
             selection_primary = 0,
             selection_secondary = 0,
             selection_special = 0;

            public Player(IPlayer _player, int _level, int _exp)
            {
                this.player = _player;
                this.user = _player.GetUser();
                this.level = _level;
                this.EXP = _exp;
            }

        }

        public List<Player> playerList = new List<Player>();
        Dictionary<PlayerTeam, int> Teams = new Dictionary<PlayerTeam, int>(4);
        PlayerTeam[] _teams = {
   PlayerTeam.Team1,
   PlayerTeam.Team2,
   PlayerTeam.Team3,
   PlayerTeam.Team4
  };

        // OnStartup - Method
        public void OnStartup()
        {
            Game.RunCommand("/MSG " + version);

            foreach (IPlayer player in Game.GetPlayers())
            {
                playerList.Add(new Player(player, AssignLevel(player), AssignEXP(player)));
            }

            {
                // I will first use "ShutDownMan's Two Teams" script for testing purpose.
                // I will not focus on teams now.
                List<IPlayer> players = new List<IPlayer>();
                foreach (IPlayer player in Game.GetPlayers()) players.Add(player);

                Teams.Add(PlayerTeam.Team1, 0);
                Teams.Add(PlayerTeam.Team2, 0);

                while (true)
                {
                    int rndIndex = random.Next(players.Count);
                    IPlayer ply = players[rndIndex];

                    if (ply.GetTeam() == PlayerTeam.Independent)
                    {
                        ply.SetTeam(SelectTeam());
                    }

                    players.Remove(players[rndIndex]);
                    if (players.Count == 0) break;
                }
            }

            SetCamera("GameCamera");
        }

        #endregion


        #region code
        // Helper - Flags
        public void AddState(State state, IPlayer player)
        {
            Player _player = GetPlayerData(player);

            if ((state & State.None) != 0)
            {
                player.SetInputEnabled(true);
            }
            if ((state & State.Bleeding) != 0)
            {
                player.SetHealth(player.GetHealth() - 0.2f);
            }
            if ((state & State.Blind) != 0)
            {
                player.SetInputEnabled(false);
            }
            if ((state & State.Healing) != 0)
            {
                player.SetHealth(player.GetHealth() + 25);
                _player.bleeding_hits = player.Statistics.TotalProjectilesHitBy + _player.bleeding_hits;
            }
        }

        // Temporary timer (test)
        bool ready_equip = false;
        public void TimerTest(TriggerArgs args)
        {
            if (playerList.Count != 0)
            {
                for (int i = playerList.Count - 1; i >= 0; --i)
                {
                    Player player = playerList[i];
                    IPlayer _player = player.player;
                    IPlayerStatistics statistics = _player.Statistics;

                    // TO-DO #1	
                    // seriously, this is a mess wtf
                    if (statistics.TotalFireDamageTaken > player.fire_ATK && !_player.IsDead)
                    {
                        _player.SetHealth(_player.GetHealth() - (fire_const_DMG * fire_DMG));
                        player.fire_ATK = statistics.TotalFireDamageTaken;
                    }
                    if (statistics.TotalFallDamageTaken > player.fall_ATK && !_player.IsDead)
                    {
                        fall_const_DMG = (fall_const_DMG == 1) ? statistics.TotalFallDamageTaken : fall_const_DMG;
                        _player.SetHealth(_player.GetHealth() - (fall_const_DMG * fall_DMG));
                        player.fall_ATK = statistics.TotalFallDamageTaken;
                    }
                    if (statistics.TotalExplosionDamageTaken > player.explosion_ATK && !_player.IsDead)
                    {
                        // To adjust
                        // int chance = random.Next(1, 10);
                        // if (chance <= player.blind_chance) { AddState(State.Bleeding, _player); Game.ShowPopupMessage("bleeding"); }

                        explosion_const_DMG = (explosion_const_DMG == 1) ? statistics.TotalExplosionDamageTaken : explosion_const_DMG;
                        _player.SetHealth(_player.GetHealth() - (explosion_const_DMG * explosion_DMG));
                        player.explosion_ATK = statistics.TotalExplosionDamageTaken * explosion_const_DMG;
                    }
                    if (statistics.TotalProjectilesHitBy > player.projectile_ATK && !_player.IsDead)
                    {
                        projectile_const_DMG = (projectile_const_DMG == 1) ? statistics.TotalProjectileDamageTaken : projectile_const_DMG;
                        _player.SetHealth(_player.GetHealth() - (projectile_const_DMG * projectile_DMG));
                        player.projectile_ATK++;
                    }
                    // TO-DO #1

                    // Bleeding event
                    if (statistics.TotalProjectilesHitBy >= player.bleeding_hits && !_player.IsDead)
                    {
                        AddState(State.Bleeding, player.player);
                    }
                    if (_player.GetHealth() <= 1 && !_player.IsDead) _player.Kill();

                    // Give equipment
                    if (available_time == 0)
                    {
                        if (!ready_equip)
                        {
                            player.player.GiveWeaponItem(WeaponString(primary_weapons[player.selection_primary]));
                            player.player.GiveWeaponItem(WeaponString(secondary_weapons[player.selection_secondary]));

                            foreach (IPlayer vplayer in Game.GetPlayers())
                            {
                                if (vplayer.GetTeam() == PlayerTeam.Team1)
                                {
                                    vplayer.SetWorldPosition(GetObject("BlueSpawn1").GetWorldPosition());
                                }
                                else if (vplayer.GetTeam() == PlayerTeam.Team2)
                                {
                                    vplayer.SetWorldPosition(GetObject("RedSpawn1").GetWorldPosition());
                                }
                            }

                            ready_equip = true;
                            selectingState = false;
                        }
                    }
                }

                // Camera movement
                if (GetObject("GameCamera").GetWorldPosition().Y > GetObject("CameraDestMAIN").GetWorldPosition().Y &&
                 available_time <= 0)
                {
                    GetObject("GameCamera").SetWorldPosition(GetObject("GameCamera").GetWorldPosition() - new Vector2(0, 3));
                    SetCamera("GameCamera");

                    foreach (IPlayer player in Game.GetPlayers()) player.SetInputEnabled(false);
                }
                else
                {
                    foreach (IPlayer player in Game.GetPlayers()) player.SetInputEnabled(true);
                }

                // Write / Check Game.Data
                foreach (IPlayer player in Game.GetPlayers())
                {
                    if (!player.IsDead || !player.IsRemoved) WriteData(player);

                    if (GetPlayerData(player).EXP > exp[GetPlayerData(player).level])
                    {
                        GetPlayerData(player).level = (GetPlayerData(player).level != 2) ? GetPlayerData(player).level + 1 : GetPlayerData(player).level;
                        GetPlayerData(player).EXP = 0;
                    }
                }

                Game.ShowPopupMessage(String.Format("Time: {0}", time_game));

                CheckGameover();
                InitializeLabels();
                PlayerUpdate();
            }
        }
        public void AvarageTimer(TriggerArgs args)
        {
            PlayerSelectionUpdate();
        }
        public void TimeGame(TriggerArgs args)
        {
            time_game = (time_game != 0) ? time_game - 1 : time_game;
            if (time_game == 0 && win)
            {
                Game.SetGameOver("LATE");
            }
        }

        // Timer (start)
        IObjectText start_count;
        public void StartTimeCount(TriggerArgs args)
        {
            available_time = (available_time > 0) ? available_time - 1 : available_time;

            start_count = (IObjectText)GetObject("StartTimeText");
            start_count.SetText("Time remaining: " + available_time.ToString());
            if (available_time <= 10)
            {
                start_count.SetTextColor(Color.Red);
            }
        }

        // Projectile spawn
        public void ProSpa(TriggerArgs args)
        {
            Game.SpawnProjectile(ProjectileItem.PISTOL, GetObject("ProjectileSpawn").GetWorldPosition(), new Vector2(0, 0));
        }

        // Initialize labels
        public void InitializeLabels()
        {
            if (playerList.Count != 0)
            {
                for (int i = playerList.Count - 1; i >= 0; --i)
                {
                    Player player = playerList[i];

                    // accurate check of the LBLs
                    if (player.LBL_owneq == null && player.LBL_equipment == null)
                    {
                        IObjectText owneq = (IObjectText)Game.CreateObject("Text");
                        IObjectText equipment = (IObjectText)Game.CreateObject("Text");

                        player.LBL_owneq = owneq;
                        player.LBL_equipment = equipment;
                    }

                    if (player.LBL_owneq != null && player.LBL_equipment != null && (!player.player.IsRemoved || player.player.IsDead))
                    {
                        player.LBL_owneq.SetText(String.Format("[ {0} ]", special_equipment[player.selection_special]));
                        player.LBL_owneq.SetWorldPosition(player.player.GetWorldPosition() + new Vector2(-2, 38));
                        player.LBL_owneq.SetTextAlignment(TextAlignment.Middle);

                        player.LBL_equipment.SetText(
                         String.Format("{0} \n{1}\nEXP: {2}/{3} \n------------------------------------------ \n{4}Primary: {5} \n{6}Secondary: {7} \n{8}Special: {9}",
                          player.player.GetProfile().Name, level[player.level], player.EXP, exp[player.level],
                          player.selection[0], primary_weapons[player.selection_primary],
                          player.selection[1], secondary_weapons[player.selection_secondary],
                          player.selection[2], special_equipment[player.selection_special])
                        );
                        player.LBL_equipment.SetWorldPosition(GetObject("PlayerStatistic" + i).GetWorldPosition());
                    }
                }
            }
        }

        // Check if the player is using an item
        public void PlayerUpdate()
        {
            for (int i = playerList.Count - 1; i >= 0; --i)
            {
                Player player = playerList[i];

                if (player.player.IsWalking /* More states */ && player.player.IsBlocking)
                {
                    UseItem(special_equipment[player.selection_special], player.player);
                    player.selection_special = 0; // none
                }
            }
        }

        public void PlayerSelectionUpdate()
        {
            if (playerList.Count != 0)
            {
                for (int i = playerList.Count - 1; i >= 0; --i)
                {
                    Player player = playerList[i];

                    if (CheckDirection(player.player) == PlayerDirection.DOWN && selectingState && !player.player.IsRolling)
                    {
                        if (player._selection + 1 != selection_MAX)
                        {
                            player.selection[player._selection] = "";
                            player.selection[player._selection + 1] = ">";
                            player._selection++;
                        }
                    }
                    if (CheckDirection(player.player) == PlayerDirection.UP && selectingState && !player.player.IsRolling)
                    {
                        if (player._selection - 1 != -1)
                        {
                            player.selection[player._selection] = "";
                            player.selection[player._selection - 1] = ">";
                            player._selection--;
                        }
                    }
                    else if (CheckDirection(player.player) == PlayerDirection.RIGHT && selectingState && !player.player.IsRolling)
                    {
                        if (player._selection == 0)
                            player.selection_primary = (player.selection_primary + 1 != primary_MAX) ? player.selection_primary + 1 : player.selection_primary;
                        else if (player._selection == 1)
                            player.selection_secondary = (player.selection_secondary + 1 != secondary_MAX) ? player.selection_secondary + 1 : player.selection_secondary;
                        else if (player._selection == 2)
                            player.selection_special = (player.selection_special + 1 != special_MAX) ? player.selection_special + 1 : player.selection_special;
                    }
                    else if (CheckDirection(player.player) == PlayerDirection.LEFT && selectingState && !player.player.IsRolling)
                    {
                        if (player._selection == 0)
                            player.selection_primary = (player.selection_primary - 1 != -1) ? player.selection_primary - 1 : player.selection_primary;
                        else if (player._selection == 1)
                            player.selection_secondary = (player.selection_secondary - 1 != -1) ? player.selection_secondary - 1 : player.selection_secondary;
                        else if (player._selection == 2)
                            player.selection_special = (player.selection_special - 1 != -1) ? player.selection_special - 1 : player.selection_special;
                    }
                }
            }
        }

        // Weapon initial set-up
        public void WeaponSetup(string weapon, IPlayer player)
        {
            player.GiveWeaponItem(WeaponString(weapon));
        }

        // Use item
        public void UseItem(string item, IPlayer player)
        {
            switch (item)
            {
                case "Small medkit":
                    AddState(State.None | State.Healing, player);
                    break;
                case "Airdrop":
                    IObjectSpawnObjectTrigger spawn_item = (IObjectSpawnObjectTrigger)GetObject("ItemSpawn");
                    spawn_item.SetWorldPosition(player.GetWorldPosition() + new Vector2(0, 250));
                    spawn_item.SetSpawnTileID("SupplyCrate00");
                    spawn_item.Trigger();
                    break;
            }
        }

        // Player direction timer
        public PlayerDirection CheckDirection(IPlayer player)
        {
            if (player.IsCrouching)
            {
                return PlayerDirection.DOWN;
            }
            else if (player.IsInMidAir)
            {
                return PlayerDirection.UP;
            }
            else if ((player.IsRunning || player.IsSprinting || player.IsWalking) && player.FacingDirection == 1)
            {
                return PlayerDirection.RIGHT;
            }
            else if ((player.IsRunning || player.IsSprinting || player.IsWalking) && player.FacingDirection == -1)
            {
                return PlayerDirection.LEFT;
            }

            return PlayerDirection.NONE;
        }

        // Little team selection
        PlayerTeam[] teams = {
   PlayerTeam.Team1,
   PlayerTeam.Team2
  };
        int players_blue = 0, players_red = 0;

        public PlayerTeam SelectTeam()
        {
            // I will first use "ShutDownMan's Two Teams" script for testing purpose.
            // I will not focus on teams now.

            PlayerTeam team = PlayerTeam.Independent;
            int less = Teams[_teams[0]];
            for (int i = Teams.Count - 1; i >= 0; i--)
            {
                if (Teams[_teams[i]] <= less)
                {
                    team = teams[i];
                    less = Teams[_teams[i]];
                }
            }

            Teams[team]++;
            return team;
        }
        // Gameover
        bool win = true, dd = false;
        int blue = 0, red = 0;
        public void CheckGameover()
        {
            if (!dd)
            {
                for (int i = Game.GetPlayers().Length - 1; i >= 0; --i)
                {
                    IPlayer player = Game.GetPlayers()[i];

                    if (player.GetTeam() == PlayerTeam.Team1) blue++;
                    else if (player.GetTeam() == PlayerTeam.Team2) red++;
                }
                dd = true;
            }

            if (blue == 0 && !selectingState && win && Game.GetActiveUsers().Length >= 2)
            {
                Game.SetGameOver("RED TEAM WINS");
                GiveEXP(PlayerTeam.Team2);
                win = false;
            }
            else if (red == 0 && !selectingState && win && Game.GetActiveUsers().Length >= 2)
            {
                Game.SetGameOver("BLUE TEAM WINS");
                GiveEXP(PlayerTeam.Team1);
                win = false;
            }
            else if (Game.GetPlayers().Length == 1)
            {
                if (Game.GetActiveUsers().Length > 1)
                {
                    Game.SetGameOver("RESTART");
                }
            }
        }

        public void PlayerDeath(TriggerArgs args)
        {
            IPlayer player = (IPlayer)args.Sender;

            GetPlayerData(player).LBL_owneq.Remove();

            if (player.GetTeam() == PlayerTeam.Team1) blue--;
            else if (player.GetTeam() == PlayerTeam.Team2) red--;
        }
        #endregion


        #region helpers
        // To seconds
        public int sToMs(float time)
        {
            return (int)(time * 1000);
        }
        // Delay
        public float TimeDelay(float delay)
        {
            return Game.TotalElapsedGameTime + delay;
        }
        // Object selection
        public IObject GetObject(string obj)
        {
            return (IObject)Game.GetSingleObjectByCustomId(obj);
        }
        // Camera set-up
        public void SetCamera(string cam)
        {
            IObject[] camera = Game.GetObjectsByCustomID(cam);

            Game.SetCameraArea(camera[0]);
        }

        public WeaponItem WeaponString(string weapon)
        {
            return (WeaponItem)Enum.Parse(typeof(WeaponItem), weapon.ToUpper());
        }

        public Player GetPlayerData(IPlayer _player)
        {
            for (int i = playerList.Count - 1; i >= 0; --i)
            {
                Player player = playerList[i];

                if (player.player.GetUser() == _player.GetUser()) return player;
            }
            return null;
        }

        public void GiveEXP(PlayerTeam team)
        {
            // really hard
            int random_exp = random.Next(1, 5);

            foreach (IPlayer player in Game.GetPlayers())
            {
                GetPlayerData(player).EXP += random_exp;
            }
        }

        // messy, really messy
        public int AssignLevel(IPlayer player)
        {
            if (Game.Data.Contains(player.GetProfile().Name + ":") && !player.IsDead)
            {
                string name = player.GetProfile().Name + ":";
                int index_name = Game.Data.IndexOf(name);

                int index_db = index_name + name.Length;
                int index_ln = Game.Data.IndexOf("|", index_name) + 1;
                int index_cut = index_ln - index_db - 1;

                string data_string = Game.Data.Substring(index_db, index_cut);
                int index_dp = data_string.IndexOf(";");
                string level_data = data_string.Substring(0, index_dp);

                return Int32.Parse(level_data);
            }
            return 0;
        }
        public int AssignEXP(IPlayer player)
        {
            if (Game.Data.Contains(player.GetProfile().Name + ":") && !player.IsDead)
            {
                string name = player.GetProfile().Name + ":";
                int index_name = Game.Data.IndexOf(name);

                int index_db = index_name + name.Length;
                int index_ln = Game.Data.IndexOf("|", index_name) + 1;
                int index_cut = index_ln - index_db - 1;

                string data_string = Game.Data.Substring(index_db, index_cut);
                int index_dp = data_string.IndexOf(";");
                string level_data = data_string.Substring(0, index_dp);
                char last = data_string[data_string.Length - 1];
                int last_index = data_string.LastIndexOf(last);
                int index_exp = last_index - index_dp;
                string exp_data = data_string.Substring(index_dp + 1, index_exp);

                return Int32.Parse(exp_data);
            }
            return 0;
        }
        public void WriteData(IPlayer player)
        {
            if (Game.Data.Contains(player.GetProfile().Name + ":") && !player.IsDead)
            {
                // Game.Data store format: {playerName}:{level};{exp}|

                string name = player.GetProfile().Name + ":";
                int index_name = Game.Data.IndexOf(name);

                int index_db = index_name + name.Length;
                int index_ln = Game.Data.IndexOf("|", index_name) + 1;
                int index_cut = index_ln - index_db - 1;

                string data_string = Game.Data.Substring(index_db, index_cut); // x;x
                int index_dp = data_string.IndexOf(";"); //  ; 
                string level_data = data_string.Substring(0, index_dp); // x
                char last = data_string[data_string.Length - 1];
                int last_index = data_string.LastIndexOf(last);
                int index_exp = last_index - index_dp;
                string exp_data = data_string.Substring(index_dp + 1, index_exp); //   x

                string before_level = Game.Data.Substring(0, index_db);
                int length = Game.Data.Length - before_level.Length - level_data.Length - exp_data.Length - 1;
                string after_level = Game.Data.Substring(index_ln - 1, length);

                level_data = GetPlayerData(player).level.ToString();
                exp_data = GetPlayerData(player).EXP.ToString();

                Game.Data = before_level + level_data + ";" + exp_data + after_level;
            }
            else if (!Game.Data.Contains(player.GetProfile().Name + ":"))
            {
                Game.RunCommand("/MSG ROTTEN CITY: Creating player data");
                Game.Data += player.GetProfile().Name + ":0;0|";
            }
        }
        #endregion
    }
}