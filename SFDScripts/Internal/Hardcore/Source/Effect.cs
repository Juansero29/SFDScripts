

namespace SFDScript
{

    public partial class GameScript : GameScriptInterfaceExtended
    {

        /* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */
        #region Effect Class
        public class TEffect
        {
            readonly string Id = "";
            int CurrentTime = 0;
            readonly int Time = 0;
            int Count = 0;
            readonly IObject Object;
            public TEffect(IObject obj, string id, int time, int count)
            {
                Id = id;
                Object = obj;
                Time = time;
                CurrentTime = time;
                Count = count;
            }
            public bool Update()
            {
                if (Object == null) return false;
                if (Count <= 0) return false;
                if (CurrentTime <= 0)
                {
                    GlobalGame.PlayEffect(Id, Object.GetWorldPosition());
                    CurrentTime = Time;
                    Count--;
                }
                CurrentTime--;
                return true;
            }
        }
        #endregion

        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
