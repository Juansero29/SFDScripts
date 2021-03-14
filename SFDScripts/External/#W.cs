
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SFDGameScriptInterface;

namespace SFDScripts
{

    class Wizards : GameScriptInterface
    {

        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public Wizards() : base(null) { }


        #region Script To Copy


        //==================================================================//
        //============<    #WIZARDS GAMEMODE BY #OCTOTHORP    >=============//
        //============<                  v1.1                 >=============//
        //==================================================================//
        //=====================< #WIZARDS - settings >======================//
        //============================<general>=============================//
        private bool UNLIMITED_MANA = false;                                // set to true to get unlimited mana (unstable now)
        private bool MANA_REGEN = true;                                     // set to true to make mana regenerate with time
        private float MANA_MAX = (float)500;                                // change "peak" mana amount to regenerate to
        private float MANA_REGEN_MULTIPLIER = (float)3.5;                   // change the amount of regenerated mana
        private bool SPELL_COOLDOWN_ACTIVATED = false;                      // set to true to enable spells cooldown
        private bool MANA_COOLDOWN_ACTIVATED = true;                        // set to true to enable mana regeneration cooldown after spell use
                                                                            //private bool TIMELIMIT_ACTIVATED = false;							// set to true to enable time limit for last 2 players
                                                                            //=========================<time limits>============================//
        private const bool TIME_LIMIT_ACTIVATED = false;                        // set to "true" to remove (or explode) host player at start
        private const int TIME_LIMIT_SECONDS = 90;                          // choose the amount of time given to players to finish the round
        private const string TIME_LIMIT_MODE = "auto";                      // set to "main": start timer at beginning; "auto": when 2 teams or 2 players left
                                                                            //===========================<utilites>=============================//
        private bool HOSTING_ACTIVATED = false;                             // set to true to kill host at the beginning of each round.
                                                                            //=========================<commentator>============================//
                                                                            //private bool COMMENTATOR_ACTIVATED = false;						// work in progress here
                                                                            //==================================================================//
                                                                            //=================< DO NOT TOUCH ANYTHING IN BELOW >===============//
                                                                            //==============< IF YOU'RE NOT SURE WHAT YOU'RE DOING >============//
                                                                            //==================================================================//

        // ====================== //
        // ===< DECLARATIONS >=== //
        // ====================== //

        // general mod data
        private class ModData
        {
            // we'll add something else later
            public string Version = "1.1a";
        }
        private ModData modData = new ModData();

        // reserved for commentator
        /*private string [] ONDEATH_TEXT = new string []{
            "{0} : Fly you fools!",
            "{0} failed a saving throw.",
            "{0} wasen't my serf anyway.",
            "Avada kedavra!",
            "Hey, {0} needs a necromancer!",
            "{0} isn't dead, he is figthing a balrog.",
            "{0} is out of mana. Forever."};

        private string [] ONSTART_TEXT = new string []{
            "Let there be figth!",
            "This isn't the magic you're looking for.",
            "Migth and Magic!", 
            "Have you heard about Gwandalf the pilgrim?",
            "Don't SPELL it wrong! Get it?",
            "Pro tip: use fireworks spell NOW!", 
            "It's like a charm",
            "(Battleground)spell. It casts a spell to battleground",
            "HADOUKEN! Uhh, wrong game.",
            "What kind of sorcery is this?"};*/

        // general player data
        private class PlayerData
        {
            public IUser User = null;
            public IPlayer Player = null;
            public bool MagicMode = true;

            public float Mana = 0f;
            public int SpellId = 0;
            public int BookId = 0;

            public float DeathTime = 0f;
            public float NextUseTime = 0f;
            public float LastUseTime = 0f;
            public float LastPushedTime = 0f;
            public bool IsFrozen = false;
            public float Poison = 0f;
            public float UnFreezeTime = 0f;

            public float StartHealth = 100f;
            public float AddedHealth = 0f;
            public float LastHealth = 100f;
            public IObjectText Label = null;

            public PlayerTeam Team = PlayerTeam.Independent;

            public List<BookData> Books = new List<BookData>();
            public class BookData
            {
                public string Name = "unnamed";
                public List<SpellData> Spells = new List<SpellData>();
                public class SpellData
                {
                    public string Name = "unnamed";
                    public float ManaCost = 0f;
                    public float Cooldown = 0f;
                    public float LastUseTime = -40000f;

                    public SpellData(string name, float cost, float cooldown)
                    {
                        this.Name = name;
                        this.ManaCost = cost;
                        this.Cooldown = cooldown;
                    }
                }

                public BookData(string name, List<SpellData> spells)
                {
                    this.Name = name;
                    this.Spells = spells;
                }
            }

            public PlayerData(IUser user, IPlayer ply, float mana)
            {
                this.User = user;
                this.Player = ply;
                this.Mana = mana;
                this.Team = ply.GetTeam();
            }

            public PlayerData() { }
        }

        // used to remove spawned tiles and objects
        private class ProjData
        {
            public IObject ProjObject = null;
            public float DeSpawnTime = 0f;
            public float UnFreezeTime = 0f;
            public bool Explosive = false;
            public string ExplosionType = "";
            public short SmokingAmount = 0;
            public short SmokingRadius = 6;

            public ProjData(IObject obj, float time)
            {
                this.ProjObject = obj;
                this.DeSpawnTime = time;
            }
        }

        // used by mind control spell
        private class SpellsData_MindControl
        {
            public IPlayer Player = null;
            public IUser User = null;
            public PlayerData ControllerData = new PlayerData();
            public float EndTime = 0f;
            public float ControllerHealth = 50f;

            public SpellsData_MindControl(IPlayer ply, IUser user, PlayerData contr, float endTime, float health)
            {
                this.Player = ply;
                this.User = user;
                this.ControllerData = contr;
                this.EndTime = endTime;
                this.ControllerHealth = health;
            }
        }

        private class SpellsData_Teleport
        {
            public IPlayer Player = null;
            public IObject Trigger = null;
            public Vector2 Position = Vector2.Zero;
            public bool Allowed = true;

            public SpellsData_Teleport(IPlayer ply, IObject trig)
            {
                this.Player = ply;
                this.Trigger = trig;
                this.Position = trig.GetWorldPosition();
            }
        }

        Random rand = new Random();

        private List<PlayerData> players = new List<PlayerData>();
        private List<ProjData> projs = new List<ProjData>();
        private List<SpellsData_MindControl> controlledPlayers = new List<SpellsData_MindControl>();
        private List<SpellsData_Teleport> pendingTeleports = new List<SpellsData_Teleport>();

        // time limit vars
        private float m_startTime = 0;
        private int m_lastElapsedSeconds = 0;
        private bool m_twoOpponentsRemaining = false;
        private bool WAS_ACTIVATED;

        string[] restrictedMaps = new string[]{
	// these maps are hard-scripted, though their MapType is Versus
	"#Abadoned Subway (BG)",
    "Brutal Games - Abadoned Subway"
};
        string[] restrictedWeapons = new string[]{
	// clear weapon drops and replace them with custom drops of items in "replacingWeapons"
	"SUPPLYCRATE00",
	// the rest is for custom maps of mapmakers who somewhy use 
	// SpawnWeapon's (cannot be removed) instead of SpawnWeaponArea's (can be removed)
	"WPNPISTOL",
    "WPNMAGNUM",
    "WPNPUMPSHOTGUN",
    "WPNKATANA",
    "WPNPIPEWRENCH",
    "WPNTOMMYGUN",
    "WPNMACHETE",
    "WPNSNIPERRIFLE",
    "WPNSAWEDOFF",
    "WPNBAT",
    "ITEMLASERSIGHT",
    "WPNBAZOOKA",
    "WPNAXE",
    "WPNASSAULTRIFLE",
    "WPNLAZER",
    "WPNCARBINE",
    "WPNMOLOTOVS",
    "WPNFLAMETHROWER",
    "WPNFLAREGUN",
    "WPNREVOLVER",
    "WPNGRENADELAUNCHER",
    "WPNM60",
    "WPNGRENADESTHROWN",
    "WPNMOLOTOVSTHROWN"
};
        // additional array for weapons that can be dropped so players could not "generate pickups"
        string[] restrictedWeapons2 = new string[]{
    "WPNHAMMER",
    "WPNSUBMACHINEGUN",
    "WPNUZI",
    "WPNSMG",
    "WPNGRENADES"
};
        string[] replacingWeapons = new string[]{
    "ITEMSLOMO5",
    "ITEMSLOMO10",
    "ITEMPILLS", "ITEMPILLS", "ITEMPILLS", // just to make them spawn more often (PILLSPILLSPILLS)
	"ITEMMEDKIT"
};

