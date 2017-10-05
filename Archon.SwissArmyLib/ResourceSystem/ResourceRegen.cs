using System;
using Archon.SwissArmyLib.Events;
using Archon.SwissArmyLib.Utils;
using Archon.SwissArmyLib.Utils.Editor;
using JetBrains.Annotations;
using UnityEngine;

namespace Archon.SwissArmyLib.ResourceSystem
{
    /// <summary>
    /// Adds resource to a <see cref="ResourcePool"/> at a constant rate or in intervals.
    /// 
    /// If the <see cref="ResourceRegen{TSource,TArgs}.Target"/> is not set, it will try to find a <see cref="ResourcePool" /> on the same GameObject.
    /// 
    /// If you need type-safety consider subclassing the generic version: <see cref="ResourceRegen{TSource,TArgs}"/>.
    /// 
    /// <remarks>
    ///     This non-generic version only works for the non-generic <see cref="ResourcePool"/>.
    /// </remarks>
    /// </summary>
    public class ResourceRegen : ResourceRegen<object, object>
    {
        [Tooltip("The target resource pool that should regen.")]
        [SerializeField, ReadOnly(OnlyWhilePlaying = true)]
        private ResourcePool _target;

        /// <inheritdoc />
        protected override void Awake()
        {
            Target = _target;
            base.Awake();
        }
    }

    /// <summary>
    /// Adds resource to a <see cref="ResourcePool{TSource, TArgs}"/> at a constant rate or in intervals.
    /// 
    /// If the <see cref="Target"/> is not set, it will try to find a <see cref="ResourcePool{TSource,TArgs}" /> on the same GameObject.
    /// 
    /// Generic version of <see cref="ResourceRegen"/> in case you want type-safety. 
    /// To be able to use this you should make a non-generic subclass.
    /// </summary>
    public class ResourceRegen<TSource, TArgs> : MonoBehaviour, IEventListener<IResourceChangeEvent<TSource, TArgs>>
    {
        [Tooltip("Time in seconds that regen should be paused when the target loses resource.")]
        [SerializeField] private float _downTimeOnResourceLoss;

        [Tooltip("Amount of resource that should be gained per second.")]
        [SerializeField] private float _constantAmountPerSecond;

        [Tooltip("Amount of resource that should be gained every interval.")]
        [SerializeField] private float _amountPerInterval;
        [Tooltip("How often in seconds that resource should be gained.")]
        [SerializeField] private float _interval;

        private ResourcePool<TSource, TArgs> _target;
        private float _lastInterval;
        private float _lastLossTime;

        /// <summary>
        /// Gets or sets how often in seconds that <see cref="AmountPerInterval"/> resources should be gained.
        /// </summary>
        public float Interval
        {
            get { return _interval; }
            set { _interval = value; }
        }

        /// <summary>
        /// Gets or sets the amount of resource that should be gained every <see cref="Interval"/>.
        /// </summary>
        public float AmountPerInterval
        {
            get { return _amountPerInterval; }
            set { _amountPerInterval = value; }
        }

        /// <summary>
        /// Gets or sets the amount of resource that should be gained per second.
        /// </summary>
        public float ConstantAmountPerSecond
        {
            get { return _constantAmountPerSecond; }
            set { _constantAmountPerSecond = value; }
        }

        /// <summary>
        /// Gets or sets the amount of time in seconds to stop healing after the <see cref="Target"/> loses resource.
        /// </summary>
        public float DownTimeOnResourceLoss
        {
            get { return _downTimeOnResourceLoss; }
            set { _downTimeOnResourceLoss = value; }
        }

        /// <summary>
        /// Gets or sets the target <see cref="ResourcePool"/> that should regen.
        /// </summary>
        public ResourcePool<TSource, TArgs> Target
        {
            get { return _target; }
            set
            {
                if (_target != null && enabled)
                    _target.OnChange.RemoveListener(this);

                _target = value;

                if (_target != null && enabled)
                    _target.OnChange.AddListener(this);
            }
        }

        /// <summary>
        /// Called when the MonoBehaviour is added to a GameObject.
        /// </summary>
        [UsedImplicitly]
        protected virtual void Awake()
        {
            if (_target == null)
                _target = GetComponent<ResourcePool<TSource, TArgs>>();
        }

        /// <summary>
        /// Called when the MonoBehaviour is enabled.
        /// </summary>
        [UsedImplicitly]
        protected void OnEnable()
        {
            if (_target != null)
                _target.OnChange.AddListener(this);
        }

        /// <summary>
        /// Called when the MonoBehaviour is disabled.
        /// </summary>
        [UsedImplicitly]
        protected void OnDisable()
        {
            if (_target != null)
                _target.OnChange.RemoveListener(this);
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        [UsedImplicitly]
        protected void Update()
        {
            if (_target == null)
                return;

            var time = BetterTime.Time;

            if (time < _lastLossTime + DownTimeOnResourceLoss)
                return;

            if (Math.Abs(ConstantAmountPerSecond) > 0.001f)
                _target.Add(ConstantAmountPerSecond * BetterTime.DeltaTime);

            if (Interval > 0 && time > _lastInterval + Interval)
            {
                _target.Add(AmountPerInterval);
                _lastInterval = time;
            }
        }

        void IEventListener<IResourceChangeEvent<TSource, TArgs>>.OnEvent(int eventId, IResourceChangeEvent<TSource, TArgs> args)
        {
            if (args.AppliedDelta < 0)
                _lastLossTime = BetterTime.Time;
        }
    }
}
