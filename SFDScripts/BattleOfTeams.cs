using SFDGameScriptInterface;
using System;
using System.Collections.Generic;


namespace SFDScripts
{
    class BattleOfTeams : GameScriptInterface
    {

        public BattleOfTeams() : base(null) { }

        List<IPlayer> marios = new List<IPlayer>();
        int mariosNumber = 0;
        int TimeCTD = 5 * 60;
        bool TextF = false;
        int vRedKills = 0;
        int vBluKills = 0;
        IPlayer cpl;
        IPlayer cpl2;
        IUser cus;
        IUser[] users;
        IObjectText ct;
        IObjectText ct2;
        IObject[] coba;
        IObject cob;
        Vector2 cve;
        Vector2 cve2;
        DeadPlayer cdpl;
        Random rnd = new Random();
        int i;
        public void TimeRemain(TriggerArgs args)
        {
            ct = (IObjectText)Game.GetSingleObjectByCustomId("TextCD");
            if (ct != null && TextF == false)
            {
                TimeCTD--;
                ct.SetText("Time Remaining: " + TimeCTD.ToString());
            }
            if (TimeCTD < 1)
            {
                WhoWins(args);
            }
        }
        public void TimeVerdin(TriggerArgs args)
        {
            cpl = (IPlayer)args.Sender;
            cpl.SetTeam(PlayerTeam.Team3);
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
                TextF = true;
                ct.SetText("Finished, space for the next one" + "\n             <-- " + vRedKills + " X " + vBluKills + " -->");
            }
            else if (vRedKills < vBluKills)
            {
                Game.SetGameOver("The Blue wins");
                TextF = true;
                ct.SetText("Finished, space for the next one" + "\n             <-- " + vRedKills + " X " + vBluKills + " -->");
            }
            else
            {
                Game.SetGameOver("Draw!");
                TextF = true;
                ct.SetText("Finished, space for the next one" + "\n             <-- " + vRedKills + " X " + vBluKills + " -->");
            }
        }
        private IObjectTrigger trig2 = null;
        private IObjectTrigger trig3 = null;
        private IObjectTrigger trig4 = null;
        private IPlayer ply = null;
        private IPlayer plyExit = null;
        private bool allowOpen = false;
        int rand;
        string[] GetposB = new string[] {
 "BluBase",
 "BluBase2",
 "MarioTwoBase",
 "BluBase4",
 "BluBase5"
};
        string[] GetposR = new string[] {
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
            cpl = (IPlayer)args.Sender;
            cpl.GiveWeaponItem(WeaponItem.PILLS);
            cpl.GiveWeaponItem(cpl.CurrentSecondaryWeapon.WeaponItem);
            if (cpl.CurrentPrimaryWeapon.WeaponItem != WeaponItem.MAGNUM)
            {
                cpl.GiveWeaponItem(cpl.CurrentPrimaryWeapon.WeaponItem);
            }
        }

        public void getAway(String id, IPlayer ply)
        {
            cob = Game.GetSingleObjectByCustomId(id);
            cve = cob.GetWorldPosition();
            ply.SetWorldPosition(cve);
        }
        private IPlayer Staffplayer = null;
        int bluTeam = 0;
        int redTeam = 0;

