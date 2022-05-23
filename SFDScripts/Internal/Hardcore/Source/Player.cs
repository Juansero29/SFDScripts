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

        #region Player Class
        public class TPlayer
        {
            //global values
            public int Level = 0;
            public float CurrentExp = 0;
            //values
            public float ProjectileDamageFactor = 1;
            public float FallDamageFactor = 1;
            public float MeleeDamageFactor = 1;
            public float ExplosionDamageFactor = 1;
            public float FireDamageFactor = 1;
            public int JumpCount = 0;
            public float MeleeDamageTaken = 0;
            public float ProjectileDamageTaken = 0;
            public float ExplosionDamageTaken = 0;
            public float FallDamageTaken = 0;
            public int BlocksCount = 0;
            public float FireDamageTaken = 0;
            public bool Bleeding = false;
            public int StartBleedingProjectile;
            public int StartBleedingMelee;
            public float Hp = 100;
            public int DyingChance;
            public int AliveChance;
            public int OvercomeChance;
            public int DyingHealth;
            public int StableHealth;
            public int OvercomeHealth;
            public int ReviveHealth;
            public float DyingSpeed;
            public float OvercomeSpeed;
            public int WeaponBreakingChance;
            public int WeaponExplosionChance;
            public int Status = 0; //0 - 682>9 //1 - C<8@05B //2 - AB018;878@>20= //3 - ?@52>7<>305B 
            public bool IsSlow = false;
            public int InSmoke = 0;
            public bool IsAdrenaline = false;
            public float DelayedDamage = 0;
            public float AdrenalineDamageFactor = 0.5f;
            public float DamageDelaySpeed = 0.5f;
            public TCustomArmor Armor = new TCustomArmor();
            public List<TCustomEquipment> Equipment = new List<TCustomEquipment>();
            public int CurrentEquipment = 0;
            public int StunTime = 0;
            //system
            public IUser User;
            public PlayerTeam Team;
            public IObjectText StatusDisplay;
            public float DiveHeight = 0;
            public bool IsDiving = false;
            public int DiveTime = 0;
            public float[] ExpSource = { 0, 0, 0, 0, 0, 0 };
            public bool IsNewLevel = false;
            public int SlowTimer = 0;
            public int BleedingEffectTimer = 0;
            public int SlowEffectTimer = 0;
            public bool ActiveStatus = true;
            public Vector2 Position;
            public string Name;
            public IPlayer Body;

            //weapon
            public TWeapon PrimaryWeapon = null;
            public TWeapon SecondaryWeapon = null;
            public TWeapon ThrownWeapon = null;

            public bool HasExtraHeavyAmmo { get; internal set; }

            //methods
            public TPlayer(IUser user)
            {
                User = user;
                Body = user.GetPlayer();
                Name = User.Name;
                Team = Body.GetTeam();
                StatusDisplay = (IObjectText)GlobalGame.CreateObject("Text");
                StatusDisplay.SetTextAlignment(TextAlignment.Middle);
            }
            public void UpdateActiveStatus()
            {
                if (User.IsRemoved)
                {
                    ActiveStatus = false;
                    return;
                }
                IUser[] users = GlobalGame.GetActiveUsers();
                for (int i = 0; i < users.Length; i++)
                {
                    if (users[i].Name == User.Name)
                    {
                        return;
                    }
                }
                ActiveStatus = false;
            }

            public bool IsActive()
            {
                if (!ActiveStatus) return false;
                UpdateActiveStatus();
                return ActiveStatus;
            }
            public bool CanRevive()
            {
                IPlayer pl = User.GetPlayer();
                return pl != null && Status > 0;
            }
            public bool IsAlive()
            {
                IPlayer pl = User.GetPlayer();
                return pl != null && (!pl.IsDead || Status == 3 || Status == 4);
            }
            public void AddExp(float value, int type)
            {
                value *= XPBonus;
                if (Name.Contains("New Player") || Name.Contains("Unnamed"))
                {
                    Level = 0;
                    CurrentExp = 0;
                    return;
                }
                ExpSource[type] += value;
                CurrentExp += value;
                while (Level + 1 < LevelList.Count && LevelList[Level + 1].NeedExp <= CurrentExp)
                {
                    Level++;
                    CurrentExp -= LevelList[Level].NeedExp;
                    IsNewLevel = true;
                }
            }
            public void Respawn(bool full = true)
            {
                JumpCount = 0;
                MeleeDamageTaken = 0;
                ProjectileDamageTaken = 0;
                ExplosionDamageTaken = 0;
                FallDamageTaken = 0;
                BlocksCount = 0;
                FireDamageTaken = 0;
                DelayedDamage = 0;
                Bleeding = false;
                Status = 0;
                IsDiving = false;
                StatusDisplay.SetText("");
                PrimaryWeapon = null;
                SecondaryWeapon = null;
                ThrownWeapon = null;
                InSmoke = 0;
                StunTime = 0;
                if (full)
                {
                    Equipment.Clear();
                    ProjectileDamageFactor = 5.75f;
                    FallDamageFactor = 3.5f;
                    MeleeDamageFactor = 2f;
                    ExplosionDamageFactor = 7.5f;
                    FireDamageFactor = 3;
                    Hp = 100;
                    DyingChance = 75;
                    AliveChance = 50;
                    OvercomeChance = 25;
                    WeaponBreakingChance = 15;
                    WeaponExplosionChance = 50;
                    IsSlow = false;
                    StartBleedingProjectile = 15;
                    StartBleedingMelee = 20;
                    DyingHealth = 5;
                    StableHealth = 10;
                    OvercomeHealth = 20;
                    ReviveHealth = 40;
                    DyingSpeed = 0.007f;
                    OvercomeSpeed = 0.1f;
                    IsAdrenaline = false;
                    AdrenalineDamageFactor = 0.5f;
                    DamageDelaySpeed = 0.5f;
                }
            }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (!(obj is TPlayer)) return false;
                var other = obj as TPlayer;
                return User.Name.Equals(other.User.Name);
            }

            public override int GetHashCode()
            {
                return unchecked(User.Name.GetHashCode() * 17);
            }

            public IProfile GetSkin()
            {
                string skinName = "";
                if (Team == PlayerTeam.Team1) skinName += "Blue";
                else if (Team == PlayerTeam.Team2) skinName += "Red";
                if (Armor.Id == 1) skinName += "Light";
                else if (Armor.Id == 2) skinName += "Fire";
                else if (Armor.Id == 3) skinName += "Suicide";
                else if (Armor.Id == 4) skinName += "Jammer";
                else if (Armor.Id == 5) skinName += "Bomb";
                else if (Armor.Id == 6) skinName += "Heavy";
                else if (Armor.Id == 7) skinName += "Kevlar";
                return ((IObjectPlayerProfileInfo)GlobalGame.GetSingleObjectByCustomId(skinName)).GetProfile();

            }
            public void SetTeam(PlayerTeam team)
            {
                Team = team;
                IPlayer pl = User.GetPlayer();
                if (pl != null) pl.SetTeam(team);
            }
            public void OnPlayerCreated()
            {
                IPlayer pl = User.GetPlayer();
                PlayerModifiers mods = pl.GetModifiers();
                mods.ProjectileCritChanceTakenModifier = 0;
                mods.ProjectileCritChanceDealtModifier = 0;
                mods.ProjectileDamageTakenModifier = ProjectileDamageFactor * Armor.ProjectileDamageFactor;
                mods.ExplosionDamageTakenModifier = ExplosionDamageFactor * Armor.ExplosionDamageFactor;
                mods.FireDamageTakenModifier = FireDamageFactor * Armor.FireDamageFactor;
                mods.MeleeDamageTakenModifier = MeleeDamageFactor * Armor.MeleeDamageFactor;
                mods.ImpactDamageTakenModifier = FallDamageFactor * Armor.FallDamageFactor;
                mods.MaxHealth = 10000;
                pl.SetModifiers(mods);
            }

            public void Revive(float hp = 100, bool bleeding = false, bool withPos = false, float x = 0, float y = 0)
            {
                Respawn(false);
                Bleeding = bleeding;
                IPlayer pl = User.GetPlayer();
                Vector2 position;
                if (!withPos) position = pl.GetWorldPosition();
                else position = new Vector2(x, y);
                if (pl != null) pl.Remove();
                pl = GlobalGame.CreatePlayer(position);

                if (KeepPlayersSkins)
                {
                    pl.SetProfile(pl.GetUser().GetProfile());
                }
                else
                {
                    pl.SetProfile(GetSkin());
                }
                pl.SetUser(User);
                pl.SetTeam(Team);
                //pl.SetStatusBarsVisible(false);
                Hp = hp;
                OnPlayerCreated();
            }
            public void AddEquipment(int id, int type)
            {
                IPlayer player = User.GetPlayer();
                if (player != null && !player.IsDead)
                {
                    if (type == 6)
                    {
                        if (id != 0) Equipment.Add(new TCustomEquipment(id));
                    }
                    else if (type == 5)
                    {
                        Armor.SetId(id);
                    }
                    else if (type == 4)
                    {
                        ApplyWeaponMod(this, id);
                    }
                    else if (type == 3)
                    {
                        ApplyThrownWeapon(this, id);
                    }
                    else
                    {
                        ApplyWeapon(this, (WeaponItem)id);
                    }
                }
                WeaponTrackingUpdate(true);
            }
            public void ReloadEquipment()
            {
                for (int i = 0; i < Equipment.Count; i++)
                {
                    Equipment[i].Reload(this);
                }
            }
            public void Start()
            {
                IPlayer player = User.GetPlayer();
                if (player != null && !player.IsDead)
                {
                    player.SetInputEnabled(true);
                }
            }
            public void Stop()
            {
                IPlayer player = User.GetPlayer();
                if (player != null && !player.IsDead)
                {
                    player.SetInputEnabled(false);
                }
                StatusDisplay.SetText("");
            }
            public string Save()
            {
                string data = "";
                data += FormatName(Name);
                data += ":" + Level.ToString() + ":" + ((int)CurrentExp).ToString() + ":";
                return data;
            }
            public void BreakWeapon(bool isExplosion)
            {
                IPlayer pl = User.GetPlayer();
                if (pl != null && !pl.IsDead)
                {
                    if (pl.CurrentWeaponDrawn == WeaponItemType.Rifle)
                    {
                        if (GlobalRandom.Next(0, 100) < (int)(WeaponBreakingChance * Armor.BreakWeaponFactor))
                        {
                            Vector2 pos = pl.GetWorldPosition();
                            RifleWeaponItem weapon = pl.CurrentPrimaryWeapon;
                            if ((weapon.WeaponItem == WeaponItem.BAZOOKA || weapon.WeaponItem == WeaponItem.GRENADE_LAUNCHER
                                || weapon.WeaponItem == WeaponItem.FLAMETHROWER) && weapon.CurrentAmmo > 0 && (GlobalRandom.Next(0, 100) < WeaponExplosionChance || isExplosion))
                            {
                                GlobalGame.TriggerExplosion(pos);
                            }
                            pl.RemoveWeaponItemType(WeaponItemType.Rifle);
                            GlobalGame.PlayEffect("CFTXT", pl.GetWorldPosition(), "WEAPON BROKEN");
                            GlobalGame.CreateObject("MetalDebris00A", pos, (float)rnd.NextDouble());
                            pos.X += 5;
                            GlobalGame.CreateObject("MetalDebris00B", pos, (float)rnd.NextDouble());
                            pos.X -= 10;
                            GlobalGame.CreateObject("MetalDebris00C", pos, (float)rnd.NextDouble());
                        }
                    }
                }
            }
            public void OnDead()
            {
                AddTeamExp(5, 0, GetEnemyTeam(Team), true);
                IPlayer pl = User.GetPlayer();
                if (pl != null)
                {
                    if (Armor.SuicideMine)
                    {
                        GlobalGame.TriggerExplosion(Position);
                    }
                }
            }
            public void Update()
            {
                if (!IsActive()) return;
                for (int i = 0; i < Equipment.Count; i++)
                {
                    if (Equipment[i].Id == 0)
                    {
                        Equipment.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        Equipment[i].Update(this);
                    }
                }
                IPlayer pl = null;
                if (User != null) pl = User.GetPlayer();
                if (pl != null && (pl.RemovalInitiated || pl.IsRemoved)) pl = null;
                if (pl != null && pl.GetProfile().Name == "CPU")
                {
                    pl.Remove();
                    ActiveStatus = false;
                    return;
                }
                if (pl != null)
                {
                    Position = pl.GetWorldPosition();
                    WeaponTrackingUpdate(false);
                    var heavyWeapon = pl.CurrentPrimaryWeapon.WeaponItem;
                    if (ExtraHeavyAmmoWeapon.Contains(heavyWeapon) && pl.CurrentPrimaryWeapon.CurrentAmmo < (pl.CurrentPrimaryWeapon.MaxTotalAmmo / 2) && HasExtraHeavyAmmo)
                    {
                        pl.GiveWeaponItem(heavyWeapon);
                        HasExtraHeavyAmmo = false;
                    }
                }
                if (pl != null && Status >= 0)
                {
                    if (Status == 0)
                    {
                        StatusDisplay.SetWorldPosition(Position + new Vector2(0, 37));
                    }
                    else
                    {
                        StatusDisplay.SetWorldPosition(Position + new Vector2(0, 15));
                    }
                    pl.SetNametagVisible(InSmoke <= 0);
                    bool wasBleeding = Bleeding;
                    IPlayerStatistics stat = pl.Statistics;
                    float lastHp = Hp;
                    if (ProjectileDamageTaken < stat.TotalProjectileDamageTaken)
                    {
                        float ch = stat.TotalProjectileDamageTaken - ProjectileDamageTaken;
                        ProjectileDamageTaken = stat.TotalProjectileDamageTaken;
                        //ch *= ProjectileDamageFactor * Armor.ProjectileDamageFactor;
                        if (InSmoke > 0) ch *= 0.25f;
                        if (ch < Armor.MaxProjectileDamageCut && ch > Armor.MaxProjectileDamage) ch = Armor.MaxProjectileDamage;
                        if (ch >= StartBleedingProjectile) Bleeding = true;
                        Hp -= ch;
                        if (InSmoke <= 0) BreakWeapon(false);

                    }
                    if (MeleeDamageTaken < stat.TotalMeleeDamageTaken)
                    {
                        float ch = stat.TotalMeleeDamageTaken - MeleeDamageTaken;
                        MeleeDamageTaken = stat.TotalMeleeDamageTaken;
                        //ch *= MeleeDamageFactor * Armor.MeleeDamageFactor;
                        if (ch > Armor.MaxMeleeDamage) ch = Armor.MaxMeleeDamage;
                        if (ch >= StartBleedingMelee) Bleeding = true;
                        Hp -= ch;
                    }
                    if (ExplosionDamageTaken < stat.TotalExplosionDamageTaken)
                    {
                        float ch = stat.TotalExplosionDamageTaken - ExplosionDamageTaken;
                        ExplosionDamageTaken = stat.TotalExplosionDamageTaken;
                        //ch *= ExplosionDamageFactor * Armor.ExplosionDamageFactor;
                        if (ch >= StartBleedingProjectile) Bleeding = true;
                        Hp -= ch;
                        BreakWeapon(true);
                    }
                    if (FallDamageTaken < stat.TotalFallDamageTaken)
                    {
                        float ch = stat.TotalFallDamageTaken - FallDamageTaken;
                        FallDamageTaken = stat.TotalFallDamageTaken;
                        //ch *= FallDamageFactor * Armor.FallDamageFactor;
                        Hp -= ch;
                    }
                    if (Armor.FireProtect)
                    {
                        if (pl.IsBurning) pl.ClearFire();
                    }
                    else
                    {
                        if (pl.IsBurning) pl.SetMaxFire();
                        if (FireDamageTaken < stat.TotalFireDamageTaken)
                        {
                            float ch = stat.TotalFireDamageTaken - FireDamageTaken;
                            FireDamageTaken = stat.TotalFireDamageTaken;
                            //ch *= FireDamageFactor * Armor.FireDamageFactor;
                            Hp -= ch;
                        }
                    }
                    if (Bleeding)
                    {
                        if (!wasBleeding && Hp > 0)
                        {
                            GlobalGame.PlayEffect("CFTXT", pl.GetWorldPosition(), "BLEEDING");
                        }
                        if (pl.IsSprinting || pl.IsRolling || pl.IsDiving || pl.IsClimbing || pl.IsMeleeAttacking || pl.IsKicking || pl.IsJumpKicking || pl.IsJumpAttacking)
                        {
                            Hp -= HardBleeding;
                        }
                        else
                        {
                            Hp -= EasyBleeding;
                        }
                        if (JumpCount < stat.TotalJumps)
                        {
                            int ch = stat.TotalJumps - JumpCount;
                            Hp -= ch * JumpBleeding;
                        }
                        if (BleedingEffectTimer == 0)
                        {
                            Vector2 effPos = pl.GetWorldPosition();
                            effPos.Y += 6;
                            GlobalGame.PlayEffect("BLD", effPos);
                            BleedingEffectTimer = BleedingEffectPeriod;
                        }
                        if (BleedingEffectTimer > 0) BleedingEffectTimer--;
                    }
                    if (IsDiving && !pl.IsDiving)
                    {
                        IsDiving = false;
                        if (DiveTime >= 45)
                        {
                            float diff = DiveHeight - pl.GetWorldPosition().Y;
                            diff -= MinDivingHeight * DivingDamageFactor * FallDamageFactor;
                            if (diff > 0) Hp -= diff;
                        }
                    }
                    else if (!IsDiving && pl.IsDiving)
                    {
                        IsDiving = true;
                        DiveTime = 0;
                        DiveHeight = pl.GetWorldPosition().Y;
                    }
                    else if (IsDiving && pl.IsDiving)
                    {
                        DiveTime++;
                    }
                    if (IsAdrenaline)
                    {
                        float ch = (lastHp - Hp) * AdrenalineDamageFactor;
                        DelayedDamage += (lastHp - Hp) - ch;
                        Hp = lastHp - ch;
                    }
                    else if (DelayedDamage > 0)
                    {
                        if (DamageDelaySpeed > DelayedDamage)
                        {
                            Hp -= DelayedDamage;
                            DelayedDamage = 0;
                        }
                        else
                        {
                            Hp -= DamageDelaySpeed;
                            DelayedDamage -= DamageDelaySpeed;
                        }
                    }
                    WeaponItem heavyWeapon = pl.CurrentPrimaryWeapon.WeaponItem;
                    if (heavyWeapon == WeaponItem.M60 || heavyWeapon == WeaponItem.GRENADE_LAUNCHER || heavyWeapon == WeaponItem.BAZOOKA || heavyWeapon == WeaponItem.SNIPER || Armor.Heavy)
                    {
                        IsSlow = true;
                    }



                    else IsSlow = false;
                    if (IsSlow)
                    {
                        PlayerModifiers mods = pl.GetModifiers();
                        mods.MaxEnergy = 0;
                        mods.MeleeStunImmunity = 1;
                        mods.EnergyRechargeModifier = 0;
                        if (Armor.Heavy) mods.SizeModifier = 1.15f;
                        pl.SetModifiers(mods);
                    }
                    if (IsSlow && ((heavyWeapon == WeaponItem.M60 || heavyWeapon == WeaponItem.GRENADE_LAUNCHER || heavyWeapon == WeaponItem.BAZOOKA || heavyWeapon == WeaponItem.SNIPER) && Armor.Heavy))
                    {

                        PlayerModifiers mods = pl.GetModifiers();
                        mods.MaxEnergy = 0;
                        mods.MeleeStunImmunity = 1;
                        mods.EnergyRechargeModifier = 0;
                        mods.RunSpeedModifier = 0.5f;
                        mods.MeleeForceModifier = 2f;
                        mods.SizeModifier = 2.6f;

                        pl.SetModifiers(mods);
                        if (pl.IsRolling || pl.IsDiving)
                        {
                            Vector2 vel = pl.GetLinearVelocity() / 3;
                            vel.Y = 0;
                            pl.SetWorldPosition(Position - vel);
                            if (SlowEffectTimer == 0) SlowEffectTimer = -1;
                        }
                        if (SlowTimer == 0 && pl.IsSprinting)
                        {
                            SlowTimer = 4;
                            if (SlowEffectTimer == 0) SlowEffectTimer = -1;
                        }
                        if (SlowTimer <= 2)
                        {
                            pl.SetInputEnabled(true);
                        }
                        else
                        {
                            pl.SetInputEnabled(false);
                        }
                        if (SlowEffectTimer == -1)
                        {
                            GlobalGame.PlayEffect("CFTXT", pl.GetWorldPosition(), "TOO HEAVY");
                            SlowEffectTimer = 100;
                        }
                        if (SlowEffectTimer > 0) SlowEffectTimer--;
                        if (SlowTimer > 0) SlowTimer--;
                    }
                    else if (!pl.IsInputEnabled)
                    {
                        pl.SetInputEnabled(true);
                    }


                    JumpCount = stat.TotalJumps;
                    if (Status == 0)
                    {
                        if (StunTime > 0 && Status != 4)
                        {
                            Status = 4;
                            pl.Kill();
                        }
                        if (Hp > 0 && StunTime <= 0)
                        {
                            pl.SetHealth(pl.GetMaxHealth() * Hp / 100.0f);
                            //if (Hp < lastHp) pl.SetHealth(99 + GlobalRandom.Next(100) / 100.0f);				
                            //else pl.SetHealth(100);
                            if (pl.IsWalking && !pl.IsManualAiming && !pl.IsClimbing && !pl.IsLedgeGrabbing && !pl.IsInMidAir && !pl.IsRolling)
                            {
                                if (CurrentEquipment < Equipment.Count)
                                {
                                    Equipment[CurrentEquipment].Use(this);
                                }
                            }
                            else if (Equipment.Count > 1 && pl.CurrentWeaponDrawn == WeaponItemType.NONE)
                            {
                                if (pl.IsMeleeAttacking || pl.IsJumpAttacking) CurrentEquipment = 0;
                                else if (pl.IsKicking || pl.IsJumpKicking) CurrentEquipment = 1;
                            }
                        }
                        else if (Hp <= 0)
                        {
                            if (!IsActive())
                            {
                                Status = -1;
                            }
                            else if (!pl.IsBurning || Armor.FireProtect)
                            {
                                int rnd = GlobalRandom.Next(0, 100);
                                Bleeding = false;
                                if (rnd < OvercomeChance)
                                {
                                    Hp = OvercomeHealth;
                                    Status = 3;
                                }
                                else if (rnd < AliveChance)
                                {
                                    Hp = StableHealth;
                                    Status = 2;
                                }
                                else if (rnd < DyingChance)
                                {
                                    Hp = DyingHealth;
                                    Status = 1;
                                }
                                else
                                {
                                    Status = -1;
                                }
                                pl.Kill();
                            }
                            else
                            {
                                Status = -1;
                                pl.Kill();
                            }
                        }
                    }
                    else if (Status == 1 || Status == 2 || Status == 3)
                    {
                        if (pl.IsBurnedCorpse && !Armor.FireProtect) Hp = 0;
                        if (Hp <= 0)
                        {
                            Status = -1;
                        }
                        else if (Hp <= DyingHealth)
                        {
                            Status = 1;
                        }
                        else if (Hp <= StableHealth)
                        {
                            Status = 2;
                        }
                        else if (Hp <= ReviveHealth)
                        {
                            Status = 3;
                        }
                    }
                    if (Status == 0)
                    {
                        StatusDisplay.SetTextColor(Color.White);
                    }
                    else
                    {
                        if (Team == PlayerTeam.Team1)
                        {
                            StatusDisplay.SetTextColor(new Color(128, 128, 255));
                        }
                        else
                        {
                            StatusDisplay.SetTextColor(new Color(255, 128, 128));
                        }
                    }
                    if (Status == 0)
                    {
                        if (CurrentEquipment < Equipment.Count && InSmoke <= 0)
                        {
                            StatusDisplay.SetText(Equipment[CurrentEquipment].GetName());
                        }
                        else
                        {
                            StatusDisplay.SetText("");
                        }
                    }
                    else if (Status == 1)
                    {
                        StatusDisplay.SetText("DYING");
                        Hp -= DyingSpeed;
                    }
                    else if (Status == 2)
                    {
                        StatusDisplay.SetText("STABLE");
                    }
                    else if (Status == 3)
                    {
                        StatusDisplay.SetText("OVERCOMING");
                        Hp += OvercomeSpeed;
                    }
                    else if (Status == 4)
                    {
                        StatusDisplay.SetText("STUN");
                        StunTime--;
                    }
                    if (Status == 3 && Hp >= ReviveHealth)
                    {
                        Revive(Hp);
                    }
                    else if (Status == 4 && StunTime <= 0)
                    {
                        Revive(Hp, Bleeding);
                    }
                }
                else if (pl == null && Status >= 0)
                {
                    Status = -1;
                }
                if (InSmoke > 0) InSmoke--;
                if (Status == -1)
                {
                    Status = -2;
                    OnDead();
                    StatusDisplay.SetText("");
                }
            }
            public void WeaponTrackingUpdate(bool onlyAdd)
            {
                IPlayer pl = User.GetPlayer();
                RifleWeaponItem rifle = pl.CurrentPrimaryWeapon;
                HandgunWeaponItem pistol = pl.CurrentSecondaryWeapon;
                ThrownWeaponItem thrown = pl.CurrentThrownItem;
                if (onlyAdd)
                {
                    if (rifle.WeaponItem != WeaponItem.NONE)
                    {
                        if (PrimaryWeapon == null) PrimaryWeapon = new TWeapon(rifle.WeaponItem);
                        PrimaryWeapon.TotalAmmo = rifle.TotalAmmo;
                    }
                    if (pistol.WeaponItem != WeaponItem.NONE)
                    {
                        if (SecondaryWeapon == null) SecondaryWeapon = new TWeapon(pistol.WeaponItem);
                        SecondaryWeapon.TotalAmmo = pistol.TotalAmmo;
                    }
                    if (thrown.WeaponItem != WeaponItem.NONE)
                    {
                        if (ThrownWeapon == null) ThrownWeapon = new TWeapon(thrown.WeaponItem);
                        ThrownWeapon.TotalAmmo = thrown.CurrentAmmo;
                    }
                    return;
                }
                if (rifle.WeaponItem == WeaponItem.NONE && PrimaryWeapon != null) PrimaryWeapon = null;
                else if ((PrimaryWeapon == null && rifle.WeaponItem != WeaponItem.NONE)
                    || (PrimaryWeapon != null && (rifle.WeaponItem != PrimaryWeapon.Weapon || PrimaryWeapon.TotalAmmo < rifle.TotalAmmo)))
                {
                    TWeapon pickUp = PlayerPickUpWeaponUpdate(Position, rifle.WeaponItem);
                    if (pickUp != null) PrimaryWeapon = pickUp;
                    else PrimaryWeapon = new TWeapon(rifle.WeaponItem);
                    PrimaryWeapon.TotalAmmo = rifle.TotalAmmo;
                }
                else if (PrimaryWeapon != null && rifle.TotalAmmo < PrimaryWeapon.TotalAmmo)
                {
                    PrimaryWeapon.TotalAmmo = rifle.TotalAmmo;
                    if (!pl.IsReloading) PrimaryWeapon.OnFire(this, WeaponItemType.Rifle);
                }
                if (pistol.WeaponItem == WeaponItem.NONE && SecondaryWeapon != null) SecondaryWeapon = null;
                else if ((SecondaryWeapon == null && pistol.WeaponItem != WeaponItem.NONE)
                    || (SecondaryWeapon != null && (pistol.WeaponItem != SecondaryWeapon.Weapon || SecondaryWeapon.TotalAmmo < pistol.TotalAmmo)))
                {
                    TWeapon pickUp = PlayerPickUpWeaponUpdate(Position, pistol.WeaponItem);
                    if (pickUp != null) SecondaryWeapon = pickUp;
                    else SecondaryWeapon = new TWeapon(pistol.WeaponItem);
                    SecondaryWeapon.TotalAmmo = pistol.TotalAmmo;
                }
                else if (SecondaryWeapon != null && pistol.TotalAmmo < SecondaryWeapon.TotalAmmo)
                {
                    SecondaryWeapon.TotalAmmo = pistol.TotalAmmo;
                    if (!pl.IsReloading) SecondaryWeapon.OnFire(this, WeaponItemType.Handgun);
                }
                if (thrown.WeaponItem == WeaponItem.NONE && ThrownWeapon != null)
                {
                    if (!IsPlayerDropWeapon(Position, thrown.WeaponItem)) ThrownWeapon.OnFire(this, WeaponItemType.Thrown);
                    ThrownWeapon = null;
                }
                else if ((ThrownWeapon == null && thrown.WeaponItem != WeaponItem.NONE)
                    || (ThrownWeapon != null && (thrown.WeaponItem != ThrownWeapon.Weapon || ThrownWeapon.TotalAmmo < thrown.CurrentAmmo)))
                {
                    TWeapon pickUp = PlayerPickUpWeaponUpdate(Position, thrown.WeaponItem);
                    if (pickUp != null)
                    {
                        if (ThrownWeapon != null && ThrownWeapon.CustomId != pickUp.CustomId) pickUp.CustomId = 0;
                        ThrownWeapon = pickUp;
                    }
                    else ThrownWeapon = new TWeapon(thrown.WeaponItem);
                    ThrownWeapon.TotalAmmo = thrown.CurrentAmmo;
                }
                else if (ThrownWeapon != null && thrown.CurrentAmmo < ThrownWeapon.TotalAmmo)
                {
                    ThrownWeapon.TotalAmmo = thrown.CurrentAmmo;
                    ThrownWeapon.OnFire(this, WeaponItemType.Thrown);
                }
                if (PrimaryWeapon != null) PrimaryWeapon.Update();
                if (SecondaryWeapon != null) SecondaryWeapon.Update();
                if (ThrownWeapon != null) ThrownWeapon.Update();
            }
        }

        #endregion

        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
