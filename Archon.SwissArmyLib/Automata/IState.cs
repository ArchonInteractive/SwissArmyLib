namespace Archon.SwissArmyLib.Automata
{
    /// <summary>
    /// Represents a state to be used in a state machine.
    /// 
    /// You might be looking for <see cref="IFsmState{T}"/> or <see cref="IPdaState{T}"/>.
    /// </summary>
    /// <typeparam name="TMachine">The type of the machine.</typeparam>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    public interface IState<TMachine, TContext>
    {
        /// <summary>
        /// The state machine this state belongs to.
        /// </summary>
        TMachine Machine { get; set; }

        /// <summary>
        /// The context for this state.
        /// </summary>
        TContext Context { get; set; }

        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        void Begin();

        /// <summary>
        /// Called every frame just before <see cref="Act"/>. 
        /// Use this to check whether you should change state.
        /// </summary>
        void Reason();

        /// <summary>
        /// Called every frame after <see cref="Reason"/>, if the state hasn't been changed.
        /// </summary>
        void Act(float deltaTime);

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        void End();
    }
}