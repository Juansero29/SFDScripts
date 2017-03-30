using SFDGameScriptInterface;
using System;
using System.Collections.Generic;


namespace SFDScripts
{
    class ChaletCode : GameScriptInterface
    {
        public ChaletCode() : base(null) { }

        //=========================================================//
        //===============< REFINED ZOMBIE GAMEMODE >===============//
        //===============<        BY CHELOG        >===============//
        //=========================================================//
        //==============| used The Adventurer's idea |=============//
        //==============|   and small parts of code  |=============//
        //=========================================================//

        Random rnd = new Random();
        int waveCount = 0;
        int botKill = 0;
        int ammoAmount = 0;
        float nextAmmo = 0f;
        Dictionary<IPlayer, float> superZombies = new Dictionary<IPlayer, float>();     //Super zombies health
        Dictionary<IPlayer, int> sZombiesToCheck = new Dictionary<IPlayer, int>();
        Dictionary<IPlayer, int> cashes = new Dictionary<IPlayer, int>(8);              // players' cash
        Dictionary<IPlayer, int> bots = new Dictionary<IPlayer, int>();         // bot's IPlayers
        Dictionary<IPlayer, float> botsUpdate = new Dictionary<IPlayer, float>();       // bot's update timestamps
        Dictionary<IPlayer, float> toInfect = new Dictionary<IPlayer, float>();     // dead players until thy're infected
        Dictionary<IPlayer, IObject> destroyers = new Dictionary<IPlayer, IObject>();   // zombies currently destroying smth
        Dictionary<IObject, int> destroyables = new Dictionary<IObject, int>();     // parts that can be destroyed

        public void OnStartup()
        {
            // create needed triggers to map to function propertly
            CreateTrigs();

            //initialize fields
            waveCount = 0;
            botKill = 0;
            foreach (IUser user in Game.GetActiveUsers()) cashes.Add(user.GetPlayer(), 0);
            foreach (IPlayer user in Game.GetPlayers())
            {
                int chance = rnd.Next(6);
                if (chance == 4)
                    user.GiveWeaponItem(WeaponItem.BAT);
                else if (chance == 5)
                    user.GiveWeaponItem(WeaponItem.PIPE);
                user.SetTeam(PlayerTeam.Team1);
            }
            updateCash();
            nextWave();

            // Initializing the destroyables list
            // HOUSE
            destroyables.Add(Game.GetSingleObjectByCustomId("Destroyable3"), 60);
            destroyables.Add(Game.GetSingleObjectByCustomId("Destroyable4"), 60);
            destroyables.Add(Game.GetSingleObjectByCustomId("Destroyable5"), 3);
            destroyables.Add(Game.GetSingleObjectByCustomId("Destroyable6"), 3);
            destroyables.Add(Game.GetSingleObjectByCustomId("Destroyable7"), 3);
            destroyables.Add(Game.GetSingleObjectByCustomId("Destroyable8"), 400);
            destroyables.Add(Game.GetSingleObjectByCustomId("Destroyable9"), 3);
            destroyables.Add(Game.GetSingleObjectByCustomId("Destroyable10"), 3);
            destroyables.Add(Game.GetSingleObjectByCustomId("Destroyable11"), 3);
            destroyables.Add(Game.GetSingleObjectByCustomId("Destroyable12"), 60);
            // DEFENCES
            // reserved Destroyable13 and Destroyable14 CustomId's
        }

        public void CreateTrigs()
        {
            IObjectTimerTrigger timerTrigger = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
            timerTrigger.SetIntervalTime(500);
            timerTrigger.SetRepeatCount(0);
            timerTrigger.SetScriptMethod("Tick");
            timerTrigger.Trigger();

            IObjectTimerTrigger timerTrigger1 = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
            timerTrigger1.SetIntervalTime(4500);
            timerTrigger1.SetRepeatCount(1);
            timerTrigger1.SetScriptMethod("onWaveStart");
            ((IObject)timerTrigger1).CustomId = "timerTrigger";

            IObjectTimerTrigger timerTrigger2 = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
            timerTrigger2.SetIntervalTime(500);
            timerTrigger2.SetRepeatCount(0);
            timerTrigger2.SetScriptMethod("onSpawnBot");
            timerTrigger2.SetEnabled(false);
            ((IObject)timerTrigger2).CustomId = "botSpawnTrigger";

            IObjectTimerTrigger timerTrigger3 = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
            timerTrigger3.SetIntervalTime(1000);
            timerTrigger3.SetRepeatCount(0);
            timerTrigger3.SetScriptMethod("countDown");
            timerTrigger3.SetEnabled(false);
            ((IObject)timerTrigger3).CustomId = "deathTimer";

            IObjectTrigger scriptTrigger = (IObjectTrigger)Game.CreateObject("ScriptTrigger");
            scriptTrigger.SetScriptMethod("updateTextLabel2"); // Method to call
            ((IObject)scriptTrigger).CustomId = "textScriptTrigger2";

            IObjectTrigger deathTrigger = (IObjectTrigger)Game.CreateObject("OnPlayerDeathTrigger");
            deathTrigger.SetScriptMethod("onPlayerDeath"); // Method to call
            ((IObject)deathTrigger).CustomId = "deathTrigger";
        }

