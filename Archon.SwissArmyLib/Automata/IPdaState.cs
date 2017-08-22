namespace Archon.SwissArmyLib.Automata
{
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