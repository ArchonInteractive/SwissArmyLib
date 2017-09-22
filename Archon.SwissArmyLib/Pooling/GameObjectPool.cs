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
        public override T Spawn()
        {
            var obj = base.Spawn();

            var gameObject = GetGameObject(obj);
            gameObject.transform.SetParent(null, false);
            if (_multiScene)
                SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
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
            if (target == null)
                throw new NullReferenceException("Target is null.");

            base.Despawn(target);

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