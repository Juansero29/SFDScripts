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

        #region Data Persistence
        public static void SaveData()
        {


            // clear the file before so that data doesn't get inserted at the end of file
            Mutex.WaitOne();   // Wait until it is safe to enter. 

            GlobalGame.GetSharedStorage(GetCurrentStorageFileName()).Clear();
            string data = OtherData;
            var menusToSave = PlayerMenuList.Where(m => m.Player != null).ToList();
            for (int i = 0; i < menusToSave.Count; i++)
            {
                var player = menusToSave[i].Player;
                var playerData = menusToSave[i].Save(saveOnlyIfActive: false);
                data += playerData;

                if (ShowDebugMessages)
                {
                    DebugLogger.DebugOnlyDialogLog("SAVED DATA FOR PLAYER " + player.Name + ". PLAYER DATA SAVED: [ " + playerData + " ]");
                }
            }
            GlobalGame.GetSharedStorage(GetCurrentStorageFileName()).SetItem("SaveData", data);
            IsDataSaved = true;
            var playersSavedCount = data.Split(';').Count();
            DebugLogger.DebugOnlyDialogLog("GAME SAVED " + playersSavedCount + " PLAYERS IN DATABASE");
            Mutex.ReleaseMutex();

            // GlobalGame.Data = "BEGIN" + "DATA" + data + "ENDDATA";
        }

        public static void LoadData()
        {
            Mutex.WaitOne();   // Wait until it is safe to enter. 
            string data;
            bool status = GlobalGame.GetSharedStorage(GetCurrentStorageFileName()).TryGetItemString("SaveData", out data);
            if (!status) data = "";
            // data = data.Replace("BEGIN" + "DATA", "").Replace("ENDDATA", "");
            string[] playerList = data.Split(';');
            var playersLoadedCount = data.Split(';').Count();
            DebugLogger.DebugOnlyDialogLog("GAME LOADED " + playersLoadedCount + " PLAYERS FROM DATABASE");
            for (int p = 0; p < playerList.Length; p++)
            {
                string[] plData = playerList[p].Split(':');
                if (plData.Length == 11)
                {
                    List<string> list = new List<string>(plData);
                    list.RemoveAt(3);
                    plData = list.ToArray();
                }
                for (int l = 0; l < PlayerList.Count; l++)
                {
                    string name = FormatName(PlayerList[l].Name);
                    if (name == plData[0])
                    {
                        if (ShowDebugMessages)
                        {
                            DebugLogger.DebugOnlyDialogLog("LOADING DATA FOR PLAYER " + name + ": LEVEL " + plData[1]);
                        }
                        PlayerMenuList[l].SetPlayer(PlayerList[l]);
                        PlayerList[l].Level = Math.Min(Convert.ToInt32(plData[1]), LevelList.Count - 1);
                        PlayerList[l].CurrentExp = Convert.ToInt32(plData[2]);
                        for (int u = 0; u < PlayerMenuList[l].Equipment.Count; u++)
                        {
                            PlayerMenuList[l].Equipment[u] = Convert.ToInt32(plData[3 + u]);
                        }
                        PlayerMenuList[l].ValidateEquipment();
                        playerList[p] = "";
                        break;
                    }
                }
                if (playerList[p] != "")
                {
                    OtherData += playerList[p] + ";";
                }
            }
            Mutex.ReleaseMutex();
        }

        private static string GetCurrentStorageFileName()
        {
            return UseDebugStorage ? "HARDCOREDEBUG" : "HARDCORE";
        }
        #endregion
        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
