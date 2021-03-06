﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPTanks.Modding
{

    /// <summary>
    /// An attribute that marks an object as a tank
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class TankAttribute : GameObjectAttribute
    {
        public TankAttribute(string componentsFile)
            : base(componentsFile)
        {

        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class MapObjectAttribute : GameObjectAttribute
    {
        public MapObjectAttribute(string componentsFile)
            : base(componentsFile)
        {

        }
        /// <summary>
        /// Whether the body for the object is static or dynamic
        /// </summary>
        public bool IsStatic { get; set; }
        public float MinWidthBlocks { get; set; }
        public float MinHeightBlocks { get; set; }
        public string[] InstanceSettingNames { get; set; }
        public string[] InstanceSettingDefaults { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class ProjectileAttribute : GameObjectAttribute
    {
        public ProjectileAttribute(string componentsFile)
            : base(componentsFile)
        {
        }
    }

    public sealed class GamemodeAttribute : GameObjectAttribute
    {
        public GamemodeAttribute()
            : base("Gamemodes do not have renderer components")
        {
        }
        /// <summary>
        /// The minimum number of players required to start a game.
        /// </summary>
        public int MinPlayersCount { get; set; }

        /// <summary>
        /// The default tank type to use when a player does not select their tank type by the time the game starts.
        /// In the form of either "ModName+ClassName" (e.g. "CoreAssets+BasicTank") or "ClassName" (e.g. BasicTank), 
        /// but only if it's from the same mod as this asset
        /// 
        /// Defaults to the first tank defined in the first loaded mod file.
        /// </summary>
        public string DefaultTankTypeReflectionName { get; set; } = 
            ModLoader.LoadedMods.FirstOrDefault().Value?.Tanks?.FirstOrDefault()?.ReflectionTypeName ?? "CoreAssets+BasicTank";

        /// <summary>
        /// Whether a player can join an active game and get spawned in or if they will have to wait for the next one.
        /// </summary>
        public bool HotJoinEnabled { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ModuleDeclarationAttribute : Attribute
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Author { get; private set; }
        public ModuleVersion Version { get; private set; }

        public ModuleDeclarationAttribute(string name, string description, string author, string version)
        {
            if (name.Contains("+"))
                throw new Exception($"{name} includes a \"+\" character, which is disallowed.");
            Name = name;
            Description = description;
            Author = author;
            Version = version;
        }

        public class ModuleVersion
        {
            public int Major { get; set; }
            public int Minor { get; set; }

            public string Tag { get; set; }

            public static implicit operator ModuleVersion(string data)
            {
                var mod = new ModuleVersion();
                if (data.Split(' ').Length > 0)
                    mod.Tag = data.Split(' ')[1];

                mod.Major = int.Parse(data.Split('.')[0]);
                mod.Minor = int.Parse(data.Split('.')[0]);

                return mod;
            }

            public static implicit operator string (ModuleVersion version)
            {
                return version.Major + "." + version.Minor + " " + version.Tag;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class GameObjectAttribute : Attribute
    {
        /// <summary>
        /// Creates a new GameObjectAttribute.
        /// </summary>
        /// <param name="reflectionName">The reflection name of the object.</param>
        /// <param name="componentsFile">The .json file that contains the component lists for the mod</param>
        public GameObjectAttribute(string componentsFile)
        {
            ComponentFile = componentsFile;
        }
        public string ComponentFile { get; private set; }
        /// <summary>
        /// The display name of the object to be shown in error messages or the map editor.
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// The description of the object to be shown in the map editor.
        /// </summary>
        public string Description { get; set; }
        public Module Owner { get; set; }
    }
}