        public void ShowAd(TriggerArgs args)
        {
            PrintText("Survive as long as possible", 30);
        }

        public void ShowAd1(TriggerArgs args)
        {
            PrintText("by #Octothorp team", 30);
        }

        public void HideAd(TriggerArgs args)
        {
            Game.HidePopupMessage();
        }

        // cleanup map and prepare level for next wave
        public void nextWave()
        {
            IObjectTrigger dtimer = getTrigger("deathTimer");
            dtimer.SetEnabled(false);
            countDownTimes = 0;

            // destroy corpses and clear player list to infect
            foreach (IPlayer player in Game.GetPlayers()) if (player.IsDead)
                {
                    player.Gib();
                }
            toInfect.Clear();
            // cleanup map from gibs and grenades
            IObject[] objectsToRemove = Game.GetObjectsByName(new string[]{
            "GIBLET00", "GIBLET01", "GIBLET02", "GIBLET03", "GIBLET04", "WpnGrenadesThrown"});
            for (int i = 0; i < objectsToRemove.Length; i++) objectsToRemove[i].Remove();

            if (waveCount % 3 == 0 && getAliveUsersCount() < Game.GetActiveUsers().Length && waveCount >= 3)
            {
                revivePlayers();
            }

            waveCount++;
            botKill = 0;
            getTrigger("timerTrigger").Trigger();
            updateText("waveTextLabel", "Next wave: " + waveCount);
        }

        // start new wave
        public void onWaveStart(TriggerArgs args)
        {
            spawnBotCount = 0;
            getTrigger("botSpawnTrigger").SetEnabled(true);
            getTrigger("deathTrigger").SetEnabled(true);
            getTrigger("botSpawnTrigger").Trigger();
            getTrigger("textScriptTrigger2").Trigger();

            IObjectTrigger dtimer = getTrigger("deathTimer");
            dtimer.SetEnabled(true);
            dtimer.Trigger();
            updateText("timeTextLabel", "Time left: " + getWaveTimeLimit());
        }

        // -1 second to timer
        int countDownTimes = 0;
        public void countDown(TriggerArgs args)
        {
            countDownTimes++;
            int timeLeft = getWaveTimeLimit() - countDownTimes;
            if (timeLeft >= 0) updateText("timeTextLabel", "Time left: " + timeLeft);
            if (timeLeft == 0)
            {
                Game.PlaySound("wilhelm", Vector2.Zero, 1f);
                PrintText("You have run out of time", 30);
                foreach (IPlayer player in Game.GetPlayers())
                {
                    if (player.GetUser() != null && !player.IsDead)
                    {
                        Game.PlayEffect("GIB", player.GetWorldPosition() + new Vector2(0, 8));
                        IObject[] profs = Game.GetObjectsByCustomId("Profile");
                        IObjectPlayerProfileInfo prof = (IObjectPlayerProfileInfo)profs[rnd.Next(profs.Length)];
                        player.SetProfile(prof.GetProfile());
                        player.SetTeam(PlayerTeam.Team3);
                    }
                }
            }
        }


        int spawnBotCount = 0;
        public void onSpawnBot(TriggerArgs args)
        {
            spawnBotCount++;
            int zombieCountLimit = (4 + (Game.GetActiveUsers().Length - 1) / 2) * (waveCount / 3 + 1);
            if (spawnBotCount <= zombieCountLimit)
            {
                getTrigger("spawnBot" + rnd.Next(1, 3)).Trigger();
            }
            else
            {
                getTrigger("botSpawnTrigger").SetEnabled(false);
            }
        }

