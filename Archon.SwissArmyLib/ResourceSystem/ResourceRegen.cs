using System;
using Archon.SwissArmyLib.Events;
using Archon.SwissArmyLib.Utils;
using Archon.SwissArmyLib.Utils.Editor;
using JetBrains.Annotations;
using UnityEngine;

namespace Archon.SwissArmyLib.ResourceSystem
{
    /// <summary>
    /// Adds resource to a pool at a constant rate or in intervals.
    /// 
    /// If the <see cref="Target"/> is not set, it will try to find a resource pool on the same GameObject.
    /// </summary>
    [RequireComponent(typeof(ResourcePool))]
    public class ResourceRegen : MonoBehaviour, IEventListener<IResourceChangeEvent>
    {
        [Tooltip("The target resource pool that should regen.")]
        [SerializeField, ReadOnly(OnlyWhilePlaying = true)]
        private ResourcePool _target;

        [Tooltip("Time in seconds that regen should be paused when the target loses resource.")]
        [SerializeField] private float _downTimeOnResourceLoss;

        [Tooltip("Amount of resource that should be gained per second.")]
        [SerializeField] private float _constantAmountPerSecond;

        [Tooltip("Amount of resource that should be gained every interval.")]
        [SerializeField] private float _amountPerInterval;
        [Tooltip("How often in seconds that resource should be gained.")]
        [SerializeField] private float _interval;

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
        public ResourcePool Target
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
        protected void Awake()
        {
            if (_target == null)
                _target = GetComponent<ResourcePool>();
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
                _target.Add(ConstantAmountPerSecond * BetterTime.DeltaTime, this);

            if (Interval > 0 && time > _lastInterval + Interval)
            {
                _target.Add(AmountPerInterval, this);
                _lastInterval = time;
            }
        }

        void IEventListener<IResourceChangeEvent>.OnEvent(int eventId, IResourceChangeEvent args)
        {
            if (args.AppliedDelta < 0)
                _lastLossTime = BetterTime.Time;
        }
    }
}
