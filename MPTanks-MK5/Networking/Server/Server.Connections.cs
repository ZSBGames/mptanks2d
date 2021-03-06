﻿using Lidgren.Network;
using MPTanks.Engine;
using MPTanks.Networking.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPTanks.Networking.Server
{
    public class ConnectionManager
    {
        public Server Server { get; private set; }
        private List<NetConnection> _activeConnections = new List<NetConnection>();
        public List<NetConnection> ActiveConnections => _activeConnections;
        private Dictionary<NetConnection, ServerPlayer> _playersReverseTable = new Dictionary<NetConnection, ServerPlayer>();
        public IReadOnlyDictionary<NetConnection, ServerPlayer> PlayerTable => _playersReverseTable;
        public ConnectionManager(Server server)
        {
            Server = server;
        }
        public void WriteServerInfo(NetOutgoingMessage message)
        {
            message.Write(Server.Game.Gamemode.HotJoinEnabled);
            message.Write(Server.Configuration.Password != null);

            message.SkipPadBits();

            message.Write(Server.Name);

            message.Write(Server.Game.Gamemode.DisplayName);
            message.Write(Server.Game.Gamemode.Description);
            message.Write(Server.Players.Count);
            message.Write(Server.Configuration.MaxPlayers);

            message.Write(Server.Game.Map.Name);

            message.Write(Modding.ModDatabase.LoadedModulesList.Count);
            foreach (var mod in Modding.ModDatabase.LoadedModulesList)
            {
                message.Write(mod.Name);
                message.Write(mod.Version.Major);
                message.Write(mod.Version.Minor);
            }
        }
        public void UpdateConnectionStatus(NetConnection connection)
        {
            //Handle leaving gracefully
            if (connection.Status == NetConnectionStatus.Disconnecting ||
                connection.Status == NetConnectionStatus.Disconnected &&
                _activeConnections.Contains(connection))
            {
                _activeConnections.Remove(connection);
                Server.RemovePlayer(_playersReverseTable[connection]);
                _playersReverseTable.Remove(connection);
            }
        }

        internal void Accept(NetConnection connection, WebInterface.WebPlayerInfoResponse info)
        {
            if (_activeConnections.Contains(connection)) return; //Stupid shield
            var player = new ServerPlayer(Server, new NetworkPlayer
            {
                IsPremium = info.Premium,
                UniqueId = info.Id,
                Username = info.Username
            })
            { Connection = connection };

            Server.AddPlayer(player);

            _activeConnections.Add(connection);
            _playersReverseTable.Add(connection, player);
        }
    }
}
