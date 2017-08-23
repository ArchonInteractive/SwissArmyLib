using System;
using System.Collections.Generic;

namespace Archon.SwissArmyLib.Utils
{
    /// <summary>
    /// A simple implementation of the service locator pattern.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, Func<object>> Resolvers = new Dictionary<Type, Func<object>>();

        /// <summary>
        /// Registers a concrete singleton of the given type.
        /// 
        /// The instance will be lazy loaded when resolved (if <paramref name="lazyload"/> is true).
        /// </summary>
        /// <typeparam name="T">The type of the singleton.</typeparam>
        /// <param name="lazyload">Should the singleton be initialized on first resolve instead of instantly.</param>
        public static void RegisterSingleton<T>(bool lazyload = true) where T : new()
        {
            RegisterSingleton<T, T>(lazyload);
        }

        /// <summary>
        /// Registers a specific instance to be a singleton for its concrete type.
        /// </summary>
        /// <typeparam name="T">The type of the singleton.</typeparam>
        public static void RegisterSingleton<T>(T instance)
        {
            Resolvers[typeof(T)] = () => instance;
        }

        /// <summary>
        /// Registers a concrete singleton of the type <typeparamref name="TConcrete"/> for the abstract type <typeparamref name="TAbstract"/>.
        /// 
        /// The instance will be lazy loaded when resolved (if <paramref name="lazyload"/> is true).
        /// </summary>
        /// <typeparam name="TAbstract">The abstract type that will be mapped to <typeparamref name="TConcrete"/>.</typeparam>
        /// <typeparam name="TConcrete">The concrete singleton implementation.</typeparam>
        /// <param name="lazyload">Should the singleton be initialized on first resolve instead of instantly.</param>
        public static void RegisterSingleton<TAbstract, TConcrete>(bool lazyload = true) where TConcrete : TAbstract, new()
        {
            Func<object> resolver;

            if (lazyload)
            {
                var lazyInstance = new Lazy<TConcrete>(() => new TConcrete());
                resolver = () => lazyInstance.Value;
            }
            else
            {
                var instance = new TConcrete();
                resolver = () => instance;
            }

            Resolvers[typeof(TAbstract)] = resolver;
        }

        /// <summary>
        /// Registers a concrete transient type. 
        /// A new instance of the given type will be returned each time it is resolved.
        /// </summary>
        /// <typeparam name="T">The concrete transient type to register.</typeparam>
        public static void RegisterTransient<T>() where T : new()
        {
            RegisterTransient<T, T>();
        }

        /// <summary>
        /// Registers a concrete transient type to return new instances of when the abstract type <typeparamref name="TAbstract"/> is resolved.
        /// </summary>
        /// <typeparam name="TAbstract">The abstract type that will be mapped to <typeparamref name="TConcrete"/>.</typeparam>
        /// <typeparam name="TConcrete">The concrete transient type to return when <typeparamref name="TAbstract"/> is resolved.</typeparam>
        public static void RegisterTransient<TAbstract, TConcrete>() where TConcrete : TAbstract, new()
        {
            RegisterTransient<TAbstract, TConcrete>(() => new TConcrete());
        }

        /// <summary>
        /// Registers a concrete transient type to return new instances of when <typeparamref name="T"/> is resolved.
        /// The specified resolver will be used for producing the instances.
        /// </summary>
        /// <typeparam name="T">The concrete transient type to register.</typeparam>
        /// <param name="factory">The factory that will be used for creating instances.</param>
        public static void RegisterTransient<T>(Func<T> factory)
        {
            RegisterTransient<T, T>(factory);
        }

        /// <summary>
        /// Registers a concrete transient type to return new instances of when the abstract type <typeparamref name="TAbstract"/> is resolved.
        /// The specified resolver will be used for producing the instances.
        /// </summary>
        /// <typeparam name="TAbstract">The abstract type that will be mapped to <typeparamref name="TConcrete"/>.</typeparam>
        /// <typeparam name="TConcrete">The concrete transient type to return when <typeparamref name="TAbstract"/> is resolved.</typeparam>
        /// <param name="factory">The factory that will be used for creating instances.</param>
        public static void RegisterTransient<TAbstract, TConcrete>(Func<TConcrete> factory) where TConcrete : TAbstract
        {
            Resolvers[typeof(TAbstract)] = () => factory();
        }

        /// <summary>
        /// Locates and returns a transient object or singleton of the specified type.
        /// 
        /// Make sure the type is registered first.
        /// 
        /// <seealso cref="RegisterSingleton{T}(T)"/>
        /// <seealso cref="RegisterTransient{T}()"/>
        /// </summary>
        /// <typeparam name="T">The type to locate an implementation for.</typeparam>
        /// <returns>
        /// The transient object or singleton that is mapped to the specified type. 
        /// If nothing is registered for <typeparamref name="T"/> the default value for the type is returned.
        /// </returns>
        public static T Resolve<T>()
        {
            Func<object> resolver;
            Resolvers.TryGetValue(typeof(T), out resolver);

            if (resolver != null)
                return (T) resolver();

            return default(T);
        }

        /// <summary>
        /// Clears all resolvers.
        /// </summary>
        public static void Reset()
        {
            Resolvers.Clear();
        }
    }
}
