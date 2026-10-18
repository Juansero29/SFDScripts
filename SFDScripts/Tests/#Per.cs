
namespace SFDScripts.Persistance
{

    public partial class GameScript : GameScriptInterfaceExtended
    {

        #region CodeForScript
        private int GamesPlayed = 0;

        public void OnStartup()
        {
            bool success = Int32.TryParse(Game.Data, out GamesPlayed);
            if (!success)
            {
                GamesPlayed = 0;
            }
            else
            {
                GamesPlayed++;
            }
            Game.RunCommand("/MSG This script has been running for " + GamesPlayed + " maps");

            IObjectTimerTrigger Timer = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
            Timer.SetIntervalTime(1000);
            Timer.SetRepeatCount(0);
            Timer.SetScriptMethod("ShowGameDotData");
            Timer.Trigger();
        }

        public void ShowGameDotData(TriggerArgs args)
        {
            Game.RunCommand("/MSG Game.Data.ToString() = " + Game.Data.ToString());
            foreach (char c in Game.Data)
            {
                 Game.RunCommand("/MSG " + c.ToString());
            }
            
        }

        public void OnShutdown()
        {
            Game.Data = GamesPlayed.ToString();
        }

        #endregion CodeForScript

    }
}
