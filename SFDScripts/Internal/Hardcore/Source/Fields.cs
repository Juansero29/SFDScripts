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

        #region Fields
        /// <summary>
        /// A semaphore used for saved data file access
        /// </summary>
        private static readonly System.Threading.Mutex Mutex = new System.Threading.Mutex();

        public static Dictionary<string, int> UserAccessList = new Dictionary<string, int>();

        /// <summary>
        /// Couple of variables used for randomness in the game
        /// </summary>
        public static Random rnd = new Random();
        public static Random GlobalRandom = new Random();

        public static IGame GlobalGame;


        /// <summary>
        /// The complete list of levels (ranks) that players can win
        /// </summary>
        public static List<TLevel> LevelList = new List<TLevel>();

        /// <summary>
        /// The list of map parts
        /// </summary>
        public static List<TMapPart> MapPartList = new List<TMapPart>();

        /// <summary>
        /// The list of all players in match
        /// </summary>
        public static List<TPlayer> PlayerList = new List<TPlayer>();

        /// <summary>
        /// The list containing all the equipment available to players
        /// </summary>
        public static List<TEquipmentSlot> EquipmentList = new List<TEquipmentSlot>();

        /// <summary>
        /// The list of menu's for the players
        /// </summary>
        public static List<TPlayerMenu> PlayerMenuList = new List<TPlayerMenu>();

        /// <summary>
        /// The timer that shows up at the beginning of the game
        /// </summary>
        public static IObjectText BeginTimer;

        /// <summary>
        /// The list of players that are inside an strike area in-game
        /// </summary>
        public static List<TPlayerStrikeInfo> AirPlayerList = new List<TPlayerStrikeInfo>();


        public static IObjectTimerTrigger BeginTimerTrigger;

        /// <summary>
        /// Is the data already saved?
        /// </summary>
        public static bool IsDataSaved = false;

        /// <summary>
        /// Is this the first match (round) in the game?
        /// </summary>
        public static bool IsFirstMatch = true;


        /// <summary>
        /// Serves as a container for the data of other players who aren't connected to the on-going match
        /// It is saved along with each player's data at the end of each match
        /// </summary>
        public static string OtherData = "";

        //other

        /// <summary>
        /// A list of objects to remove from the game, it's cleaned at each GameState == 1
        /// </summary>
        public static List<IObject> ObjectsToRemove = new List<IObject>();
        public static float MaxSlowSpeed = 1;

        /// <summary>
        /// List of <see cref="TEffect"/> available
        /// </summary>
        public static List<TEffect> EffectList = new List<TEffect>();
        public static float XPBonus = 1f;

        //electinoc warfare
        public static int[] TeamJamming = { 0, 0, 0 };
        public static int[] TeamHacking = { 0, 0, 0 };

        //turrets
        public static List<TTurret> TurretList = new List<TTurret>();
        /// <summary>
        /// Objects used by the turrets to know if they can shoot or not
        /// </summary>
        public static Dictionary<string, int> VisionObjects = new Dictionary<string, int>();

        /// <summary>
        /// List of shield generators
        /// </summary>
        public static List<TShieldGenerator> ShieldGeneratorList = new List<TShieldGenerator>();

        /// <summary>
        /// The drone map they use to navigate
        /// </summary>
        public static List<List<int>> DroneMap1x1 = new List<List<int>>();


        /// <summary>
        /// Defines the current game state.
        /// 
        /// States:
        /// 
        /// 0 - equipment selection state
        /// 1 - game is starting: map cleansing is done here, also checks if there is still enough players to start a new round
        /// 3 - battle starts: players are spawned into the arena and are free to move
        /// 4 & 7 - blue won this round: all the red team is dead
        /// 5 & 8 - red won this round: all the blue team is dead
        /// 6 - no one won the round: all players are dead - tie
        /// 100 - no enough players to start the game
        /// </summary>
        public static int GameState = 0;
        #endregion

        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