        // =============== //
        // ===< CODE > === //
        // =============== //

        public void OnStartup()
        {
            // check if map is not hard-scripted so that scripts won't conflict
            bool mapIsGood = true;
            if (Game.GetMapType() == MapType.Custom) mapIsGood = false;
            else foreach (string mapName in restrictedMaps) if (mapName == Game.MapName) mapIsGood = false;

            if (mapIsGood)
            {
                // for custom gameover
                Game.SetMapType(MapType.Custom);
                // give each player a set of spells
                foreach (IPlayer ply in Game.GetPlayers())
                    if (ply.GetUser() != null) players.Add(new PlayerData(ply.GetUser(), ply, MANA_MAX));
                foreach (PlayerData data in players)
                    GiveBasicSpells(data);

                // set up the triggers
                // fast tick trigger
                CreateTimer(100, 0, "FastTick", "");

                // mid tick trigger
                CreateTimer(150, 0, "MidTick", "");

                // slow tick trigger
                CreateTimer(500, 0, "SlowTick", "");

                // slow tick trigger
                CreateTimer(1000, 0, "VerySlowTick", "");

                // death trigger
                IObjectTrigger deathTrigger = (IObjectTrigger)Game.CreateObject("OnPlayerDeathTrigger");
                deathTrigger.SetScriptMethod("Death");

                // prepare map
                //foreach (IObject obj in Game.GetObjectsByName(new string[]{"SpawnWeapon", "SpawnWeaponArea", "SpawnWeaponAreaCeiling"})) obj.Remove();
                if (HOSTING_ACTIVATED) Game.GetPlayers()[0].Remove();

                // some messages
                ModMessage("v" + modData.Version);
                if (HOSTING_ACTIVATED) ModMessage("Host is AFK");
                Game.ShowPopupMessage("Controls:\n[KEY=ATTACK] - use spell\n[KEY=BLOCK_AIM] - change spell\n[KEY=DRAW_MELEE], [KEY=DRAW_HANDGUN], [KEY=DRAW_RIFLE], [KEY=DRAW_GRENADE] - change spellbook\n[KEY=WALKING] + [KEY=BLOCK_AIM] - toggle spell using\n", new Color(100, 220, 255));
            }
            else
            {
                ModMessage("This map is restricted.");
            }
        }

        // triggered on players' death
        public void Death(TriggerArgs args)
        {
            IPlayer ply = (IPlayer)args.Sender;
            // remove player's player data if player's body is removed or gibbed
            if (ply.IsRemoved)
            {
                for (int i = players.Count - 1; i >= 0; i--)
                {
                    PlayerData data = players[i];
                    if (data.Player == ply)
                    {
                        if (players[i].Label != null) players[i].Label.Remove();
                        players.RemoveAt(i);
                    }
                }
            }
            CheckGameover();
        }

        public void FastTick(TriggerArgs args)
        {
            Tick_Display();
            Tick_Spell_Teleport();
        }

        public void MidTick(TriggerArgs args)
        {
            Tick_Stats();
            Tick_Controls();
            Tick_Projectiles();
            Tick_Spell_MindControl();
        }

        public void SlowTick(TriggerArgs args)
        {
            Tick_PopupMessage();
            Tick_Weapons();
        }

        public void VerySlowTick(TriggerArgs args)
        {
            Tick_DeadPlayers();
            if (TIME_LIMIT_ACTIVATED) Tick_TimeLimit();
        }

        private bool messageHidden = false;
        private void Tick_PopupMessage()
        {
            if (Game.TotalElapsedGameTime > 8000f && !messageHidden)
            {
                Game.HidePopupMessage();
                messageHidden = true;
            }
        }

        // check dead players if their body is present
        private void Tick_DeadPlayers()
        {
            for (int i = players.Count - 1; i >= 0; i--)
            {
                if (players[i].Player.IsRemoved)
                {
                    if (players[i].Label != null) players[i].Label.Remove();
                    players.RemoveAt(i);
                }
            }
            CheckGameover();
        }

        // display mana and other HUD
        private void Tick_Display()
        {
            for (int i = players.Count - 1; i >= 0; i--)
            {
                PlayerData data = players[i];
                IPlayer ply = data.Player;
                float mana = data.Mana;

                // show mana only when it doesn't equal 100 or while player isn't dead (create new text label)
                if (!data.Player.IsDead && mana != MANA_MAX && data.Label == null)
                {
                    IObjectText labelNew = (IObjectText)Game.CreateObject("Text", Vector2.Zero, 0f);
                    labelNew.SetTextColor(GetTeamColor(ply.GetTeam()));
                    data.Label = labelNew;
                }
                else if ((mana == MANA_MAX || data.Player.IsDead) && data.Label != null)
                {
                    data.Label.Remove();
                    data.Label = null;
                }

                // if text label exists, just move it to player
                if (data.Label != null)
                {
                    data.Label.SetWorldPosition(ply.GetWorldPosition() + new Vector2(-18, 36));

                    // show mana as bar
                    /*string filling = "";
                    for (int j = 1; j <= 6; j++)
                        if (((float)j / 6f) <= ((float)(data.Mana)) / MANA_MAX) filling += "=";
                        else filling += "  ";
                    data.Label.SetText("[" + filling + "]");*/

                    data.Label.SetText("Mana: " + ((int)mana).ToString());
                }
            }
        }

