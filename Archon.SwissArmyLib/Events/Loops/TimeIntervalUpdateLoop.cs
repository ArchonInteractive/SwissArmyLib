using Archon.SwissArmyLib.Utils;

namespace Archon.SwissArmyLib.Events.Loops
{
    /// <summary>
    /// A basic custom update loop that runs every nth second either in scaled or unscaled time.
    /// 
    /// <seealso cref="FrameIntervalUpdateLoop"/>
    /// </summary>
    public class TimeIntervalUpdateLoop : CustomUpdateLoopBase
    {
        private float _nextUpdateTime;
        private float _interval;

        /// <inheritdoc />
        public override bool IsTimeToRun
        {
            get
            {
                var time = UsingScaledTime ? BetterTime.Time : BetterTime.UnscaledTime;
                return time >= _nextUpdateTime;
            }
        }

        /// <summary>
        /// Gets or sets the amount of seconds between each time this update loop runs.
        /// </summary>
        public float Interval
        {
            get { return _interval; }
            set
            {
                _interval = value;
                var previousTime = UsingScaledTime ? PreviousRunTimeScaled : PreviousRunTimeUnscaled;
                _nextUpdateTime = previousTime + value;
            }
        }

        /// <summary>
        /// Gets whether this interval update loop uses scaled or unscaled time for its interval.
        /// </summary>
        public bool UsingScaledTime { get; private set; }

        /// <summary>
        /// Creates a new TimeIntervalUpdateLoop.
        /// </summary>
        /// <param name="eventId">The event id that the update loop should use.</param>
        /// <param name="interval">The amount of seconds between each time this update loop should run.</param>
        /// <param name="usingScaledTime">Whether the interval should use scaled or unscaled time.</param>
        public TimeIntervalUpdateLoop(int eventId, float interval, bool usingScaledTime = true)
        {
            Event = new Event(eventId);
            Interval = interval;
            UsingScaledTime = usingScaledTime;
        }

        /// <inheritdoc />
        public override void Invoke()
        {
            base.Invoke();

            var time = UsingScaledTime ? BetterTime.Time : BetterTime.UnscaledTime;
            _nextUpdateTime = time + Interval;
        }
    }
}