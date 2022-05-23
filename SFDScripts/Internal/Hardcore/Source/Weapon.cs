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

        #region Weapon Class
        public class TWeapon
        {
            public IObject Object = null;
            public WeaponItem Weapon = WeaponItem.NONE;
            public Vector2 Position = new Vector2(0, 0);
            public int TotalAmmo = 0;
            public bool DNAProtection = false;
            public PlayerTeam TeamDNA = PlayerTeam.Independent;
            public bool CanOverheat = false;
            public float Overheating = 0;
            public float ShotHeat = 0;
            public float Cooling = 0;
            public int CustomId = 0;
            //methods
            public TWeapon(WeaponItem id)
            {
                Weapon = id;
                if (Weapon == WeaponItem.MAGNUM)
                {
                    CanOverheat = true;
                    ShotHeat = 50;
                    Cooling = 0.2f;
                }
            }
            public void OnFire(TPlayer player, WeaponItemType type)
            {
                if (CanOverheat)
                {
                    Overheating += ShotHeat;
                    if (Overheating >= 100)
                    {
                        player.User.GetPlayer().RemoveWeaponItemType(type);
                        GlobalGame.PlayEffect("CFTXT", player.Position, "OVERHEATED");
                        GlobalGame.PlayEffect("EXP", player.Position);
                        player.Hp -= 50;
                        player.Bleeding = true;
                        Vector2 pos = player.Position;
                        GlobalGame.CreateObject("MetalDebris00A", pos, (float)rnd.NextDouble());
                        pos.X += 5;
                        GlobalGame.CreateObject("MetalDebris00B", pos, (float)rnd.NextDouble());
                        pos.X -= 10;
                        GlobalGame.CreateObject("MetalDebris00C", pos, (float)rnd.NextDouble());

                    }
                    else
                    {
                        GlobalGame.PlayEffect("CFTXT", player.Position, "OVERHEATING " + ((int)Overheating).ToString() + "%");
                    }
                }
                if (DNAProtection && player.Team != TeamDNA)
                {
                    player.User.GetPlayer().RemoveWeaponItemType(type);
                    GlobalGame.PlayEffect("CFTXT", player.Position, "DNA SCAN ERROR");
                    GlobalGame.TriggerExplosion(player.Position);
                    DNAProtection = false;
                }
                if (type == WeaponItemType.Thrown)
                {
                    CreateThrownWeapon(Weapon, CustomId, player.Position);
                }
            }
            public void Update()
            {
                if (CanOverheat)
                {
                    if (Overheating > 0) Overheating -= Cooling;
                    if (Overheating < 0) Overheating = 0;
                }
            }
            public void Remove()
            {
                if (Object != null) Object.Remove();
            }
        }

        public void RemoveWeapons()
        {
            for (int i = 0; i < WeaponTrackingList.Count; i++)
            {
                WeaponTrackingList[i].Remove();
            }
            WeaponTrackingList.Clear();
        }

        public static TWeapon PlayerDropWeaponUpdate(Vector2 pos, WeaponItem id)
        {
            float dist = 16;
            TPlayer player = null;
            TWeapon weapon = null;
            for (int i = 0; i < PlayerList.Count; i++)
            {
                float currentDist = (pos - PlayerList[i].Position).Length();
                if (currentDist <= dist)
                {
                    bool hasWeapon = false;
                    IPlayer pl = PlayerList[i].User.GetPlayer();
                    if (PlayerList[i].PrimaryWeapon != null && PlayerList[i].PrimaryWeapon.Weapon == id && (pl != null && pl.CurrentPrimaryWeapon.WeaponItem == WeaponItem.NONE) || pl == null) hasWeapon = true;
                    else if (PlayerList[i].SecondaryWeapon != null && PlayerList[i].SecondaryWeapon.Weapon == id && (pl != null && pl.CurrentSecondaryWeapon.WeaponItem == WeaponItem.NONE) || pl == null) hasWeapon = true;
                    else if (PlayerList[i].ThrownWeapon != null && PlayerList[i].ThrownWeapon.Weapon == id && (pl != null && pl.CurrentThrownItem.WeaponItem == WeaponItem.NONE) || pl == null) hasWeapon = true;
                    if (hasWeapon)
                    {
                        player = PlayerList[i];
                        dist = currentDist;
                    }
                }
            }
            if (player == null) return null;
            if (player.PrimaryWeapon != null && player.PrimaryWeapon.Weapon == id)
            {
                weapon = player.PrimaryWeapon;
                player.PrimaryWeapon = null;
            }
            else if (player.SecondaryWeapon != null && player.SecondaryWeapon.Weapon == id)
            {
                weapon = player.SecondaryWeapon;
                player.SecondaryWeapon = null;
            }
            else if (player.ThrownWeapon != null && player.ThrownWeapon.Weapon == id)
            {
                weapon = player.ThrownWeapon;
                player.ThrownWeapon = null;
            }
            return weapon;
        }

        public static bool IsPlayerDropWeapon(Vector2 pos, WeaponItem id)
        {
            float dist = 16;
            TWeapon weapon = null;
            for (int i = 0; i < WeaponTrackingList.Count; i++)
            {
                if (!WeaponTrackingList[i].Object.RemovalInitiated) continue;
                float currentDist = (pos - WeaponTrackingList[i].Position).Length();
                if (WeaponTrackingList[i].Weapon == id && currentDist <= dist)
                {
                    weapon = WeaponTrackingList[i];
                    dist = currentDist;
                }
            }
            return weapon != null;
        }

        public static void PreWeaponTrackingUpdate()
        {
            string[] argArray = new string[WeaponItemNames.Count];
            WeaponItemNames.Values.CopyTo(argArray, 0);
            IObject[] list = GlobalGame.GetObjectsByName(argArray);
            for (int i = 0; i < list.Length; i++)
            {
                IObject obj = list[i];
                bool found = false;
                for (int j = 0; j < WeaponTrackingList.Count; j++)
                {
                    if (WeaponTrackingList[j].Object == obj)
                    {
                        found = true;
                        break;
                    }
                }
                if (found) continue;
                TWeapon weapon = PlayerDropWeaponUpdate(obj.GetWorldPosition(), ((IObjectWeaponItem)obj).WeaponItem);
                if (weapon == null)
                {
                    weapon = new TWeapon(((IObjectWeaponItem)obj).WeaponItem);
                }
                weapon.Object = obj;
                WeaponTrackingList.Add(weapon);
            }
        }

        public static TWeapon PlayerPickUpWeaponUpdate(Vector2 pos, WeaponItem id)
        {
            if (WeaponTrackingList.Count == 0) return null;
            float dist = 16;
            TWeapon weapon = null;
            int index = 0;
            for (int i = 0; i < WeaponTrackingList.Count; i++)
            {
                if (!WeaponTrackingList[i].Object.RemovalInitiated) continue;
                float currentDist = (pos - WeaponTrackingList[i].Position).Length();
                if (WeaponTrackingList[i].Weapon == id && currentDist <= dist)
                {
                    weapon = WeaponTrackingList[i];
                    dist = currentDist;
                    index = i;
                }
            }
            if (weapon == null) return null;
            WeaponTrackingList.RemoveAt(index);
            return weapon;
        }

        public static void PostWeaponTrackingUpdate()
        {
            for (int i = 0; i < WeaponTrackingList.Count; i++)
            {
                if (WeaponTrackingList[i].Object == null || WeaponTrackingList[i].Object.RemovalInitiated)
                {
                    WeaponTrackingList.RemoveAt(i);
                    i--;
                }
                else
                {
                    WeaponTrackingList[i].Position = WeaponTrackingList[i].Object.GetWorldPosition();
                    WeaponTrackingList[i].Update();
                }
            }
        }

        #endregion

        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
