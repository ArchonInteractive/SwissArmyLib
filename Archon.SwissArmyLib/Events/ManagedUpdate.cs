using Archon.SwissArmyLib.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Archon.SwissArmyLib.Events
{
    /// <summary>
    /// A relay for Unity update events.
    /// Here's why you might want to use this:
    /// https://blogs.unity3d.com/2015/12/23/1k-update-calls/
    /// In short; avoid overhead of Native C++ --> Managed C# calls.
    /// 
    /// Also useful for non-MonoBehaviours that needs to be part of the update loop as well.
    /// 
    /// Events your can subscribe to:
    /// <list type="bullet">
    ///     <item><description><see cref="OnUpdate"/></description></item>
    ///     <item><description><see cref="OnLateUpdate"/></description></item>
    ///     <item><description><see cref="OnFixedUpdate"/></description></item>
    ///     <item><description><see cref="OnFrameIntervalUpdate"/></description></item>
    ///     <item><description><see cref="OnTimeIntervalUpdate"/></description></item>
    /// </list>
    /// 
    /// <seealso cref="ManagedUpdateBehaviour"/>
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

        /// <summary>
        /// Event handler that is called every nth update.
        /// </summary>
        public static readonly Event OnFrameIntervalUpdate = new Event(EventIds.FrameIntervalUpdate);

        /// <summary>
        /// Event handler that is called every nth unscaled second.
        /// </summary>
        public static readonly Event OnTimeIntervalUpdate = new Event(EventIds.TimeIntervalUpdate);

        /// <summary>
        /// Gets or sets the interval (in frames) at which <see cref="OnFrameIntervalUpdate"/> is invoked.
        /// </summary>
        public static int FrameInterval
        {
            get { return _frameInterval; }
            set
            {
                _frameInterval = value;
                _nextFrameUpdate = BetterTime.FrameCount + value;
            }
        }

        /// <summary>
        /// Gets or sets the interval (in unscaled seconds) at which <see cref="OnTimeIntervalUpdate"/> is invoked.
        /// </summary>
        public static float TimeInterval
        {
            get { return _timeInterval; }
            set
            {
                _timeInterval = value;
                _nextTimeUpdate = BetterTime.UnscaledTime + value;
            }
        }

        /// <summary>
        /// Gets the difference in seconds since the previous update of the currently running type. (Scaled according to <see cref="Time.timeScale"/>)
        /// </summary>
        public static float DeltaTime { get; private set; }

        /// <summary>
        /// Gets the unscaled difference in seconds since the previous update of the currently running type.
        /// </summary>
        public static float UnscaledDeltaTime { get; private set; }

        private static float _nextTimeUpdate = float.MinValue;
        private static int _nextFrameUpdate = int.MinValue;

        private static float _lastFrameUpdateTime;
        private static float _lastFrameUpdateUnscaledTime;
        private static float _lastTimeUpdateTime;
        private static float _lastTimeUpdateUnscaledTime;

        private static int _frameInterval = 10;
        private static float _timeInterval = 1/7f;

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
                FixedUpdate = BuiltinEventIds.FixedUpdate,
                // ReSharper restore MemberHidesStaticFromOuterClass
                FrameIntervalUpdate = BuiltinEventIds.FrameIntervalUpdate,
                TimeIntervalUpdate = BuiltinEventIds.TimeIntervalUpdate;
#pragma warning restore 1591
        }

        static ManagedUpdate()
        {
            if (!ServiceLocator.IsRegistered<ManagedUpdateTicker>())
                ServiceLocator.RegisterSingleton<ManagedUpdateTicker>();

            ServiceLocator.GlobalReset += () => ServiceLocator.RegisterSingleton<ManagedUpdateTicker>();
        }

        internal static void Update()
        {
            DeltaTime = BetterTime.DeltaTime;
            UnscaledDeltaTime = BetterTime.UnscaledDeltaTime;
            OnUpdate.Invoke();

            var time = BetterTime.Time;
            var unscaledTime = BetterTime.UnscaledTime;
            var frameCount = BetterTime.FrameCount;

            if (frameCount >= _nextFrameUpdate)
            {
                DeltaTime = time - _lastFrameUpdateTime;
                UnscaledDeltaTime = unscaledTime - _lastFrameUpdateUnscaledTime;

                OnFrameIntervalUpdate.Invoke();

                _lastFrameUpdateTime = time;
                _lastFrameUpdateUnscaledTime = unscaledTime;
                _nextFrameUpdate = frameCount + _frameInterval;
            }

            if (unscaledTime >= _nextTimeUpdate)
            {
                DeltaTime = time - _lastTimeUpdateTime;
                UnscaledDeltaTime = unscaledTime - _lastTimeUpdateUnscaledTime;

                OnTimeIntervalUpdate.Invoke();

                _lastTimeUpdateTime = time;
                _lastTimeUpdateUnscaledTime = unscaledTime;
                _nextTimeUpdate = unscaledTime + TimeInterval;
            }
        }

        internal static void LateUpdate()
        {
            DeltaTime = BetterTime.DeltaTime;
            UnscaledDeltaTime = BetterTime.UnscaledDeltaTime;
            OnLateUpdate.Invoke();
        }

        internal static void FixedUpdate()
        {
            DeltaTime = BetterTime.FixedDeltaTime;
            UnscaledDeltaTime = BetterTime.FixedUnscaledDeltaTime;
            OnFixedUpdate.Invoke();
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