        // catch player's movement to control spell changing and usage
        private void Tick_Controls()
        {
            for (int i = players.Count - 1; i >= 0; i--)
            {
                PlayerData data = players[i];
                IPlayer ply = data.Player;

                // toggle spells mode
                if (ply.IsBlocking && ply.IsWalking && data.NextUseTime <= Game.TotalElapsedGameTime)
                {
                    if (data.MagicMode)
                    {
                        data.MagicMode = false;
                        Game.PlayEffect(
                            "CFTXT",
                            ply.GetWorldPosition() + new Vector2(0f, 30f),
                            "No spell"
                        );
                    }
                    else
                    {
                        data.MagicMode = true;
                        Game.PlayEffect(
                            "CFTXT",
                            ply.GetWorldPosition() + new Vector2(0f, 30f),
                            data.Books[data.BookId].Spells[data.SpellId].Name +
                                " (" + ((int)data.Books[data.BookId].Spells[data.SpellId].ManaCost).ToString() + //", " +
                                                                                                                 /*(data.Books[data.BookId].Spells[data.SpellId].Cooldown / 1000f).ToString().Replace(",", ".") +*/ ")"
                        );
                    }
                    data.NextUseTime = Game.TotalElapsedGameTime + 500f;
                }

                // book changing
                bool bookChanged = false;
                // if player is choosing weapon, remove it and add back to prevent from getting it in hands
                if (!ply.IsDead)
                {
                    if (ply.CurrentWeaponDrawn == WeaponItemType.Melee)
                    {
                        data.BookId = 0;
                        ply.RemoveWeaponItemType(WeaponItemType.Melee);
                        bookChanged = true;
                    }
                    else if (ply.CurrentWeaponDrawn == WeaponItemType.Handgun)
                    {
                        data.BookId = 1;
                        ply.RemoveWeaponItemType(WeaponItemType.Handgun);
                        bookChanged = true;
                    }
                    else if (ply.CurrentWeaponDrawn == WeaponItemType.Thrown)
                    {
                        data.BookId = 2;
                        ply.RemoveWeaponItemType(WeaponItemType.Thrown);
                        bookChanged = true;
                    }
                    else if (ply.CurrentWeaponDrawn == WeaponItemType.Rifle)
                    {
                        data.BookId = 3;
                        ply.RemoveWeaponItemType(WeaponItemType.Rifle);
                        bookChanged = true;
                    }

                    if (ply.CurrentMeleeWeapon.WeaponItem == WeaponItem.NONE) ply.GiveWeaponItem(WeaponItem.HAMMER);
                    else if (ply.CurrentPrimaryWeapon.WeaponItem == WeaponItem.NONE) ply.GiveWeaponItem(WeaponItem.SMG);
                    else if (ply.CurrentSecondaryWeapon.WeaponItem == WeaponItem.NONE) ply.GiveWeaponItem(WeaponItem.UZI);
                    else if (ply.CurrentThrownItem.WeaponItem == WeaponItem.NONE) ply.GiveWeaponItem(WeaponItem.GRENADES);
                }

                // display a book name and reset spell id to prevent "index out of bounds"
                if (bookChanged)
                {
                    data.SpellId = 0;
                    Game.PlayEffect(
                        "CFTXT",
                        ply.GetWorldPosition() + new Vector2(0f, 2f) + new Vector2(0f, 30f),
                        data.Books[data.BookId].Name + " book"
                    );
                }

                // if spells mode is on
                if (data.MagicMode)
                {
                    /*if (ply.IsBlocking && ply.IsWalking && data.NextUseTime <= Game.TotalElapsedGameTime){
                        if (ply.FacingDirection == 1)
                            // start from beginning if it's the end
                            if (data.BookId == data.Books.Count - 1) data.BookId = 0;
                            else data.BookId ++;
                        else if (ply.FacingDirection == -1)
                            if (data.BookId == 0) data.BookId = data.Books.Count - 1;
                            else data.BookId --;
                        // set spell id to 0 to prevent "index out of bounds"
                        data.SpellId = 0;		
                        // "multiple uses in one time"-protection
                        data.NextUseTime = Game.TotalElapsedGameTime + 500f;
                    }*/

                    // spell changing 
                    if (ply.IsBlocking && !ply.IsWalking && data.NextUseTime <= Game.TotalElapsedGameTime)
                    {
                        if (ply.FacingDirection == 1)
                            if (data.SpellId == data.Books[data.BookId].Spells.Count - 1) data.SpellId = 0;
                            else data.SpellId++;
                        else if (ply.FacingDirection == -1)
                            if (data.SpellId == 0) data.SpellId = data.Books[data.BookId].Spells.Count - 1;
                            else data.SpellId--;
                        // show information of chosen spell
                        Game.PlayEffect(
                            "CFTXT",
                            ply.GetWorldPosition() + new Vector2(0f, 30f),
                            data.Books[data.BookId].Spells[data.SpellId].Name +
                                " (" + ((int)data.Books[data.BookId].Spells[data.SpellId].ManaCost).ToString() + //", " +
                                                                                                                 /*(data.Books[data.BookId].Spells[data.SpellId].Cooldown / 1000f).ToString().Replace(",", ".") +*/ ")"
                        );
                        // "multiple uses in one time"-protection
                        data.NextUseTime = Game.TotalElapsedGameTime + 500f;
                    }

                    // spell using
                    if ((ply.IsMeleeAttacking || ply.IsJumpAttacking) && (data.NextUseTime <= Game.TotalElapsedGameTime) && Game.TotalElapsedGameTime > 3000f)
                    {
                        PlayerData.BookData.SpellData spellData = data.Books[data.BookId].Spells[data.SpellId];

                        // "multiple uses in one time"-protection
                        data.NextUseTime = Game.TotalElapsedGameTime + 500f;
                        if (spellData.LastUseTime + spellData.Cooldown <= Game.TotalElapsedGameTime)
                        {
                            if (data.Mana >= spellData.ManaCost) SpellUsed(data);
                            // insufficient mana
                            else Game.PlayEffect("CFTXT", ply.GetWorldPosition() + new Vector2(0f, 30f), "No mana");
                        }
                        else
                        {
                            // cooldown is active
                            int cooldownLeft = ((int)(((spellData.LastUseTime + spellData.Cooldown) - Game.TotalElapsedGameTime + 500f) / 1000));
                            if (cooldownLeft > 0)
                                Game.PlayEffect("CFTXT",
                                    ply.GetWorldPosition() + new Vector2(0f, 30f),
                                    cooldownLeft.ToString() + " sec"
                                    );
                        }
                    }
                }
            }
        }

        // set player's behaviour and conditions like "input enabled" or health
        private void Tick_Stats()
        {
            for (int i = players.Count - 1; i >= 0; i--)
            {
                PlayerData data = players[i];
                IPlayer ply = data.Player;

                // mana pickups
                if (ply.GetHealth() > data.LastHealth)
                    data.Mana += ply.GetHealth() - data.LastHealth;

                //poison
                if (data.Poison != 0)
                {
                    if (data.Poison > ply.GetHealth())
                    {
                        data.Poison = 0f;
                        ply.Kill();
                    }
                    else if (data.Poison > 0f)
                    {
                        data.AddedHealth -= 0.5f;
                        data.Poison -= 0.5f;
                        if (rand.Next(5) == 0) Game.PlayEffect("ACS", ply.GetWorldPosition() + new Vector2(0f, 4f));
                    }
                    if (data.Poison < 0f) data.Poison = 0f;
                }

                // damage
                if (ply.GetUser() != null) ply.SetHealth(data.StartHealth -
                    (ply.Statistics.TotalFallDamageTaken +
                    ply.Statistics.TotalMeleeDamageTaken / 5f +
                    ply.Statistics.TotalProjectileDamageTaken +
                    ply.Statistics.TotalExplosionDamageTaken +
                    ply.Statistics.TotalFireDamageTaken) / 2f +
                    data.AddedHealth);
                data.LastHealth = ply.GetHealth();

                // mana regeneration
                if (Game.TotalElapsedGameTime >= data.LastUseTime + 2000f)
                {
                    if (data.Mana <= (MANA_MAX - MANA_REGEN_MULTIPLIER)) data.Mana += MANA_REGEN_MULTIPLIER;
                    else if (data.Mana < MANA_MAX) data.Mana = MANA_MAX;
                }

                // freezing
                if (data.IsFrozen)
                {
                    if (data.UnFreezeTime <= Game.TotalElapsedGameTime)
                    {
                        Game.PlayEffect("Electric", ply.GetWorldPosition() + new Vector2(0, 8f));
                        ply.SetInputEnabled(true);
                        data.IsFrozen = false;
                    }
                    else
                    {
                        ply.SetInputEnabled(false);
                        ply.AddCommand(new PlayerCommand(PlayerCommandType.StartMoveToPosition, ply.GetWorldPosition(), 0));
                    }
                }
            }
        }

        // remove objects and tiles that exceeded their lifetime
        private void Tick_Projectiles()
        {
            for (int i = projs.Count - 1; i >= 0; i--)
            {
                // frozen objects will be unfrozen here
                if (projs[i].UnFreezeTime != 0f && projs[i].UnFreezeTime <= Game.TotalElapsedGameTime && projs[i].ProjObject.GetBodyType() == BodyType.Static)
                    projs[i].ProjObject.SetBodyType(BodyType.Dynamic);
                // smoking objects will create smoke here
                if (projs[i].SmokingAmount > 0 && !projs[i].ProjObject.IsRemoved)
                {
                    Vector2 pos = projs[i].ProjObject.GetWorldPosition();
                    short radius = projs[i].SmokingRadius;
                    for (int j = 0; j < projs[i].SmokingAmount; j++)
                        Game.PlayEffect("TR_D", pos + new Vector2(rand.Next(-radius, radius), rand.Next(-radius, radius)));
                }
                // objects die here
                if (Game.TotalElapsedGameTime >= projs[i].DeSpawnTime)
                {
                    if (!projs[i].ProjObject.IsRemoved && projs[i].Explosive) TriggerExplosion(projs[i]);
                    projs[i].ProjObject.Remove();
                    projs.RemoveAt(i);
                }
            }
        }

        // remove weapons from map and replace them with items in "replacingWeapons" (custom weapon spawning)
        private void Tick_Weapons()
        {
            foreach (IObject obj in Game.GetObjectsByName(restrictedWeapons))
            {
                if (rand.Next(2) == 0)
                    Game.CreateObject(
                        replacingWeapons[rand.Next(replacingWeapons.Length)],
                        obj.GetWorldPosition(),
                        0f
                    );
                obj.Remove();
            }
            foreach (IObject obj in Game.GetObjectsByName(restrictedWeapons2))
            {
                obj.Remove();
            }
        }

        // if timelimit is on, count down
        private void Tick_TimeLimit()
        {
            if (TimeLimit_Check() && Game.GetSingleObjectByCustomId("HideSettingsTrigger") == null)
            {
                if (!m_twoOpponentsRemaining)
                {
                    // start tracking time
                    // store current elapsed time
                    m_startTime = Game.TotalElapsedGameTime;
                    m_twoOpponentsRemaining = true;
                    WAS_ACTIVATED = true;
                }
                int currentElapsedSeconds = (int)((Game.TotalElapsedGameTime - m_startTime) / 1000f);
                if (currentElapsedSeconds != m_lastElapsedSeconds)
                {
                    // update text
                    int totalSecondsRemaining = TIME_LIMIT_SECONDS - currentElapsedSeconds;
                    int minutesRemaining = totalSecondsRemaining / 60;
                    int secondsRemaining = totalSecondsRemaining - minutesRemaining * 60;
                    if (totalSecondsRemaining > 0)
                    {
                        Game.ShowPopupMessage(string.Format("Time remaining {0}:{1:00}", minutesRemaining, secondsRemaining));
                    }
                    else
                    {
                        Game.HidePopupMessage();
                        Game.SetGameOver("Time's up!");
                    }
                    m_lastElapsedSeconds = currentElapsedSeconds;
                }
            }
            else if (WAS_ACTIVATED)
            {
                Game.HidePopupMessage();
            }
        }

