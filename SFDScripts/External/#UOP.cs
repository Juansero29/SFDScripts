using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SFDGameScriptInterface;

namespace SFDScripts
{

    class UsersOP : GameScriptInterface
    {

        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public UsersOP() : base(null) { }
        bool DestroyEveryThing = false;

        bool AffectStaticles = false;

        float AddedKickForce = 8f;

        float AddedKickDamage = 10f;

        float AddedKickDamage_Players = 7.5f;

        float AddedKickForce_Players = 5f;

        bool StrongerPunch = false;

        float AddedPunchForce = 8f;

        float AddedPunchDamage = 20f;

        float AddedPunchDamage_Players = 15f;


        string[] MeleeDamageObjects = new string[] { "Crate00", "FileCab00", "Barrel00", "Desk00", "StoneWeak00C", "Crate02", "Crate01", "WoodBarrel01", "ReinforcedGlass00A", };
        string[] PrevilagePlayers = new string[] { "django", "nerdist", "(gs)hunor" };

        Area kickarea = new Area(new Vector2(-4, -2), new Vector2(4, 4));
        Area puncharea = new Area(new Vector2(-4, 6), new Vector2(4, 12));


        List<IObject> NoMeleeObject = new List<IObject>();

        Dictionary<IPlayer, float> KickBook = new Dictionary<IPlayer, float>();
        Dictionary<IPlayer, float> KickPlyBook = new Dictionary<IPlayer, float>();

        Dictionary<IPlayer, float> PunchBook = new Dictionary<IPlayer, float>();


        public void OnStartup()
        {
            kickarea.Normalize();
            puncharea.Normalize();

            CreateTimer(125, 0, "KickStronker", true);
            if (StrongerPunch)
            {
                CreateTimer(125, 0, "PunchStronker", true);
            }


            foreach (IObjectAlterCollisionTile tile in Game.GetObjectsByName("AlterCollisionTile"))
            {
                if (tile.GetDisablePlayerMelee())
                {
                    foreach (IObject obj in tile.GetTargetObjects())
                    {
                        NoMeleeObject.Add(obj);
                    }
                }
            }
        }
        private IObjectTimerTrigger CreateTimer(int interval, int count, string method, bool trigger)
        {

            IObjectTimerTrigger trig = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
            trig.SetIntervalTime(interval);
            trig.SetRepeatCount(count);
            trig.SetScriptMethod(method);
            if (trigger) trig.Trigger();
            return trig;
        }
        public void KickStronker(TriggerArgs args)
        {
            foreach (IUser user in Game.GetActiveUsers())
            {
                if (user.GetPlayer() != null)
                {
                    IPlayer ply = user.GetPlayer();

                    if (!(user.GetPlayer()).IsDead)
                    {
                        if (!KickBook.ContainsKey(ply))
                        {
                            KickBook.Add(ply, Game.TotalElapsedGameTime);
                        }
                        if (!KickPlyBook.ContainsKey(ply))
                        {
                            KickPlyBook.Add(ply, Game.TotalElapsedGameTime);
                        }
                        if (KickBook.ContainsKey(ply))
                        {
                            if (ply.IsKicking || ply.IsJumpKicking)
                            {
                                float time = KickBook[ply];
                                if (time + 375f <= Game.TotalElapsedGameTime)
                                {
                                    int i = 1;
                                    if (PrevilagePlayers.Contains(user.Name.ToLower()))
                                    {
                                        i = 3;
                                    }
                                    KickBook[ply] = Game.TotalElapsedGameTime;

                                    Area area = kickarea;
                                    area.Move(new Vector2(ply.FacingDirection * 4, 0) + ply.GetWorldPosition());

                                    foreach (IObject obj in Game.GetObjectsByArea(area))
                                    {
                                        if (obj.Name != "SupplyCrate00" && !(obj is IObjectWeaponItem) && !(obj is IPlayer) && !obj.Name.StartsWith("Bg") && !NoMeleeObject.Contains(obj) && (obj.GetBodyType() == BodyType.Dynamic || AffectStaticles))
                                        {
                                            float mass = Math.Max(obj.GetMass(), 0.001f) * 50;
                                            obj.SetLinearVelocity(obj.GetLinearVelocity() + new Vector2(ply.FacingDirection, 1) * (i * AddedKickForce / mass));
                                            obj.SetAngularVelocity(obj.GetAngularVelocity() - AddedKickForce / mass * ply.FacingDirection);

                                            if (obj.Name.StartsWith("Wood") || obj.Name.StartsWith("Crate"))
                                            {
                                                Game.PlayEffect("W_P", new Vector2(ply.FacingDirection * 6, -4) + ply.GetWorldPosition());
                                            }
                                            if (obj.Name.Contains("Paper"))
                                            {
                                                Game.PlayEffect("PPR_D", new Vector2(ply.FacingDirection * 6, -4) + ply.GetWorldPosition());
                                            }
                                            if (obj.Name.Contains("Glass"))
                                            {
                                                Game.PlayEffect("G_P", new Vector2(ply.FacingDirection * 6, -4) + ply.GetWorldPosition());
                                            }
                                            bool damag = MeleeDamageObjects.Contains(obj.Name);
                                            if (damag || DestroyEveryThing)
                                            {
                                                float div = 1f;
                                                if (DestroyEveryThing && !damag)
                                                {
                                                    div = 20f;
                                                }
                                                obj.SetHealth(obj.GetHealth() - i * AddedKickDamage / div);
                                            }
                                        }
                                    }

                                }

                            }
                        }
                        if (KickPlyBook.ContainsKey(ply))
                        {
                            if (ply.IsKicking || ply.IsJumpKicking)
                            {
                                float time = KickPlyBook[ply];

                                if (time + 300f <= Game.TotalElapsedGameTime)
                                {


                                    Area area = kickarea;
                                    area.Move(new Vector2(ply.FacingDirection * 4, 0) + ply.GetWorldPosition());
                                    int j = 0;
                                    foreach (IObject obj in Game.GetObjectsByArea(area))
                                    {
                                        if (obj is IPlayer && obj != ply)
                                        {
                                            IPlayer plye = (IPlayer)obj;
                                            if ((plye.GetTeam() != ply.GetTeam() || ply.GetTeam() == PlayerTeam.Independent) && (plye.IsStaggering || plye.IsFalling || plye.IsDead))
                                            {
                                                plye.SetHealth(plye.GetHealth() - AddedKickDamage_Players);
                                                plye.SetLinearVelocity(plye.GetLinearVelocity() + new Vector2(ply.FacingDirection, 1) * (AddedKickForce_Players));
                                                j = 1;
                                            }
                                        }
                                    }
                                    if (j == 1)
                                    {
                                        KickPlyBook[ply] = Game.TotalElapsedGameTime;
                                    }
                                }

                            }
                        }
                    }
                }
            }
        }

