using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SFDGameScriptInterface;

namespace SFDScripts
{

    class UberKick : GameScriptInterface
    {


        public UberKick() : base(null) { }

        #region Script To Copy

        public void OnStartup()
        {
            // Game.GetPlayers()[0].Remove();
            //Game.RunCommand( "/MSG SuperKick mod. Do not blame me for bugs and all. Currently I'm developing the mod." );

            // tick trigger
            CreateTimer(100, 0, "Tick_Fast", true);
            // death tigger
            IObjectTrigger deathTrigger = (IObjectTrigger)Game.CreateObject("OnPlayerDeathTrigger");
            deathTrigger.SetScriptMethod("Death");
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

        public void Tick_Fast(TriggerArgs args)
        {
            foreach (IPlayer ply in Game.GetPlayers())
            {
                if (ply.IsKicking || ply.IsJumpKicking)
                {
                    float minx = 0f;
                    float maxx = 0f;
                    if (ply.FacingDirection == 1)
                    {
                        minx = -4f;
                        maxx = 12f;
                    }
                    else
                    {
                        minx = -12f;
                        maxx = 4f;
                    }

                    Vector2 plyPos = ply.GetWorldPosition();
                    Vector2 minPos = plyPos + new Vector2(minx, -4);
                    Vector2 maxPos = plyPos + new Vector2(maxx, 8);

                    foreach (IObject obj in Game.GetObjectsByArea(new Area(minPos, maxPos)))
                    {
                        if (obj is IPlayer && ply == obj) continue;

                        if (!(obj is IPlayer) || ((IPlayer)obj).IsStaggering || ((IPlayer)obj).IsFalling)
                        {
                            float mass = 1f;
                            if (obj is IPlayer) mass = 1f;

                            Vector2 vel = obj.GetLinearVelocity() * 3 + new Vector2(ply.FacingDirection * 3, 3);
                            obj.SetLinearVelocity(vel / mass);
                        }
                    }
                }
            }
        }

        public void Death(TriggerArgs args)
        {

        } 
        #endregion
    }
}
