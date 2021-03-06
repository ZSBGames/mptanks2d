﻿using MPTanks.Engine.Settings;
using MPTanks.Modding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MPTanks.Engine
{
    public partial class GameObject
    {
        #region Reflection helper
        //We cache the info for performance. Multiple calls only create one instance
        private string _cachedReflectionName;
        public string ReflectionName => ContainingModule.Name + "+" + GetType().Name;
        private string _cachedDisplayName;
        public string DisplayName
        {
            get
            {
                if (_cachedDisplayName == null)
                    _cachedDisplayName = ((GameObjectAttribute[])(GetType()
                          .GetCustomAttributes(typeof(GameObjectAttribute), true)))[0]
                          .DisplayName;

                return _cachedDisplayName;
            }
        }
        private string _cachedDescription;
        public string Description
        {
            get
            {
                if (_cachedDescription == null)
                    _cachedDescription = ((GameObjectAttribute[])(GetType()
                          .GetCustomAttributes(typeof(GameObjectAttribute), true)))[0]
                          .Description;

                return _cachedDescription;
            }
        }

        private Module _cachedModule;
        /// <summary>
        /// The module that contains this object
        /// </summary>
        [JsonIgnore]
        public Module ContainingModule
        {
            get
            {
                if (_cachedModule == null)
                    _cachedModule = ModDatabase.TypeToModuleTable[GetType()];

                return _cachedModule;
            }
        }
        #endregion

        private static Dictionary<string, Type> _types = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
        public static IReadOnlyDictionary<string, Type> AvailableTypes => _types;
        private static void RegisterType<T>(Module module) where T : GameObject
        {
            //get the name
            var name = module.Name + "+" + typeof(T).Name;
            if (_types.ContainsKey(name)) throw new Exception("Already registered!");

            _types.Add(name.ToLower(), typeof(T));
        }

        public static T ReflectiveInitialize<T>
            (string reflectionName, GameCore game, bool authorized = false) where T : GameObject =>
            (T)ReflectiveInitialize(reflectionName, game, authorized);

        public static GameObject ReflectiveInitialize(string reflectionName, GameCore game, bool authorized = false)
        {
            long totalMem = 0;
            if (GlobalSettings.Debug)
                totalMem = GC.GetTotalMemory(true);

            var obj = Activator.CreateInstance(_types[reflectionName], game, authorized);
            if (GlobalSettings.Debug)
            {
                var memUsageBytes = (GC.GetTotalMemory(true) - totalMem) / 1024f;
                game.Logger.Trace($"Allocating Generic (Game)Object {reflectionName}, size is: {memUsageBytes.ToString("N2")} KiB");
            }
            return (GameObject)obj;
        }
    }
}