        // all possible events that happen when any player dies
        public void onPlayerDeath(TriggerArgs args)
        {
            IPlayer p = (IPlayer)args.Sender;
            if (p.GetTeam() == PlayerTeam.Team3 && !p.IsBot && p.GetUser() != null)
            {   // ZOMBIE PLAYER DIED
                botKill++;
                addCash(30);
                if (botKill >= botsSpawned)
                {
                    botsSpawned = 0;
                    nextWave();
                }
                updateZombieList();
            }
            else if (p.GetUser() == null && p.IsBot)
            {                   // ZOMBIE BOT DIED
                botKill++;
                bots.Remove(p);
                botsUpdate.Remove(p);
                if (destroyers.ContainsKey(p)) destroyers.Remove(p);
                if (superZombies.ContainsKey(p))
                {
                    sZombiesToCheck.Remove(p);
                    superZombies.Remove(p);
                }

                addCash(5);

                // start next wave if no bots are alive
                if (botKill == botsSpawned)
                {
                    botsSpawned = 0;
                    nextWave();
                }
                updateCash();
            }
            else if (p.GetTeam() == PlayerTeam.Team1 && p.GetUser() != null)
            {               // PLAYER DIED
                cashes.Remove(p);
                PrintText(p.GetProfile().Name + " is down", 30);
                toInfect.Add(p, Game.TotalElapsedGameTime + 3 * 1000);
                updateCash();
            }
            else if (p.GetUser() == null && !p.IsBot)
            {                           // PLAYER LEFT GAME
                if (p.GetTeam() == PlayerTeam.Team3)
                {
                    botKill++;
                    if (botKill == botsSpawned)
                    {
                        botsSpawned = 0;
                        nextWave();
                    }
                }
                cashes.Remove(p);
                updateCash();
                PrintText(p.GetProfile().Name + " left us", 30);
            }
        }

        // give everybody some amount of cash
        public void addCash(int amount)
        {
            List<IPlayer> users = getList(cashes.Keys);
            foreach (IPlayer user in users)
            {
                int cash = cashes[user];
                cashes[user] = cash + amount;
            }
        }

        public void revivePlayers()
        {
            PrintText("Fighters are revived!", 30);
            if (getTrigger("revivePlayerTrigger") != null) getTrigger("revivePlayerTrigger").SetEnabled(true);
            getTrigger("reviveTimer").Trigger();
        }

        public void onRevive(TriggerArgs args)
        {
            if (getAliveUsersCount() >= Game.GetActiveUsers().Length || getAliveUsersCount() == 0)
            {
                args.Handled = true;
            }
        }

        public void updateTextLabel2(TriggerArgs args)
        {
            if (waveCount != 1) PrintText("Wave " + waveCount + " started", 25);
            updateText("waveTextLabel", "Current Wave: " + waveCount);
            updateText("nextReviveTextLabel", "Next revive on wave: " + ((((waveCount + 2) / 3) * 3) + 1));
        }

        int tickDelay = 6;      // 6*500ms = 3 seconds update time for GameTick
        int tickDelay2 = 3;     // 3*500ms = 1.5 seconds update time for WeaponTick
        int tickDelay3 = 2;     // 2*500ms = 1 seconds update time for DestroyTick
        public void Tick(TriggerArgs args)
        {
            BotTick();
            InfectTick();

            tickDelay--;
            if (tickDelay < 1)
            {
                GameTick();
                tickDelay = 6;
            }

            tickDelay2--;
            if (tickDelay2 < 1)
            {
                WeaponTick();
                tickDelay2 = 3;
            }

            tickDelay3--;
            if (tickDelay3 < 1)
            {
                DestroyTick();
                SuperZombiesTick();
                tickDelay3 = 2;
            }
        }

        // check game for alive players and set gameover if needed
        public void GameTick()
        {
            //Game.PlayEffect("PWT",Vector2.Zero,"Tick!");
            if (getAliveUsersCount() < 1)
            {
                Game.SetGameOver("You are dead. Not big surprise.\n         You survived " + (waveCount - 1) + " waves.");
                getTrigger("deathTimer").SetEnabled(false);
            }
            refreshCashList();
        }