        public void PunchStronker(TriggerArgs args)
        {
            foreach (IUser user in Game.GetActiveUsers())
            {
                if (user.GetPlayer() != null)
                {
                    IPlayer ply = user.GetPlayer();

                    if (!(user.GetPlayer()).IsDead)
                    {

                        if (PunchBook.ContainsKey(ply))
                        {

                            if (ply.IsMeleeAttacking || ply.IsJumpAttacking)
                            {
                                float time = PunchBook[ply];
                                if (time + 510f <= Game.TotalElapsedGameTime)
                                {

                                    PunchBook[ply] = Game.TotalElapsedGameTime;
                                    Area area = puncharea;
                                    area.Move(new Vector2(ply.FacingDirection * 4, 0) + ply.GetWorldPosition());
                                    foreach (IObject obj in Game.GetObjectsByArea(area))
                                    {
                                        if (obj.Name != "SupplyCrate00" && !(obj is IObjectWeaponItem) && !(obj is IPlayer) && !obj.Name.StartsWith("Bg") && !NoMeleeObject.Contains(obj) && (obj.GetBodyType() == BodyType.Dynamic || AffectStaticles))
                                        {
                                            float mass = Math.Max(obj.GetMass(), 0.001f) * 50;
                                            obj.SetLinearVelocity(obj.GetLinearVelocity() + new Vector2(ply.FacingDirection, 0) * (AddedPunchForce / mass));
                                            obj.SetAngularVelocity(obj.GetAngularVelocity() - AddedPunchForce / mass * Math.Sign(ply.GetWorldPosition().X - obj.GetWorldPosition().X));
                                            Game.PlayEffect("H_T", new Vector2(ply.FacingDirection * 6, 0) + ply.GetWorldPosition());

                                            if (obj.Name.StartsWith("Wood") || obj.Name.StartsWith("Crate"))
                                            {
                                                Game.PlayEffect("W_P", new Vector2(ply.FacingDirection * 6, -4) + ply.GetWorldPosition());
                                            }
                                            if (obj.Name.Contains("Paper"))
                                            {
                                                Game.PlayEffect("PPR_D", new Vector2(ply.FacingDirection * 6, -4) + ply.GetWorldPosition());
                                            }
                                            if (obj.Name.Contains("Glass"))
                                            {
                                                Game.PlayEffect("G_P", new Vector2(ply.FacingDirection * 6, -4) + ply.GetWorldPosition());
                                            }
                                            if (MeleeDamageObjects.Contains(obj.Name) || DestroyEveryThing)
                                            {
                                                obj.SetHealth(obj.GetHealth() - AddedPunchDamage);
                                            }
                                            if (obj.GetBodyType() == BodyType.Dynamic && (obj.GetWorldPosition().X < ply.GetWorldPosition().X - 8 || obj.GetWorldPosition().X > ply.GetWorldPosition().X + 8))
                                            {
                                                obj.TrackAsMissile(true);
                                            }
                                        }
                                        if (obj is IPlayer)
                                        {
                                            IPlayer plye = (IPlayer)obj;
                                            if ((plye.GetTeam() != ply.GetTeam() || ply.GetTeam() == PlayerTeam.Independent) && ply != plye)
                                            {
                                                if (plye.IsStaggering || plye.IsFalling || plye.IsDead)
                                                {
                                                    plye.SetLinearVelocity(plye.GetLinearVelocity() + new Vector2(ply.FacingDirection, 0) * (AddedPunchForce));
                                                }
                                                plye.SetHealth(plye.GetHealth() - AddedPunchDamage_Players);
                                            }
                                        }
                                    }

                                }

                            }
                        }
                        if (!PunchBook.ContainsKey(ply))
                        {
                            PunchBook.Add(ply, Game.TotalElapsedGameTime);
                        }
                    }
                }
            }
        }
    }
}