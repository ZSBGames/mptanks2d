﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Settings
    {
        /// <summary>
        /// The physics engine runs at 1/10 scale for some idiotic reason.
        /// Basically, that means that a 3x5 tank is 0.3x0.5 blocks because
        /// Box2D a.k.a. Farseer works best with objects between 0.1 and 10 units
        /// in size.
        /// </summary>
        public const float PhysicsScale = 0.1f;


        /// <summary>
        /// The scale the rendering runs at relative to the blocks.
        /// This way, we can pass integers around safely.
        /// </summary>
        public const float RenderScale = 100f;

        public const float RenderLineThickness = 5;

        /// <summary>
        /// The amount of "blocks" to compensate for in rendering because of the 
        /// skin on physics objects
        /// </summary>
        public const float PhysicsCompensationForRendering = 0.085f;

        public const float TankDensity = 15;

        /// <summary>
        /// Number of milliseconds after GameCore initialization to wait before starting
        /// the game because we want all of the setup to be done and for people to be connected.
        /// </summary>
        public const float TimeToWaitBeforeStartingGame = 5000;

        /// <summary>
        /// The number of milliseconds after game to continue updating
        /// </summary>
        public const float TimePostGameToContinueRunning = 5000;

        /// <summary>
        /// The maximum number of particles to allow in game
        /// </summary>
        public const int ParticleLimit = 100000;
    }
}
