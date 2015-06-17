﻿using Microsoft.Xna.Framework;
using MPTanks.Engine.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPTanks.Engine
{
    public partial class GameObject
    {
        public byte[] FullState { get { return GetFullState(); } set { SetFullState(value); } }
        private enum __SerializationGameObjectType : byte
        {
            GameObject,
            Tank,
            Projectile,
            MapObject
        }
        public static GameObject CreateAndAddFromSerializationInformation(GameCore game, byte[] serializationData, bool authorized = true)
        {
            var nameLength = SerializationHelpers.GetValue<ushort>(serializationData, 0);
            var name = Encoding.UTF8.GetString(serializationData, 4, nameLength);
            var type = (__SerializationGameObjectType)serializationData[nameLength + 4];
            var guid = new Guid(serializationData.Slice(nameLength + 4 + 1, 16));

            GameObject obj;
            if (type == __SerializationGameObjectType.Tank)
                obj = game.AddTank(name, game.PlayersById[guid], authorized);
            else if (type == __SerializationGameObjectType.Projectile)
                obj = game.AddProjectile(name, game.PlayersById[guid].Tank, authorized);
            else if (type == __SerializationGameObjectType.MapObject)
                obj = game.AddMapObject(name, authorized);
            else
                obj = game.AddGameObject(name, authorized);

            obj.SetFullState(serializationData);

            return obj;
        }
        /// <summary>
        /// Gets the full state of the object, ergo.
        /// </summary>
        /// <returns></returns>
        public byte[] GetFullState()
        {
            //Layout:
            //variable string reflection name
            //1 byte GameObjectType
            //[used in all, only useful for tanks and projectiles]: player GUID
            //      If projectile, its the tank that spawned it
            //      If tank, the player that ids it
            //1 byte is sensor
            //1 byte is static
            //4 byte object id
            //4 byte color mask
            //4 byte time alive in ms
            //8 byte size
            //8 byte pos
            //8 byte lin velocity
            //4 byte rotation
            //4 byte rot velocity
            //4 byte restitution
            /////Then, there's the data from the instance
            //variable Private state data bytes
            var privateStateObject = GetPrivateStateData();
            byte[] privateState;

            //Figure out what the final output should be (encoded object, string, or plain byte array)
            if (privateStateObject.GetType() == typeof(byte[]))
                privateState = (byte[])privateStateObject;
            else if (privateStateObject.GetType() == typeof(string))
            {
                privateState = SerializationHelpers.AllocateArray(true,
                    SerializationHelpers.StringSerializationBytes,
                    privateStateObject);
            }
            else
            {
                var serialized = SerializeStateChangeObject(privateStateObject);
                privateState = SerializationHelpers.AllocateArray(true,
                    SerializationHelpers.JSONSerilizationBytes,
                    serialized);
            }


            //And figure out which guid to print
            var guidToWrite = new Guid();

            if (GetSerializationType().GetType().IsSubclassOf(typeof(Tanks.Tank)))
                guidToWrite = ((Tanks.Tank)this).Player.Id;
            else if (GetSerializationType().GetType().IsSubclassOf(typeof(Projectiles.Projectile)))
                guidToWrite = ((Projectiles.Projectile)this).Owner.Player.Id;

            return SerializationHelpers.AllocateArray(true,
            ObjectId,
            ReflectionName,
            (byte)GetSerializationType(),
            guidToWrite,
            IsSensor,
            IsStatic,
            ColorMask,
            TimeAliveMs,
            Size,
            Position,
            LinearVelocity,
            Rotation,
            AngularVelocity,
            Restitution,
            privateState
            );
        }

        private __SerializationGameObjectType GetSerializationType()
        {
            if (GetType().IsSubclassOf(typeof(Tanks.Tank)))
                return __SerializationGameObjectType.Tank;
            if (GetType().IsSubclassOf(typeof(Maps.MapObjects.MapObject)))
                return __SerializationGameObjectType.MapObject;
            if (GetType().IsSubclassOf(typeof(Projectiles.Projectile)))
                return __SerializationGameObjectType.Projectile;

            return __SerializationGameObjectType.GameObject;
        }

        /// <summary>
        /// Return a byte array for optimal performance, or either a string or other random object for ease of use.
        /// </summary>
        /// <returns></returns>
        protected virtual object GetPrivateStateData() => SerializationHelpers.EmptyByteArray;

        public void SetFullState(byte[] state)
        {
            var reflectionNameLength = SerializationHelpers.GetValue<ushort>(state, 0);
            int offset = 0;
            SetStateHeader(state, ref offset);
            

            var privateState = state.GetByteArray(offset);

            if (privateState.SequenceBegins(SerializationHelpers.JSONSerilizationBytes))
                SetFullStateInternal(DeserializeStateChangeObject(
                    privateState.GetString(SerializationHelpers.JSONSerilizationBytes.Length)));
            else if (privateState.SequenceBegins(SerializationHelpers.StringSerializationBytes))
                SetFullStateInternal(privateState.GetString(SerializationHelpers.StringSerializationBytes.Length));
            else SetFullStateInternal(privateState);
        }


        private void SetStateHeader(byte[] header, ref int offset)
        {
            var id = SerializationHelpers.GetValue<ushort>(header, offset); offset += 2;
            var nameLength = SerializationHelpers.GetValue<ushort>(header, offset); offset += 2;
            var name = Encoding.UTF8.GetString(header, offset, nameLength); offset += nameLength;
            var type = (__SerializationGameObjectType)header[offset++];
            var guid = new Guid(header.Slice(offset, 16)); offset += 16;
            var isSensor = header[offset++] == 1;
            var isStatic = header[offset++] == 1;
            var color = SerializationHelpers.GetColor(header, offset); offset += 4;
            var timeAlive = SerializationHelpers.GetFloat(header, offset); offset += 4;
            var size = SerializationHelpers.GetVector(header, offset); offset += 8;
            var position = SerializationHelpers.GetVector(header, offset); offset += 8;
            var linVel = SerializationHelpers.GetVector(header, offset); offset += 8;
            var rot = SerializationHelpers.GetFloat(header, offset); offset += 4;
            var rotVel = SerializationHelpers.GetFloat(header, offset); offset += 4;
            var restitution = SerializationHelpers.GetFloat(header, offset); offset += 4;

            IsSensor = isSensor;
            IsStatic = isStatic;
            ObjectId = id;
            ColorMask = color;
            TimeAliveMs = timeAlive;
            Size = size;
            Position = position;
            LinearVelocity = linVel;
            Rotation = rot;
            AngularVelocity = rotVel;
        }

        protected virtual void SetFullStateInternal(byte[] stateData)
        {

        }

        protected virtual void SetFullStateInternal(string state)
        {

        }

        protected virtual void SetFullStateInternal(dynamic state)
        {

        }

    }
}
