using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SFDGameScriptInterface;

namespace SFDScripts
{

    class Uberthrow : GameScriptInterface
    {

        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public Uberthrow() : base(null) { }

        #region ScriptToCopy
        /* SCRIPT STARTS HERE */

        /// <summary>
        /// This number controls the speed of the objects that are launched.
        /// </summary>
        float ThrowSpeedMultiply = 1.45f;


        /// <summary>
        /// if this is true, grenades and mines will not impact with any player unless they are hit from a short distance.
        /// </summary>
        bool DisableThrownWeaponsCollision = false;


        /// <summary>
        /// If false, thrown weapons such as grenades, molotovs and  mines will not be accelerated through script.
        /// </summary>
        bool AccelerateThrownWeapons = false;


        /// <summary>
        /// Set any flammable thrown missile (chairs, cuesticks, Suitcase...) to burn in mid air .
        /// </summary>
        bool SetMissileOnFire = true;

        /// <summary>
        /// Melee weapons are ten times heavier when thrown.
        /// </summary>
        bool IncreaseMeleeWeaponMass = false;

        /// <summary>
        /// Gives each player a certain weapon every 15 seconds. (knife if the default weapon)
        /// </summary>
        bool GiveWeaponDefault = true;

        /// <summary>
        /// Contains the enum for the default weapon given to players if <see cref="GiveWeaponDefault"/> is set to true. Users can change this to any weapon they wish.
        /// </summary>
        public WeaponItem DefaultWeapon = WeaponItem.KNIFE;        ///***  YOU CAN FIND A LIST OF WEAPONS BELOW ***///

        /// <summary>
        /// Missiles will not collide, won't do any damage and will be decelerated (if thrown). This might not work for close distances.
        /// </summary>
        bool DisableThrowning = false;


        /// <summary>
        /// Creates a fire circle when the missile is slowed down or no longer a missile. Then it destroys the missile.
        /// </summary>
        bool CreateFireCircleOnImpact = false;


        /// <summary>
        /// Creates an explosion when the missile is slowed down or when it is not longer a missile. Then it destroyes the missile.
        /// </summary>
        bool CreateExplosionOnImpact = false;

        /// <summary>
        /// Creates a random impact.
        /// </summary>
        bool CreateRandomImpact = true;

        int RandomProb = 1;
        string effect = "S_P";

        string[] ThrownWeapons = new string[] { "WpnGrenadesThrown", "WpnMolotovsThrown", "WpnMineThrown" };

        WeaponItem[] ThrownWeaponsClass = new WeaponItem[]
        {
            WeaponItem.GRENADES, WeaponItem.MOLOTOVS, WeaponItem.MINES,
        };


        WeaponItem[] RifleWeaponsClass = new WeaponItem[]
        {
            WeaponItem.SHOTGUN,WeaponItem.TOMMYGUN,WeaponItem.M60,WeaponItem.SNIPER,
            WeaponItem.SAWED_OFF,WeaponItem.BAZOOKA,WeaponItem.ASSAULT,WeaponItem.FLAMETHROWER,
            WeaponItem.GRENADE_LAUNCHER,WeaponItem.SMG,WeaponItem.SUB_MACHINEGUN,
        };

        WeaponItem[] HandWeaponsClass = new WeaponItem[]
        {
            WeaponItem.PISTOL,WeaponItem.MAGNUM,WeaponItem.UZI,WeaponItem.FLAREGUN,
            WeaponItem.REVOLVER,WeaponItem.SILENCEDPISTOL,WeaponItem.SILENCEDUZI,
        };


        WeaponItem[] MeleeWeaponsClass = new WeaponItem[]
        {
            WeaponItem.KATANA,WeaponItem.PIPE,WeaponItem.MACHETE,WeaponItem.BAT,WeaponItem.AXE,
            WeaponItem.HAMMER,WeaponItem.BATON,WeaponItem.KNIFE,WeaponItem.CHAIN,
        };


        WeaponItem[] MakeShiftWeaponsClass = new WeaponItem[]
        {
            WeaponItem.CHAIR,WeaponItem.CHAIR_LEG,WeaponItem.BOTTLE,WeaponItem.BROKEN_BOTTLE,WeaponItem.CUESTICK,
            WeaponItem.CUESTICK_SHAFT,WeaponItem.SUITCASE,WeaponItem.PILLOW,WeaponItem.FLAGPOLE,WeaponItem.TEAPOT,
        };

        Random Rnd = new Random();