        // ultimate method to update bots' AI
        public void BotTick()
        {
            Vector2[] plyPos = new Vector2[9];
            int i = 0;
            // a little optimisation moment where players' positions are stored and checked every time from array
            // this makes game lag MUCH less causing game not to ask player list every time for single bot and get its position
            if (getAlivePlayers().Length != 0) foreach (IPlayer ply in getAlivePlayers())
                {
                    plyPos[i] = ply.GetWorldPosition();
                    i++;
                }
            IObject[] grn = Game.GetObjectsByName("WpnGrenadesThrown");
            // add grenades to this list to make zombies follow them
            if (grn.Length > 0) plyPos[8] = grn[0].GetWorldPosition();
            foreach (KeyValuePair<IPlayer, int> pair in bots)
            {
                switch (pair.Value)
                {
                    case 0:
                    case 1:
                    case 2:
                        {
                            bool following = false;
                            Vector2 closest = Vector2.Zero;
                            if (plyPos.Length != 0) closest = plyPos[0];
                            // find closest player to track him
                            if (closest != Vector2.Zero) foreach (Vector2 pos in plyPos)
                                {
                                    if (Math.Abs(pos.Y - pair.Key.GetWorldPosition().Y) < 20f)
                                    {
                                        if (Math.Abs(pos.X - pair.Key.GetWorldPosition().X) < Math.Abs(closest.X - pair.Key.GetWorldPosition().X))
                                            closest = pos;
                                        // if player is reachable, follow him
                                        if (Math.Abs(closest.X - pair.Key.GetWorldPosition().X) < 200f && Math.Abs(closest.Y - pair.Key.GetWorldPosition().Y) < 20f) following = true;
                                    }
                                }

                            // idle behaviour - walk around from time to time (if update time has passed)
                            if (Game.TotalElapsedGameTime > botsUpdate[pair.Key] && !following)
                            {
                                pair.Key.RunToPosition(pair.Key.GetWorldPosition() + new Vector2(rnd.Next(-100, 100), 0), -1 + rnd.Next(2) * 2);
                                // pick a random time of next update for this zombie
                                botsUpdate[pair.Key] = Game.TotalElapsedGameTime + rnd.Next(1, 6) * 1000;
                                // a little "blabla" add-on
                                if (pair.Value == 2)
                                {
                                    int chance = rnd.Next(100);
                                    if (chance == 97) Game.PlayEffect("PWT", pair.Key.GetWorldPosition(), "Braaains");
                                    else if (chance == 98) Game.PlayEffect("PWT", pair.Key.GetWorldPosition(), "Blarrrgh");
                                    else if (chance == 99) Game.PlayEffect("PWT", pair.Key.GetWorldPosition(), "Hrgm bla bla");
                                }
                                // following behaviour - follow closest player and attack when it reach him
                            }
                            else if (following)
                            {
                                // make zombie run to closest player
                                pair.Key.RunToPosition(closest, 1);
                                // attack if bot is close enough
                                if (Math.Abs(closest.X - pair.Key.GetWorldPosition().X) < 24f) pair.Key.SetBotType(BotType.TutorialA);
                                else pair.Key.SetBotType(BotType.None);
                                // a little "blabla" add-on
                                if (pair.Value == 2)
                                {
                                    int chance = rnd.Next(75);
                                    if (chance == 72) Game.PlayEffect("PWT", pair.Key.GetWorldPosition(), "Braaains");
                                    else if (chance == 73) Game.PlayEffect("PWT", pair.Key.GetWorldPosition(), "Blarrrgh");
                                    else if (chance == 74) Game.PlayEffect("PWT", pair.Key.GetWorldPosition(), "Hrgm bla bla");
                                }
                                // idle behaviour - let bot continue his previous task (if update time is upcoming yet)
                            }
                            else pair.Key.SetBotType(BotType.None);
                            break;
                        }
                    default:
                        // display error if something's broken in AI
                        Game.PlayEffect("PWT", pair.Key.GetWorldPosition(), "error here");
                        break;
                }
                // make zombies run away from sides of map avoiding them to make a crowd there
                if (pair.Key.GetWorldPosition().X < -335) pair.Key.RunToPosition(new Vector2(16, -56) + new Vector2(rnd.Next(-60, 60), 0), 1);
                if (pair.Key.GetWorldPosition().X > 335) pair.Key.RunToPosition(new Vector2(16, -56) + new Vector2(rnd.Next(-60, 60), 0), -1);
            }
        }

        // infect players if it's time to
        public void InfectTick()
        {
            List<IPlayer> toInfectDelete = new List<IPlayer>();
            foreach (KeyValuePair<IPlayer, float> pair in toInfect)
            {
                if (pair.Value < Game.TotalElapsedGameTime && pair.Key.GetUser() != null && !pair.Key.IsRemoved)
                {
                    Infect(pair.Key);
                    toInfectDelete.Add(pair.Key);
                }
            }
            foreach (IPlayer ply in toInfectDelete) toInfect.Remove(ply);
        }

        // remove weapons from zombies
        public void WeaponTick()
        {
            IPlayer[] plys = getZombiePlayers();
            for (int i = 0; i < plys.Length; i++)
            {
                IPlayer ply = plys[i];
                if (ply != null)
                {
                    ply.RemoveWeaponItemType(WeaponItemType.Melee);
                    ply.RemoveWeaponItemType(WeaponItemType.Handgun);
                    ply.RemoveWeaponItemType(WeaponItemType.Rifle);
                    ply.RemoveWeaponItemType(WeaponItemType.Thrown);
                }
            }
        }

