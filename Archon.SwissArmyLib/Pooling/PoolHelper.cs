using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Archon.SwissArmyLib.Pooling
{
    /// <summary>
    /// Simple static helper class for pooling Unity prefab instances.
    /// 
    /// If the pooled objects implement <see cref="IPoolable"/> they will be notified when they're spawned and despawned.
    /// 
    /// For non-Unity objects see <see cref="PoolHelper{T}"/>.
    /// </summary>
    public static class PoolHelper
    {
        private static readonly Dictionary<Object, GameObjectPool<Object>> PrefabToPool = new Dictionary<Object, GameObjectPool<Object>>();
        private static readonly Dictionary<Object, Object> InstanceToPrefab = new Dictionary<Object, Object>();

        private static readonly List<Object> DestroyedInstances = new List<Object>();

        static PoolHelper()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private static void OnSceneUnloaded(Scene unloadedScene)
        {
            foreach (var instance in InstanceToPrefab.Keys)
            {
                if (!instance)
                    DestroyedInstances.Add(instance);
            }

            for (var i = 0; i < DestroyedInstances.Count; i++)
                InstanceToPrefab.Remove(DestroyedInstances[i]);

            DestroyedInstances.Clear();
        }

        /// <summary>
        /// Spawns a recycled object if there's one available, otherwise creates a new instance.
        /// </summary>
        /// <returns>The spawned object.</returns>
        public static T Spawn<T>(T prefab)
            where T : Object
        {
            if (ReferenceEquals(prefab, null))
                throw new ArgumentNullException("prefab");

            var pool = GetPool(prefab);
            var obj = pool.Spawn();
            InstanceToPrefab[obj] = prefab;

            return obj as T;
        }

        /// <summary>
        /// Spawns a recycled object if there's one available, otherwise creates a new instance.
        /// </summary>
        /// <returns>The spawned object.</returns>
        public static T Spawn<T>(T prefab, Transform parent)
            where T : Object
        {
            if (ReferenceEquals(prefab, null))
                throw new ArgumentNullException("prefab");

            var pool = GetPool(prefab);
            var obj = pool.Spawn(parent);
            InstanceToPrefab[obj] = prefab;

            return obj as T;
        }

        /// <summary>
        /// Spawns a recycled object if there's one available, otherwise creates a new instance.
        /// </summary>
        /// <returns>The spawned object.</returns>
        public static T Spawn<T>(T prefab, Vector3 position)
            where T : Object
        {
            if (ReferenceEquals(prefab, null))
                throw new ArgumentNullException("prefab");

            var pool = GetPool(prefab);
            var obj = pool.Spawn(position);
            InstanceToPrefab[obj] = prefab;

            return obj as T;
        }

        /// <summary>
        /// Spawns a recycled object if there's one available, otherwise creates a new instance.
        /// </summary>
        /// <returns>The spawned object.</returns>
        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation)
            where T : Object
        {
            if (ReferenceEquals(prefab, null))
                throw new ArgumentNullException("prefab");

            var pool = GetPool(prefab);
            var obj = pool.Spawn(position, rotation);
            InstanceToPrefab[obj] = prefab;

            return obj as T;
        }

        /// <summary>
        /// Spawns a recycled object if there's one available, otherwise creates a new instance.
        /// </summary>
        /// <returns>The spawned object.</returns>
        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent)
            where T : Object
        {
            if (ReferenceEquals(prefab, null))
                throw new ArgumentNullException("prefab");

            var pool = GetPool(prefab);
            var obj = pool.Spawn(position, rotation, parent);
            InstanceToPrefab[obj] = prefab;

            return obj as T;
        }

        /// <summary>
        /// Despawns an instance and marks it for reuse.
        /// </summary>
        /// <param name="target">The instance to despawn.</param>
        public static void Despawn(Object target)
        {
            if (ReferenceEquals(target, null))
                throw new ArgumentNullException("target");

            var prefab = GetPrefab(target);

            if (ReferenceEquals(prefab, null))
                throw new ArgumentException("Cannot find prefab for target.");

            var pool = GetPool(prefab);
            pool.Despawn(target);
        }


        /// <summary>
        /// Despawns an instance after a delay.
        /// </summary>
        /// <param name="target">The instance to despawn.</param>
        /// <param name="delay">Time in seconds to wait before despawning the target.</param>
        /// <param name="unscaledTime">Should the delay be according to <see cref="Time.time"/> or <see cref="Time.unscaledTime"/>?</param>
        public static void Despawn(Object target, float delay, bool unscaledTime = false)
        {
            if (ReferenceEquals(target, null))
                throw new ArgumentNullException("target");

            var prefab = GetPrefab(target);
            var pool = GetPool(prefab);
            pool.Despawn(target, delay, unscaledTime);
        }

        /// <summary>
        /// Gets the amount of free instances in the pool for the specified prefab.
        /// </summary>
        /// <returns>The amount of free instances in the pool.</returns>
        public static int GetFreeCount(Object prefab)
        {
            if (ReferenceEquals(prefab, null))
                throw new ArgumentNullException("prefab");

            var pool = GetPool(prefab);
            return pool != null ? pool.FreeCount : 0;
        }

        /// <summary>
        /// Gets the prefab that was used to spawn <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">The instance to get the prefab for.</param>
        /// <returns>The prefab for the instance, or null if not found.</returns>
        public static Object GetPrefab(Object instance)
        {
            if (ReferenceEquals(instance, null))
                throw new ArgumentNullException("instance");

            Object prefab;
            InstanceToPrefab.TryGetValue(instance, out prefab);
            return prefab;
        }

        /// <summary>
        /// Gets the prefab that was used to spawn <paramref name="instance"/>.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="instance">The instance to get the prefab for.</param>
        /// <returns>The prefab for the instance, or null if not found.</returns>
        public static T GetPrefab<T>(T instance)
            where T : Object
        {
            if (ReferenceEquals(instance, null))
                throw new ArgumentNullException("instance");

            Object prefab;
            InstanceToPrefab.TryGetValue(instance, out prefab);
            return prefab as T;
        }

        /// <summary>
        /// Gets or creates the pool for the given prefab.
        /// </summary>
        /// <param name="prefab">The prefab to get a pool for.</param>
        /// <returns>The pool for the prefab.</returns>
        public static GameObjectPool<Object> GetPool(Object prefab)
        {
            if (ReferenceEquals(prefab, null))
                throw new ArgumentNullException("prefab");

            GameObjectPool<Object> pool;

            if (!PrefabToPool.TryGetValue(prefab, out pool))
            {
                pool = new GameObjectPool<Object>(prefab, true);
                PrefabToPool[prefab] = pool;
            }

            return pool;
        }
    }

    /// <summary>
    /// Simple static helper class for pooling non-Unity objects.
    /// 
    /// If the pooled objects implement <see cref="IPoolable"/> they will be notified when they're spawned and despawned.
    /// 
    /// For Unity GameObjects see <see cref="PoolHelper"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to pool.</typeparam>
    public static class PoolHelper<T> where T : class, new()
    {
        private static readonly Pool<T> Pool = new Pool<T>(() => new T());

        /// <summary>
        /// Gets the amount of free instances in the pool.
        /// </summary>
        public static int FreeCount { get { return Pool.FreeCount; } }

        /// <summary>
        /// Spawns a recycled or new instance of the type <typeparamref name="T"/>.
        /// </summary>
        /// <returns>The spawned instance.</returns>
        public static T Spawn()
        {
            return Pool.Spawn();
        }

        /// <summary>
        /// Despawns an instance of the type <typeparamref name="T"/> and marks it for reuse.
        /// </summary>
        /// <param name="target">The instance to despawn.</param>
        public static void Despawn(T target)
        {
            Pool.Despawn(target);
        }

        /// <summary>
        /// Despawns an object after a delay.
        /// </summary>
        /// <param name="target">The target to despawn.</param>
        /// <param name="delay">Time in seconds to wait before despawning the target.</param>
        /// <param name="unscaledTime">Should the delay be according to <see cref="Time.time"/> or <see cref="Time.unscaledTime"/>?</param>
        public static void Despawn(T target, float delay, bool unscaledTime = false)
        {
            Pool.Despawn(target, delay, unscaledTime);
        }
    }
}