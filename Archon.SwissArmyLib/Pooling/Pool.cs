using System;
using System.Collections.Generic;
using Archon.SwissArmyLib.Events;

namespace Archon.SwissArmyLib.Pooling
{
    public class Pool<T> : IPool<T>, TellMeWhen.ITimerCallback where T : class
    {
        private readonly Stack<T> _free = new Stack<T>();

        private readonly Func<T> _factory;

        private int _nextTimerId;
        private readonly Dictionary<T, int> _instanceToTimerId = new Dictionary<T, int>();

        public Pool(Func<T> create)
        {
            _factory = create;
        }

        public void Prewarm(int targetCount)
        {
            for (var i = 0; i < targetCount && _free.Count < targetCount; i++)
                _free.Push(_factory());
        }

        public virtual T Spawn()
        {
            var obj = _free.Count > 0
                ? _free.Pop()
                : _factory();

            OnSpawned(obj);

            return obj;
        }

        public virtual void Despawn(T target)
        {
            _instanceToTimerId.Remove(target);
            OnDespawned(target);
            _free.Push(target);
        }

        protected virtual void OnSpawned(T target)
        {
            var poolable = target as IPoolable;

            if (poolable != null)
                poolable.OnSpawned();
        }

        protected virtual void OnDespawned(T target)
        {
            var poolable = target as IPoolable;

            if (poolable != null)
                poolable.OnDespawned();
        }

        public void Despawn(T target, float delay, bool unscaledTime = false)
        {
            var id = _nextTimerId++;
            _instanceToTimerId[target] = id;

            if (unscaledTime)
                TellMeWhen.SecondsUnscaled(delay, this, id);
            else
                TellMeWhen.Seconds(delay, this, id);
        }

        public void CancelDespawn(T target)
        {
            int id;
            if (_instanceToTimerId.TryGetValue(target, out id))
                _instanceToTimerId.Remove(target);
        }

        public void OnTimesUp(int id, object args)
        {
            var target = args as T;

            if (target != null && _instanceToTimerId.ContainsKey(target))
                Despawn(target);
        }
    }
}