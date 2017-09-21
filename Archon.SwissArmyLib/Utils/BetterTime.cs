using Archon.SwissArmyLib.Utils.Editor;
using JetBrains.Annotations;
using UnityEngine;
using UnityTime = UnityEngine.Time;

namespace Archon.SwissArmyLib.Utils
{
    [ExecutionOrder(Order = int.MinValue, Forced = true)]
    [AddComponentMenu("")]
    internal class BetterTimeUpdater : MonoBehaviour
    {
        [UsedImplicitly]
        private void Awake()
        {
            BetterTime.Update();
        }

        [UsedImplicitly]
        private void OnEnable()
        {
            var instance = ServiceLocator.Resolve<BetterTimeUpdater>();

            if (instance == null)
                ServiceLocator.RegisterSingleton(this);
            else if (instance != this)
                Destroy(this);
        }

        [UsedImplicitly]
        private void Start()
        {
            BetterTime.Update();
        }

        [UsedImplicitly]
        private void Update()
        {
            BetterTime.Update();
        }

        [UsedImplicitly]
        private void FixedUpdate()
        {
            BetterTime.Update();
        }
    }

    /// <summary>
    /// A simple wrapper for Unity's <see cref="UnityEngine.Time"/> that caches values to avoid the marshal overhead of each call.
    /// 
    /// The performance benefit is very small, but completely free.
    /// 
    /// Only readonly <see cref="UnityEngine.Time"/> properties (except for <see cref="FixedDeltaTime"/>) are cached, but everything is wrapped anyway so you don't have to use multiple time classes.
    /// 
    /// Since this is just a wrapper just refer to Unity's documentation about what each property does.
    /// </summary>
    public static class BetterTime
    {
        private static float _fixedDeltaTime;

        /// <summary>
        /// Gets or sets the scalar that is applied to <see cref="Time"/>, <see cref="DeltaTime"/> etc.
        /// </summary>
        public static float TimeScale
        {
            get { return UnityTime.timeScale; }
            set { UnityTime.timeScale = value; }
        }

        /// <summary>
        /// Gets the total number of frames that have passed.
        /// </summary>
        public static int FrameCount { get; private set; }

        /// <summary>
        /// Gets the (scaled) time in seconds at the beginning of this frame.
        /// </summary>
        public static float Time { get; private set; }

        /// <summary>
        /// Gets the difference in seconds since the last frame. (Scaled according to <see cref="TimeScale"/>)
        /// </summary>
        public static float DeltaTime { get; private set; }

        /// <summary>
        /// Gets a smoothed out time difference in seconds since the last frame. (Scaled according to <see cref="TimeScale"/>)
        /// </summary>
        public static float SmoothDeltaTime { get; private set; }

        /// <summary>
        /// Gets the unscaled time difference in seconds since the last frame.
        /// </summary>
        public static float UnscaledDeltaTime { get; private set; }

        /// <summary>
        /// Gets the unscaled time in seconds at the beginning of this frame.
        /// </summary>
        public static float UnscaledTime { get; private set; }

        /// <summary>
        /// Gets or sets the fixed time in seconds between FixedUpdate.
        /// </summary>
        public static float FixedDeltaTime
        {
            get { return _fixedDeltaTime; }
            set
            {
                _fixedDeltaTime = value;
                UnityTime.fixedDeltaTime = value;
            }
        }

        /// <summary>
        /// Gets the time that the latest FixedUpdate started. This is scaled according to <see cref="TimeScale"/>.
        /// </summary>
        public static float FixedTime { get; private set; }

        /// <summary>
        /// Gets the unscaled time difference since the previous FixedUpdate.
        /// </summary>
        public static float FixedUnscaledDeltaTime { get; private set; }

        /// <summary>
        /// Gets the unscaled time that the latest FixedUpdate started. 
        /// </summary>
        public static float FixedUnscaledTime { get; private set; }

        /// <summary>
        /// Gets whether we're inside a FixedUpdate at the moment.
        /// </summary>
        public static bool InFixedTimeStep { get { return UnityTime.inFixedTimeStep; } }

        /// <summary>
        /// Gets the real time in seconds since the game started.
        /// </summary>
        public static float RealTimeSinceStartup { get { return UnityTime.realtimeSinceStartup; }}

        /// <summary>
        /// Gets the time since the last level was loaded.
        /// </summary>
        public static float TimeSinceLevelLoad { get; private set; }

        /// <summary>
        /// Gets or sets the maximum time in seconds that a fixed timestep frame can take. 
        /// </summary>
        public static float MaximumDeltaTime
        {
            get { return UnityTime.maximumDeltaTime; }
            set { UnityTime.maximumDeltaTime = value; }
        }

        /// <summary>
        /// Gets or sets the maximum time in seconds that a frame can spend on updating particles.
        /// </summary>
        public static float MaximumParticleDeltaTime
        {
            get { return UnityTime.maximumParticleDeltaTime; }
            set { UnityTime.maximumParticleDeltaTime = value; }
        }

        /// <summary>
        /// Slows game playback time to allow screenshots to be saved between frames.
        /// </summary>
        public static int CaptureFramerate
        {
            get { return UnityTime.captureFramerate; }
            set { UnityTime.captureFramerate = value; }
        }

        static BetterTime()
        {
            if (!ServiceLocator.IsRegistered<BetterTimeUpdater>())
                ServiceLocator.RegisterSingleton<BetterTimeUpdater>();

            ServiceLocator.GlobalReset += () => ServiceLocator.RegisterSingleton<BetterTimeUpdater>();
        }

        internal static void Update()
        {
            FrameCount = UnityTime.frameCount;

            Time = UnityTime.time;
            TimeSinceLevelLoad = UnityTime.timeSinceLevelLoad;
            DeltaTime = UnityTime.deltaTime;
            SmoothDeltaTime = UnityTime.smoothDeltaTime;

            UnscaledTime = UnityTime.unscaledTime;
            UnscaledDeltaTime = UnityTime.unscaledDeltaTime;

            FixedTime = UnityTime.fixedTime;
            _fixedDeltaTime = UnityTime.fixedDeltaTime;
            FixedUnscaledTime = UnityTime.fixedUnscaledTime;
            FixedUnscaledDeltaTime = UnityTime.fixedUnscaledDeltaTime;
        }
    }
}
