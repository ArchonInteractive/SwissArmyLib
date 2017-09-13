using System;
using System.Collections;
using System.Collections.Generic;
using Archon.SwissArmyLib.Events;
using Archon.SwissArmyLib.Pooling;
using Archon.SwissArmyLib.Utils;
using UnityEngine;

namespace Archon.SwissArmyLib.Coroutines
{
    /// <summary>
    /// A very similar but more performant alternative to Unity's coroutines.
    /// 
    /// A coroutine do not belong to any gameobject and therefore doesn't depend on their life cycle. 
    /// Which update loop they're part of can be specified when they're started.
    /// 
    /// They also allocate less garbage (especially with <see cref="WaitForSeconds"/> and <see cref="WaitForSecondsRealtime"/>).
    /// 
    /// Just as with Unity's coroutines you can yield a <see cref="WWW"/> and <see cref="AsyncOperation"/> to wait for them to finish.
    /// 
    /// Instead of using Unity's YieldInstructions you should use the pooled replacements:
    /// <list type="bullet">
    ///     <item><description><see cref="WaitForSeconds"/></description></item>
    ///     <item><description><see cref="WaitForSecondsRealtime"/></description></item>
    ///     <item><description><see cref="WaitUntil"/></description></item>
    ///     <item><description><see cref="WaitWhile"/></description></item>
    ///     <item><description><see cref="WaitForWWW"/></description></item>
    ///     <item><description><see cref="WaitForAsyncOperation"/></description></item>
    /// </list>
    /// </summary>
    public class BetterCoroutines : IEventListener
    {
        private static readonly LinkedList<BetterCoroutine> CoroutinesUpdate = new LinkedList<BetterCoroutine>();
        private static readonly LinkedList<BetterCoroutine> CoroutinesLateUpdate = new LinkedList<BetterCoroutine>();
        private static readonly LinkedList<BetterCoroutine> CoroutinesFixedUpdate = new LinkedList<BetterCoroutine>();

        private static LinkedListNode<BetterCoroutine> _current;

        static BetterCoroutines()
        {
            ServiceLocator.RegisterSingleton(new BetterCoroutines());
        }

