﻿using MPTanks.Engine.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPTanks.Clients.GameClient.Menus
{
    class ClientSettings : SettingsBase
    {
        public static ClientSettings Instance { get; private set; } = new ClientSettings("Client Settings.json");

        public Setting<string[]> ModSearchPaths { get; private set; }

        public Setting<string> ClientLogLocation { get; private set; }

        private ClientSettings(string file) : base(file)
        {
            ModSearchPaths = new Setting<string[]>(this, "Mod search paths", "The paths in which to search for packed *.mod files.",
                new[] {
                    Path.Combine(Directory.GetCurrentDirectory(), "mods"),
                    Path.Combine(ConfigDir, "mods")
                });

            ClientLogLocation = new Setting<string>(this, "Client log directory",
                "The directory where the client (menus and watchdog) logs are stored.",
                Path.Combine(ConfigDir, "clientlogs", "client.log"));
        }

    }
}