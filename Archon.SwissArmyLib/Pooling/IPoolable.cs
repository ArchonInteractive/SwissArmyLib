namespace Archon.SwissArmyLib.Pooling
{
    /// <summary>
    /// Represents an object that can be recycled in an <see cref="IPool{T}"/> and should be notified when it's spawned and despawned.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Called when the object is spawned (either fresh or recycled).
        /// </summary>
        void OnSpawned();

        /// <summary>
        /// Called when the object is despawned and marked for recycling.
        /// </summary>
        void OnDespawned();
    }
}