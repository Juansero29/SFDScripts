using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SFDGameScriptInterface;

namespace SFDScripts
{

    class TeamRotation : GameScriptInterface
    {

        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public TeamRotation() : base(null) { }

        /*
        * TeamRotation.txt
        * By Gurt - Mytho-Logic Interactive.
        *
        * Disable even teams and shuffle teams options and set all teams to independent. This script will handle the rest.
        */

        private static string MESSAGE = "First 12 minutes each half hour is FREE FOR ALL. Remaining time is TEAM GAMEPLAY (if enough players).";

        private void CheckMessage()
        {
            int prevMinute = 0;
            if (!int.TryParse(Game.Data, out prevMinute))
            {
                prevMinute = -60; // Forces message to be shown if not yet shown
            }
            int currentMinute = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            if (Math.Abs(currentMinute - prevMinute) >= 10)
            {
                // Show it each 10 mintues or so...
                Game.RunCommand("/MSG " + "==================================");
                Game.RunCommand("/MSG " + MESSAGE);
                Game.RunCommand("/MSG " + "==================================");
                Game.Data = currentMinute.ToString();
            }
        }

        private int m_minute = 0;
        private string m_lastTeamSetup = "";
        private string m_lastAlonePlayer = "";
        private Random m_rnd = null;

        public void OnStartup()
        {
            m_rnd = new Random((int)DateTime.Now.Millisecond * (int)DateTime.Now.Minute * 1000);
            if (Game.IsFirstUpdate)
            {
                CheckMessage();

                m_minute = DateTime.Now.Minute;
                if (m_minute >= 30)
                    m_minute -= 30;

                BalanceTeams(false);
                // after 3 seconds - rebalance teams if anyone left.
                IObjectTimerTrigger timer = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
                timer.SetRepeatCount(2);
                timer.SetIntervalTime(1000);
                timer.SetScriptMethod("RecheckBalance");
                timer.Trigger();
            }
        }

        public void RecheckBalance(TriggerArgs args)
        {
            BalanceTeams(true);
        }

        // Based on players and current clock - even teams
        public void BalanceTeams(bool isRecheck)
        {
            // cycle team rotation each 30 minutes   
            List<IPlayer> players = GetActivePlayers();
            if (players.Count == 0)
                return;

            bool freeForAllTime = (m_minute < 12); // 12 minutes each 30 minutes do FREE FOR ALL else TEAMS
            bool bigTeamsTime = (m_minute >= 25); // 5 minutes each 30 minutes when TEAMS do BIG TEAMS!

            if ((players.Count == 5 || players.Count == 7) && !freeForAllTime)
            {
                // special. If 5 or 7 players and it's not FREE-FOR-ALL time remove the one with HIGHEST WIN-RATIO to play ALONE
                IPlayer highestScoringPlayer = GetPlayerUserWithHighestWinRatio(players);
                if (highestScoringPlayer != null)
                {
                    players.Remove(highestScoringPlayer);
                    highestScoringPlayer.SetTeam(PlayerTeam.Team4);
                    IUser user = highestScoringPlayer.GetUser();
                    if (user != null && m_lastAlonePlayer != user.Name)
                    {
                        m_lastAlonePlayer = user.Name;
                        Game.RunCommand("/MSG " + user.Name + " with highest win-ratio will be playing alone this round.");
                    }
                }
            }

            if (players.Count % 2 == 1 || players.Count < 4 || freeForAllTime)
            {
                // UNEVEN COUNT FOR TEAMS or FREE-FOR-ALL TIME
                DistributeTeams(players, PlayerTeam.Independent,
                                   PlayerTeam.Independent,
                                   PlayerTeam.Independent,
                                   PlayerTeam.Independent,
                                   PlayerTeam.Independent,
                                   PlayerTeam.Independent,
                                   PlayerTeam.Independent,
                                   PlayerTeam.Independent);
                AnnouncCurrentTeamSetup("FREE FOR ALL", isRecheck);
            }
            else
            {
                // possible setups:
                // 2,2
                // 2,2,2 / 3,3 (BIG TEAMS)
                // 2,2,2,2 / 4,4 (BIG TEAMS)
                if (bigTeamsTime && players.Count == 6)
                {
                    DistributeTeams(players, PlayerTeam.Team1,
                                       PlayerTeam.Team1,
                                       PlayerTeam.Team1,
                                       PlayerTeam.Team2,
                                       PlayerTeam.Team2,
                                       PlayerTeam.Team2);
                    AnnouncCurrentTeamSetup("TEAMS (3v3)", isRecheck);
                }
                else if (bigTeamsTime && players.Count == 8)
                {
                    DistributeTeams(players, PlayerTeam.Team1,
                                       PlayerTeam.Team1,
                                       PlayerTeam.Team1,
                                       PlayerTeam.Team1,
                                       PlayerTeam.Team2,
                                       PlayerTeam.Team2,
                                       PlayerTeam.Team2,
                                       PlayerTeam.Team2);
                    AnnouncCurrentTeamSetup("TEAMS (4v4)", isRecheck);
                }
                else
                {
                    // this distribute for 2v2, 2v2v2 and 2v2v2v2... 
                    if (players.Count == 4)
                    {
                        DistributeTeams(players, PlayerTeam.Team1,
                                           PlayerTeam.Team1,
                                           PlayerTeam.Team2,
                                           PlayerTeam.Team2);
                    }
                    else if (players.Count == 6)
                    {
                        DistributeTeams(players, PlayerTeam.Team1,
                                           PlayerTeam.Team1,
                                           PlayerTeam.Team2,
                                           PlayerTeam.Team2,
                                           PlayerTeam.Team3,
                                           PlayerTeam.Team3);
                    }
                    else
                    {
                        DistributeTeams(players, PlayerTeam.Team1,
                                           PlayerTeam.Team1,
                                           PlayerTeam.Team2,
                                           PlayerTeam.Team2,
                                           PlayerTeam.Team3,
                                           PlayerTeam.Team3,
                                           PlayerTeam.Team4,
                                           PlayerTeam.Team4);
                    }
                    AnnouncCurrentTeamSetup("TEAMS (2v2)", isRecheck);
                }
            }
        }


