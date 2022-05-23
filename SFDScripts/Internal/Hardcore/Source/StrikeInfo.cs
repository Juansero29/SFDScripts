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

        #region Strikes Related Classes
        public class TStrikeInfo
        {
            public int Id = 0;
            public int Angle = 0;
        }

        public class TPlayerStrikeInfo
        {
            public IPlayer Player;
            public List<TStrikeInfo> StrikeList = new List<TStrikeInfo>();
        }
        #endregion

        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
