using Archon.SwissArmyLib.Events;
using Archon.SwissArmyLib.Pooling;
using Archon.SwissArmyLib.Utils.Editor;
using UnityEngine;

namespace Archon.SwissArmyLib.ResourceSystem
{
    public class ResourcePool : MonoBehaviour
    {
        /// <summary>
        /// Event ids for resource change events.
        /// </summary>
        public static class EventIds
        {
#pragma warning disable 1591
            public const int
                PreChange = -8000,
                Change = -8001,
                Empty = -8002,
                Full = -8003,
                Renew = -8004;
#pragma warning restore 1591
        }

        /// <summary>
        /// Event called just before the resource amount is changed. 
        /// You can affect the applied change by modifying <see cref="IResourcePreChangeEvent.ModifiedDelta"/>.
        /// </summary>
        public readonly Event<IResourcePreChangeEvent> OnPreChange = new Event<IResourcePreChangeEvent>(EventIds.PreChange);

        /// <summary>
        /// Event called after the resource amount has been changed.
        /// </summary>
        public readonly Event<IResourceChangeEvent> OnChange = new Event<IResourceChangeEvent>(EventIds.Change);

        /// <summary>
        /// Event called once the pool has been completely emptied.
        /// </summary>
        public readonly Event<IResourceEvent> OnEmpty = new Event<IResourceEvent>(EventIds.Empty);

        /// <summary>
        /// Event called when the pool has been completely filled.
        /// </summary>
        public readonly Event<IResourceEvent> OnFull = new Event<IResourceEvent>(EventIds.Full);

        /// <summary>
        /// Event called when the pool is renewed using <see cref="Renew(object,object,bool)"/>.
        /// </summary>
        public readonly Event<IResourceEvent> OnRenew = new Event<IResourceEvent>(EventIds.Renew);

        [SerializeField, ReadOnly] private float _current;
        [SerializeField] private float _max = 100;
        [SerializeField] private bool _emptyTillRenewed = true;

        private bool _isEmpty, _isFull;

        /// <summary>
        /// Gets the current amount of resource in this pool.
        /// </summary>
        public float Current
        {
            get { return _current; }
            private set { _current = Mathf.Clamp(value, 0, Max); }
        }

        /// <summary>
        /// Gets or sets the maximum amount of source that can be in this pool.
        /// </summary>
        public float Max
        {
            get { return _max; }
            set { _max = value; }
        }

        /// <summary>
        /// Gets or sets whether adding resource should be disabled after the pool is completely empty, until it is renewed using <see cref="Renew(object,object,bool)"/> again.
        /// </summary>
        public bool EmptyTillRenewed
        {
            get { return _emptyTillRenewed; }
            set { _emptyTillRenewed = value; }
        }

        /// <summary>
        /// Gets a how full the resource is percentage-wise (0 to 1)
        /// </summary>
        public float Percentage
        {
            get
            {
                return Current / Max;
            }
        }

        /// <summary>
        /// Gets whether the pool is completely empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return _isEmpty; }
            private set { _isEmpty = value; }
        }

        /// <summary>
        /// Gets whether the pool is completely empty.
        /// </summary>
        public bool IsFull
        {
            get { return _isFull; }
            private set { _isFull = value; }
        }

        /// <summary>
        /// Called when the MonoBehaviour is added to a GameObject.
        /// </summary>
        protected virtual void Awake()
        {
            _current = Max;
        }

        /// <summary>
        /// Adds the specified amount of resource to the pool.
        /// </summary>
        /// <param name="amount">The amount to add.</param>
        /// <param name="source">The source of the change.</param>
        /// <param name="args">Optional args that will be passed to listeners.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public float Add(float amount, object source = null, object args = null, bool forced = false)
        {
            return Change(amount, source, args, forced);
        }

        /// <summary>
        /// Removes the specified amount of resource to the pool.
        /// </summary>
        /// <param name="amount">The amount to remove.</param>
        /// <param name="source">The source of the change.</param>
        /// <param name="args">Optional args that will be passed to listeners.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public float Remove(float amount, object source = null , object args = null, bool forced = false)
        {
            return -Change(-amount, source, args, forced);
        }