        // check if timelimit should be activated now
        private bool TimeLimit_Check()
        {
            if (TIME_LIMIT_MODE == "main") return true;
            if (Game.GetActiveUsers().Length <= 2) return false;
            else if (TIME_LIMIT_MODE == "auto")
            {
                Dictionary<PlayerTeam, int> teams = new Dictionary<PlayerTeam, int>();
                PlayerTeam playerATeam = PlayerTeam.Independent;
                PlayerTeam playerBTeam = PlayerTeam.Independent;
                int opponents = 0;

                foreach (IPlayer ply in Game.GetPlayers())
                {
                    if (!ply.IsDead)
                    {
                        PlayerTeam team = ply.GetTeam();
                        if (!teams.ContainsKey(team)) teams.Add(team, 1);
                        else teams[team]++;

                        opponents++;
                        switch (opponents)
                        {
                            case 1: playerATeam = ply.GetTeam(); break;
                            case 2: playerBTeam = ply.GetTeam(); break;
                        }
                    }
                }

                return ((teams.Count == 2 && teams.ContainsValue(1)) || (opponents == 2 && playerATeam == PlayerTeam.Independent));
            }

            return false;
        }

        // use player data to perform a spell and remove mana and set a cooldown
        private void SpellUsed(PlayerData data)
        {
            IPlayer ply = data.Player;
            bool usedNow = true;

            switch (data.Books[data.BookId].Spells[data.SpellId].Name)
            {
                case "FirePunch":
                    Game.SpawnFireNodes(
                        ply.GetWorldPosition() + new Vector2(ply.FacingDirection * 10, 10),
                        12,
                        new Vector2(ply.FacingDirection * 6, 0),
                        1,
                        2,
                        FireNodeType.Flamethrower);
                    break;
                case "FireBall":
                    Game.SpawnFireNodes(
                        ply.GetWorldPosition() + new Vector2(ply.FacingDirection * 6, 10),
                        80,
                        new Vector2(ply.FacingDirection * 20, 0),
                        0.5f,
                        0.5f,
                        FireNodeType.Flamethrower);
                    break;
                case "Crate of doom":
                    {
                        Vector2 pos = ply.GetWorldPosition() + new Vector2(ply.FacingDirection * 22, 16);
                        IObject proj = Game.CreateObject(
                        "Crate01",
                        pos,
                        0f,
                        new Vector2(ply.FacingDirection * 15, 3),
                        (float)rand.Next(-50, 50));
                        for (int i = 0; i < 15; i++) Game.PlayEffect("TR_D", pos + new Vector2(rand.Next(-6, 6), rand.Next(-6, 6)));
                        projs.Add(new ProjData(proj, Game.TotalElapsedGameTime + 8000f));
                    }
                    break;
                case "Paper":
                    {
                        Vector2 pos = ply.GetWorldPosition() + new Vector2(ply.FacingDirection * 6, 10);
                        IObject proj = Game.CreateObject(
                        "CrumpledPaper00",
                        pos,
                        0f,
                        new Vector2(ply.FacingDirection * 10, 3),
                        (float)rand.Next(-30, 30));
                        Game.PlayEffect("Electric", pos);
                        ProjData projData = new ProjData(proj, Game.TotalElapsedGameTime + 1500f);
                        projData.Explosive = true;
                        projData.ExplosionType = "basic";
                        projData.SmokingAmount = 2;
                        projData.SmokingRadius = 2;
                        projs.Add(projData);
                    }
                    break;
                case "Bottle":
                    {
                        Vector2 pos = ply.GetWorldPosition() + new Vector2(ply.FacingDirection * 15, 10);
                        IObject proj = Game.CreateObject(
                        "Bottle00",
                        pos,
                        0f,
                        new Vector2(ply.FacingDirection * 10, 3),
                        (float)rand.Next(-30, 30));
                        Game.PlayEffect("Electric", pos);
                        ProjData projData = new ProjData(proj, Game.TotalElapsedGameTime + 1500f);
                        projData.Explosive = true;
                        projData.ExplosionType = "fire_big";
                        projs.Add(projData);
                    }
                    break;
                case "Balloon":
                    {
                        Vector2 pos = ply.GetWorldPosition() + new Vector2(ply.FacingDirection * 15, 10);
                        IObject proj = Game.CreateObject(
                        "Balloon00",
                        pos,
                        0f,
                        new Vector2(ply.FacingDirection * 5, 3),
                        (float)rand.Next(-30, 30));
                        Game.PlayEffect("Electric", pos);
                        ProjData projData = new ProjData(proj, Game.TotalElapsedGameTime + 1000f);
                        projData.Explosive = true;
                        projData.ExplosionType = "cluster";
                        projs.Add(projData);
                    }
                    break;
                case "ANVIL":
                    {
                        Vector2 pos = ply.GetWorldPosition() + new Vector2(ply.FacingDirection * 48, 50);
                        IObject proj = Game.CreateObject("StoneWeak00C", pos, 0f);
                        proj.SetBodyType(BodyType.Dynamic);
                        proj.SetLinearVelocity(new Vector2(0, -10));
                        projs.Add(new ProjData(proj, Game.TotalElapsedGameTime + 1000f));
                        for (int i = 0; i < 15; i++) Game.PlayEffect("TR_D", pos + new Vector2(rand.Next(-6, 6), rand.Next(-6, 6)));
                    }
                    break;
                case "Heal":
                    {
                        float healAmount = 6f;
                        if (ply.GetHealth() >= 50f)
                        {
                            usedNow = false;
                            Game.PlayEffect("CFTXT", ply.GetWorldPosition() + new Vector2(0f, 30f), "Health full!");
                        }
                        else if (ply.GetHealth() > 50f - healAmount)
                            data.AddedHealth += 50f - ply.GetHealth();
                        else data.AddedHealth += healAmount;
                    }
                    break;
                case "Revive":
                    for (int i = 0; i <= 2; i++)
                    {
                        IObjectTrigger areaTrigger = ((IObjectTrigger)Game.CreateObject(
                            "AreaTrigger",
                            ply.GetWorldPosition() + new Vector2(((i - 1) * 8), 2f),
                            0
                        ));
                        areaTrigger.SetScriptMethod("SpellTrigger_Revive");
                        areaTrigger.CustomId = "ReviveTrigger";
                        projs.Add(new ProjData(areaTrigger, Game.TotalElapsedGameTime + 50f));
                    }
                    m_reviveTeam = ply.GetTeam();
                    break;
                case "Defender":
                    PlayerTeam defendingTeam = ply.GetTeam();
                    IPlayer defender = Game.CreatePlayer(ply.GetWorldPosition() + new Vector2(ply.FacingDirection * 25, 0f));
                    BotBehavior bb = new BotBehavior() { TutorialMelee = true };
                    defender.SetBotBehavior(bb);
                    defender.SetTeam(defendingTeam);
                    break;
                /*case "Block":
                    IObject grab = Game.CreateObject("Wood03F", ply.GetWorldPosition() + new Vector2(ply.FacingDirection*7, 0), 0);
                    projs.Add(new ProjData(grab, Game.TotalElapsedGameTime + 1200f));
                    break;*/
                case "Shield":
                    {
                        float delay = 3000f;
                        float lifetime = 30000f;
                        Vector2 pos = ply.GetWorldPosition() + new Vector2(ply.FacingDirection * 30, 3);
                        {
                            IObject piece = Game.CreateObject("StoneWeak00C", pos, 0f);
                            ProjData proj = new ProjData(piece, Game.TotalElapsedGameTime + lifetime);
                            proj.UnFreezeTime = Game.TotalElapsedGameTime + delay;
                            projs.Add(proj);
                        }
                        {
                            IObject piece = Game.CreateObject("StoneWeak00C", pos + new Vector2(8, 24), 0f);
                            ProjData proj = new ProjData(piece, Game.TotalElapsedGameTime + lifetime);
                            proj.UnFreezeTime = Game.TotalElapsedGameTime + delay;
                            projs.Add(proj);
                        }
                        {
                            IObject piece = Game.CreateObject("StoneWeak00B", pos + new Vector2(12, 0), -1.57f);
                            ProjData proj = new ProjData(piece, Game.TotalElapsedGameTime + lifetime);
                            proj.UnFreezeTime = Game.TotalElapsedGameTime + delay;
                            projs.Add(proj);
                        }
                        {
                            IObject piece = Game.CreateObject("StoneWeak00B", pos + new Vector2(8, 12), 0f);
                            ProjData proj = new ProjData(piece, Game.TotalElapsedGameTime + lifetime);
                            proj.UnFreezeTime = Game.TotalElapsedGameTime + delay;
                            projs.Add(proj);
                        }
                        {
                            IObject piece = Game.CreateObject("StoneWeak00B", pos + new Vector2(-4, 16), 1.57f);
                            ProjData proj = new ProjData(piece, Game.TotalElapsedGameTime + lifetime);
                            proj.UnFreezeTime = Game.TotalElapsedGameTime + delay;
                            projs.Add(proj);
                        }
                        {
                            IObject piece = Game.CreateObject("StoneWeak00B", pos + new Vector2(-4, 32), 1.57f);
                            ProjData proj = new ProjData(piece, Game.TotalElapsedGameTime + lifetime);
                            proj.UnFreezeTime = Game.TotalElapsedGameTime + delay;
                            projs.Add(proj);
                        }
                        {
                            IObject piece = Game.CreateObject("StoneWeak00B", pos + new Vector2(8, 36), 0f);
                            ProjData proj = new ProjData(piece, Game.TotalElapsedGameTime + lifetime);
                            proj.UnFreezeTime = Game.TotalElapsedGameTime + delay;
                            projs.Add(proj);
                        }
                    }
                    break;
                case "Mind Lock":
                    for (int i = 0; i <= 9; i++)
                    {
                        int k = i / 5;
                        IObjectTrigger areaTrigger = ((IObjectTrigger)Game.CreateObject(
                            "AreaTrigger",
                            ply.GetWorldPosition() + new Vector2(ply.FacingDirection * (24 + (i - 1 - (k * 5)) * 10), 2f + (k * 16)),
                            0
                        ));
                        areaTrigger.SetScriptMethod("SpellTrigger_MindLock");
                        projs.Add(new ProjData(areaTrigger, Game.TotalElapsedGameTime + 150f));
                    }
                    m_mindLocker = ply;
                    break;
                case "Mind Control":
                    m_mindController = data;
                    for (int i = 0; i <= 9; i++)
                    {
                        int k = i / 5;
                        IObjectTrigger areaTrigger = ((IObjectTrigger)Game.CreateObject(
                            "AreaTrigger",
                            ply.GetWorldPosition() + new Vector2(ply.FacingDirection * (24 + (i - 1 - (k * 5)) * 10), 2f + (k * 16)),
                            0
                        ));
                        areaTrigger.CustomId = "MindControlTrigger";
                        areaTrigger.SetScriptMethod("SpellTrigger_MindControl");
                        projs.Add(new ProjData(areaTrigger, Game.TotalElapsedGameTime + 150f));
                    }
                    break;
                case "FUS":
                    {
                        for (int i = 0; i <= 2; i++)
                        {
                            Vector2 pos = ply.GetWorldPosition() + new Vector2(ply.FacingDirection * 50 + ((i - 1) * 18), 2f);
                            IObjectTrigger areaTrigger = ((IObjectTrigger)Game.CreateObject(
                                "AreaTrigger",
                                pos,
                                0
                            ));
                            for (int j = 0; j < 15; j++) Game.PlayEffect("TR_D", pos + new Vector2(rand.Next(-6, 6), rand.Next(4, 16)));
                            if (data.Player.FacingDirection == 1) areaTrigger.SetScriptMethod("SpellTrigger_FusRoDah_Right");
                            else areaTrigger.SetScriptMethod("SpellTrigger_FusRoDah_Left");
                            projs.Add(new ProjData(areaTrigger, Game.TotalElapsedGameTime + 100f));
                        }
                        Game.PlayEffect("CAM_S", Vector2.Zero, 1f, 500f, true);
                    }
                    break;
                case "To The Moon!":
                    {
                        for (int i = 0; i <= 2; i++)
                        {
                            Vector2 pos = ply.GetWorldPosition() + new Vector2(ply.FacingDirection * 50 + ((i - 1) * 18), 4f);
                            IObjectTrigger areaTrigger = ((IObjectTrigger)Game.CreateObject(
                                "AreaTrigger",
                                pos,
                                0
                            ));
                            for (int j = 0; j < 15; j++) Game.PlayEffect("TR_D", pos + new Vector2(rand.Next(-6, 6), rand.Next(-6, 6)));
                            areaTrigger.SetScriptMethod("SpellTrigger_ToTheMoon");
                            projs.Add(new ProjData(areaTrigger, Game.TotalElapsedGameTime + 100f));
                        }
                        Game.PlayEffect("CAM_S", Vector2.Zero, 1f, 500f, true);
                    }
                    break;
                case "SuperJump":
                    {
                        ply.SetLinearVelocity(new Vector2(ply.FacingDirection * 2f, 12f));
                        for (int i = 0; i < 10; i++)
                            Game.PlayEffect("TR_D", ply.GetWorldPosition() + new Vector2(rand.Next(-12, 12), rand.Next(-4, 4)));
                        Game.PlayEffect("CAM_S", Vector2.Zero, 1f, 500f, true);
                        if (ply.IsJumpAttacking) data.NextUseTime = Game.TotalElapsedGameTime + 2500f;
                    }

                    break;
                case "Teleport":
                    {
                        Vector2 pos = Vector2.Zero;
                        if (ply.IsWalking && ply.IsJumpAttacking) pos = ply.GetWorldPosition() + new Vector2(0f, 50f);
                        else if (ply.IsWalking) pos = ply.GetWorldPosition() + new Vector2(0f, -28f);
                        else pos = ply.GetWorldPosition() + new Vector2(ply.FacingDirection * 70f, 10f);
                        IObjectTrigger areaTrigger = ((IObjectTrigger)Game.CreateObject(
                            "AreaTrigger",
                            pos,
                            0
                        ));
                        pendingTeleports.Add(new SpellsData_Teleport(ply, areaTrigger));
                        areaTrigger.SetScriptMethod("SpellTrigger_Teleport");
                        // make it dynamic to allow detecting static objects
                        areaTrigger.SetBodyType(BodyType.Dynamic);
                        projs.Add(new ProjData(areaTrigger, Game.TotalElapsedGameTime + 10f));
                    }
                    break;
                case "Fireworks":
                    Game.SpawnFireNodes(
                        ply.GetWorldPosition() + new Vector2(0f, 4f),
                        12,
                        new Vector2(-ply.FacingDirection * 6, 0),
                        1,
                        2,
                        FireNodeType.Flamethrower);
                    Game.SpawnProjectile(ProjectileItem.BAZOOKA, ply.GetWorldPosition() + new Vector2(-ply.FacingDirection * 8f, 4f), new Vector2(ply.FacingDirection, 0f));
                    //for (int i = 0; i < 5; i++) Game.TriggerExplosion(ply.GetWorldPosition() + new Vector2(rand.Next(-40, 40), rand.Next(-40, 40)));
                    //ply.Gib();
                    Game.PlaySound("Wilhelm", ply.GetWorldPosition(), 75f);
                    break;
                case "Tarot Card":
                    Vector2 posCard = ply.GetWorldPosition() + new Vector2(ply.FacingDirection * 10, 10);
                    switch (rand.Next(35))
                    {
                        case 0:
                            {
                                Game.SpawnProjectile(ProjectileItem.SNIPER, posCard, new Vector2(ply.FacingDirection, 0f));
                                Game.PlayEffect("CFTXT", ply.GetWorldPosition() + new Vector2(0f, 30f), "The fool");
                                Game.PlaySound("Flaregun", ply.GetWorldPosition(), 50f);
                                Game.PlayEffect("Electric", ply.GetWorldPosition() + new Vector2(0, 8f));
                                break;
                            }
                        case 1:
                            {
                                Game.SpawnProjectile(ProjectileItem.REVOLVER, posCard, new Vector2(ply.FacingDirection, 0f));
                                Game.PlayEffect("CFTXT", ply.GetWorldPosition() + new Vector2(0f, 30f), "The magician");
                                Game.PlaySound("ItemSpawn", ply.GetWorldPosition(), 50f);
                                usedNow = false;
                                data.Mana += 10;
                                data.LastUseTime = 0f;
                                Game.PlayEffect("Electric", ply.GetWorldPosition() + new Vector2(0, 8f));
                                break;
                            }
                        case 2:
                            {
                                Game.SpawnProjectile(ProjectileItem.REVOLVER, posCard, new Vector2(ply.FacingDirection, 0f));
                                Game.PlayEffect("CFTXT", ply.GetWorldPosition() + new Vector2(0f, 30f), "The judgment");
                                Game.PlaySound("ElevatorDing", ply.GetWorldPosition(), 50f);
                                Game.PlaySound("FootstepPiano", ply.GetWorldPosition(), 50f);
                                float healAmount = 9f;
                                if (ply.GetHealth() > 50f - healAmount) data.AddedHealth += 50f - ply.GetHealth();
                                else data.AddedHealth += healAmount;
                                Game.PlayEffect("Electric", ply.GetWorldPosition() + new Vector2(0, 8f));
                                break;
                            }
                        default:
                            {
                                Game.SpawnProjectile(ProjectileItem.CARBINE, posCard, new Vector2(ply.FacingDirection, 0f));
                                Game.PlaySound("MeleeDraw", ply.GetWorldPosition(), 50f);
                                break;
                            }
                    }
                    break;
                case "Cloud":
                    for (int i = 0; i < 4; i++)
                    {
                        IObject obj = Game.CreateObject("InvisiblePlatform", ply.GetWorldPosition() + new Vector2(ply.FacingDirection * (8f * i - 16f), -10f), 0f);
                        ProjData proj = new ProjData(obj, Game.TotalElapsedGameTime + 7000f);
                        proj.SmokingAmount = 4;
                        projs.Add(proj);
                    }
                    break;
                case "Skunk":
                    IObject objCan = Game.CreateObject(
                        "CrabCan00",
                        ply.GetWorldPosition() + new Vector2(ply.FacingDirection * 15f, 8f),
                        0f,
                        new Vector2(ply.FacingDirection * 5f, 3f),
                        3f);
                    ProjData projCan = new ProjData(objCan, Game.TotalElapsedGameTime + 1500f);
                    projCan.Explosive = true;
                    projCan.ExplosionType = "poison_gaz";
                    projCan.SmokingAmount = 1;
                    projs.Add(projCan);
                    break;
                /*case "BALLOONS!":
                    for (int i = 0; i < 3; i++){
                        IObject proj = Game.CreateObject(
                        "Balloon00",
                        ply.GetWorldPosition() + new Vector2(ply.FacingDirection * 12, 10),
                        0f,
                        new Vector2(ply.FacingDirection * rand.Next(3,10), rand.Next(-5,5)),
                        (float)rand.Next(-30, 30));
                        projs.Add(new ProjData(proj, Game.TotalElapsedGameTime + 1000f, true)); }
                    break;*/
                /*case "Demigod":
                    Game.TriggerExplosion(ply.GetWorldPosition() + new Vector2(ply.FacingDirection*100, 0f));
                    break;*/
                default:
                    Game.PlayEffect("CFTXT", ply.GetWorldPosition() + new Vector2(0f, 30f), "SHIT, ERROR!!!");
                    break;
            }



            if (!UNLIMITED_MANA && usedNow) data.Mana -= data.Books[data.BookId].Spells[data.SpellId].ManaCost;
            if (MANA_COOLDOWN_ACTIVATED && usedNow) data.LastUseTime = Game.TotalElapsedGameTime;
            if (SPELL_COOLDOWN_ACTIVATED && usedNow) data.Books[data.BookId].Spells[data.SpellId].LastUseTime = Game.TotalElapsedGameTime;
        }

