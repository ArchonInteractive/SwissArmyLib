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
        private readonly Stack<T> _free = new Stack<T>();

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
            for (var i = 0; i < targetCount && _free.Count < targetCount; i++)
                _free.Push(_factory());
        }

        /// <summary>
        /// Spawns a recycled object if there's one available, otherwise creates a new instance.
        /// </summary>
        /// <returns>The spawned object.</returns>
        public virtual T Spawn()
        {
            var obj = _free.Count > 0
                ? _free.Pop()
                : _factory();

            OnSpawned(obj);

            return obj;
        }

        /// <summary>
        /// Despawns an object, adding it back to the pool.
        /// </summary>
        /// <param name="target">The object to despawn.</param>
        public virtual void Despawn(T target)
        {
            _instanceToTimerId.Remove(target);
            OnDespawned(target);
            _free.Push(target);
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
                TellMeWhen.SecondsUnscaled(delay, this, id);
            else
                TellMeWhen.Seconds(delay, this, id);
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

            if (target != null && _instanceToTimerId.ContainsKey(target))
                Despawn(target);
        }
    }
}