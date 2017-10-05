using UnityEngine;

namespace Archon.SwissArmyLib.ResourceSystem
{
    /// <summary>
    /// Non-generic base class for <see cref="ResourcePool"/> to allow its editor to work for subclasses.
    /// </summary>
    public abstract class ResourcePoolBase : MonoBehaviour
    {
        /// <summary>
        /// Gets the current amount of resource in this pool.
        /// </summary>
        public abstract float Current { get; protected set; }

        /// <summary>
        /// Gets or sets the maximum amount of source that can be in this pool.
        /// </summary>
        public abstract float Max { get; set;  }

        /// <summary>
        /// Gets or sets whether adding resource should be disabled after the pool is completely empty, until it is renewed again.
        /// </summary>
        public abstract bool EmptyTillRenewed { get; set; }

        /// <summary>
        /// Gets a how full the resource is percentage-wise (0 to 1)
        /// </summary>
        public abstract float Percentage { get; }

        /// <summary>
        /// Gets whether the pool is completely empty.
        /// </summary>
        public abstract bool IsEmpty { get; }

        /// <summary>
        /// Gets whether the pool is completely empty.
        /// </summary>
        public abstract bool IsFull { get; }

        /// <summary>
        /// Get the (scaled) time since this pool was last empty.
        /// </summary>
        public abstract float TimeSinceEmpty { get; }

        /// <summary>
        /// Adds the specified amount of resource to the pool.
        /// </summary>
        /// <param name="amount">The amount to add.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public abstract float Add(float amount, bool forced = false);

        /// <summary>
        /// Removes the specified amount of resource to the pool.
        /// </summary>
        /// <param name="amount">The amount to remove.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public abstract float Remove(float amount, bool forced = false);

        /// <summary>
        /// Completely empties the pool.
        /// </summary>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public abstract float Empty(bool forced = false);

        /// <summary>
        /// Fully fills the pool.
        /// </summary>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public abstract float Fill(bool forced = false);

        /// <summary>
        /// Fills the pool to the specified amount.
        /// </summary>
        /// <param name="toValue">The amount of resource to restore to.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public abstract float Fill(float toValue, bool forced = false);

        /// <summary>
        /// Fully restores the pool, regardless of <see cref="ResourcePool{TSource,TArgs}.EmptyTillRenewed"/>.
        /// </summary>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public abstract float Renew(bool forced = false);

        /// <summary>
        /// Restores the pool to the specified amount, regardless of <see cref="ResourcePool{TSource,TArgs}.EmptyTillRenewed"/>.
        /// </summary>
        /// <param name="toValue">The amount of resource to restore to.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public abstract float Renew(float toValue, bool forced = false);
    }
}