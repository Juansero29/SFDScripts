using SFDGameScriptInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFDScripts.Tests
{
    class StreetSweeper : GameScriptInterface
    {
        public StreetSweeper() : base(null) { }




        #region Script To Copy
        /// <summary>
        /// The game will always call the following method "public void OnStartup()" during a map start (or script activates). 
        /// </summary>
        /// <remarks>
        /// No triggers required. This is run before triggers that activate on startup (and before OnStartup triggers).
        /// </remarks>
        public void OnStartup()
        {
            Events.UpdateCallback.Start(OnUpdate, 200);
            Events.PlayerDeathCallback.Start(Death);
            // ...
        }

        /// <summary>
        /// The game will always call the following method "public void AfterStartup()" after a map start (or script activates). 
        /// </summary>
        /// <remarks>
        /// No triggers required. This is run after triggers that activate on startup (and after OnStartup triggers).
        /// </remarks>
        public void AfterStartup()
        {
            var users = Game.GetActiveUsers();
            if (users.Length == 0) return;
            for (int i = 0; i < users.Length; i++)
            {
                var user = users[i];
                if (user == null) continue;
                var player = user.GetPlayer();
                if (player == null) continue;
                
                
            }
        }

        /// <summary>
        /// Update loop (must be enabled in the OnStartup() function or AfterStartup() function).
        /// </summary>
        /// <param name="elapsed">Time elapsed</param>
        public void OnUpdate(float elapsed)
        {
        }

        /// <summary>
        /// The game will always call the following method "public void OnShutdown()" before a map restart (or script deactivates). 
        /// </summary>
        /// <remarks>
        /// Perform some cleanup here or store some final information to Game.Data/Game.LocalStorage/Game.SessionStorage if needed.
        /// </remarks>
        public void OnShutdown()
        {

        }


        /// <summary>
        /// Method called when a player dies
        /// </summary>
        /// <param name="args"></param>
        public void Death(IPlayer deadPlayer, PlayerDeathArgs args)
        {

            Game.WriteToConsole("Death() deadPlayer: ", deadPlayer.Name);
            Game.WriteToConsole("Death() args.IsRemoved: ", args.Removed);
        }
        #endregion

    }
}
