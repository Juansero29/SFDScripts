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

        #region Lifecycle
        /// <summary>
        /// The game will always call the following method "public void OnStartup()" during a map start (or script activates). 
        /// </summary>
        /// <remarks>
        /// No triggers required. This is run before triggers that activate on startup (and before OnStartup triggers).
        /// </remarks>
        public void OnStartup()
        {
            Game.StartupSequenceEnabled = false;
            Game.DeathSequenceEnabled = false;
            GlobalGame = Game;

            // so all player's see the complete menu
            GlobalGame.SetCurrentCameraMode(CameraMode.Static);

            var menuCameraLedgeGrab = GlobalGame.GetSingleObjectByCustomId("MenuCameraPosition");
            var menuCameraPosition = menuCameraLedgeGrab.GetWorldPosition();
            CameraPosition.X = menuCameraPosition.X;
            CameraPosition.Y = menuCameraPosition.Y;
            CameraSpeed = 20000.0f;

            UpdateCamera();
            SpawnPlayers();

            BeginTimer = (IObjectText)Game.GetSingleObjectByCustomId("BeginTimer");

            #region Vision Objects

            VisionObjects.Add("Concrete01A", 3);
            VisionObjects.Add("Concrete01B", 3);
            VisionObjects.Add("Concrete00A", 3);
            VisionObjects.Add("Concrete00B", 3);
            VisionObjects.Add("Concrete00C", 3);
            VisionObjects.Add("Concrete01C", 3);
            VisionObjects.Add("Concrete02B", 3);
            VisionObjects.Add("Concrete02C", 3);
            VisionObjects.Add("Concrete02K", 3);
            VisionObjects.Add("Stone00C", 3);
            VisionObjects.Add("Concrete02H", 3);
            VisionObjects.Add("Dirt01A", 3);
            VisionObjects.Add("Dirt01C", 3);
            VisionObjects.Add("Dirt01D", 3);
            VisionObjects.Add("Metal06A", 3);
            VisionObjects.Add("Metal06B", 3);
            VisionObjects.Add("Metal06C", 3);
            VisionObjects.Add("Metal02A", 3);
            VisionObjects.Add("Metal02B", 3);
            VisionObjects.Add("Metal02C", 3);
            VisionObjects.Add("HangingCrate00", 3);
            VisionObjects.Add("CargoContainer00A", 3);

            VisionObjects.Add("Crate00", 1);
            VisionObjects.Add("Barrel00", 2);
            VisionObjects.Add("BarrelExplosive", 1);
            VisionObjects.Add("CashRegister00", 1);
            VisionObjects.Add("SwivelChair02", 1);
            VisionObjects.Add("Desk00", 2);
            VisionObjects.Add("FileCab00", 2);
            VisionObjects.Add("Chair00", 1);
            VisionObjects.Add("MetalTable00", 1);
            VisionObjects.Add("Safe00", 2);

            #endregion

            #region Weapon Item Names

            WeaponItemNames.Add(WeaponItem.PISTOL, "WpnPistol");
            WeaponItemNames.Add(WeaponItem.SILENCEDPISTOL, "WpnSilencedPistol");
            WeaponItemNames.Add(WeaponItem.MAGNUM, "WpnMagnum");
            WeaponItemNames.Add(WeaponItem.REVOLVER, "WpnRevolver");
            WeaponItemNames.Add(WeaponItem.SHOTGUN, "WpnPumpShotgun");
            WeaponItemNames.Add(WeaponItem.TOMMYGUN, "WpnTommygun");
            WeaponItemNames.Add(WeaponItem.SMG, "WpnSMG");
            WeaponItemNames.Add(WeaponItem.M60, "WpnM60");
            WeaponItemNames.Add(WeaponItem.SAWED_OFF, "WpnSawedOff");
            WeaponItemNames.Add(WeaponItem.UZI, "WpnUzi");
            WeaponItemNames.Add(WeaponItem.SILENCEDUZI, "WpnSilencedUzi");
            WeaponItemNames.Add(WeaponItem.BAZOOKA, "WpnBazooka");
            WeaponItemNames.Add(WeaponItem.ASSAULT, "WpnAssaultRifle");
            WeaponItemNames.Add(WeaponItem.SNIPER, "WpnSniperRifle");
            WeaponItemNames.Add(WeaponItem.CARBINE, "WpnCarbine");
            WeaponItemNames.Add(WeaponItem.FLAMETHROWER, "WpnFlamethrower");
            WeaponItemNames.Add(WeaponItem.FLAREGUN, "WpnFlareGun");
            WeaponItemNames.Add(WeaponItem.GRENADE_LAUNCHER, "WpnGrenadeLauncher");
            WeaponItemNames.Add(WeaponItem.GRENADES, "WpnGrenades");
            WeaponItemNames.Add(WeaponItem.MOLOTOVS, "WpnMolotovs");
            WeaponItemNames.Add(WeaponItem.MINES, "WpnMines");
            WeaponItemNames.Add(WeaponItem.SHURIKEN, "WpnShuriken");
            WeaponItemNames.Add(WeaponItem.BOW, "WpnBow");
            WeaponItemNames.Add(WeaponItem.SHOCK_BATON, "WpnShockBaton");

            #endregion

            #region Slots
            TEquipmentSlot meleeWeaponSlot = AddEquipmentSlot("Melee Weapon");
            TEquipmentSlot secondaryWeaponSlot = AddEquipmentSlot("Secondary Weapon");
            TEquipmentSlot primaryWeaponSlot = AddEquipmentSlot("Primary Weapon");
            TEquipmentSlot thrownWeaponSlot = AddEquipmentSlot("Thrown Weapon");
            TEquipmentSlot weaponModSlot = AddEquipmentSlot("Weapon Mod");
            TEquipmentSlot bodySlot = AddEquipmentSlot("Body");
            TEquipmentSlot equipmentSlot = AddEquipmentSlot("Equipment");
            #endregion

            #region Adding Weapons and Equipment
            meleeWeaponSlot.AddEquipment(8, 0, 0, "Machete", "A wonderful mexican machete."); //1
            meleeWeaponSlot.AddEquipment(49, 25, 3, "Knife", "A knife that can be thrown.");
            meleeWeaponSlot.AddEquipment(4, 0, 0, "Pipe", "A solid metal pipe."); //2
            meleeWeaponSlot.AddEquipment(11, 0, 0, "Baseball Bat", "A wooden baseball bat."); //3
            meleeWeaponSlot.AddEquipment(31, 0, 0, "Hammer", "A big hammer, don't use it to repair stuff."); //4
            meleeWeaponSlot.AddEquipment(18, 25, 3, "Axe", "Big damage. Good way to end the fight. Can be thrown."); //5
            meleeWeaponSlot.AddEquipment(41, 0, 0, "Baton", "Used in a lot of riots. Feel the unrest in your hands."); //6
            meleeWeaponSlot.AddEquipment(3, 50, 5, "Katana", "Huge damage asian katana. Can be thrown."); //7
            meleeWeaponSlot.AddEquipment(57, 50, 7, "Shock Baton", "Police officers love these."); //8

            secondaryWeaponSlot.AddEquipment(24, 50, 0, "Pistol", "A normal 9mm pistol. 12 bullets mag size and 1 extra mag."); //1
            secondaryWeaponSlot.AddEquipment(39, 50, 0, "Silenced Pistol", "A silenced 9mm pistol. 12 bullets mag size and 1 extra mag."); //2
            secondaryWeaponSlot.AddEquipment(12, 75, 1, "Uzi", "Israeli Uzi pistol, 19mm bullets. 25 bullets mag size and 1 extra mag."); //3
            secondaryWeaponSlot.AddEquipment(40, 75, 1, "Silenced Uzi", "Silenced Israeli Uzi pistol, 19mm bullets. 25 bullets mag size and 1 extra mag."); //4
            secondaryWeaponSlot.AddEquipment(27, 50, 6, "Flare Gun", "A fire shooting flare gun. 1 bullet mag size and 2 extra mags. "); //5
            secondaryWeaponSlot.AddEquipment(28, 200, 9, "Revolver", "That ol' cowboy revolver. 6 bullets mag size and 1 extra mag."); //6
            secondaryWeaponSlot.AddEquipment(1, 225, 14, "Magnum", "Like a revolver but with higher damage and slower recharge. 6 bullets mag size and 1 extra mag."); //7

            primaryWeaponSlot.AddEquipment(5, 100, 2, "Tommy Gun", "Having a rather high spread when compared to other automatic rifles, it should be used as a primary counterpart of the Uzi. 35 bullets mag size with 1 extra mag."); //1
            primaryWeaponSlot.AddEquipment(10, 100, 0, "Sawed-Off Shotgun", "This shotgun shoots extremely fast, dealing tremendous damage at close range if all the bullets hit the target. The small clip compensates this, so it is best used when cover is available to reload. 2 shells with 6 extra shells"); //2
            primaryWeaponSlot.AddEquipment(30, 100, 0, "SMG", "The Submachine Gun has a high rate of fire and good accuracy, however, it does little damage, and is less accurate than the Assault Rifle. 30 bullets mag with 1 extra mag."); //3
            primaryWeaponSlot.AddEquipment(23, 100, 2, "Carbine", "Very accurate weapon with good damage, but low fire rate. 12 bullets mag size and 1 extra mag"); //4
            primaryWeaponSlot.AddEquipment(19, 125, 5, "Assault Rifle", "Good damage and accuracy. Medium fire rate. 24 bullets mag and 1 extra mag."); //5
            primaryWeaponSlot.AddEquipment(2, 125, 4, "Shotgun", "Great close and medium range. Extremely high damage. Slow reload. 6 shells with  6 extra shells.");   //6
            primaryWeaponSlot.AddEquipment(26, 125, 8, "Flamethrower", "LET IT BURN!!!! 50 mag size with 0 extra mags."); //7
            primaryWeaponSlot.AddEquipment(6, 150, 15, "M60", "Big damage and fire rate, but low accuracy. Too heavy, you can't sprint."); //9
            primaryWeaponSlot.AddEquipment(9, 200, 13, "Sniper Rifle", "Best accurate weapon with huge damage. Too heavy, you can't sprint."); //10
            primaryWeaponSlot.AddEquipment((int)WeaponItem.BOW, 200, 20, "Bow", "How 'bout this, with some fire arrows? huh?"); //11
            primaryWeaponSlot.AddEquipment(17, 250, 12, "Bazooka", "The Bazooka fires a rocket off in a straight line, however the trajectory is not constantly straight, as it has the tendency to shift which can help avoid excessively long ranged kills. The random directional shift can partially be prevented with a Laser Sight attached to the Bazooka. 1 rocket mag and two extra mags."); //11
            primaryWeaponSlot.AddEquipment(29, 300, 13, "Grenade Launcher", "When you account for the arc of the grenade, it can become much more accurate than a bazooka (without a laser sight). The grenade launcher also must reload after each shot fired. 1 grenade mag and two extra mags"); //8


            thrownWeaponSlot.AddEquipment(1, 50, 6, "Grenades", "Everyone loves grenades, make them explode. These grenades can't touch people though. 3 grenades"); //1	
            thrownWeaponSlot.AddEquipment(2, 25, 5, "Molotovs", "These little russian bottles we love. 3 in here for your pleasure."); //2
            thrownWeaponSlot.AddEquipment(3, 50, 7, "Mines", "Mines have a priming 'timer' in which while it flashes it will not detonate. 3 mines."); //3
            thrownWeaponSlot.AddEquipment(4, 100, 10, "Incendiary grenades"); //4
            thrownWeaponSlot.AddEquipment(5, 25, 23, "Smoke grenades", ""); //5
            thrownWeaponSlot.AddEquipment(6, 50, 24, "Flashbang", ""); //6
            thrownWeaponSlot.AddEquipment(7, 50, 3, "Shuriken"); //7

            weaponModSlot.AddEquipment(1, 25, 3, "Lazer Scope", "Helps to aim precisely."); //1
            weaponModSlot.AddEquipment(2, 25, 4, "Extra Ammo", "Add extra ammo to your light weapon."); //2
            weaponModSlot.AddEquipment(3, 50, 8, "Extra Explosives", "Add extra ammo to your explosive weapon."); //3
            weaponModSlot.AddEquipment(4, 50, 10, "Extra Heavy Ammo", "Add extra ammo to your heavy weapon: Revolvers, Snipers, Magnums and M60s"); //4
            weaponModSlot.AddEquipment(5, 25, 4, "DNA Scanner", "If the enemy tries to shoot from your gun, it will explode!"); //4
            weaponModSlot.AddEquipment(6, 150, 17, "Bouncing ammo", "Bounce some ammo around"); //5
            weaponModSlot.AddEquipment(7, 175, 25, "Fire ammo", "Fire? Who asked for it anyway!"); //5

            //equipment
            equipmentSlot.AddEquipment(1, 25, 1, "Small Medkit", "Allows one time stop the bleeding or revive teammate."); //1
            equipmentSlot.AddEquipment(2, 50, 3, "Big Medkit", "Allows 3 times stop the bleeding or revive teammate."); //2
            equipmentSlot.AddEquipment(3, 25, 1, "Airdrop", "Drops one supply crate with random weapon."); //3
            equipmentSlot.AddEquipment(4, 100, 10, "Napalm Strike", "Strike of Napalm bombs on the whole map."); //4
            equipmentSlot.AddEquipment(5, 100, 11, "Pinpoint Strike", "The missile tries to hit your enemy."); //5
            equipmentSlot.AddEquipment(6, 125, 14, "Airstrike", "Attack aircraft tries to hit your enemy."); //6
            equipmentSlot.AddEquipment(7, 50, 7, "Big Airdrop", "Drops three supply crates with random weapon."); //7
            equipmentSlot.AddEquipment(8, 125, 13, "Artillery Strike", "150mm cannons bombards all the map."); //8
            equipmentSlot.AddEquipment(9, 50, 8, "Mine Strike", "Mines are falling from the air all over the map."); //9
            equipmentSlot.AddEquipment(10, 250, 15, "Reinforcement", "Revives all your dead teammates and drops them by parachute."); //10
            equipmentSlot.AddEquipment(11, 25, 9, "Supply Jammer", "Your enemies can't call supply while jammer is working. Jammer works for 10 seconds."); //11
            equipmentSlot.AddEquipment(12, 75, 11, "Supply Hacking", "Try to hack enemy supply. Who knows what will happen?"); //12
            equipmentSlot.AddEquipment(13, 150, 11, "Light Turret", "Automatically shoots at enemies in range of the minigun");
            equipmentSlot.AddEquipment(14, 175, 14, "Rocket Turret", "Automatically shoots at enemies in range of the rocket launcher");
            equipmentSlot.AddEquipment(15, 200, 15, "Heavy Turret", "Automatically shoots at enemies in range. It have minigun and rocket launcher.");
            equipmentSlot.AddEquipment(16, 275, 21, "Sniper Turret", "Automatically shoots at enemies in range of the sniper.");
            equipmentSlot.AddEquipment(17, 50, 4, "Police Shield", "Protects you from some bullets.");
            equipmentSlot.AddEquipment(18, 100, 3, "Adrenaline", "Gives temporary immunity to damage. You will receive all damage when adrenaline is over.");
            // equipmentSlot.AddEquipment(19, 200, 15, "Shield Generator", "Creates an energy shield that protects from bullets and enemies.", 2);
            equipmentSlot.AddEquipment(20, 100, 15, "Jet Pack", "Allows you to make jet jumps. And protect from falling.");
            equipmentSlot.AddEquipment(21, 250, 17, "Streetsweeper", "Hate drones? Well try this shit."); // 21
            equipmentSlot.AddEquipment(22, 475, 22, "Melee Drone", "You want a robot that actually kicks ass? Try this baby."); // 22
            equipmentSlot.AddEquipment(23, 500, 26, "Zap Drone", "I know, streetsweepers are dumb. This one is also kind of dumb, but it has a tazer."); // 23
            equipmentSlot.AddEquipment(24, 550, 27, "Fire Drone", "Now try this. A drone that actually moves, with a flame thrower."); // 24
            equipmentSlot.AddEquipment(25, 600, 29, "Assault Drone", "Now try this. A drone that actually moves, with a machine gun."); // 25

            //armor
            bodySlot.AddEquipment(1, 50, 2, "Light Armor", "Decrease the damage a bit."); //1
            bodySlot.AddEquipment(2, 50, 7, "Fire Suit", "Protects you from fire."); //2
            bodySlot.AddEquipment(3, 25, 6, "Suicide Vest", "Leaves a small surprise after your death. "); //3
            bodySlot.AddEquipment(4, 50, 12, "Personal Jammer", "You can't be a target for strikes."); //4
            bodySlot.AddEquipment(5, 50, 10, "Blast Suit", "Decrease the explosion damage."); //5
            bodySlot.AddEquipment(6, 200, 12, "Heavy Armor", "Decrease the damage greatly. Very heavy. You can't sprint and roll."); //6
            bodySlot.AddEquipment(7, 75, 9, "Kevlar Armor", "Protects you from one-shot death."); //7

            #endregion

            #region Adding Levels
            //human 0
            AddLevel("Private", 0, 100); // 1
            AddLevel("First Private", 100, 125); // 2
            AddLevel("Private First Class", 150, 150); // 3
            AddLevel("Specialist", 200, 175); // 4 
            AddLevel("Corporal", 250, 200); // 5
            AddLevel("Sergeant", 350, 200); // 6
            AddLevel("Staff Sergeant", 400, 225); // 7
            AddLevel("Sergeant First Class", 470, 225); // 8
            AddLevel("Master Sergeant", 550, 250); // 9
            AddLevel("First Sergeant", 620, 275); // 10
            AddLevel("Sergeant Major", 690, 275); // 11
            AddLevel("Command Sergeant Major", 800, 300); // 12
            AddLevel("Sergeant Major \n of the Army", 900, 325); // 13
            AddLevel("Chief Warrant Officer 2", 1000, 350); // 14
            AddLevel("Chief Warrant Officer 3", 1200, 350); // 15
            AddLevel("Chief Warrant Officer 4", 1500, 350); // 16
            AddLevel("Chief Warrant Officer 5", 2000, 375); // 17 
            AddLevel("Second Lieutenant", 2500, 400); // 18
            AddLevel("First Lieutenant", 3000, 425); // 19 
            AddLevel("Captain", 3500, 450); // 20
            AddLevel("Major", 4000, 450); // 21
            AddLevel("Lieutenant Colonel", 4500, 475); // 22
            AddLevel("Colonel", 4500, 500); // 23
            AddLevel("Brigadier General", 5000, 525); // 24
            AddLevel("Major General", 6000, 550); // 25
            AddLevel("Lieutenant General", 7000, 575); // 26
            AddLevel("General", 8000, 600); // 27
            AddLevel("General of the Army (GOA)", 9000, 625); // 28
            AddLevel("Führer", 10000, 650); // 29
            AddLevel("Marshal of the Russian Federation", 10000, 700); // 30


            #endregion

            // load different map parts
            for (int i = 0; i < NumberOfMapParts; i++)
            {
                MapPartList.Add(new TMapPart());
                var mapPosition = GlobalGame.GetSingleObjectByCustomId("Map" + i + "CameraPosition").GetWorldPosition();
                MapPartList[i].MapPosition = new Vector2((float)Math.Round(mapPosition.X), (float)Math.Round(mapPosition.Y));

                MapPartList[i].PointList.Add(new TCapturePoint((IObjectText)Game.GetSingleObjectByCustomId("Point" + i + "0"), -100));
                MapPartList[i].PointList.Add(new TCapturePoint((IObjectText)Game.GetSingleObjectByCustomId("Point" + i + "1"), 0));
                MapPartList[i].PointList.Add(new TCapturePoint((IObjectText)Game.GetSingleObjectByCustomId("Point" + i + "2"), 100));
                //MapPartList[0].PointList.Add(new TCapturePoint((IObjectText)Game.GetSingleObjectByCustomId("Point" + i + "..."), 0));
                for (int j = 0; j < 8; j++)
                {
                    var redSpawn = Game.GetSingleObjectByCustomId("RedSpawnPoint" + i + j);
                    var blueSpawn = Game.GetSingleObjectByCustomId("BlueSpawnPoint" + i + j);
                    MapPartList[i].RedSpawnPosition.Add(redSpawn);
                    MapPartList[i].BlueSpawnPosition.Add(blueSpawn);
                }
            }



            GenerateDroneMap();

            PreparePlayerMenus();
            RefreshPlayerMenus();

            CameraSpeed = 2.0f;

            Events.UpdateCallback.Start(OnUpdate, 1);


            BeginTimerTrigger = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger", new Vector2(0, 0), 0);
            BeginTimerTrigger.SetRepeatCount(0);
            BeginTimerTrigger.SetIntervalTime(1000);
            BeginTimerTrigger.SetScriptMethod("OnBeginTimer");
            BeginTimerTrigger.Trigger();

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
        /// Update loop (must be enabled in the OnStartup() function or AfterStartup() function).
        /// </summary>
        /// <param name="elapsed">Time elapsed</param>
        public void OnUpdate(float elapsed)
        {
            try
            {
                if (GameState == 0)
                {
                    try
                    {
                        RefreshPlayerMenus();
                    }
                    catch (Exception e)
                    {
                        DebugLogger.DebugOnlyDialogLog("CAUSED BY REFRESHPLAYERMENUS" + e.StackTrace, CameraPosition);
                        DebugLogger.DebugOnlyDialogLog(e.Message, CameraPosition);
                        var st = new System.Diagnostics.StackTrace(e, true);
                        var frame = st.GetFrame(0);
                        var line = frame.GetFileLineNumber();
                        DebugLogger.DebugOnlyDialogLog("METHOD: " + e.TargetSite.Name + " LINE: " + line, CameraPosition);
                    }

                    foreach (var p in PlayerList)
                    {
                        if (p == null) continue;
                        if (p.User == null) continue;
                        var player = p.User.GetPlayer();
                        if (player == null) continue;
                        PlayerModifiers mods = player.GetModifiers();
                        mods.MaxEnergy = 0;
                        mods.MeleeStunImmunity = 1;
                        mods.EnergyRechargeModifier = 0;
                        player.SetModifiers(mods);
                    }

                    for (int i = 0; i < PlayerMenuList.Count; i++)
                    {
                        PlayerMenuList[i].Update();
                    }
                }
                else if (GameState == 1)
                {
                    if (TimeToStart > 0) return;
                    GlobalGame.SetCurrentCameraMode(CameraMode.Static);
                    GameState = 2;
                    AirPlayerList.Clear();
                    ThrownTrackingList.Clear();
                    PreWeaponTrackingUpdate();
                    PostWeaponTrackingUpdate();
                    RemoveObjects();
                    RemoveTurrets();
                    RemoveShieldGenerators();
                    ResetElectronicWarfare();
                    RespawnUnknownObjects();
                    ResetEffects();
                    RemoveWeapons();
                    MapPartList[CurrentMapPartIndex].Start();
                    TimeToStart = AreaTime;
                    if (!IsDebug && PlayerList.Count < 2)
                    {
                        Game.SetGameOver("NOT ENOUGH PLAYERS");
                        GameState = 100;
                        return;
                    }
                }
                else if (GameState == 2)
                {

                    if (Game.GetCameraArea().Left == CameraPosition.X && Game.GetCameraArea().Top == CameraPosition.Y)
                    {
                        if (IsFirstMatch)
                        {
                            TeamBalance();
                        }
                        Game.RunCommand("MSG BATTLE BEGINS");
                        GameState = 3;
                    }
                }
                else if (GameState == 3)
                {
                    int areaStatus = MapPartList[CurrentMapPartIndex].Update();
                    int capturedBy = MapPartList[CurrentMapPartIndex].CapturedBy;
                    UpdateEffects();
                    ThrownWeaponUpdate();
                    PreWeaponTrackingUpdate();
                    for (int i = 0; i < PlayerList.Count; i++)
                    {
                        PlayerList[i].Update();
                    }
                    PostWeaponTrackingUpdate();
                    UpdateTurrets();
                    UpdateShieldGenerators();
                    ThrowingUpdate();
                    if (IsAllPlayerDead())
                    {
                        GameState = 6;
                        TimeToStart = 5;
                    }
                    else if (TimeToStart <= 0)
                    {
                        if (capturedBy == 0) SpawnDrone(4, PlayerTeam.Team3);
                        else if (capturedBy == 1) SpawnDrone(5, PlayerTeam.Team1);
                        else if (capturedBy == 2) SpawnDrone(5, PlayerTeam.Team2);
                        TimeToStart = 30;
                    }
                    int teamStatus = IsOneTeamDead();
                    if (teamStatus != 0 || GameState == 6)
                    {
                        for (int i = 0; i < PlayerList.Count; i++)
                        {
                            PlayerList[i].Stop();
                        }
                        for (int i = 0; i < TurretList.Count; i++)
                        {
                            TurretList[i].StopMovement();
                        }
                    }
                    if (areaStatus == 1)
                    {
                        Game.RunCommand("MSG BLUE TEAM CAPTURED ALL POINTS");
                        SpawnDrone(4, PlayerTeam.Team1);
                        TimeToStart = 30;
                    }
                    else if (areaStatus == 2)
                    {
                        Game.RunCommand("MSG RED TEAM CAPTURED ALL POINTS");
                        SpawnDrone(4, PlayerTeam.Team2);
                        TimeToStart = 30;
                    }
                    if (teamStatus == 1)
                    {
                        TimeToStart = 5;
                        GameState = 7;
                    }
                    else if (teamStatus == 2)
                    {
                        TimeToStart = 5;
                        GameState = 8;
                    }

                }
                else if (GameState == 4)
                {
                    var mapPart = MapPartList[CurrentMapPartIndex];
                    mapPart.BlueRoundsWon++;
                    MapPartList[CurrentMapPartIndex].Restart();
                    AddTeamExp(10, 3, PlayerTeam.Team1, false);

                    if (mapPart.CurrentRound < RoundsPerMapPart && mapPart.BlueRoundsWon != RoundsPerMapPart - 1)
                    {
                        // won this round, but not enough to advance
                        RemoveWeapons();
                        GameState = 1;
                        TimeToStart = 5;
                        AddTeamExp(10, 4, PlayerTeam.Team1, false);
                        IsFirstMatch = false;
                        Game.RunCommand("MSG BLUE WON THIS ROUND!");
                        Game.RunCommand("MSG RED: " + mapPart.RedRoundsWon + " - BLUE: " + mapPart.BlueRoundsWon);
                        mapPart.CurrentRound++;
                        Game.RunCommand("MSG STARTING NEXT ROUND (" + MapPartList[CurrentMapPartIndex].CurrentRound + "/" + RoundsPerMapPart + ")");
                    }
                    else
                    {
                        // won this round, and it's enough to advance
                        if (RoundsPerMapPart > 1)
                        {
                            // more than one win was needed to advance
                            Game.RunCommand("MSG BLUE WAS THE BEST OF " + RoundsPerMapPart + "!");
                            mapPart.RedRoundsWon = 0;
                            mapPart.BlueRoundsWon = 0;
                            TimeToStart = 5;
                        }

                        if (CurrentMapPartIndex > 0)
                        {
                            CurrentMapPartIndex--;
                            IsFirstMatch = false;
                            GameState = 1;
                        }
                        else
                        {
                            AddTeamExp(30 + RoundsPerMapPart, 4, PlayerTeam.Team1, false);
                            GameState = -1;
                        }
                    }
                }
                else if (GameState == 5)
                {
                    var mapPart = MapPartList[CurrentMapPartIndex];
                    mapPart.RedRoundsWon++;
                    mapPart.Restart();
                    AddTeamExp(10, 3, PlayerTeam.Team2, false);

                    if (mapPart.CurrentRound < RoundsPerMapPart && mapPart.RedRoundsWon != RoundsPerMapPart - 1)
                    {
                        RemoveWeapons();
                        GameState = 1;
                        TimeToStart = 5;
                        IsFirstMatch = false;
                        AddTeamExp(10, 4, PlayerTeam.Team2, false);
                        Game.RunCommand("MSG RED TEAM WON THIS ROUND!");
                        Game.RunCommand("MSG RED: " + mapPart.RedRoundsWon + " - BLUE: " + mapPart.BlueRoundsWon);
                        mapPart.CurrentRound++;
                        Game.RunCommand("MSG STARTING NEXT ROUND (" + MapPartList[CurrentMapPartIndex].CurrentRound + "/" + RoundsPerMapPart + ")");
                    }
                    else
                    {
                        //  won this round, and it's enough to advance

                        if (RoundsPerMapPart > 1)
                        {
                            // more than one win was needed to advance
                            Game.RunCommand("MSG RED TEAM WAS THE BEST OF " + RoundsPerMapPart + "!");
                            mapPart.RedRoundsWon = 0;
                            mapPart.BlueRoundsWon = 0;
                            TimeToStart = 5;
                        }

                        if (CurrentMapPartIndex < MapPartList.Count - 1)
                        {
                            CurrentMapPartIndex++;
                            IsFirstMatch = false;
                            GameState = 1;
                        }
                        else
                        {
                            AddTeamExp(30 + RoundsPerMapPart, 4, PlayerTeam.Team2, false);
                            GameState = -2;
                        }
                    }


                }
                else if (GameState == 6)
                {
                    if (TimeToStart <= 0)
                    {
                        var mapPart = MapPartList[CurrentMapPartIndex];
                        mapPart.Restart();
                        TimeToStart = 5;
                        Game.RunCommand("MSG NOBODY CAPTURED THE AREA!");
                        GameState = 1;
                    }
                }
                else if (GameState == 7)
                {
                    if (TimeToStart <= 0)
                    {
                        GameState = 4;
                    }
                }
                else if (GameState == 8)
                {
                    if (TimeToStart <= 0)
                    {
                        GameState = 5;
                    }
                }
                else if (GameState == -1 || GameState == -2)
                {
                    var menuCameraPosition = GlobalGame.GetSingleObjectByCustomId("MenuCameraPosition").GetWorldPosition();
                    CameraPosition.X = menuCameraPosition.X;
                    CameraPosition.Y = menuCameraPosition.Y;
                    for (int i = 0; i < PlayerMenuList.Count; i++)
                    {
                        PlayerMenuList[i].ShowExp();
                    }
                    GlobalGame.SetWeatherType(WeatherType.None);
                    GameState -= 2;
                }
                else if (GameState == -3)
                {
                    if (!IsDataSaved)
                    {
                        SaveData();
                    }

                    if (TimeToStart <= 0)
                    {
                        Game.SetGameOver("BLUE TEAM WINS!");

                    }
                }
                else if (GameState == -4)
                {
                    if (!IsDataSaved)
                    {
                        SaveData();
                    }

                    if (TimeToStart <= 0)
                    {
                        Game.SetGameOver("RED TEAM WINS!");
                    }
                }
                UpdateCamera();

            }
            catch (Exception e)
            {
                DebugLogger.DebugOnlyDialogLog(e.StackTrace, CameraPosition);
                DebugLogger.DebugOnlyDialogLog(e.Message, CameraPosition);
                var st = new System.Diagnostics.StackTrace(e, true);
                var frame = st.GetFrame(0);
                var line = frame.GetFileLineNumber();
                DebugLogger.DebugOnlyDialogLog("METHOD: " + e.TargetSite.Name + " LINE: " + line, CameraPosition);
            }

        }

        private readonly List<string> PossibleUnknownObjects = new List<string>() { "Barrel00", "BarrelExplosive", "PropaneTank", "Crate00" };

        private void RespawnUnknownObjects()
        {
            var unknownSpawnPositions = Game.GetObjectsByCustomId("SpawnUnknown");
            if (unknownSpawnPositions.Length == 0) return;
            for (int i = 0; i < unknownSpawnPositions.Length; i++)
            {
                var trigger = unknownSpawnPositions[i];
                if (trigger == null) return;
                var position = trigger.GetWorldPosition();
                if (position == null) return;
                var index = GlobalRandom.Next(0, PossibleUnknownObjects.Count);
                var obj = Game.CreateObject(PossibleUnknownObjects[index], position);
                ObjectsToRemove.Add(obj);
            }
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

        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
