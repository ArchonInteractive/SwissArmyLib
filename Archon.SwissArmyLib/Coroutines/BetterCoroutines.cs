using System;
using System.Collections;
using System.Collections.Generic;
using Archon.SwissArmyLib.Collections;
using Archon.SwissArmyLib.Events;
using Archon.SwissArmyLib.Pooling;
using Archon.SwissArmyLib.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Archon.SwissArmyLib.Coroutines
{
    /// <summary>
    /// A very similar but more performant alternative to Unity's coroutines.
    /// 
    /// A coroutine do not belong to any gameobject and therefore doesn't depend on their life cycle. 
    /// Which update loop they're part of can be specified when they're started.
    /// 
    /// They also allocate less garbage (especially with the yield instructions).
    /// 
    /// Just as with Unity's coroutines you can yield a <see cref="WWW"/> and <see cref="AsyncOperation"/> to wait for them to finish.
    /// 
    /// Instead of using Unity's YieldInstructions you should use the pooled replacements:
    /// <list type="bullet">
    ///     <item><description><see cref="WaitForOneFrame"/></description></item>
    ///     <item><description><see cref="WaitForSeconds"/></description></item>
    ///     <item><description><see cref="WaitForSecondsRealtime"/></description></item>
    ///     <item><description><see cref="WaitForEndOfFrame"/></description></item>
    ///     <item><description><see cref="WaitUntil"/></description></item>
    ///     <item><description><see cref="WaitWhile"/></description></item>
    ///     <item><description><see cref="WaitForWWW"/></description></item>
    ///     <item><description><see cref="WaitForAsyncOperation"/></description></item>
    /// </list>
    /// </summary>
    public class BetterCoroutines : IEventListener
    {
        /// <summary>
        /// Suspends a coroutine for one frame.
        /// </summary>
        public const object WaitForOneFrame = null;

        /// <summary>
        /// Suspends a coroutine until the very end of the current frame.
        /// </summary>
        public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();

        private static readonly LinkedList<BetterCoroutine> CoroutinesUpdate = new LinkedList<BetterCoroutine>();
        private static readonly LinkedList<BetterCoroutine> CoroutinesLateUpdate = new LinkedList<BetterCoroutine>();
        private static readonly LinkedList<BetterCoroutine> CoroutinesFixedUpdate = new LinkedList<BetterCoroutine>();
        private static readonly LinkedList<BetterCoroutine> CoroutinesWaitingForEndOfFrame = new LinkedList<BetterCoroutine>();

        private static LinkedListNode<BetterCoroutine> _current;

        private static readonly DictionaryWithDefault<int, BetterCoroutine> IdToCoroutine = new DictionaryWithDefault<int, BetterCoroutine>();
        private static int _nextId = 1;

        static BetterCoroutines()
        {
            var instance = new BetterCoroutines();
            ServiceLocator.RegisterSingleton(instance);
            ServiceLocator.RegisterSingleton<BetterCoroutinesEndOfFrame>();

            ServiceLocator.GlobalReset += () =>
            {
                ServiceLocator.RegisterSingleton(instance);
                ServiceLocator.RegisterSingleton<BetterCoroutinesEndOfFrame>();
            };
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
        /// <returns>The id of the coroutine.</returns>
        public static int Start(IEnumerator enumerator, UpdateLoop updateLoop = UpdateLoop.Update)
        {
            var id = GetNextId();

            var routine = PoolHelper<BetterCoroutine>.Spawn();
            routine.Id = id;
            routine.Enumerator = enumerator;
            routine.UpdateLoop = updateLoop;

            var scaledTime = GetTime(updateLoop, false);
            var unscaledTime = GetTime(updateLoop, true);

            var continueRunning = UpdateCoroutine(scaledTime, unscaledTime, routine);
            if (!continueRunning)
            {
                PoolHelper<BetterCoroutine>.Despawn(routine);
                return id;
            }

            IdToCoroutine[id] = routine;

            if (_current != null)
                _current.List.AddBefore(_current, routine);
            else
            {
                var list = GetList(updateLoop);
                list.AddFirst(routine);
            }

            return id;
        }

        private static void StartChild(IEnumerator enumerator, BetterCoroutine parent)
        {
            var subroutineId = Start(enumerator, parent.UpdateLoop);
            var subroutine = IdToCoroutine[subroutineId];
            subroutine.Parent = parent;
            parent.Child = subroutine;
        }

        /// <summary>
        /// Checks whether a coroutine with the given ID is running.
        /// </summary>
        /// <param name="id">The id of the coroutine to check.</param>
        /// <returns>True if running, otherwise false.</returns>
        public static bool IsRunning(int id)
        {
            return id > 0 && IdToCoroutine.ContainsKey(id);
        }

        /// <summary>
        /// Stops a running coroutine prematurely. 
        /// 
        /// This will stop any child coroutines as well.
        /// </summary>
        /// <param name="id">The id of the coroutine to stop.</param>
        /// <returns>True if the coroutine was found and stopped, otherwise false.</returns>
        public static bool Stop(int id)
        {
            var routine = IdToCoroutine[id];

            if (routine != null)
            {
                Stop(routine);
                return true;
            }

            return false;
        }

        internal static void Stop(BetterCoroutine coroutine)
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

                Stop(routine);

                if (!isInUpdate)
                {
                    IdToCoroutine.Remove(routine.Id);
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

        private static int GetNextId()
        {
            if (_nextId < 1)
                _nextId = 1;

            return _nextId++;
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

                    Stop(routine);
                    coroutines.Remove(_current);
                    IdToCoroutine.Remove(routine.Id);
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

            if (coroutine.WaitingForEndOfFrame)
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

            if (current is WaitForEndOfFrame)
            {
                coroutine.WaitingForEndOfFrame = true;
                CoroutinesWaitingForEndOfFrame.AddFirst(coroutine);
                return true;
            }

            var www = current as WWW;
            if (www != null)
            {
                if (!www.isDone)
                    StartChild(Coroutines.WaitForWWW.Create(www), coroutine);
                return true;
            }

            var asyncOperation = current as AsyncOperation;
            if (asyncOperation != null)
            {
                if (!asyncOperation.isDone)
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

        internal static void ProcessEndOfFrame()
        {
            var current = CoroutinesWaitingForEndOfFrame.First;
            while (current != null)
            {
                var next = current.Next;
                var routine = current.Value;

                routine.WaitingForEndOfFrame = false;

                var scaledTime = GetTime(routine.UpdateLoop, false);
                var unscaledTime = GetTime(routine.UpdateLoop, true);

                var shouldContinue = UpdateCoroutine(scaledTime, unscaledTime, routine);

                if (!shouldContinue)
                    Stop(routine);

                CoroutinesWaitingForEndOfFrame.Remove(current);

                current = next;
            }
        }
    }

    [AddComponentMenu("")]
    internal class BetterCoroutinesEndOfFrame : MonoBehaviour
    {
        private Coroutine _endOfFrameCoroutine;

        [UsedImplicitly]
        private void OnEnable()
        {
            _endOfFrameCoroutine = StartCoroutine(EndOfFrameCoroutine());
        }

        [UsedImplicitly]
        private void OnDisable()
        {
            StopCoroutine(_endOfFrameCoroutine);
            _endOfFrameCoroutine = null;
        }

        private static IEnumerator EndOfFrameCoroutine()
        {
            while (true)
            {
                yield return BetterCoroutines.WaitForEndOfFrame;

                BetterCoroutines.ProcessEndOfFrame();
            }

            // ReSharper disable once IteratorNeverReturns
        }
    }
}