        /// <summary>
        /// This method is executed when map starts
        /// </summary>
        public void OnStartup()
        {

            if (CreateFireCircleOnImpact || CreateExplosionOnImpact || CreateRandomImpact)
            {
                /* This timer controls fire circles, explosions on impact and/or random impacts */
                IObjectTimerTrigger Timer0 = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
                Timer0.SetIntervalTime(75);
                Timer0.SetRepeatCount(0);
                Timer0.SetScriptMethod("VeryFast");
                Timer0.Trigger();
            }

            IObjectTimerTrigger Timer = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
            Timer.SetIntervalTime(100);
            Timer.SetRepeatCount(0);
            Timer.SetScriptMethod("Fast");
            Timer.Trigger();

            IObjectTimerTrigger Timer2 = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
            Timer2.SetIntervalTime(400);
            if (CreateRandomImpact)
            {
                Timer2.SetIntervalTime(100);
            }
            Timer2.SetRepeatCount(0);
            Timer2.SetScriptMethod("Slow");
            Timer2.Trigger();

            if (GiveWeaponDefault)
            {
                Game.WriteToConsole("Weapon " + GiveWeaponDefault.ToString() + " is going to be given to players!");

                IObjectTimerTrigger Timer3 = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
                Timer3.SetIntervalTime(15000);
                Timer3.SetRepeatCount(0);

                
                if (ThrownWeaponsClass.Contains(DefaultWeapon))
                {
                    // Chosen weapon is a thrown weapon.
                    Timer3.SetScriptMethod("MidOne");
                }
                if (RifleWeaponsClass.Contains(DefaultWeapon))
                {
                    // Chosen weapon is a rifle.
                    Timer3.SetScriptMethod("MidTwo");
                }
                if (HandWeaponsClass.Contains(DefaultWeapon))
                {
                    // Chosen weapon is a hand weapon.
                    Timer3.SetScriptMethod("MidThree");
                }
                if (MeleeWeaponsClass.Contains(DefaultWeapon))
                {
                    // Chosen weapon is a melee weapon.
                    Timer3.SetScriptMethod("MidFour");
                }
                if (MakeShiftWeaponsClass.Contains(DefaultWeapon))
                {
                    // Chosen weapon is a throwable weak weapon.
                    Timer3.SetScriptMethod("MidFive");
                }
                Timer3.Trigger();
            }
        }


