﻿using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPTanks.Engine.Gamemodes
{
    public class Team
    {
        private static Team _null = new Team() { TeamId = -1, TeamName = "Null" };
        public static Team Null { get { return _null; } }
        private static Team _tied = new Team() { TeamId = -2, TeamName = "Indeterminate" };
        /// <summary>
        /// The team to flag as a tie or when no one wins.
        /// </summary>
        public static Team Indeterminate { get { return _tied; } }
        private GamePlayer[] _players;
        [JsonIgnore]
        public GamePlayer[] Players
        {
            get { return _players; }
            set
            {
                _players = value;
                foreach (var p in _players)
                    if (p != null) p.Team = this;
            }
        }
        public string TeamName { get; set; }
        public Color TeamColor { get; set; }
        public short TeamId { get; set; }
        /// <summary>
        /// The team's goal, for an explanation to the players.
        /// </summary>
        public string Objective { get; set; }


        public int LivingPlayersCount
        {
            get
            {
                int alive = 0;
                foreach (var plr in Players)
                {
                    if (plr.Tank != null && plr.Tank.Alive)
                        alive++;
                }
                return alive;
            }
        }
        public Team()
        {
            _players = new GamePlayer[0];
            Objective = "ERR_TEAM_NOT_CONFIGURED";
            TeamName = "ERR_TEAM_NOT_CONFIGURED";
            TeamId = 1102;
        }
    }
}
