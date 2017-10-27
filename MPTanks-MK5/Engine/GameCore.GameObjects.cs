using Microsoft.Xna.Framework;
using MPTanks.Engine.Settings;
using MPTanks.Engine.Tanks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPTanks.Engine
{
    public partial class GameCore
    {

        #region Add and Remove GameObjects
        private bool _inUpdateLoop = false;
        private HashSet<GameObject> _addQueue =
            new HashSet<GameObject>();
        private HashSet<GameObject> _removeQueue =
            new HashSet<GameObject>();
        private HashSet<GameObject> _objectsCurrentlyInDestructors =
            new HashSet<GameObject>();
        public void AddGameObject(GameObject obj, GameObject creator = null, bool authorized = false)
        {
            if (obj.ObjectId == ushort.MaxValue) obj.ObjectId = NextObjectId;
            if (!authorized && !Authoritative)
            {
                Logger.Error("Unauthorized object addition attempted.");
                return;
            }

            if (creator == null)
            {
                if (GlobalSettings.Debug)
                    Logger.Trace("Object created: " + $"{obj.GetType().FullName}" +
                    $"[{obj.ObjectId}]. Authorized: {authorized}");
            }
            else
            {
                if (GlobalSettings.Debug)
                    Logger.Trace("Object created: " + $"{obj.GetType().FullName}" +
                        $"[{obj.ObjectId}]. Created by {creator.GetType().FullName}" +
                        $"[{creator.ObjectId}]. Authorized: {authorized}");
            }

            if (_inUpdateLoop) //In update loop, wait a frame.
            {
                _addQueue.Add(obj);
            }
            else
            {
                _gameObjects.Add(obj.ObjectId, obj);
                obj.Create(); //Call the creator function
            }
        }

        public void RemoveGameObject(GameObject obj, GameObject destructor = null, bool authorized = false)
        {
            if (GlobalSettings.Debug)
            {
                if (!authorized && !Authoritative)
                    throw new Exception("Unauthorized removal of object");
            }
            else
            {
                if (!authorized && !Authoritative)
                {
                    Logger.Error("Unauthorized object destruction attempted.");
                    return;
                }
            }

            if (destructor == null)
            {
                if (GlobalSettings.Debug)
                    Logger.Trace("Object destroyed: " + $"{obj.GetType().FullName}" +
                    $"[{obj.ObjectId}]. Authorized: {authorized}");
            }
            else
            {
                if (GlobalSettings.Debug)
                    Logger.Trace("Object destroyed: " + $"{obj.GetType().FullName}" +
                    $"[{obj.ObjectId}]. Destroyed by {destructor.GetType().FullName}" +
                    $"[{destructor.ObjectId}]. Authorized: {authorized}");
            }

            //Sanity checks
            bool found = _gameObjects.ContainsValue(obj) || _addQueue.Contains(obj);
            //We want to prevent people from disposing of the bodies
            if (obj.Body.IsDisposed && found)
                Logger.Warning("Body already disposed, Trace:\n" + Environment.StackTrace);
            if (!found)
                return; //It doesn't exist - probably was already deleted by a previous object

            BeginDeletion(obj, destructor);
        }

        /// <summary>
        /// Processes the add and remove queue for gameobjects
        /// </summary>
        private void ProcessGameObjectQueues()
        {
            ProcessGameObjectDestructorQueue();
            ProcessGameObjectDeletionAndAdditionQueue();
        }

        private void ProcessGameObjectDeletionAndAdditionQueue()
        {
            foreach (var obj in _addQueue)
            {
                _gameObjects.Add(obj.ObjectId, obj);
                obj.Create(); //Call the creator function
            }

            foreach (var obj in _removeQueue)
            {
                _gameObjects.Remove(obj.ObjectId);
                obj.EndDestruction(); //Call final destructor
            }

            _addQueue.Clear();
            _removeQueue.Clear();
        }

        private HashSet<GameObject> _tempDestructorRemovalQueue =
            new HashSet<GameObject>();
        private void ProcessGameObjectDestructorQueue()
        {
            foreach (var obj in _objectsCurrentlyInDestructors)
            {
                if (obj.IsDestructionCompleted)
                    _tempDestructorRemovalQueue.Add(obj);
            }

            foreach (var obj in _tempDestructorRemovalQueue)
            {
                MarkGameObjectForDeletion(obj);
                _objectsCurrentlyInDestructors.Remove(obj);
            }
            _tempDestructorRemovalQueue.Clear();
        }

        private void MarkGameObjectForDeletion(GameObject obj)
        {
            if (_inUpdateLoop) //We're in the for loop so wait a frame
            {
                if (_addQueue.Contains(obj))
                    _addQueue.Remove(obj);
                else
                    _removeQueue.Add(obj);
            }
            else
            {
                _gameObjects.Remove(obj.ObjectId);
                obj.EndDestruction(); //Call final destructor
            }
        }

        private void AddGameObjectToDestructorQueue(GameObject obj)
        {
            _objectsCurrentlyInDestructors.Add(obj);
        }

        public void ImmediatelyForceObjectDestruction(GameObject obj)
        {
            if (_objectsCurrentlyInDestructors.Contains(obj))
            {
                _objectsCurrentlyInDestructors.Remove(obj);
                MarkGameObjectForDeletion(obj);
            }
            else if (GameObjectsById.ContainsKey(obj.ObjectId))
            {
                _gameObjects.Remove(obj.ObjectId);
                obj.Destroy();
                obj.EndDestruction();
            }
        }

        private void BeginDeletion(GameObject obj, GameObject destructor = null)
        {
            //Housekeeping
            if (obj.Destroy(destructor))
            {
                //It wants some time to finish up
                AddGameObjectToDestructorQueue(obj);
            }
            else
            {
                //No time needed, delete it
                MarkGameObjectForDeletion(obj);
            }
        }
        #endregion

        #region Helpers for object creation
        public Tank AddTank(string reflectionName, GamePlayer player, bool authorized, ushort? id = null)
        {
            return AddTank(reflectionName, player, Vector2.Zero, 0, authorized, id);
        }

        public Tank AddTank(string reflectionName, GamePlayer player, Vector2 position = default(Vector2), float rotation = 0, bool authorized = false, ushort? id = null)
        {
            var tank = Tank.ReflectiveInitialize(reflectionName, player, this, authorized);
            tank.Position = position;
            tank.Rotation = rotation;
            player.Tank = tank;
            if (id != null) tank.ObjectId = id.Value;
            AddGameObject(tank, null, authorized);
            return tank;
        }

        public Projectiles.Projectile AddProjectile(string reflectionName, Tank spawner, bool authorized, ushort? id = null)
        {
            return AddProjectile(reflectionName, spawner, Vector2.Zero, 0, authorized, id);
        }
        public Projectiles.Projectile AddProjectile(string reflectionName, Tank spawner, Vector2 position = default(Vector2), float rotation = 0, bool authorized = false, ushort? id = null)
        {
            var projectile = Projectiles.Projectile.ReflectiveInitialize(
                reflectionName, spawner, this, authorized, position, rotation);
            projectile.Position = position;
            projectile.Rotation = rotation;
            if (id != null) projectile.ObjectId = id.Value;
            AddGameObject(projectile, null, authorized);
            return projectile;
        }
        public Maps.MapObjects.MapObject AddMapObject(string reflectionName, bool authorized, ushort? id = null)
        {
            return AddMapObject(reflectionName, Vector2.Zero, 0, authorized, id);
        }
        public Maps.MapObjects.MapObject AddMapObject(string reflectionName, Vector2 position = default(Vector2), float rotation = 0, bool authorized = false, ushort? id = null)
        {
            var obj = Maps.MapObjects.MapObject.ReflectiveInitialize(reflectionName, this, authorized, position, rotation);
            if (id != null) obj.ObjectId = id.Value;
            AddGameObject(obj, null, authorized);
            return obj;
        }

        public GameObject AddGameObject(string reflectionName, bool authorized, ushort? id = null)
        {
            return AddGameObject(reflectionName, Vector2.Zero, 0, authorized, id);
        }
        public GameObject AddGameObject(string reflectionName, Vector2 position = default(Vector2), float rotation = 0, bool authorized = false, ushort? id = null)
        {
            var obj = GameObject.ReflectiveInitialize(reflectionName, this, authorized);
            obj.Position = position;
            obj.Rotation = rotation;
            if (id != null) obj.ObjectId = id.Value;
            AddGameObject(obj, null, authorized);
            return obj;
        }
        #endregion
    }
}
