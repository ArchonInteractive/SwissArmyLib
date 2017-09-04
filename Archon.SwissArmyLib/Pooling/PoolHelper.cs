using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Archon.SwissArmyLib.Pooling
{
    public static class PoolHelper
    {
        private static readonly Dictionary<Object, GameObjectPool<Object>> Pools = new Dictionary<Object, GameObjectPool<Object>>();
        private static readonly Dictionary<Object, Object> Prefabs = new Dictionary<Object, Object>();

        public static T Spawn<T>(T prefab)
            where T : Object
        {
            var pool = GetPool(prefab);
            var obj = pool.Spawn();
            Prefabs[obj] = prefab;

            return obj as T;
        }

        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent)
            where T : Object
        {
            var pool = GetPool(prefab);
            var obj = pool.Spawn(position, rotation, parent);
            Prefabs[obj] = prefab;

            return obj as T;
        }

        public static void Despawn(IPoolable target)
        {
            var unityObject = target as Object;

            if (unityObject == null)
                throw new InvalidOperationException("Cannot despawn target because it is not a UnityEngine.Object!");

            var prefab = GetPrefab(unityObject);
            var pool = GetPool(prefab);
            pool.Despawn(unityObject);
        }

        public static Object GetPrefab(Object instance)
        {
            Object prefab;
            Prefabs.TryGetValue(instance, out prefab);
            return prefab;
        }

        public static T GetPrefab<T>(T instance)
            where T : Object
        {
            Object prefab;
            Prefabs.TryGetValue(instance, out prefab);
            return prefab as T;
        }

        public static GameObjectPool<Object> GetPool(Object prefab)
        {
            GameObjectPool<Object> pool;
            Pools.TryGetValue(prefab, out pool);

            if (pool == null)
            {
                pool = new GameObjectPool<Object>(prefab);
                Pools[prefab] = pool;
            }

            return pool;
        }
    }

    public static class PoolHelper<T> where T : class, new()
    {
        private static readonly Pool<T> Pool = new Pool<T>(() => new T());

        public static T Spawn()
        {
            return Pool.Spawn();
        }

        public static void Despawn(T target)
        {
            Pool.Despawn(target);
        }
    }
}