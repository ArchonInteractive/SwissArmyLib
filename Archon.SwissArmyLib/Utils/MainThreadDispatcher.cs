using System;
using System.Collections.Generic;
using System.Threading;
using Archon.SwissArmyLib.Events.Loops;
using UnityEngine;

namespace Archon.SwissArmyLib.Utils
{
    /// <summary>
    /// A dispatcher for running actions on the main thread from other threads. 
    /// Useful for when you need to use the Unity API in your own threads, since most of the API requires to be run on the main thread.
    /// 
    /// Initialize the dispatcher by running <see cref="Initialize"/> from the main thread (eg. a MonoBehaviour).
    /// After initialization you can enqueue actions to run via <see cref="Enqueue"/>. The actions will be run in the next Unity update loop.
    /// </summary>
    public static class MainThreadDispatcher
    {
        /// <summary>
        /// Checks whether the current thread is the main thread.
        /// </summary>
        public static bool IsMainThread
        {
            get
            {
                EnsureInitialized();
                return Thread.CurrentThread == _mainThread;
            }
        }

        private static readonly Queue<Action> ActionQueue = new Queue<Action>();
        private static Thread _mainThread;

        /// <summary>
        /// Initializes the MainThreadDispatcher if not already done. 
        /// Make sure this is run from the main thread.
        /// </summary>
        public static void Initialize()
        {
            if (_mainThread != null)
                return;

            _mainThread = Thread.CurrentThread;
            ManagedUpdate.OnUpdate.AddListener(RunPendingActions);
        }

        /// <summary>
        /// Enqueues the action to be run on the main thread instead of the active thread.
        /// </summary>
        /// <param name="action">The action to enqueue.</param>
        public static void Enqueue(Action action)
        {
            EnsureInitialized();

            if (IsMainThread)
                Debug.LogWarning("Enqueue called from the main thread. Are you sure this is what you meant to do?");

            lock (ActionQueue)
            {
                ActionQueue.Enqueue(action);
            }
        }

        private static void EnsureInitialized()
        {
            if (_mainThread == null)
            {
                Debug.LogError("Dispatcher has not been initialized yet. Did you forget to call Initialize()?");
                throw new InvalidOperationException("Dispatcher is not initialized.");
            }
        }

        private static void RunPendingActions()
        {
            lock (ActionQueue)
            {
                while (ActionQueue.Count > 0)
                {
                    try
                    {
                        ActionQueue.Dequeue().Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }
        }
    }
}
