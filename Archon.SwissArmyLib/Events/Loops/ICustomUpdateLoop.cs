namespace Archon.SwissArmyLib.Events.Loops
{
    /// <summary>
    /// Represents an implementation of a custom update loop.
    /// 
    /// You will probably be better of subclassing <see cref="CustomUpdateLoopBase"/> for
    /// a simpler start, but it's here if you need it.
    /// </summary>
    public interface ICustomUpdateLoop
    {
        /// <summary>
        /// Gets the event associated with this update loop.
        /// </summary>
        Event Event { get; }

        /// <summary>
        /// Gets whether it's time for this update loop to run again.
        /// </summary>
        bool IsTimeToRun { get; }

        /// <summary>
        /// Gets the scaled time since this update loop last ran.
        /// </summary>
        float DeltaTime { get; }

        /// <summary>
        /// Gets the unscaled time since this update loop last ran.
        /// </summary>
        float UnscaledDeltaTime { get; }

        /// <summary>
        /// Runs this update loop's event.
        /// </summary>
        void Invoke();
    }
}