namespace Archon.SwissArmyLib.Pooling
{
    /// <summary>
    /// Represents an object pool that has methods for spawning and despawning objects.
    /// </summary>
    /// <typeparam name="T">The type of objects that this pool can be used for.</typeparam>
    public interface IPool<T>
    {
        /// <summary>
        /// Spawns a recycled or new instance of the type <typeparamref name="T"/>.
        /// </summary>
        /// <returns>The spawned instance.</returns>
        T Spawn();

        /// <summary>
        /// Despawns an instance of the type <typeparamref name="T"/> and marks it for reuse.
        /// </summary>
        /// <param name="target">The instance to despawn.</param>
        void Despawn(T target);
    }
}
