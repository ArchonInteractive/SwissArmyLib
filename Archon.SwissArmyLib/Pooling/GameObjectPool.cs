using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Archon.SwissArmyLib.Pooling
{
    /// <summary>
    /// An object pool that can recycle prefab instances.
    /// </summary>
    /// <typeparam name="T">The type of the component on the prefab.</typeparam>
    public class GameObjectPool<T> : Pool<T> where T : Object
    {
        /// <summary>
        /// Gets the prefab used to instantiate GameObjects.
        /// </summary>
        public T Prefab { get; private set; }

        private readonly Transform _root;

        /// <summary>
        /// Creates a new GameObject pool for the specified prefab.
        /// </summary>
        /// <param name="prefab">The prefab used for instantiating instances.</param>
        public GameObjectPool(T prefab) : this(prefab.name, () => Object.Instantiate(prefab))
        {
            Prefab = prefab;
        }

        /// <summary>
        /// Creates a new GameObject pool with a custom name and a factory method used for instantiating instances.
        /// </summary>
        /// <param name="name">The name of the pool.</param>
        /// <param name="create">The factory method used to instantiating instances.</param>
        public GameObjectPool(string name, Func<T> create) : base(create)
        {
            var rootGO = new GameObject(string.Format("'{0}' Pool", name));
            _root = rootGO.transform;
        }

        /// <inheritdoc />
        public override T Spawn()
        {
            var obj = base.Spawn();

            var gameObject = GetGameObject(obj);
            gameObject.SetActive(true);

            return obj;
        }

        /// <summary>
        /// Spawns a recycled object if there's one available, otherwise creates a new instance.
        /// </summary>
        /// <returns>The spawned object.</returns>
        public T Spawn(Vector3 position, Quaternion rotation, Transform parent)
        {
            var obj = Spawn();

            var gameObject = GetGameObject(obj);

            var transform = gameObject.transform;
            transform.position = position;
            transform.rotation = rotation;
            transform.parent = parent;

            return obj;
        }

        /// <inheritdoc />
        public override void Despawn(T target)
        {
            base.Despawn(target);

            CancelDespawn(target);

            var gameObject = GetGameObject(target);
            gameObject.SetActive(false);

            var transform = gameObject.transform;
            transform.SetParent(_root, false);
        }

        private static GameObject GetGameObject(T obj)
        {
            var component = obj as Component;
            if (component != null)
                return component.gameObject;

            return obj as GameObject;
        }
    }
}