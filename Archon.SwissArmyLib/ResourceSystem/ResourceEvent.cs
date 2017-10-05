namespace Archon.SwissArmyLib.ResourceSystem
{
    internal class ResourceEvent<TSource, TArgs> : IResourceChangeEvent<TSource, TArgs>, IResourcePreChangeEvent<TSource, TArgs>
    {
        /// <inheritdoc />
        public float OriginalDelta { get; set; }

        /// <inheritdoc />
        public float ModifiedDelta { get; set; }

        /// <inheritdoc />
        public float AppliedDelta { get; set; }

        /// <inheritdoc />
        public TSource Source { get; set; }

        /// <inheritdoc />
        public TArgs Args { get; set; }
    }

    /// <summary>
    /// Defines an event for after a resource pool has been changed.
    /// </summary>
    public interface IResourceChangeEvent<TSource, TArgs> : IResourceEvent<TSource, TArgs>
    {
        /// <summary>
        /// Gets the originally requested resource change.
        /// </summary>
        float OriginalDelta { get; }

        /// <summary>
        /// Gets the modified delta after listeners of <see cref="ResourcePool{TSource, TArgs}.OnPreChange"/> had their chance to affect it.
        /// </summary>
        float ModifiedDelta { get; }

        /// <summary>
        /// Gets the actual applied (and clamped) delta. 
        /// Basically just the difference in resource amount before and after the change.
        /// </summary>
        float AppliedDelta { get; }
    }

    /// <summary>
    /// Defines a change event that has not yet happened, and can be altered.
    /// </summary>
    public interface IResourcePreChangeEvent<TSource, TArgs> : IResourceEvent<TSource, TArgs>
    {
        /// <summary>
        /// Gets the originally requested resource change.
        /// </summary>
        float OriginalDelta { get; }

        /// <summary>
        /// Gets or sets the modified delta that will be applied after this event.
        /// </summary>
        float ModifiedDelta { get; set; }
    }

    /// <summary>
    /// Defines a barebones resource event.
    /// </summary>
    public interface IResourceEvent<TSource, TArgs>
    {
        /// <summary>
        /// Gets or sets the source of the resource change.
        /// </summary>
        TSource Source { get; set; }

        /// <summary>
        /// Gets or sets the args that the sender sent with the change.
        /// </summary>
        TArgs Args { get; set; }
    }
}
