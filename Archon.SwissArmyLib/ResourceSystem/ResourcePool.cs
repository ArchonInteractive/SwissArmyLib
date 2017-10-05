using Archon.SwissArmyLib.Events;
using Archon.SwissArmyLib.Pooling;
using Archon.SwissArmyLib.Utils;
using Archon.SwissArmyLib.Utils.Editor;
using UnityEngine;

namespace Archon.SwissArmyLib.ResourceSystem
{
    /// <summary>
    /// A flexible resource pool (eg. health, mana, energy).
    /// 
    /// If you need type-safety consider subclassing the generic version: <see cref="ResourcePool{TSource,TArgs}"/>.
    /// 
    /// <seealso cref="ResourceRegen"/>
    /// <seealso cref="Shield"/>
    /// </summary>
    public class ResourcePool : ResourcePool<object, object>
    {
        
    }

    /// <summary>
    /// A flexible resource pool (eg. health, mana, energy).
    /// 
    /// Generic version of <see cref="ResourcePool"/> in case you want type-safety. 
    /// To be able to use this you should make a non-generic subclass.
    /// 
    /// <seealso cref="ResourceRegen"/>
    /// <seealso cref="Shield"/>
    /// </summary>
    public class ResourcePool<TSource, TArgs> : ResourcePoolBase
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
        /// You can affect the applied change by modifying <see cref="IResourcePreChangeEvent{TSource,TArgs}.ModifiedDelta"/>.
        /// </summary>
        public readonly Event<IResourcePreChangeEvent<TSource, TArgs>> OnPreChange = new Event<IResourcePreChangeEvent<TSource, TArgs>>(EventIds.PreChange);

        /// <summary>
        /// Event called after the resource amount has been changed.
        /// </summary>
        public readonly Event<IResourceChangeEvent<TSource, TArgs>> OnChange = new Event<IResourceChangeEvent<TSource, TArgs>>(EventIds.Change);

        /// <summary>
        /// Event called once the pool has been completely emptied.
        /// </summary>
        public readonly Event<IResourceEvent<TSource, TArgs>> OnEmpty = new Event<IResourceEvent<TSource, TArgs>>(EventIds.Empty);

        /// <summary>
        /// Event called when the pool has been completely filled.
        /// </summary>
        public readonly Event<IResourceEvent<TSource, TArgs>> OnFull = new Event<IResourceEvent<TSource, TArgs>>(EventIds.Full);

        /// <summary>
        /// Event called when the pool is renewed using <see cref="Renew(TSource,TArgs,bool)"/>.
        /// </summary>
        public readonly Event<IResourceEvent<TSource, TArgs>> OnRenew = new Event<IResourceEvent<TSource, TArgs>>(EventIds.Renew);

        [Tooltip("Current amount of resource in the pool.")]
        [SerializeField, ReadOnly] private float _current;
        [Tooltip("Max amount of resource that can be in the pool.")]
        [SerializeField] private float _max = 100;
        [Tooltip("Whether the pool should remain empty until it is renewed using Renew().")]
        [SerializeField] private bool _emptyTillRenewed = true;

        private bool _isEmpty, _isFull;
        private float _timeEmptied;

        /// <summary>
        /// Gets the current amount of resource in this pool.
        /// </summary>
        public override float Current
        {
            get { return _current; }
            protected set { _current = Mathf.Clamp(value, 0, Max); }
        }

        /// <summary>
        /// Gets or sets the maximum amount of source that can be in this pool.
        /// </summary>
        public override float Max
        {
            get { return _max; }
            set { _max = value; }
        }

        /// <summary>
        /// Gets or sets whether adding resource should be disabled after the pool is completely empty, until it is renewed using <see cref="Renew(TSource,TArgs,bool)"/> again.
        /// </summary>
        public override bool EmptyTillRenewed
        {
            get { return _emptyTillRenewed; }
            set { _emptyTillRenewed = value; }
        }

        /// <summary>
        /// Gets a how full the resource is percentage-wise (0 to 1)
        /// </summary>
        public override float Percentage
        {
            get
            {
                return Current / Max;
            }
        }

        /// <summary>
        /// Gets whether the pool is completely empty.
        /// </summary>
        public override bool IsEmpty
        {
            get { return _isEmpty; }
        }

        /// <summary>
        /// Gets whether the pool is completely empty.
        /// </summary>
        public override bool IsFull
        {
            get { return _isFull; }
        }

