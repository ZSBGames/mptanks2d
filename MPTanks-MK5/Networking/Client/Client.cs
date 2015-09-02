﻿using Microsoft.Xna.Framework;
using MPTanks.Engine;
using MPTanks.Engine.Logging;
using MPTanks.Engine.Settings;
using MPTanks.Engine.Tanks;
using MPTanks.Networking.Common;
using MPTanks.Networking.Common.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPTanks.Networking.Client
{
    public partial class Client
    {
        public enum ClientStatus
        {
            NotStarted,
            Connecting,
            ConnectionFailed,
            Errored,
            Connected,
            LoggingIn,
            DownloadingMods,
        }
        public ClientStatus Status { get; private set; } = ClientStatus.NotStarted;

        public Lidgren.Network.NetClient NetworkClient { get; private set; }
        public bool Connected =>
            Status == ClientStatus.Connected ||
            Status == ClientStatus.LoggingIn ||
            Status == ClientStatus.DownloadingMods;
        public NetworkedGame GameInstance { get; private set; }
        public ushort? PlayerId => Player?.Id;
        private InputState _input;
        public InputState Input
        {
            get { return _input; }
            set
            {
                if (_input != value)
                    MessageProcessor.SendMessage(new Common.Actions.ToServer.InputChangedAction(
                        Player?.Tank?.Position ?? Vector2.Zero, value));
                _input = value;
            }
        }
        public ClientNetworkProcessor MessageProcessor { get; private set; }
        public Chat.ChatClient Chat { get; private set; }
        public GameCore Game => GameInstance.Game;
        public GamePlayer Player
        {
            get
            {
                return
                    GameInstance.Game.Players.FirstOrDefault(
                        a => (a as NetworkPlayer)?.UniqueId == _userPlayerInfo.Id);
            }
        }
        public bool NeedsToSelectTank { get; internal set; }
        public bool IsInCountdown { get; internal set; }
        public TimeSpan RemainingCountdownTime { get; internal set; }
        public struct UserSuppliedPlayerInfo
        {
            public string Name { get; set; }
            public string Clan { get; set; }
            public Guid Id { get; set; }
        }

        public string Host { get; private set; }
        public ushort Port { get; private set; }
        public string Password { get; private set; }

        public ILogger Logger { get; set; }

        public bool GameRunning { get { return Connected && GameInstance != null; } }
        private UserSuppliedPlayerInfo _userPlayerInfo;
        public Client(string connection, ushort port, UserSuppliedPlayerInfo pInfo, ILogger logger = null,
            string password = null, bool connectOnInit = true)
        {
            _userPlayerInfo = pInfo;

            Logger = logger ?? new NullLogger();

            MessageProcessor = new ClientNetworkProcessor(this);
            GameInstance = new NetworkedGame(null);
            Chat = new Chat.ChatClient(this);

            Host = connection;
            Port = port;
            Password = password;

            NetworkClient = new Lidgren.Network.NetClient(
                new Lidgren.Network.NetPeerConfiguration("MPTANKS")
                {
                    ConnectionTimeout = GlobalSettings.Debug ? (float)Math.Pow(2, 16) : 15,
                    AutoFlushSendQueue = false
                });
            SetupNetwork();

            if (connectOnInit)
                Connect();
        }

        private void TickCountdown(GameTime gameTime)
        {
            if (!IsInCountdown) return;

            RemainingCountdownTime -= gameTime.ElapsedGameTime;
            if (RemainingCountdownTime < TimeSpan.Zero)
                IsInCountdown = false;
        }
        private bool _hasConnected;
        public void Connect()
        {
            if (_hasConnected == true) return;
            _hasConnected = true;
            Status = ClientStatus.Connecting;
            if (!string.IsNullOrWhiteSpace(Host) && Port != 0)
            {
                var msg = NetworkClient.CreateMessage();
                msg.Write(_userPlayerInfo.Id.ToByteArray());
                msg.Write(_userPlayerInfo.Clan);
                msg.Write(_userPlayerInfo.Name);
                if (Password != null) msg.Write(Password);
                NetworkClient.Start();
                NetworkClient.Connect(Host, Port, msg);
            }
        }

        /// <summary>
        /// Waits until it connects to the server and downloads the game state or returns false if the connection
        /// timed out;
        /// </summary>
        /// <returns></returns>
        public bool WaitForConnection()
        {
            while (NetworkClient.ConnectionStatus == Lidgren.Network.NetConnectionStatus.InitiatedConnect ||
                NetworkClient.ConnectionStatus == Lidgren.Network.NetConnectionStatus.ReceivedInitiation ||
                NetworkClient.ConnectionStatus == Lidgren.Network.NetConnectionStatus.RespondedAwaitingApproval ||
                NetworkClient.ConnectionStatus == Lidgren.Network.NetConnectionStatus.RespondedConnect)
                ProcessMessages();

            return NetworkClient.ConnectionStatus == Lidgren.Network.NetConnectionStatus.Connected;
        }

        public void Update(GameTime gameTime)
        {
            if (RemainingCountdownTime > TimeSpan.Zero)
                RemainingCountdownTime -= gameTime.ElapsedGameTime;
            ProcessMessages();
            Game.Update(gameTime);
            if (MessageProcessor.MessageQueue.Count > 0 &&
                NetworkClient.ConnectionStatus == Lidgren.Network.NetConnectionStatus.Connected)
            {
                var msg = NetworkClient.CreateMessage();
                MessageProcessor.WriteMessages(msg);
                NetworkClient.SendMessage(msg, Lidgren.Network.NetDeliveryMethod.ReliableOrdered);
            }
            MessageProcessor.ClearQueue();
            NetworkClient.FlushSendQueue();
        }

        private bool _hasDisconnected = false;
        public void Disconnect()
        {
            if (!_hasConnected || _hasDisconnected) return;
            _hasDisconnected = true;
            NetworkClient.Disconnect("Leaving");
        }

    }
}