        /// <summary>
        /// Completely empties the pool.
        /// </summary>
        /// <param name="source">The source of the change.</param>
        /// <param name="args">Optional args that will be passed to listeners.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public float Empty(object source = null, object args = null, bool forced = false)
        {
            return Remove(float.MaxValue, source, args, forced);
        }

        /// <summary>
        /// Fully fills the pool.
        /// </summary>
        /// <param name="source">The source of the change.</param>
        /// <param name="args">Optional args that will be passed to listeners.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public float Fill(object source = null, object args = null, bool forced = false)
        {
            return Fill(float.MaxValue, source, args, forced);
        }

        /// <summary>
        /// Fills the pool to the specified amount.
        /// </summary>
        /// <param name="toValue">The amount of resource to restore to.</param>
        /// <param name="source">The source of the change.</param>
        /// <param name="args">Optional args that will be passed to listeners.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public float Fill(float toValue, object source = null, object args = null, bool forced = false)
        {
            return Change(toValue - Current, source, args, forced);
        }

        /// <summary>
        /// Fully restores the pool, regardless of <see cref="EmptyTillRenewed"/>.
        /// </summary>
        /// <param name="source">The source of the change.</param>
        /// <param name="args">Optional args that will be passed to listeners.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public float Renew(object source = null, object args = null, bool forced = false)
        {
            return Renew(float.MaxValue, source, args, forced);
        }

        /// <summary>
        /// Restores the pool to the specified amount, regardless of <see cref="EmptyTillRenewed"/>.
        /// </summary>
        /// <param name="toValue">The amount of resource to restore to.</param>
        /// <param name="source">The source of the change.</param>
        /// <param name="args">Optional args that will be passed to listeners.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public float Renew(float toValue, object source = null, object args = null, bool forced = false)
        {
            var before = _emptyTillRenewed;
            _emptyTillRenewed = false;
            var appliedDelta = Fill(toValue, source, args, forced);
            _emptyTillRenewed = before;

            var e = PoolHelper<ResourceEvent>.Spawn();
            e.Source = source;
            e.Args = args;
            OnRenew.Invoke(e);
            PoolHelper<ResourceEvent>.Despawn(e);

            return appliedDelta;
        }

        /// <summary>
        /// Changes the resource amount by <paramref name="delta"/>.
        /// </summary>
        /// <param name="delta">The delta to apply.</param>
        /// <param name="source">The source of the change.</param>
        /// <param name="args">Optional args that will be passed to listeners.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        protected virtual float Change(float delta, object source = null, object args = null, bool forced = false)
        {
            if (_isEmpty && _emptyTillRenewed)
                return 0;

            var resourceEvent = PoolHelper<ResourceEvent>.Spawn();
            resourceEvent.OriginalDelta = delta;
            resourceEvent.ModifiedDelta = delta;
            resourceEvent.Source = source;
            resourceEvent.Args = args;

            OnPreChange.Invoke(resourceEvent);

            if (forced)
                resourceEvent.ModifiedDelta = resourceEvent.OriginalDelta;

            if (Mathf.Approximately(resourceEvent.ModifiedDelta, 0))
            {
                // change was nullified completely
                PoolHelper<ResourceEvent>.Despawn(resourceEvent);
                return 0;
            }

            var valueBefore = _current;
            Current += resourceEvent.ModifiedDelta;
            resourceEvent.AppliedDelta = _current - valueBefore;

            var wasEmpty = _isEmpty;
            IsEmpty = _current < 0.01f;

            var wasFull = _isFull;
            IsFull = _current > _max - 0.01f;

            OnChange.Invoke(resourceEvent);

            if (_isEmpty && _isEmpty != wasEmpty)
                OnEmpty.Invoke(resourceEvent);
            
            if (_isFull && _isFull != wasFull)
                OnFull.Invoke(resourceEvent);

            var appliedDelta = resourceEvent.AppliedDelta;
            PoolHelper<ResourceEvent>.Despawn(resourceEvent);
            return appliedDelta;
        }
    }
}
