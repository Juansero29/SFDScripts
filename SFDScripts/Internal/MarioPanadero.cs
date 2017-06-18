using System;
using System.Collections.Generic;
using SFDGameScriptInterface;


namespace SFDScripts
{
    class MarioPanadero : GameScriptInterface
    {

        public MarioPanadero() : base(null) { }
        List<IPlayer> marios = new List<IPlayer>();
        int mariosNumber = 0;

        int TimeCTD = 5 * 60;
        bool TextF = false;
        int vRedKills = 0;
        int vBluKills = 0;
        IPlayer mCallerPlayer;
        IPlayer cpl2;
        IUser cus;
        IUser[] users;
        IObjectText ct;
        IObjectText ct2;
        IObject[] coba;
        IObject mCallerObject;
        Vector2 cve;
        Vector2 cve2;
        DeadPlayer cdpl;
        Random rnd = new Random();
        int i;
        bool gonein = false;
        PlayerTeam winner = PlayerTeam.Team3;
        public void Start(TriggerArgs args)
        {
            Shoot(300, 0, "Shoot", "");
            ConnectedPlayersTick(args);
            RefreshCounter(vBluKills, "BlueT");
            RefreshCounter(vRedKills, "RedT");
            crate = ((IObject)Game.GetSingleObjectByCustomId("crate"));
            cpos = crate.GetWorldPosition();
            SpawnPeach();
        }

        public void TimeRemaining(TriggerArgs args)
        {
            ct = (IObjectText)Game.GetSingleObjectByCustomId("TextCD");
            if (ct != null && TextF == false && gonein == false)
            {
                TimeCTD--;
                ct.SetText("Time Remaining: " + PrintMinutes(TimeCTD));
            }
            if (TimeCTD < 1 && !gonein)
            {
                gonein = true;
                WhoWins(args);
            }
        }
        public void TimerGreenPlayerIn(TriggerArgs args)
        {
            mCallerPlayer = (IPlayer)args.Sender;
            mCallerPlayer.SetTeam(PlayerTeam.Team3);
        }
        public void ClearPopup(TriggerArgs args)
        {
            Game.HidePopupMessage();
        }
        public void WhoWins(TriggerArgs args)
        {
            ct = (IObjectText)Game.GetSingleObjectByCustomId("TextCD");
            if (vRedKills > vBluKills)
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
            else if (vRedKills < vBluKills)
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
                TextF = true;
                ExplodePeach();
            }
        }
        private IObjectTrigger trig2 = null;
        private IObjectTrigger trig3 = null;
        private IObjectTrigger trig4 = null;
        private IPlayer ply = null;
        private IPlayer plyExit = null;
        private bool allowOpen = false;
        int rand;
        string[] GetposB = new string[]
        {
            "BluBase",
            "BluBase2",
            "MarioTwoBase",
            "BluBase4",
            "BluBase5"
        };
        string[] GetposR = new string[]
        {
        "MarioOneBase",
        "RedBase2",
        "RedBase3",
        "RedBase4",
        "RedBase5"
        };
        public void CheckEnter(TriggerArgs args)
        {
            if (args.Sender != null && args.Sender is IPlayer)
            {
                allowOpen = true;
            }
        }
        Vector2 cpos;
        IObject crate;
        bool didit = false;
        public void crateOpen(TriggerArgs args)
        {
            if (crate.GetHealth() == 0 && didit == false)
            {
                Game.SpawnWeaponItem(WeaponItem.GRENADE_LAUNCHER, cpos, true, 10000);
                didit = true;
                Game.GetSingleObjectByCustomId("IT").Destroy();
            }
            else
            {
                cpos = crate.GetWorldPosition();
            }
        }
        public string BlueRnd()
        {
            rand = RandNumber(0, 4);
            return (GetposB[rand]);
        }
        public string RedRnd()
        {
            rand = RandNumber(0, 4);
            return (GetposR[rand]);
        }
        public void refil(TriggerArgs args)
        {
            mCallerPlayer = (IPlayer)args.Sender;
            mCallerPlayer.GiveWeaponItem(WeaponItem.PILLS);
            mCallerPlayer.GiveWeaponItem(mCallerPlayer.CurrentSecondaryWeapon.WeaponItem);
            if (mCallerPlayer.CurrentPrimaryWeapon.WeaponItem != WeaponItem.MAGNUM)
            {
                mCallerPlayer.GiveWeaponItem(mCallerPlayer.CurrentPrimaryWeapon.WeaponItem);
            }
        }