        private void TriggerExplosion(ProjData proj)
        {
            if (proj.Explosive)
            {
                switch (proj.ExplosionType)
                {
                    case "fire_big":
                        float X = rand.Next(314) / 100;
                        Game.SpawnFireNodes(
                            new Vector2(proj.ProjObject.GetWorldPosition().X, proj.ProjObject.GetWorldPosition().Y),
                            80,
                            new Vector2((float)Math.Cos(X) * 20, (float)Math.Sin(X) * 20),
                            0.1f,
                            0.1f,
                            FireNodeType.Flamethrower);

                        Game.SpawnFireNodes(
                            new Vector2(proj.ProjObject.GetWorldPosition().X, proj.ProjObject.GetWorldPosition().Y),
                            80,
                            new Vector2(-(float)Math.Cos(X) * 20, -(float)Math.Sin(X) * 20),
                            0.1f,
                            0.1f,
                            FireNodeType.Flamethrower);

                        Game.TriggerExplosion(proj.ProjObject.GetWorldPosition());
                        Game.TriggerFireplosion(proj.ProjObject.GetWorldPosition(), 10f);
                        break;

                    case "cluster":
                        for (int i = 0; i <= 2; i++)
                        {
                            Vector2 vel = new Vector2((float)rand.Next(-10, 10), (float)rand.Next(2, 10));
                            Vector2 pos = proj.ProjObject.GetWorldPosition();
                            IObject obj = Game.CreateObject(
                                "CrumpledPaper00",
                                pos,
                                0f,
                                vel,
                                (float)rand.Next(-30, 30));
                            ProjData projData = new ProjData(obj, Game.TotalElapsedGameTime + (float)(rand.Next(10, 20)) * 100f);
                            projData.Explosive = true;
                            projData.ExplosionType = "basic";
                            projData.SmokingAmount = 2;
                            projData.SmokingRadius = 2;
                            projs.Add(projData);
                            Game.PlaySound("Break", pos, 50f);
                        }
                        break;

                    case "poison_gaz":
                        {
                            Vector2 pos = proj.ProjObject.GetWorldPosition();
                            for (int x = -2; x <= 2; x++)
                            {
                                for (int y = -2; y <= 2; y++)
                                {
                                    IObjectTrigger areaTrigger = ((IObjectTrigger)Game.CreateObject(
                                        "AreaTrigger",
                                        pos + new Vector2((x * 10f), (y * 10f)),
                                        0
                                    ));
                                    areaTrigger.SetScriptMethod("SpellTrigger_PoisonLight");
                                    ProjData projData = new ProjData(areaTrigger, Game.TotalElapsedGameTime + 8000f);
                                    if (x * x + y * y < 4)
                                    {
                                        projData.SmokingAmount = 1;
                                        projData.SmokingRadius = 16;
                                    }
                                    projs.Add(projData);
                                }
                            }
                            proj.ProjObject.Destroy();
                            for (int j = 0; j < 10; j++)
                                Game.PlayEffect("TR_D", pos + new Vector2(rand.Next(-8, 8), rand.Next(-8, 8)));
                        }
                        break;

                    case "basic":
                    default:
                        Game.TriggerExplosion(proj.ProjObject.GetWorldPosition());
                        break;
                }
            }
        }