        /// <summary>
        /// Get the (scaled) time since this pool was last empty.
        /// </summary>
        public override float TimeSinceEmpty
        {
            get
            {
                if (_isEmpty)
                    return 0;

                return BetterTime.Time - _timeEmptied;
            }
        }

        /// <summary>
        /// Gets the source to fallback on if no source is specified.
        /// </summary>
        public virtual TSource DefaultSource { get { return default(TSource); } }

        /// <summary>
        /// Gets the args to fallback on if no args is specified.
        /// </summary>
        public virtual TArgs DefaultArgs { get { return default(TArgs); } }

        /// <summary>
        /// Called when the MonoBehaviour is added to a GameObject.
        /// </summary>
        protected virtual void Awake()
        {
            _current = Max;
        }

        /// <inheritdoc />
        public override float Add(float amount, bool forced = false)
        {
            return Change(amount, DefaultSource, DefaultArgs, forced);
        }

        /// <summary>
        /// Adds the specified amount of resource to the pool.
        /// </summary>
        /// <param name="amount">The amount to add.</param>
        /// <param name="source">The source of the change.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public float Add(float amount, TSource source, bool forced = false)
        {
            return Change(amount, source, DefaultArgs, forced);
        }

        /// <summary>
        /// Adds the specified amount of resource to the pool.
        /// </summary>
        /// <param name="amount">The amount to add.</param>
        /// <param name="source">The source of the change.</param>
        /// <param name="args">Optional args that will be passed to listeners.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public float Add(float amount, TSource source, TArgs args, bool forced = false)
        {
            return Change(amount, source, args, forced);
        }

        /// <inheritdoc />
        public override float Remove(float amount, bool forced = false)
        {
            return -Change(-amount, DefaultSource, DefaultArgs, forced);
        }

        /// <summary>
        /// Removes the specified amount of resource to the pool.
        /// </summary>
        /// <param name="amount">The amount to remove.</param>
        /// <param name="source">The source of the change.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public float Remove(float amount, TSource source, bool forced = false)
        {
            return -Change(-amount, source, DefaultArgs, forced);
        }

        /// <summary>
        /// Removes the specified amount of resource to the pool.
        /// </summary>
        /// <param name="amount">The amount to remove.</param>
        /// <param name="source">The source of the change.</param>
        /// <param name="args">Optional args that will be passed to listeners.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public float Remove(float amount, TSource source, TArgs args, bool forced = false)
        {
            return -Change(-amount, source, args, forced);
        }

        /// <inheritdoc />
        public override float Empty(bool forced = false)
        {
            return Remove(float.MaxValue, DefaultSource, DefaultArgs, forced);
        }

        /// <summary>
        /// Completely empties the pool.
        /// </summary>
        /// <param name="source">The source of the change.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public float Empty(TSource source, bool forced = false)
        {
            return Remove(float.MaxValue, source, DefaultArgs, forced);
        }

        /// <summary>
        /// Completely empties the pool.
        /// </summary>
        /// <param name="source">The source of the change.</param>
        /// <param name="args">Optional args that will be passed to listeners.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public float Empty(TSource source, TArgs args, bool forced = false)
        {
            return Remove(float.MaxValue, source, args, forced);
        }

        /// <inheritdoc />
        public override float Fill(bool forced = false)
        {
            return Fill(float.MaxValue, DefaultSource, DefaultArgs, forced);
        }

        /// <summary>
        /// Fully fills the pool.
        /// </summary>
        /// <param name="source">The source of the change.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public float Fill(TSource source, bool forced = false)
        {
            return Fill(float.MaxValue, source, DefaultArgs, forced);
        }

        /// <summary>
        /// Fully fills the pool.
        /// </summary>
        /// <param name="source">The source of the change.</param>
        /// <param name="args">Optional args that will be passed to listeners.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public float Fill(TSource source, TArgs args, bool forced = false)
        {
            return Fill(float.MaxValue, source, args, forced);
        }

        /// <inheritdoc />
        public override float Fill(float toValue, bool forced = false)
        {
            return Change(toValue - Current, DefaultSource, DefaultArgs, forced);
        }

        /// <summary>
        /// Fills the pool to the specified amount.
        /// </summary>
        /// <param name="toValue">The amount of resource to restore to.</param>
        /// <param name="source">The source of the change.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public float Fill(float toValue, TSource source, bool forced = false)
        {
            return Change(toValue - Current, source, DefaultArgs, forced);
        }

