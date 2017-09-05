namespace Archon.SwissArmyLib.Automata
{
    /// <summary>
    /// Represents a state to be used in a <see cref="PushdownAutomaton{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the context.</typeparam>
    public interface IPdaState<T> : IState<PushdownAutomaton<T>, T>
    {
        /// <summary>
        /// Called when a state is pushed ontop of this state.
        /// </summary>
        void Pause();

        /// <summary>
        /// Called when the state above us is popped.
        /// </summary>
        void Resume();
    }
}