        // | these methods are used by spells
        // | which create temporary triggers
        // V
        IPlayer m_mindLocker = null;
        public void SpellTrigger_MindLock(TriggerArgs args)
        {
            if (args.Sender is IPlayer)
            {
                IPlayer ply = (IPlayer)args.Sender;
                if (ply == m_mindLocker) return;
                if (ply.GetUser() != null)
                {
                    PlayerData data = GetPlayerData((IPlayer)args.Sender);
                    data.IsFrozen = true;
                    data.UnFreezeTime = Game.TotalElapsedGameTime + 5000f;
                    ply.SetInputEnabled(false);
                }
                else
                {
                    ply.SetBotBehavior(new BotBehavior());
                }
                Game.PlayEffect("Electric", ply.GetWorldPosition() + new Vector2(0, 8f));
            }
        }

        // commented is old player pushing system
        public void SpellTrigger_FusRoDah_Right(TriggerArgs args)
        {
            if (args.Sender is IPlayer)
            {
                IPlayer ply = (IPlayer)args.Sender;

                // side

                /* ply.SetWorldPosition(ply.GetWorldPosition() + new Vector2(0, 10));
                IObject obj = Game.CreateObject("InvisibleBlock",
                    ply.GetWorldPosition() + new Vector2(-6, 0),
                    -1.2f);
                obj.SetBodyType(BodyType.Dynamic);
        //		obj.SetStickyFeet(true);
                obj.SetLinearVelocity(new Vector2(12, 8));
                obj.CustomId = "PushBlock";
                projs.Add(new ProjData(obj, Game.TotalElapsedGameTime + 50f));*/

                //ply.SetWorldPosition(ply.GetWorldPosition() + new Vector2(0, 10));
                ply.SetLinearVelocity(new Vector2(15f, 3f));

                Game.PlayEffect("TR_D", ply.GetWorldPosition() + new Vector2(rand.Next(4, 16), rand.Next(4, 16)));
            }
            else if (args.Sender is IObject)
            {
                IObject obj = (IObject)args.Sender;
                obj.SetLinearVelocity(new Vector2(15f, 2f));
                for (int i = 0; i < 8; i++)
                    Game.PlayEffect("TR_D", obj.GetWorldPosition() + new Vector2(rand.Next(4, 16), rand.Next(-6, 6)));
            }
        }

