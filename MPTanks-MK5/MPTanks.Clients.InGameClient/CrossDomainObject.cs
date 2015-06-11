﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPTanks.Clients.GameClient
{
    public class CrossDomainObject : MarshalByRefObject
    {
        private static CrossDomainObject _inst;
        public static CrossDomainObject Instance
        {
            get
            {
                if (_inst == null)
                {
                    _inst = new CrossDomainObject
                    {
                        
                    };
                }
                return _inst;
            }
            set
            {
                _inst = value;
            }
        }

        public CrossDomainObject()
        {
            Instance = this;
        }

        public int WindowPositionX { get; set; }
        public int WindowPositionY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public bool ShouldBeGameHost { get; set; }
        public bool GameHostIsLanGame { get; set; }

        public string SettingsJSON { get; set; }

        public bool Connected { get; set; }
        public bool Connecting { get; set; }
        public string ConnectionFailureCause { get; set; }

        public string[] ModsToInject { get; set; }

        public string AuthKey { get; set; }
        public string Username { get; set; }

        public string ServerIp { get; set; }
        public ushort ServerPort { get; set; }
        public string ServerPassword { get; set; }

        public bool ServerRequiresIntroduction { get; set; }
        public ulong IntroductionId { get; set; }
        public string IntroductionServerAddress { get; set; }
    }
}