        // remove "HP" from destroyables and destroy them
        public void DestroyTick()
        {
            List<IPlayer> destroyersDelete = new List<IPlayer>();
            foreach (KeyValuePair<IPlayer, IObject> pair in destroyers)
            {
                if (destroyables.ContainsKey(pair.Value))
                {
                    destroyables[pair.Value]--;
                    Game.PlayEffect("W_P", pair.Value.GetWorldPosition() + new Vector2(rnd.Next(-8, 8), rnd.Next(-8, 8)));
                    Game.PlaySound("BulletHitWood", pair.Value.GetWorldPosition(), 1f);
                    if (destroyables[pair.Value] <= 0)
                    {
                        pair.Value.Destroy();
                        Game.PlayEffect("CAM_S", Vector2.Zero, 0.8f, 500f, true);
                        destroyables.Remove(pair.Value);
                        destroyersDelete.Add(pair.Key);
                    }
                }
            }
            foreach (IPlayer ply in destroyersDelete) if (destroyers.ContainsKey(ply)) destroyers.Remove(ply);
        }

        // start destroying the object
        public void BeginDestroy(TriggerArgs args)
        {
            IObjectTrigger trig = (IObjectTrigger)args.Caller;
            if (args.Sender != null)
            {
                IPlayer ply = (IPlayer)args.Sender;
                string num = trig.CustomId.Substring(9, trig.CustomId.Length - 9);
                IObject obj = Game.GetSingleObjectByCustomId("Destroyable" + num);
                if (ply.IsBot && !ply.IsDead && !destroyers.ContainsKey(ply)) destroyers.Add(ply, obj);
            }
        }

        // stop destroying the object
        public void EndDestroy(TriggerArgs args)
        {
            if (args.Sender != null)
            {
                IPlayer ply = (IPlayer)args.Sender;
                if (destroyers.ContainsKey(ply)) destroyers.Remove(ply);
            }
        }



        //===========//
        // SHORTCUTS //
        //===========//
        // I think everything's clear here

        // some king of "printing" text
        string text = "";
        public void PrintText(string textIn, int charsPerSecond)
        {
            // if text is currently printng, cleanup previous triggers
            if (Game.GetSingleObjectByCustomId("AddLetter") != null)
            {
                Game.GetSingleObjectByCustomId("AddLetter").Remove();
                letter = 0;
            }
            if (Game.GetSingleObjectByCustomId("HideAd") != null)
                Game.GetSingleObjectByCustomId("HideAd").Remove();

            // create new trigger
            IObjectTimerTrigger tick = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
            tick.SetIntervalTime(1000 / charsPerSecond);
            tick.SetRepeatCount(0);
            text = textIn;
            tick.SetScriptMethod("AddLetter");
            tick.Trigger();
            ((IObject)tick).CustomId = "AddLetter";
        }

        int letter = 0;
        public void AddLetter(TriggerArgs args)
        {
            if (letter < text.Length)
            {
                Game.ShowPopupMessage(text.Substring(0, letter + 1), new Color(64, 128, 128));
                Game.PlaySound("OutOfAmmoLight", Vector2.Zero, 1f);
                letter++;
            }
            else
            {
                Game.GetSingleObjectByCustomId("AddLetter").Remove();
                IObjectTimerTrigger hideAd = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
                hideAd.SetIntervalTime(4000);
                hideAd.SetRepeatCount(1);
                hideAd.SetScriptMethod("HideAd");
                hideAd.Trigger();
                ((IObject)hideAd).CustomId = "HideAd";
                letter = 0;
            }
        }

        public int getAliveUsersCount()
        {
            int count = 0;
            foreach (IPlayer player in Game.GetPlayers())
            {
                if (player.GetUser() != null && !player.IsDead && player.GetTeam() == PlayerTeam.Team1)
                {
                    //Game.PlayEffect("PWT",player.GetWorldPosition(),"I am just a player!");
                    count++;
                }
            }
            return count;
        }

        public IPlayer[] getAlivePlayers()
        {
            IPlayer[] plys = new IPlayer[getAliveUsersCount()];
            int count = 0;
            foreach (IPlayer player in Game.GetPlayers())
            {
                if (player.GetUser() != null && !player.IsDead && player.GetTeam() == PlayerTeam.Team1)
                {
                    plys[count] = player;
                    count++;
                }
            }
            return plys;
        }

        public int getZombiePlayersCount()
        {
            int count = 0;
            foreach (IPlayer player in Game.GetPlayers())
            {
                if (player.GetUser() != null && !player.IsDead && player.GetTeam() == PlayerTeam.Team3)
                {
                    //Game.PlayEffect("PWT",player.GetWorldPosition(),"I am a zombie player!");
                    count++;
                }
            }
            return count;
        }