        public void SpellTrigger_FusRoDah_Left(TriggerArgs args)
        {
            if (args.Sender is IPlayer)
            {
                IPlayer ply = (IPlayer)args.Sender;

                // side
                /* ply.SetWorldPosition(ply.GetWorldPosition() + new Vector2(0, 10));
                IObject obj = Game.CreateObject("InvisibleBlock",
                    ply.GetWorldPosition() + new Vector2(6, 0),
                    1.2f);
                obj.SetBodyType(BodyType.Dynamic);
        //		obj.SetStickyFeet(true);
                obj.SetLinearVelocity(new Vector2(-12, 8));
                obj.CustomId = "PushBlock";
                projs.Add(new ProjData(obj, Game.TotalElapsedGameTime + 50f));*/

                //ply.SetWorldPosition(ply.GetWorldPosition() + new Vector2(0, 10));
                ply.SetLinearVelocity(new Vector2(-15f, 3f));

                Game.PlayEffect("TR_D", ply.GetWorldPosition() + new Vector2(rand.Next(-16, 4), rand.Next(4, 16)));
            }
            else if (args.Sender is IObject)
            {
                IObject obj = (IObject)args.Sender;
                obj.SetLinearVelocity(new Vector2(-15f, 2f));
                for (int i = 0; i < 8; i++)
                    Game.PlayEffect("TR_D", obj.GetWorldPosition() + new Vector2(rand.Next(4, 16), rand.Next(-6, 6)));
            }
        }

        public void SpellTrigger_ToTheMoon(TriggerArgs args)
        {
            if (args.Sender is IPlayer)
            {
                IPlayer ply = (IPlayer)args.Sender;
                //		ply.SetWorldPosition(ply.GetWorldPosition() + new Vector2(0f, 24f));
                //		ply.SetLinearVelocity(new Vector2(0,-50f));
                ply.SetLinearVelocity(new Vector2(0f, 15f));
            }
            else if (args.Sender is IObject)
            {
                IObject obj = (IObject)args.Sender;
                obj.SetLinearVelocity(new Vector2(0, 14f));
                for (int i = 0; i < 8; i++)
                    Game.PlayEffect("TR_D", obj.GetWorldPosition() + new Vector2(rand.Next(-6, 6), rand.Next(4, 16)));
            }
        }

        PlayerTeam m_reviveTeam = PlayerTeam.Independent;
        float m_nextRevive = 0f;
        public void SpellTrigger_Revive(TriggerArgs args)
        {
            if (args.Sender is IPlayer)
            {
                IPlayer ply = (IPlayer)args.Sender;
                if (ply.IsDead && GetPlayerData(ply).Player != null && Game.TotalElapsedGameTime > m_nextRevive)
                {
                    foreach (IObject obj in Game.GetObjectsByCustomId("ReviveTrigger")) obj.Remove();

                    PlayerData data = GetPlayerData(ply);
                    if (UserStillHere(data.User))
                    {
                        float health = (float)rand.Next(15, 40);

                        IPlayer revivedPlayer = Game.CreatePlayer(ply.GetWorldPosition());
                        revivedPlayer.SetUser(data.User);
                        revivedPlayer.SetProfile(data.User.GetProfile());
                        revivedPlayer.SetHealth(health);
                        revivedPlayer.SetTeam(m_reviveTeam);
                        data.Player.Remove();
                        data.Player = revivedPlayer;
                        data.Team = m_reviveTeam;

                        GetPlayerData(revivedPlayer).StartHealth = health;
                        GetPlayerData(revivedPlayer).Player = revivedPlayer;

                        Game.PlayEffect("CFTXT", revivedPlayer.GetWorldPosition() + new Vector2(0f, 30f), "REVIVE!");
                        Game.PlayEffect("Electric", revivedPlayer.GetWorldPosition());
                    }
                    else
                    {
                        Game.PlayEffect("CFTXT", ply.GetWorldPosition() + new Vector2(0f, 30f), "Player left!");
                        ply.Gib();
                    }

                    m_nextRevive = Game.TotalElapsedGameTime + 50f;
                }
            }
        }

        PlayerData m_mindController;
        float m_nextMindControl = 0f;
        public void SpellTrigger_MindControl(TriggerArgs args)
        {
            if (args.Sender is IPlayer)
            {
                IPlayer body = (IPlayer)args.Sender;

                foreach (SpellsData_MindControl data in controlledPlayers)
                    if (data.Player == body) return;

                if (!body.IsDead && Game.TotalElapsedGameTime > m_nextMindControl && body != m_mindController.Player)
                {
                    foreach (IObject obj in Game.GetObjectsByCustomId("MindControlTrigger")) obj.Remove();

                    controlledPlayers.Add(new SpellsData_MindControl(body, body.GetUser(), m_mindController, Game.TotalElapsedGameTime + 7000f, m_mindController.Player.GetHealth()));

                    Game.PlayEffect("Electric", body.GetWorldPosition() + new Vector2(0, 8f));
                    Game.PlayEffect("Electric", m_mindController.Player.GetWorldPosition() + new Vector2(0, 8f));

                    // set user for victim's body
                    body.SetUser(m_mindController.User);

                    // delete controller's player and data (data is removed in Death() method immediately)
                    m_mindController.Player.Remove();
                }
                m_nextMindControl = Game.TotalElapsedGameTime + 50f;
            }
        }

        private void Tick_Spell_MindControl()
        {
            for (int i = controlledPlayers.Count - 1; i >= 0; i--)
            {
                SpellsData_MindControl mindData = controlledPlayers[i];
                PlayerData contData = mindData.ControllerData;
                IPlayer victPlayer = mindData.Player;
                IUser victUser = mindData.User;

                if (victPlayer.IsRemoved)
                {
                    // if player body is removed, delete entry
                    controlledPlayers.RemoveAt(i);
                    CheckGameover();
                    // exit body with walk
                }
                else if (mindData.EndTime <= Game.TotalElapsedGameTime || (victPlayer.IsWalking && (victPlayer.IsKicking || victPlayer.IsJumpKicking)))
                {
                    Game.PlayEffect("Electric", victPlayer.GetWorldPosition() + new Vector2(0, 8f));

                    // if enough time passed, return user
                    int plyId = GetPlayerDataIndex(victPlayer);
                    if (victUser != null)
                    {
                        victPlayer.SetUser(victUser);
                    }
                    // if player is not dead, make controller "exit"
                    if (!victPlayer.IsDead)
                    {
                        IPlayer controller = Game.CreatePlayer(victPlayer.GetWorldPosition() + new Vector2(victPlayer.FacingDirection * 6f, 0));
                        controller.SetHealth(mindData.ControllerHealth);
                        if (contData.User != null)
                        {
                            Game.PlayEffect("Electric", controller.GetWorldPosition() + new Vector2(0, 8f));
                            contData.Player = controller;
                            contData.LastHealth = mindData.ControllerHealth;
                            contData.StartHealth = contData.LastHealth;
                            controller.SetUser(contData.User);
                            controller.SetProfile(contData.User.GetProfile());
                            controller.SetTeam(contData.Team);
                            players.Add(contData);
                        }
                        else
                        {
                            controller.Kill();
                        }
                    }
                    else
                    {
                        Game.PlayEffect("CFTXT", victPlayer.GetWorldPosition() + new Vector2(0f, 30f), "Cannot exit\ndead body");
                    }

                    controlledPlayers.RemoveAt(i);
                    CheckGameover();
                }
            }
        }

        public void SpellTrigger_Teleport(TriggerArgs args)
        {
            if (args.Sender is IObject)
            {
                IObject obj = (IObject)args.Sender;
                IObject trig = (IObject)args.Caller;

                if (!obj.Name.StartsWith("Bg"))
                {
                    for (int i = pendingTeleports.Count - 1; i >= 0; i--)
                    {
                        if (pendingTeleports[i].Trigger == trig)
                        {
                            pendingTeleports[i].Trigger.Remove();
                            pendingTeleports[i].Allowed = false;
                            Game.PlayEffect("CFTXT", pendingTeleports[i].Player.GetWorldPosition() + new Vector2(0f, 30f), "Can't teleport");
                            for (int j = 0; j < 15; j++) Game.PlayEffect("TR_D", pendingTeleports[i].Position + new Vector2(rand.Next(-6, 6), rand.Next(-6, 6)));
                        }
                    }
                }
            }
        }