        private IPlayer GetPlayerUserWithHighestWinRatio(List<IPlayer> players)
        {
            float highestWinRatio = -1f;
            List<IPlayer> highestScoringPlayers = new List<IPlayer>();
            foreach (IPlayer player in players)
            {
                IUser user = player.GetUser();
                if (user != null)
                {
                    float userWinRatio = (user.TotalGames >= 2 ? (float)user.TotalWins / (float)user.TotalGames : 0f);
                    if (userWinRatio == highestWinRatio)
                    {
                        highestScoringPlayers.Add(player);
                    }
                    else if (userWinRatio > highestWinRatio)
                    {
                        highestWinRatio = userWinRatio;
                        highestScoringPlayers.Clear();
                        highestScoringPlayers.Add(player);
                    }
                }
            }
            if (highestScoringPlayers.Count == 0)
                return null;
            if (highestScoringPlayers.Count == 1)
                return highestScoringPlayers[0];
            // Return random highest scoring player

            return highestScoringPlayers[m_rnd.Next(0, highestScoringPlayers.Count)];
        }

        private void DistributeTeams(List<IPlayer> players, params PlayerTeam[] teams)
        {
            if (players == null || teams == null) return;

            // If teams already distributed correctly - ignore randomizing a new distribution!
            if (CheckTeamsDistributed(players, teams)) return;

            // randomize team distribution
            List<IPlayer> playersToShuffle = new List<IPlayer>(players);
            int teamIndex = 0;
            while (playersToShuffle.Count > 0 && teamIndex < teams.Length)
            {
                int playerIndex = m_rnd.Next(0, playersToShuffle.Count);
                IPlayer player = playersToShuffle[playerIndex];
                playersToShuffle.RemoveAt(playerIndex);
                player.SetTeam(teams[teamIndex]);
                teamIndex++;
            }
        }

        // Checks if the players are distributed according to the teams layout.
        private bool CheckTeamsDistributed(List<IPlayer> players, params PlayerTeam[] teams)
        {
            if (players == null || teams == null) return false;

            List<PlayerTeam> checkTeams = new List<PlayerTeam>(teams);
            foreach (IPlayer player in players)
            {
                PlayerTeam playerTeam = player.GetTeam();
                if (!checkTeams.Remove(playerTeam))
                {
                    return false;
                }
            }
            // Check if checkTeams contains team 1-4 - if so distribution is not fullfilled
            return !checkTeams.Any(x => x != PlayerTeam.Independent);
        }

        // ActiveUsers
        // ActiveUsers controlling a player
        private List<IPlayer> GetActivePlayers()
        {
            List<IPlayer> activePlayers = new List<IPlayer>();
            IUser[] users = Game.GetActiveUsers();
            foreach (IUser user in users)
            {
                IPlayer plr = user.GetPlayer();
                if (plr != null)
                {
                    activePlayers.Add(plr);
                }
            }
            return activePlayers;
        }

        private void AnnouncCurrentTeamSetup(string msg, bool isRecheck)
        {
            if (m_lastTeamSetup != msg)
            {
                m_lastTeamSetup = msg;
                Game.RunCommand("/MSG " + msg + (isRecheck ? " (Someone left the game early)" : ""));
            }
        }
    }
}