        public IPlayer[] getZombiePlayers()
        {
            IPlayer[] plys = new IPlayer[getZombiePlayersCount()];
            int count = 0;
            foreach (IPlayer player in Game.GetPlayers())
            {
                if (player.GetUser() != null && !player.IsDead && player.GetTeam() == PlayerTeam.Team3)
                {
                    plys[count] = player;
                    count++;
                }
            }
            return plys;
        }

        public IUser getDeadUser()
        {
            foreach (IUser user in Game.GetActiveUsers())
            {
                IUser userFound = user;
                foreach (IPlayer player in Game.GetPlayers())
                {
                    if (!player.IsBot && !player.IsDead)
                    {
                        if (user.Name.Equals(player.GetUser().Name))
                        {
                            userFound = null;
                            break;
                        }
                    }
                }
                if (userFound != null)
                {
                    return user;
                }
            }
            return null;
        }

        public int getWaveTimeLimit()
        {
            return (waveCount / 3 + 3) * 18;
        }

        public void refreshCashList()
        {
            List<IPlayer> cashesToRemove = new List<IPlayer>();
            foreach (KeyValuePair<IPlayer, int> pair in cashes)
            {
                if (pair.Key.IsRemoved || pair.Key.IsDead || pair.Key == null) cashesToRemove.Add(pair.Key);
            }
            foreach (IPlayer ply in cashesToRemove) cashes.Remove(ply);
            cashesToRemove.Clear();
            updateCash();
        }

        public void updateCash()
        {
            int updateCashCount = 1;
            foreach (KeyValuePair<IPlayer, int> pair in cashes)
            {
                updateText("playerCashLabel" + updateCashCount, pair.Key.GetProfile().Name + ": " + pair.Value);
                updateCashCount++;
            }
            while (updateCashCount <= 8)
            {
                updateText("playerCashLabel" + updateCashCount, "");
                updateCashCount++;
            }
        }

        public List<T> getList<T>(ICollection<T> col)
        {
            return new List<T>(col);
        }

        public void updateText(String label, String text)
        {
            ((IObjectText)Game.GetSingleObjectByCustomId(label)).SetText(text);
        }

        public IObjectTrigger getTrigger(String customID)
        {
            return (IObjectTrigger)Game.GetSingleObjectByCustomId(customID);
        }

        public void updateZombieList()
        {
            string playerList = "";
            IPlayer[] plys = getZombiePlayers();
            for (int i = 0; i < plys.Length; i++)
                playerList += plys[i].GetUser().Name + "\n";
            if (plys.Length < 8) for (int i = plys.Length - 1; i < 8; i++)
                    playerList += " \n";
            updateText("Zomlist", playerList);
        }

        // SPAWN METHODS

        int botsSpawned = 0;
        public void spawnBot(CreateTriggerArgs args)
        {
            botsSpawned++;
            IPlayer bot = (IPlayer)(args.CreatedObject);
            int type = rnd.Next(0, 3);
            bots.Add(bot, type);
            botsUpdate.Add(bot, Game.TotalElapsedGameTime + rnd.Next(2, 5) * 1000);
            if (type == 2) bot.SetBotType(BotType.TutorialA);
            else bot.SetBotType(BotType.None);
            bot.SetTeam(PlayerTeam.Team3);
            int chance = rnd.Next(100);
            if (chance < waveCount)
            {
                bot.SetHealth(100f);
                IObject[] profsSuper = Game.GetObjectsByCustomId("ProfileSuper");
                bot.SetProfile(((IObjectPlayerProfileInfo)profsSuper[rnd.Next(profsSuper.Length)]).GetProfile());
                superZombies.Add(bot, 100f);
                sZombiesToCheck.Add(bot, 0);
            }
            else
            {
                bot.SetHealth(22f);
                IObject[] profs = Game.GetObjectsByCustomId("Profile");
                bot.SetProfile(((IObjectPlayerProfileInfo)profs[rnd.Next(profs.Length)]).GetProfile());
            }
        }

        public void spawnPlayer(CreateTriggerArgs args)
        {
            IUser user = getDeadUser();
            IPlayer player = (IPlayer)(args.CreatedObject);
            if (user == null)
            {
                ((IObjectTrigger)args.Caller).SetEnabled(false);
                player.Remove();
                return;
            }
            player.SetUser(user);
            player.SetProfile(user.GetProfile());
            player.SetTeam(PlayerTeam.Team1);
            int chance = rnd.Next(6);
            if (chance == 4)
                player.GiveWeaponItem(WeaponItem.BAT);
            else if (chance == 5)
                player.GiveWeaponItem(WeaponItem.PIPE);
            cashes.Add(player, 0);
            updateCash();
        }

