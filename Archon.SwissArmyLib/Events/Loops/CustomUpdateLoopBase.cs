using Archon.SwissArmyLib.Utils;

namespace Archon.SwissArmyLib.Events.Loops
{
    /// <summary>
    /// An abstract class for custom update loops that implement basic 
    /// functionality to track invokation times and deltatimes.
    /// </summary>
    public abstract class CustomUpdateLoopBase : ICustomUpdateLoop
    {
        /// <summary>
        /// Gets or sets in scaled time when this update loop last ran.
        /// </summary>
        protected float PreviousRunTimeScaled;

        /// <summary>
        /// Gets or sets in unscaled time when this update loop last ran.
        /// </summary>
        protected float PreviousRunTimeUnscaled;

        /// <inheritdoc />
        public Event Event { get; protected set; }

        /// <inheritdoc />
        public abstract bool IsTimeToRun { get; }

        /// <inheritdoc />
        public float DeltaTime
        {
            get { return BetterTime.Time - PreviousRunTimeScaled; }
        }

        /// <inheritdoc />
        public float UnscaledDeltaTime
        {
            get { return BetterTime.UnscaledTime - PreviousRunTimeUnscaled; }
        }

        /// <inheritdoc />
        public virtual void Invoke()
        {
            Event.Invoke();

            PreviousRunTimeScaled = BetterTime.Time;
            PreviousRunTimeUnscaled = BetterTime.UnscaledTime;
        }
    }
}