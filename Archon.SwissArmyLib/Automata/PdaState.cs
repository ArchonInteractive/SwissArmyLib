namespace Archon.SwissArmyLib.Automata
{
    public class PdaState<T> : State<PushdownAutomaton<T>, T>, IPdaState<T>
    {
        /// <summary>
        /// Called when a state is pushed ontop of this state.
        /// </summary>
        public virtual void Pause() { }

        /// <summary>
        /// Called when the state above us is popped.
        /// </summary>
        public virtual void Resume() { }
    }
}