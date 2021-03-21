using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SFDGameScriptInterface;

namespace SFDScripts
{

    class Fly : GameScriptInterface
    {

        public Fly() : base(null) { }

        #region Script To Copy

        readonly float force = 0.26f; // 0.26f = Zero Gravity
        readonly float[] demforce = new float[] { 225, 175, 125, }; // Thrust percentage

        readonly bool WorkOutsideBorders = false;
        readonly bool Change_With_Activate_Button = false;

        IObject concrete = null;
        Dictionary<IPlayer, IObjectPullJoint> book;
        Dictionary<IPlayer, int> Gbook;
        Dictionary<IPlayer, bool> Bbook;
        public void OnStartup()
        {
            Game.RunCommand("/MSG Alt To Fly!");

            book = new Dictionary<IPlayer, IObjectPullJoint>();
            Gbook = new Dictionary<IPlayer, int>();
            Bbook = new Dictionary<IPlayer, bool>();
            concrete = Game.CreateObject("InvisibleBlockNoCollision", new Vector2(0, 500));
            CreateTimer(300, 0, "JetPack", true);
            CreateTimer(100, 0, "FireEffect", true);
            foreach (IPlayer ply in Game.GetPlayers())
            {
                Gbook.Add(ply, 2);
                Bbook.Add(ply, true);
                IObjectWeldJoint weld = (IObjectWeldJoint)Game.CreateObject("WeldJoint", ply.GetWorldPosition());
                IObjectActivateTrigger trig = (IObjectActivateTrigger)Game.CreateObject("ActivateTrigger", ply.GetWorldPosition() + new Vector2(0, 8));
                trig.SetBodyType(BodyType.Dynamic);
                trig.SetScriptMethod("Toggle");
                weld.AddTargetObject(trig);
                weld.AddTargetObject(ply);
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
        public void Toggle(TriggerArgs args)
        {
            if (Change_With_Activate_Button)
            {
                if (args.Caller != null && args.Sender != null)
                {
                    if (args.Sender is IPlayer)
                    {
                        IPlayer ply = (IPlayer)args.Sender;
                        if (Gbook.ContainsKey(ply) && Bbook.ContainsKey(ply))
                        {
                            ;
                            int c = Gbook[ply] % demforce.Length;
                            Gbook[ply] = (c + 1) % demforce.Length;
                            Game.PlayEffect("CFTXT", ply.GetWorldPosition() + new Vector2(0, 8), demforce[Gbook[ply]].ToString() + " %");
                        }
                    }
                }
            }
        }
        public void JetPack(TriggerArgs args)
        {
            foreach (IPlayer ply in Game.GetPlayers())
            {
                IObjectPullJoint pullit = null;
                IObjectTargetObjectJoint targetit = null;
                float forc = 0;
                if (!Bbook.ContainsKey(ply) && !ply.IsDead)
                {
                    ;
                    Bbook.Add(ply, true);
                    IObjectWeldJoint weld = (IObjectWeldJoint)Game.CreateObject("WeldJoint", ply.GetWorldPosition());
                    IObjectActivateTrigger trig = (IObjectActivateTrigger)Game.CreateObject("ActivateTrigger", ply.GetWorldPosition() + new Vector2(0, 8));
                    trig.SetBodyType(BodyType.Dynamic);
                    trig.SetScriptMethod("Toggle");
                    weld.AddTargetObject(trig);
                    weld.AddTargetObject(ply);
                }
                if (!Gbook.ContainsKey(ply) && !ply.IsDead)
                {
                    ;
                    Gbook.Add(ply, 2);
                }
                if (Gbook.ContainsKey(ply) && !ply.IsDead)
                {
                    int c = Gbook[ply] % demforce.Length;
                    forc = force * demforce[c] / 100;
                    if (ply.IsBlocking && ply.IsWalking)
                    {
                        Gbook[ply] = (c + 1) % demforce.Length;
                        Game.PlayEffect("CFTXT", ply.GetWorldPosition() + new Vector2(0, 8), demforce[Gbook[ply]].ToString() + " %");
                    }
                }
                if (book.ContainsKey(ply))
                {
                    pullit = book[ply];
                    targetit = pullit.GetTargetObjectJoint();
                    book[ply].SetWorldPosition(targetit.GetWorldPosition() + new Vector2(0, 500));

                }
                bool jeton = (ply.IsWalking && !ply.IsDead && !ply.IsStaggering && !ply.IsRolling && (!ply.IsBlocking || ply.IsInMidAir) && !ply.IsManualAiming);
                bool mapborder = WorkOutsideBorders || Game.GetCameraArea().Contains(ply.GetWorldPosition());
                if (jeton)
                {
                    if (book.ContainsKey(ply))
                    {
                        book[ply].Destroy();
                        book.Remove(ply);

                    }
                    pullit = (IObjectPullJoint)Game.CreateObject("PullJoint", ply.GetWorldPosition() + new Vector2(0, 500));
                    book.Add(ply, pullit);
                    if (!mapborder)
                    {
                        forc = (float)(forc / 2);
                    }
                    pullit.SetForce(forc);
                    targetit = (IObjectTargetObjectJoint)Game.CreateObject("TargetObjectJoint", ply.GetWorldPosition());
                    targetit.SetTargetObject(ply);
                    pullit.SetTargetObject(concrete);
                    pullit.SetTargetObjectJoint(targetit);
                }
                if (!jeton)
                {
                    if (pullit != null)
                    {
                        //	pullit .SetWorldPosition(ply.GetWorldPosition() + new Vector2(0,50));
                        pullit.SetForce(0f);

                    }
                }
            }
        }

        public void FireEffect(TriggerArgs args)
        {
            foreach (IPlayer ply in Game.GetPlayers())
            {
                bool jeton = (ply.IsWalking && !ply.IsDead);
                if (jeton)
                {
                    string effct = "FIRE";
                    if (Gbook.ContainsKey(ply) && !ply.IsDead)
                    {
                        ;
                        int c = Gbook[ply] % demforce.Length;
                        if (c == 0)
                        {
                            effct = "TR_F";
                        }
                        if (c == 1)
                        {
                            effct = "FNDTRA";
                        }
                        if (c == 2)
                        {
                            effct = "TR_D";
                        }
                        if (c == 3)
                        {
                            effct = "GLM";
                        }
                    }
                    Game.PlayEffect(effct, ply.GetWorldPosition() + new Vector2(0, -4));
                }
            }
        } 
        #endregion
    }
}