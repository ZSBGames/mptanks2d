﻿using Microsoft.Xna.Framework;
using MPTanks.Engine.Gamemodes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPTanks.Engine
{
    public class GamePlayer
    {
        /// <summary>
        /// Whether the player wants to be a spectator
        /// </summary>
        public virtual bool PlayerWantsToBeSpectator { get; set; }
        /// <summary>
        /// Whether the game wishes for them to be a spectator
        /// </summary>
        public bool IsSpectatorFlagSet { get; set; }
        /// <summary>
        /// Whether the player is in the current game
        /// </summary>
        public virtual bool IsSpectator { get { return IsSpectatorFlagSet || PlayerWantsToBeSpectator; } set { IsSpectatorFlagSet = value; } }
        private string _selectedTankReflectionNameBacking;
        public virtual string SelectedTankReflectionName
        {
            get { return _selectedTankReflectionNameBacking; }
            set { _selectedTankReflectionNameBacking = value?.ToLower(); }
        }
        public virtual bool HasSelectedTankYet => SelectedTankReflectionName != null;
        public virtual ushort Id { get; set; }
        public virtual string[] AllowedTankTypes { get; set; }
        [JsonIgnore]
        public virtual Tanks.Tank Tank { get; set; }
        [JsonIgnore]
        public virtual Team Team { get; set; }
        [JsonIgnore]
        public virtual GameCore Game { get; set; }
        public virtual Vector2 SpawnPoint { get; set; }

        public bool HasTank => Tank != null && Tank.Alive;

        public event EventHandler<RespawnEventArgs> OnRespawn = delegate { };

        public struct RespawnEventArgs
        {
            public GamePlayer Player { get; set; }
            public Tanks.Tank NewTank { get; set; }
        }

        private string _name;
        public virtual string Username
        {
            get
            {
                if (_name == null)
                    return "Player ID: " + Id.ToString();

                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public GamePlayer()
        {
            AllowedTankTypes = new string[0];
        }

        public virtual bool TankSelectionIsValid
        {
            get
            {
                if (SelectedTankReflectionName == null) return false;
                if (!HasSelectedTankYet) return false;
                if (AllowedTankTypes == null || AllowedTankTypes.Length == 0)
                    if (Tanks.Tank.GetAllTankTypes().Contains(SelectedTankReflectionName) &&
                        Game.Gamemode.CheckPlayerTankSelectionValid(this, SelectedTankReflectionName))
                        return true;
                    else return false;

                return AllowedTankTypes.Contains(SelectedTankReflectionName) &&
                    Tanks.Tank.GetAllTankTypes().Contains(SelectedTankReflectionName) &&
                    Game.Gamemode.CheckPlayerTankSelectionValid(this, SelectedTankReflectionName);
            }
        }
        /// <summary>
        /// Respawns a player in the game
        /// </summary>
        public virtual Tanks.Tank Respawn(bool authorized = false)
        {
            if (Game.GameObjects.Contains(Tank))
                Game.RemoveGameObject(Tank);

            Tank = Tanks.Tank.ReflectiveInitialize(SelectedTankReflectionName, this, Game, authorized);
            Tank.Position = SpawnPoint;
            Game.AddGameObject(Tank);
            OnRespawn(this, new RespawnEventArgs { Player = this, NewTank = Tank });
            return Tank;
        }
    }
}
