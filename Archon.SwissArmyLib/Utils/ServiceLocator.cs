using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Archon.SwissArmyLib.Utils
{
    /// <summary>
    ///     A (somewhat) simple implementation of the service locator pattern.
    ///     The ServiceLocator knows about MonoBehaviours and how to work with them.
    ///     Creating scene-specific resolvers that only live as long as their respective scene is also supported.
    ///     <remarks>
    ///         Please note that when you load a new scene, the MonoBehaviours in that scene will have their Awake()
    ///         method called before their scene becomes the active one. This means you can't rely on
    ///         SceneManager.GetActiveScene() to return the scene they're in, so you might want to use GameObject.scene
    ///         to specify which scene to register the resolver for.
    ///     </remarks>
    /// </summary>
    public static class ServiceLocator
    {
        /// <summary>
        ///     Called when the global resolvers are reset.
        /// </summary>
        public static event Action GlobalReset;

        /// <summary>
        ///     Called when a scene's resolvers are reset.
        /// </summary>
        public static event Action<Scene> SceneReset;

        private static readonly Dictionary<Type, Func<object>> GlobalResolvers = new Dictionary<Type, Func<object>>();
        private static readonly Dictionary<Scene, SceneData> SceneResolvers = new Dictionary<Scene, SceneData>();

        private static readonly List<Scene> TempSceneList = new List<Scene>();

        private static GameObject _multiSceneGameObject;
        private static Scene _currentScene;

        private const string MultisceneGameObjectName = "ServiceLocator - Multi-scene";

        static ServiceLocator()
        {
            // in case unity has hot-reloaded
            if (Application.isEditor)
                _multiSceneGameObject = GameObject.Find(MultisceneGameObjectName);

            _currentScene = SceneManager.GetActiveScene();
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private static void OnActiveSceneChanged(Scene previous, Scene current)
        {
            _currentScene = current;
        }

        private static void OnSceneUnloaded(Scene unloadedScene)
        {
            ResetScene(unloadedScene);
        }

        /// <summary>
        ///     Registers a concrete singleton of the given type.
        /// </summary>
        /// <typeparam name="T">The type of the singleton.</typeparam>
        public static T RegisterSingleton<T>() where T : new()
        {
            return RegisterSingleton<T, T>();
        }

        /// <summary>
        ///     Registers a concrete singleton of the given type.
        ///     The instance won't be created until the first time it is resolved.
        /// </summary>
        /// <typeparam name="T">The type of the singleton.</typeparam>
        public static Lazy<T> RegisterLazySingleton<T>() where T : new()
        {
            return RegisterLazySingleton<T, T>();
        }

        /// <summary>
        ///     Registers a scene-specific concrete singleton of the given type.
        ///     <remarks>The resolver is registered for the active scene according to <see cref="SceneManager.GetActiveScene()" />.</remarks>
        /// </summary>
        /// <typeparam name="T">The type of the singleton.</typeparam>
        public static T RegisterSingletonForScene<T>() where T : new()
        {
            return RegisterSingletonForScene<T, T>(_currentScene);
        }

        /// <summary>
        ///     Registers a scene-specific concrete singleton of the given type.
        ///     The instance won't be created until the first time it is resolved.
        ///     <remarks>The resolver is registered for the active scene according to <see cref="SceneManager.GetActiveScene()" />.</remarks>
        /// </summary>
        /// <typeparam name="T">The type of the singleton.</typeparam>
        public static Lazy<T> RegisterLazySingletonForScene<T>() where T : new()
        {
            return RegisterLazySingletonForScene<T, T>(_currentScene);
        }

        /// <summary>
        ///     Registers a scene-specific concrete singleton of the given type.
        /// </summary>
        /// <typeparam name="T">The type of the singleton.</typeparam>
        /// <param name="scene">The scene to register the transient type for.</param>
        public static T RegisterSingletonForScene<T>(Scene scene) where T : new()
        {
            return RegisterSingletonForScene<T, T>(scene);
        }

        /// <summary>
        ///     Registers a scene-specific concrete singleton of the given type.
        ///     The instance won't be created until the first time it is resolved.
        /// </summary>
        /// <typeparam name="T">The type of the singleton.</typeparam>
        /// <param name="scene">The scene to register the transient type for.</param>
        public static Lazy<T> RegisterLazySingletonForScene<T>(Scene scene) where T : new()
        {
            return RegisterLazySingletonForScene<T, T>(scene);
        }

        /// <summary>
        ///     Registers a concrete singleton of the type <typeparamref name="TConcrete" /> for the abstract type
        ///     <typeparamref name="TAbstract" />.
        /// </summary>
        /// <typeparam name="TAbstract">The abstract type that will be mapped to <typeparamref name="TConcrete" />.</typeparam>
        /// <typeparam name="TConcrete">The concrete singleton implementation.</typeparam>
        public static TConcrete RegisterSingleton<TAbstract, TConcrete>() where TConcrete : TAbstract, new()
        {
            var instance = CreateInstance<TConcrete>();
            Func<object> resolver = () => instance;

            GlobalResolvers[typeof(TAbstract)] = resolver;

            return instance;
        }

        /// <summary>
        ///     Registers a concrete singleton of the type <typeparamref name="TConcrete" /> for the abstract type
        ///     <typeparamref name="TAbstract" />.
        ///     The instance won't be created until the first time it is resolved.
        /// </summary>
        /// <typeparam name="TAbstract">The abstract type that will be mapped to <typeparamref name="TConcrete" />.</typeparam>
        /// <typeparam name="TConcrete">The concrete singleton implementation.</typeparam>
        public static Lazy<TConcrete> RegisterLazySingleton<TAbstract, TConcrete>() where TConcrete : TAbstract, new()
        {
            var lazyInstance = new Lazy<TConcrete>(CreateFactory<TConcrete>());
            Func<object> resolver = () => lazyInstance.Value;

            GlobalResolvers[typeof(TAbstract)] = resolver;

            return lazyInstance;
        }

        /// <summary>
        ///     Registers a scene-specific concrete singleton of the type <typeparamref name="TConcrete" /> for the abstract type
        ///     <typeparamref name="TAbstract" />.
        ///     <remarks>The resolver is registered for the active scene according to <see cref="SceneManager.GetActiveScene()" />.</remarks>
        /// </summary>
        /// <typeparam name="TAbstract">The abstract type that will be mapped to <typeparamref name="TConcrete" />.</typeparam>
        /// <typeparam name="TConcrete">The concrete singleton implementation.</typeparam>
        public static TConcrete RegisterSingletonForScene<TAbstract, TConcrete>() where TConcrete : TAbstract, new()
        {
            return RegisterSingletonForScene<TAbstract, TConcrete>(_currentScene);
        }

        /// <summary>
        ///     Registers a scene-specific concrete singleton of the type <typeparamref name="TConcrete" /> for the abstract type
        ///     <typeparamref name="TAbstract" />.
        ///     The instance won't be created until the first time it is resolved.
        ///     <remarks>The resolver is registered for the active scene according to <see cref="SceneManager.GetActiveScene()" />.</remarks>
        /// </summary>
        /// <typeparam name="TAbstract">The abstract type that will be mapped to <typeparamref name="TConcrete" />.</typeparam>
        /// <typeparam name="TConcrete">The concrete singleton implementation.</typeparam>
        public static Lazy<TConcrete> RegisterLazySingletonForScene<TAbstract, TConcrete>()
            where TConcrete : TAbstract, new()
        {
            return RegisterLazySingletonForScene<TAbstract, TConcrete>(_currentScene);
        }

        /// <summary>
        ///     Registers a scene-specific concrete singleton of the type <typeparamref name="TConcrete" /> for the abstract type
        ///     <typeparamref name="TAbstract" />.
        /// </summary>
        /// <typeparam name="TAbstract">The abstract type that will be mapped to <typeparamref name="TConcrete" />.</typeparam>
        /// <typeparam name="TConcrete">The concrete singleton implementation.</typeparam>
        /// <param name="scene">The scene to register the transient type for.</param>
        public static TConcrete RegisterSingletonForScene<TAbstract, TConcrete>(Scene scene)
            where TConcrete : TAbstract, new()
        {
            var sceneData = GetOrCreateSceneData(scene);

            var instance = CreateInstance<TConcrete>(scene);
            Func<object> resolver = () => instance;

            sceneData.Resolvers[typeof(TAbstract)] = resolver;

            return instance;
        }

        /// <summary>
        ///     Registers a scene-specific concrete singleton of the type <typeparamref name="TConcrete" /> for the abstract type
        ///     <typeparamref name="TAbstract" />.
        ///     The instance won't be created until the first time it is resolved.
        /// </summary>
        /// <typeparam name="TAbstract">The abstract type that will be mapped to <typeparamref name="TConcrete" />.</typeparam>
        /// <typeparam name="TConcrete">The concrete singleton implementation.</typeparam>
        /// <param name="scene">The scene to register the transient type for.</param>
        public static Lazy<TConcrete> RegisterLazySingletonForScene<TAbstract, TConcrete>(Scene scene)
            where TConcrete : TAbstract, new()
        {
            var sceneData = GetOrCreateSceneData(scene);

            var lazyInstance = new Lazy<TConcrete>(CreateFactory<TConcrete>(scene));
            Func<object> resolver = () => lazyInstance.Value;

            sceneData.Resolvers[typeof(TAbstract)] = resolver;

            return lazyInstance;
        }

        /// <summary>
        ///     Registers a specific instance to be a singleton for its concrete type.
        /// </summary>
        /// <typeparam name="T">The type of the singleton.</typeparam>
        /// <param name="instance">The instance to register as a singleton.</param>
        public static void RegisterSingleton<T>(T instance)
        {
            RegisterSingleton<T, T>(instance);
        }

        /// <summary>
        ///     Registers a specific instance to be a singleton for the abstract type.
        /// </summary>
        /// <typeparam name="TAbstract">The abstract ype that will be mapped to <typeparamref name="TConcrete"/>.</typeparam>
        /// <typeparam name="TConcrete">The concrete singleton implementation.</typeparam>
        /// <param name="instance">The instance to register as a singleton.</param>
        public static void RegisterSingleton<TAbstract, TConcrete>(TConcrete instance)
        {
            GlobalResolvers[typeof(TAbstract)] = () => instance;
        }

        /// <summary>
        ///     Registers a specific instance to be a scene-specific singleton for its concrete type.
        ///     <remarks>The resolver is registered for the active scene according to <see cref="SceneManager.GetActiveScene()" />.</remarks>
        /// </summary>
        /// <typeparam name="T">The type of the singleton.</typeparam>
        /// <param name="instance">The instance to register as a singleton.</param>
        public static void RegisterSingletonForScene<T>(T instance)
        {
            RegisterSingletonForScene(instance, _currentScene);
        }

        /// <summary>
        ///     Registers a specific instance to be a scene-specific singleton for the abstract type.
        ///     <remarks>The resolver is registered for the active scene according to <see cref="SceneManager.GetActiveScene()" />.</remarks>
        /// </summary>
        /// <typeparam name="TAbstract">The abstract type that will be mapped to <typeparamref name="TConcrete"/>.</typeparam>
        /// <typeparam name="TConcrete">The concrete singleton implementation.</typeparam>
        /// <param name="instance">The instance to register as a singleton.</param>
        public static void RegisterSingletonForScene<TAbstract, TConcrete>(TConcrete instance)
        {
            RegisterSingletonForScene<TAbstract, TConcrete>(instance, _currentScene);
        }

        /// <summary>
        ///     Registers a specific instance to be a scene-specific singleton for its concrete type.
        /// </summary>
        /// <typeparam name="T">The type of the singleton.</typeparam>
        /// <param name="instance">The instance to register as a singleton.</param>
        /// <param name="scene">The scene to register the singleton type for.</param>
        public static void RegisterSingletonForScene<T>(T instance, Scene scene)
        {
            RegisterSingletonForScene<T, T>(instance, scene);
        }

        /// <summary>
        ///     Registers a specific instance to be a scene-specific singleton for the abstract type.
        /// </summary>
        /// <typeparam name="TAbstract">The abstract ype that will be mapped to <typeparamref name="TConcrete"/>.</typeparam>
        /// <typeparam name="TConcrete">The concrete singleton implementation.</typeparam>
        /// <param name="instance">The instance to register as a singleton.</param>
        /// <param name="scene">The scene to register the singleton type for.</param>
        public static void RegisterSingletonForScene<TAbstract, TConcrete>(TConcrete instance, Scene scene)
        {
            var resolverMap = GetOrCreateSceneData(scene).Resolvers;
            resolverMap[typeof(TAbstract)] = () => instance;
        }

        /// <summary>
        ///     Registers a concrete transient type.
        ///     A new instance of the given type will be returned each time it is resolved.
        /// </summary>
        /// <typeparam name="T">The concrete transient type to register.</typeparam>
        public static void RegisterTransient<T>() where T : new()
        {
            RegisterTransient<T, T>();
        }

        /// <summary>
        ///     Registers scene-specific a concrete transient type.
        ///     A new instance of the given type will be returned each time it is resolved.
        ///     <remarks>The resolver is registered for the active scene according to <see cref="SceneManager.GetActiveScene()" />.</remarks>
        /// </summary>
        /// <typeparam name="T">The concrete transient type to register.</typeparam>
        public static void RegisterTransientForScene<T>() where T : new()
        {
            RegisterTransientForScene<T, T>(_currentScene);
        }

        /// <summary>
        ///     Registers scene-specific a concrete transient type.
        ///     A new instance of the given type will be returned each time it is resolved.
        /// </summary>
        /// <typeparam name="T">The concrete transient type to register.</typeparam>
        /// <param name="scene">The scene to register the transient type for.</param>
        public static void RegisterTransientForScene<T>(Scene scene) where T : new()
        {
            RegisterTransientForScene<T, T>(scene);
        }

        /// <summary>
        ///     Registers a concrete transient type to return new instances of when the abstract type
        ///     <typeparamref name="TAbstract" /> is resolved.
        /// </summary>
        /// <typeparam name="TAbstract">The abstract type that will be mapped to <typeparamref name="TConcrete" />.</typeparam>
        /// <typeparam name="TConcrete">The concrete transient type to return when <typeparamref name="TAbstract" /> is resolved.</typeparam>
        public static void RegisterTransient<TAbstract, TConcrete>() where TConcrete : TAbstract, new()
        {
            var factory = CreateFactory<TConcrete>();
            RegisterTransient<TAbstract, TConcrete>(factory);
        }

        /// <summary>
        ///     Registers a scene-specific concrete transient type to return new instances of when the abstract type
        ///     <typeparamref name="TAbstract" /> is resolved.
        ///     <remarks>The resolver is registered for the active scene according to <see cref="SceneManager.GetActiveScene()" />.</remarks>
        /// </summary>
        /// <typeparam name="TAbstract">The abstract type that will be mapped to <typeparamref name="TConcrete" />.</typeparam>
        /// <typeparam name="TConcrete">The concrete transient type to return when <typeparamref name="TAbstract" /> is resolved.</typeparam>
        public static void RegisterTransientForScene<TAbstract, TConcrete>() where TConcrete : TAbstract, new()
        {
            RegisterTransientForScene<TAbstract, TConcrete>(_currentScene);
        }

        /// <summary>
        ///     Registers a scene-specific concrete transient type to return new instances of when the abstract type
        ///     <typeparamref name="TAbstract" /> is resolved.
        /// </summary>
        /// <typeparam name="TAbstract">The abstract type that will be mapped to <typeparamref name="TConcrete" />.</typeparam>
        /// <typeparam name="TConcrete">The concrete transient type to return when <typeparamref name="TAbstract" /> is resolved.</typeparam>
        /// <param name="scene">The scene to register the transient type for.</param>
        public static void RegisterTransientForScene<TAbstract, TConcrete>(Scene scene)
            where TConcrete : TAbstract, new()
        {
            var factory = CreateFactory<TConcrete>(scene);
            RegisterTransientForScene<TAbstract, TConcrete>(factory, scene);
        }

        /// <summary>
        ///     Registers a concrete transient type to return new instances of when <typeparamref name="T" /> is resolved.
        ///     The specified resolver will be used for producing the instances.
        /// </summary>
        /// <typeparam name="T">The concrete transient type to register.</typeparam>
        /// <param name="factory">The factory that will be used for creating instances.</param>
        public static void RegisterTransient<T>(Func<T> factory)
        {
            RegisterTransient<T, T>(factory);
        }

        /// <summary>
        ///     Registers a scene-specific concrete transient type to return new instances of when <typeparamref name="T" /> is
        ///     resolved.
        ///     The specified resolver will be used for producing the instances.
        ///     <remarks>The resolver is registered for the active scene according to <see cref="SceneManager.GetActiveScene()" />.</remarks>
        /// </summary>
        /// <typeparam name="T">The concrete transient type to register.</typeparam>
        /// <param name="factory">The factory that will be used for creating instances.</param>
        public static void RegisterTransientForScene<T>(Func<T> factory)
        {
            RegisterTransientForScene<T, T>(factory, _currentScene);
        }

        /// <summary>
        ///     Registers a scene-specific concrete transient type to return new instances of when <typeparamref name="T" /> is
        ///     resolved.
        ///     The specified resolver will be used for producing the instances.
        /// </summary>
        /// <typeparam name="T">The concrete transient type to register.</typeparam>
        /// <param name="factory">The factory that will be used for creating instances.</param>
        /// <param name="scene">The scene to register the transient type for.</param>
        public static void RegisterTransientForScene<T>(Func<T> factory, Scene scene)
        {
            RegisterTransientForScene<T, T>(factory, scene);
        }

        /// <summary>
        ///     Registers a concrete transient type to return new instances of when the abstract type
        ///     <typeparamref name="TAbstract" /> is resolved.
        ///     The specified resolver will be used for producing the instances.
        /// </summary>
        /// <typeparam name="TAbstract">The abstract type that will be mapped to <typeparamref name="TConcrete" />.</typeparam>
        /// <typeparam name="TConcrete">The concrete transient type to return when <typeparamref name="TAbstract" /> is resolved.</typeparam>
        /// <param name="factory">The factory that will be used for creating instances.</param>
        public static void RegisterTransient<TAbstract, TConcrete>(Func<TConcrete> factory) where TConcrete : TAbstract
        {
            if (ReferenceEquals(factory, null))
                throw new ArgumentNullException("factory");

            GlobalResolvers[typeof(TAbstract)] = () => factory();
        }

        /// <summary>
        ///     Registers a scene-specific concrete transient type to return new instances of when the abstract type
        ///     <typeparamref name="TAbstract" /> is resolved.
        ///     The specified resolver will be used for producing the instances.
        ///     <remarks>The resolver is registered for the active scene according to <see cref="SceneManager.GetActiveScene()" />.</remarks>
        /// </summary>
        /// <typeparam name="TAbstract">The abstract type that will be mapped to <typeparamref name="TConcrete" />.</typeparam>
        /// <typeparam name="TConcrete">The concrete transient type to return when <typeparamref name="TAbstract" /> is resolved.</typeparam>
        /// <param name="factory">The factory that will be used for creating instances.</param>
        public static void RegisterTransientForScene<TAbstract, TConcrete>(Func<TConcrete> factory)
            where TConcrete : TAbstract
        {
            RegisterTransientForScene<TAbstract, TConcrete>(factory, _currentScene);
        }

        /// <summary>
        ///     Registers a scene-specific concrete transient type to return new instances of when the abstract type
        ///     <typeparamref name="TAbstract" /> is resolved.
        ///     The specified resolver will be used for producing the instances.
        /// </summary>
        /// <typeparam name="TAbstract">The abstract type that will be mapped to <typeparamref name="TConcrete" />.</typeparam>
        /// <typeparam name="TConcrete">The concrete transient type to return when <typeparamref name="TAbstract" /> is resolved.</typeparam>
        /// <param name="factory">The factory that will be used for creating instances.</param>
        /// <param name="scene">The scene to register the transient type for.</param>
        public static void RegisterTransientForScene<TAbstract, TConcrete>(Func<TConcrete> factory, Scene scene)
            where TConcrete : TAbstract
        {
            if (ReferenceEquals(factory, null))
                throw new ArgumentNullException("factory");

            GetOrCreateSceneData(scene).Resolvers[typeof(TAbstract)] = () => factory();
        }

        /// <summary>
        ///     Locates and returns a transient object or singleton of the specified type.
        ///     Searches for a global object first, if nothing is found and <paramref name="includeActiveScene" /> is true then it
        ///     searches for a scene specific resolver.
        ///     Make sure the type is registered first.
        ///     <seealso cref="RegisterSingleton{T}(T)" />
        ///     <seealso cref="RegisterTransient{T}()" />
        ///     <seealso cref="ResolveForScene{T}()" />
        /// </summary>
        /// <typeparam name="T">The type to locate an implementation for.</typeparam>
        /// <param name="includeActiveScene">Whether to search for a scene specific resolver if a global one isn't found.</param>
        /// <returns>
        ///     The transient object or singleton that is mapped to the specified type.
        ///     If nothing is registered for <typeparamref name="T" /> the default value for the type is returned.
        /// </returns>
        public static T Resolve<T>(bool includeActiveScene = true)
        {
            Func<object> resolver;

            var type = typeof(T);

            if (!GlobalResolvers.TryGetValue(type, out resolver) && includeActiveScene)
                return ResolveForScene<T>();

            if (resolver != null)
                return (T) resolver();

            return default(T);
        }

        /// <summary>
        ///     Locates and returns a transient object or singleton of the specified type for the currently active scene.
        ///     Make sure the type is registered first.
        ///     <seealso cref="RegisterSingletonForScene{T}(T)" />
        ///     <seealso cref="RegisterTransientForScene{T}()" />
        /// </summary>
        /// <typeparam name="T">The type to locate an implementation for.</typeparam>
        /// <returns>
        ///     The transient object or singleton that is mapped to the specified type.
        ///     If nothing is registered for <typeparamref name="T" /> the default value for the type is returned.
        /// </returns>
        public static T ResolveForScene<T>()
        {
            return ResolveForScene<T>(_currentScene);
        }

        /// <summary>
        ///     Locates and returns a transient object or singleton of the specified type for the given scene.
        ///     Make sure the type is registered first.
        ///     <seealso cref="RegisterSingletonForScene{T}(T)" />
        ///     <seealso cref="RegisterTransientForScene{T}()" />
        /// </summary>
        /// <typeparam name="T">The type to locate an implementation for.</typeparam>
        /// <returns>
        ///     The transient object or singleton that is mapped to the specified type.
        ///     If nothing is registered for <typeparamref name="T" /> the default value for the type is returned.
        /// </returns>
        public static T ResolveForScene<T>(Scene scene)
        {
            SceneData sceneData;
            if (!SceneResolvers.TryGetValue(scene, out sceneData))
                return default(T);

            Func<object> resolver;
            sceneData.Resolvers.TryGetValue(typeof(T), out resolver);

            if (resolver != null)
                return (T) resolver();

            return default(T);
        }

        /// <summary>
        /// Checks whether there's registered a resolver for a specific type.
        /// </summary>
        /// <typeparam name="T">The type to check if registered.</typeparam>
        /// <param name="includeActiveScene">Whether to search for a scene specific resolver if a global one isn't found.</param>
        /// <returns>True if registered, false otherwise.</returns>
        public static bool IsRegistered<T>(bool includeActiveScene = true)
        {
            return GlobalResolvers.ContainsKey(typeof(T)) 
                || (includeActiveScene && IsRegisteredInScene<T>());
        }

        /// <summary>
        /// Checks whether there's registered a scene-specific resolver for a specific type in the currently active scene.
        /// </summary>
        /// <typeparam name="T">The type to check if registered.</typeparam>
        /// <returns>True if registered, false otherwise.</returns>
        public static bool IsRegisteredInScene<T>()
        {
            return IsRegisteredInScene<T>(_currentScene);
        }

        /// <summary>
        /// Checks whether there's registered a scene-specific resolver for a specific type in the specified scene.
        /// </summary>
        /// <typeparam name="T">The type to check if registered.</typeparam>
        /// <returns>True if registered, false otherwise.</returns>
        public static bool IsRegisteredInScene<T>(Scene scene)
        {
            SceneData sceneData;
            if (!SceneResolvers.TryGetValue(scene, out sceneData))
                return false;

            return sceneData.Resolvers.ContainsKey(typeof(T));
        }

        /// <summary>
        ///     Clears all resolvers.
        /// </summary>
        public static void Reset()
        {
            ResetScenes();
            ResetGlobal();
        }

        /// <summary>
        ///     Clears global resolvers.
        /// </summary>
        public static void ResetGlobal()
        {
            GlobalResolvers.Clear();

            if (_multiSceneGameObject != null)
            {
                Object.Destroy(_multiSceneGameObject);
                _multiSceneGameObject = null;
            }

            if (GlobalReset != null)
                GlobalReset();
        }

        /// <summary>
        ///     Clears the currently active scene's resolvers.
        /// </summary>
        public static void ResetScene()
        {
            ResetScene(_currentScene);
        }

        /// <summary>
        ///     Clears a specific scene's resolvers.
        /// </summary>
        public static void ResetScene(Scene scene)
        {
            SceneData sceneData;
            if (SceneResolvers.TryGetValue(scene, out sceneData))
            {
                if (sceneData.GameObject != null)
                    Object.Destroy(sceneData.GameObject);
                SceneResolvers.Remove(scene);

                if (SceneReset != null)
                    SceneReset(scene);
            }
        }

        /// <summary>
        ///     Clears all scene specific resolvers for all scenes.
        /// </summary>
        public static void ResetScenes()
        {
            TempSceneList.AddRange(SceneResolvers.Keys);

            for (var i = 0; i < TempSceneList.Count; i++)
            {
                var scene = TempSceneList[i];
                ResetScene(scene);
            }

            TempSceneList.Clear();
        }

        private static SceneData GetOrCreateSceneData(Scene scene)
        {
            SceneData sceneData;

            if (!SceneResolvers.TryGetValue(scene, out sceneData))
                SceneResolvers[scene] = sceneData = new SceneData();

            return sceneData;
        }

        private static bool IsComponent<T>()
        {
#if NETFX_CORE
			return typeof(T).GetTypeInfo().IsSubclassOf(typeof(MonoBehaviour));
#else
            return typeof(T).IsSubclassOf(typeof(MonoBehaviour));
#endif
        }

        private static Func<T> CreateFactory<T>() where T : new()
        {
            var isComponent = IsComponent<T>();
            return () => CreateInstance<T>(isComponent);
        }

        private static Func<T> CreateFactory<T>(Scene scene) where T : new()
        {
            var isComponent = IsComponent<T>();
            return () => CreateInstance<T>(scene, isComponent);
        }

        private static T CreateInstance<T>() where T : new()
        {
            return CreateInstance<T>(IsComponent<T>());
        }

        private static T CreateInstance<T>(bool isComponent) where T : new()
        {
            if (!isComponent)
                return new T();

            return CreateComponent<T>();
        }

        private static T CreateInstance<T>(Scene scene) where T : new()
        {
            return CreateInstance<T>(scene, IsComponent<T>());
        }

        private static T CreateInstance<T>(Scene scene, bool isComponent) where T : new()
        {
            if (!isComponent)
                return new T();

            return CreateComponent<T>(scene);
        }

        private static T CreateComponent<T>()
        {
            if (_multiSceneGameObject == null)
            {
                _multiSceneGameObject = new GameObject(MultisceneGameObjectName);
                Object.DontDestroyOnLoad(_multiSceneGameObject);
            }

            return (T) (object) _multiSceneGameObject.AddComponent(typeof(T));
        }

        private static T CreateComponent<T>(Scene scene)
        {
            var sceneData = SceneResolvers[scene];

            if (sceneData.GameObject == null)
            {
                sceneData.GameObject = new GameObject("ServiceLocator - Scene: " + scene.name);
                SceneManager.MoveGameObjectToScene(sceneData.GameObject, scene);
            }

            return (T) (object) sceneData.GameObject.AddComponent(typeof(T));
        }

        private class SceneData
        {
            public readonly Dictionary<Type, Func<object>> Resolvers = new Dictionary<Type, Func<object>>();
            public GameObject GameObject;
        }
    }
}