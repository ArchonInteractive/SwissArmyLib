using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Archon.SwissArmyLib.Pooling
{
    /// <summary>
    /// An object pool that can recycle prefab instances.
    /// </summary>
    /// <typeparam name="T">The type of the component on the prefab.</typeparam>
    public class GameObjectPool<T> : Pool<T>, IDisposable where T : Object
    {
        /// <summary>
        /// Gets the prefab used to instantiate GameObjects.
        /// </summary>
        public T Prefab { get; private set; }

        private readonly Transform _root;
        private readonly bool _multiScene;

        /// <summary>
        /// Creates a new GameObject pool for the specified prefab.
        /// </summary>
        /// <param name="prefab">The prefab used for instantiating instances.</param>
        /// <param name="multiScene">Should the pool and its contents survive a scene change?</param>
        public GameObjectPool(T prefab, bool multiScene) : this(prefab.name, () => Object.Instantiate(prefab), multiScene)
        {
            if (ReferenceEquals(prefab, null))
                throw new ArgumentNullException("prefab");

            Prefab = prefab;
        }

        /// <summary>
        /// Creates a new GameObject pool with a custom name and a factory method used for instantiating instances.
        /// </summary>
        /// <param name="name">The name of the pool.</param>
        /// <param name="create">The factory method used to instantiating instances.</param>
        /// <param name="multiScene">Should the pool and its contents survive a scene change?</param>
        public GameObjectPool(string name, Func<T> create, bool multiScene) : base(create)
        {
            var rootGO = new GameObject(string.Format("'{0}' Pool", name));
            _root = rootGO.transform;

            _multiScene = multiScene;
            if (multiScene)
                Object.DontDestroyOnLoad(rootGO);

            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~GameObjectPool()
        {
            if (_root)
                Object.Destroy(_root.gameObject);
        }

        /// <summary>
        /// Destroys the pool and any despawned objects in it.
        /// </summary>
        public void Dispose()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            if (_root)
                Object.Destroy(_root.gameObject);
            Free.Clear();
        }

        private void OnSceneUnloaded(Scene unloadedScene)
        {
            // clean up any instances that might've been destroyed
            for (var i = Free.Count - 1; i >= 0; i--)
            {
                if (!Free[i])
                    Free.RemoveAt(i);
            }
        }

        /// <inheritdoc />
        protected override T SpawnInternal()
        {
            var obj = base.SpawnInternal();

            var gameObject = GetGameObject(obj);
            gameObject.transform.SetParent(null, false);
            if (_multiScene)
                SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());

            return obj;
        }

        /// <summary>
        /// Spawns a recycled object if there's one available, otherwise creates a new instance.
        /// </summary>
        /// <returns>The spawned object.</returns>
        public T Spawn(Transform parent)
        {
            var obj = SpawnInternal();

            var gameObject = GetGameObject(obj);

            var transform = gameObject.transform;
            transform.SetParent(parent, false);

            OnSpawned(obj);

            return obj;
        }

        /// <summary>
        /// Spawns a recycled object if there's one available, otherwise creates a new instance.
        /// </summary>
        /// <returns>The spawned object.</returns>
        public T Spawn(Vector3 position)
        {
            var obj = SpawnInternal();

            var gameObject = GetGameObject(obj);

            var transform = gameObject.transform;
            transform.SetPositionAndRotation(position, Quaternion.identity);

            OnSpawned(obj);

            return obj;
        }

        /// <summary>
        /// Spawns a recycled object if there's one available, otherwise creates a new instance.
        /// </summary>
        /// <returns>The spawned object.</returns>
        public T Spawn(Vector3 position, Quaternion rotation)
        {
            var obj = SpawnInternal();

            var gameObject = GetGameObject(obj);

            var transform = gameObject.transform;
            transform.SetPositionAndRotation(position, rotation);

            OnSpawned(obj);

            return obj;
        }

        /// <summary>
        /// Spawns a recycled object if there's one available, otherwise creates a new instance.
        /// </summary>
        /// <returns>The spawned object.</returns>
        public T Spawn(Vector3 position, Quaternion rotation, Transform parent)
        {
            var obj = SpawnInternal();

            var gameObject = GetGameObject(obj);

            var transform = gameObject.transform;
            transform.SetParent(parent, false);
            transform.SetPositionAndRotation(position, rotation);

            OnSpawned(obj);

            return obj;
        }

        /// <inheritdoc />
        public override void Despawn(T target)
        {
            if (ReferenceEquals(target, null))
                throw new ArgumentNullException("target");

            try
            {
                base.Despawn(target);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException(string.Format("Target '{0}' is already despawned!", target.name), "target");
            }

            var gameObject = GetGameObject(target);
            gameObject.SetActive(false);

            var transform = gameObject.transform;
            transform.SetParent(_root, false);
        }

        /// <inheritdoc />
        protected override void OnSpawned(T target)
        {
            var gameObject = GetGameObject(target);
            gameObject.SetActive(true);

            base.OnSpawned(target);
        }

        private static GameObject GetGameObject(T obj)
        {
            if (ReferenceEquals(obj, null))
                throw new ArgumentNullException("obj");

            var component = obj as Component;
            if (component != null)
                return component.gameObject;

            return obj as GameObject;
        }
    }
}