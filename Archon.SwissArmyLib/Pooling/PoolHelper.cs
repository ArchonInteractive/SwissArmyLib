using System.Collections.Generic;
using UnityEngine;

namespace Archon.SwissArmyLib.Pooling
{
    public static class PoolHelper
    {
        private static readonly Dictionary<Object, object> Pools = new Dictionary<Object, object>();

        private static readonly Dictionary<Object, Object> Prefabs = new Dictionary<Object, Object>();

        public static T Spawn<T>(T prefab)
            where T : Object
        {
            var pool = GetPool(prefab);
            var obj = pool.Spawn();
            Prefabs[obj] = prefab;

            return obj;
        }

        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent)
            where T : Object
        {
            var pool = GetPool(prefab);
            var obj = pool.Spawn(position, rotation, parent);
            Prefabs[obj] = prefab;

            return obj;
        }

        public static void Despawn<T>(T target)
            where T : Object
        {
            var prefab = GetPrefab(target);
            var pool = GetPool(prefab);
            pool.Despawn(target);
        }

        public static T GetPrefab<T>(T target)
            where T : Object
        {
            Object prefab;

            Prefabs.TryGetValue(target, out prefab);

            return prefab as T;
        }

        public static GameObjectPool<T> GetPool<T>(T prefab)
            where T : Object
        {
            object obj;
            Pools.TryGetValue(prefab, out obj);

            var pool = obj as GameObjectPool<T>;

            if (obj == null)
            {
                pool = new GameObjectPool<T>(prefab);
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