        /// <summary>
        /// Fills the pool to the specified amount.
        /// </summary>
        /// <param name="toValue">The amount of resource to restore to.</param>
        /// <param name="source">The source of the change.</param>
        /// <param name="args">Optional args that will be passed to listeners.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public float Fill(float toValue, TSource source, TArgs args, bool forced = false)
        {
            return Change(toValue - Current, source, args, forced);
        }

        /// <inheritdoc />
        public override float Renew(bool forced = false)
        {
            return Renew(float.MaxValue, DefaultSource, DefaultArgs, forced);
        }

        /// <summary>
        /// Fully restores the pool, regardless of <see cref="ResourcePool{TSource,TArgs}.EmptyTillRenewed"/>.
        /// </summary>
        /// <param name="source">The source of the change.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public float Renew(TSource source, bool forced = false)
        {
            return Renew(float.MaxValue, source, DefaultArgs, forced);
        }

        /// <summary>
        /// Fully restores the pool, regardless of <see cref="ResourcePool{TSource,TArgs}.EmptyTillRenewed"/>.
        /// </summary>
        /// <param name="source">The source of the change.</param>
        /// <param name="args">Optional args that will be passed to listeners.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public float Renew(TSource source, TArgs args, bool forced = false)
        {
            return Renew(float.MaxValue, source, args, forced);
        }

        /// <inheritdoc />
        public override float Renew(float toValue, bool forced = false)
        {
            return Renew(toValue, DefaultSource, DefaultArgs, forced);
        }

        /// <summary>
        /// Restores the pool to the specified amount, regardless of <see cref="ResourcePool{TSource,TArgs}.EmptyTillRenewed"/>.
        /// </summary>
        /// <param name="toValue">The amount of resource to restore to.</param>
        /// <param name="source">The source of the change.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public float Renew(float toValue, TSource source, bool forced = false)
        {
            return Renew(toValue, source, DefaultArgs, forced);
        }

        /// <summary>
        /// Restores the pool to the specified amount, regardless of <see cref="ResourcePool{TSource,TArgs}.EmptyTillRenewed"/>.
        /// </summary>
        /// <param name="toValue">The amount of resource to restore to.</param>
        /// <param name="source">The source of the change.</param>
        /// <param name="args">Optional args that will be passed to listeners.</param>
        /// <param name="forced">Controls whether to force the change, despite modifications by listeners.</param>
        /// <returns>The resulting change in the pool.</returns>
        public float Renew(float toValue, TSource source, TArgs args, bool forced = false)
        {
            var before = _emptyTillRenewed;
            _emptyTillRenewed = false;
            var appliedDelta = Fill(toValue, source, args, forced);
            _emptyTillRenewed = before;

            var e = PoolHelper<ResourceEvent<TSource, TArgs>>.Spawn();
            e.Source = source;
            e.Args = args;
            OnRenew.Invoke(e);
            PoolHelper<ResourceEvent<TSource, TArgs>>.Despawn(e);

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
        protected virtual float Change(float delta, TSource source, TArgs args, bool forced = false)
        {
            if (_isEmpty && _emptyTillRenewed)
                return 0;

            var resourceEvent = PoolHelper<ResourceEvent<TSource, TArgs>>.Spawn();
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
                PoolHelper<ResourceEvent<TSource, TArgs>>.Despawn(resourceEvent);
                return 0;
            }

            var valueBefore = _current;
            Current += resourceEvent.ModifiedDelta;
            resourceEvent.AppliedDelta = _current - valueBefore;

            var wasEmpty = _isEmpty;
            _isEmpty = _current < 0.01f;

            var wasFull = _isFull;
            _isFull = _current > _max - 0.01f;

            OnChange.Invoke(resourceEvent);

            if (_isEmpty && _isEmpty != wasEmpty)
            {
                _timeEmptied = BetterTime.Time;
                OnEmpty.Invoke(resourceEvent);
            }

            if (_isFull && _isFull != wasFull)
                OnFull.Invoke(resourceEvent);

            var appliedDelta = resourceEvent.AppliedDelta;
            PoolHelper<ResourceEvent<TSource, TArgs>>.Despawn(resourceEvent);
            return appliedDelta;
        }
    }
}
