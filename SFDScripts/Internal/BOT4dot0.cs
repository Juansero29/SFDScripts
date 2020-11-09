using SFDGameScriptInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFDScripts.Internal
{
    public class BOT4dot0 : GameScriptInterface
	{

		public BOT4dot0() : base(null) { }
        #region Script To Copy

        List<PlayerData> Players = new List<PlayerData>(8);

        private const int Time_Limit = 3 * 60;

        bool EditingMode = false;
        Random Rnd = new Random();

        int Equilibrium = 0;
        int BlueDeaths = 0;
        int RedDeaths = 0;

        private class PlayerData
        {
            public IPlayer Player = null;
            public IUser User = null;
            public IProfile Profile = null;
            public PlayerTeam Team = PlayerTeam.Independent;
            public Dictionary<short, WeaponItem> Weapons = new Dictionary<short, WeaponItem>();

            public int Points = 20;
            public float TimeStamp = 0;
            public int Faults = 0;
            public float ButtonTimeStamp = 0f;
            public int ButtonPressesFaults = 0;

            public bool IsEditing = false;
            public bool IsChoosingClass = true;
            public bool Leave = false;

            public int Index = 0;

            public PlayerData(IUser user)
            {
                this.Player = user.GetPlayer();
                this.User = user;
                this.Profile = user.GetProfile();
                this.Team = Player.GetTeam();
            }

            public PlayerData() { }
        }

        public void OnStartup()
        {
            foreach (IUser usr in Game.GetActiveUsers())
            {
                usr.GetPlayer().SetTeam(PlayerTeam.Team3);
                Players.Add(new PlayerData(usr));
            }
            Time_Placar = (IObjectText)Game.GetSingleObjectByCustomId("TimePlacar");
            CreateTimer(500, 0, "Mid_Tick", "");
            CreateTimer(1000, 0, "Timing_Tick", "");
            IObjectTrigger d_Trigger = (IObjectTrigger)Game.CreateObject("OnPlayerDeathTrigger");
            d_Trigger.SetScriptMethod("OnPlayerDeath");
            PriceSetup();
        }

        private class Teleporting
        {
            public IPlayer Player = null;
            public Vector2 Pos = Vector2.Zero;
            public int Times = 0;

            public Teleporting(IPlayer ply, Vector2 pos)
            {
                this.Player = ply;
                this.Pos = pos;
            }
        }

        public void SelectTeam(TriggerArgs args)
        {
            IPlayer sender = (IPlayer)args.Sender;
            IObject caller = (IObject)args.Caller;
            int ind = GetPlayerDataIndex(sender);
            bool IsAllowed = false;
            CheckEquilibrium();
            if (!sender.IsDiving && ind != 42)
                switch (caller.CustomId)
                {
                    case "Team Select blue":
                        if (Equilibrium + 1 < 2)
                        {
                            Players[ind].Team = PlayerTeam.Team1;
                            IsAllowed = true;
                        }
                        break;

                    case "Team Select red":
                        if (Equilibrium - 1 > -2)
                        {
                            Players[ind].Team = PlayerTeam.Team2;
                            IsAllowed = true;
                        }
                        break;

                    default: break;
                }
            if (IsAllowed)
            {
                SetPosition(sender, "Old_Classes_Room");
                Players[ind].IsChoosingClass = true;
                Players[ind].Index = ind;
            }
            else
                Game.PlaySound("MenuCancel", sender.GetWorldPosition(), 1f);

            RefreshPlacar();
            CheckEquilibrium();
        }

        public void Second_Room(TriggerArgs args)
        {
            IPlayer sender = (IPlayer)args.Sender;
            IObject caller = (IObject)args.Caller;
            int ind = GetPlayerDataIndex(sender);
            if (!sender.IsDiving && ind != 42)
            {
                switch (caller.CustomId)
                {
                    case "Fight":
                        string spawnPos = "";
                        if (Players[ind].Team == PlayerTeam.Team1)
                            spawnPos += "Blue_";
                        else
                            spawnPos += "Red_";
                        spawnPos += "Spawn_";
                        spawnPos += Rnd.Next(0, 6).ToString();
                        SetPosition(sender, spawnPos);
                        Players[ind].IsChoosingClass = false;
                        Players[ind].ButtonPressesFaults = 0;
                        sender.SetTeam(Players[ind].Team);
                        if (Players[ind].Weapons.Count == 0)
                        {
                            sender.GiveWeaponItem(WeaponItem.SLOWMO_10);
                            sender.GiveWeaponItem(WeaponItem.BAT);
                        }
                        break;

                    case "Player's class":
                        SetPosition(sender, "P_Class_Room");
                        Players[ind].IsEditing = true;
                        Players[ind].TimeStamp = Game.TotalElapsedGameTime;
                        break;

                    case "Edit class":
                        SetPosition(sender, "Edit_Room");
                        Players[ind].IsEditing = true;
                        Players[ind].TimeStamp = Game.TotalElapsedGameTime;
                        EditingScreen();
                        break;

                    case "Back":
                        SetPosition(sender, "Second_Room");
                        Players[ind].IsEditing = false;
                        Players[ind].TimeStamp = 0f;
                        break;
                }
            }
            RefreshPlacar();
        }

        public void Old_Classes_Room(TriggerArgs args)
        {
            IPlayer sender = (IPlayer)args.Sender;
            int ind = GetPlayerDataIndex(sender);
            SetPosition(sender, "Old_Classes_Room");
            Players[ind].IsEditing = false;
        }

        public void Old_Classes(TriggerArgs args)
        {
            IPlayer sender = (IPlayer)args.Sender;
            if ((sender != null) && (sender is IPlayer) && (!sender.IsDiving))
            {
                IObject caller = (IObject)args.Caller;
                int ind = GetPlayerDataIndex(sender);
                Dictionary<short, WeaponItem> weapons = new Dictionary<short, WeaponItem>();
                switch (caller.CustomId)
                {
                    case "ASSAULT":
                        sender = (IPlayer)args.Sender;
                        removeWeapons(sender);
                        weapons.Add(3, WeaponItem.ASSAULT);
                        weapons.Add(2, WeaponItem.UZI);
                        break;


                    case "MEXICAN":
                        sender = (IPlayer)args.Sender;
                        removeWeapons(sender);
                        weapons.Add(1, WeaponItem.MACHETE);
                        weapons.Add(2, WeaponItem.MAGNUM);
                        weapons.Add(5, WeaponItem.SLOWMO_5);
                        break;


                    case "RUNNER":
                        sender = (IPlayer)args.Sender;
                        removeWeapons(sender);
                        weapons.Add(1, WeaponItem.KATANA);
                        weapons.Add(3, WeaponItem.SAWED_OFF);
                        weapons.Add(2, WeaponItem.REVOLVER);
                        weapons.Add(5, WeaponItem.SLOWMO_5);
                        break;


                    case "MARKSMAN":
                        sender = (IPlayer)args.Sender;
                        removeWeapons(sender);
                        weapons.Add(3, WeaponItem.SNIPER);
                        weapons.Add(2, WeaponItem.PISTOL);
                        weapons.Add(5, WeaponItem.SLOWMO_10);
                        break;


                    case "TECHNICIAN":
                        sender = (IPlayer)args.Sender;
                        removeWeapons(sender);
                        weapons.Add(1, WeaponItem.PIPE);
                        weapons.Add(3, WeaponItem.SHOTGUN);
                        weapons.Add(5, WeaponItem.SLOWMO_10);
                        break;


                    case "REVOLUTIONIST":
                        sender = (IPlayer)args.Sender;
                        removeWeapons(sender);
                        weapons.Add(1, WeaponItem.AXE);
                        weapons.Add(3, WeaponItem.SUB_MACHINEGUN);
                        weapons.Add(2, WeaponItem.PISTOL);
                        weapons.Add(5, WeaponItem.SLOWMO_5);
                        break;


                    default:
                        Game.ShowPopupMessage("Error dood");
                        break;
                }
                ResetWeapons(ind);
                Players[ind].Points = 20;
                Players[ind].Weapons = weapons;
                foreach (KeyValuePair<short, WeaponItem> wpn in weapons)
                {
                    sender.GiveWeaponItem(wpn.Value);
                    Players[ind].Points -= Prices[wpn.Value];
                }

                if (Game.TotalElapsedGameTime - Players[ind].ButtonTimeStamp < 800)
                {
                    Players[ind].ButtonPressesFaults++;
                }
                if (Players[ind].ButtonPressesFaults >= 12)
                {
                    Game.RunCommand(string.Format("/MSG Server: {0} STOP SPAMMING IT!!", sender.Name));
                    Second_Room(new TriggerArgs((IObjectTrigger)Game.GetSingleObjectByCustomId("Fight"), Players[ind].Player, true));
                    Players[ind].ButtonPressesFaults = 0;
                }
                Players[ind].ButtonTimeStamp = Game.TotalElapsedGameTime;
            }
        }

        public void Mid_Tick(TriggerArgs args)
        {
            EditingScreen();
            Drop_in();
            WeaponRefresh();
        }

        IObjectText Time_Placar = null;
        Color yellow = new Color(255, 255, 0);
        int Total_Secs_Remaining = 0;
        float startTime = 0;
        public void Timing_Tick(TriggerArgs args)
        {
            int secsNow = (int)((Game.TotalElapsedGameTime - startTime) / 1000f);
            int totalSecsRemaining = Time_Limit - secsNow; Total_Secs_Remaining = totalSecsRemaining;
            int minsRemaining = totalSecsRemaining / 60;
            int SecsRemaining = totalSecsRemaining - minsRemaining * 60;
            if (Game.GetActiveUsers().Length == 1) startTime = Game.TotalElapsedGameTime;
            if (totalSecsRemaining > 0)
            {
                if (minsRemaining > 0)
                {
                    Time_Placar.SetText(string.Format("Time remaining {0}:{1:00}", minsRemaining, SecsRemaining));
                }
                else
                {
                    Time_Placar.SetText(string.Format("Time remaining 0:{0:00}", SecsRemaining));
                }
            }
            else
            {
                Color color = Color.White;
                if (!Time_Placar.GetTextColor().Equals(yellow))
                {
                    color = yellow;
                }

                Time_Placar.SetText(string.Format("<== Blue kills: {0:00}     \n     Red kills: {1:00} ==>", RedDeaths, BlueDeaths));
                Time_Placar.SetTextColor(color);
                CheckGameOver(true);
            }
        }

        private void EditingScreen()
        {
            bool SomeoneEditing = false;
            for (int i = Players.Count - 1; i >= 0; i--)
            {
                if (!SomeoneEditing && Players[i].IsEditing && Players[i].Player.GetWorldPosition().Y < 250f)
                {
                    SomeoneEditing = true;
                }
                if (Players[i].TimeStamp != 0f && Game.TotalElapsedGameTime - Players[i].TimeStamp > 20000f)
                {
                    Second_Room(new TriggerArgs((IObjectTrigger)Game.GetSingleObjectByCustomId("Back"), Players[i].Player, true));
                    Players[i].Faults++;
                }
                if (Players[i].Faults >= 3)
                {
                    Second_Room(new TriggerArgs((IObjectTrigger)Game.GetSingleObjectByCustomId("Fight"), Players[i].Player, true));
                    Players[i].Faults = 0;
                }
            }

            Game.WriteToConsole(Equilibrium.ToString());

            if (Players.Count == 2)
            {
                if (Players[0].Team == Players[1].Team)
                {
                    int rnd = Rnd.Next(0, 3);
                    Players[rnd].Team = PlayerTeam.Team3;
                    Players[rnd].Player.SetTeam(PlayerTeam.Team3);
                    SetPosition(Players[rnd].Player, "Choose Team Room");
                    CheckEquilibrium();
                }
            }
            int equilibrium = CheckEquilibrium();
            if (equilibrium > 0)
            {
                for (int a = 0; a > equilibrium; a--)
                {
                    for (int i = Players.Count - 1; i >= 0; i--)
                    {
                        if (Players[i].Team == PlayerTeam.Team1 && !Players[i].Leave)
                        {
                            Players[i].Team = PlayerTeam.Team2;
                            Players[i].Player.SetTeam(PlayerTeam.Team3);
                            Second_Room(new TriggerArgs((IObjectTrigger)Game.GetSingleObjectByCustomId("Back"), Players[i].Player, true));
                        }
                    }
                }
            }
            if (equilibrium < 0)
            {
                for (int a = 0; a < equilibrium; a++)
                {
                    for (int i = Players.Count - 1; i >= 0; i--)
                    {
                        if (Players[i].Team == PlayerTeam.Team2 && !Players[i].Leave)
                        {
                            Players[i].Team = PlayerTeam.Team1;
                            Players[i].Player.SetTeam(PlayerTeam.Team3);
                            Second_Room(new TriggerArgs((IObjectTrigger)Game.GetSingleObjectByCustomId("Back"), Players[i].Player, true));
                        }
                    }
                }
            }

            if (SomeoneEditing && !EditingMode)
            {
                Game.SetCameraArea(70, -300, -340, 240);
                EditingMode = true;
                RefreshCam();
            }
            else if (!SomeoneEditing && EditingMode)
            {
                Game.SetCameraArea(70, -300, -250, 240);
                EditingMode = false;
                RefreshCam();
            }
        }

        public void SetText(TriggerArgs args)
        {
            IObject caller = (IObject)args.Caller;
            string text = "";
            if (args.Sender is IPlayer)
            {
                IPlayer sender = (IPlayer)args.Sender;
                int ind = GetPlayerDataIndex(sender);
                switch (caller.CustomId)
                {
                    case "Text_01_AreaTrigger":
                        text = "Blue team";
                        break;

                    case "Text_02_AreaTrigger":
                        text = "Red team";
                        break;

                    case "Text_03_AreaTrigger":
                        text = "Players' class";
                        break;

                    case "Text_04_AreaTrigger":
                        text = "Assault";
                        break;

                    case "Text_05_AreaTrigger":
                        text = "Mexican";
                        break;

                    case "Text_06_AreaTrigger":
                        text = "Runner";
                        break;

                    case "Text_07_AreaTrigger":
                        text = "Marksman";
                        break;

                    case "Text_08_AreaTrigger":
                        text = "Technician";
                        break;

                    case "Text_09_AreaTrigger":
                        text = "Revolutionist";
                        break;

                    case "Text_10_AreaTrigger":
                        text = "Baseball Bat\n" + (-Prices[WeaponItem.BAT]).ToString();
                        break;

                    case "Text_11_AreaTrigger":
                        text = "Machete\n" + (-Prices[WeaponItem.MACHETE]).ToString();
                        break;

                    case "Text_12_AreaTrigger":
                        text = "Axe\n" + (-Prices[WeaponItem.AXE]).ToString();
                        break;

                    case "Text_13_AreaTrigger":
                        text = "Hammer\n" + (-Prices[WeaponItem.HAMMER]).ToString();
                        break;

                    case "Text_14_AreaTrigger":
                        text = "Katana\n" + (-Prices[WeaponItem.KATANA]).ToString();
                        break;

                    case "Text_15_AreaTrigger":
                        text = "pistol\n" + (-Prices[WeaponItem.KATANA]).ToString();
                        break;

                    case "Text_16_AreaTrigger":
                        text = "Revolver\n" + (-Prices[WeaponItem.REVOLVER]).ToString();
                        break;

                    case "Text_17_AreaTrigger":
                        text = "Flare Gun\n" + (-Prices[WeaponItem.FLAREGUN]).ToString();
                        break;

                    case "Text_18_AreaTrigger":
                        text = "Magnum\n" + (-Prices[WeaponItem.MACHETE]).ToString();
                        break;

                    case "Text_19_AreaTrigger":
                        text = "Uzi\n" + (-Prices[WeaponItem.UZI]).ToString();
                        break;

                    case "Text_20_AreaTrigger":
                        text = "Grenades\n" + (-Prices[WeaponItem.GRENADES]).ToString();
                        break;

                    case "Text_21_AreaTrigger":
                        text = "SLOWMO_5\n" + (-Prices[WeaponItem.SLOWMO_5]).ToString();
                        break;

                    case "Text_22_AreaTrigger":
                        text = "Laser\n" + (-Prices[WeaponItem.LAZER]).ToString();
                        break;

                    case "Text_23_AreaTrigger":
                        text = "Pipe whrench\n" + (-Prices[WeaponItem.PIPE]).ToString();
                        break;

                    case "Text_24_AreaTrigger":
                        text = "Molotovs\n" + (-Prices[WeaponItem.MOLOTOVS]).ToString();
                        break;

                    case "Text_25_AreaTrigger":
                        text = "Shotgun\n" + (-Prices[WeaponItem.SHOTGUN]).ToString();
                        break;

                    case "Text_26_AreaTrigger":
                        text = "Assault\n" + (-Prices[WeaponItem.ASSAULT]).ToString();
                        break;

                    case "Text_27_AreaTrigger":
                        text = "Tommy gun\n" + (-Prices[WeaponItem.SMG]).ToString();
                        break;

                    case "Text_28_AreaTrigger":
                        text = "Sniper\n" + (-Prices[WeaponItem.SNIPER]).ToString();
                        break;

                    case "Text_29_AreaTrigger":
                        text = "Sawed off\n" + (-Prices[WeaponItem.SAWED_OFF]).ToString();
                        break;

                    case "Text_30_AreaTrigger":
                        text = sender.Name + "'s points: " + Players[ind].Points.ToString();
                        break;

                    case "Text_31_AreaTrigger":
                        text = PlayerClassName(0);
                        break;

                    case "Text_32_AreaTrigger":
                        text = PlayerClassName(1);
                        break;

                    case "Text_33_AreaTrigger":
                        text = PlayerClassName(2);
                        break;

                    case "Text_34_AreaTrigger":
                        text = PlayerClassName(3);
                        break;

                    case "Text_35_AreaTrigger":
                        text = PlayerClassName(4);
                        break;

                    case "Text_36_AreaTrigger":
                        text = PlayerClassName(5);
                        break;

                    case "Text_37_AreaTrigger":
                        text = PlayerClassName(6);
                        break;

                    case "Text_38_AreaTrigger":
                        text = PlayerClassName(7);
                        break;
                }
            }
            ((IObjectText)Game.GetSingleObjectByCustomId(caller.CustomId.Substring(0, 7))).SetText(text);
        }

        Dictionary<WeaponItem, short> Prices = new Dictionary<WeaponItem, short>();
        private void PriceSetup()
        {
            Prices.Add(WeaponItem.BAT, 4);
            Prices.Add(WeaponItem.MACHETE, 5);
            Prices.Add(WeaponItem.AXE, 6);
            Prices.Add(WeaponItem.HAMMER, 5);
            Prices.Add(WeaponItem.KATANA, 7);
            Prices.Add(WeaponItem.PISTOL, 4);
            Prices.Add(WeaponItem.REVOLVER, 3);
            Prices.Add(WeaponItem.FLAREGUN, 6);
            Prices.Add(WeaponItem.MAGNUM, 7);
            Prices.Add(WeaponItem.UZI, 6);
            Prices.Add(WeaponItem.GRENADES, 5);
            Prices.Add(WeaponItem.SLOWMO_5, 3);
            Prices.Add(WeaponItem.SLOWMO_10, 7);
            //Prices.Add(WeaponItem.LAZER,	3);
            Prices.Add(WeaponItem.PIPE, 4);
            Prices.Add(WeaponItem.MOLOTOVS, 4);
            Prices.Add(WeaponItem.SHOTGUN, 7);
            Prices.Add(WeaponItem.ASSAULT, 6);
            Prices.Add(WeaponItem.SMG, 5);
            Prices.Add(WeaponItem.SNIPER, 8);
            Prices.Add(WeaponItem.SAWED_OFF, 6);
        }

        public void Buy_Weapon(TriggerArgs args)
        {
            IPlayer sender = (IPlayer)args.Sender;
            IObject caller = (IObject)args.Caller;
            int ind = GetPlayerDataIndex(sender);
            WeaponItem wpn = WeaponItem.NONE;
            short weaponIndex = 0;
            switch (caller.CustomId)
            {
                case "BAT":
                    wpn = WeaponItem.BAT;
                    weaponIndex = 1;
                    break;

                case "MACHETE":
                    wpn = WeaponItem.MACHETE;
                    weaponIndex = 1;
                    break;

                case "AXE":
                    wpn = WeaponItem.AXE;
                    weaponIndex = 1;
                    break;

                case "HAMMER":
                    wpn = WeaponItem.HAMMER;
                    weaponIndex = 1;
                    break;

                case "KATANA":
                    wpn = WeaponItem.KATANA;
                    weaponIndex = 1;
                    break;

                case "PISTOL":
                    wpn = WeaponItem.PISTOL;
                    weaponIndex = 2;
                    break;

                case "REVOLVER":
                    wpn = WeaponItem.REVOLVER;
                    weaponIndex = 2;
                    break;

                case "FLAREGUN":
                    wpn = WeaponItem.FLAREGUN;
                    weaponIndex = 2;
                    break;

                case "MAGNUM":
                    wpn = WeaponItem.MAGNUM;
                    weaponIndex = 2;
                    break;

                case "UZI":
                    wpn = WeaponItem.UZI;
                    weaponIndex = 2;
                    break;

                case "GRENADES":
                    wpn = WeaponItem.GRENADES;
                    weaponIndex = 4;
                    break;

                case "SLOWMO_5":
                    if (Prices[WeaponItem.SLOWMO_5] <= Players[ind].Points && !Players[ind].Weapons.ContainsKey(5))
                    {
                        wpn = WeaponItem.SLOWMO_5;
                    }
                    else if (Prices[WeaponItem.SLOWMO_10] <= Players[ind].Points)
                    {
                        wpn = WeaponItem.SLOWMO_10;
                    }
                    weaponIndex = 5;
                    break;

                case "LAZER":
                    wpn = WeaponItem.LAZER;
                    weaponIndex = 6;
                    break;

                case "PIPE":
                    wpn = WeaponItem.PIPE;
                    weaponIndex = 1;
                    break;

                case "MOLOTOVS":
                    wpn = WeaponItem.MOLOTOVS;
                    weaponIndex = 4;
                    break;

                case "SHOTGUN":
                    wpn = WeaponItem.SHOTGUN;
                    weaponIndex = 3;
                    break;

                case "ASSAULT":
                    wpn = WeaponItem.ASSAULT;
                    weaponIndex = 3;
                    break;

                case "TOMMYGUN":
                    wpn = WeaponItem.SMG;
                    weaponIndex = 3;
                    break;

                case "SNIPER":
                    wpn = WeaponItem.SNIPER;
                    weaponIndex = 3;
                    break;

                case "SAWED_OFF":
                    wpn = WeaponItem.SAWED_OFF;
                    weaponIndex = 3;
                    break;

            }

            if (wpn != WeaponItem.NONE)
            {
                if (Players[ind].Weapons.ContainsKey(weaponIndex))
                {
                    if ((Prices[Players[ind].Weapons[weaponIndex]] - Prices[wpn]) + Players[ind].Points >= 0)
                    {
                        Players[ind].Points += Prices[Players[ind].Weapons[weaponIndex]] - Prices[wpn];
                        Game.PlayEffect("PWT", sender.GetWorldPosition(), (Prices[Players[ind].Weapons[weaponIndex]] - Prices[wpn]).ToString());
                        Players[ind].Weapons[weaponIndex] = wpn;
                        sender.GiveWeaponItem(wpn);
                    }
                    else
                        Game.PlayEffect("PWT", sender.GetWorldPosition(), "not enough points\n" + Players[ind].Points.ToString() + " spare points");
                }
                else if (Prices[wpn] <= Players[ind].Points)
                {
                    Players[ind].Points -= Prices[wpn];
                    Players[ind].Weapons.Add(weaponIndex, wpn);
                    sender.GiveWeaponItem(wpn);
                }
                else
                    Game.PlayEffect("PWT", sender.GetWorldPosition(), "not enough points\n" + Players[ind].Points.ToString() + " spare points");
            }
            else
                Game.PlayEffect("PWT", sender.GetWorldPosition(), "not enough points\n" + Players[ind].Points.ToString() + " spare points");

            if (Game.TotalElapsedGameTime - Players[ind].ButtonTimeStamp < 800)
            {
                Players[ind].ButtonPressesFaults++;
            }
            if (Players[ind].ButtonPressesFaults >= 8)
            {
                Game.RunCommand(string.Format("/MSG Server: {0} STOP SPAMMING IT!!", sender.Name));
                Second_Room(new TriggerArgs((IObjectTrigger)Game.GetSingleObjectByCustomId("Fight"), Players[ind].Player, true));
                Players[ind].ButtonPressesFaults = 0;
            }
            Players[ind].ButtonTimeStamp = Game.TotalElapsedGameTime;
        }

        public void ResetButton(TriggerArgs args)
        {
            IPlayer sender = (IPlayer)args.Sender;
            int ind = GetPlayerDataIndex(sender);
            Players[ind].Points = 20;
            removeWeapons(sender);
            ResetWeapons(ind);
        }

        public void Gun_Destroyer(TriggerArgs args)
        {
            IObject sender = (IObject)args.Sender;
            sender.Remove();
            WeaponRefresh();
        }

        public void OnPlayerDeath(TriggerArgs args)
        {
            IPlayer sender = (IPlayer)args.Sender;
            IObject caller = (IObject)args.Caller;
            int ind = GetPlayerDataIndex(sender);
            if (sender != null && sender.GetUser() == null && ind != 42)
            {
                sender.Gib();
                Players[ind].Leave = true;
                CheckEquilibrium();
            }
            else if (sender != null && ind != 42)
            {
                if (Players[ind].Team == PlayerTeam.Team1 && Total_Secs_Remaining > 0)
                {
                    BlueDeaths++;
                }
                if (Players[ind].Team == PlayerTeam.Team2 && Total_Secs_Remaining > 0)
                {
                    RedDeaths++;
                }
                Players[ind].User = sender.GetUser();
                CreateTimer(10000, 1, "ReviveTimer", "");
                PlayersToRevive.Add(Players[ind]);
            }
            CheckGameOver(false);
            RefreshPlacar();
        }

        List<PlayerData> PlayersToRevive = new List<PlayerData>();
        public void ReviveTimer(TriggerArgs args)
        {
            int ind = PlayersToRevive[0].Index;
            if (PlayersToRevive[0].Player.GetUser() != null)
            {
                PlayersToRevive[0].Player.Remove();
                Players[ind].Player = SpawnUser(PlayersToRevive[0]);
                if (Players[ind].Team != PlayerTeam.Team3)
                    SetPosition(Players[ind].Player, "Second_Room");
                else
                    SetPosition(Players[ind].Player, "Choose Team Room");
                Players[ind].IsChoosingClass = true;
                Players[ind].Player.SetTeam(PlayerTeam.Team3);
                WeaponRefresh(); WeaponRefresh();
            }
            ((IObject)args.Caller).Remove();
            PlayersToRevive.Remove(PlayersToRevive[0]);
        }

        public void Drop_in()
        {
            foreach (IUser usr in Game.GetActiveUsers())
            {
                if (usr.GetPlayer() == null)
                {
                    if (GetUserDataIndex(usr) == 42)
                    {
                        IPlayer ply = Game.CreatePlayer(((IObject)Game.GetSingleObjectByCustomId("Choose Team Room")).GetWorldPosition());
                        ply.SetProfile(usr.GetProfile());
                        ply.SetTeam(PlayerTeam.Team3);
                        ply.SetUser(usr);
                        Players.Add(new PlayerData(usr));
                    }
                    else
                    {
                        int ind = GetUserDataIndex(usr);
                        if (Players[ind].Leave && ind != 42)
                        {
                            IPlayer ply = SpawnUser(Players[ind]);
                            ply.SetUser(usr);
                            Players[ind].Player = ply;
                            ply.SetTeam(PlayerTeam.Team3);
                            Players[ind].Leave = false;
                            foreach (KeyValuePair<short, WeaponItem> wpn in Players[ind].Weapons)
                                ply.GiveWeaponItem(wpn.Value);
                            SetPosition(Players[ind].Player, "Second_Room");
                        }
                    }
                }
            }
        }

        public void CheckGameOver(bool TimesOver)
        {
            if (!TimesOver)
            {
                if (BlueDeaths >= 5)
                {
                    Game.SetGameOver("Red Wins");
                }
                if (RedDeaths >= 5)
                {
                    Game.SetGameOver("Blue Wins");
                }
            }
            else
            {
                if (BlueDeaths > RedDeaths)
                {
                    Game.SetGameOver("Red Wins");
                }
                if (BlueDeaths < RedDeaths)
                {
                    Game.SetGameOver("Blue Wins");
                }
                if (BlueDeaths == RedDeaths)
                {
                    Game.SetGameOver("Draw");
                }
            }
        }

        List<Teleporting> ToTeleport = new List<Teleporting>();
        public void Teleport(TriggerArgs args)
        {
            if (args.Caller != null)
            {
                foreach (Teleporting TP in ToTeleport)
                {
                    if (TP.Times < 3)
                    {
                        TP.Player.SetWorldPosition(TP.Pos);
                        CreateTimer(50, 1, "Teleport", "");
                        TP.Times++;
                    }
                    else
                    {
                        ToTeleport.Remove(TP);
                        break;
                    }
                }
                ((IObject)args.Caller).Remove();
            }
            else
                CreateTimer(50, 1, "Teleport", "");
        }

        public void GetPlayerClassButt(TriggerArgs args)
        {
            IPlayer sender = (IPlayer)args.Sender;
            IObject caller = (IObject)args.Caller;
            int ind = GetPlayerDataIndex(sender);
            if (Players.Count - 1 >= Convert.ToInt32(caller.CustomId.Substring(12)))
            {
                removeWeapons(sender);
                ResetWeapons(ind);
                Players[ind].Points = 20;
                Dictionary<short, WeaponItem> wpns = PlayerClass(Players[Convert.ToInt32(caller.CustomId.Substring(12))].Player);
                foreach (KeyValuePair<short, WeaponItem> wpn in wpns)
                {
                    Players[ind].Points -= Prices[wpn.Value];
                    sender.GiveWeaponItem(wpn.Value);
                }
                Players[ind].Weapons = wpns;
            }
        }

        public void GiveAmmo(TriggerArgs args)
        {
            IPlayer sender = (IPlayer)args.Sender;
            IObjectTrigger caller = (IObjectTrigger)args.Caller;
            int ind = GetPlayerDataIndex(sender);
            foreach (KeyValuePair<short, WeaponItem> wpn in ActualWeapons(sender))
            {
                sender.GiveWeaponItem(wpn.Value);
            }
            sender.SetHealth(sender.GetHealth() + 25f);
            caller.SetEnabled(false);
            ToDespawn.Add(caller);
            CreateTimer(60000, 1, "DespawnTimer", "");
        }

        List<IObjectTrigger> ToDespawn = new List<IObjectTrigger>(10);
        public void DespawnTimer(TriggerArgs args)
        {
            ToDespawn[0].SetEnabled(true);
            ToDespawn.Remove(ToDespawn[0]);
            ((IObject)args.Caller).Remove();
        }

        int SecretButts = 0;
        public void SpawnGL(TriggerArgs args)
        {
            IObjectTrigger caller = (IObjectTrigger)args.Caller;
            if (caller.CustomId != "Master")
            {
                SecretButts++;
                caller.SetEnabled(false);
            }
            else if (SecretButts == 4)
            {
                foreach (IObject xxx in (IObject[])Game.GetObjectsByCustomId("XXX"))
                {
                    xxx.Remove();
                }
                Vector2 thisJoint = ((IObject)Game.GetSingleObjectByCustomId("This distance Joint")).GetWorldPosition();
                ((IObjectTrigger)Game.GetSingleObjectByCustomId("TriggerDebris")).Trigger();
                Game.TriggerExplosion(thisJoint);
                Game.SpawnWeaponItem(WeaponItem.GRENADE_LAUNCHER, thisJoint, true, 30000);
                caller.SetEnabled(false);
            }
        }

        public void DropedOff(TriggerArgs args)
        {
            IPlayer sender = (IPlayer)args.Sender;
            IObjectTrigger caller = (IObjectTrigger)args.Caller;
            int ind = GetPlayerDataIndex(sender);
            Players.Remove(Players[ind]);
            sender.Remove();
        }

        //-HELPERS-//

        private int GetPlayerDataIndex(IPlayer ply)
        {
            for (int i = Players.Count - 1; i >= 0; i--)
            {
                if (Players[i].Player.Name == ply.Name)
                {
                    return i;
                }
            }
            return 42;
        }

        private int GetUserDataIndex(IUser usr)
        {
            for (int i = PlayersToRevive.Count - 1; i >= 0; i--)
            {
                if (Players[i].User.UserId == usr.UserId)
                {
                    return i;
                }
            }
            return 42;
        }

        private IPlayer SpawnUser(PlayerData data)
        {
            IPlayer newPlayer = Game.CreatePlayer(Vector2.Zero);
            newPlayer.SetUser(data.User);
            newPlayer.SetProfile(data.Profile);
            return newPlayer;
        }

        private void WeaponRefresh()
        {
            for (int i = Players.Count - 1; i >= 0; i--)
            {
                if (Players[i].IsChoosingClass)
                {
                    Players[i].Player.SetHealth(100f);
                    Players[i].Player.RemoveWeaponItemType(Players[i].Player.CurrentWeaponDrawn);
                    foreach (KeyValuePair<short, WeaponItem> wpn in Players[i].Weapons)
                    {
                        if (ActualWeapons(Players[i].Player).ContainsKey(wpn.Key))
                        {
                            continue;
                        }
                        else
                        {
                            Players[i].Player.GiveWeaponItem(wpn.Value);
                        }
                    }
                }
            }
        }

        private IDictionary<short, WeaponItem> ActualWeapons(IPlayer ply)
        {
            Dictionary<short, WeaponItem> weapons = new Dictionary<short, WeaponItem>();
            if (ply.CurrentMeleeWeapon.WeaponItem != WeaponItem.NONE) weapons.Add(1, ply.CurrentMeleeWeapon.WeaponItem);
            if (ply.CurrentSecondaryWeapon.WeaponItem != WeaponItem.NONE) weapons.Add(2, ply.CurrentSecondaryWeapon.WeaponItem);
            if (ply.CurrentPrimaryWeapon.WeaponItem != WeaponItem.NONE) weapons.Add(3, ply.CurrentPrimaryWeapon.WeaponItem);
            if (ply.CurrentThrownItem.WeaponItem != WeaponItem.NONE) weapons.Add(4, ply.CurrentThrownItem.WeaponItem);
            if (ply.CurrentPowerupItem.WeaponItem != WeaponItem.NONE) weapons.Add(5, ply.CurrentPowerupItem.WeaponItem);
            return weapons;
        }

        private void RefreshPlacar()
        {
            int blues = 0;
            int reds = 0;
            ((IObjectText)Game.GetSingleObjectByCustomId("Blue Placar")).SetText("Blue Kills: " + RedDeaths.ToString());
            ((IObjectText)Game.GetSingleObjectByCustomId("Red Placar")).SetText("Red Kills: " + BlueDeaths.ToString());
            foreach (PlayerData data in Players)
            {
                if (data.Team == PlayerTeam.Team1 && !data.Leave)
                {
                    blues++;
                }
                if (data.Team == PlayerTeam.Team2 && !data.Leave)
                {
                    reds++;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                ((IObjectText)Game.GetSingleObjectByCustomId("Blue_Teams_Counter_" + i.ToString())).SetText("");
                ((IObjectText)Game.GetSingleObjectByCustomId("Red_Teams_Counter_" + i.ToString())).SetText("");
            }

            if (blues > 0)
                for (int i = 0; i < blues; i++)
                {
                    ((IObjectText)Game.GetSingleObjectByCustomId("Blue_Teams_Counter_" + i.ToString())).SetText(".");
                    ((IObjectTrigger)Game.GetSingleObjectByCustomId("AmmoCrate_" + (i + 1).ToString())).SetEnabled(true);

                }
            if (reds > 0)
                for (int i = 0; i < reds; i--)
                {
                    ((IObjectText)Game.GetSingleObjectByCustomId("Red_Teams_Counter_" + i.ToString())).SetText(".");
                    ((IObjectTrigger)Game.GetSingleObjectByCustomId("AmmoCrate_" + (i + 6).ToString())).SetEnabled(true);
                }
        }

        private Vector2 SetPosition(object obj, string oPos)
        {
            Vector2 pos = ((IObject)Game.GetSingleObjectByCustomId(oPos)).GetWorldPosition();
            if (obj is IPlayer)
            {
                ToTeleport.Add(new Teleporting((IPlayer)obj, pos));
                Teleport(new TriggerArgs(null, null, true));
                return ((IPlayer)obj).GetWorldPosition();
            }
            else if (obj is IObject)
            {
                ((IObject)obj).SetWorldPosition(pos, true);
                return ((IObject)obj).GetWorldPosition();
            }
            return pos;
        }

        public void RemoveText(TriggerArgs args)
        {
            IObject caller = (IObject)args.Caller;
            ((IObjectText)Game.GetSingleObjectByCustomId(caller.CustomId.Substring(0, 7))).SetText("");
        }

        public void removeWeapons(IPlayer ply)
        {
            ply.RemoveWeaponItemType(WeaponItemType.Rifle);
            ply.RemoveWeaponItemType(WeaponItemType.Handgun);
            ply.RemoveWeaponItemType(WeaponItemType.Melee);
            ply.RemoveWeaponItemType(WeaponItemType.Thrown);
            ply.RemoveWeaponItemType(WeaponItemType.Powerup);
        }
        private void ResetWeapons(int index)
        {
            Players[index].Weapons = new Dictionary<short, WeaponItem>();
        }

        private void RefreshCam()
        {
            Game.SetCurrentCameraMode(CameraMode.Static);
            Game.SetAllowedCameraModes(CameraMode.Static | CameraMode.Dynamic);
        }

        private string PlayerClassName(int ind)
        {
            if (Players.Count - 1 >= ind)
            {
                return Players[ind].Player.Name + "'s class";
            }
            return "";
        }

        private Dictionary<short, WeaponItem> PlayerClass(IPlayer ply)
        {
            int ind = GetPlayerDataIndex(ply);
            return Players[ind].Weapons;
        }

        private int CheckEquilibrium()
        {
            Equilibrium = 0;
            foreach (PlayerData data in Players)
            {
                if (data.Team == PlayerTeam.Team1)
                {
                    Equilibrium++;
                }
                if (data.Team == PlayerTeam.Team2)
                {
                    Equilibrium--;
                }
            }

            if (Equilibrium > 1)
            {
                return Equilibrium - 1;
            }
            if (Equilibrium < -1)
            {
                return Equilibrium + 1;
            }
            return 0;
        }

        private void CreateTimer(int interval, int count, string method, string id)
        {
            IObjectTimerTrigger timerTrigger = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
            timerTrigger.SetIntervalTime(interval);
            timerTrigger.SetRepeatCount(count);
            timerTrigger.SetScriptMethod(method);
            timerTrigger.CustomId = id;
            timerTrigger.Trigger();
        }
        #endregion
    }
}