        public void Fast(TriggerArgs args)
        {
            foreach (IObject missile in Game.GetMissileObjects())
            {
                if (missile is IObjectWeaponItem)
                {
                    float speed = 100;
                    if ((speed > 1 || ((IObjectWeaponItem)missile).WeaponItemType == WeaponItemType.Rifle) && !(missile.CustomId == "ScriptMarkedMissile") && (!(ThrownWeapons.Contains(missile.Name)) || AccelerateThrownWeapons) && !DisableThrowning)
                    {
                        missile.SetLinearVelocity(missile.GetLinearVelocity() * ThrowSpeedMultiply);
                        missile.CustomId = "ScriptMarkedMissile";

                        if ((CreateFireCircleOnImpact || CreateExplosionOnImpact))
                        {
                            IObjectTargetObjectJoint targetjoint = (IObjectTargetObjectJoint)Game.CreateObject("TargetObjectJoint", missile.GetWorldPosition(), 0f, missile.GetLinearVelocity(), missile.GetAngularVelocity());
                            targetjoint.SetTargetObject(missile);
                            targetjoint.CustomId = "destructjoint";
                        }

                        if (CreateRandomImpact && Rnd.Next(0, RandomProb) == 0)
                        {
                            int x = Rnd.Next(0, 25);
                            IObjectTargetObjectJoint targetjoint = (IObjectTargetObjectJoint)Game.CreateObject("TargetObjectJoint", missile.GetWorldPosition(), 0f, missile.GetLinearVelocity(), missile.GetAngularVelocity());
                            targetjoint.SetTargetObject(missile);

                            if (x < 25)
                            {

                                targetjoint.CustomId = "Minejoint";
                                missile.SetLinearVelocity(missile.GetLinearVelocity() * 1.5f);

                                IObjectTimerTrigger Timer = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
                                Timer.SetIntervalTime(500);
                                Timer.SetRepeatCount(1);
                                Timer.SetScriptMethod("Mine");
                                Timer.Trigger();


                            }


                            if (x >= 9 && x < 15)
                            {
                                targetjoint.CustomId = "Grenadejoint";
                                missile.SetLinearVelocity(missile.GetLinearVelocity() * 1.5f);

                                IObjectTimerTrigger Timer = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
                                Timer.SetIntervalTime(250);
                                Timer.SetRepeatCount(1);
                                Timer.SetScriptMethod("grenade");
                                Timer.Trigger();


                            }

                            if (x >= 3 && x < 9)
                            {
                                targetjoint.CustomId = "Bazookajoint";
                                missile.SetLinearVelocity(missile.GetLinearVelocity() * 1.5f);

                                IObjectTimerTrigger Timer = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
                                Timer.SetIntervalTime(250);
                                Timer.SetRepeatCount(1);
                                Timer.SetScriptMethod("bazooka");
                                Timer.Trigger();

                            }
                            if (x <= 2)
                            {
                                targetjoint.CustomId = "Electricjoint";
                                missile.SetLinearVelocity(missile.GetLinearVelocity() * 1.5f);
                            }
                        }
                        if (IncreaseMeleeWeaponMass && ((IObjectWeaponItem)missile).WeaponItemType == WeaponItemType.Melee)
                        {
                            missile.SetMass((float)0.8);
                        }
                    }
                    if ((CreateFireCircleOnImpact || CreateExplosionOnImpact) && speed < 4 && missile.CustomId == "ScriptMarkedMissile")
                    {
                        missile.TrackAsMissile(false);
                    }

                    if (DisableThrownWeaponsCollision || DisableThrowning)
                    {
                        if (ThrownWeapons.Contains(missile.Name))
                        {
                            missile.TrackAsMissile(false);
                        }
                    }

                    if (DisableThrowning)
                    {
                        missile.TrackAsMissile(false);
                        if (speed > 5)
                        {
                            missile.SetLinearVelocity(missile.GetLinearVelocity() / 3);
                        }
                    }


                    if (SetMissileOnFire && !DisableThrowning && (missile.CustomId == "ScriptMarkedMissile"))
                    {
                        missile.SetMaxFire();
                        missile.SetHealth(100);
                    }
                }
            }
        }
        public void Slow(TriggerArgs args)
        {
            foreach (IObjectTargetObjectJoint joint in Game.GetObjectsByCustomId("Electricjoint"))
            {
                Game.PlayEffect(effect, joint.GetWorldPosition());
            }
            foreach (IObject obj in Game.GetObjectsByCustomId("ScriptMarkedMissile"))
            {
                if (!obj.IsMissile && !CreateFireCircleOnImpact)
                {
                    obj.CustomId = "";
                    obj.ClearFire();
                    if (IncreaseMeleeWeaponMass && ((IObjectWeaponItem)obj).WeaponItemType == WeaponItemType.Melee)
                    {
                        obj.SetMass((float)0.08);
                    }
                }
            }
        }
        public void MidOne(TriggerArgs args)
        {
            foreach (IPlayer ply in Game.GetPlayers())
            {
                if (ply.CurrentThrownItem.WeaponItem == WeaponItem.NONE && !ply.IsBot && !ply.IsRolling && !ply.IsDead && !ply.IsStaggering && !ply.IsLayingOnGround)
                {
                    ply.GiveWeaponItem(DefaultWeapon);
                }
            }
        }
        public void MidTwo(TriggerArgs args)
        {
            foreach (IPlayer ply in Game.GetPlayers())
            {
                if (ply.CurrentPrimaryWeapon.WeaponItem == WeaponItem.NONE && !ply.IsBot && !ply.IsRolling && !ply.IsDead && !ply.IsStaggering && !ply.IsLayingOnGround)
                {
                    ply.GiveWeaponItem(DefaultWeapon);
                }
            }
        }
        public void MidThree(TriggerArgs args)
        {
            foreach (IPlayer ply in Game.GetPlayers())
            {
                if (ply.CurrentSecondaryWeapon.WeaponItem == WeaponItem.NONE && !ply.IsBot && !ply.IsRolling && !ply.IsDead && !ply.IsStaggering && !ply.IsLayingOnGround)
                {
                    ply.GiveWeaponItem(DefaultWeapon);
                }
            }
        }
        public void MidFour(TriggerArgs args)
        {
            foreach (IPlayer ply in Game.GetPlayers())
            {
                if (ply.CurrentMeleeWeapon.WeaponItem == WeaponItem.NONE && !ply.IsBot && !ply.IsRolling && !ply.IsDead && !ply.IsStaggering && !ply.IsLayingOnGround)
                {
                    ply.GiveWeaponItem(DefaultWeapon);
                }
            }
        }
        public void MidFive(TriggerArgs args)
        {
            foreach (IPlayer ply in Game.GetPlayers())
            {
                if (ply.CurrentWeaponDrawn == WeaponItemType.NONE && !ply.IsBot && !ply.IsRolling && !ply.IsDead && !ply.IsStaggering && !ply.IsLayingOnGround)
                {
                    ply.GiveWeaponItem(DefaultWeapon);
                }
            }
        }