        public void MovetoBase(TriggerArgs args)
        {
            cpl = (IPlayer)args.Sender;
            if ((cpl != null) && (cpl is IPlayer) && (!cpl.IsDiving))
            {
                cob = (IObject)args.Caller;
                cpl = (IPlayer)args.Sender;
                switch (cob.CustomId)
                {
                    case "MarioOne":
                        if (Game.GetActiveUsers().Length * 5 > redTeam * 10)
                        {
                            getAway("MarioOneBase", cpl);
                            cpl.SetTeam(PlayerTeam.Team2);
                            redTeam++;
                            SetPlayerToMarioOne(cpl);
                        }
                        else
                        {
                            Game.ShowPopupMessage("Choose another team");
                        }
                        break;
                    case "MarioTwo":
                        if (Game.GetActiveUsers().Length * 5 > bluTeam * 10)
                        {
                            getAway("MarioTwoBase", cpl);
                            cpl.SetTeam(PlayerTeam.Team1);
                            bluTeam++;
                            SetPlayerToMarioTwo(cpl);
                        }
                        else
                        {
                            Game.ShowPopupMessage("Choose another team");
                        }
                        break;
                    case "ReMarioOne":
                        removeWeapons(cpl);
                        if (cpl.GetTeam() == PlayerTeam.Team1)
                        {
                            getAway(BlueRnd(), cpl);
                            SetPlayerToMarioTwo(cpl);
                        }
                        else
                        {
                            getAway(RedRnd(), cpl);
                            SetPlayerToMarioOne(cpl);
                        }
                        break;
                    case "ReMarioTwo":
                        removeWeapons(cpl);
                        if (cpl.GetTeam() == PlayerTeam.Team1)
                        {
                            getAway(BlueRnd(), cpl);
                            SetPlayerToMarioTwo(cpl);
                        }
                        else
                        {
                            getAway(RedRnd(), cpl);
                            SetPlayerToMarioOne(cpl);
                        }
                        break;
                    default:
                        break;
                }
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
        public int RandNumber(int Low, int High)
        {
            rand = rnd.Next(Low, High);
            return rand;
        }
        public void XstroyGun(TriggerArgs args)
        {
            if (args.Sender is IObjectWeaponItem)
            {
                cob = (IObject)args.Sender;
                cob.Remove();
            }
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
        public void Start(TriggerArgs args)
        {
            Shoot(300, 0, "Shoot", "");
            ConnectedPlayersTick(args);
            RefreshCounter(vBluKills, "BlueT");
            RefreshCounter(vRedKills, "RedT");
            crate = ((IObject)Game.GetSingleObjectByCustomId("crate"));
            cpos = crate.GetWorldPosition();
        }
        private List<DeadPlayer> m_deadPlayers = new List<DeadPlayer>();
        private int deathNum = 0;
        private int usersConnectedTickDelay = 15;
        public void Tick(TriggerArgs args)
        {
            Fly(args);
            RespawnTick(args);
            ConnectedPlayersTick(args);
            if (Staffplayer != null)
                Staffplayer.SetHealth(100);
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
                            if (GIB_CORPSES)
                            {
                                cdpl.DeadBody.Gib();
                            }
                            else
                            {
                                (cdpl.DeadBody).Remove();
                            }
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
            if (CheckUserStillActive(user))
            {
                cpl2 = Game.CreatePlayer(cve);
                if (team == PlayerTeam.Team1)
                {
                    cpl2.SetTeam(PlayerTeam.Team1);
                    cve = Game.GetSingleObjectByCustomId("Blue").GetWorldPosition();
                }
                if (team == PlayerTeam.Team2)
                {
                    cpl2.SetTeam(PlayerTeam.Team2);
                    cve = Game.GetSingleObjectByCustomId("Red").GetWorldPosition();
                }
                cpl2.SetUser(user);
                cpl2.SetProfile(user.GetProfile());
                cpl2.SetWorldPosition(cve);
            }
            else
            {
                CheckTeams(team);
            }
        }
        private void SpawnUser2(IUser user)
        {
            if (CheckUserStillActive(user))
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
        int buttonPressCount = 0;
        public void Fly(TriggerArgs args)
        {
            buttonPressCount++;
            cve = Vector2.Zero;
            cve2 = Game.GetSingleObjectByCustomId("fly").GetLinearVelocity();
            if (Staffplayer != null)
            {
                cve = Staffplayer.GetWorldPosition() + new Vector2(0f, -8.15f);
                if (Staffplayer != null && buttonPressCount <= 10 && !(Staffplayer.IsCrouching) && (Staffplayer.IsInMidAir))
                {
                    cve = Staffplayer.GetWorldPosition() + new Vector2(0f, -8.15f);
                    IObject createdObject = Game.CreateObject("InvisiblePlatform", cve, 0f, cve2, 0f);
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
                        IObject createdObject = Game.CreateObject("InvisiblePlatform", cve, 0f, cve2, 0f);
                        createdObject.CustomId = "Flying";
                    }
                }
            }
        }
        public void Delete(TriggerArgs args)
        {
            coba = Game.GetObjectsByCustomId("Flying");
            for (int i = 0; i < coba.Length; i++)
            {
                coba[i].Remove();
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
    }
}