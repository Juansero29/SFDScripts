using SFDGameScriptInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFDScripts.Internal
{
    public class DataStorageTest : GameScriptInterface
    {
        public DataStorageTest() : base(null) { }

        #region Script To Copy

        #region Local Storage Keys
        private const string MY_SCRIPT_STORAGE_ID = "DSTST";
        private const string SHOTS_FIRED_KEY = "SHOTSFIRED";
        #endregion

        #region Item Ids
        private const string COUNT_TEXT = "NumberOfShotsCountLabel";
        #endregion

        #region Private Fields


        /// <summary>
        /// This script's storage, gives access to persistance
        /// </summary>
        IScriptStorage mScriptStorage;

        /// <summary>
        /// The stone block in the map
        /// </summary>
        readonly IObject mStoneBlock;

        /// <summary>
        /// The caller player
        /// </summary>
        IPlayer mCallerPlayer;

        /// <summary>
        /// The total amount of shots the player has fired in all game sessions
        /// </summary>
        int mTotalShotsFiredCount = 0;

        /// <summary>
        /// The amout of shots the player has fired in the current game session
        /// </summary>
        int mCurrentSessionShotsFiredCount = 0;

        /// <summary>
        /// The caller object
        /// </summary>
        readonly IObject mCallerObject;

        /// <summary>
        /// The number label on the screen
        /// </summary>
        IObjectText mShotsFiredCountLabel;

        #endregion

        #region Game Lifecycle
        public void OnStartup()
        {
            // The game will always call the following method "public void OnStartup()" during a map start (or script activates). 
            // No triggers required. This is run before triggers that activate on startup (and before OnStartup triggers).
            // Game.ShowPopupMessage("OnStartup is run when the map or script is started.");

            System.Diagnostics.Debugger.Break();

            LoadData();
        }


        public void AfterStartup()
        {
            // The game will always call the following method "public void AfterStartup()" after a map start (or script activates). 
            // No triggers required. This is run after triggers that activate on startup (and after OnStartup triggers).

            mCallerPlayer = Game.GetPlayers().ToList().FirstOrDefault();
            mShotsFiredCountLabel = Game.GetSingleObjectByCustomId(COUNT_TEXT) as IObjectText;
            mShotsFiredCountLabel.SetText("SHOOT TO START COUNTING");

            SetPlayerToMarioOne(mCallerPlayer);
            GivePlayerWeapon(mCallerPlayer, WeaponItem.PISTOL);

            Events.UpdateCallback.Start((x) =>
            {
                mCurrentSessionShotsFiredCount = mCallerPlayer.Statistics.TotalShotsFired;
                mShotsFiredCountLabel.SetText((mTotalShotsFiredCount + mCurrentSessionShotsFiredCount).ToString());
            }, 1);
        }

        public void OnShutdown()
        {
            // The game will always call the following method "public void OnShutdown()" before a map restart (or script deactivates). 
            // Perform some cleanup here or store some final information to Game.Data if needed.
            SaveData();
        }


        #endregion

        #region Script Methods

        #region Trigger Methods

        #endregion

        #region Persistence Methods
        private void LoadData()
        {
            // This can be read and written from multiple scripts at the same time.
            // It uses a file with a name. Returns null only if the name is invalid.
            // mScriptStorage = Game.GetSharedStorage(MY_SCRIPT_STORAGE_ID);

            // No file involved in with this method. 
            // Data will not persist once the server restarts or the map is changed
            // While restarting sessions on the same map, the data will persist.
            // mScriptStorage = Game.SessionStorage;

            // This is tied to my script. It uses a file.
            mScriptStorage = Game.LocalStorage;

            

            if (mScriptStorage.ContainsKey(SHOTS_FIRED_KEY))
            {
                if (!mScriptStorage.TryGetItemInt(SHOTS_FIRED_KEY, out mTotalShotsFiredCount))
                {
                    mTotalShotsFiredCount = 0;
                }
            }
        }

        private void SaveData()
        {
            mTotalShotsFiredCount += mCurrentSessionShotsFiredCount;
            mScriptStorage.SetItem(SHOTS_FIRED_KEY, mTotalShotsFiredCount);
        }

        #endregion

        #endregion

        #region Utility Methods

        private void GivePlayerWeapon(IPlayer mCallerPlayer, WeaponItem weapon)
        {
            mCallerPlayer.GiveWeaponItem(weapon);
        }

        private void SetPlayerToMarioOne(IPlayer player)
        {
            IProfile marioProfile = new IProfile()
            {
                Gender = Gender.Male,
                Head = new IProfileClothingItem("Cap", "ClothingWhite"),
                ChestOver = new IProfileClothingItem("Suspenders", "ClothingRed", "ClothingYellow"),
                ChestUnder = new IProfileClothingItem("TrainingShirt", "ClothingWhite"),
                Hands = new IProfileClothingItem("Gloves", "ClothingRed"),
                Accesory = new IProfileClothingItem("Belt", "ClothingRed"),
                Legs = new IProfileClothingItem("Pants", "ClothingRed"),
                Feet = new IProfileClothingItem("Boots", "ClothingBrown")
            };
            player.SetProfile(marioProfile);
        }
        #endregion


        #endregion
    }
}