        public void Mine(TriggerArgs args)
        {
            foreach (IObjectTargetObjectJoint joint in Game.GetObjectsByCustomId("Minejoint"))
            {

                for (int i = 0; i <= 6; i++)
                {
                    if (joint != null)
                    {
                        Vector2 vec = new Vector2(Rnd.Next(-3, 3), Rnd.Next(-3, 3)) * 4;
                        Game.CreateObject("WpnMineThrown", joint.GetWorldPosition() + vec, 0f, joint.GetLinearVelocity() + vec, 0f);
                    }
                }
                if (joint != null)
                {
                    joint.GetTargetObject().Destroy();
                    joint.Destroy();
                }
            }
        }

        public void Grenade(TriggerArgs args)
        {
            foreach (IObjectTargetObjectJoint joint in Game.GetObjectsByCustomId("Grenadejoint"))
            {
                for (int i = 0; i <= 6; i++)
                {
                    if (joint != null)
                    {
                        Vector2 vec = new Vector2(Rnd.Next(-3, 3), Rnd.Next(-3, 3)) * 4;
                        Game.CreateObject("WpnGrenadesThrown", joint.GetWorldPosition() + vec, 0f, joint.GetLinearVelocity() + vec, 0f);
                    }
                }
                if (joint != null)
                {
                    joint.GetTargetObject().Destroy();
                    joint.Destroy();
                }
            }
        }

        public void Bazooka(TriggerArgs args)
        {
            foreach (IObjectTargetObjectJoint joint in Game.GetObjectsByCustomId("Bazookajoint"))
            {
                for (int i = 0; i <= 6; i++)
                {
                    if (joint != null)
                    {
                        Game.SpawnProjectile(ProjectileItem.BAZOOKA, joint.GetWorldPosition(), joint.GetWorldPosition() + joint.GetLinearVelocity());
                    }
                }
                if (joint != null)
                {
                    joint.GetTargetObject().Destroy();
                    joint.Destroy();
                }
            }
        }

        public void VeryFast(TriggerArgs args)
        {
            if (CreateRandomImpact)
            {

                foreach (IObjectTargetObjectJoint joint in Game.GetObjectsByCustomId("Electricjoint"))
                {
                    if (joint.GetTargetObject() != null)
                    {
                        if (joint.GetTargetObject().CustomId != "ScriptMarkedMissile")
                        {
                            joint.GetTargetObject().Destroy();
                        }
                    }
                    //////////////////////////////////////////////////////////////////////////
                    if (joint.GetTargetObject() == null)
                    {
                        for (int i = 0; i <= 6; i++)
                        {
                            if (joint != null)
                            {
                                Game.PlayEffect("Electric", joint.GetWorldPosition() + new Vector2(Rnd.Next(-8, 8), Rnd.Next(-8, 8)) * 4);
                                Game.TriggerExplosion(joint.GetWorldPosition() + new Vector2(Rnd.Next(-8, 8), Rnd.Next(-8, 8)) * 4);
                            }
                        }
                        joint.Destroy();
                    }
                }

            }

            foreach (IObjectTargetObjectJoint joint in Game.GetObjectsByCustomId("destructjoint"))
            {
                if (joint.GetTargetObject() != null)
                {
                    if (joint.GetTargetObject().CustomId != "ScriptMarkedMissile")
                    {
                        joint.GetTargetObject().Destroy();
                        Game.TriggerExplosion(joint.GetWorldPosition());
                    }
                }

                if (joint.GetTargetObject() == null)
                {
                    if (CreateFireCircleOnImpact)
                    {
                        Game.SpawnFireNodes(joint.GetWorldPosition(), 75, joint.GetLinearVelocity(), 2, 2, FireNodeType.Flamethrower);
                        joint.Destroy();
                    }
                    if (CreateExplosionOnImpact)
                    {
                        Game.TriggerExplosion(joint.GetWorldPosition());
                        joint.Destroy();
                    }
                }
            }

            foreach (IObjectWeaponItem obj in Game.GetObjectsByCustomId("ScriptMarkedMissile"))
            {
                if (!obj.IsMissile)
                {
                    if (CreateFireCircleOnImpact)
                    {
                        Game.SpawnFireNodes(obj.GetWorldPosition(), 75, obj.GetLinearVelocity(), 2, 2, FireNodeType.Flamethrower);
                        if (MakeShiftWeaponsClass.Contains(obj.WeaponItem))
                        {
                            obj.Destroy();
                        }
                    }
                    if (CreateExplosionOnImpact)
                    {
                        Game.TriggerExplosion(obj.GetWorldPosition());
                        if (MakeShiftWeaponsClass.Contains(obj.WeaponItem))
                        {
                            obj.Destroy();
                        }
                    }
                    obj.CustomId = "";
                }
            }
        }

        /* AND ENDS HERE */
        #endregion ScriptToCopy
    }
}