﻿using Microsoft.Xna.Framework;
using MPTanks.Engine.Tanks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPTanks.Engine
{
    public partial class GameCore
    {
        public const int MaxPlayersAllowed = 1024;
        public const ushort ReservedEmptyPlayerId = 1023;
        public const int PlayerIdNumberOfBits = 10;
        private List<ushort> _playerIds = new List<ushort>();
        private Dictionary<ushort, GamePlayer> _playersById = new Dictionary<ushort, GamePlayer>();
        public IReadOnlyDictionary<ushort, GamePlayer> PlayersById { get { return _playersById; } }

        public IEnumerable<GamePlayer> Players { get { return _playersById.Values; } }

        public ushort AvailablePlayerId
        {
            get
            {
                ushort id = 0;
                while (_playerIds.Contains(id)) id++;

                if (id > MaxPlayersAllowed - 1) return ReservedEmptyPlayerId;

                return id;
            }
        }

        public GamePlayer AddPlayer(ushort playerId)
        {
            return AddPlayer(new GamePlayer()
            {
                Id = playerId,
                Game = this
            });
        }

        private List<GamePlayer> _hotJoinPlayersWaitingForTankSelection =
            new List<GamePlayer>();
        public GamePlayer AddPlayer<GamePlayer>(GamePlayer player) where GamePlayer : Engine.GamePlayer
        {
            if (!_playerIds.Contains(player.Id))
            {
                _playerIds.Add(player.Id);
                player.IsSpectator = false; //Default them to being in game except in exigent circumstances (e.g. they chose to spectate)
                player.Game = this;
                player.AllowedTankTypes = Gamemode.GetPlayerAllowedTankTypes(player);
                _playersById.Add(player.Id, player);
            }

            if (Authoritative)
            {
                if (Running && !Gamemode.HotJoinEnabled)
                    player.IsSpectator = true; //Force spectator
                else if (Running && Gamemode.HotJoinCanPlayerJoin(player))
                {
                    player.IsSpectator = false;
                    //Let them join - first find the team and size the player list correctly
                    player.Team = Gamemode.HotJoinGetPlayerTeam(player);
                    var newPlayerArray = new Engine.GamePlayer[player.Team.Players.Length + 1];
                    Array.Copy(player.Team.Players, newPlayerArray, player.Team.Players.Length);
                    newPlayerArray[newPlayerArray.Length - 1] = player;
                    player.Team.Players = newPlayerArray;

                    //Then tanks
                    player.AllowedTankTypes = Gamemode.HotJoinGetAllowedTankTypes(player);

                    //And add a timeout for them
                    TimerFactory.CreateTimer(a =>
                    {
                        if (player.HasTank || player.HasSelectedTankYet) return;
                        _hotJoinPlayersWaitingForTankSelection.Remove(player);
                        SetupPlayer(player);
                    }, Settings.HotJoinTankSelectionTime);

                    _hotJoinPlayersWaitingForTankSelection.Add(player);
                }
            }
            return player;
        }

        private List<GamePlayer> _hotJoinRemovalTempList = new List<GamePlayer>();
        private void UpdateHotJoinPlayers()
        {
            if (!Gamemode.HotJoinEnabled) return;

            foreach (var plr in _hotJoinPlayersWaitingForTankSelection)
                if (plr.HasSelectedTankYet && plr.TankSelectionIsValid)
                {
                    SetupPlayer(plr);
                    _hotJoinRemovalTempList.Add(plr);
                }

            foreach (var gp in _hotJoinRemovalTempList)
                _hotJoinPlayersWaitingForTankSelection.Remove(gp);
            _hotJoinRemovalTempList.Clear();
        }

        /// <summary>
        /// Kill and remove the player
        /// </summary>
        /// <param name="playerId"></param>
        public void RemovePlayer(ushort playerId)
        {
            if (_playersById.ContainsKey(playerId))
                RemovePlayer(_playersById[playerId]);
        }

        /// <summary>
        /// Kill and remove the player
        /// </summary>
        /// <param name="playerId"></param>
        public void RemovePlayer(GamePlayer player)
        {
            if (_playersById.ContainsKey(player.Id))
            {
                //kill their tank (fast)
                if (player.HasTank)
                    ImmediatelyForceObjectDestruction(player.Tank);
                //remove them from the team
                if (player.Team != null && player.Team.Players.Contains(player))
                    player.Team.Players = player.Team.Players.Where(a => a != player).ToArray();
                //And remove the team if no one is there
                if (player.Team?.Players.Length == 0)
                    Gamemode.Teams = Gamemode.Teams.Where(t => t != player.Team).ToArray();
                //and remove them from the players list
                _playersById.Remove(player.Id);
                _playerIds.Remove(player.Id);
            }
        }

        public GamePlayer FindPlayer<GamePlayer>(ushort playerId) where GamePlayer : Engine.GamePlayer
        {
            return PlayersById.ContainsKey(playerId) ? PlayersById[playerId] as GamePlayer : null;
        }

        public GamePlayer FindPlayer(ushort playerId) => FindPlayer<GamePlayer>(playerId);

        public void InjectPlayerInput(ushort playerId, InputState state)
        {
            if (Running && PlayersById.ContainsKey(playerId) && PlayersById[playerId].HasTank)
                PlayersById[playerId].Tank.InputState = state;
        }

        public void InjectPlayerInput(GamePlayer player, InputState state)
        {
            if (Running && player.HasTank)
                player.Tank.InputState = state;
        }

        /// <summary>
        /// Sets up the players for the game. Server only.
        /// </summary>
        private void SetupGamePlayers()
        {
            Gamemode.MakeTeams(Players.ToArray());

            foreach (var player in Players)
            {
                if (player.IsSpectator) continue; //Ignore spectators
                else SetupPlayer(player);
            }
        }

        private void SetupPlayer(GamePlayer player)
        {
            if (!player.TankSelectionIsValid) //Do the selection for them if their selection is invalid
                player.SelectedTankReflectionName = Gamemode.DefaultTankTypeReflectionName;

            var tank = Tank.ReflectiveInitialize(player.SelectedTankReflectionName, player, this, false);
            player.SpawnPoint = Map.GetSpawnPosition(Gamemode.GetTeamIndex(player));

            tank.Position = player.SpawnPoint;
            tank.ColorMask = player.Team.TeamColor;

            AddGameObject(tank);
            player.Tank = tank;
        }
    }
}