        public void getAway(String id, IPlayer ply)
        {
            mCallerObject = Game.GetSingleObjectByCustomId(id);
            cve = mCallerObject.GetWorldPosition();
            ply.SetWorldPosition(cve);
        }
        int mNumberOfBluTeamPlayers = 0;
        int mNumberOfRedTeamPlayers = 0;

        public void MovetoBase(TriggerArgs args)
        {
            mCallerPlayer = (IPlayer)args.Sender;
            if ((mCallerPlayer != null) && (mCallerPlayer is IPlayer) && (!mCallerPlayer.IsDiving))
            {
                mCallerObject = (IObject)args.Caller;
                mCallerPlayer = (IPlayer)args.Sender;
                switch (mCallerObject.CustomId)
                {
                    case "MarioOne":
                        if (mNumberOfBluTeamPlayers >= mNumberOfRedTeamPlayers)
                        {
                            getAway(RedRnd(), mCallerPlayer);
                            mCallerPlayer.SetTeam(PlayerTeam.Team2);
                            mNumberOfRedTeamPlayers++;
                            SetPlayerToMarioOne(mCallerPlayer);
                        }
                        else
                        {
                            Game.ShowPopupMessage("Choose another team");
                        }
                        break;
                    case "MarioTwo":
                        if (mNumberOfRedTeamPlayers >= mNumberOfBluTeamPlayers)
                        {
                            getAway(BlueRnd(), mCallerPlayer);
                            mCallerPlayer.SetTeam(PlayerTeam.Team1);
                            mNumberOfBluTeamPlayers++;
                            SetPlayerToMarioTwo(mCallerPlayer);
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
        public void SubstractPlayerFromThisTeam(PlayerTeam team)
        {
            if (team == PlayerTeam.Team1)
            {
                mNumberOfBluTeamPlayers--;
            }
            if (team == PlayerTeam.Team2)
            {
                mNumberOfRedTeamPlayers--;
            }
        }
        public int RandNumber(int Low, int High)
        {
            return rnd.Next(Low, High);
        }
        public void removeWeapons(IPlayer ply)
        {
            ply.RemoveWeaponItemType(WeaponItemType.Rifle);
            ply.RemoveWeaponItemType(WeaponItemType.Handgun);
            ply.RemoveWeaponItemType(WeaponItemType.Melee);
            ply.RemoveWeaponItemType(WeaponItemType.Thrown);
            ply.SetHealth(100);
        }
        private
        const int USER_RESPAWN_DELAY_MS = 10000;
        private
        const bool GIB_CORPSES = false;
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

        public void Tick(TriggerArgs args)
        {
            RespawnTick(args);
            ConnectedPlayersTick(args);
        }

        public void Death(TriggerArgs args)
        {
            cpl2 = (IPlayer)args.Sender;
            if ((args.Sender != null) && (args.Sender is IPlayer))
            {
                deathNum++;
                if (cpl2.GetTeam() == PlayerTeam.Team1)
                {
                    vRedKills++;
                    RefreshCounter(vRedKills, "RedT");
                }
                if (cpl2.GetTeam() == PlayerTeam.Team2)
                {
                    vBluKills++;
                    RefreshCounter(vBluKills, "BlueT");
                }
                cus = cpl2.GetUser();
                if (cus != null)
                {
                    m_deadPlayers.Add(new DeadPlayer(Game.TotalElapsedGameTime, cus, cpl2, cpl2.GetTeam()));
                }
            }
        }

        
        public void RespawnTick(TriggerArgs args)
        {
            if (m_deadPlayers.Count > 0)
            {
                for (int i = m_deadPlayers.Count - 1; i >= 0; i--)
                {
                    cdpl = m_deadPlayers[i];
                    if (cdpl.Timestamp + USER_RESPAWN_DELAY_MS < Game.TotalElapsedGameTime)
                    {

                        m_deadPlayers.RemoveAt(i);
                        if (cdpl.DeadBody != null)
                        {
                            cdpl.DeadBody.SetWorldPosition(Game.GetSingleObjectByCustomID("ThePit").GetWorldPosition());
                        }
                        cpl2 = cdpl.User.GetPlayer();
                        if (((cpl2 == null) || (cpl2.IsDead)))
                        {
                            SpawnUser(cdpl.User, cdpl.Team);
                        }
                    }
                }
            }
        }
        int usersConnected = 0;
        public void ConnectedPlayersTick(TriggerArgs args)
        {
            if (usersConnectedTickDelay > 0)
            {
                usersConnectedTickDelay--;
                if (usersConnectedTickDelay <= 0)
                {
                    users = Game.GetActiveUsers();
                    if (usersConnected == 0)
                    {
                        usersConnected = users.Length;
                    }
                    else if (users.Length > usersConnected)
                    {
                        for (i = 0; i < users.Length; i++)
                        {
                            cpl2 = users[i].GetPlayer();
                            if ((cpl2 == null)) SpawnUser2(users[i]);
                        }
                    }
                    usersConnected = users.Length;
                    ct = (IObjectText)Game.GetSingleObjectByCustomId("UsersHere");
                    ct.SetText(usersConnected.ToString());
                    string playerList = "";
                    for (i = 0; i < users.Length; i++)
                        playerList += users[i].GetProfile().Name + "\n";
                    if (users.Length < 8)
                        for (int i = users.Length - 1; i < 8; i++)
                            playerList += " \n";
                    ct = (IObjectText)Game.GetSingleObjectByCustomId("UsersList");
                    ct.SetText(playerList);
                    usersConnectedTickDelay = 15;
                }
            }
        }
        private void SpawnUser(IUser user, PlayerTeam team)
        {
            if (CheckUserStillActive(user) && !user.IsSpectator)
            {
                cpl2 = Game.CreatePlayer(cve);
                cpl2.SetProfile(user.GetProfile());
                if (team == PlayerTeam.Team1)
                {
                    cpl2.SetTeam(PlayerTeam.Team1);
                    cve = Game.GetSingleObjectByCustomId(BlueRnd()).GetWorldPosition();

                    if (winner == PlayerTeam.Team1)
                    {
                        cve = Game.GetSingleObjectByCustomID("PlayerPeach").GetWorldPosition();
                    }
                    SetPlayerToMarioTwo(cpl2);
                }
                if (team == PlayerTeam.Team2)
                {
                    cpl2.SetTeam(PlayerTeam.Team2);
                    cve = Game.GetSingleObjectByCustomId(RedRnd()).GetWorldPosition();
                    if (winner == PlayerTeam.Team2)
                    {
                        Game.WriteToConsole("Winner is read and dead red player is sent to peach");
                        cve = Game.GetSingleObjectByCustomID("PlayerPeach").GetWorldPosition();
                    }
                    SetPlayerToMarioOne(cpl2);
                }
                if (team.Equals(PlayerTeam.Team3))
                {
                    cpl2.SetTeam(PlayerTeam.Team3);
                    cve = Game.GetSingleObjectByCustomID("Middle").GetWorldPosition();
                }
                cpl2.SetUser(user);
                cpl2.SetWorldPosition(cve);
            }
            else
            {
                SubstractPlayerFromThisTeam(team);
            }
        }
        private void SpawnUser2(IUser user)
        {
            if (CheckUserStillActive(user) && !user.IsSpectator)
            {
                cve = Game.GetSingleObjectByCustomId("Middle").GetWorldPosition();
                cpl2 = Game.CreatePlayer(cve);
                cpl2.SetUser(user);
                cpl2.SetProfile(user.GetProfile());
                cpl2.SetWorldPosition(cve);
                cpl2.SetTeam(PlayerTeam.Team3);
            }
        }
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
            ct = (IObjectText)Game.GetSingleObjectByCustomId("DeathCounterBl");
            ct2 = (IObjectText)Game.GetSingleObjectByCustomId("DeathCounterRe");
            if (TextF != true)
            {
                if (ct != null && team == "BlueT")
                    ct2.SetText("Blue score: " + value.ToString());
                if (ct2 != null && team == "RedT")
                    ct.SetText("Red score: " + value.ToString());
            }
        }
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
            marios.Add(player);
        }

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
            marios.Add(player);
        }

        public void Shoot(TriggerArgs args)
        {
            foreach (IPlayer ply in marios)
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

        private void Shoot(int interval, int count, string method, string id)
        {
            IObjectTimerTrigger timerTrigger = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
            timerTrigger.SetIntervalTime(interval);
            timerTrigger.SetRepeatCount(count);
            timerTrigger.SetScriptMethod(method);
            timerTrigger.CustomId = id;
            timerTrigger.Trigger();
        }

        private String PrintMinutes(int toConvertSeconds)
        {
            int minutes = toConvertSeconds / 60;
            int seconds = toConvertSeconds % 60;
            String x = minutes.ToString("00") + ":" + seconds.ToString("00");
            return x;
        }

        private void SpawnPeach()
        {
            Vector2 pos = Game.GetSingleObjectByCustomID("PeachCenter").GetWorldPosition();
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
            peachPlayer.SetProfile(peach);
            peachPlayer.SetNametagVisible(false);
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

        public void ExplodePeach()
        {
            IObjectExplosionTrigger explode = (IObjectExplosionTrigger)Game.GetSingleObjectByCustomID("Explode");
            explode.Trigger();
            explode.Trigger();
            explode.Trigger();
            explode.Trigger();
        }
    }
}
