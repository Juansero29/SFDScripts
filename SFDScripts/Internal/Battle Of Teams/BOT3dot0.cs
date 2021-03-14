using SFDGameScriptInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFDScripts.Internal
{
    class BOT3dot0 : GameScriptInterface
    {
        public BOT3dot0() : base(Game) { }

		#region Script To Copy
		//===========================================================//
		//=========<      DEATHMATCH GAMEMODE BY CHELOG      >=======//
		//=========<             MODIFIED BY GURT            >=======//
		//=========<                 V 0.2.2                 >=======//
		//===========================================================//
		//======================< DM - settings >====================//
		private const int DAMAGE_LIMIT = 5000;                // DeathLimit after that round will restart (only integer)
		private const int USER_RESPAWN_DELAY_MS = 10000;        // Time in ms after a player will respawn
		private const bool GIB_CORPSES = false;               // if set to "true" - gib corpses; "false" - remove corpses
															  //===========================================================//

		int TimeCDT = 5 * 60;
		int TimeM;
		int TimeS = 00;
		bool TextF = false;

		public void TimeRemain(TriggerArgs args)
		{
			IObjectText TimeText = (IObjectText)Game.GetSingleObjectByCustomId("TextCD");
			if (Game.GetPlayers().Length > 1)
			{
				if (TimeS == 00)
				{
					TimeM--;
					TimeS = 59;
				}
				if (TimeText != null && Game.GetPlayers().Length > 1)
				{
					if (TextF == false)
					{
						TimeS--;
						string text = string.Format("Time remaining {0}:{1:00}", TimeM.ToString(), TimeS.ToString());
						TimeText.SetText(text);
					}

					if (TimeM == 0 && TimeS == 0)
					{
						WhoWins();
						blueFDamage = blueDamage;
						redFDamage = redDamage;
						Game.GetSingleObjectByCustomId("timer").Destroy();
					}
				}
			}
			else
			{
				TimeText.SetText("Time remaining " + TimeM.ToString() + ":" + "00");
			}
		}

		public void Start(TriggerArgs args)
		{
			TimeM = TimeCDT / 60;
			ConnectedPlayersTick(args);
			RefreshCounter("Red score: ", blueDamage, "RedT");
			RefreshCounter("Blue score: ", redDamage, "BlueT");
			wee = ((IObject)Game.GetSingleObjectByCustomId("crate"));
			here = wee.GetWorldPosition();
			((IObjectTimerTrigger)Game.GetSingleObjectByCustomId("Booom")).SetIntervalTime(RandNumber(10000, 5 * 60000));
			((IObjectTimerTrigger)Game.GetSingleObjectByCustomId("ActTrig")).SetIntervalTime(RandNumber(10000, 5 * 50000));
			((IObjectTimerTrigger)Game.GetSingleObjectByCustomId("Booom")).Trigger();
			((IObjectTimerTrigger)Game.GetSingleObjectByCustomId("ActTrig")).Trigger();
			foreach (IPlayer plyr in Game.GetPlayers())
			{
				players.Add(plyr, PlayerTeam.Team3);
				plyr.SetTeam(PlayerTeam.Team3);
			}
		}


		public void Cleaningup()
		{
			IObject[] objectsToRemove = Game.GetObjectsByName(new string[] { "GIBLET00", "GIBLET01", "GIBLET02", "GIBLET03", "GIBLET04" });
			for (int i = 0; i < objectsToRemove.Length; i++)
			{
				objectsToRemove[i].Remove();
			}
		}

		public void TimeVerdin(TriggerArgs args)
		{
			IPlayer tiozinho = (IPlayer)args.Sender;
			tiozinho.SetTeam(PlayerTeam.Team3);
		}

		public void ClearPopup(TriggerArgs args)
		{
			Game.HidePopupMessage();
		}

		int vRedKills = 0;

		int vBluKills = 0;

		double blueFDamage;

		double redFDamage;

		public void WhoWins()
		{
			IObjectText TimeText = (IObjectText)Game.GetSingleObjectByCustomId("TextCD");

			blueFDamage = Math.Round(blueDamage);
			redFDamage = Math.Round(redDamage);

			if (redFDamage > blueFDamage)
			{
				Game.SetGameOver("The Red wins");
				TextF = true;
				TimeText.SetText("Finished, space for the next one" + "\n             <-- " + blueFDamage + " X " + redFDamage + " -->");
			}

			if (blueFDamage > redFDamage)
			{
				Game.SetGameOver("The Blue wins");  // if limit is exceeded, restart round
				TextF = true;
				TimeText.SetText("Finished, space for the next one" + "\n             <-- " + blueFDamage + " X " + redFDamage + " -->");
			}

			if (blueFDamage == redFDamage)
			{
				Game.SetGameOver("Draw!");  // if limit is exceeded, restart round
				TextF = true;
				TimeText.SetText("Finished, space for the next one" + "\n             <-- " + blueFDamage + " X " + redFDamage + " -->");
			}
		}



		//--//

		private IObjectTrigger trig2 = null;
		private IObjectTrigger trig3 = null;
		private IObjectTrigger trig4 = null;
		private IPlayer ply = null;
		private IPlayer plyExit = null;
		private bool allowOpen = false;

		public void CheckEnter(TriggerArgs args)
		{
			if ((args.Sender != null) && (args.Sender is IPlayer))
			{
				allowOpen = true;
			}
		}

		Vector2 here;
		IObject wee;
		bool didit = false;

		public void crateOpen(TriggerArgs args)
		{
			if (wee.GetHealth() == 0 && didit == false)
			{
				Game.SpawnWeaponItem(WeaponItem.M60, here, true, 10000);
				didit = true;
				Game.GetSingleObjectByCustomId("IT").Destroy();
			}
			else { here = wee.GetWorldPosition(); }
		}
		//--//

		String VBlueRnd;

		public void BlueRnd()
		{
			int whereB = RandNumber(0, 4);
			string[] GetposB = new string[] { "BluBase", "BluBase2", "BluBase3", "BluBase4", "BluBase5" };
			VBlueRnd = (GetposB[whereB]);
		}

		String VRedRnd;

		public void RedRnd()
		{
			int whereR = RandNumber(0, 4);
			String[] GetposR = new string[] { "RediBase", "RedBase2", "RedBase3", "RedBase4", "RedBase5" };
			VRedRnd = (GetposR[whereR]);
		}

		public void Booom(TriggerArgs args)
		{
			Vector2 heya = Game.GetSingleObjectByCustomId("booom").GetWorldPosition();
			Game.PlayEffect("EXP", heya, 1);
			Game.PlaySound("EXP", heya, 1);
			Game.TriggerExplosion((Game.GetSingleObjectByCustomId("Blash")).GetWorldPosition());
			((IObjectTrigger)Game.GetSingleObjectByCustomId("Blash")).Trigger();
			((IObjectTrigger)Game.GetSingleObjectByCustomId("Blash")).Trigger();
			((IObjectTrigger)Game.GetSingleObjectByCustomId("Blash")).Trigger();
			((IObjectTrigger)Game.GetSingleObjectByCustomId("Shuuu")).Trigger();

		}

		public void CheckD(TriggerArgs args)
		{
			if (((IObject)args.Sender).CustomId == "Dogs")
			{
				((IObjectTrigger)Game.GetSingleObjectByCustomId("tatattt")).SetEnabled(false);
				((IObjectTrigger)Game.GetSingleObjectByCustomId("ActTrig")).SetEnabled(false);
				Game.GetSingleObjectByCustomId("toHere").Destroy();
				Game.GetSingleObjectByCustomId("fromHere").Destroy();
			}
		}

		public void Bomb(TriggerArgs args)
		{
			if (((IObject)args.Sender).CustomId == "fromHere")
			{
				(Game.GetSingleObjectByCustomId("BOMB!")).SetBodyType(BodyType.Dynamic);
			}
		}

		public void getDoown(TriggerArgs args)
		{
			if (((IObject)args.Sender).CustomId != "BOMB!")
			{
				Game.TriggerExplosion((Game.GetSingleObjectByCustomId("BOMB!")).GetWorldPosition());
				((IObjectTrigger)Game.GetSingleObjectByCustomId("blashBoom")).Trigger();
				(Game.GetSingleObjectByCustomId("BOMB")).Destroy();
				(Game.GetSingleObjectByCustomId("BOMB!")).Destroy();
				(Game.GetSingleObjectByCustomId("BOMB!!")).Destroy();
				(Game.GetSingleObjectByCustomId("tester")).Destroy();
			}
		}

		bool didIt = false;
		public void tattattaaa(TriggerArgs args)
		{
			Vector2 gunPosition = ((IObject)Game.GetSingleObjectByCustomId("fromHere")).GetWorldPosition() + new Vector2(0f, -5f);
			Vector2 Tohere = ((IObject)Game.GetSingleObjectByCustomId("toHere")).GetWorldPosition() - gunPosition;
			if (gunPosition != null && Tohere != null)
			{
				Game.SpawnProjectile(ProjectileItem.SNIPER, gunPosition, Tohere);
			}
			if (!didIt)
			{
				IObjectRailAttachmentJoint rail = (IObjectRailAttachmentJoint)Game.GetSingleObjectByCustomId("Dog");
				rail.SetMotorEnabled(true);
				IObjectRailAttachmentJoint rails = (IObjectRailAttachmentJoint)Game.GetSingleObjectByCustomId("Dogs");
				rails.SetMotorEnabled(true);
				didIt = true;
			}
		}


		public void refil(TriggerArgs args)
		{
			IPlayer helped = (IPlayer)args.Sender;
			helped.GiveWeaponItem(WeaponItem.PILLS);
			helped.GiveWeaponItem(helped.CurrentSecondaryWeapon.WeaponItem);
			if (helped.CurrentPrimaryWeapon.WeaponItem != WeaponItem.MAGNUM)
			{
				helped.GiveWeaponItem(helped.CurrentPrimaryWeapon.WeaponItem);
			}
		}

		public void ClassChooser(TriggerArgs args)
		{
			IPlayer sender = (IPlayer)args.Sender;
			if ((sender != null) && (sender is IPlayer) && (!sender.IsDiving))
			{
				IObject Clas = (IObject)args.Caller;
				switch (Clas.CustomId)
				{
					case "a":
						sender = (IPlayer)args.Sender;
						removeWeapons(sender);
						sender.GiveWeaponItem(WeaponItem.ASSAULT);
						sender.GiveWeaponItem(WeaponItem.UZI);
						sender.GiveWeaponItem(WeaponItem.ASSAULT);
						sender.GiveWeaponItem(WeaponItem.UZI);
						sender.GiveWeaponItem(WeaponItem.ASSAULT);
						sender.GiveWeaponItem(WeaponItem.UZI);
						sender.GiveWeaponItem(WeaponItem.SLOWMO_5);
						if (sender.GetTeam() == PlayerTeam.Team1) { getAway(VBlueRnd, sender); }
						else if (sender.GetTeam() == PlayerTeam.Team2) { getAway(VRedRnd, sender); }
						break;


					case "s":
						sender = (IPlayer)args.Sender;
						removeWeapons(sender);
						sender.GiveWeaponItem(WeaponItem.MACHETE);
						sender.GiveWeaponItem(WeaponItem.MAGNUM);
						sender.GiveWeaponItem(WeaponItem.SLOWMO_5);
						sender.GiveWeaponItem(WeaponItem.MACHETE);
						sender.GiveWeaponItem(WeaponItem.MAGNUM);
						sender.GiveWeaponItem(WeaponItem.SLOWMO_5);
						if (sender.GetTeam() == PlayerTeam.Team1) { getAway(VBlueRnd, sender); }
						else if (sender.GetTeam() == PlayerTeam.Team2) { getAway(VRedRnd, sender); }
						break;


					case "d":
						sender = (IPlayer)args.Sender;
						removeWeapons(sender);
						sender.GiveWeaponItem(WeaponItem.KATANA);
						sender.GiveWeaponItem(WeaponItem.SAWED_OFF);
						sender.GiveWeaponItem(WeaponItem.SAWED_OFF);
						sender.GiveWeaponItem(WeaponItem.REVOLVER);
						sender.GiveWeaponItem(WeaponItem.REVOLVER);
						sender.GiveWeaponItem(WeaponItem.SLOWMO_5);
						if (sender.GetTeam() == PlayerTeam.Team1) { getAway(VBlueRnd, sender); }
						else if (sender.GetTeam() == PlayerTeam.Team2) { getAway(VRedRnd, sender); }
						break;


					case "f":
						sender = (IPlayer)args.Sender;
						removeWeapons(sender);
						sender.GiveWeaponItem(WeaponItem.SNIPER);
						sender.GiveWeaponItem(WeaponItem.PISTOL);
						sender.GiveWeaponItem(WeaponItem.PISTOL);
						sender.GiveWeaponItem(WeaponItem.SNIPER);
						sender.GiveWeaponItem(WeaponItem.SLOWMO_10);
						if (sender.GetTeam() == PlayerTeam.Team1) { getAway(VBlueRnd, sender); }
						else if (sender.GetTeam() == PlayerTeam.Team2) { getAway(VRedRnd, sender); }

						break;


					case "g":
						sender = (IPlayer)args.Sender;
						removeWeapons(sender);
						sender.GiveWeaponItem(WeaponItem.PIPE);
						sender.GiveWeaponItem(WeaponItem.SHOTGUN);
						sender.GiveWeaponItem(WeaponItem.REVOLVER);
						sender.GiveWeaponItem(WeaponItem.REVOLVER);
						sender.GiveWeaponItem(WeaponItem.SHOTGUN);
						sender.GiveWeaponItem(WeaponItem.GRENADES);
						sender.GiveWeaponItem(WeaponItem.SLOWMO_10);
						if (sender.GetTeam() == PlayerTeam.Team1) { getAway(VBlueRnd, sender); }
						else if (sender.GetTeam() == PlayerTeam.Team2) { getAway(VRedRnd, sender); }

						break;


					case "h":
						sender = (IPlayer)args.Sender;
						removeWeapons(sender);
						sender.GiveWeaponItem(WeaponItem.AXE);
						sender.GiveWeaponItem(WeaponItem.SUB_MACHINEGUN);
						sender.GiveWeaponItem(WeaponItem.SUB_MACHINEGUN);
						sender.GiveWeaponItem(WeaponItem.SUB_MACHINEGUN);
						sender.GiveWeaponItem(WeaponItem.PISTOL);
						sender.GiveWeaponItem(WeaponItem.PISTOL);
						sender.GiveWeaponItem(WeaponItem.SLOWMO_5);
						if (sender.GetTeam() == PlayerTeam.Team1) { getAway(VBlueRnd, sender); }
						else if (sender.GetTeam() == PlayerTeam.Team2) { getAway(VRedRnd, sender); }
						break;


					default:
						Game.ShowPopupMessage("Something is wrong, Check the ClassChooser void");
						break;
				}
			}
		}


		public void getAway(String id, IPlayer movingPlayer)
		{
			IObject FinalDestination = Game.GetSingleObjectByCustomId(id);
			Vector2 wee = FinalDestination.GetWorldPosition();
			movingPlayer.SetWorldPosition(wee);
		}

		private IPlayer Staffplayer = null;
		int bluTeam = 0;
		int redTeam = 0;


		public void MovetoBase(TriggerArgs args)
		{
			IPlayer randomGuy = (IPlayer)args.Sender;
			checkTeams();
			if ((randomGuy != null) && (randomGuy is IPlayer) && (!randomGuy.IsDiving))
			{
				IObject Where = (IObject)args.Caller;
				IUser user = randomGuy.GetUser();

				switch (Where.CustomId)
				{
					case "RedBase":
						if (bluTeam >= redTeam)
						{
							getAway("Red", randomGuy);
							randomGuy.SetTeam(PlayerTeam.Team2);
							players[randomGuy] = PlayerTeam.Team2;
						}

						break;

					case "BlueBase":
						if (bluTeam <= redTeam)
						{
							getAway("Blue", randomGuy);
							randomGuy.SetTeam(PlayerTeam.Team1);
							players[randomGuy] = PlayerTeam.Team1;
						}

						break;

					case "Staff-joiner":
						if (randomGuy.GetProfile().Name == "#ShutDownMan")
						{
							Staffplayer = randomGuy;
							getAway("Staff", randomGuy);
							randomGuy.SetTeam(PlayerTeam.Team4);
						}

						break;

					default:
						Game.ShowPopupMessage("Something is wrong, Check the MovetoBase void");
						break;
				}
			}
		}


		public static int RandNumber(int Low, int High)
		{
			Random rndNum = new Random();

			int rnd = rndNum.Next(Low, High);

			return rnd;
		}

		public void XstroyGun(TriggerArgs args)
		{
			if (args.Sender is IObjectWeaponItem)
			{
				IObject destroyingGun = (IObject)args.Sender;
				destroyingGun.Remove();

			}
		}

		public void removeWeapons(IPlayer pleyrr)
		{
			pleyrr.RemoveWeaponItemType(WeaponItemType.Rifle);
			pleyrr.RemoveWeaponItemType(WeaponItemType.Handgun);
			pleyrr.RemoveWeaponItemType(WeaponItemType.Melee);
			pleyrr.RemoveWeaponItemType(WeaponItemType.Thrown);
			pleyrr.SetHealth(100);
		}


		private class DeadPlayer
		{
			public float Timestamp = 0f;
			public IUser User = null;
			public IPlayer DeadBody = null;
			public PlayerTeam Team = PlayerTeam.Independent;
			public DeadPlayer(float timestamp, IUser user, IPlayer deadBody, PlayerTeam team)
			{
				this.Timestamp = timestamp;
				this.User = user;
				this.DeadBody = deadBody;
				this.Team = team;
			}
		}

		private List<DeadPlayer> m_deadPlayers = new List<DeadPlayer>();
		private int deathNum = 0;
		private int usersConnectedTickDelay = 15;

		// called each 200ms, makes a game tick
		public void Tick(TriggerArgs args)
		{

			checkTeams();
			Fly(args);
			ConnectedPlayersTick(args);
			RespawnTick(args);
			BlueRnd();
			RedRnd();
			if (Staffplayer != null)
				Staffplayer.SetHealth(100);
			CheckDamage();
		}



		// called on player's death
		public void Death(TriggerArgs args)
		{
			BlueRnd();
			RedRnd();
			IPlayer killedPlayer = (IPlayer)args.Sender;
			if (killedPlayer.GetTeam() == PlayerTeam.Team2) { bluDeathG += GetTDamage(killedPlayer); bluKills++; }
			if (killedPlayer.GetTeam() == PlayerTeam.Team1) { redDeathG += GetTDamage(killedPlayer); redKills++; }
			if (killedPlayer.GetTeam() == PlayerTeam.Team3)
			{
				Vector2 pos = killedPlayer.GetWorldPosition();
				Game.PlaySound("MenuCancel", pos, 1);
				killedPlayer.Remove();
			}
			if ((args.Sender != null) && (args.Sender is IPlayer))
			{
				deathNum++;
				if (redFDamage < DAMAGE_LIMIT || blueFDamage < DAMAGE_LIMIT && bluTeam > 0 && redTeam > 0)
				{
					IUser user = killedPlayer.GetUser();
					if (user != null)
					{
						//store user to respawn and body to remove
						m_deadPlayers.Add(new DeadPlayer(Game.TotalElapsedGameTime, user, killedPlayer, killedPlayer.GetTeam()));
					}
				}
				else { WhoWins(); }
			}
		}


		public void RespawnTick(TriggerArgs args)
		{
			if (m_deadPlayers.Count > 0)
			{
				for (int i = m_deadPlayers.Count - 1; i >= 0; i--)
				{ // traverse list backwards for easy removal of elements in list
					DeadPlayer deadPlayer = m_deadPlayers[i];
					if (deadPlayer.Timestamp + USER_RESPAWN_DELAY_MS < Game.TotalElapsedGameTime)
					{
						// time to respawn this user
						// remove entry from list over deadPlayers
						m_deadPlayers.RemoveAt(i);
						// remove old body (if any)
						if (deadPlayer.DeadBody != null)
						{
							if (GIB_CORPSES) { deadPlayer.DeadBody.Gib(); } else { (deadPlayer.DeadBody).Remove(); Cleaningup(); }
						}
						IPlayer ply = deadPlayer.User.GetPlayer();
						// respawn user
						if (CheckUserStillActive(deadPlayer.User))
						{
							if (((ply == null) || (ply.IsDead))) { SpawnUser(deadPlayer.User, deadPlayer.Team); }
						}
					}
				}
			}
		}

		int usersConnected = 0;
		//bool justConnected = true;
		public void ConnectedPlayersTick(TriggerArgs args)
		{
			if (usersConnectedTickDelay > 0)
			{
				checkTeams();
				usersConnectedTickDelay--;
				if (usersConnectedTickDelay <= 0)
				{
					IUser[] users = Game.GetActiveUsers(); // get all players
					if (usersConnected == 0) { usersConnected = users.Length; } // update amount of users at start
					else if (users.Length > usersConnected)
					{
						// check users list for non-spawned users
						for (int i = 0; i < users.Length; i++)
						{
							IPlayer ply = users[i].GetPlayer();
							if ((ply == null) || (ply.IsDead)) { SpawnUser(users[i], PlayerTeam.Team3); }
						}
					}
					usersConnected = users.Length; // update number of connected users
					usersConnectedTickDelay = 15;
				}
			}
		}




		private void SpawnUser(IUser user, PlayerTeam team)
		{
			IPlayer newPlayer = null;
			if (CheckUserStillActive(user))
			{
				Vector2 spawnPos = Game.GetSingleObjectByCustomId("Middle").GetWorldPosition();
				newPlayer = Game.CreatePlayer(spawnPos); // create a new blank player
				if (!players.ContainsKey(newPlayer)) { players.Add(newPlayer, PlayerTeam.Team3); }
				if (team == PlayerTeam.Team1)
				{
					newPlayer.SetTeam(team);
					spawnPos = Game.GetSingleObjectByCustomId("Blue").GetWorldPosition();
				}
				if (team == PlayerTeam.Team2)
				{
					newPlayer.SetTeam(team);
					spawnPos = Game.GetSingleObjectByCustomId("Red").GetWorldPosition();
				}
				if (team == PlayerTeam.Team3)
				{
					newPlayer.SetTeam(team);
					spawnPos = Game.GetSingleObjectByCustomId("Middle").GetWorldPosition();
				}
				newPlayer.SetUser(user); // set user (this will make the user control the player instance)
				newPlayer.SetProfile(user.GetProfile()); // set user's profile
				newPlayer.SetWorldPosition(spawnPos);
			}
			else if (GIB_CORPSES)
			{
				newPlayer.Gib();
			}
			else { newPlayer.Remove(); }
		}

		private bool CheckUserStillActive(IUser user)
		{
			foreach (IUser activeUser in Game.GetActiveUsers())
			{
				if (activeUser.UserId == user.UserId)
				{
					return true;
				}
			}
			return false;
		}


		private void RefreshCounter(string label, double value, string team)
		{
			IObjectText DeathCounter = (IObjectText)Game.GetSingleObjectByCustomId(team);

			if (TextF != true)
			{
				DeathCounter.SetText(label + value.ToString());

			}
		}

		public void checkTeams()
		{
			bluTeam = 0;
			redTeam = 0;
			foreach (KeyValuePair<IPlayer, PlayerTeam> ply in players)
			{
				if (ply.Value == PlayerTeam.Team1 && ply.Key.GetUser() != null) { bluTeam++; }
				if (ply.Value == PlayerTeam.Team2 && ply.Key.GetUser() != null) { redTeam++; }
			}
		}


		Dictionary<IPlayer, PlayerTeam> players = new Dictionary<IPlayer, PlayerTeam>();
		int Nn;
		float blueDamage;
		float redDamage;
		float blastDamage;
		float rlastDamage;
		float redDeathG;
		float bluDeathG;

		int bluKills;
		int redKills;

		public void CheckDamage()
		{
			foreach (KeyValuePair<IPlayer, PlayerTeam> pair in players)
			{
				if (pair.Key != null)
				{
					if (pair.Value == PlayerTeam.Team1 && !pair.Key.IsDead)
					{
						blueDamage += GetTDamage(pair.Key);
					}
					if (pair.Value == PlayerTeam.Team2 && !pair.Key.IsDead)
					{
						redDamage += GetTDamage(pair.Key);
					}
				}
			}

			blueDamage -= blastDamage - (redDeathG);
			blastDamage = blueDamage;
			redDamage -= rlastDamage - (bluDeathG);
			rlastDamage = redDamage;
			RefreshCounter("Red score: ", Math.Round(blueDamage), "RedT");
			RefreshCounter("Blue score: ", Math.Round(redDamage), "BlueT");
			RefreshCounter("Blue kills: ", bluKills, "BluKills");
			RefreshCounter("Red kills: ", redKills, "RedKills");

		}


		private float GetTDamage(IPlayer Damaged)
		{
			IPlayerStatistics pry = Damaged.Statistics;
			return pry.TotalDamageTaken;
		}


		int times = 0;
		public void Fly(TriggerArgs args)
		{
			if (Staffplayer != null)
			{
				times++;

				Vector2 worldPosition = Vector2.Zero;
				Vector2 flying = Game.GetSingleObjectByCustomId("fly").GetLinearVelocity();
				if (Staffplayer != null)
				{
					worldPosition = Staffplayer.GetWorldPosition() + new Vector2(0f, -8.15f);
					if (Staffplayer != null && times <= 10 && !(Staffplayer.IsCrouching) && (Staffplayer.IsInMidAir))
					{
						worldPosition = Staffplayer.GetWorldPosition() + new Vector2(0f, -8.15f);
						IObject createdObject = Game.CreateObject("InvisiblePlatform", worldPosition, 0f, flying, 0f);
						createdObject.CustomId = "Fly";
					}
					else
					{
						times = 0;
						IObject[] flyobject = Game.GetObjectsByCustomId("Fly");
						for (int j = 0; j < flyobject.Length; j++)
						{
							flyobject[j].Remove();
						}
						if (!(Staffplayer.IsCrouching) && !(Staffplayer.IsInMidAir) && !(Staffplayer.IsStaggering))
						{
							IObject createdObject = Game.CreateObject("InvisiblePlatform", worldPosition, 0f, flying, 0f);
							createdObject.CustomId = "Flying";
						}
					}
				}
			}
		}

		public void Delete(TriggerArgs args)
		{
			IObject[] flyingobj = Game.GetObjectsByCustomId("Flying");
			for (int i = 0; i < flyingobj.Length; i++)
			{
				flyingobj[i].Remove();
			}
		}


		#endregion
	}
}
