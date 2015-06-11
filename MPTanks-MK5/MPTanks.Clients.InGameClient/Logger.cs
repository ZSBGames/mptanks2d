﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPTanks.Clients.GameClient
{
    public static class Logger
    {
        private static NLog.Logger logger;

        public static NLog.Logger Instance { get { return logger; } }

        static Logger()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var st = GameSettings.Instance.GameLogLocation;
            var fileTarget =
                new NLog.Targets.Wrappers.AsyncTargetWrapper(
                    new NLog.Targets.FileTarget()
                    {
                        FileName = (string)GameSettings.Instance.GameLogLocation,
                        ArchiveOldFileOnStartup = true,
                        KeepFileOpen = true,
                        MaxArchiveFiles = 10,
                        EnableFileDelete = true,
                        CreateDirs = true
                    },
                    10000, NLog.Targets.Wrappers.AsyncTargetWrapperOverflowAction.Grow);

            config.AddTarget("logfile", fileTarget);
#if DEBUG
            config.LoggingRules.Add(new NLog.Config.LoggingRule("*", NLog.LogLevel.Debug, fileTarget));
#else
            config.LoggingRules.Add(new NLog.Config.LoggingRule("*", NLog.LogLevel.Info, fileTarget));
#endif

            NLog.LogManager.Configuration = config;

            logger = NLog.LogManager.GetLogger("Client");
        }
        public static void Debug(string message)
        {
            Instance.Debug(message);
        }

        public static void Error(Exception ex)
        {
            Instance.ErrorException("Severe Error/Exception", ex);
        }

        public static void Error(string message)
        {
            Error(message);
        }

        public static void Fatal(Exception ex)
        {
            Instance.FatalException("Fatal Exception", ex);
            AppDomain.Unload(AppDomain.CurrentDomain);
            throw ex;
        }

        public static void Fatal(string message)
        {
            Instance.Fatal(message);
        }

        public static void Info(object data)
        {
            Info("[" + data.GetType().AssemblyQualifiedName + "]\n" +
                JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        public static void Info(string message)
        {
            Instance.Info(message);
        }

        public static void Trace(Exception ex)
        {
            Instance.TraceException("Code Trace", ex);
        }

        public static void Trace(object data)
        {
            Trace("[" + data.GetType().AssemblyQualifiedName + "]\n" +
                JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        public static void Trace(string message)
        {
            Instance.Trace(message);
        }

        public static void Warning(string message)
        {
            Instance.Warn(message);
        }

        public static void Warning(object data)
        {
            Warning("[" + data.GetType().AssemblyQualifiedName + "]\n" +
                JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        private static string GetStackTrace()
        {
            return Environment.StackTrace;
        }
    }
}