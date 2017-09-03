namespace Archon.SwissArmyLib.Events
{
    /// <summary>
    /// Defines a method to be used for event callbacks.
    /// </summary>
    public interface IEventListener
    {
        /// <summary>
        /// Called when an event is invoked.
        /// </summary>
        /// <param name="eventId">The id of the event.</param>
        void OnEvent(int eventId);
    }

    /// <summary>
    /// Defines a method to be used for event callbacks with a parameter of type <typeparamref name="TArgs"/>.
    /// </summary>
    /// <typeparam name="TArgs"></typeparam>
    public interface IEventListener<in TArgs>
    {
        /// <summary>
        /// Called when an event is invoked.
        /// </summary>
        /// <param name="eventId">The id of the event.</param>
        /// <param name="args">The args for the event.</param>
        void OnEvent(int eventId, TArgs args);
    }
}