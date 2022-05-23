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

        #region Equipment Slot Class
        public class TEquipmentSlot
        {
            public string Name = "";
            public List<TEquipment> EquipmentList = new List<TEquipment>();
            public TEquipmentSlot(string name)
            {
                Name = name;
            }
            public void AddEquipment(int id, int cost, int level, string name, string description = "", int accessLevel = 0)
            {
                TEquipment equipment = new TEquipment(id, cost, level, name, description, accessLevel);
                EquipmentList.Add(equipment);
            }
            public TEquipment Get(int id)
            {
                for (int i = 0; i < EquipmentList.Count; i++)
                {
                    if (EquipmentList[i].Id == id) return EquipmentList[i];
                }
                return null;
            }
        }

        public void AddLevel(string name, int needExp, int allowPoints)
        {
            TLevel level = new TLevel(name, needExp, allowPoints);
            LevelList.Add(level);
        }

        public TEquipmentSlot AddEquipmentSlot(string name)
        {
            TEquipmentSlot newSlot = new TEquipmentSlot(name);
            newSlot.AddEquipment(0, 0, 0, "None");
            EquipmentList.Add(newSlot);
            return newSlot;
        }
        #endregion

        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