        private BetterCoroutines()
        {
            ManagedUpdate.OnUpdate.AddListener(this, int.MinValue);
            ManagedUpdate.OnLateUpdate.AddListener(this, int.MinValue);
            ManagedUpdate.OnFixedUpdate.AddListener(this, int.MinValue);
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~BetterCoroutines()
        {
            ManagedUpdate.OnUpdate.RemoveListener(this);
            ManagedUpdate.OnLateUpdate.RemoveListener(this);
            ManagedUpdate.OnFixedUpdate.RemoveListener(this);
        }

        /// <summary>
        /// Starts a new coroutine.
        /// </summary>
        /// <param name="enumerator"></param>
        /// <param name="updateLoop">Which update loop should the coroutine be part of?</param>
        /// <returns>The coroutine.</returns>
        public static IBetterCoroutine Start(IEnumerator enumerator, UpdateLoop updateLoop = UpdateLoop.Update)
        {
            var routine = PoolHelper<BetterCoroutine>.Spawn();
            routine.Enumerator = enumerator;
            routine.UpdateLoop = updateLoop;

            var scaledTime = GetTime(updateLoop, false);
            var unscaledTime = GetTime(updateLoop, true);

            var continueRunning = UpdateCoroutine(scaledTime, unscaledTime, routine);
            if (!continueRunning)
            {
                PoolHelper<BetterCoroutine>.Despawn(routine);
                return null;
            }

            if (_current != null)
                _current.List.AddBefore(_current, routine);
            else
            {
                var list = GetList(updateLoop);
                list.AddFirst(routine);
            }

            return routine;
        }

        private static void StartChild(IEnumerator enumerator, BetterCoroutine parent)
        {
            var subroutine = (BetterCoroutine) Start(enumerator, parent.UpdateLoop);
            subroutine.Parent = parent;
            parent.Child = subroutine;
        }

        /// <summary>
        /// Stops a running coroutine prematurely. 
        /// 
        /// This will stop any child coroutines as well.
        /// </summary>
        /// <param name="coroutine">The coroutine to stop.</param>
        public static void Stop(IBetterCoroutine coroutine)
        {
            var routine = coroutine as BetterCoroutine;
            if (routine != null)
                StopInternal(routine);
        }

        private static void StopInternal(BetterCoroutine coroutine)
        {
            if (coroutine.IsDone)
                return;

            if (coroutine.Parent != null)
                coroutine.Parent.Child = null;

            while (coroutine != null)
            {
                coroutine.IsDone = true;
                coroutine = coroutine.Child;
            }
        }

        /// <summary>
        /// Stops all coroutines.
        /// </summary>
        public static void StopAll()
        {
            StopAll(UpdateLoop.Update);
            StopAll(UpdateLoop.LateUpdate);
            StopAll(UpdateLoop.FixedUpdate);
        }

        /// <summary>
        /// Stops all coroutines that are running in the specified update loop.
        /// </summary>
        public static void StopAll(UpdateLoop updateLoop)
        {
            var coroutines = GetList(updateLoop);
            var isInUpdate = _current != null && _current.List == coroutines;

            var current = coroutines.First;
            while (current != null)
            {
                var next = current.Next;
                var routine = current.Value;

                StopInternal(routine);

                if (!isInUpdate)
                {
                    PoolHelper<BetterCoroutine>.Despawn(routine);
                    coroutines.Remove(current);
                }

                current = next;
            }
        }

        /// <summary>
        /// Waits for the specified amount of seconds, either in scaled time (just as Unity's <see cref="UnityEngine.WaitForSeconds"/>) or in unscaled time.
        /// </summary>
        /// <param name="seconds">Duration in seconds to wait before continuing.</param>
        /// <param name="unscaled">Should the wait time ignore <see cref="Time.timeScale"/>?</param>
        /// <returns></returns>
        public static object WaitForSeconds(float seconds, bool unscaled = false)
        {
            var waitForSeconds = WaitForSecondsLite.Instance;
            waitForSeconds.Unscaled = unscaled;
            waitForSeconds.Duration = seconds;
            return waitForSeconds;
        }

        /// <summary>
        /// Waits for the specified amount of seconds in real time.
        /// 
        /// Lighter replacement for <see cref="UnityEngine.WaitForSecondsRealtime"/>.
        /// </summary>
        /// <param name="seconds">The amount of seconds to wait for.</param>
        /// <returns></returns>
        public static IEnumerator WaitForSecondsRealtime(float seconds)
        {
            return WaitForSecondsRealtimeLite.Create(seconds);
        }

        /// <summary>
        /// Waits until the specified <see cref="AsyncOperation"/> is done.
        /// </summary>
        /// <param name="operation">The async operation to wait for.</param>
        /// <returns></returns>
        public static IEnumerator WaitForAsyncOperation(AsyncOperation operation)
        {
            return Coroutines.WaitForAsyncOperation.Create(operation);
        }

        /// <summary>
        /// Waits until the specified <see cref="WWW"/> object has finished.
        /// </summary>
        /// <param name="www">The WWW object to wait for.</param>
        /// <returns></returns>
        public static IEnumerator WaitForWWW(WWW www)
        {
            return Coroutines.WaitForWWW.Create(www);
        }

        /// <summary>
        /// Waits until the given predicate returns true.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IEnumerator WaitUntil(Func<bool> predicate)
        {
            return WaitUntilLite.Create(predicate);
        }

        /// <summary>
        /// Waits until the given predicate returns false.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IEnumerator WaitWhile(Func<bool> predicate)
        {
            return WaitWhileLite.Create(predicate);
        }

        private static LinkedList<BetterCoroutine> GetList(UpdateLoop updateLoop)
        {
            switch (updateLoop)
            {
                case UpdateLoop.Update:
                    return CoroutinesUpdate;
                case UpdateLoop.LateUpdate:
                    return CoroutinesLateUpdate;
                case UpdateLoop.FixedUpdate:
                    return CoroutinesFixedUpdate;
                default:
                    throw new ArgumentOutOfRangeException("updateLoop", updateLoop, null);
            }
        }

        private static void Update(LinkedList<BetterCoroutine> coroutines)
        {
            var scaledTime = BetterTime.Time;
            var unscaledTime = BetterTime.UnscaledTime;

            _current = coroutines.First;
            while (_current != null)
            {
                var routine = _current.Value;

                var shouldContinue = UpdateCoroutine(scaledTime, unscaledTime, routine);

                if (!shouldContinue)
                {
                    var next = _current.Next;

                    StopInternal(routine);
                    coroutines.Remove(_current);
                    PoolHelper<BetterCoroutine>.Despawn(routine);

                    _current = next;
                    continue;
                }

                _current = _current.Next;
            }
        }

        private static bool UpdateCoroutine(float scaledTime, float unscaledTime, BetterCoroutine coroutine)
        {
            if (coroutine.IsDone)
                return false;

            if (coroutine.Child != null)
                return true;

            var time = coroutine.WaitTimeIsUnscaled ? unscaledTime : scaledTime;

            if (coroutine.WaitTillTime > time)
                return true;

            var enumerator = coroutine.Enumerator;

            try
            {
                if (!enumerator.MoveNext())
                    return false;
            }
            catch (Exception e)
            {
                // something bad happened in the coroutine, just print out the error and stop the problematic routine.
                // todo: should this stop parent coroutines as well?
                Debug.LogError(e);
                return false;
            }

            var current = enumerator.Current;

            if (current == null) return true;

            var subroutine = current as BetterCoroutine;
            if (subroutine != null)
            {
                coroutine.Child = subroutine;
                subroutine.Parent = coroutine;
                return true;
            }

            var subEnumerator = current as IEnumerator;
            if (subEnumerator != null)
            {
                StartChild(subEnumerator, coroutine);
                return true;
            }

            var waitForSeconds = current as WaitForSecondsLite;
            if (waitForSeconds != null)
            {
                time = waitForSeconds.Unscaled ? unscaledTime : scaledTime;
                coroutine.WaitTimeIsUnscaled = waitForSeconds.Unscaled;
                coroutine.WaitTillTime = time + waitForSeconds.Duration;
                return true;
            }

            var www = current as WWW;
            if (www != null)
            {
                StartChild(Coroutines.WaitForWWW.Create(www), coroutine);
                return true;
            }

            var asyncOperation = current as AsyncOperation;
            if (asyncOperation != null)
            {
                StartChild(Coroutines.WaitForAsyncOperation.Create(asyncOperation), coroutine);
                return true;
            }

            // we could use reflection, but it's slow and users really should switch over.
            if (current is WaitForSeconds)
                Debug.LogError("UnityEngine.WaitForSeconds is not supported in BetterCoroutines. Please use BetterCoroutines.WaitForSeconds() instead.");

            return true;
        }

        void IEventListener.OnEvent(int eventId)
        {
            switch (eventId)
            {
                case ManagedUpdate.EventIds.Update:
                    Update(CoroutinesUpdate);
                    return;
                case ManagedUpdate.EventIds.LateUpdate:
                    Update(CoroutinesLateUpdate);
                    return;
                case ManagedUpdate.EventIds.FixedUpdate:
                    Update(CoroutinesFixedUpdate);
                    return;
            }
        }

        private static float GetTime(UpdateLoop updateLoop, bool unscaled)
        {
            float currentTime;

            if (updateLoop == UpdateLoop.FixedUpdate)
            {
                currentTime = unscaled
                    ? BetterTime.FixedUnscaledTime
                    : BetterTime.FixedTime;
            }
            else
            {
                currentTime = unscaled
                    ? BetterTime.UnscaledTime
                    : BetterTime.Time;
            }

            return currentTime;
        }
    }
}
