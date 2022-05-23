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

        #region Game Mechanics Settings

        // capture points
        public static int MaxCapturePointProgress = 100;
        public static int CapturePointRadius = 35;

        //bleeding
        public static float EasyBleeding = (float)0.01;
        public static float HardBleeding = (float)0.015;
        public static float JumpBleeding = (float)2.5;
        public static int BleedingEffectPeriod = 100;

        //diving
        public static float DivingDamageFactor = 0.25f;
        public static float MinDivingHeight = 50;


        //weapons
        public static Dictionary<WeaponItem, string> WeaponItemNames = new Dictionary<WeaponItem, string>();
        public static List<TWeapon> WeaponTrackingList = new List<TWeapon>();
        public static List<TThrownWeapon> ThrownTrackingList = new List<TThrownWeapon>();
        public static List<WeaponItem> ExtraAmmoWeapon = new List<WeaponItem>(new WeaponItem[] { WeaponItem.PISTOL, WeaponItem.SHOTGUN, WeaponItem.SMG, WeaponItem.TOMMYGUN, WeaponItem.CARBINE, WeaponItem.ASSAULT, WeaponItem.SAWED_OFF, WeaponItem.UZI, WeaponItem.SILENCEDPISTOL, WeaponItem.SILENCEDUZI });
        public static List<WeaponItem> ExtraExplosiveWeapon = new List<WeaponItem>(new WeaponItem[] { WeaponItem.GRENADE_LAUNCHER, WeaponItem.GRENADES, WeaponItem.MOLOTOVS, WeaponItem.MINES, WeaponItem.FLAREGUN, WeaponItem.FLAMETHROWER, WeaponItem.BAZOOKA });
        public static List<WeaponItem> ExtraHeavyAmmoWeapon = new List<WeaponItem>(new WeaponItem[] { WeaponItem.REVOLVER, WeaponItem.SNIPER, WeaponItem.MAGNUM, WeaponItem.M60 });


        // weapons allowed to be thrown
        public static List<string> AllowedMissile = new List<string>(new string[] { "WpnKnife", "WpnKatana", "WpnAxe", "WpnShurikenThrown" });
        #endregion

        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
