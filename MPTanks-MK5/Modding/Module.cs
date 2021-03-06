﻿using MPTanks.Modding.Unpacker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MPTanks.Modding
{
    public class Module
    {
        public string Name { get; internal set; }
        public string Description { get; internal set; }
        public string Author { get; internal set; }

        public ModInfo ModInfo
        {
            get
            {
                return new ModInfo()
                {
                    ModName = Name,
                    ModMajor = Version.Major,
                    ModMinor = Version.Minor
                };
            }
        }

        public ModHeader Header { get; internal set; }
        public bool UsesWhitelist { get; internal set; }
        public Dictionary<string, string> AssetMappings { get; internal set; }
        = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        public string PackedFile { get; internal set; }
        public ModuleDeclarationAttribute.ModuleVersion Version { get; internal set; }
        public Assembly[] Assemblies { get; internal set; }
        public Assembly[] Dependencies { get; internal set; }
        /// <summary>
        /// A list of the types of tanks
        /// </summary>
        public TankType[] Tanks { get; internal set; }
        /// <summary>
        /// A list of the types of projectiles
        /// </summary>
        public ProjectileType[] Projectiles { get; internal set; }
        /// <summary>
        /// A list of the types of gamemodes
        /// </summary>
        public GamemodeType[] Gamemodes { get; internal set; }
        /// <summary>
        /// A list of the types of mapobjects
        /// </summary>
        public MapObjectType[] MapObjects { get; internal set; }
        /// <summary>
        /// A list of the types of mapobjects
        /// </summary>
        public GameObjectType[] GameObjects { get; internal set; }

        /// <summary>
        /// Gets the data from a file that is packed in the *.mod container (paths not supported).
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public byte[] GetPackedFileData(string filename)
        {
            return File.ReadAllBytes(AssetMappings[filename]);
        }
        /// <summary>
        /// Gets the string data from a file that is packed in the *.mod container (paths not supported).
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public string GetPackedFileString(string filename)
        {
            return File.ReadAllText(AssetMappings[filename]);
        }

        public ModAssetInfo GetAsset(string name) =>
            new ModAssetInfo
            {
                AssetName = name,
                ModInfo = new ModInfo { ModMajor = Version.Major, ModMinor = Version.Minor, ModName = Name }
            };
    }

    public class GamemodeType
    {
        public Type Type { get; internal set; }
        public string ReflectionTypeName { get; internal set; }
        public string DisplayName { get; internal set; }
        public string DisplayDescription { get; internal set; }
        /// <summary>
        /// The minimum number of players required to start a game.
        /// </summary>
        public int MinPlayersCount { get; set; }
        public bool HotJoinAllowed { get; set; }

        public GamemodeType(Type t, Module module)
        {
            var attrib = t.GetCustomAttribute<GamemodeAttribute>(true);
            if (attrib == null)
                throw new Exception(t.FullName + " is missing [GamemodeAttribute]");

            Type = t;
            ReflectionTypeName = module.Name + "+" + t.Name;
            DisplayName = attrib.DisplayName;
            DisplayDescription = attrib.Description;
            MinPlayersCount = attrib.MinPlayersCount;
            HotJoinAllowed = attrib.HotJoinEnabled;
        }
        internal static bool IsGamemodeType(Type t)
        {
            var gamemode = GetTypeHelper.GetType(ModSettings.GamemodeTypeName);

            return t.IsSubclassOf(gamemode);
        }
    }
    public class TankType
    {
        public Type Type { get; internal set; }
        public string ReflectionTypeName { get; internal set; }
        public string DisplayName { get; internal set; }
        public string DisplayDescription { get; internal set; }

        public TankType(Type t, Module module)
        {
            var attrib = t.GetCustomAttribute<TankAttribute>(true);
            if (attrib == null)
                throw new Exception(t.FullName + " is missing [TankAttribute]");

            Type = t;
            ReflectionTypeName = module.Name + "+" + t.Name;
            DisplayName = attrib.DisplayName;
            DisplayDescription = attrib.Description;
        }
        internal static bool IsTankType(Type t)
        {
            var tank = GetTypeHelper.GetType(ModSettings.TankTypeName);

            return t.IsSubclassOf(tank);
        }
    }

    public class ProjectileType
    {
        public Type Type { get; internal set; }
        public string DisplayName { get; internal set; }
        public string ReflectionTypeName { get; internal set; }

        public ProjectileType(Type t, Module module)
        {
            var attrib = t.GetCustomAttribute<ProjectileAttribute>(true);
            if (attrib == null)
                throw new Exception(t.FullName + " is missing [ProjectileAttribute]");

            Type = t;
            ReflectionTypeName = module.Name + "+" + t.Name;
            DisplayName = attrib.DisplayName;
        }

        internal static bool IsProjectileType(Type t)
        {
            var prj = GetTypeHelper.GetType(ModSettings.ProjectileTypeName);

            return t.IsSubclassOf(prj);
        }
    }
    public class MapObjectType
    {
        public Type Type { get; internal set; }
        public string DisplayName { get; internal set; }
        public string ReflectionTypeName { get; internal set; }
        public bool StaticObject { get; internal set; }
        public float MinWidth { get; internal set; }
        public float MinHeight { get; internal set; }

        public MapObjectType(Type t, Module module)
        {
            var attrib = t.GetCustomAttribute<MapObjectAttribute>(true);
            if (attrib == null)
                throw new Exception(t.FullName + " is missing [MapObjectAttribute]");

            Type = t;
            ReflectionTypeName = module.Name + "+" + t.Name;
            DisplayName = attrib.DisplayName;

        }

        internal static bool IsMapObjectType(Type t)
        {
            var prj = GetTypeHelper.GetType(ModSettings.MapObjectTypeName);

            return t.IsSubclassOf(prj);
        }
    }

    public class GameObjectType
    {
        public Type Type { get; set; }
        public string ReflectionTypeName { get; set; }
        public string DisplayName { get; set; }
        public GameObjectType(Type t, Module module)
        {
            var attrib = t.GetCustomAttribute<GameObjectAttribute>(true);
            if (attrib == null)
                throw new Exception(t.FullName + " is missing [GameObjectAttribute]");

            Type = t;
            ReflectionTypeName = module.Name + "+" + t.Name;
            DisplayName = attrib.DisplayName;

        }

        internal static bool IsGameObjectType(Type t)
        {
            var prj = GetTypeHelper.GetType(ModSettings.GameObjectTypeName);

            return t.IsSubclassOf(prj);
        }
    }

    internal static class GetTypeHelper
    {
        private static Dictionary<string, Type> _types =
            new Dictionary<string, Type>();
        public static Type GetType(string name)
        {
            if (_types.ContainsKey(name))
                return _types[name];

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                foreach (var type in asm.GetTypes())
                    if (type.FullName == name)
                    {
                        _types.Add(name, type);
                        return type;
                    }

            return null;
        }
    }
}
