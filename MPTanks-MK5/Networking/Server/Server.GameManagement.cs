﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPTanks.Networking.Server
{
    public partial class Server
    {
        public bool GameStartTimeoutHasEnded => GameStartRemainingTimeout <= TimeSpan.Zero;
        public TimeSpan GameStartRemainingTimeout { get; private set; }
            = TimeSpan.FromMilliseconds(ServerSettings.Instance.TimeToWaitForPlayersReady);

        public enum ServerGameStatus
        {
            WaitingForPlayers,
            InCountdown,
            InGame,
            GameEnded
        }

        public ServerGameStatus GameStatus
        {
            get
            {
                if (!Game.HasEnoughPlayersToStart)
                    return ServerGameStatus.WaitingForPlayers;
                if (!GameStartTimeoutHasEnded)
                    return ServerGameStatus.InCountdown;
                if (Game.Ended)
                    return ServerGameStatus.GameEnded;

                return ServerGameStatus.InGame;
            }
        }

        public bool CanStartGame
        {
            get
            {
                foreach (var plr in Players)
                    if (!GameStartTimeoutHasEnded)
                        if (!plr.IsReady) return false;

                if (!Game.HasEnoughPlayersToStart) return false;
                if (Game.HasStarted) return false; //can't start

                return true;
            }
        }
        public void StartGame()
        {
            if (!CanStartGame) return;

            Game.BeginGame();
        }

        public void TickGameStartCountdown(GameTime gameTime)
        {
            if (!Game.HasEnoughPlayersToStart)
            {
                var ttw = TimeSpan.FromMilliseconds(ServerSettings.Instance.TimeToWaitForPlayersReady);
                if (GameStartRemainingTimeout != ttw)
                {
                    //Send a countdown notification
                    MessageProcessor.SendMessage(new Common.Actions.ToClient.CountdownStartedAction(ttw));
                    GameStartRemainingTimeout = ttw;
                }
                return;
            }
            GameStartRemainingTimeout -= gameTime.ElapsedGameTime;

            if (GameStartTimeoutHasEnded) StartGame();
        }
    }
}