        public void Infect(IPlayer ply)
        {
            if (ply != null)
            {
                IUser user = ply.GetUser();
                if (user.GetPlayer().IsDead)
                {
                    botsSpawned++;
                    IPlayer zombie = Game.CreatePlayer(ply.GetWorldPosition());
                    zombie.SetUser(user);
                    IObject[] profs = Game.GetObjectsByCustomId("Profile");
                    IObjectPlayerProfileInfo prof = (IObjectPlayerProfileInfo)profs[rnd.Next(profs.Length)];
                    zombie.SetProfile(prof.GetProfile());
                    zombie.SetTeam(PlayerTeam.Team3);
                    zombie.SetHealth(22f);
                    Game.PlayEffect("GIB", zombie.GetWorldPosition() + new Vector2(0, 8));
                    ply.Remove();
                    updateZombieList();
                }
            }
        }

        // BUTTON METHODS - shop

        public void BuyWeapon(TriggerArgs args)
        {
            IObject but = (IObject)args.Caller;
            IPlayer player = (IPlayer)args.Sender;
            int cash = 0;
            if (player.GetTeam() == PlayerTeam.Team1) cash = cashes[player];

            int cost = 0;
            WeaponItem weap = WeaponItem.NONE;

            switch (but.CustomId)
            {
                case "Carbine":
                    cost = 50;
                    weap = WeaponItem.CARBINE;
                    break;
                case "Magnum":
                    cost = 100;
                    weap = WeaponItem.MAGNUM;
                    break;
                case "Shotgun":
                    cost = 150;
                    weap = WeaponItem.SHOTGUN;
                    break;
                case "SubMachinegun":
                    cost = 250;
                    weap = WeaponItem.SUB_MACHINEGUN;
                    break;
                case "M60":
                    cost = 250;
                    weap = WeaponItem.M60;
                    break;
                case "Grenades":
                    cost = 400;
                    weap = WeaponItem.GRENADES;
                    break;
                case "Ammo":
                    if (player.CurrentPrimaryWeapon.WeaponItem == WeaponItem.M60 ||
                    player.CurrentPrimaryWeapon.WeaponItem == WeaponItem.GRENADE_LAUNCHER)
                        Game.PlayEffect("PWT", player.GetWorldPosition() + new Vector2(0, 4), "No ammo for this weapon");
                    else if ((player.CurrentMeleeWeapon.WeaponItem != WeaponItem.NONE) ||
                    (player.CurrentPrimaryWeapon.WeaponItem != WeaponItem.NONE) ||
                    (player.CurrentSecondaryWeapon.WeaponItem != WeaponItem.NONE) ||
                    (player.CurrentThrownItem.WeaponItem != WeaponItem.NONE) ||
                    (player.CurrentPowerupItem.WeaponItem != WeaponItem.NONE))
                        cost = 35;
                    else Game.PlayEffect("PWT", player.GetWorldPosition() + new Vector2(0, 4), "No weapon");
                    break;
                case "Health":
                    if (player.GetHealth() != 100f) cost = 5;
                    break;
                default:
                    Game.ShowPopupMessage("Error here. Lol. (buttons)");
                    break;
            }

            if (cash >= cost && player.GetTeam() == PlayerTeam.Team1)
            {
                if (but.CustomId != "Health")
                    if ((player.CurrentMeleeWeapon.WeaponItem != weap) &&
                        (player.CurrentPrimaryWeapon.WeaponItem != weap) &&
                        (player.CurrentSecondaryWeapon.WeaponItem != weap) &&
                        (player.CurrentThrownItem.WeaponItem != weap) &&
                        (player.CurrentPowerupItem.WeaponItem != weap))
                    {
                        player.GiveWeaponItem(weap);
                        Game.PlaySound("ElevatorDing", player.GetWorldPosition(), 1f);
                    }
                    else if (but.CustomId != "Ammo" && but.CustomId != "Health")
                    {
                        Game.PlayEffect("PWT", player.GetWorldPosition() + new Vector2(0, 4), "You already own one");
                        cost = 0;
                    }
                if (but.CustomId == "Ammo" && player.CurrentPrimaryWeapon.WeaponItem != WeaponItem.M60)
                {
                    player.GiveWeaponItem(player.CurrentPrimaryWeapon.WeaponItem);
                    player.GiveWeaponItem(player.CurrentSecondaryWeapon.WeaponItem);
                }
                if (but.CustomId != "Health")
                    Game.PlayEffect("PWT", player.GetWorldPosition(), "-" + cost);
                else if (but.CustomId == "Health" && player.GetHealth() != 100f)
                {
                    Game.PlaySound("ItemSpawn", player.GetWorldPosition(), 1f);
                    Game.PlayEffect("PWT", player.GetWorldPosition(), "+10 hp");
                    Game.PlayEffect("TR_B", player.GetWorldPosition() + new Vector2(0, 8));
                    player.SetHealth(player.GetHealth() + 10f);
                }
                cashes[player] = cash - cost;
                updateCash();
            }
            else
            {
                Game.PlaySound("MenuCancel", but.GetWorldPosition(), 1f);
                Game.PlayEffect("PWT", player.GetWorldPosition(), "No money");
            }

        }

