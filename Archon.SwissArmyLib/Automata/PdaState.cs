namespace Archon.SwissArmyLib.Automata
{
    /// <summary>
    /// A simple abstract class that implements <see cref="IPdaState{T}"/> and can be used in a <see cref="PushdownAutomaton{T}"/>
    /// 
    /// You're not required to use this, but it's easier.
    /// </summary>
    /// <typeparam name="T">The type of the context.</typeparam>
    public class PdaState<T> : BaseState<PushdownAutomaton<T>, T>, IPdaState<T>
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