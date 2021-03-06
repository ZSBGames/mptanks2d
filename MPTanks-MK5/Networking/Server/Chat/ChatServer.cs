﻿using MPTanks.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPTanks.Networking.Server.Chat
{
    public partial class ChatServer
    {
        public Server Server { get; private set; }
        public ChatServer(Server server)
        {
            Server = server;
        }
        /// <summary>
        /// The characters that a command must start with to be treated as a command
        /// </summary>
        public string CommandMarker { get; set; } = "/";

        public void SendMessage(string message, params ServerPlayer[] targets)
        {
            foreach (var target in targets)
                SendMessage(message, target);

            string playerList = string.Join(", ", targets.Select(a => a.Player.Username));
            Server.Logger.Info($"[CHAT] Server to {playerList}: {message}");

        }

        public void SendMessage(string message)
        {
            foreach (var player in Server.Players)
                SendMessage(message, player);
            Server.Logger.Info($"[CHAT] Server to all: {message}");
        }

        private void SendMessage(string message, ServerPlayer target)
        {
            Server.MessageProcessor.SendPrivateMessage(target,
                new Common.Actions.ToClient.ReceivedChatMessageAction(message, GameCore.ReservedEmptyPlayerId));
        }

        public void ForwardMessage(string message, ServerPlayer sender, params ServerPlayer[] targets)
        {
            bool isWideband = targets == null || targets.Length == 0;

            if (isWideband)
            {
                Server.Logger.Info($"[CHAT] {sender.Player.Username} to all: {message}");
                foreach (var target in Server.Players)
                    Server.MessageProcessor.SendPrivateMessage(target,
                        new Common.Actions.ToClient.ReceivedChatMessageAction(message, sender.Player.Id));

            }
            else
            {
                string playerList = string.Join(", ", targets.Select(a => a.Player.Username));
                Server.Logger.Info($"[CHAT] {sender.Player.Username} to {playerList}: {message}");
                foreach (var target in targets)
                    Server.MessageProcessor.SendPrivateMessage(target,
                        new Common.Actions.ToClient.ReceivedChatMessageAction(message, sender.Player.Id));
            }
        }
    }
}
