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

        #region Custom Armor Class

        public class TCustomArmor
        {
            public int Id = 0;
            public float ProjectileDamageFactor = 1;
            public float FallDamageFactor = 1;
            public float MeleeDamageFactor = 1;
            public float ExplosionDamageFactor = 1;
            public float FireDamageFactor = 1;
            public bool FireProtect = false;
            public bool SuicideMine = false;
            public bool Jammer = false;
            public bool Heavy = false;
            public float MaxProjectileDamage = 75f;
            public float MaxProjectileDamageCut = 27 * 5.75f;
            public float BreakWeaponFactor = 1;
            public float MaxMeleeDamage = 50f;
            public void SetId(int id)
            {
                Id = id;
                ProjectileDamageFactor = 1;
                FallDamageFactor = 1;
                MeleeDamageFactor = 1;
                ExplosionDamageFactor = 1;
                FireDamageFactor = 1;
                BreakWeaponFactor = 1;
                FireProtect = false;
                SuicideMine = false;
                Jammer = false;
                Heavy = false;
                MaxProjectileDamage = 75f;
                MaxProjectileDamageCut = 27 * 5.75f;
                MaxMeleeDamage = 50;
                switch (id)
                {
                    case 1:
                        {
                            ProjectileDamageFactor = 0.65f;
                            MeleeDamageFactor = 0.65f;
                            ExplosionDamageFactor = 0.65f;
                            break;
                        }
                    case 2:
                        {
                            FireDamageFactor = 0.25f;
                            FireProtect = true;
                            break;
                        }
                    case 3:
                        {
                            SuicideMine = true;
                            break;
                        }
                    case 4:
                        {
                            Jammer = true;
                            break;
                        }
                    case 5:
                        {
                            ExplosionDamageFactor = 0.075f;
                            break;
                        }
                    case 6:
                        {
                            Heavy = true;
                            ProjectileDamageFactor = 0.3f;
                            MeleeDamageFactor = 0.3f;
                            MaxMeleeDamage = 25;
                            ExplosionDamageFactor = 0.1f;
                            BreakWeaponFactor = 0.3f;
                            break;
                        }
                    case 7:
                        {
                            MaxProjectileDamageCut = 5000f;
                            MaxProjectileDamage = 50f;
                            break;
                        }
                }
            }
        }

        #endregion

        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
