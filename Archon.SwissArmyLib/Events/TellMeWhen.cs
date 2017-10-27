using System;
using System.Collections.Generic;
using Archon.SwissArmyLib.Collections;
using Archon.SwissArmyLib.Pooling;
using Archon.SwissArmyLib.Utils;
using UnityEngine;

namespace Archon.SwissArmyLib.Events
{
    /// <summary>
    /// A utility class for getting notified after a specific amount of time.
    /// </summary>
    public class TellMeWhen : IEventListener
    {
        /// <summary>
        /// Default id, if none is supplied
        /// </summary>
        public const int NoId = -1;

        private static readonly Pool<LinkedListNode<Entry>> SharedNodePool = new Pool<LinkedListNode<Entry>>(() => new LinkedListNode<Entry>(default(Entry)));
        private static readonly PooledLinkedList<Entry> EntriesScaled = new PooledLinkedList<Entry>(SharedNodePool);
        private static readonly PooledLinkedList<Entry> EntriesUnscaled = new PooledLinkedList<Entry>(SharedNodePool);

        static TellMeWhen()
        {
            var instance = new TellMeWhen();

            ServiceLocator.RegisterSingleton(instance);
            ServiceLocator.GlobalReset += () => ServiceLocator.RegisterSingleton(instance);
        }

