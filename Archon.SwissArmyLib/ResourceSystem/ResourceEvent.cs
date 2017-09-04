using JetBrains.Annotations;

namespace Archon.SwissArmyLib.ResourceSystem
{
    internal class ResourceEvent : IResourceChangeEvent, IResourcePreChangeEvent
    {
        /// <inheritdoc />
        public float OriginalDelta { get; set; }

        /// <inheritdoc />
        public float ModifiedDelta { get; set; }

        /// <inheritdoc />
        public float AppliedDelta { get; set; }

        /// <inheritdoc />
        public object Source { get; set; }

        /// <inheritdoc />
        public object Args { get; set; }
    }

    /// <summary>
    /// Defines an event for after a resource pool has been changed.
    /// </summary>
    public interface IResourceChangeEvent : IResourceEvent
    {
        /// <summary>
        /// Gets the originally requested resource change.
        /// </summary>
        float OriginalDelta { get; }

        /// <summary>
        /// Gets the modified delta after listeners of <see cref="ResourcePool.OnPreChange"/> had their chance to affect it.
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
    public interface IResourcePreChangeEvent : IResourceEvent
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
    public interface IResourceEvent
    {
        /// <summary>
        /// Gets the source of the resource change. 
        /// Can be null.
        /// </summary>
        [CanBeNull]
        object Source { get; }

        /// <summary>
        /// Gets the args that the sender sent with the change.
        /// </summary>
        [CanBeNull]
        object Args { get; }
    }
}
