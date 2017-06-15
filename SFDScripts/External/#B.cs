using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SFDGameScriptInterface;

namespace SFDScripts
{

    class Battle : GameScriptInterface
    {

        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public Battle() : base(null) { }
        public void OnStartup()
        {
            Shoot(300, 0, "Shoot", "");
        }
        public void Shoot(TriggerArgs args)
        {
            foreach (IPlayer ply in Game.GetPlayers())
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