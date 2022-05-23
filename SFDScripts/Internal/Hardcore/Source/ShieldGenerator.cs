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

        #region Shield Generator Class
        public class TShieldGenerator
        {
            public class TShieldBlock
            {
                public Vector2 Position;
                public float Angle = 0;
                public List<IObject> ShieldGlass = new List<IObject>();
                public IObject ShieldBlock = null;
                public List<IObject> ShieldShard = new List<IObject>();
                public PlayerTeam Team;
                public float Sin;
                public float Cos;
                public float NSin;
                public float NCos;
                public int CheckShieldTimer = 0;
                public bool Disabled = false;
                public int NeedPower = 0;
                public int AnimationTime = 0;
                public TShieldBlock(Vector2 position, float angle, PlayerTeam team)
                {
                    Position = position;
                    Angle = angle;
                    Team = team;
                    Sin = (float)Math.Sin(Angle);
                    Cos = (float)Math.Cos(Angle);
                    NSin = (float)Math.Sin(Angle + (float)Math.PI / 2);
                    NCos = (float)Math.Cos(Angle + (float)Math.PI / 2);
                    for (int i = 0; i < 2; i++)
                    {
                        ShieldGlass.Add(null);
                        ShieldShard.Add(null);
                    }
                    AnimationTime = GlobalRandom.Next(50, 150);
                }
                public void Update()
                {
                    NeedPower = 0;
                    if (CheckShieldTimer == 0)
                    {
                        bool d = false;
                        for (int i = 0; i < PlayerList.Count; i++)
                        {
                            if (PlayerList[i].Team == Team)
                            {
                                IPlayer pl = PlayerList[i].User.GetPlayer();
                                if (pl == null) continue;
                                if (TestDistance(Position, PlayerList[i].Position, 28))
                                {
                                    d = true;
                                    break;
                                }
                            }
                        }
                        if (d && !Disabled)
                        {
                            Remove();
                            NeedPower = -2;
                        }
                        Disabled = d;
                        CheckShieldTimer = 30;
                    }
                    else if (CheckShieldTimer > 0)
                    {
                        CheckShieldTimer--;
                    }
                    if (Disabled) return;
                    if (ShieldBlock == null || ShieldBlock.IsRemoved)
                    {
                        ShieldBlock = GlobalGame.CreateObject("InvisibleBlock", Position + new Vector2(NCos * 4, NSin * 4), Angle);
                        ShieldBlock.SetSizeFactor(new Point(1, 2));
                    }
                    for (int i = 0; i < ShieldGlass.Count; i++)
                    {
                        if (ShieldGlass[i] == null)
                        {
                            int s = (int)Math.Pow(-1, i);
                            ShieldGlass[i] = GlobalGame.CreateObject("ReinforcedGlass00A", Position + new Vector2(NCos * 4 * s, NSin * 4 * s) - new Vector2(Cos, Sin), Angle);
                        }
                        else if (ShieldGlass[i].IsRemoved)
                        {
                            ShieldGlass[i].Remove();
                            ShieldGlass[i] = null;
                        }
                    }
                    for (int i = 0; i < ShieldShard.Count; i++)
                    {
                        int s = (int)Math.Pow(-1, i);
                        if (ShieldShard[i] == null)
                        {
                            ShieldShard[i] = GlobalGame.CreateObject("GlassShard00A", Position + new Vector2(NCos * 4 * s, NSin * 4 * s) + new Vector2(Cos * 4, Sin * 4), Angle - (float)Math.PI / 2);
                            ShieldShard[i].SetBodyType(BodyType.Static);
                        }
                        else if (ShieldShard[i].IsRemoved)
                        {
                            ShieldShard[i].Remove();
                            ShieldShard[i] = null;
                            NeedPower++;
                        }
                    }
                    if (NeedPower > 0)
                    {
                        int offset = GlobalRandom.Next(-8, 8);
                        GlobalGame.PlayEffect("S_P", Position + new Vector2(Cos * offset, Sin * offset));
                    }
                    if (AnimationTime <= 0)
                    {
                        AnimationTime = GlobalRandom.Next(50, 150);
                        int offset = GlobalRandom.Next(-8, 8);
                        GlobalGame.PlayEffect("GLM", Position + new Vector2(Cos * offset, Sin * offset));
                    }
                    else if (AnimationTime > 0)
                    {
                        AnimationTime--;
                    }
                }
                public void Remove()
                {
                    if (ShieldBlock != null) ShieldBlock.Remove();
                    for (int i = 0; i < ShieldGlass.Count; i++)
                    {
                        if (ShieldGlass[i] != null) ShieldGlass[i].Remove();
                    }
                    for (int i = 0; i < ShieldShard.Count; i++)
                    {
                        if (ShieldShard[i] != null) ShieldShard[i].Remove();
                    }
                }
                public void Destroy()
                {
                    if (ShieldBlock != null) ShieldBlock.Remove();
                    for (int i = 0; i < ShieldGlass.Count; i++)
                    {
                        if (ShieldGlass[i] != null) ShieldGlass[i].Destroy();
                    }
                    for (int i = 0; i < ShieldShard.Count; i++)
                    {
                        if (ShieldShard[i] != null) ShieldShard[i].Destroy();
                    }
                }
            }
            public IObject CoreObject;
            public IObjectText TextName;
            public List<IObject> OtherObjects = new List<IObject>();
            public List<TShieldBlock> ShieldBlocks = new List<TShieldBlock>();
            public PlayerTeam Team;
            public float Power = 0;
            public float Radius = 0;
            public bool IsEnabled = false;
            public int Loading = 50;
            public TShieldGenerator(int power, Vector2 position, float radius, PlayerTeam team)
            {
                Team = team;
                Power = power;
                Radius = radius;
                CoreObject = GlobalGame.CreateObject("Computer00", position, 0);
                TextName = (IObjectText)GlobalGame.CreateObject("Text", position);
                TextName.SetTextAlignment(TextAlignment.Middle);
                TextName.SetTextScale(0.8f);
                IObject leftLeg = GlobalGame.CreateObject("Duct00C_D", position + new Vector2(-5, -2), (float)Math.PI / 2);
                IObject rightLeg = GlobalGame.CreateObject("Duct00C_D", position + new Vector2(6, -2), (float)Math.PI / 2);
                IObjectWeldJoint joint = (IObjectWeldJoint)GlobalGame.CreateObject("WeldJoint", position);
                joint.AddTargetObject(CoreObject);
                joint.AddTargetObject(leftLeg);
                joint.AddTargetObject(rightLeg);
                OtherObjects.Add(leftLeg);
                OtherObjects.Add(rightLeg);
                OtherObjects.Add(joint);
                OtherObjects.Add(TextName);
            }
            public void CreateShield()
            {
                float angleStep = (float)Math.Acos((double)(1f - (16f * 16f) / (2f * Radius * Radius)));
                int count = (int)Math.Round(Math.PI * 2 / angleStep);
                angleStep = (float)(Math.PI * 2 / count);
                float currentAngle = 0;
                for (int i = 0; i < count; i++)
                {
                    Vector2 position = CoreObject.GetWorldPosition() + new Vector2((float)Math.Cos(currentAngle) * Radius, (float)Math.Sin(currentAngle) * Radius);
                    int status = TracePath(CoreObject.GetWorldPosition(), position, PlayerTeam.Independent, true);
                    if (status < 3)
                    {
                        TShieldBlock block = new TShieldBlock(position, currentAngle, Team);
                        ShieldBlocks.Add(block);
                    }
                    currentAngle += angleStep;
                }
            }
            public void DestroyShield()
            {
                for (int i = 0; i < ShieldBlocks.Count; i++)
                {
                    ShieldBlocks[i].Destroy();
                }
                ShieldBlocks.Clear();
            }
            public void RemoveShield()
            {
                for (int i = 0; i < ShieldBlocks.Count; i++)
                {
                    ShieldBlocks[i].Remove();
                }
                ShieldBlocks.Clear();
            }
            public void Update()
            {
                if (Loading > 0)
                {
                    Loading--;
                    return;
                }
                if (CoreObject == null || CoreObject.IsRemoved)
                {
                    if (OtherObjects.Count > 0)
                    {
                        for (int i = 0; i < OtherObjects.Count; i++)
                        {
                            if (OtherObjects[i] != null) OtherObjects[i].Destroy();
                        }
                        OtherObjects.Clear();
                    }
                    return;
                }
                TextName.SetWorldPosition(CoreObject.GetWorldPosition() + new Vector2(0, 10));
                string name = "Shield Generator" + "[" + Power + "]";
                TextName.SetText(name);
                if (Team == PlayerTeam.Team1) TextName.SetTextColor(new Color(122, 122, 224));
                else if (Team == PlayerTeam.Team2) TextName.SetTextColor(new Color(224, 122, 122));
                else if (Team == PlayerTeam.Team3) TextName.SetTextColor(new Color(112, 224, 122));
                if (Power > 0)
                {
                    float velocity = CoreObject.GetLinearVelocity().Length();
                    if (!IsEnabled && velocity == 0)
                    {
                        IsEnabled = true;
                        CreateShield();
                    }
                    else if (IsEnabled && velocity != 0)
                    {
                        IsEnabled = false;
                        DestroyShield();
                    }
                    for (int i = 0; i < ShieldBlocks.Count; i++)
                    {
                        ShieldBlocks[i].Update();
                        Power -= ShieldBlocks[i].NeedPower;
                    }
                    Power = Math.Max(0, Power);
                    if (Power <= 0) DestroyShield();
                }
            }
            public void Remove()
            {
                CoreObject.Remove();
                for (int i = 0; i < OtherObjects.Count; i++)
                {
                    OtherObjects[i].Remove();
                }
                RemoveShield();
            }
        }

        public static void CreateShieldGenerator(int power, Vector2 position, float radius, PlayerTeam team)
        {
            TShieldGenerator generator = new TShieldGenerator(power, position, radius, team);
            ShieldGeneratorList.Add(generator);
        }

        #endregion

        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
