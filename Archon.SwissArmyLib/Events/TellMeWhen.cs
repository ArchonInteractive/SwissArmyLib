using System;
using System.Collections.Generic;
using UnityEngine;

namespace Archon.SwissArmyLib.Events
{
    public class TellMeWhen : MonoBehaviour
    {
        public const int NoId = -1;

        private static readonly LinkedList<Entry> EntriesScaled = new LinkedList<Entry>();
        private static readonly LinkedList<Entry> EntriesUnscaled = new LinkedList<Entry>();

        private static TellMeWhen Instance { get; set; }

        static TellMeWhen()
        {
            var gameObject = new GameObject("TellMeWhen");
            DontDestroyOnLoad(gameObject);
            Instance = gameObject.AddComponent<TellMeWhen>();
        }

        public static void Exact(float time, ITimerCallback callback, int id = NoId, object args = null)
        {
            var entry = new Entry(time, callback, id, args);
            InsertIntoList(entry, EntriesScaled);
        }

        public static void Exact(float time, float repeatInterval, ITimerCallback callback, int id = NoId, object args = null)
        {
            var entry = new Entry(time, callback, id, args)
            {
                Repeating = true,
                RepeatingInterval = repeatInterval
            };

            InsertIntoList(entry, EntriesScaled);
        }

        public static void Seconds(float seconds, ITimerCallback callback, int id = NoId, object args = null, bool repeating = false)
        {
            if (repeating)
                Exact(Time.time + seconds, seconds, callback, id, args);
            else
                Exact(Time.time + seconds, callback, id, args);
        }

        public static void Minutes(float minutes, ITimerCallback callback, int id = NoId, object args = null, bool repeating = false)
        {
            Seconds(minutes * 60, callback, id, args, repeating);
        }

        public static void Hours(float hours, ITimerCallback callback, int id = NoId, object args = null, bool repeating = false)
        {
            Seconds(hours * 60 * 60, callback, id, args, repeating);
        }

        public static void ExactUnscaled(float time, ITimerCallback callback, int id = NoId, object args = null)
        {
            var entry = new Entry(time, callback, id, args);
            InsertIntoList(entry, EntriesUnscaled);
        }

        public static void ExactUnscaled(float time, float repeatInterval, ITimerCallback callback, int id = NoId, object args = null)
        {
            var entry = new Entry(time, callback, id, args)
            { 
                Repeating = true,
                RepeatingInterval = repeatInterval
            };

            InsertIntoList(entry, EntriesUnscaled);
        }

        public static void SecondsUnscaled(float seconds, ITimerCallback callback, int id = NoId, object args = null, bool repeating = false)
        {
            if (repeating)
                ExactUnscaled(Time.unscaledTime + seconds, seconds, callback, id, args);
            else
                ExactUnscaled(Time.unscaledTime + seconds, callback, id, args);
        }

        public static void MinutesUnscaled(float minutes, ITimerCallback callback, int id = NoId, object args = null, bool repeating = false)
        {
            SecondsUnscaled(minutes * 60, callback, id, args, repeating);
        }

        public static void HoursUnscaled(float hours, ITimerCallback callback, int id = NoId, object args = null, bool repeating = false)
        {
            SecondsUnscaled(hours * 60 * 60, callback, id, args, repeating);
        }

        private static void CancelInternal(ITimerCallback callback, LinkedList<Entry> list)
        {
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

        private static void CancelInternal(ITimerCallback callback, int id, LinkedList<Entry> list)
        {
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

        public static void CancelScaled(ITimerCallback callback)
        {
            CancelInternal(callback, EntriesScaled);
        }

        public static void CancelScaled(ITimerCallback callback, int id)
        {
            CancelInternal(callback, id, EntriesScaled);
        }

        public static void CancelUnscaled(ITimerCallback callback)
        {
            CancelInternal(callback, EntriesUnscaled);
        }

        public static void CancelUnscaled(ITimerCallback callback, int id)
        {
            CancelInternal(callback, id, EntriesUnscaled);
        }

        public static void CancelAll()
        {
            EntriesScaled.Clear();
            EntriesUnscaled.Clear();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("You shouldn't add TellMeWhen to a GameObject manually.");
                Destroy(this);
            }
        }

        private void Update()
        {
            UpdateList(Time.time, EntriesScaled);
            UpdateList(Time.unscaledTime, EntriesUnscaled);
        }

        private static void UpdateList(float time, LinkedList<Entry> list)
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

        private static void InsertIntoList(Entry entry, LinkedList<Entry> list)
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
                    Debug.LogError(e.Message);
                }
            }
        }

        public interface ITimerCallback
        {
            void OnTimesUp(int id, object args);
        }
    }
}
