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

        #region Custom Equipment Class
        public class TCustomEquipment
        {
            public int Id = 0;
            public int Reloading = 0;
            public int FastReloading = 0;
            public bool IsActive = false;
            public int Stage = 0;
            public string Name = "";
            public bool ReloadUse = false;
            public int MaxAmmo = 1;
            public int CurrentAmmo;
            public int AmmoRegeneration = 0;
            public Vector2 TargetPosition;
            public Vector2 BeginPosition;
            public List<IObject> ObjectList = new List<IObject>();
            public List<float> OtherData = new List<float>();
            public bool ForceShowReloading = false;
            public TCustomEquipment(int id = 0)
            {
                SetId(id);
            }
            public void SetId(int id)
            {
                Id = id;
                ReloadUse = false;
                Reloading = 0;
                FastReloading = 0;
                IsActive = false;
                Stage = 0;
                MaxAmmo = 1;
                AmmoRegeneration = 0;
                if (Id == 0)
                {
                    Name = "";
                    return;
                }
                if (Id == 4 || Id == 5 || Id == 6 || Id == 8 || Id == 9 || id == 10) Reloading = 5;
                if (Id == 2) MaxAmmo = 3;
                if (Id == 17)
                {
                    OtherData.Add(25);
                    OtherData.Add(25);
                    OtherData.Add(25);
                    OtherData.Add(25);
                    OtherData.Add(0);
                }
                if (Id == 20)
                {
                    MaxAmmo = 10;
                    OtherData.Add(0);
                }
                CurrentAmmo = MaxAmmo;
                Name = EquipmentList[6].Get(id).Name;
            }
            public string GetName()
            {
                string name = Name;
                if (MaxAmmo > 1) name += "[" + CurrentAmmo.ToString() + "]";
                if (ForceShowReloading || (Reloading > 0 && !ReloadUse && !IsActive)) name += "(" + Reloading.ToString() + ")";
                return name;
            }
            public void Reload(TPlayer player)
            {
                if (CurrentAmmo < MaxAmmo && AmmoRegeneration > 0)
                {
                    CurrentAmmo = Math.Min(CurrentAmmo + AmmoRegeneration, MaxAmmo);
                }
                if (Reloading > 0)
                {
                    Reloading--;
                }
                else if (ReloadUse)
                {
                    Use(player);
                    ReloadUse = false;
                }
            }
            public void DestroyObjects()
            {
                for (int i = 0; i < ObjectList.Count; i++)
                {
                    ObjectList[i].Destroy();
                }
                ObjectList.Clear();
            }
            public void MinusAmmo()
            {
                CurrentAmmo--;
                if (CurrentAmmo <= 0 && AmmoRegeneration <= 0)
                {
                    SetId(0);
                    return;
                }
            }
            public void Update(TPlayer player)
            {
                if (FastReloading > 0)
                {
                    FastReloading--;
                }
                switch (Id)
                {
                    case 4:
                        {
                            if (IsActive)
                            {
                                ContinueNapalmStrike();
                            }
                            break;
                        }
                    case 6:
                        {
                            if (IsActive)
                            {
                                ContinueAirstrike();
                            }
                            break;
                        }
                    case 8:
                        {
                            if (IsActive)
                            {
                                ContinueArtilleryStrike();
                            }
                            break;
                        }
                    case 9:
                        {
                            if (IsActive)
                            {
                                ContinueMineStrike();
                            }
                            break;
                        }
                    case 17:
                        {
                            UpdatePoliceShield(player);
                            break;
                        }
                    case 20:
                        {
                            UpdateJetPack(player);
                            break;
                        }
                }
            }
            public void Use(TPlayer player)
            {
                if (Reloading > 0 || FastReloading > 0 || CurrentAmmo <= 0) return;
                switch (Id)
                {
                    case 1:
                        {
                            if (RevivePlayer(player)) { }
                            else if (StopBleedingSelf(player)) { }
                            else if (StopBleedingNear(player)) { }
                            else return;
                            MinusAmmo();
                            break;
                        }
                    case 2:
                        {
                            if (RevivePlayer(player)) { }
                            else if (StopBleedingSelf(player)) { }
                            else if (StopBleedingNear(player)) { }
                            else return;
                            Reloading = 5;
                            MinusAmmo();
                            break;
                        }
                    case 3:
                        {
                            if (!IsJamming(player.Team))
                            {
                                CallAirDrop(player);
                                MinusAmmo();
                            }
                            break;
                        }
                    case 4:
                        {
                            if (!ReloadUse && !IsActive)
                            {
                                if (!IsJamming(player.Team))
                                {
                                    ReloadUse = true;
                                    Reloading = 3;
                                    GlobalGame.RunCommand("MSG NAPALM STRIKE IS COMING");
                                }
                            }
                            else
                            {
                                if (!IsHacking(player.Team)) IsActive = true;
                                else
                                {
                                    MinusAmmo();
                                    GlobalGame.RunCommand("MSG NAPALM STRIKE HAS BEED HACKED");
                                }

                            }
                            break;
                        }
                    case 5:
                        {
                            if (!ReloadUse)
                            {
                                if (CheckAirPlayer(player, 1) && !IsJamming(player.Team))
                                {
                                    ReloadUse = true;
                                    Reloading = 3;
                                    if (player.Team == PlayerTeam.Team1)
                                    {
                                        GlobalGame.RunCommand("MSG BLUE TEAM CALLED PINPOINT STRIKE");
                                    }
                                    else
                                    {
                                        GlobalGame.RunCommand("MSG RED TEAM CALLED PINPOINT STRIKE");
                                    }
                                }
                            }
                            else
                            {
                                if (!CallPinpointStrike(player))
                                {
                                    GlobalGame.RunCommand("MSG PINPOINT STRIKE: TARGET LOST");
                                    ReloadUse = false;
                                    Reloading = 5;
                                }
                                else
                                {
                                    MinusAmmo();
                                }
                            }
                            break;
                        }
                    case 6:
                        {
                            if (!IsActive)
                            {
                                if (!ReloadUse)
                                {
                                    if (CheckAirPlayer(player, 2) && !IsJamming(player.Team))
                                    {
                                        ReloadUse = true;
                                        Reloading = 3;
                                        if (player.Team == PlayerTeam.Team1)
                                        {
                                            GlobalGame.RunCommand("MSG BLUE TEAM CALLED AIRSTRIKE");
                                        }
                                        else
                                        {
                                            GlobalGame.RunCommand("MSG RED TEAM CALLED AIRSTRIKE");
                                        }
                                    }
                                }
                                else
                                {
                                    if (CallAirstrike(player)) IsActive = true;
                                    else
                                    {
                                        GlobalGame.RunCommand("MSG AIRSTRIKE: TARGET LOST");
                                        ReloadUse = false;
                                        Reloading = 5;
                                    }
                                }
                            }
                            break;

                        }
                    case 7:
                        {
                            if (!IsJamming(player.Team))
                            {
                                CallAirDrop(player, 3);
                                MinusAmmo();
                            }
                            break;
                        }
                    case 8:
                        {
                            if (!ReloadUse && !IsActive)
                            {
                                if (!IsJamming(player.Team))
                                {
                                    ReloadUse = true;
                                    Reloading = 3;
                                    GlobalGame.RunCommand("MSG ARTILLERY STRIKE IS COMING");
                                }
                            }
                            else
                            {
                                if (!IsHacking(player.Team)) IsActive = true;
                                else
                                {
                                    MinusAmmo();
                                    GlobalGame.RunCommand("MSG ARTILLERY STRIKE HAS BEED HACKED");
                                }
                            }
                            break;
                        }
                    case 9:
                        {
                            if (!ReloadUse && !IsActive)
                            {
                                if (!IsJamming(player.Team))
                                {
                                    ReloadUse = true;
                                    Reloading = 3;
                                    GlobalGame.RunCommand("MSG MINE STRIKE IS COMING");
                                }
                            }
                            else
                            {
                                if (!IsHacking(player.Team)) IsActive = true;
                                else
                                {
                                    MinusAmmo();
                                    GlobalGame.RunCommand("MSG MINE STRIKE HAS BEED HACKED");
                                }
                            }
                            break;
                        }
                    case 10:
                        {
                            if (!IsJamming(player.Team))
                            {
                                CallReinforcement(player);
                                SetId(0);
                                if (player.Team == PlayerTeam.Team1)
                                {
                                    GlobalGame.RunCommand("MSG BLUE TEAM CALLED REINFORCEMENT");
                                }
                                else
                                {
                                    GlobalGame.RunCommand("MSG RED TEAM CALLED REINFORCEMENT");
                                }
                            }
                            break;
                        }
                    case 11:
                        {
                            TeamJamming[(int)player.Team - 1] += 10;
                            MinusAmmo();
                            if (player.Team == PlayerTeam.Team1)
                            {
                                GlobalGame.RunCommand("MSG BLUE TEAM ENABLE SUPPLY JAMMER");
                            }
                            else
                            {
                                GlobalGame.RunCommand("MSG RED TEAM ENABLE SUPPLY JAMMER");
                            }
                            break;
                        }
                    case 12:
                        {
                            TeamHacking[(int)player.Team - 1] += 10;
                            MinusAmmo();
                            if (player.Team == PlayerTeam.Team1)
                            {
                                GlobalGame.RunCommand("MSG BLUE TEAM ENABLE SUPPLY HACKING");
                            }
                            else
                            {
                                GlobalGame.RunCommand("MSG RED TEAM ENABLE SUPPLY HACKING");
                            }
                            break;
                        }
                    case 13:
                        {
                            MinusAmmo();
                            PlaceTurret(player, 0);
                            break;
                        }
                    case 14:
                        {
                            MinusAmmo();
                            PlaceTurret(player, 1);
                            break;
                        }
                    case 15:
                        {
                            MinusAmmo();
                            PlaceTurret(player, 2);
                            break;
                        }
                    case 16:
                        {
                            MinusAmmo();
                            PlaceTurret(player, 3);
                            break;
                        }
                    case 18:
                        {
                            if (!ReloadUse)
                            {
                                if (player.IsAdrenaline)
                                {
                                    MinusAmmo();
                                }
                                else
                                {
                                    ReloadUse = true;
                                    Reloading = 5;
                                    ForceShowReloading = true;
                                    player.IsAdrenaline = true;
                                    player.AdrenalineDamageFactor = 0.2f;
                                    player.DamageDelaySpeed = 1;
                                    GlobalGame.PlaySound("GetHealthSmall", player.Position, 1);
                                }
                            }
                            else
                            {
                                player.IsAdrenaline = false;
                                MinusAmmo();
                            }
                            break;
                        }
                    case 19:
                        {
                            MinusAmmo();
                            PlaceShieldGenerator(player);
                            break;
                        }

                    case 21:
                        {
                            MinusAmmo();
                            SpawnStreetsweeper(player);
                            break;
                        }
                    case 22:
                        {
                            MinusAmmo();
                            SpawnDrone(player, 7);
                            break;
                        }

                    case 23:
                        {
                            MinusAmmo();
                            SpawnDrone(player, 6);
                            break;
                        }

                    case 24:
                        {
                            MinusAmmo();
                            SpawnDrone(player, 5);
                            break;
                        }
                    case 25:
                        {
                            MinusAmmo();
                            SpawnDrone(player, 4);
                            break;
                        }
                }
            }
            public bool StopBleedingNear(TPlayer player)
            {
                for (int i = 0; i < PlayerList.Count; i++)
                {
                    TPlayer pl = PlayerList[i];
                    if (pl.User.GetPlayer() != null && pl.Status == 0 && pl.Bleeding == true
                    && pl.Team == player.Team && TestDistance(player.User.GetPlayer().GetWorldPosition(), pl.User.GetPlayer().GetWorldPosition(), 10))
                    {
                        GlobalGame.PlaySound("GetHealthSmall", player.User.GetPlayer().GetWorldPosition(), 1);
                        pl.Bleeding = false;
                        player.AddExp(2.5f, 1);
                        return true;
                    }
                }
                return false;
            }
            public bool StopBleedingSelf(TPlayer player)
            {
                if (player.Bleeding)
                {
                    GlobalGame.PlaySound("GetHealthSmall", player.User.GetPlayer().GetWorldPosition(), 1);
                    player.Bleeding = false;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            public bool RevivePlayer(TPlayer player)
            {
                for (int i = 0; i < PlayerList.Count; i++)
                {
                    TPlayer pl = PlayerList[i];
                    if (!pl.CanRevive()) continue;
                    if (pl.Team == player.Team && TestDistance(player.User.GetPlayer().GetWorldPosition(), pl.User.GetPlayer().GetWorldPosition(), 10))
                    {
                        GlobalGame.PlaySound("GetHealthSmall", player.User.GetPlayer().GetWorldPosition(), 1);
                        pl.Revive(pl.ReviveHealth);
                        player.AddExp(5, 1);
                        return true;
                    }
                }
                return false;
            }
            public void CallAirDrop(TPlayer player, int count = 1)
            {
                Vector2 pos = player.User.GetPlayer().GetWorldPosition();
                if (IsHacking(player.Team))
                {
                    TPlayer enemy = GetRandomPlayer(GetEnemyTeam(player.Team), true);
                    if (enemy != null) pos = enemy.User.GetPlayer().GetWorldPosition();
                    else pos = GetRandomWorldPoint();
                }
                pos.Y = WorldTop;
                int offset = 20;
                pos.X -= offset * (count - 1);
                for (int i = 0; i < count; i++)
                {
                    GlobalGame.CreateObject("SupplyCrate00", pos);
                    pos.X += offset;
                    pos.Y += GlobalRandom.Next(-offset, offset);
                }
            }
            public Vector2 GetRandomAirEnemy(PlayerTeam friendTeam, int id, ref int angle)
            {
                angle = 0;
                List<int> indexList = new List<int>();
                for (int i = 0; i < AirPlayerList.Count; i++)
                {
                    if (AirPlayerList[i].Player != null && (friendTeam != AirPlayerList[i].Player.GetTeam() || friendTeam == PlayerTeam.Independent) && !AirPlayerList[i].Player.IsDead)
                    {
                        for (int j = 0; j < AirPlayerList[i].StrikeList.Count; j++)
                        {
                            if (AirPlayerList[i].StrikeList[j].Id == id)
                            {
                                angle = AirPlayerList[i].StrikeList[j].Angle;
                                indexList.Add(i);
                            }
                        }
                    }
                }
                if (indexList.Count == 0) return new Vector2(0, 0);
                int rnd = GlobalRandom.Next(indexList.Count);
                return AirPlayerList[indexList[rnd]].Player.GetWorldPosition();
            }
            public Vector2 GetBeginPointTarget(Vector2 target, int angle)
            {
                Vector2 position = target;
                position.Y = WorldTop;
                position.X += (int)(Math.Tan(angle / 180.0f * Math.PI) * Math.Abs(WorldTop - target.Y));
                return position;
            }
            public void ContinueNapalmStrike()
            {
                Stage++;
                int continuing = 100;
                int bulletPer = 10;
                if (Stage % bulletPer == 0)
                {
                    Area area = GlobalGame.GetCameraArea();
                    Vector2 newPos = new Vector2(GlobalRandom.Next((int)area.Left, (int)area.Right), WorldTop);
                    GlobalGame.CreateObject("WpnMolotovsThrown", newPos);
                }
                if (Stage >= continuing)
                {
                    MinusAmmo();
                }
            }
            public bool CallPinpointStrike(TPlayer player)
            {
                int angle = 0;
                PlayerTeam team = player.Team;
                if (IsHacking(player.Team))
                {
                    team = GetEnemyTeam(player.Team);
                    GlobalGame.RunCommand("MSG PINPOINT STRIKE HAS BEEN HACKED");
                }
                Vector2 target = GetRandomAirEnemy(team, 1, ref angle);
                if (target.X == 0 && target.Y == 0)
                {
                    if (IsHacking(player.Team)) target = GetRandomWorldPoint();
                    else return false;
                }
                if (IsJamming(player.Team))
                {
                    target.X += GlobalRandom.Next(-99, 100);
                    GlobalGame.RunCommand("MSG PINPOINT STRIKE HAS BEEN JAMMED");
                }
                else target.X += GlobalRandom.Next(-12, 13);
                Vector2 position = GetBeginPointTarget(target, angle);
                GlobalGame.SpawnProjectile(ProjectileItem.BAZOOKA, position, (target - position));
                GlobalGame.PlaySound("Explosion", position, 1);
                return true;
            }
            public bool CallAirstrike(TPlayer player)
            {
                int angle = 0;
                PlayerTeam team = player.Team;
                if (IsHacking(player.Team))
                {
                    team = GetEnemyTeam(player.Team);
                    GlobalGame.RunCommand("MSG AIRSTRIKE HAS BEEN HACKED");
                }
                Vector2 target = GetRandomAirEnemy(team, 2, ref angle);
                if (target.X == 0 && target.Y == 0)
                {
                    if (IsHacking(player.Team)) target = GetRandomWorldPoint();
                    else return false;
                }
                if (IsJamming(player.Team))
                {
                    target.X += GlobalRandom.Next(-99, 100);
                    GlobalGame.RunCommand("MSG PINPOINT STRIKE HAS BEEN JAMMED");
                }
                Vector2 position = GetBeginPointTarget(target, angle);
                TargetPosition = target;
                BeginPosition = position;
                return true;
            }
            public void ContinueAirstrike()
            {
                Stage++;
                int continuing = 100;
                int bulletPer = 5;
                int rocketPer = 50;
                if (Stage % bulletPer == 0)
                {
                    Vector2 newPos = TargetPosition;
                    newPos.X += GlobalRandom.Next(-16, 17);
                    GlobalGame.SpawnProjectile(ProjectileItem.SNIPER, BeginPosition, (newPos - BeginPosition));
                }
                if (Stage % rocketPer == 0)
                {
                    Vector2 newPos = TargetPosition;
                    newPos.X += GlobalRandom.Next(-16, 17);
                    GlobalGame.SpawnProjectile(ProjectileItem.BAZOOKA, BeginPosition, (newPos - BeginPosition));

                }
                if (Stage >= continuing)
                {
                    MinusAmmo();
                }
            }
            public void ContinueArtilleryStrike()
            {
                Stage++;
                int continuing = 100;
                int bulletPer = 10;
                if (Stage % bulletPer == 0)
                {
                    Area area = GlobalGame.GetCameraArea();
                    Vector2 newPos = new Vector2(GlobalRandom.Next((int)area.Left, (int)area.Right), WorldTop);
                    GlobalGame.SpawnProjectile(ProjectileItem.GRENADE_LAUNCHER, newPos, new Vector2(0, -1));
                }
                if (Stage >= continuing)
                {
                    MinusAmmo();
                }
            }
            public void ContinueMineStrike()
            {
                Stage++;
                int continuing = 70;
                int bulletPer = 10;
                if (Stage % bulletPer == 0)
                {
                    Area area = GlobalGame.GetCameraArea();
                    Vector2 newPos = new Vector2(GlobalRandom.Next((int)area.Left, (int)area.Right), WorldTop);
                    GlobalGame.CreateObject("WpnMineThrown", newPos);
                }
                if (Stage >= continuing)
                {
                    MinusAmmo();
                }
            }
            public bool CheckAirPlayer(TPlayer player, int id)
            {
                for (int i = 0; i < AirPlayerList.Count; i++)
                {
                    if (AirPlayerList[i].Player != null && (player.Team != AirPlayerList[i].Player.GetTeam() || player.Team == PlayerTeam.Independent) && !AirPlayerList[i].Player.IsDead)
                    {
                        for (int j = 0; j < AirPlayerList[i].StrikeList.Count; j++)
                        {
                            if (AirPlayerList[i].StrikeList[j].Id == id)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            public void CallReinforcement(TPlayer player)
            {
                Area area = GlobalGame.GetCameraArea();
                for (int i = 0; i < PlayerList.Count; i++)
                {
                    TPlayer pl = PlayerList[i];
                    if (pl != null && pl.IsActive() && pl != player && pl.Team == player.Team && !pl.IsAlive())
                    {
                        float x = GlobalRandom.Next((int)(area.Left + area.Width / 5), (int)(area.Right - area.Width / 5));
                        float y = WorldTop + 50;
                        IObject crate = GlobalGame.CreateObject("SupplyCrate00", new Vector2(x, y));
                        IObject platf = GlobalGame.CreateObject("Lift00A", new Vector2(x, y - 10));
                        IObject leftBorder = GlobalGame.CreateObject("Lift00A", new Vector2(x - 10, y), (float)Math.PI / -2);
                        IObject rightBorder = GlobalGame.CreateObject("Lift00A", new Vector2(x + 10, y), (float)Math.PI / 2);
                        leftBorder.SetBodyType(BodyType.Dynamic);
                        rightBorder.SetBodyType(BodyType.Dynamic);
                        IObjectDestroyTargets destroy = (IObjectDestroyTargets)GlobalGame.CreateObject("DestroyTargets", new Vector2(x, y));
                        platf.SetMass(1e-3f);
                        leftBorder.SetMass(1e-3f);
                        rightBorder.SetMass(1e-3f);
                        IObjectWeldJoint joint = (IObjectWeldJoint)GlobalGame.CreateObject("WeldJoint", new Vector2(x, y));
                        joint.AddTargetObject(crate);
                        joint.AddTargetObject(platf);
                        joint.AddTargetObject(rightBorder);
                        joint.AddTargetObject(leftBorder);
                        destroy.AddTriggerDestroyObject(crate);
                        destroy.AddObjectToDestroy(joint);
                        destroy.AddObjectToDestroy(platf);
                        destroy.AddObjectToDestroy(leftBorder);
                        destroy.AddObjectToDestroy(rightBorder);
                        ObjectsToRemove.Add(destroy);
                        ObjectsToRemove.Add(platf);
                        ObjectsToRemove.Add(joint);
                        ObjectsToRemove.Add(leftBorder);
                        ObjectsToRemove.Add(rightBorder);
                        pl.Equipment.Clear();
                        pl.Armor.SetId(0);
                        pl.Revive(100, false, true, x, y);
                        player.AddExp(5, 5);
                    }
                }
            }
            public void PlaceTurret(TPlayer player, int id)
            {
                Vector2 position = player.User.GetPlayer().GetWorldPosition();
                position += new Vector2(player.User.GetPlayer().FacingDirection * 10, 15);
                CreateTurret(id, position, player.User.GetPlayer().FacingDirection, player.Team);
            }
            public void PlaceShieldGenerator(TPlayer player)
            {
                Vector2 position = player.User.GetPlayer().GetWorldPosition();
                position += new Vector2(player.User.GetPlayer().FacingDirection * 10, 15);
                CreateShieldGenerator(50, position, 50, player.Team);
            }

            public void SpawnStreetsweeper(TPlayer player)
            {
                var streetSweeper = Game.CreateObject("Streetsweeper", new Vector2(player.Position.X, WorldTop)) as IObjectStreetsweeper;
                streetSweeper.SetOwnerTeam(player.Team);
                streetSweeper.SetOwnerPlayer(player.User.GetPlayer());
                ObjectsToRemove.Add(streetSweeper);
            }

            public void SpawnDrone(TPlayer player, int id)
            {
                Area area = GlobalGame.GetCameraArea();
                float x = GlobalRandom.Next((int)(area.Left + area.Width / 5), (int)(area.Right - area.Width / 5));
                float y = area.Top + 10;
                CreateTurret(id, new Vector2(x, y), player.User.GetPlayer().FacingDirection, player.Team);
                GlobalGame.PlayEffect("EXP", new Vector2(x, y));
                GlobalGame.PlaySound("Explosion", new Vector2(x, y), 1.0f);
            }
            public void UpdateJetPack(TPlayer player)
            {
                if (CurrentAmmo <= 0)
                {
                    SetId(0);
                    return;
                }
                IPlayer pl = player.User.GetPlayer();
                if (pl == null) return;
                bool velChange = false;
                Vector2 vel = pl.GetLinearVelocity();
                if (pl.IsWalking && !pl.IsDiving && !pl.IsClimbing && !pl.IsManualAiming && Reloading == 0 && CurrentAmmo >= 2)
                {
                    Reloading = 5;
                    velChange = true;
                    vel.Y = 13;
                    CurrentAmmo -= 2;
                    OtherData[0] = 20;
                }
                else if (!pl.IsDiving && !pl.IsFalling && vel.Y < -13)
                {
                    velChange = true;
                    vel.Y = -6.5f;
                    OtherData[0] = 5;
                    CurrentAmmo -= 1;
                }
                if (velChange)
                {
                    if (vel.X != 0) vel.X = Math.Min(3, Math.Abs(vel.X)) * Math.Abs(vel.X) / vel.X;
                    pl.SetLinearVelocity(vel);
                }
                if (OtherData[0] > 0)
                {
                    DrawJetPackEffect(player);
                    OtherData[0]--;
                }
            }
            public void DrawJetPackEffect(TPlayer player)
            {
                IPlayer pl = player.User.GetPlayer();
                Vector2 vel = pl.GetLinearVelocity();
                Vector2 pos = pl.GetWorldPosition();
                if (vel.Y < 0)
                {
                    pos.Y -= 6;
                    GlobalGame.PlayEffect("FIRE", pos + new Vector2(10, 0));
                    GlobalGame.PlayEffect("FIRE", pos - new Vector2(10, 0));
                }
                else
                {
                    pos.Y -= 8;
                    GlobalGame.PlayEffect("FIRE", pos);
                }
                GlobalGame.PlaySound("Flamethrower", pos, 1f);
            }
            public void UpdatePoliceShield(TPlayer player)
            {
                IPlayer pl = player.User.GetPlayer();
                if (pl == null) return;
                Vector2 pos = pl.GetWorldPosition();
                int dir = pl.FacingDirection;
                Vector2 shieldPos = pos + new Vector2(10 * dir, 4);
                Area area = new Area(shieldPos.Y + 4, shieldPos.X - 2, shieldPos.Y, shieldPos.X);
                if (ObjectList.Count > 0)
                {
                    float damageFactor = 0.125f;
                    if (!CheckAreaToCollision(area))
                    {
                        ObjectList[0].SetWorldPosition(pos + new Vector2(10 * dir, 4));
                        ObjectList[1].SetWorldPosition(pos + new Vector2(8 * dir, 17));
                        ObjectList[1].SetAngle(0.3f * dir);
                    }
                    float ch = (OtherData[0] - ObjectList[0].GetHealth()) * damageFactor;
                    OtherData[1] -= ch;
                    ObjectList[0].SetHealth(OtherData[0]);
                    ch = (OtherData[2] - ObjectList[1].GetHealth()) * damageFactor;
                    OtherData[3] -= ch;
                    ObjectList[1].SetHealth(OtherData[2]);
                    if (ObjectList[0].DestructionInitiated || OtherData[1] <= 0 || ObjectList[1].DestructionInitiated || OtherData[3] <= 0)
                    {
                        ObjectList[0].Destroy();
                        ObjectList[1].Destroy();
                        MinusAmmo();
                        ObjectList.Clear();
                        return;
                    }
                }
                if (!pl.IsDead && pl.FacingDirection == OtherData[4] && !pl.IsManualAiming && !pl.IsHipFiring && !pl.IsClimbing && !pl.IsDiving && !pl.IsRolling && !pl.IsStaggering && !pl.IsTakingCover && !pl.IsInMidAir && !pl.IsLayingOnGround && !pl.IsLedgeGrabbing && !pl.IsMeleeAttacking && pl.IsWalking)
                {
                    if (ObjectList.Count == 0 && !CheckAreaToCollision(area))
                    {
                        IObject shield = GlobalGame.CreateObject("MetalRailing00", pos + new Vector2(10 * dir, 4));
                        ObjectList.Add(shield);
                        shield = GlobalGame.CreateObject("MetalRailing00", pos + new Vector2(8 * dir, 17), 0.3f * dir);
                        ObjectList.Add(shield);
                    }
                }
                else
                {
                    if (pl.FacingDirection != OtherData[4])
                    {
                        OtherData[4] = pl.FacingDirection;
                        FastReloading = 10;
                    }
                    if (ObjectList.Count > 0)
                    {
                        ObjectList[0].Remove();
                        ObjectList[1].Remove();
                        ObjectList.Clear();
                    }
                }
            }
        }

        #endregion

        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
