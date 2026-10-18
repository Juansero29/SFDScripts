

namespace SFDScript
{

    public partial class GameScript : GameScriptInterfaceExtended
    {

        /* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */

        #region Level Class
        public class TLevel
        {
            public string Name;
            public int NeedExp;
            public int AllowPoints;
            public TLevel(string name, int needExp, int allowPoints)
            {
                Name = name;
                NeedExp = needExp;
                AllowPoints = allowPoints;
            }
        }

        #endregion

        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
