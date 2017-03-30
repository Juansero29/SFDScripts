// All available system namespaces in the ScriptAPI (as of Alpha 1.0.0).
using SFDGameScriptInterface;
using System;
using System.Collections.Generic;

namespace SFDScripts
{

    class NoGrav : GameScriptInterface
    {
        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public NoGrav() : base(null) { }

        /* SCRIPT STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */

        /*		/*														/*
// ======================================= \\
||	The Ultimate Zero Gravity Method     ||
||	 __  __            _		 	   ||
||	|  \/  |Made by_ _| |_	  	   ||
||	| \  / | ___  | |_   _|#73	 	   ||
||	| |\/| |/ _ \|   || |/ _ \	 	   ||
||	| |  | | (_)  | | | | (_) |	 	   ||
||	|_|  |_|\___/ |_| |_|\___/ 	 	   ||
||					  	   ||
||	With thanks to Nerdist for	   	   ||
||   giving me the idea in the forums!	   ||
\\  ======================================= //						*/

        public void OnStart()
        {
            CreateTimer(500, 0, "GCheck", "");
        }
        public void GAdder(TriggerArgs m73)
        {
            IObject obj = (IObject)m73.Sender;
            if (obj.GetBodyType() == BodyType.Dynamic && !(obj is IPlayer))
            {
                IObject obje = Game.CreateObject("BgWall00", obj.GetWorldPosition() + new Vector2(0, 800));
                IObjectPullJoint pull = (IObjectPullJoint)Game.CreateObject("PullJoint", obj.GetWorldPosition() + new Vector2(0, 800));
                IObjectTargetObjectJoint targ = (IObjectTargetObjectJoint)Game.CreateObject("TargetObjectJoint", obj.GetWorldPosition());
                targ.SetTargetObject(obj);
                pull.SetTargetObjectJoint(targ);
                pull.SetTargetObject(obje);
                pull.SetForce(25.985459270084f * (float)obj.GetMass());
                GObjects.Add(new GObject(obj, pull, obje));
            }
        }
        public void BAdder(TriggerArgs m73)
        {
            IObject obj = (IObject)m73.Sender;
            if (1 == 1)
            {
                IObject obje = Game.CreateObject("InvisibleBlockNoCollision", Game.GetSingleObjectByCustomId("BlackHole").GetWorldPosition());
                IObjectPullJoint pull = (IObjectPullJoint)Game.CreateObject("PullJoint", Game.GetSingleObjectByCustomId("BlackHole").GetWorldPosition());
                IObjectTargetObjectJoint targ = (IObjectTargetObjectJoint)Game.CreateObject("TargetObjectJoint", obj.GetWorldPosition());
                targ.SetTargetObject(obj);
                pull.SetTargetObjectJoint(targ);
                pull.SetTargetObject(obje);
                pull.SetForce(25.985459270084f * (float)obj.GetMass());
                BObjects.Add(new GObject(obj, pull, obje));
            }
        }
        public List<GObject> GObjects = new List<GObject>();
        public List<GObject> BObjects = new List<GObject>();
        public void GCheck(TriggerArgs args)
        {
            if (GObjects.Count != 0)
            {
                foreach (GObject gobj in GObjects) gobj.hangerObject.SetWorldPosition(gobj.mainObject.GetWorldPosition() + new Vector2(0, 800));
            }
        }
        public class GObject
        {
            public IObjectPullJoint pullJoint = null;
            public IObject mainObject = null;
            public IObject hangerObject = null;
            public GObject(IObject main, IObjectPullJoint pull, IObject hang)
            {
                this.pullJoint = (IObjectPullJoint)pull;
                this.mainObject = main;
                this.hangerObject = hang;
            }
        }
        public void CreateTimer(int interval, int count, string method, string id)
        {
            IObjectTimerTrigger timerTrigger = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
            timerTrigger.SetIntervalTime(interval);
            timerTrigger.SetRepeatCount(count);
            timerTrigger.SetScriptMethod(method);
            timerTrigger.CustomId = id;
            timerTrigger.Trigger();
        }

    WeaponItem[] SpaceWeps ={
    WeaponItem.ASSAULT,
    WeaponItem.SUB_MACHINEGUN,
    WeaponItem.GRENADES,
    WeaponItem.BOTTLE,
    WeaponItem.MOLOTOVS,
    WeaponItem.ASSAULT,
    WeaponItem.SUB_MACHINEGUN,
    WeaponItem.GRENADES,
    WeaponItem.BOTTLE,
    WeaponItem.MOLOTOVS,
    WeaponItem.BAT,
    WeaponItem.BAZOOKA,
    WeaponItem.GRENADE_LAUNCHER
};
        Random rnd = new Random();
        public void OnStartup()
        {
            CreateTimer(100, 0, "Fast", "Fast");
            OnStart();
            foreach (IPlayer ply in Game.GetPlayers())
            {
                ply.SetProfile(((IObjectPlayerProfileInfo)Game.GetObjectsByCustomId("Mariner")[rnd.Next(0, Game.GetObjectsByCustomId("Mariner").Length)]).GetProfile());
                int ii = rnd.Next(0, SpaceWeps.Length);
                ply.GiveWeaponItem(SpaceWeps[ii]);
                ii = rnd.Next(0, SpaceWeps.Length);
                ply.GiveWeaponItem(SpaceWeps[ii]);
            }
        }

        public void Move(TriggerArgs args)
        {
            if (args.Sender is IPlayer)
            {
                IObject o = (IObject)args.Sender;
                string str = "";
                switch (((IObject)args.Caller).CustomID)
                {
                    case "Up":
                        str = "DownP";
                        goto case "Vert";
                    case "Down":
                        str = "UpP";
                        goto case "Vert";
                    case "Left":
                        str = "RightP";
                        goto case "Hor";
                    case "Right":
                        str = "LeftP";
                        goto case "Hor";
                    case "Vert":
                        o.SetWorldPosition(new Vector2(o.GetWorldPosition().X, Game.GetSingleObjectByCustomId(str).GetWorldPosition().Y));
                        break;
                    case "Hor":
                        o.SetWorldPosition(new Vector2(Game.GetSingleObjectByCustomId(str).GetWorldPosition().X, o.GetWorldPosition().Y));
                        break;
                }
            }
        }
        public void Fast(TriggerArgs args)
        {
            foreach (IPlayer ply in Game.GetPlayers())
            {
                if (ply.IsWalking && ply.IsInMidAir)
                {
                    ply.SetLinearVelocity(ply.GetLinearVelocity() + new Vector2(-ply.FacingDirection, 10));
                    ShowDust(ply.GetWorldPosition());
                }
                if ((ply.IsBlocking || ply.IsManualAiming) && ply.IsInMidAir)
                {
                    ply.SetLinearVelocity(new Vector2(0, 0));
                    ShowDust(ply.GetWorldPosition());
                }
            }
        }
        public void ShowDust(Vector2 pos) { for (int i = 0; i <= 4; i++) { Game.PlayEffect("TR_D", pos + new Vector2(rnd.Next(-16, 16), rnd.Next(-16, 16))); } }



        /* SCRIPT ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */
    }
}