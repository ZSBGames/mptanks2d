﻿using Microsoft.Xna.Framework;
using MPTanks.Engine;
using MPTanks.Engine.Projectiles;
using MPTanks.Networking.Common.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPTanks.Networking.Client
{
    public class PseudoStateInterpolator
    {
        private struct PseudoStateObjectCorrection
        {
            public GameObject Object { get; set; }
            public Vector2 DistanceOffset { get; set; }
            public Vector2 DistanceAmountApplied { get; set; }
            public float RotationOffset { get; set; }
            public float RotationAmountApplied { get; set; }
        }
        private List<PseudoStateObjectCorrection> _corrections
            = new List<PseudoStateObjectCorrection>();
        private GameCore _game;
        public TimeSpan ElapsedTime { get; set; }
        public TimeSpan InterpolationTime { get; set; } = TimeSpan.FromMilliseconds(500);
        public float DistanceCorrectionLimit { get; set; } = 3f;
        public float VelocityLimit { get; set; } = 3f;
        public float RotationCorrectionLimit { get; set; } = MathHelper.PiOver4 / 2;
        public void Compute(GameCore game, PseudoFullGameWorldState state)
        {
            ElapsedTime = TimeSpan.Zero;
            _game = game;
            _corrections.Clear();
            foreach (var obj in state.ObjectStates)
            {
                if (_game.GameObjectsById.ContainsKey(obj.Key))
                {
                    var gameObj = _game.GameObjectsById[obj.Key];
                    if (!obj.Value.PositionChanged && !obj.Value.RotationChanged) continue;

                    //Handle destroyed
                    if (obj.Value.WasDestroyed)
                    {
                        game.ImmediatelyForceObjectDestruction(gameObj);
                        continue;
                    }

                    //Apply the non interpolated updates
                    gameObj.IsSensor = obj.Value.IsSensorObject;
                    gameObj.IsStatic = obj.Value.IsStaticObject;

                    if (obj.Value.RestitutionChanged)
                        gameObj.Restitution = obj.Value.Restitution;
                    if (obj.Value.RotationChanged)
                        gameObj.Rotation = obj.Value.Rotation;
                    if (obj.Value.RotationVelocityChanged)
                        gameObj.AngularVelocity = obj.Value.RotationVelocity;
                    if (obj.Value.SizeChanged)
                        gameObj.Size = obj.Value.Size.ToVector2();
                    if (obj.Value.VelocityChanged)
                        gameObj.LinearVelocity = obj.Value.Velocity.ToVector2();

                    Vector2 dist;
                    if (obj.Value.PositionChanged)
                        dist = obj.Value.Position - gameObj.Position;
                    else dist = Vector2.Zero;

                    float rot;
                    if (obj.Value.RotationChanged)
                        rot = obj.Value.Rotation - gameObj.Rotation;
                    else rot = 0;

                    //Apply directly if it's too large
                    if (DistanceCorrectionLimit < Math.Abs(dist.X) ||
                        DistanceCorrectionLimit < Math.Abs(dist.Y) ||
                        RotationCorrectionLimit < rot)
                    {
                        gameObj.Position = obj.Value.Position;
                        gameObj.Rotation = obj.Value.Rotation;
                    }
                    else
                    {
                        //Correct slowly if it's small
                        var corr = new PseudoStateObjectCorrection();
                        corr.DistanceOffset = dist;
                        corr.RotationOffset = rot;
                        corr.Object = gameObj;
                        _corrections.Add(corr);
                    }
                }
            }
        }
        public void Apply(GameTime gameTime)
        {
            if (_game == null) return;
            if (ElapsedTime > InterpolationTime) return; //Short circuit optimization
            var amount = (float)(gameTime.ElapsedGameTime.TotalMilliseconds / InterpolationTime.TotalMilliseconds);
            ElapsedTime += gameTime.ElapsedGameTime;
            for (var i = 0; i < _corrections.Count; i++)
            {
                var correction = _corrections[i];
                if (correction.Object.Alive)
                {
                    //Compute how far to rotate it
                    float rotationAmount;

                    if (correction.RotationOffset > 0)
                        rotationAmount = Math.Min(correction.RotationOffset * amount,
                        correction.RotationOffset - correction.RotationAmountApplied);
                    else
                        rotationAmount = Math.Max(correction.RotationOffset * amount,
                        correction.RotationOffset - correction.RotationAmountApplied);
                    //And how far to move it
                    float distanceX, distanceY;
                    if (correction.DistanceOffset.X > 0)
                        distanceX = Math.Min(correction.DistanceOffset.X * amount,
                            correction.DistanceOffset.X - correction.DistanceAmountApplied.X);
                    else
                        distanceX = Math.Max(correction.DistanceOffset.X * amount,
                            correction.DistanceOffset.X - correction.DistanceAmountApplied.X);

                    if (correction.DistanceOffset.Y > 0)
                        distanceY = Math.Min(correction.DistanceOffset.Y * amount,
                          correction.DistanceOffset.Y - correction.DistanceAmountApplied.Y);
                    else
                        distanceY = Math.Max(correction.DistanceOffset.Y * amount,
                          correction.DistanceOffset.Y - correction.DistanceAmountApplied.Y);
                    //Make a note for the next iteration
                    correction.RotationAmountApplied += rotationAmount;
                    correction.DistanceAmountApplied += new Vector2(distanceX, distanceY);
                    //And apply
                    var obj = correction.Object;
                    obj.Position += new Vector2(distanceX, distanceY);
                    obj.Rotation += rotationAmount;
                }

                _corrections[i] = correction;
            }
        }
    }
}
