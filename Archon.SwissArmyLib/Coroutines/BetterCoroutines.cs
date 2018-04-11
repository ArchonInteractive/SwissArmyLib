using System;
using System.Collections;
using System.Collections.Generic;
using Archon.SwissArmyLib.Collections;
using Archon.SwissArmyLib.Events;
using Archon.SwissArmyLib.Events.Loops;
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
    /// You can however optionally link the coroutines to a gameobject or component.
    /// 
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

        private static readonly Pool<LinkedListNode<BetterCoroutine>> SharedNodePool = new Pool<LinkedListNode<BetterCoroutine>>(() => new LinkedListNode<BetterCoroutine>(null));

        private static readonly Dictionary<int, PooledLinkedList<BetterCoroutine>> UpdateLoopToCoroutines = new Dictionary<int, PooledLinkedList<BetterCoroutine>>();
        private static readonly PooledLinkedList<BetterCoroutine> CoroutinesWaitingForEndOfFrame = new PooledLinkedList<BetterCoroutine>(SharedNodePool);

        private static LinkedListNode<BetterCoroutine> _current;

        private static readonly DictionaryWithDefault<int, BetterCoroutine> IdToCoroutine = new DictionaryWithDefault<int, BetterCoroutine>();
        private static int _nextId = 1;

        private static readonly BetterCoroutines Instance = new BetterCoroutines();

        static BetterCoroutines()
        {
            ServiceLocator.RegisterSingleton(Instance);

            if (!ServiceLocator.IsRegistered<BetterCoroutinesEndOfFrame>())
                ServiceLocator.RegisterSingleton<BetterCoroutinesEndOfFrame>();

            ServiceLocator.GlobalReset += () =>
            {
                ServiceLocator.RegisterSingleton(Instance);
                ServiceLocator.RegisterSingleton<BetterCoroutinesEndOfFrame>();
            };
        }

        private BetterCoroutines()
        {

        }

        /// <summary>
        /// Starts a new coroutine.
        /// </summary>
        /// <param name="enumerator"></param>
        /// <param name="updateLoop">Which update loop should the coroutine be part of?</param>
        /// <returns>The id of the coroutine.</returns>
        public static int Start(IEnumerator enumerator, UpdateLoop updateLoop = UpdateLoop.Update)
        {
            return Start(enumerator, GetEventId(updateLoop));
        }

        /// <summary>
        /// Starts a new coroutine.
        /// </summary>
        /// <param name="enumerator"></param>
        /// <param name="updateLoopId">Which update loop should the coroutine be part of?</param>
        /// <returns>The id of the coroutine.</returns>
        public static int Start(IEnumerator enumerator, int updateLoopId)
        {
            if (ReferenceEquals(enumerator, null))
                throw new ArgumentNullException("enumerator");

            var coroutine = SpawnCoroutine(enumerator, updateLoopId);

            Start(coroutine);

            return coroutine.Id;
        }

        /// <summary>
        /// Starts a new coroutine and links its lifetime to a gameobject.
        /// The coroutine will be stopped when the linked gameobject is disabled or destroyed.
        /// </summary>
        /// <param name="enumerator"></param>
        /// <param name="linkedObject">Which gameobject to link the coroutine's lifetime with.</param>
        /// <param name="updateLoop">Which update loop should the coroutine be part of?</param>
        /// <returns>The id of the coroutine.</returns>
        public static int Start(IEnumerator enumerator, GameObject linkedObject,
            UpdateLoop updateLoop = UpdateLoop.Update)
        {
            return Start(enumerator, linkedObject, GetEventId(updateLoop));
        }

        /// <summary>
        /// Starts a new coroutine and links its lifetime to a gameobject.
        /// The coroutine will be stopped when the linked gameobject is disabled or destroyed.
        /// </summary>
        /// <param name="enumerator"></param>
        /// <param name="linkedObject">Which gameobject to link the coroutine's lifetime with.</param>
        /// <param name="updateLoopId">Which update loop should the coroutine be part of?</param>
        /// <returns>The id of the coroutine.</returns>
        public static int Start(IEnumerator enumerator, GameObject linkedObject,
            int updateLoopId)
        {
            if (ReferenceEquals(enumerator, null))
                throw new ArgumentNullException("enumerator");

            if (ReferenceEquals(linkedObject, null))
                throw new ArgumentNullException("linkedObject");

            var coroutine = SpawnCoroutine(enumerator, updateLoopId);
            coroutine.LinkedObject = linkedObject;
            coroutine.IsLinkedToObject = true;

            Start(coroutine);

            return coroutine.Id;
        }

        /// <summary>
        /// Starts a new coroutine and links its lifetime to a component.
        /// The coroutine will be stopped when the linked component is disabled or destroyed.
        /// </summary>
        /// <param name="enumerator"></param>
        /// <param name="linkedComponent">Which component to link the coroutine's lifetime with.</param>
        /// <param name="updateLoop">Which update loop should the coroutine be part of?</param>
        /// <returns>The id of the coroutine.</returns>
        public static int Start(IEnumerator enumerator, MonoBehaviour linkedComponent,
            UpdateLoop updateLoop = UpdateLoop.Update)
        {
            return Start(enumerator, linkedComponent, GetEventId(updateLoop));
        }

        /// <summary>
        /// Starts a new coroutine and links its lifetime to a component.
        /// The coroutine will be stopped when the linked component is disabled or destroyed.
        /// </summary>
        /// <param name="enumerator"></param>
        /// <param name="linkedComponent">Which component to link the coroutine's lifetime with.</param>
        /// <param name="updateLoopId">Which update loop should the coroutine be part of?</param>
        /// <returns>The id of the coroutine.</returns>
        public static int Start(IEnumerator enumerator, MonoBehaviour linkedComponent,
            int updateLoopId)
        {
            if (ReferenceEquals(enumerator, null))
                throw new ArgumentNullException("enumerator");

            if (ReferenceEquals(linkedComponent, null))
                throw new ArgumentNullException("linkedComponent");

            var coroutine = SpawnCoroutine(enumerator, updateLoopId);
            coroutine.LinkedComponent = linkedComponent;
            coroutine.IsLinkedToComponent = true;

            Start(coroutine);

            return coroutine.Id;
        }

        private static void Start(BetterCoroutine coroutine)
        {
            var scaledTime = GetTime(coroutine.UpdateLoopId, false);
            var unscaledTime = GetTime(coroutine.UpdateLoopId, true);

            IdToCoroutine[coroutine.Id] = coroutine;

            var list = GetList(coroutine.UpdateLoopId);
            if (list == null)
            {
                UpdateLoopToCoroutines[coroutine.UpdateLoopId] = list = new PooledLinkedList<BetterCoroutine>(SharedNodePool);
                ManagedUpdate.AddListener(coroutine.UpdateLoopId, Instance);
            }

            var node = list.AddFirst(coroutine);

            var prevCurrent = _current;
            _current = node;

            var continueRunning = UpdateCoroutine(scaledTime, unscaledTime, coroutine);
            if (!continueRunning)
            {
                IdToCoroutine.Remove(coroutine.Id);
                list.Remove(coroutine);
                if (coroutine.Parent != null)
                    coroutine.Parent.Child = null;
                PoolHelper<BetterCoroutine>.Despawn(coroutine);
            }

            _current = prevCurrent;
        }

        private static void StartChild(IEnumerator enumerator, BetterCoroutine parent)
        {
            var child = SpawnCoroutine(enumerator, parent.UpdateLoopId);
            child.Parent = parent;
            parent.Child = child;
            Start(child);
        }

        /// <summary>
        /// Checks whether a coroutine with the given ID is running.
        /// 
        /// A paused coroutine is still considered running.
        /// </summary>
        /// <param name="id">The id of the coroutine to check.</param>
        /// <returns>True if running, otherwise false.</returns>
        public static bool IsRunning(int id)
        {
            return id > 0 && IdToCoroutine.ContainsKey(id);
        }

        /// <summary>
        /// Pauses or unpauses a coroutine.
        /// </summary>
        /// <param name="id">The id of the coroutine.</param>
        /// <param name="paused">True to pause, false to unpause.</param>
        public static void SetPaused(int id, bool paused)
        {
            var coroutine = IdToCoroutine[id];

            if (coroutine == null)
                throw new ArgumentException("No coroutine is running with the specified ID", "id");

            if (coroutine.IsPaused == paused)
                return;

            coroutine.IsPaused = paused;

            var child = coroutine.Child;
            while (child != null)
            {
                child.IsParentPaused = paused;

                if (child.IsPaused)
                    break;

                child = child.Child;
            }
        }

        /// <summary>
        /// Checks whether a coroutine is currently paused either directly or because of a paused parent.
        /// </summary>
        /// <param name="id">Id of the coroutine.</param>
        /// <returns>True if paused or parent is paused, otherwise false.</returns>
        public static bool IsPaused(int id)
        {
            var coroutine = IdToCoroutine[id];

            if (coroutine == null)
                throw new ArgumentException("No coroutine is running with the specified ID", "id");

            return coroutine.IsPaused || coroutine.IsParentPaused;
        }

        /// <summary>
        /// Pauses a coroutine.
        /// </summary>
        /// <param name="id">Id of the coroutine to pause.</param>
        public static void Pause(int id)
        {
            SetPaused(id, true);
        }

        /// <summary>
        /// Unpauses a paused coroutine.
        /// </summary>
        /// <param name="id">Id of the coroutine to unpause.</param>
        public static void Unpause(int id)
        {
            SetPaused(id, false);
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

        private static void Stop(BetterCoroutine coroutine)
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
            foreach (var updateLoop in UpdateLoopToCoroutines.Keys)
                StopAll(updateLoop);
        }

        /// <summary>
        /// Stops all coroutines that are running in the specified update loop.
        /// </summary>
        public static void StopAll(UpdateLoop updateLoop)
        {
            StopAll(GetEventId(updateLoop));
        }

        /// <summary>
        /// Stops all coroutines that are running in the specified update loop.
        /// </summary>
        public static void StopAll(int updateLoopId)
        {
            var coroutines = GetList(updateLoopId);

            if (coroutines == null)
                return;

            var isInUpdate = _current != null && _current.Value.UpdateLoopId == updateLoopId;

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
            if (ReferenceEquals(operation, null))
                throw new ArgumentNullException("operation");

            return Coroutines.WaitForAsyncOperation.Create(operation);
        }

        /// <summary>
        /// Waits until the specified <see cref="WWW"/> object has finished.
        /// </summary>
        /// <param name="www">The WWW object to wait for.</param>
        /// <returns></returns>
        public static IEnumerator WaitForWWW(WWW www)
        {
            if (ReferenceEquals(www, null))
                throw new ArgumentNullException("www");

            return Coroutines.WaitForWWW.Create(www);
        }

        /// <summary>
        /// Waits until the given predicate returns true.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IEnumerator WaitUntil(Func<bool> predicate)
        {
            if (ReferenceEquals(predicate, null))
                throw new ArgumentNullException("predicate");

            return WaitUntilLite.Create(predicate);
        }

        /// <summary>
        /// Waits until the given predicate returns false.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IEnumerator WaitWhile(Func<bool> predicate)
        {
            if (ReferenceEquals(predicate, null))
                throw new ArgumentNullException("predicate");

            return WaitWhileLite.Create(predicate);
        }

        private static BetterCoroutine SpawnCoroutine(IEnumerator enumerator, int updateLoopId)
        {
            var coroutine = PoolHelper<BetterCoroutine>.Spawn();
            coroutine.Id = GetNextId();
            coroutine.Enumerator = enumerator;
            coroutine.UpdateLoopId = updateLoopId;
            return coroutine;
        }

        private static int GetNextId()
        {
            if (_nextId < 1)
                _nextId = 1;

            return _nextId++;
        }

        private static PooledLinkedList<BetterCoroutine> GetList(int updateLoopId)
        {
            PooledLinkedList<BetterCoroutine> list;
            UpdateLoopToCoroutines.TryGetValue(updateLoopId, out list);
            return list;
        }

        private static void Update(PooledLinkedList<BetterCoroutine> coroutines)
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

        // this method has gotten out of hand..
        private static bool UpdateCoroutine(float scaledTime, float unscaledTime, BetterCoroutine coroutine)
        {
            if (coroutine.IsDone)
                return false;

            if (coroutine.IsLinkedToObject && (!coroutine.LinkedObject || !coroutine.LinkedObject.activeInHierarchy))
                return false;

            if (coroutine.IsLinkedToComponent && (!coroutine.LinkedComponent || !coroutine.LinkedComponent.isActiveAndEnabled))
                return false;

            if (coroutine.IsPaused || coroutine.IsParentPaused)
                return true;

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

            var subroutineId = current as int?;
            if (subroutineId != null)
            {
                var subroutine = IdToCoroutine[subroutineId.Value];

                if (subroutine != null)
                {
                    coroutine.Child = subroutine;
                    subroutine.Parent = coroutine;
                }
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
            PooledLinkedList<BetterCoroutine> coroutineList;
            if (UpdateLoopToCoroutines.TryGetValue(eventId, out coroutineList))
                Update(coroutineList);
        }

        private static int GetEventId(UpdateLoop updateLoop)
        {
            switch (updateLoop)
            {
                case UpdateLoop.Update:
                    return ManagedUpdate.EventIds.Update;
                case UpdateLoop.LateUpdate:
                    return ManagedUpdate.EventIds.LateUpdate;
                case UpdateLoop.FixedUpdate:
                    return ManagedUpdate.EventIds.FixedUpdate;
                default:
                    throw new ArgumentOutOfRangeException("updateLoop", updateLoop, null);
            }
        }

        private static float GetTime(int updateLoopId, bool unscaled)
        {
            float currentTime;

            if (updateLoopId == ManagedUpdate.EventIds.FixedUpdate 
                || ManagedUpdate.GetParentLoopForid(updateLoopId) == UpdateLoop.FixedUpdate)
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

                if (routine.WaitingForEndOfFrame)
                {
                    routine.WaitingForEndOfFrame = false;

                    var scaledTime = GetTime(routine.UpdateLoopId, false);
                    var unscaledTime = GetTime(routine.UpdateLoopId, true);

                    var shouldContinue = UpdateCoroutine(scaledTime, unscaledTime, routine);

                    if (!shouldContinue)
                        Stop(routine);
                }

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
            var instance = ServiceLocator.Resolve<BetterCoroutinesEndOfFrame>();

            if (instance == null)
            {
                ServiceLocator.RegisterSingleton(this);
                _endOfFrameCoroutine = StartCoroutine(EndOfFrameCoroutine());
            }
            else if (instance != this)
                Destroy(this);
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