        private void Tick_Spell_Teleport()
        {
            for (int i = pendingTeleports.Count - 1; i >= 0; i--)
            {
                SpellsData_Teleport data = pendingTeleports[i];

                if (data.Trigger.IsRemoved)
                {
                    if (data.Allowed)
                    {
                        Game.PlayEffect("Electric", data.Player.GetWorldPosition() + new Vector2(0, 8f));
                        data.Player.SetWorldPosition(data.Position);
                        data.Player.SetLinearVelocity(new Vector2(0f, 0f));
                        Game.PlayEffect("Electric", data.Player.GetWorldPosition() + new Vector2(0, 8f));
                        players[GetPlayerDataIndex(data.Player)].NextUseTime = Game.TotalElapsedGameTime + 2000f;
                    }
                    pendingTeleports.RemoveAt(i);
                }
            }
        }

        public void SpellTrigger_PoisonLight(TriggerArgs args)
        {
            if (args.Sender is IPlayer)
            {
                if (((IPlayer)args.Sender).GetUser() != null && !((IPlayer)args.Sender).IsDead)
                {
                    PlayerData data = GetPlayerData((IPlayer)args.Sender);
                    data.Poison = 20f;
                    Game.PlayEffect("CFTXT", ((IPlayer)args.Sender).GetWorldPosition() + new Vector2(0f, 30f), "POISONED!");
                }
            }
        }

        private void CheckGameover()
        {
            if (Game.GetActiveUsers().Length < 2)
            {
                IPlayer[] plys = Game.GetPlayers();
                if (plys.Length == 0 || plys[0].IsDead) Game.SetGameOver("You died");
                return;
            }
            else
            {
                List<PlayerTeam> teams = new List<PlayerTeam>();
                String winner = "";

                foreach (PlayerData data in players)
                    if (!data.Player.IsRemoved && !data.Player.IsDead && !teams.Contains(data.Team)) teams.Add(data.Team);

                foreach (SpellsData_MindControl MindData in controlledPlayers)
                {
                    PlayerData contData = MindData.ControllerData;
                    if (UserStillHere(contData.User) && !teams.Contains(contData.Team)) teams.Add(contData.Team);
                }

                switch (teams.Count)
                {
                    case 5:
                        Game.ShowPopupMessage("Wut?");
                        break;
                    case 4:
                    case 3:
                    case 2:
                        return;
                    case 1:
                        if (teams[0] != PlayerTeam.Independent)
                        {
                            switch (teams[0])
                            {
                                case PlayerTeam.Team1: winner = "Blue team"; break;
                                case PlayerTeam.Team2: winner = "Red team"; break;
                                case PlayerTeam.Team3: winner = "Green team"; break;
                                case PlayerTeam.Team4: winner = "Yellow team"; break;
                            }
                        }
                        else
                        {
                            List<IPlayer> plysAlive = new List<IPlayer>();
                            foreach (IPlayer ply in Game.GetPlayers())
                                if (!ply.IsDead) plysAlive.Add(ply);
                            foreach (SpellsData_MindControl MindData in controlledPlayers)
                            {
                                PlayerData contData = MindData.ControllerData;
                                if (UserStillHere(contData.User)) plysAlive.Add(contData.Player);
                            }
                            if (plysAlive.Count == 1)
                                winner = plysAlive[0].GetProfile().Name;
                            else return;
                        }
                        Game.SetGameOver(winner + " wins!");
                        break;
                    case 0:
                        Game.SetGameOver("Draw");
                        break;
                }
            }
        }

        //==================================================================//
        //============================< HELPERS >===========================//
        //==================================================================//

        private void ModMessage(string text)
        {
            Game.RunCommand("/MSG #Wizards mod: " + text);
        }

        // get player data by IPlayer
        private PlayerData GetPlayerData(IPlayer ply)
        {
            for (int i = players.Count - 1; i >= 0; i--)
            {
                if (players[i].Player == ply) return players[i];
            }
            return new PlayerData();
        }

        private int GetPlayerDataIndex(IPlayer ply)
        {
            for (int i = players.Count - 1; i >= 0; i--)
                if (ply == players[i].Player) return i;
            return 0;
        }

        private bool UserStillHere(IUser user)
        {
            foreach (IUser usr in Game.GetActiveUsers())
                if (usr.UserId == user.UserId) return true;
            return false;
        }

        // create timer trigger
        private void CreateTimer(int interval, int count, string method, string id)
        {
            IObjectTimerTrigger timerTrigger = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
            timerTrigger.SetIntervalTime(interval);
            timerTrigger.SetRepeatCount(count);
            timerTrigger.SetScriptMethod(method);
            timerTrigger.CustomId = id;
            timerTrigger.Trigger();
        }

        private Color GetTeamColor(PlayerTeam team)
        {
            if (team == PlayerTeam.Team1) return new Color(124, 124, 246);
            if (team == PlayerTeam.Team2) return new Color(245, 78, 78);
            if (team == PlayerTeam.Team3) return new Color(5, 219, 5);
            if (team == PlayerTeam.Team4) return new Color(214, 214, 0);
            return new Color(255, 255, 255);
        }

        private void GiveBasicSpells(PlayerData data)
        {
            {   // light destruction spellbook
                List<PlayerData.BookData.SpellData> temp = new List<PlayerData.BookData.SpellData>();

                //temp.Add(new PlayerData.BookData.SpellData("Tarot Card", 20f, 200f));
                temp.Add(new PlayerData.BookData.SpellData("FirePunch", 40f, 1000f));
                temp.Add(new PlayerData.BookData.SpellData("FireBall", 30f, 1000f));
                temp.Add(new PlayerData.BookData.SpellData("ANVIL", 40f, 5000f));
                temp.Add(new PlayerData.BookData.SpellData("Paper", 40f, 500f));

                data.Books.Add(new PlayerData.BookData("Light destruction", temp));
            }
            {   // heavy destruction spellbook
                List<PlayerData.BookData.SpellData> temp = new List<PlayerData.BookData.SpellData>();

                temp.Add(new PlayerData.BookData.SpellData("Bottle", 65f, 500f));
                temp.Add(new PlayerData.BookData.SpellData("Balloon", 100f, 500f));
                temp.Add(new PlayerData.BookData.SpellData("Crate of doom", 60f, 10000f));
                temp.Add(new PlayerData.BookData.SpellData("Fireworks", 125f, 0f));
                temp.Add(new PlayerData.BookData.SpellData("Skunk", 50f, 1000f));

                data.Books.Add(new PlayerData.BookData("Heavy destruction", temp));
            }
            {   // protection spellbook
                List<PlayerData.BookData.SpellData> temp = new List<PlayerData.BookData.SpellData>();

                temp.Add(new PlayerData.BookData.SpellData("Heal", 25f, 500f));
                temp.Add(new PlayerData.BookData.SpellData("Cloud", 35f, 500f));
                temp.Add(new PlayerData.BookData.SpellData("Shield", 75f, 15000f));
                temp.Add(new PlayerData.BookData.SpellData("Revive", 100f, 20000f));

                data.Books.Add(new PlayerData.BookData("Protection", temp));
            }
            {   // alteration spellbook
                List<PlayerData.BookData.SpellData> temp = new List<PlayerData.BookData.SpellData>();

                temp.Add(new PlayerData.BookData.SpellData("FUS", 70f, 15000f));
                temp.Add(new PlayerData.BookData.SpellData("SuperJump", 30f, 1000f));
                temp.Add(new PlayerData.BookData.SpellData("To The Moon!", 40f, 8000f));
                temp.Add(new PlayerData.BookData.SpellData("Mind Lock", 100f, 20000f));
                temp.Add(new PlayerData.BookData.SpellData("Mind Control", 150f, 40000f));
                temp.Add(new PlayerData.BookData.SpellData("Teleport", 60f, 1000f));

                data.Books.Add(new PlayerData.BookData("Alteration", temp));
            }

            // disabled spells
            //		temp.Add(new PlayerData.BookData.SpellData("Demigod", 75, 10000f));
            //		temp.Add(new PlayerData.BookData.SpellData("Block", 25f, 500f));
            //		temp.Add(new PlayerData.BookData.SpellData("Minion", 100, 30000f));
        } 
        #endregion
    }
}