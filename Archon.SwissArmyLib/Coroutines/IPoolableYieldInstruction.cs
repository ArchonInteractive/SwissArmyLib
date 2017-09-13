namespace Archon.SwissArmyLib.Coroutines
{
    /// <summary>
    /// Represents a yield instruction that can be freed when the coroutine they're running in is despawned.
    /// </summary>
    public interface IPoolableYieldInstruction
    {
        /// <summary>
        /// Frees the yield instruction placing them back in its pool.
        /// </summary>
        void Despawn();
    }
}