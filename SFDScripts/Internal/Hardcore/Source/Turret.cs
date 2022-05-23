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

        #region Turret Class
        public class TTurret
        {
            public class TTurretWeapon
            {
                public int BulletType;
                public string Sound;
                public int ReloadingTime;
                public int CurrentReloading = 100;
                public int Ammo;
                public int Scatter;
                public bool SuppressiveFire = false;
                public int MaxFireDelay = 0;
                public int MaxBulletCount = 0;
                public int FireDelay = 1;
                public int BulletCount = 0;
                public int Distance = 2000;
                public bool TurretTarget = true;
            }
            public int Id;
            public PlayerTeam Team;
            public List<TTurretWeapon> WeaponList = new List<TTurretWeapon>();
            public List<IObject> OtherObjects = new List<IObject>();
            public List<IObject> DamagedObjects = new List<IObject>();
            public List<float> DamagedObjectHp = new List<float>();
            public List<float> DamagedObjectMaxHp = new List<float>();
            public IObject Hull;
            public IObject MainBlock;
            public IObjectRevoluteJoint MainMotor;
            public IObjectText TextName;
            public IObjectRailJoint RailJoint = null;
            public IObjectTargetObjectJoint TargetObject = null;
            public IObjectRailAttachmentJoint RailAttachment = null;
            public string Name;
            public float RotationSpeed = 1f;
            public float RotationLimit = (float)Math.PI / 3;
            public float MainBlockAngle;
            public float DefaultAngle;
            public float DamageFactor = 0.15f;
            public IObject Target = null;
            public int TargetVision = 3;
            public int LastTargetFinding = 10;
            public int LastPathFinding = 0;
            public bool EnableMovement = false;
            public int PathSize = 0;
            public float Speed = 0;
            public List<Vector2> CurrentPath = new List<Vector2>();
            public bool HackingProtection = false;
            public bool IsProtector = false;
            public bool IsCapturer = false;
            public float DroneMinDistance = 0;
            public int SmokeEffectTime = 0;

            public bool CanFire = false;

            public bool ChangingRoute { get; private set; }

            public TTurret(int id, Vector2 position, int dir, PlayerTeam team)
            {
                Id = id;
                Team = team;
                if (dir < 0)
                {
                    MainBlockAngle = (float)Math.PI / 2;
                    DefaultAngle = (float)Math.PI;
                }
                else
                {
                    MainBlockAngle = (float)Math.PI / 2;
                    DefaultAngle = 0;
                }
                IObject leftLeg = null;
                IObject rightLeg = null;
                IObject hull2 = null;
                IObject hull3 = null;
                IObject hull4 = null;

                if (Id < 4)
                {
                    leftLeg = GlobalGame.CreateObject("Duct00C_D", position + new Vector2(-3, -9), 1.2f);
                    rightLeg = GlobalGame.CreateObject("Duct00C_D", position + new Vector2(3, -9), -1.2f);
                    leftLeg.SetMass(leftLeg.GetMass() * 20);
                    rightLeg.SetMass(rightLeg.GetMass() * 20);
                }
                else if (Id == 4 || Id == 5 || Id == 6 || Id == 7)
                {
                    RotationLimit = 0;
                    Hull = GlobalGame.CreateObject("BgMetal08H", position + new Vector2(-4, 4), (float)Math.PI / 2);
                    hull2 = GlobalGame.CreateObject("BgMetal08H", position + new Vector2(4, 4), 0);
                    hull3 = GlobalGame.CreateObject("BgMetal08H", position + new Vector2(4, -4), -(float)Math.PI / 2);
                    hull4 = GlobalGame.CreateObject("BgMetal08H", position + new Vector2(-4, -4), -(float)Math.PI);
                    hull2.SetBodyType(BodyType.Dynamic);
                    hull3.SetBodyType(BodyType.Dynamic);
                    hull4.SetBodyType(BodyType.Dynamic);
                }
                else if (Id == 6 || Id == 7)
                {
                    RotationLimit = 0;
                    Hull = GlobalGame.CreateObject("BgMetal08H", position + new Vector2(-4, 4), (float)Math.PI / 2);
                    hull2 = GlobalGame.CreateObject("BgMetal08H", position + new Vector2(4, 4), 0);
                    hull3 = GlobalGame.CreateObject("BgMetal08H", position + new Vector2(4, -4), -(float)Math.PI / 2);
                    hull4 = GlobalGame.CreateObject("BgMetal08H", position + new Vector2(-4, -4), -(float)Math.PI);
                    hull2.SetBodyType(BodyType.Dynamic);
                    hull3.SetBodyType(BodyType.Dynamic);
                    hull4.SetBodyType(BodyType.Dynamic);
                }


                if (Id == 6 || Id == 7) MainBlock = GlobalGame.CreateObject("CrabCan00", position, -(float)Math.PI / 2 * dir);
                else MainBlock = GlobalGame.CreateObject("Computer00", position, -(float)Math.PI / 2 * dir);

                MainBlock.SetHealth(1000);

                if (Id >= 4)
                {
                    IObjectAlterCollisionTile collisionDisabler = (IObjectAlterCollisionTile)GlobalGame.CreateObject("AlterCollisionTile", position);
                    collisionDisabler.SetDisableCollisionTargetObjects(true);
                    collisionDisabler.SetDisableProjectileHit(false);
                    collisionDisabler.AddTargetObject(MainBlock);
                    IObject[] platforms = GlobalGame.GetObjectsByName(new string[] { "MetalPlat01A", "Lift00C", "Lift00B", "MetalPlat00G", "Elevator02B", "InvisiblePlatform", "MetalPlat01F" });
                    for (int i = 0; i < platforms.Length; ++i)
                    {
                        collisionDisabler.AddTargetObject(platforms[i]);
                    }
                }
                IObject antenna = GlobalGame.CreateObject("BgAntenna00B", position + new Vector2(-2 * dir, 9));
                antenna.SetBodyType(BodyType.Dynamic);
                antenna.SetMass(0.0000001f);
                IObjectWeldJoint bodyJoint = (IObjectWeldJoint)GlobalGame.CreateObject("WeldJoint", position);
                IObjectWeldJoint hullJoint = (IObjectWeldJoint)GlobalGame.CreateObject("WeldJoint", position);
                if (Id < 4)
                {
                    hullJoint.AddTargetObject(leftLeg);
                    hullJoint.AddTargetObject(rightLeg);
                    OtherObjects.Add(leftLeg);
                    OtherObjects.Add(rightLeg);
                }
                else if (Id == 4 || Id == 5 || Id == 6 || Id == 7)
                {
                    hullJoint.AddTargetObject(Hull);
                    hullJoint.AddTargetObject(hull2);
                    hullJoint.AddTargetObject(hull3);
                    hullJoint.AddTargetObject(hull4);
                    OtherObjects.Add(Hull);
                    OtherObjects.Add(hull2);
                    OtherObjects.Add(hull3);
                    OtherObjects.Add(hull4);
                }
                bodyJoint.AddTargetObject(MainBlock);
                bodyJoint.AddTargetObject(antenna);
                bodyJoint.SetPlatformCollision(WeldJointPlatformCollision.PerObject);
                OtherObjects.Add(antenna);
                OtherObjects.Add(bodyJoint);
                OtherObjects.Add(hullJoint);
                DamagedObjects.Add(MainBlock);
                if (Id == 4 || Id == 5)
                {
                    HackingProtection = true;
                    EnableMovement = true;
                    PathSize = 2;
                    Speed = 3;
                    RotationSpeed = 3f;
                    DamageFactor = 1f;
                    DroneMinDistance = 5 * 8;
                }
                else if (Id == 6)
                {
                    HackingProtection = false;
                    EnableMovement = true;
                    PathSize = 2;
                    Speed = 4;
                    RotationSpeed = 2f;
                    DamageFactor = 1.5f;
                    DroneMinDistance = 4 * 8;
                }
                else if (Id == 7)
                {
                    HackingProtection = false;
                    EnableMovement = true;
                    PathSize = 2;
                    Speed = 4;
                    RotationSpeed = 2f;
                    DamageFactor = 1f;
                    DroneMinDistance = 4 * 8;
                }

                if (id == 0) Name = "Light Turret";
                else if (id == 1) Name = "Rocket Turret";
                else if (id == 2) Name = "Heavy Turret";
                else if (id == 3) Name = "Sniper Turret";
                else if (id == 4) Name = "Assault Drone";
                else if (id == 5) Name = "Fire Drone";
                else if (id == 6) Name = "Tazer Drone";
                else if (id == 7) Name = "Melee Drone";
                if (id == 1 || id == 2)
                { //@0:5B=8F0
                    IObject gun2 = GlobalGame.CreateObject("BgBarberPole00", position + new Vector2(dir, -2), (float)Math.PI / 2);
                    IObject gun1 = GlobalGame.CreateObject("BgBarberPole00", position + new Vector2(dir, 3), (float)Math.PI / 2);
                    gun1.SetBodyType(BodyType.Dynamic);
                    gun2.SetBodyType(BodyType.Dynamic);
                    gun1.SetMass(0.0000001f);
                    gun2.SetMass(0.0000001f);
                    bodyJoint.AddTargetObject(gun1);
                    bodyJoint.AddTargetObject(gun2);
                    OtherObjects.Add(gun1);
                    OtherObjects.Add(gun2);
                    TTurretWeapon weapon = new TTurretWeapon
                    {
                        BulletType = (int)ProjectileItem.BAZOOKA,
                        Sound = "Bazooka",
                        ReloadingTime = 200
                    };
                    if (id == 2) weapon.Ammo = 4;
                    else
                    {
                        weapon.Ammo = 6;
                        weapon.SuppressiveFire = true;
                        weapon.MaxFireDelay = 1;
                        weapon.MaxBulletCount = 1;
                    }
                    weapon.Scatter = 0;
                    WeaponList.Add(weapon);
                }
                if (id == 0 || id == 2 || id == 4)
                { //?C;5<5B		
                    IObject gun00 = GlobalGame.CreateObject("BgPipe02A", position + new Vector2(8 * dir, 0));
                    IObject gun01 = GlobalGame.CreateObject("BgPipe02B", position + new Vector2(14 * dir, 0));
                    IObject gun10 = GlobalGame.CreateObject("BgPipe02A", position + new Vector2(8 * dir, 2));
                    IObject gun11 = GlobalGame.CreateObject("BgPipe02B", position + new Vector2(14 * dir, 2));
                    gun00.SetBodyType(BodyType.Dynamic);
                    gun01.SetBodyType(BodyType.Dynamic);
                    gun10.SetBodyType(BodyType.Dynamic);
                    gun11.SetBodyType(BodyType.Dynamic);
                    gun00.SetMass(0.0000001f);
                    gun01.SetMass(0.0000001f);
                    gun10.SetMass(0.0000001f);
                    gun11.SetMass(0.0000001f);
                    bodyJoint.AddTargetObject(gun00);
                    bodyJoint.AddTargetObject(gun01);
                    bodyJoint.AddTargetObject(gun10);
                    bodyJoint.AddTargetObject(gun11);
                    OtherObjects.Add(gun00);
                    OtherObjects.Add(gun01);
                    OtherObjects.Add(gun10);
                    OtherObjects.Add(gun11);
                    TTurretWeapon weapon = new TTurretWeapon
                    {
                        BulletType = (int)ProjectileItem.UZI,
                        Sound = "AssaultRifle",
                        ReloadingTime = 7,
                        Ammo = 150,
                        Scatter = 2,
                        SuppressiveFire = true,
                        MaxFireDelay = 30,
                        MaxBulletCount = 3
                    };
                    WeaponList.Add(weapon);
                }
                if (id == 3)
                { //A=09?5@:0
                    IObject gun1 = GlobalGame.CreateObject("BgPipe02A", position + new Vector2((dir == -1) ? -14 : 6, 0));
                    IObject gun2 = GlobalGame.CreateObject("BgPipe02B", position + new Vector2(22 * dir, 0));
                    IObject gun3 = GlobalGame.CreateObject("BgPipe02B", position + new Vector2(7 * dir, 2));
                    gun1.SetSizeFactor(new Point(2, 1));
                    gun1.SetBodyType(BodyType.Dynamic);
                    gun2.SetBodyType(BodyType.Dynamic);
                    gun3.SetBodyType(BodyType.Dynamic);
                    gun1.SetMass(0.0000001f);
                    gun2.SetMass(0.0000001f);
                    gun3.SetMass(0.0000001f);
                    bodyJoint.AddTargetObject(gun1);
                    bodyJoint.AddTargetObject(gun2);
                    bodyJoint.AddTargetObject(gun3);
                    OtherObjects.Add(gun1);
                    OtherObjects.Add(gun2);
                    OtherObjects.Add(gun3);
                    TTurretWeapon weapon = new TTurretWeapon
                    {
                        BulletType = (int)ProjectileItem.SNIPER,
                        Sound = "Sniper",
                        ReloadingTime = 150,
                        Ammo = 10,
                        Scatter = 0
                    };
                    WeaponList.Add(weapon);
                }
                if (id == 5)
                { //>3=5<QB
                    IObject gun1 = GlobalGame.CreateObject("BgPipe02A", position + new Vector2((dir == -1) ? -14 : 6, 0));
                    IObject gun2 = GlobalGame.CreateObject("BgPipe02B", position + new Vector2(14 * dir, 0));
                    IObject gun3 = GlobalGame.CreateObject("BgPipe02B", position + new Vector2(7 * dir, 2));
                    IObject gun4 = GlobalGame.CreateObject("BgPipe02B", position + new Vector2(7 * dir, -2));
                    gun1.SetBodyType(BodyType.Dynamic);
                    gun2.SetBodyType(BodyType.Dynamic);
                    gun3.SetBodyType(BodyType.Dynamic);
                    gun4.SetBodyType(BodyType.Dynamic);
                    gun1.SetMass(0.0000001f);
                    gun2.SetMass(0.0000001f);
                    gun3.SetMass(0.0000001f);
                    gun4.SetMass(0.0000001f);
                    bodyJoint.AddTargetObject(gun1);
                    bodyJoint.AddTargetObject(gun2);
                    bodyJoint.AddTargetObject(gun3);
                    bodyJoint.AddTargetObject(gun4);
                    OtherObjects.Add(gun1);
                    OtherObjects.Add(gun2);
                    OtherObjects.Add(gun3);
                    OtherObjects.Add(gun4);
                    TTurretWeapon weapon = new TTurretWeapon
                    {
                        Distance = 150,
                        BulletType = -1,
                        Sound = "Flamethrower",
                        ReloadingTime = 3,
                        Ammo = 300,
                        SuppressiveFire = true,
                        Scatter = 10,
                        TurretTarget = false
                    };
                    WeaponList.Add(weapon);
                }
                if (Id == 6)
                {
                    TTurretWeapon weapon = new TTurretWeapon
                    {
                        Distance = 50,
                        BulletType = -2,
                        Sound = "Splash",
                        ReloadingTime = 150,
                        SuppressiveFire = true,
                        Ammo = 25,
                        TurretTarget = false
                    };
                    WeaponList.Add(weapon);
                }
                if (Id == 7)
                {
                    TTurretWeapon weapon = new TTurretWeapon
                    {
                        Distance = 60,
                        BulletType = -3,
                        Sound = "MeleeSwing",
                        ReloadingTime = 50,
                        SuppressiveFire = true,
                        Ammo = 15,
                        TurretTarget = false
                    };
                    WeaponList.Add(weapon);
                }
                TextName = (IObjectText)GlobalGame.CreateObject("Text", position);
                TextName.SetTextAlignment(TextAlignment.Middle);
                TextName.SetText(Name);
                TextName.SetTextScale(0.8f);
                OtherObjects.Add(TextName);
                MainMotor = (IObjectRevoluteJoint)GlobalGame.CreateObject("RevoluteJoint", position);
                MainMotor.SetTargetObjectB(MainBlock);
                if (Id < 4) MainMotor.SetTargetObjectA(leftLeg);
                else MainMotor.SetTargetObjectA(hull2);
                MainMotor.SetMotorEnabled(true);
                MainMotor.SetMaxMotorTorque(100000);
                MainMotor.SetMotorSpeed(0);
                OtherObjects.Add(MainMotor);
                InitHealth();
            }
            public void InitHealth()
            {
                for (int i = 0; i < DamagedObjects.Count; i++)
                {
                    DamagedObjectHp.Add(DamagedObjects[i].GetHealth());
                    DamagedObjectMaxHp.Add(DamagedObjects[i].GetHealth());
                }
            }
            public bool HaveAmmo()
            {
                for (int i = 0; i < WeaponList.Count; i++)
                {
                    if (WeaponList[i].Ammo > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            public bool CanHitTurret()
            {
                for (int i = 0; i < WeaponList.Count; i++) if (WeaponList[i].TurretTarget) return true;
                return false;
            }
            public int GetMaxDistance()
            {
                int max = 0;
                for (int i = 0; i < WeaponList.Count; i++)
                {
                    if (WeaponList[i].Ammo > 0 && WeaponList[i].Distance > max)
                    {
                        max = WeaponList[i].Distance;
                    }
                }
                return max;
            }



            public void Update()
            {
                if (SmokeEffectTime > 0) SmokeEffectTime--;
                if (MainBlock.GetHealth() <= 5 && SmokeEffectTime == 0)
                {
                    GlobalGame.PlayEffect("TR_S", MainMotor.GetWorldPosition());
                    GlobalGame.PlayEffect("TR_S", MainMotor.GetWorldPosition() + new Vector2(0, 5));
                    SmokeEffectTime = 5;
                }
                TextName.SetWorldPosition(MainMotor.GetWorldPosition() + new Vector2(0, 10));
                string name = Name;
                for (int i = 0; i < WeaponList.Count; i++)
                {
                    name += "[" + WeaponList[i].Ammo + "]";
                }
                TextName.SetText(name);
                if (!HackingProtection && IsJamming(Team))
                {
                    TextName.SetTextColor(Color.White);
                    if (GlobalRandom.Next(100) <= 10)
                    {
                        string line = " ";
                        for (int i = 0; i < 10; i++)
                        {
                            switch (GlobalRandom.Next(5))
                            {
                                case 0: line += "#"; break;
                                case 1: line += "@"; break;
                                case 2: line += "%"; break;
                                case 3: line += "_"; break;
                                case 4: line += "*"; break;
                            }
                        }
                        TextName.SetText(line);
                    }
                }
                else if (Team == PlayerTeam.Independent || (!HackingProtection && IsHacking(Team))) TextName.SetTextColor(Color.White);
                else if (Team == PlayerTeam.Team1) TextName.SetTextColor(new Color(122, 122, 224));
                else if (Team == PlayerTeam.Team2) TextName.SetTextColor(new Color(224, 122, 122));
                else if (Team == PlayerTeam.Team3) TextName.SetTextColor(new Color(112, 224, 122));
                if (UpdateHealth()) return;
                UpdateWeapon();
                if (!HaveAmmo() || (IsJamming(Team) && !HackingProtection)) return;
                if (LastTargetFinding == 0)
                {
                    PlayerTeam team = Team;
                    if (IsHacking(Team) && !HackingProtection) team = PlayerTeam.Independent;
                    List<IObject> targetList;
                    if (IsProtector) targetList = GetFriendList(team, MainMotor.GetWorldPosition(), GetMaxDistance());
                    else targetList = GetTargetList(team, MainMotor.GetWorldPosition(), GetMaxDistance(), CanHitTurret(), HackingProtection);
                    Target = null;
                    TargetVision = 3;
                    for (int i = 0; i < targetList.Count; i++)
                    {
                        IObject obj = targetList[i];
                        if (obj == MainMotor) continue;
                        if (RotationLimit > 0)
                        {
                            float angle = TwoPointAngle(MainMotor.GetWorldPosition(), obj.GetWorldPosition());
                            if (Math.Abs(GetAngleDistance(angle, DefaultAngle + MainMotor.GetAngle())) > RotationLimit) continue;
                        }
                        int trace = TraceToObject(obj);
                        if (trace < TargetVision)
                        {
                            TargetVision = trace;
                            Target = obj;
                            break;
                        }
                    }
                    LastTargetFinding = 10;
                }
                else
                {
                    LastTargetFinding--;
                }
                if (EnableMovement)
                {
                    if (Target != null && (MainMotor.GetWorldPosition() - Target.GetWorldPosition()).Length() <= DroneMinDistance && CanFire)
                    {
                        CurrentPath.Clear();
                        StopMovement();
                    }
                    else if (LastPathFinding == 0)
                    {
                        CurrentPath.Clear();
                        StopMovement();
                        FindPathToTarget(randomTarget: ChangingRoute);
                        LastPathFinding = 200;
                    }
                    if (LastPathFinding > 0)
                    {
                        LastPathFinding--;
                    }
                    Movement();
                }
                if (Target == null)
                {
                    MainMotor.SetMotorSpeed(0);
                    return;
                }
                Vector2 targetPos = Target.GetWorldPosition();
                if (IsPlayer(Target.Name)) targetPos = GetPlayerCenter((IPlayer)Target);
                float targetAngle = TwoPointAngle(MainMotor.GetWorldPosition(), targetPos);
                targetAngle = GetAngleDistance(targetAngle, MainBlockAngle + MainBlock.GetAngle());


                if (Math.Abs(targetAngle) > 0.01f * RotationSpeed)
                {
                    if (targetAngle > 0) MainMotor.SetMotorSpeed(RotationSpeed);
                    else MainMotor.SetMotorSpeed(-RotationSpeed);
                }
                else
                {
                    MainMotor.SetMotorSpeed(0);
                    MainBlock.SetAngle(MainBlock.GetAngle() + targetAngle);

                    var startPos = MainMotor.GetWorldPosition();
                    var endPos = targetPos;
                    Game.DrawLine(startPos, endPos, Color.Red);
                    var rci = new RayCastInput()
                    {
                        // doesn't include the objects that are overlaping the source of the raycast (the drone)
                        IncludeOverlap = true,
                        // only look at the closest hit
                        ClosestHitOnly = false,
                        // mark as hit the objects that projectiles hit
                        ProjectileHit = RayCastFilterMode.True,
                        // mark as hit the objects that absorb the projectile
                        AbsorbProjectile = RayCastFilterMode.Any
                    };

                    var raycastResult = Game.RayCast(startPos, endPos, rci);

                    foreach (var result in raycastResult)
                    {
                        Game.DrawCircle(result.Position, 1f, Color.Yellow);
                        Game.DrawLine(result.Position, result.Position + result.Normal * 5f, Color.Yellow);
                        Game.DrawArea(result.HitObject.GetAABB(), Color.Yellow);
                        Game.DrawText((MainMotor.GetWorldPosition() - Target.GetWorldPosition()).Length().ToString(), result.Position, Color.Green);
                        var raycastResultPlayersCount = raycastResult.Where(r => r.IsPlayer).Count();

                        if (raycastResult.Length > 2 + raycastResultPlayersCount && (MainMotor.GetWorldPosition() - Target.GetWorldPosition()).Length() <= 50)
                        {
                            // if there's more than 2 objects between drone and player, don't shoot
                            // and choose another target
                            ChangingRoute = true;
                            IPlayer ply = Target as IPlayer;
                            if (ply == null) continue;
                            var originalTarget = Target;
                            var possibleTargets = PlayerList.Where(pl => pl.Team != Team && !pl.Name.Equals(ply.Name));
                            var i = GlobalRandom.Next(0, possibleTargets.Count());
                            var player = possibleTargets.ElementAt(i);
                            Target = player.Body;
                            DebugLogger.DebugOnlyDialogLog("Target changed from " + ply.Name + " to " + player.Name);
                            break;
                        }
                        else if (result.Hit &&
                            !(raycastResult.Length > 2 + raycastResultPlayersCount) &&
                          (result.IsPlayer ||
                              result.HitObject.Name.IndexOf("crab", StringComparison.OrdinalIgnoreCase) >= 0 ||
                              result.HitObject.Name.IndexOf("computer", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            CanFire = true;
                            ChangingRoute = false;
                            // no obstacles in the way, fire!
                            Fire(TargetVision);

                        }
                        else
                        {
                            CanFire = false;
                        }

                        if (result.Fraction < 0.3f && result.HitObject.Name.IndexOf("glass", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            result.HitObject.Destroy();
                        }
                    }

                }
            }
            public void FindPathToTarget(bool randomTarget = false)
            {
                PlayerTeam team = Team;
                if (IsHacking(Team)) team = PlayerTeam.Independent;
                List<IObject> targetList = GetTargetList(team, MainMotor.GetWorldPosition(), 2000, CanHitTurret(), HackingProtection);
                IObject nearestTarget = null;
                float distance = 2000;
                for (int i = 0; i < targetList.Count; ++i)
                {
                    float dist = (targetList[i].GetWorldPosition() - MainMotor.GetWorldPosition()).Length();
                    if (dist < distance)
                    {
                        nearestTarget = targetList[i];
                        distance = dist;
                    }
                }
                if (randomTarget)
                {
                    var index = GlobalRandom.Next(0, targetList.Count);
                    nearestTarget = targetList[index];
                }
                if (nearestTarget == null) return;
                Vector2 targetCell = GetNearestDroneMapCell(nearestTarget.GetWorldPosition(), PathSize);
                Vector2 currentCell = MainMotor.GetWorldPosition() - DroneAreaBegin;
                currentCell.X = (int)currentCell.X / 8;
                currentCell.Y = (int)currentCell.Y / 8;
                CurrentPath = FindDronePath(currentCell, targetCell, PathSize);
            }
            public void Movement()
            {
                if (!EnableMovement || CurrentPath.Count == 0)
                {
                    StopMovement();
                    return;
                }
                Vector2 position = MainMotor.GetWorldPosition();
                if ((position - CurrentPath[CurrentPath.Count - 1]).Length() <= 1 || RailJoint == null)
                {
                    CurrentPath.RemoveAt(CurrentPath.Count - 1);
                    if (CurrentPath.Count == 0)
                    {
                        StopMovement();
                        return;
                    }
                    else StopMovement(false);
                    RailJoint = (IObjectRailJoint)GlobalGame.CreateObject("RailJoint", position);
                    TargetObject = (IObjectTargetObjectJoint)GlobalGame.CreateObject("TargetObjectJoint", CurrentPath[CurrentPath.Count - 1]);
                    RailAttachment = (IObjectRailAttachmentJoint)GlobalGame.CreateObject("RailAttachmentJoint", position);
                    RailJoint.SetTargetObjectJoint(TargetObject);
                    RailAttachment.SetRailJoint(RailJoint);
                    RailAttachment.SetTargetObject(Hull);
                    RailAttachment.SetMotorEnabled(true);
                    RailAttachment.SetMaxMotorTorque(10);
                    RailAttachment.SetMotorSpeed(Speed);
                    Hull.SetBodyType(BodyType.Dynamic);
                }
            }
            public void StopMovement(bool makeStatic = true)
            {
                if (RailJoint == null) return;
                RailAttachment.SetTargetObject(null);
                RailAttachment.SetRailJoint(null);
                if (makeStatic) Hull.SetBodyType(BodyType.Static);
                RailJoint.Remove();
                TargetObject.Remove();
                RailAttachment.Remove();
                RailJoint = null;
            }
            public bool UpdateHealth()
            {
                for (int i = 0; i < DamagedObjects.Count; i++)
                {
                    if (DamagedObjects[i] == null)
                    {
                        Destroy();
                        return true;
                    }
                    float ch = (DamagedObjectMaxHp[i] - DamagedObjects[i].GetHealth()) * DamageFactor;
                    DamagedObjectHp[i] -= ch;
                    DamagedObjects[i].SetHealth(DamagedObjectMaxHp[i]);

                    Game.DrawText(DamagedObjectHp[i].ToString(), MainMotor.GetWorldPosition());
                    if (DamagedObjectHp[i] <= 0)
                    {
                        Destroy();
                        return true;
                    }
                }
                return false;
            }
            public void Destroy()
            {
                StopMovement();
                GlobalGame.TriggerExplosion(MainMotor.GetWorldPosition());
                for (int i = 0; i < DamagedObjects.Count; i++)
                {
                    DamagedObjects[i].Destroy();
                }
                for (int i = 0; i < OtherObjects.Count; i++)
                {
                    OtherObjects[i].Destroy();
                }
                DamagedObjects.Clear();
                OtherObjects.Clear();
            }
            public void Remove()
            {
                for (int i = 0; i < DamagedObjects.Count; i++)
                {
                    DamagedObjects[i].Remove();
                }
                for (int i = 0; i < OtherObjects.Count; i++)
                {
                    OtherObjects[i].Remove();
                }
            }
            public void UpdateWeapon()
            {
                for (int i = 0; i < WeaponList.Count; i++)
                {
                    if (WeaponList[i].CurrentReloading > 0) WeaponList[i].CurrentReloading--;
                    if (WeaponList[i].FireDelay > 0)
                    {
                        WeaponList[i].FireDelay--;
                        if (WeaponList[i].FireDelay == 0) WeaponList[i].BulletCount = WeaponList[i].MaxBulletCount;
                    }
                }
            }
            public void Fire(int type)
            {
                float dist = 2000;
                if (Target != null) dist = (Target.GetWorldPosition() - MainMotor.GetWorldPosition()).Length();
                for (int i = 0; i < WeaponList.Count; i++)
                {
                    if (WeaponList[i].CurrentReloading == 0 && WeaponList[i].Ammo > 0 && dist <= WeaponList[i].Distance && (type < 2 || (type == 2 && WeaponList[i].SuppressiveFire && WeaponList[i].BulletCount > 0)))
                    {
                        WeaponList[i].Ammo--;
                        if (type == 2)
                        {
                            WeaponList[i].BulletCount--;
                            if (WeaponList[i].BulletCount == 0)
                            {
                                WeaponList[i].FireDelay = WeaponList[i].MaxFireDelay;
                            }
                        }
                        WeaponList[i].CurrentReloading = WeaponList[i].ReloadingTime;
                        CreateProjectile(WeaponList[i]);
                    }
                }

            }
            public void CreateProjectile(TTurretWeapon weapon)
            {
                float angle = MainBlock.GetAngle() + MainBlockAngle + GlobalRandom.Next(-weapon.Scatter, weapon.Scatter + 1) / 180.0f * (float)Math.PI;
                Vector2 pos = MainMotor.GetWorldPosition();
                Vector2 dir = new Vector2((float)Math.Cos(angle) * 10, (float)Math.Sin(angle) * 10);
                if (weapon.BulletType >= 0)
                {
                    GlobalGame.SpawnProjectile((ProjectileItem)weapon.BulletType, pos + dir, dir);
                }
                else if (weapon.BulletType == -1)
                {
                    GlobalGame.SpawnFireNode(pos + dir, dir * 2, FireNodeType.Flamethrower);
                }
                else if (weapon.BulletType == -2)
                {
                    ElectricExplosion(MainMotor.GetWorldPosition(), 20, 50);
                }
                else if (weapon.BulletType == -3)
                {
                    ForceImpulse(MainMotor.GetWorldPosition(), 10, 50);
                }
                GlobalGame.PlaySound(weapon.Sound, MainMotor.GetWorldPosition(), 1.0f);
            }


            private void ForceImpulse(Vector2 position, int damage, int range)
            {
                for (int i = 0; i < PlayerList.Count; i++)
                {
                    var player = PlayerList[i];
                    if (player.User.GetPlayer() == null) continue;
                    if (player.User.GetTeam() == Team) continue;
                    float dist = (player.Position - position).Length();
                    if (dist <= range)
                    {
                        Vector2 vel = player.User.GetPlayer().GetLinearVelocity() * 10 + new Vector2(MainMotor.GetFaceDirection() * 10, 10);
                        float mass = 1f;
                        player.User.GetPlayer().SetLinearVelocity(vel / mass);
                        PlayerList[i].Hp -= damage;
                        CreateEffect(PlayerList[i].User.GetPlayer(), "STM", 10, 10);
                    }
                }
            }



            public int TraceToObject(IObject obj)
            {
                float angle = TwoPointAngle(MainMotor.GetWorldPosition(), obj.GetWorldPosition());
                Vector2 tracePoint = MainMotor.GetWorldPosition();
                tracePoint.X += (float)Math.Cos(angle) * 20;
                tracePoint.Y += (float)Math.Sin(angle) * 20;
                PlayerTeam team = Team;
                if (IsHacking(Team)) team = PlayerTeam.Independent;
                Vector2 targetPos = obj.GetWorldPosition();
                if (IsPlayer(obj.Name)) targetPos = GetPlayerCenter((IPlayer)obj);
                return TracePath(tracePoint, targetPos, team);
            }
        }

        public static void CreateTurret(int id, Vector2 position, int dir, PlayerTeam team)
        {
            TTurret turret = new TTurret(id, position, dir, team);
            TurretList.Add(turret);
        }

        #endregion

        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
