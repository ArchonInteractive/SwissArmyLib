using Archon.SwissArmyLib.Utils;

namespace Archon.SwissArmyLib.Events.Loops
{
    /// <summary>
    /// A basic custom update loop that runs every nth frame.
    /// 
    /// <seealso cref="TimeIntervalUpdateLoop"/>
    /// </summary>
    public class FrameIntervalUpdateLoop : CustomUpdateLoopBase
    {
        private int _previousUpdateFrame;
        private int _nextUpdateFrame;
        private int _interval;

        /// <inheritdoc />
        public override bool IsTimeToRun
        {
            get
            {
                return BetterTime.FrameCount >= _nextUpdateFrame;
            }
        }

        /// <summary>
        /// Gets or sets the frame interval that this update loop should run.
        /// </summary>
        public int Interval
        {
            get { return _interval; }
            set
            {
                _interval = value;
                _nextUpdateFrame = _previousUpdateFrame + value;
            }
        }

        /// <summary>
        /// Creates a new FrameIntervalUpdateLoop.
        /// </summary>
        /// <param name="eventId">The event id that this update loop should use.</param>
        /// <param name="interval">The amount of frames between each call of this update loop.</param>
        public FrameIntervalUpdateLoop(int eventId, int interval)
        {
            Event = new Event(eventId);
            Interval = interval;
        }

        /// <inheritdoc />
        public override void Invoke()
        {
            base.Invoke();

            _previousUpdateFrame = BetterTime.FrameCount;
            _nextUpdateFrame = _previousUpdateFrame + Interval;
        }
    }
}