        public void BuyDefence(TriggerArgs args)
        {
            IObject but = (IObject)args.Caller;
            IPlayer player = (IPlayer)args.Sender;
            int cash = 0;
            if (player.GetTeam() == PlayerTeam.Team1) cash = cashes[player];

            int cost = 0;
            string type = "";
            string side = "";

            switch (but.CustomId)
            {
                case "WoodenLeft":
                    cost = 200;
                    type = "wooden";
                    side = "left";
                    break;
                case "WoodenRight":
                    cost = 200;
                    type = "wooden";
                    side = "right";
                    break;
                case "MetalLeft":
                    cost = 450;
                    type = "metal";
                    side = "left";
                    break;
                case "MetalRight":
                    cost = 450;
                    type = "metal";
                    side = "right";
                    break;
                default:
                    Game.ShowPopupMessage("Error here. Lol. (buttons)");
                    break;
            }

            if (cash >= cost && player.GetTeam() == PlayerTeam.Team1)
            {
                IObject[] def = Game.GetObjectsByCustomId(type + "_" + side);
                IObject defOld = null;
                if (side == "left") defOld = Game.GetSingleObjectByCustomId("Destroyable13");
                else if (side == "right") defOld = Game.GetSingleObjectByCustomId("Destroyable14");
                if (def.Length > 0)
                {
                    if (defOld != null) defOld.Destroy();
                    def[0].SetWorldPosition(Game.GetSingleObjectByCustomId("defplace_" + side).GetWorldPosition(), true);
                    if (side == "left")
                    {
                        IObject toRemove = Game.GetSingleObjectByCustomId("Destroyable13");
                        if (toRemove != null) destroyables.Remove(toRemove);
                        def[0].CustomId = "Destroyable13";
                    }
                    else if (side == "right")
                    {
                        IObject toRemove = Game.GetSingleObjectByCustomId("Destroyable14");
                        if (toRemove != null) destroyables.Remove(toRemove);
                        def[0].CustomId = "Destroyable14";
                    }
                    if (type == "wooden") destroyables.Add(def[0], 100);
                    else if (type == "metal") destroyables.Add(def[0], 250);
                    cashes[player] = cash - cost;
                    updateCash();
                    Game.PlaySound("ElevatorDing", player.GetWorldPosition(), 1f);
                    Game.PlayEffect("PWT", player.GetWorldPosition() + new Vector2(0, 4), type + " defence");
                    Game.PlayEffect("PWT", player.GetWorldPosition(), "-" + cost);
                    PrintText(player.Name + " has built " + type + " barricade on the " + side, 30);
                }
                else
                {
                    Game.PlaySound("MenuCancel", but.GetWorldPosition(), 1f);
                    Game.PlayEffect("PWT", player.GetWorldPosition(), "No resources");
                }
            }
            else
            {
                Game.PlaySound("MenuCancel", but.GetWorldPosition(), 1f);
                Game.PlayEffect("PWT", player.GetWorldPosition(), "No money");
            }

        }

        public void SuperZombiesTick()
        {
            Dictionary<IPlayer, int> stableLife = new Dictionary<IPlayer, int>();
            foreach (KeyValuePair<IPlayer, int> pair in sZombiesToCheck)
            {
                stableLife.Add(pair.Key, pair.Value);
                if (superZombies[pair.Key] <= pair.Key.GetHealth())
                {
                    superZombies[pair.Key] = pair.Key.GetHealth();
                    if (stableLife[pair.Key] == 10)
                    {
                        if (pair.Key.GetHealth() != 100)
                        {
                            pair.Key.SetHealth(pair.Key.GetHealth() + 3);
                            Game.PlayEffect("PWT", pair.Key.GetWorldPosition(), "+");
                        }
                    }
                    else stableLife[pair.Key]++;
                }
                else
                {
                    stableLife[pair.Key] = 0;
                    superZombies[pair.Key] = pair.Key.GetHealth();
                }
            }
            foreach (KeyValuePair<IPlayer, int> pair in stableLife)
            {
                if (pair.Key != null)
                {
                    sZombiesToCheck[pair.Key] = pair.Value;
                }
            }
        }
    }
}
