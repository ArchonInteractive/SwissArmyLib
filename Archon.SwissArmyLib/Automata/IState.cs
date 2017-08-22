namespace Archon.SwissArmyLib.Automata
{
    public interface IState<TMachine, TContext>
    {
        TMachine Machine { get; set; }
        TContext Context { get; set; }

        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        void Begin();

        /// <summary>
        /// Called every frame just before <see cref="Update"/>. 
        /// Use this to check whether you should change state.
        /// </summary>
        void Reason();

        /// <summary>
        /// Called every frame after <see cref="Reason"/>, if the state hasn't been changed.
        /// </summary>
        void Update(float deltaTime);

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        void End();
    }
}