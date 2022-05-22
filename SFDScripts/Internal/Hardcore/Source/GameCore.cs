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

        #region Game Core Methods

        private void SpawnPlayers()
        {
            var spawn = Game.GetSingleObjectByCustomId("StartSpawnPoint");
            if (spawn == null)
            {
                DebugLogger.DebugOnlyDialogLog("Starting spawn point doesn't exist");
                return;
            }
            var positionToSpawn = spawn.GetWorldPosition();
            foreach (var user in Game.GetActiveUsers())
            {
                IPlayer p;
                if (user.GetPlayer() == null)
                {
                    p = Game.CreatePlayer(positionToSpawn);
                    p.SetUser(user);
                    p.SetTeam(PlayerTeam.Team3);
                    p.SetProfile(user.GetProfile());
                }
                else
                {
                    p = user.GetPlayer();
                    p.SetTeam(PlayerTeam.Team3);
                    p.SetWorldPosition(positionToSpawn);
                }
            }
        }


        public static bool TestDistance(Vector2 p1, Vector2 p2, int radius)
        {
            return (Math.Abs(p1.X - p2.X) <= radius) && (Math.Abs(p1.Y - p2.Y) <= radius);
        }

        public static bool IsAllPlayerDead()
        {
            if (IsDebug) return false;
            for (int i = 0; i < PlayerList.Count; i++)
            {
                if (PlayerList[i].IsAlive() && PlayerList[i].Team != PlayerTeam.Independent)
                {
                    return false;
                }
            }
            return true;
        }

        public static int IsOneTeamDead()
        {
            if (IsDebug) return 0;
            bool team1 = false;
            bool team2 = false;
            for (int i = 0; i < PlayerList.Count; i++)
            {
                if (PlayerList[i].IsAlive())
                {
                    if (PlayerList[i].Team == PlayerTeam.Team1)
                    {
                        team1 = true;
                    }
                    else if (PlayerList[i].Team == PlayerTeam.Team2)
                    {
                        team2 = true;
                    }
                }
            }
            if (!team2 && team1) return 1;
            else if (!team1 && team2) return 2;
            else return 0;
        }


        public static void AddTeamExp(int exp, int type, PlayerTeam team, bool aliveOnly)
        {
            for (int i = 0; i < PlayerList.Count; i++)
            {
                if (PlayerList[i].Team == team && (PlayerList[i].IsAlive() || !aliveOnly))
                {
                    PlayerList[i].AddExp(exp, type);
                }
            }
        }

        public int StrikeNameToInt(string name)
        {
            if (name == "Napalm") return 0;
            if (name == "Pinpoint") return 1;
            if (name == "Airstrike") return 2;
            return 0;
        }

        public bool CanStrikeJamming(int id)
        {
            if (id >= 0 && id <= 2) return true;
            else return false;
        }

        public void AirAreaEnter(TriggerArgs args)
        {
            string data = ((IObject)args.Caller).CustomId;
            IPlayer pl = (IPlayer)args.Sender;
            TPlayer player = GetPlayer(pl);
            if (player == null) return;
            string[] typeList = data.Split(';');
            TPlayerStrikeInfo plInfo = new TPlayerStrikeInfo
            {
                Player = pl
            };
            for (int i = 0; i < typeList.Length; i++)
            {
                string[] strikeInfo = typeList[i].Split(':');
                int id = StrikeNameToInt(strikeInfo[0]);
                if (CanStrikeJamming(id) && player.Armor.Jammer) continue;
                int angle = 0;
                if (strikeInfo.Length > 1) angle = Convert.ToInt32(strikeInfo[1]);
                TStrikeInfo info = new TStrikeInfo
                {
                    Id = id,
                    Angle = angle
                };
                plInfo.StrikeList.Add(info);
            }
            if (plInfo.StrikeList.Count > 0) AirPlayerList.Add(plInfo);
        }

        public void AirAreaLeave(TriggerArgs args)
        {
            IPlayer pl = (IPlayer)args.Sender;
            for (int i = 0; i < AirPlayerList.Count; i++)
            {
                if (AirPlayerList[i].Player == pl)
                {
                    AirPlayerList.RemoveAt(i);
                    return;
                }
            }
        }

        public static TPlayer GetPlayer(IPlayer player)
        {
            for (int i = 0; i < PlayerList.Count; i++)
            {
                IPlayer pl = PlayerList[i].User.GetPlayer();
                if (pl != null && pl == player) return PlayerList[i];
            }
            return null;
        }

        public void RemoveObjects()
        {
            string[] name = { "WpnMineThrown", "SupplyCrate00", "MetalRailing00", "WpnGrenadesThrown" };
            IObject[] objects = GlobalGame.GetObjectsByName(name);
            for (int i = 0; i < objects.Length; i++)
            {
                objects[i].Remove();
            }
            for (int i = 0; i < ObjectsToRemove.Count; i++)
            {
                if (ObjectsToRemove[i] != null)
                {
                    ObjectsToRemove[i].Remove();
                }
            }
            ObjectsToRemove.Clear();
        }

        public static PlayerTeam GetEnemyTeam(PlayerTeam team)
        {
            if (team == PlayerTeam.Team1) return PlayerTeam.Team2;
            else return PlayerTeam.Team1;
        }

        public static void ResetElectronicWarfare()
        {
            TeamJamming = new int[] { 0, 0, 0 };
            TeamHacking = new int[] { 0, 0, 0 };
        }

        public static TPlayer GetRandomPlayer(PlayerTeam team, bool isAlive)
        {
            List<TPlayer> players = new List<TPlayer>();
            for (int i = 0; i < PlayerList.Count; i++)
            {
                TPlayer player = PlayerList[i];
                if (player.Team == team && player.IsAlive() == isAlive) players.Add(player);
            }
            if (players.Count == 0) return null;
            int index = GlobalRandom.Next(0, players.Count);
            return players[index];
        }

        public static Vector2 GetRandomWorldPoint()
        {
            Area area = GlobalGame.GetCameraArea();
            return new Vector2(GlobalRandom.Next((int)area.Left, (int)area.Right),
                     GlobalRandom.Next((int)area.Bottom, (int)area.Top));
        }

        public static void UpdateEffects()
        {
            for (int i = 0; i < EffectList.Count; i++)
            {
                if (!EffectList[i].Update())
                {
                    EffectList.RemoveAt(i);
                    i--;
                }
            }
        }

        public static void ResetEffects()
        {
            EffectList.Clear();
        }

        public static void CreateEffect(IObject obj, string id, int time, int count)
        {
            TEffect effect = new TEffect(obj, id, time, count);
            EffectList.Add(effect);
        }

        public static bool IsPlayer(string name)
        {
            for (int i = 0; i < PlayerList.Count; i++)
            {
                IPlayer pl = PlayerList[i].User.GetPlayer();
                if (pl == null) continue;
                if (pl.Name == name) return true;
            }
            return false;
        }

        /// <summary>
        /// Method in charge of balancing the teams.
        /// The method creates a key/value pair of players with their level modified by a factor as a key
        /// and the index of the player as a value. It then proceeds to 
        /// </summary>
        public static void TeamBalance()
        {
            List<KeyValuePair<float, int>> levelList = new List<KeyValuePair<float, int>>();
            for (int i = 0; i < PlayerList.Count; i++)
            {
                levelList.Add(new KeyValuePair<float, int>(PlayerList[i].Level / 4.0f + 1, i));
            }

            // sorts the levels list in an ascending manner
            levelList.Sort((x, y) => x.Key.CompareTo(y.Key));

            if (GlobalRandom.Next(0, 2) == 0)
            {
                // there's a chance in two to get the list ordered in a descending manner
                // this has been added so that teams don't always stay the same between matches
                levelList.Reverse();
            }
            float teamPower1 = 0;
            float teamPower2 = 0;
            for (int i = 0; i < levelList.Count; i++)
            {
                // if the power of team one is less than the power of the team 2
                // of if (coincidentally) they have the exact same power and you had the chance in two to be added
                if (teamPower1 < teamPower2 || (teamPower1 == teamPower2 && GlobalRandom.Next(0, 2) == 0))
                {
                    // then add the coefficient to the team one's power
                    teamPower1 += levelList[i].Key;
                    // and set the player at this index to team one
                    PlayerList[levelList[i].Value].SetTeam(PlayerTeam.Team1);
                }
                else
                {
                    // else add the coefficient to the team two's power
                    teamPower2 += levelList[i].Key;
                    // and set the player at this index to team two
                    PlayerList[levelList[i].Value].SetTeam(PlayerTeam.Team2);
                }
            }
        }

        public static void UpdateShieldGenerators()
        {
            for (int i = 0; i < ShieldGeneratorList.Count; i++)
            {
                ShieldGeneratorList[i].Update();
                if (ShieldGeneratorList[i].CoreObject == null)
                {
                    ShieldGeneratorList.RemoveAt(i);
                    i--;
                }
            }
        }

        public static void RemoveShieldGenerators()
        {
            for (int i = 0; i < ShieldGeneratorList.Count; i++)
            {
                ShieldGeneratorList[i].Remove();
            }
            ShieldGeneratorList.Clear();
        }

        public static void UpdateTurrets()
        {
            for (int i = 0; i < TurretList.Count; i++)
            {
                TurretList[i].Update();
                if (TurretList[i].OtherObjects.Count == 0)
                {
                    TurretList.RemoveAt(i);
                    i--;
                }
            }
        }

        public static void RemoveTurrets()
        {
            for (int i = 0; i < TurretList.Count; i++)
            {
                TurretList[i].Remove();
            }
            TurretList.Clear();
        }





        public static int TracePath(Vector2 fromPos, Vector2 toPos, PlayerTeam team, bool fullCheck = false)
        {
            int width = 4;
            float angle = TwoPointAngle(fromPos, toPos);
            Vector2 diff = toPos - fromPos;
            float offsetX = (float)Math.Cos(angle) * (width + 8);
            float offsetY = (float)Math.Sin(angle) * (width + 8);
            int itCount = (int)Math.Ceiling(diff.X / offsetX);
            Vector2 currentPos = fromPos;
            int vision = 0;
            for (int i = 0; i < itCount; i++)
            {
                Area area = new Area(currentPos.Y + width / 2.0f, currentPos.X - width / 2.0f, currentPos.Y - width / 2.0f, currentPos.X + width / 2.0f);
                IObject[] objList = GlobalGame.GetObjectsByArea(area);
                for (int j = 0; j < objList.Length; j++)
                {
                    string name = objList[j].Name;
                    if (name.StartsWith("Bg") || name.StartsWith("FarBg")) continue;
                    if (IsPlayer(name))
                    {
                        TPlayer pl = GetPlayer((IPlayer)objList[j]);
                        if (pl.IsAlive() && !fullCheck)
                        {
                            if (pl.Team == team) vision = 3;
                            else return vision;
                        }
                        else vision = Math.Max(vision, 1);
                    }
                    else if (VisionObjects.ContainsKey(name)) vision = Math.Max(vision, VisionObjects[name]);
                    if (vision >= 3) return vision;
                }
                currentPos.X += offsetX;
                currentPos.Y += offsetY;
            }
            return vision;
        }

        public static bool CheckAreaToCollision(Area area)
        {
            IObject[] objList = GlobalGame.GetObjectsByArea(area);
            for (int j = 0; j < objList.Length; j++)
            {
                string name = objList[j].Name;
                if (name.StartsWith("Bg") || name.StartsWith("FarBg")) continue;
                if (VisionObjects.ContainsKey(name)) return true;
            }
            return false;
        }

        public static float TwoPointAngle(Vector2 beginPos, Vector2 endPos)
        {
            Vector2 diff = endPos - beginPos;
            return (float)Math.Atan2(diff.Y, diff.X);
        }

        public static List<IObject> GetTargetList(PlayerTeam team, Vector2 position, float distance, bool withTurrets, bool jammerProtection)
        {
            List<KeyValuePair<float, IObject>> targetList = new List<KeyValuePair<float, IObject>>();
            for (int i = 0; i < PlayerList.Count; i++)
            {
                if (PlayerList[i].IsAlive() && PlayerList[i].Team != team && (!PlayerList[i].Armor.Jammer || jammerProtection))
                {
                    IObject obj = PlayerList[i].User.GetPlayer();
                    float dist = (position - obj.GetWorldPosition()).Length();
                    if (dist > distance) continue;
                    targetList.Add(new KeyValuePair<float, IObject>(dist, obj));
                }
            }
            if (withTurrets)
            {
                for (int i = 0; i < TurretList.Count; i++)
                {
                    if (TurretList[i].MainMotor != null && TurretList[i].Team != team)
                    {
                        IObject obj = TurretList[i].MainMotor;
                        float dist = (position - obj.GetWorldPosition()).Length();
                        if (dist > distance) continue;
                        targetList.Add(new KeyValuePair<float, IObject>(dist, obj));
                    }
                }
            }
            targetList.Sort((x, y) => x.Key.CompareTo(y.Key));
            List<IObject> list = new List<IObject>();
            for (int i = 0; i < targetList.Count; i++)
            {
                list.Add(targetList[i].Value);
            }
            return list;
        }

        public static List<IObject> GetFriendList(PlayerTeam team, Vector2 position, float distance)
        {
            List<KeyValuePair<float, IObject>> targetList = new List<KeyValuePair<float, IObject>>();
            for (int i = 0; i < PlayerList.Count; i++)
            {
                if (PlayerList[i].IsAlive() && PlayerList[i].Team == team)
                {
                    IObject obj = PlayerList[i].User.GetPlayer();
                    float dist = (position - obj.GetWorldPosition()).Length();
                    if (dist > distance) continue;
                    targetList.Add(new KeyValuePair<float, IObject>(dist, obj));
                }
            }
            targetList.Sort((x, y) => x.Key.CompareTo(y.Key));
            List<IObject> list = new List<IObject>();
            for (int i = 0; i < targetList.Count; i++)
            {
                list.Add(targetList[i].Value);
            }
            return list;

        }

        public static float GetAngleDistance(float angle1, float angle2)
        {
            while (Math.Abs(angle1 - angle2) > Math.PI)
            {
                if (angle1 > angle2) angle1 -= (float)Math.PI * 2;
                else angle2 -= (float)Math.PI * 2;
            }
            return angle1 - angle2;
        }

        public static Vector2 GetPlayerCenter(IPlayer pl)
        {
            if (pl == null) return new Vector2(0, 0);
            Vector2 pos = pl.GetWorldPosition();
            if (!pl.IsCrouching && !pl.IsDiving && !pl.IsFalling && !pl.IsLayingOnGround) pos.Y += 4;
            return pos;
        }
        public static int[] CalculateSpecialAmmoSecondary(HandgunWeaponItem secondary)
        {
            int[] ammo = { 0, 1 };
            if (secondary.WeaponItem == WeaponItem.FLAREGUN)
            {
                ammo = new int[] { 2, 0 };
            }
            return ammo;
        }


        public static int[] CalculateSpecialAmmoPrimary(RifleWeaponItem primary)
        {
            int[] ammo = { (int)Math.Ceiling((double)primary.MaxTotalAmmo / 4), 0 };
            return ammo;
        }
        public static void ApplyWeaponMod(TPlayer player, int id)
        {
            IPlayer pl = player.User.GetPlayer();
            switch (id)
            {
                case 1:
                    {
                        pl.GiveWeaponItem(WeaponItem.LAZER);
                        break;
                    }
                case 2:
                    {
                        WeaponItem first = pl.CurrentPrimaryWeapon.WeaponItem;
                        WeaponItem second = pl.CurrentSecondaryWeapon.WeaponItem;
                        if (ExtraAmmoWeapon.Contains(first))
                        {

                            pl.GiveWeaponItem(first);

                        }
                        if (ExtraAmmoWeapon.Contains(second))
                        {
                            pl.GiveWeaponItem(second);
                        }
                        break;
                    }
                case 3:
                    {
                        WeaponItem first = pl.CurrentPrimaryWeapon.WeaponItem;
                        WeaponItem second = pl.CurrentSecondaryWeapon.WeaponItem;
                        WeaponItem thrown = pl.CurrentThrownItem.WeaponItem;
                        if (ExtraExplosiveWeapon.Contains(first)) pl.GiveWeaponItem(first);
                        if (ExtraExplosiveWeapon.Contains(second)) pl.GiveWeaponItem(second);
                        if (ExtraExplosiveWeapon.Contains(thrown)) pl.GiveWeaponItem(thrown);
                        break;
                    }
                case 4:
                    {
                        WeaponItem first = pl.CurrentPrimaryWeapon.WeaponItem;
                        WeaponItem second = pl.CurrentSecondaryWeapon.WeaponItem;
                        if (ExtraHeavyAmmoWeapon.Contains(first))
                        {
                            pl.GiveWeaponItem(first);
                            player.HasExtraHeavyAmmo = true;
                            DebugLogger.DebugOnlyDialogLog("player.HasExtraHeavyAmmo = " + player.HasExtraHeavyAmmo);
                        }
                        if (ExtraHeavyAmmoWeapon.Contains(second))
                        {
                            pl.GiveWeaponItem(second);
                            player.HasExtraHeavyAmmo = true;
                            DebugLogger.DebugOnlyDialogLog("player.HasExtraHeavyAmmo = " + player.HasExtraHeavyAmmo);
                        }
                        break;
                    }
                case 5:
                    {
                        if (player.PrimaryWeapon != null)
                        {
                            player.PrimaryWeapon.DNAProtection = true;
                            player.PrimaryWeapon.TeamDNA = player.Team;
                        }
                        else if (player.SecondaryWeapon != null)
                        {
                            player.SecondaryWeapon.DNAProtection = true;
                            player.SecondaryWeapon.TeamDNA = player.Team;
                        }
                        break;
                    }
                case 6:
                    {
                        if (player.PrimaryWeapon != null && pl.CurrentPrimaryWeapon.WeaponItem != WeaponItem.FLAMETHROWER)
                        {
                            RifleWeaponItem first = pl.CurrentPrimaryWeapon;
                            int[] ammo = CalculateSpecialAmmoPrimary(first);
                            pl.SetCurrentPrimaryWeaponAmmo(ammo[0], ammo[1], ProjectilePowerup.Bouncing);
                            pl.GiveWeaponItem(first.WeaponItem);

                        }
                        else if (player.SecondaryWeapon != null)
                        {
                            HandgunWeaponItem second = pl.CurrentSecondaryWeapon;
                            int[] ammo = CalculateSpecialAmmoSecondary(second);
                            pl.SetCurrentSecondaryWeaponAmmo(ammo[0], ammo[1], ProjectilePowerup.Bouncing);
                            pl.GiveWeaponItem(second.WeaponItem);
                        }
                        break;
                    }
                case 7:
                    {
                        if (player.PrimaryWeapon != null && pl.CurrentPrimaryWeapon.WeaponItem != WeaponItem.FLAMETHROWER)
                        {
                            RifleWeaponItem first = pl.CurrentPrimaryWeapon;
                            int[] ammo = CalculateSpecialAmmoPrimary(first);
                            pl.SetCurrentPrimaryWeaponAmmo(ammo[0], ammo[1], ProjectilePowerup.Fire);
                            pl.GiveWeaponItem(first.WeaponItem);

                        }
                        else if (player.SecondaryWeapon != null)
                        {
                            HandgunWeaponItem second = pl.CurrentSecondaryWeapon;
                            int[] ammo = CalculateSpecialAmmoSecondary(second);
                            pl.SetCurrentSecondaryWeaponAmmo(ammo[0], ammo[1], ProjectilePowerup.Fire);
                            pl.GiveWeaponItem(second.WeaponItem);
                        }
                        break;
                    }
            }
        }

        public static void ApplyThrownWeapon(TPlayer player, int id)
        {
            IPlayer pl = player.User.GetPlayer();
            switch (id)
            {
                case 1:
                    {
                        pl.GiveWeaponItem(WeaponItem.GRENADES);
                        break;
                    }
                case 2:
                    {
                        pl.GiveWeaponItem(WeaponItem.MOLOTOVS);
                        break;
                    }
                case 3:
                    {
                        pl.GiveWeaponItem(WeaponItem.MINES);
                        break;
                    }
                case 4:
                    {
                        pl.GiveWeaponItem(WeaponItem.GRENADES);
                        player.WeaponTrackingUpdate(true);
                        player.ThrownWeapon.CustomId = 1;
                        break;
                    }
                case 5:
                    {
                        pl.GiveWeaponItem(WeaponItem.GRENADES);
                        player.WeaponTrackingUpdate(true);
                        player.ThrownWeapon.CustomId = 2;
                        break;
                    }
                case 6:
                    {
                        pl.GiveWeaponItem(WeaponItem.GRENADES);
                        player.WeaponTrackingUpdate(true);
                        player.ThrownWeapon.CustomId = 3;
                        break;
                    }
                case 7:
                    {
                        pl.GiveWeaponItem(WeaponItem.SHURIKEN);
                        break;
                    }
            }
            if (id < 4) player.WeaponTrackingUpdate(true);
        }

        public static void ApplyWeapon(TPlayer player, WeaponItem id)
        {
            IPlayer pl = player.User.GetPlayer();
            pl.GiveWeaponItem(id);
        }

        public static bool IsJamming(PlayerTeam team)
        {
            for (int i = 0; i < TeamJamming.Length; ++i)
            {
                if (i == (int)team - 1) continue;
                else if (TeamJamming[i] > 0) return true;
            }
            return false;
        }

        public static bool IsHacking(PlayerTeam team)
        {
            for (int i = 0; i < TeamHacking.Length; ++i)
            {
                if (i == (int)team - 1) continue;
                else if (TeamHacking[i] > 0) return true;
            }
            return false;
        }

        public static string FormatName(string name)
        {
            name = name.Replace(";", "_").Replace(":", "_").Replace("\\", "_").Replace("\'", "_");
            return name;
        }

        public static void GenerateDroneMap()
        {
            for (int i = 0; i < DroneAreaSize.X; ++i)
            {
                List<int> line = new List<int>();
                for (int j = 0; j < DroneAreaSize.Y; ++j)
                {
                    line.Add(0);
                }
                DroneMap1x1.Add(line);
            }
            IObject[] areaList = GlobalGame.GetObjectsByCustomId("Drone");
            for (int i = 0; i < areaList.Length; ++i)
            {
                Vector2 begin = areaList[i].GetWorldPosition() - DroneAreaBegin;
                begin.X = (int)begin.X / 8;
                begin.Y = (int)begin.Y / 8;
                for (float x = begin.X; x < begin.X + (float)areaList[i].GetSizeFactor().X; x += 1)
                {
                    for (float y = begin.Y - (float)areaList[i].GetSizeFactor().Y + 1; y <= begin.Y; y += 1)
                    {
                        DroneMap1x1[(int)x][(int)y] = 1;
                    }
                }
            }
            for (int i = 0; i < DroneAreaSize.X; ++i)
            {
                for (int j = 0; j < DroneAreaSize.Y; ++j)
                {
                    if (DroneMap1x1[i][j] > 0)
                    {
                        if (DroneMap1x1[i + 1][j] > 0 && DroneMap1x1[i + 1][j + 1] > 0 && DroneMap1x1[i][j + 1] > 0) DroneMap1x1[i][j] = 2;
                        else if (DroneMap1x1[i + 1][j] > 0 && DroneMap1x1[i + 1][j - 1] > 0 && DroneMap1x1[i][j - 1] > 0) DroneMap1x1[i][j] = 2;
                        else if (DroneMap1x1[i - 1][j] > 0 && DroneMap1x1[i - 1][j - 1] > 0 && DroneMap1x1[i][j - 1] > 0) DroneMap1x1[i][j] = 2;
                        else if (DroneMap1x1[i - 1][j] > 0 && DroneMap1x1[i - 1][j + 1] > 0 && DroneMap1x1[i][j + 1] > 0) DroneMap1x1[i][j] = 2;
                    }
                }
            }
        }

        public static Vector2 GetNearestDroneMapCell(Vector2 position, int size)
        {
            Vector2 center = position - DroneAreaBegin;
            center.X = (int)center.X / 8;
            center.Y = (int)center.Y / 8;
            if (DroneMap1x1[(int)center.X][(int)center.Y] >= size) return center;
            for (int i = 1; i < 100; ++i)
            {
                if (center.Y + i < DroneAreaSize.Y)
                {
                    for (int j = Math.Max(0, (int)center.X - i); j <= Math.Min((int)center.X + i, DroneAreaSize.X - 1); ++j)
                    {
                        if (DroneMap1x1[(int)j][(int)center.Y + i] >= size) return new Vector2(j, center.Y + i);
                    }
                }
                if (center.Y - i > 0)
                {
                    for (int j = Math.Max(0, (int)center.X - i); j <= Math.Min((int)center.X + i, DroneAreaSize.X - 1); ++j)
                    {
                        if (DroneMap1x1[(int)j][(int)center.Y - i] >= size) return new Vector2(j, center.Y - i);
                    }
                }
                if (center.X - i > 0)
                {
                    for (int j = Math.Max(0, (int)center.Y - i + 1); j <= Math.Min((int)center.Y + i - 1, DroneAreaSize.Y - 1); ++j)
                    {
                        if (DroneMap1x1[(int)center.X - i][(int)j] >= size) return new Vector2(center.X - i, j);
                    }
                }
                if (center.X + i < DroneAreaSize.X)
                {
                    for (int j = Math.Max(0, (int)center.Y - i + 1); j <= Math.Min((int)center.Y + i - 1, DroneAreaSize.Y - 1); ++j)
                    {
                        if (DroneMap1x1[(int)center.X + i][(int)j] >= size) return new Vector2(center.X + i, j);
                    }
                }
            }
            return center;
        }

        public static List<Vector2> FindDronePath(Vector2 from, Vector2 to, int size)
        {
            List<Vector2> toCheck = new List<Vector2>();
            List<List<float>> pathMap = new List<List<float>>();
            for (int i = 0; i < DroneAreaSize.X; ++i)
            {
                List<float> line = new List<float>();
                for (int j = 0; j < DroneAreaSize.Y; ++j)
                {
                    line.Add(1000);
                }
                pathMap.Add(line);
            }
            pathMap[(int)from.X][(int)from.Y] = 0;
            toCheck.Add(from);
            while (toCheck.Count > 0)
            {
                Vector2 current = toCheck[0];
                toCheck.RemoveAt(0);
                if (pathMap[(int)current.X][(int)current.Y] >= pathMap[(int)to.X][(int)to.Y]) continue;
                if (current.X > 0 && DroneMap1x1[(int)current.X - 1][(int)current.Y] >= size && pathMap[(int)current.X - 1][(int)current.Y] > pathMap[(int)current.X][(int)current.Y] + 1)
                {
                    pathMap[(int)current.X - 1][(int)current.Y] = pathMap[(int)current.X][(int)current.Y] + 1;
                    toCheck.Add(new Vector2(current.X - 1, current.Y));
                }
                if (current.X + 1 < DroneAreaSize.X && DroneMap1x1[(int)current.X + 1][(int)current.Y] >= size && pathMap[(int)current.X + 1][(int)current.Y] > pathMap[(int)current.X][(int)current.Y] + 1)
                {
                    pathMap[(int)current.X + 1][(int)current.Y] = pathMap[(int)current.X][(int)current.Y] + 1;
                    toCheck.Add(new Vector2(current.X + 1, current.Y));
                }
                if (current.Y > 0 && DroneMap1x1[(int)current.X][(int)current.Y - 1] >= size && pathMap[(int)current.X][(int)current.Y - 1] > pathMap[(int)current.X][(int)current.Y] + 1)
                {
                    pathMap[(int)current.X][(int)current.Y - 1] = pathMap[(int)current.X][(int)current.Y] + 1;
                    toCheck.Add(new Vector2(current.X, current.Y - 1));
                }
                if (current.Y + 1 < DroneAreaSize.Y && DroneMap1x1[(int)current.X][(int)current.Y + 1] >= size && pathMap[(int)current.X][(int)current.Y + 1] > pathMap[(int)current.X][(int)current.Y] + 1)
                {
                    pathMap[(int)current.X][(int)current.Y + 1] = pathMap[(int)current.X][(int)current.Y] + 1;
                    toCheck.Add(new Vector2(current.X, current.Y + 1));
                }

                if (current.X > 0 && current.Y > 0 && DroneMap1x1[(int)current.X - 1][(int)current.Y] >= size
                && DroneMap1x1[(int)current.X][(int)current.Y - 1] >= size
                && DroneMap1x1[(int)current.X - 1][(int)current.Y - 1] >= size
                && pathMap[(int)current.X - 1][(int)current.Y - 1] > pathMap[(int)current.X][(int)current.Y] + 1.4f)
                {
                    pathMap[(int)current.X - 1][(int)current.Y - 1] = pathMap[(int)current.X][(int)current.Y] + 1.4f;
                    toCheck.Add(new Vector2(current.X - 1, current.Y - 1));
                }

                if (current.X > 0 && current.Y + 1 < DroneAreaSize.Y && DroneMap1x1[(int)current.X - 1][(int)current.Y] >= size
                && DroneMap1x1[(int)current.X][(int)current.Y + 1] >= size
                && DroneMap1x1[(int)current.X - 1][(int)current.Y + 1] >= size
                && pathMap[(int)current.X - 1][(int)current.Y + 1] > pathMap[(int)current.X][(int)current.Y] + 1.4f)
                {
                    pathMap[(int)current.X - 1][(int)current.Y + 1] = pathMap[(int)current.X][(int)current.Y] + 1.4f;
                    toCheck.Add(new Vector2(current.X - 1, current.Y + 1));
                }

                if (current.X + 1 < DroneAreaSize.X && current.Y > 0 && DroneMap1x1[(int)current.X + 1][(int)current.Y] >= size
                && DroneMap1x1[(int)current.X][(int)current.Y - 1] >= size
                && DroneMap1x1[(int)current.X + 1][(int)current.Y - 1] >= size
                && pathMap[(int)current.X + 1][(int)current.Y - 1] > pathMap[(int)current.X][(int)current.Y] + 1.4f)
                {
                    pathMap[(int)current.X + 1][(int)current.Y - 1] = pathMap[(int)current.X][(int)current.Y] + 1.4f;
                    toCheck.Add(new Vector2(current.X + 1, current.Y - 1));
                }

                if (current.X + 1 < DroneAreaSize.X && current.Y + 1 < DroneAreaSize.Y && DroneMap1x1[(int)current.X + 1][(int)current.Y] >= size
                && DroneMap1x1[(int)current.X][(int)current.Y + 1] >= size
                && DroneMap1x1[(int)current.X + 1][(int)current.Y + 1] >= size
                && pathMap[(int)current.X + 1][(int)current.Y + 1] > pathMap[(int)current.X][(int)current.Y] + 1.4f)
                {
                    pathMap[(int)current.X + 1][(int)current.Y + 1] = pathMap[(int)current.X][(int)current.Y] + 1.4f;
                    toCheck.Add(new Vector2(current.X + 1, current.Y + 1));
                }
            }
            List<Vector2> path = new List<Vector2>();
            if (pathMap[(int)to.X][(int)to.Y] == 1000) return path;
            Vector2 next = to;
            path.Add(GetDroneMapPosition((int)to.X, (int)to.Y, size));
            while (next != from)
            {
                Vector2 dir = new Vector2(0, 0);
                float dist = 1000;
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (i == 0 && j == 0) continue;
                        if (next.X + i > 0 && next.Y + j > 0 && next.X + i < DroneAreaSize.X && next.Y + j < DroneAreaSize.Y
                        && pathMap[(int)next.X + i][(int)next.Y + j] < dist)
                        {
                            dir.X = i;
                            dir.Y = j;
                            dist = pathMap[(int)next.X + i][(int)next.Y + j];
                        }
                    }
                }
                next += dir;
                Vector2 waypoint = GetDroneMapPosition((int)next.X, (int)next.Y, size);
                if (path.Count >= 2)
                {
                    Vector2 last1 = path[path.Count - 1];
                    Vector2 last2 = path[path.Count - 2];
                    if ((last1.X == last2.X || last1.Y == last2.Y) && (last1.X == waypoint.X || last1.Y == waypoint.Y))
                    {
                        path[path.Count - 1] = waypoint;
                        continue;
                    }
                }
                path.Add(waypoint);
            }
            return path;
        }

        public static Vector2 GetDroneMapPosition(int x, int y, int size)
        {
            if (size == 2)
            {
                if (DroneMap1x1[x + 1][y] > 0 && DroneMap1x1[x + 1][y + 1] > 0 && DroneMap1x1[x][y + 1] > 0) return new Vector2(x * 8 + 8, y * 8 + 6) + DroneAreaBegin;
                else if (DroneMap1x1[x + 1][y] > 0 && DroneMap1x1[x + 1][y - 1] > 0 && DroneMap1x1[x][y - 1] > 0) return new Vector2(x * 8 + 8, y * 8) + DroneAreaBegin;
                else if (DroneMap1x1[x - 1][y] > 0 && DroneMap1x1[x - 1][y - 1] > 0 && DroneMap1x1[x][y - 1] > 0) return new Vector2(x * 8 - 2, y * 8) + DroneAreaBegin;
                else if (DroneMap1x1[x - 1][y] > 0 && DroneMap1x1[x - 1][y + 1] > 0 && DroneMap1x1[x][y + 1] > 0) return new Vector2(x * 8 - 2, y * 8 + 6) + DroneAreaBegin;
            }
            return new Vector2(x * 8, y * 8) + DroneAreaBegin;
        }

        public static void SpawnDrone(int id, PlayerTeam team)
        {
            Area area = GlobalGame.GetCameraArea();
            float x = GlobalRandom.Next((int)(area.Left + area.Width / 5), (int)(area.Right - area.Width / 5));
            float y = area.Top - 10;
            CreateTurret(id, new Vector2(x, y), 1, team);
            GlobalGame.PlayEffect("EXP", new Vector2(x, y));
            GlobalGame.PlaySound("Explosion", new Vector2(x, y), 1.0f);
        }


        public static void ElectricExplosion(Vector2 position, int damage, float range)
        {
            for (int i = 0; i < PlayerList.Count; i++)
            {
                if (PlayerList[i].User.GetPlayer() == null) continue;
                float dist = (PlayerList[i].Position - position).Length();
                if (dist <= range)
                {
                    PlayerList[i].Hp -= damage;
                    CreateEffect(PlayerList[i].User.GetPlayer(), "S_P", 10, 10);
                }
            }
        }

        public static void ThrowingUpdate()
        {
            IObject[] objects = GlobalGame.GetMissileObjects();
            for (int i = 0; i < objects.Length; i++)
            {
                if (!AllowedMissile.Contains(objects[i].Name))
                {
                    objects[i].TrackAsMissile(false);
                }
            }
        }


        #endregion

        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
