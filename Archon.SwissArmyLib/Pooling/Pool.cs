using System;
using System.Collections.Generic;
using Archon.SwissArmyLib.Events;
using UnityEngine;

namespace Archon.SwissArmyLib.Pooling
{
    /// <summary>
    /// An object pool that can recycle objects of the type <typeparamref name="T"/>.
    /// 
    /// If the type implements <see cref="IPoolable"/> they will be notified when they're spawned and despawned.
    /// </summary>
    /// <typeparam name="T">The type of objects this object pool should contain.</typeparam>
    public class Pool<T> : IPool<T>, TellMeWhen.ITimerCallback where T : class
    {
        /// <summary>
        /// Gets the current amount of free instances in the pool.
        /// </summary>
        public int FreeCount { get { return Free.Count; } }

        /// <summary>
        /// Contains the items ready to be reused.
        /// </summary>
        protected readonly List<T> Free = new List<T>();

        private readonly Func<T> _factory;

        private int _nextTimerId;
        private readonly Dictionary<T, int> _instanceToTimerId = new Dictionary<T, int>();

        /// <summary>
        /// Creates a new object pool that uses the specified factory method to create object instances.
        /// </summary>
        /// <param name="create">Factory method to use for creating new instances.</param>
        public Pool(Func<T> create)
        {
            _factory = create;
        }

        /// <summary>
        /// Fills the pool with objects so that it contains the specified amount of objects.
        /// 
        /// If it already contains the specified amount or more, nothing will be done.
        /// </summary>
        /// <param name="targetCount"></param>
        public void Prewarm(int targetCount)
        {
            for (var i = 0; i < targetCount && Free.Count < targetCount; i++)
                Free.Add(_factory());
        }

        /// <summary>
        /// Spawns a recycled object if there's one available, otherwise creates a new instance.
        /// </summary>
        /// <returns>The spawned object.</returns>
        public virtual T Spawn()
        {
            T obj;
            if (Free.Count > 0)
            {
                obj = Free[Free.Count - 1];
                Free.RemoveAt(Free.Count - 1);
            }
            else
                obj = _factory();

            OnSpawned(obj);

            return obj;
        }

        /// <summary>
        /// Despawns an object, adding it back to the pool.
        /// </summary>
        /// <param name="target">The object to despawn.</param>
        public virtual void Despawn(T target)
        {
#if !TEST
            // costly check, so we only run it in the editor
            if (Debug.isDebugBuild && Free.Contains(target))
                throw new ArgumentException("Target is already despawned!", "target");
#endif

            _instanceToTimerId.Remove(target);
            OnDespawned(target);
            Free.Add(target);
        }

        /// <summary>
        /// Called when an object has been spawned and removed from the pool.
        /// </summary>
        /// <param name="target">The spawned object.</param>
        protected virtual void OnSpawned(T target)
        {
            var poolable = target as IPoolable;

            if (poolable != null)
                poolable.OnSpawned();
        }

        /// <summary>
        /// Called when an object has been despawned and placed back in the pool.
        /// </summary>
        /// <param name="target">The despawned object.</param>
        protected virtual void OnDespawned(T target)
        {
            var poolable = target as IPoolable;

            if (poolable != null)
                poolable.OnDespawned();
        }

        /// <summary>
        /// Despawns an object after a delay.
        /// </summary>
        /// <param name="target">The target to despawn.</param>
        /// <param name="delay">Time in seconds to wait before despawning the target.</param>
        /// <param name="unscaledTime">Should the delay be according to <see cref="Time.time"/> or <see cref="Time.unscaledTime"/>?</param>
        public void Despawn(T target, float delay, bool unscaledTime = false)
        {
            var id = _nextTimerId++;
            _instanceToTimerId[target] = id;

            if (unscaledTime)
                TellMeWhen.SecondsUnscaled(delay, this, id, target);
            else
                TellMeWhen.Seconds(delay, this, id, target);
        }

        /// <summary>
        /// Cancels a pending timed despawn.
        /// </summary>
        /// <param name="target">The target that shouldn't despawn after all.</param>
        public void CancelDespawn(T target)
        {
            int id;
            if (_instanceToTimerId.TryGetValue(target, out id))
                _instanceToTimerId.Remove(target);
        }

        void TellMeWhen.ITimerCallback.OnTimesUp(int id, object args)
        {
            var target = args as T;

            int mappedId;

            if (target != null && _instanceToTimerId.TryGetValue(target, out mappedId) && mappedId == id)
                Despawn(target);
        }
    }
}