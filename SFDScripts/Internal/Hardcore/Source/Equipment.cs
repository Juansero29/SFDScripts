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

        #region Equipment Class
        public class TEquipment
        {
            public int Id = 0;
            public int AccessLevel = 0;
            public int Cost = 0;
            public int Level = 0;
            public string Name = "";
            public string Description = "";
            public TEquipment(int id, int cost, int level, string name, string description, int accessLevel = 0)
            {
                Id = id;
                Cost = cost;
                Level = level;
                Name = name;
                Description = description;
                AccessLevel = accessLevel;
            }
        }
        #endregion

        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
