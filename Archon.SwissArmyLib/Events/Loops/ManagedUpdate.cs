using System;
using System.Collections.Generic;
using Archon.SwissArmyLib.Collections;
using Archon.SwissArmyLib.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Archon.SwissArmyLib.Events.Loops
{
    /// <summary>
    /// A relay for Unity update events.
    /// Here's why you might want to use this:
    /// https://blogs.unity3d.com/2015/12/23/1k-update-calls/
    /// In short; avoid overhead of Native C++ --> Managed C# calls.
    /// 
    /// Also useful for non-MonoBehaviours that needs to be part of the update loop as well.
    /// 
    /// Built-in events your can subscribe to:
    /// <list type="bullet">
    ///     <item><description><see cref="OnUpdate"/></description></item>
    ///     <item><description><see cref="OnLateUpdate"/></description></item>
    ///     <item><description><see cref="OnFixedUpdate"/></description></item>
    /// </list>
    /// 
    /// You can also create your own custom update loops (eg. to run every nth second) using 
    /// <see cref="AddCustomUpdateLoop"/> and <see cref="AddListener(int,Action,int)"/>.
    /// 
    /// <seealso cref="ManagedUpdateBehaviour"/>
    /// <seealso cref="TimeIntervalUpdateLoop"/>
    /// <seealso cref="FrameIntervalUpdateLoop"/>
    /// </summary>
    [AddComponentMenu("")]
    public static class ManagedUpdate
    {
        /// <summary>
        /// Event handler that is called every update.
        /// </summary>
        public static readonly Event OnUpdate = new Event(EventIds.Update);

        /// <summary>
        /// Event handler that is called every update but after the regular Update.
        /// <seealso cref="OnUpdate"/>
        /// </summary>
        public static readonly Event OnLateUpdate = new Event(EventIds.LateUpdate);

        /// <summary>
        /// Event handler that is called every fixed update.
        /// </summary>
        public static readonly Event OnFixedUpdate = new Event(EventIds.FixedUpdate);

        private static Dictionary<int, ICustomUpdateLoop> _idToUpdateLoop;
        private static Dictionary<int, UpdateLoop> _idToUnityUpdateLoop;
        private static Dictionary<int, PrioritizedList<ICustomUpdateLoop>> _customUpdateLoops;

        /// <summary>
        /// Gets the difference in seconds since the previous update of the currently running type. (Scaled according to <see cref="Time.timeScale"/>)
        /// </summary>
        public static float DeltaTime { get; private set; }

        /// <summary>
        /// Gets the unscaled difference in seconds since the previous update of the currently running type.
        /// </summary>
        public static float UnscaledDeltaTime { get; private set; }

        /// <summary>
        /// Relayed event ids.
        /// </summary>
        public static class EventIds
        {
#pragma warning disable 1591
            public const int
                // ReSharper disable MemberHidesStaticFromOuterClass
                Update = BuiltinEventIds.Update,
                LateUpdate = BuiltinEventIds.LateUpdate,
                FixedUpdate = BuiltinEventIds.FixedUpdate;
                // ReSharper restore MemberHidesStaticFromOuterClass
#pragma warning restore 1591
        }

        static ManagedUpdate()
        {
            if (!ServiceLocator.IsRegistered<ManagedUpdateTicker>())
                ServiceLocator.RegisterSingleton<ManagedUpdateTicker>();

            ServiceLocator.GlobalReset += () => ServiceLocator.RegisterSingleton<ManagedUpdateTicker>();
        }

        /// <summary>
        /// Adds a custom update loop.
        /// </summary>
        /// <param name="updateLoop">The custom update loop implementation.</param>
        /// <param name="parentLoop">Which Unity update loop should this be run under?</param>
        /// <param name="priority">A priority that decides whether this update loop runs before or after other custom update loops.</param>
        public static void AddCustomUpdateLoop(ICustomUpdateLoop updateLoop, UpdateLoop parentLoop = UpdateLoop.Update, int priority = 0)
        {
            if (_idToUpdateLoop == null)
            {
                _idToUpdateLoop = new Dictionary<int, ICustomUpdateLoop>(4);
                _idToUnityUpdateLoop = new Dictionary<int, UpdateLoop>(4);
                _customUpdateLoops = new Dictionary<int, PrioritizedList<ICustomUpdateLoop>>(3);
            }

            PrioritizedList<ICustomUpdateLoop> customLoops;
            if (!_customUpdateLoops.TryGetValue((int)parentLoop, out customLoops))
                _customUpdateLoops[(int)parentLoop] = customLoops = new PrioritizedList<ICustomUpdateLoop>(4);

            var id = updateLoop.Event.Id;

            if (_idToUpdateLoop.ContainsKey(id))
            {
                Debug.LogErrorFormat("An update loop with ID '{0}' is already registered.", id);
                return;
            }

            _idToUpdateLoop[id] = updateLoop;
            _idToUnityUpdateLoop[id] = parentLoop;
            customLoops.Add(updateLoop, priority);
        }

        /// <summary>
        /// Removes a custom update loop.
        /// </summary>
        /// <param name="updateLoop">The update loop to remove.</param>
        public static void RemoveCustomUpdateLoop(ICustomUpdateLoop updateLoop)
        {
            if (_customUpdateLoops == null)
                return;

            var id = updateLoop.Event.Id;

            UpdateLoop unityUpdateLoop;
            if (!_idToUnityUpdateLoop.TryGetValue(id, out unityUpdateLoop))
                return;

            PrioritizedList<ICustomUpdateLoop> customLoops;
            if (!_customUpdateLoops.TryGetValue((int)unityUpdateLoop, out customLoops))
                return;


            customLoops.Remove(updateLoop);
            _idToUpdateLoop.Remove(id);
            _idToUnityUpdateLoop.Remove(id);
        }

        /// <summary>
        /// Removes a custom update loop with the given event id.
        /// </summary>
        /// <param name="eventId">The event id that the custom update loop uses.</param>
        public static void RemoveCustomUpdateLoop(int eventId)
        {
            var updateLoop = GetCustomUpdateLoop(eventId);

            if (updateLoop != null)
                RemoveCustomUpdateLoop(updateLoop);
        }

        /// <summary>
        /// Gets the custom update loop implementation for the given event id.
        /// </summary>
        /// <typeparam name="T">The type of the update loop.</typeparam>
        /// <param name="eventId">The event id that the update loop uses.</param>
        /// <returns>The custom update loop or null if not found or wrong type.</returns>
        public static T GetCustomUpdateLoop<T>(int eventId) where T : class, ICustomUpdateLoop
        {
            return GetCustomUpdateLoop(eventId) as T;
        }

        /// <summary>
        /// Gets the custom update loop implementation for the given event id.
        /// </summary>
        /// <param name="eventId">The event id for the update loop.</param>
        /// <returns>The custom update loop or null if not found.</returns>
        public static ICustomUpdateLoop GetCustomUpdateLoop(int eventId)
        {
            if (_idToUpdateLoop == null)
                return null;

            ICustomUpdateLoop updateLoop;
            _idToUpdateLoop.TryGetValue(eventId, out updateLoop);
            return updateLoop;
        }

        /// <summary>
        /// Adds a listener for an update loop.
        /// 
        /// If it's not a custom update loop, you can instead subscribe directly using 
        /// <see cref="OnUpdate"/>, <see cref="OnLateUpdate"/> or <see cref="OnFixedUpdate"/>.
        /// </summary>
        /// <param name="eventId">The event id of the update loop to subscribe to.</param>
        /// <param name="listener">The listener to add to the update loop.</param>
        /// <param name="priority">The priority of the listener, controlling whether the listener 
        /// is called before (lower) or later (higher) than other listeners.</param>
        public static void AddListener(int eventId, IEventListener listener, int priority = 0)
        {
            var e = GetEventForId(eventId);
            if (e != null)
                e.AddListener(listener, priority);
        }

        /// <summary>
        /// Adds a listener for an update loop.
        /// 
        /// If it's not a custom update loop, you can instead subscribe directly using 
        /// <see cref="OnUpdate"/>, <see cref="OnLateUpdate"/> or <see cref="OnFixedUpdate"/>.
        /// </summary>
        /// <param name="eventId">The event id of the update loop to subscribe to.</param>
        /// <param name="listener">The listener to add to the update loop.</param>
        /// <param name="priority">The priority of the listener, controlling whether the listener 
        /// is called before (lower) or later (higher) than other listeners.</param>
        public static void AddListener(int eventId, Action listener, int priority = 0)
        {
            var e = GetEventForId(eventId);
            if (e != null)
                e.AddListener(listener, priority);
        }

        /// <summary>
        /// Removes a listener for an update loop.
        /// 
        /// If it's not a custom update loop, you can instead unsubscribe directly using 
        /// <see cref="OnUpdate"/>, <see cref="OnLateUpdate"/> or <see cref="OnFixedUpdate"/>.
        /// </summary>
        /// <param name="eventId">The event id of the update loop to unsubscribe from.</param>
        /// <param name="listener">The listener to remove from the update loop.</param>
        public static void RemoveListener(int eventId, IEventListener listener)
        {
            var e = GetEventForId(eventId);
            if (e != null)
                e.RemoveListener(listener);
        }

        /// <summary>
        /// Removes a listener for an update loop.
        /// 
        /// If it's not a custom update loop, you can instead unsubscribe directly using 
        /// <see cref="OnUpdate"/>, <see cref="OnLateUpdate"/> or <see cref="OnFixedUpdate"/>.
        /// </summary>
        /// <param name="eventId">The event id of the update loop to unsubscribe from.</param>
        /// <param name="listener">The listener to remove from the update loop.</param>
        public static void RemoveListener(int eventId, Action listener)
        {
            var e = GetEventForId(eventId);
            if (e != null)
                e.RemoveListener(listener);
        }

        internal static void Update()
        {
            DeltaTime = BetterTime.DeltaTime;
            UnscaledDeltaTime = BetterTime.UnscaledDeltaTime;
            OnUpdate.Invoke();

            ProcessCustomLoops(UpdateLoop.Update);
        }

        internal static void LateUpdate()
        {
            DeltaTime = BetterTime.DeltaTime;
            UnscaledDeltaTime = BetterTime.UnscaledDeltaTime;
            OnLateUpdate.Invoke();

            ProcessCustomLoops(UpdateLoop.LateUpdate);
        }

        internal static void FixedUpdate()
        {
            DeltaTime = BetterTime.FixedDeltaTime;
            UnscaledDeltaTime = BetterTime.FixedUnscaledDeltaTime;
            OnFixedUpdate.Invoke();

            ProcessCustomLoops(UpdateLoop.FixedUpdate);
        }

        internal static UpdateLoop GetParentLoopForid(int eventId)
        {
            switch (eventId)
            {
                case EventIds.Update:
                    return UpdateLoop.Update;
                case EventIds.LateUpdate:
                    return UpdateLoop.LateUpdate;
                case EventIds.FixedUpdate:
                    return UpdateLoop.FixedUpdate;
                default:
                    UpdateLoop updateLoop;
                    _idToUnityUpdateLoop.TryGetValue(eventId, out updateLoop);
                    return updateLoop;
            }
        }

        private static void ProcessCustomLoops(UpdateLoop unityLoop)
        {
            if (_customUpdateLoops == null) return;

            PrioritizedList<ICustomUpdateLoop> customLoops;
            if (!_customUpdateLoops.TryGetValue((int)unityLoop, out customLoops))
                return;

            for (var i = 0; i < customLoops.Count; i++)
            {
                var updateLoop = customLoops[i].Item;
                if (updateLoop.IsTimeToRun)
                {
                    DeltaTime = updateLoop.DeltaTime;
                    UnscaledDeltaTime = updateLoop.UnscaledDeltaTime;
                    updateLoop.Invoke();
                }
            }
        }

        private static Event GetEventForId(int eventId)
        {
            switch (eventId)
            {
                case EventIds.Update:
                    return OnUpdate;
                case EventIds.LateUpdate:
                    return OnLateUpdate;
                case EventIds.FixedUpdate:
                    return OnFixedUpdate;
                default:
                    var updateLoop = GetCustomUpdateLoop(eventId);
                    if (updateLoop != null)
                        return updateLoop.Event;
                    return null;
            }
        }
    }

    [AddComponentMenu("")]
    internal sealed class ManagedUpdateTicker : MonoBehaviour
    {
        [UsedImplicitly]
        private void OnEnable()
        {
            var instance = ServiceLocator.Resolve<ManagedUpdateTicker>();

            if (instance == null)
                ServiceLocator.RegisterSingleton(this);
            else if (instance != this)
                Destroy(this);
        }

        [UsedImplicitly]
        private void Update()
        {
            ManagedUpdate.Update();
        }

        [UsedImplicitly]
        private void LateUpdate()
        {
            ManagedUpdate.LateUpdate();
        }

        [UsedImplicitly]
        private void FixedUpdate()
        {
            ManagedUpdate.FixedUpdate();
        }
    }
}