        private TellMeWhen()
        {
            Loops.ManagedUpdate.OnUpdate.AddListener(this);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~TellMeWhen()
        {
            Loops.ManagedUpdate.OnUpdate.RemoveListener(this);
        }

        /// <summary>
        /// Schedule a callback to be called at a specific <see cref="Time.time"/>.
        /// </summary>
        /// <param name="time">The <see cref="Time.time"/> at which the callback should be called.</param>
        /// <param name="callback">The callback that will be notified.</param>
        /// <param name="id">An id so that you can reidentify the origin of the timer. Optional, but useful if you have more than one timer.</param>
        /// <param name="args">An optional args object that will be passed to the callback.</param>
        public static void Exact(float time, ITimerCallback callback, int id = NoId, object args = null)
        {
            if (ReferenceEquals(callback, null))
                throw new ArgumentNullException("callback");

            var entry = new Entry(time, callback, id, args);
            InsertIntoList(entry, EntriesScaled);
        }

        /// <summary>
        /// Schedule a callback to be called at a specific <see cref="Time.time"/> and repeatedly every <paramref name="repeatInterval"/> there after.
        /// </summary>
        /// <param name="time">The <see cref="Time.time"/> at which the callback should be called.</param>
        /// <param name="repeatInterval">The interval in seconds to repeat the timer.</param>
        /// <param name="callback">The callback that will be notified.</param>
        /// <param name="id">An id so that you can reidentify the origin of the timer. Optional, but useful if you have more than one timer.</param>
        /// <param name="args">An optional args object that will be passed to the callback.</param>
        public static void Exact(float time, float repeatInterval, ITimerCallback callback, int id = NoId, object args = null)
        {
            if (ReferenceEquals(callback, null))
                throw new ArgumentNullException("callback");

            var entry = new Entry(time, callback, id, args)
            {
                Repeating = true,
                RepeatingInterval = repeatInterval
            };

            InsertIntoList(entry, EntriesScaled);
        }

        /// <summary>
        /// Schedule a callback to be called after a specific amount of (scaled) seconds.
        /// </summary>
        /// <param name="seconds">The amount of seconds before the callback should be called.</param>
        /// <param name="callback">The callback that will be notified.</param>
        /// <param name="id">An id so that you can reidentify the origin of the timer. Optional, but useful if you have more than one timer.</param>
        /// <param name="args">An optional args object that will be passed to the callback.</param>
        /// <param name="repeating">Whether the timer should repeat untill cancelled.</param>
        public static void Seconds(float seconds, ITimerCallback callback, int id = NoId, object args = null, bool repeating = false)
        {
            if (repeating)
                Exact(BetterTime.Time + seconds, seconds, callback, id, args);
            else
                Exact(BetterTime.Time + seconds, callback, id, args);
        }

        /// <summary>
        /// Schedule a callback to be called after a specific amount of (scaled) minutes.
        /// </summary>
        /// <param name="minutes">The amount of minutes before the callback should be called.</param>
        /// <param name="callback">The callback that will be notified.</param>
        /// <param name="id">An id so that you can reidentify the origin of the timer. Optional, but useful if you have more than one timer.</param>
        /// <param name="args">An optional args object that will be passed to the callback.</param>
        /// <param name="repeating">Whether the timer should repeat untill cancelled.</param>
        public static void Minutes(float minutes, ITimerCallback callback, int id = NoId, object args = null, bool repeating = false)
        {
            Seconds(minutes * 60, callback, id, args, repeating);
        }

        /// <summary>
        /// Schedule a callback to be called at a specific <see cref="Time.unscaledTime"/>.
        /// </summary>
        /// <param name="time">The <see cref="Time.unscaledTime"/> at which the callback should be called.</param>
        /// <param name="callback">The callback that will be notified.</param>
        /// <param name="id">An id so that you can reidentify the origin of the timer. Optional, but useful if you have more than one timer.</param>
        /// <param name="args">An optional args object that will be passed to the callback.</param>
        public static void ExactUnscaled(float time, ITimerCallback callback, int id = NoId, object args = null)
        {
            if (ReferenceEquals(callback, null))
                throw new ArgumentNullException("callback");

            var entry = new Entry(time, callback, id, args);
            InsertIntoList(entry, EntriesUnscaled);
        }

        /// <summary>
        /// Schedule a callback to be called at a specific <see cref="Time.unscaledTime"/> and repeatedly every <paramref name="repeatInterval"/> there after.
        /// </summary>
        /// <param name="time">The <see cref="Time.unscaledTime"/> at which the callback should be called.</param>
        /// <param name="repeatInterval">The interval in seconds to repeat the timer.</param>
        /// <param name="callback">The callback that will be notified.</param>
        /// <param name="id">An id so that you can reidentify the origin of the timer. Optional, but useful if you have more than one timer.</param>
        /// <param name="args">An optional args object that will be passed to the callback.</param>
        public static void ExactUnscaled(float time, float repeatInterval, ITimerCallback callback, int id = NoId, object args = null)
        {
            if (ReferenceEquals(callback, null))
                throw new ArgumentNullException("callback");

            var entry = new Entry(time, callback, id, args)
            { 
                Repeating = true,
                RepeatingInterval = repeatInterval
            };

            InsertIntoList(entry, EntriesUnscaled);
        }

        /// <summary>
        /// Schedule a callback to be called after a specific amount of (unscaled) seconds.
        /// </summary>
        /// <param name="seconds">The amount of seconds before the callback should be called.</param>
        /// <param name="callback">The callback that will be notified.</param>
        /// <param name="id">An id so that you can reidentify the origin of the timer. Optional, but useful if you have more than one timer.</param>
        /// <param name="args">An optional args object that will be passed to the callback.</param>
        /// <param name="repeating">Whether the timer should repeat untill cancelled.</param>
        public static void SecondsUnscaled(float seconds, ITimerCallback callback, int id = NoId, object args = null, bool repeating = false)
        {
            if (repeating)
                ExactUnscaled(BetterTime.UnscaledTime + seconds, seconds, callback, id, args);
            else
                ExactUnscaled(BetterTime.UnscaledTime + seconds, callback, id, args);
        }

        /// <summary>
        /// Schedule a callback to be called after a specific amount of (unscaled) minutes.
        /// </summary>
        /// <param name="minutes">The amount of minutes before the callback should be called.</param>
        /// <param name="callback">The callback that will be notified.</param>
        /// <param name="id">An id so that you can reidentify the origin of the timer. Optional, but useful if you have more than one timer.</param>
        /// <param name="args">An optional args object that will be passed to the callback.</param>
        /// <param name="repeating">Whether the timer should repeat untill cancelled.</param>
        public static void MinutesUnscaled(float minutes, ITimerCallback callback, int id = NoId, object args = null, bool repeating = false)
        {
            SecondsUnscaled(minutes * 60, callback, id, args, repeating);
        }

        private static void CancelInternal(ITimerCallback callback, PooledLinkedList<Entry> list)
        {
            if (ReferenceEquals(callback, null))
                throw new ArgumentNullException("callback");

            var current = list.First;

            while (current != null)
            {
                var entry = current.Value;

                if (entry.Callback == callback)
                {
                    var next = current.Next;
                    list.Remove(current);
                    current = next;
                }
                else
                    current = current.Next;
            }
        }

        private static void CancelInternal(ITimerCallback callback, int id, PooledLinkedList<Entry> list)
        {
            if (ReferenceEquals(callback, null))
                throw new ArgumentNullException("callback");

            var current = list.First;

            while (current != null)
            {
                var entry = current.Value;

                if (entry.Callback == callback
                    && entry.Id == id)
                {
                    var next = current.Next;
                    list.Remove(current);
                    current = next;
                }
                else
                    current = current.Next;
            }
        }

        /// <summary>
        /// Cancels all scaled timers for the given callback.
        /// </summary>
        /// <param name="callback">The callback of the timers to cancel.</param>
        public static void CancelScaled(ITimerCallback callback)
        {
            CancelInternal(callback, EntriesScaled);
        }

        /// <summary>
        /// Cancels a scaled timer for the given callback with a specific id.
        /// </summary>
        /// <param name="callback">The callback of the timer to cancel.</param>
        /// <param name="id">The id of the timer to cancel.</param>
        public static void CancelScaled(ITimerCallback callback, int id)
        {
            CancelInternal(callback, id, EntriesScaled);
        }

        /// <summary>
        /// Cancels all unscaled timers for the given callback.
        /// </summary>
        /// <param name="callback">The callback of the timers to cancel.</param>
        public static void CancelUnscaled(ITimerCallback callback)
        {
            CancelInternal(callback, EntriesUnscaled);
        }

        /// <summary>
        /// Cancels a unscaled timer for the given callback with a specific id.
        /// </summary>
        /// <param name="callback">The callback of the timer to cancel.</param>
        /// <param name="id">The id of the timer to cancel.</param>
        public static void CancelUnscaled(ITimerCallback callback, int id)
        {
            CancelInternal(callback, id, EntriesUnscaled);
        }

        /// <summary>
        /// Cancels all (both scaled and unscaled) timers for all callbacks.
        /// </summary>
        public static void CancelAll()
        {
            EntriesScaled.Clear();
            EntriesUnscaled.Clear();
        }

        private static void UpdateList(float time, PooledLinkedList<Entry> list)
        {
            LinkedListNode<Entry> current;
            while ((current = list.First) != null)
            {
                var entry = current.Value;

                if (entry.Time > time) break;

                entry.Invoke();
                list.RemoveFirst();

                if (entry.Repeating)
                {
                    // Just in case the repeating interval is 0, we add a tiny bit to avoid an endless loop
                    entry.Time = time + entry.RepeatingInterval + 0.00001f;
                    InsertIntoList(entry, list);
                }
            }
        }

        private static void InsertIntoList(Entry entry, PooledLinkedList<Entry> list)
        {
            var current = list.First;

            while (current != null)
            {
                var otherEntry = current.Value;

                if (otherEntry.Time > entry.Time)
                {
                    list.AddBefore(current, entry);
                    return;
                }

                current = current.Next;
            }

            list.AddLast(entry);
        }

        private struct Entry
        {
            public int Id;
            public object Args;
            public ITimerCallback Callback;
            public float Time;
            public bool Repeating;
            public float RepeatingInterval;

            public Entry(float time, ITimerCallback callback, int id = NoId, object args = null)
            {
                Time = time;
                Callback = callback;
                Id = id;
                Args = args;
                Repeating = false;
                RepeatingInterval = 0;
            }

            public void Invoke()
            {
                try
                {
                    Callback.OnTimesUp(Id, Args);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        /// <summary>
        /// Defines a method to be used for timer events.
        /// </summary>
        public interface ITimerCallback
        {
            /// <summary>
            /// Called when a timer is triggered (eg. after X amount of time).
            /// </summary>
            /// <param name="id">The id of the timer.</param>
            /// <param name="args">The supplied event args if supplied when the timer was scheduled.</param>
            void OnTimesUp(int id, object args);
        }

        void IEventListener.OnEvent(int eventId)
        {
            if (eventId != Loops.ManagedUpdate.EventIds.Update) return;

            UpdateList(BetterTime.Time, EntriesScaled);
            UpdateList(BetterTime.UnscaledTime, EntriesUnscaled);
        }
    }
}
