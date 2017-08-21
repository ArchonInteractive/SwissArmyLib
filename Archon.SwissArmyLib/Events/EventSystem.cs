using System;
using System.Collections.Generic;
using Archon.SwissArmyLib.Collections;
using UnityEngine;

namespace Archon.SwissArmyLib.Events
{
    public static class EventSystem
    {
        private static readonly Dictionary<int, PrioritizedList<IEventListener>> EventListeners = new Dictionary<int, PrioritizedList<IEventListener>>();

        public static void Invoke(int eventId)
        {
            PrioritizedList<IEventListener> listeners;
            if (EventListeners.TryGetValue(eventId, out listeners))
            {
                for (var i = 0; i < listeners.Count; i++)
                {
                    // gotta wrap it up so one guy doesn't spoil it for everyone
                    try
                    {
                        listeners[i].OnEvent(eventId);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                }
            }
        }

        public static void AddListener(int eventId, IEventListener listener, int priority = 0)
        {
            PrioritizedList<IEventListener> listeners;
            if (!EventListeners.TryGetValue(eventId, out listeners))
                EventListeners[eventId] = listeners = new PrioritizedList<IEventListener>();

            listeners.Add(listener, priority);
        }

        public static void RemoveListener(int eventId, IEventListener listener)
        {
            PrioritizedList<IEventListener> listeners;
            if (EventListeners.TryGetValue(eventId, out listeners))
                listeners.Remove(listener);
        }

        public static void Clear()
        {
            foreach (var listenerList in EventListeners.Values)
                listenerList.Clear();
        }

        public static void Clear(int eventId)
        {
            PrioritizedList<IEventListener> listeners;
            if (EventListeners.TryGetValue(eventId, out listeners))
                listeners.Clear();
        }
    }

    public static class EventSystem<T>
    {
        private static readonly Dictionary<int, PrioritizedList<IEventListener<T>>> EventListeners = new Dictionary<int, PrioritizedList<IEventListener<T>>>();

        public static void Invoke(int eventId, T args)
        {
            PrioritizedList<IEventListener<T>> listeners;
            if (EventListeners.TryGetValue(eventId, out listeners))
            {
                for (var i = 0; i < listeners.Count; i++)
                {
                    // gotta wrap it up so one guy doesn't spoil it for everyone
                    try
                    {
                        listeners[i].OnEvent(eventId, args);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                }
            }
        }

        public static void AddListener(int eventId, IEventListener<T> listener, int priority = 0)
        {
            PrioritizedList<IEventListener<T>> listeners;
            if (!EventListeners.TryGetValue(eventId, out listeners))
                EventListeners[eventId] = listeners = new PrioritizedList<IEventListener<T>>();

            listeners.Add(listener, priority);
        }

        public static void RemoveListener(int eventId, IEventListener<T> listener)
        {
            PrioritizedList<IEventListener<T>> listeners;
            if (EventListeners.TryGetValue(eventId, out listeners))
                listeners.Remove(listener);
        }

        public static void Clear()
        {
            foreach (var listenerList in EventListeners.Values)
                listenerList.Clear();
        }

        public static void Clear(int eventId)
        {
            PrioritizedList<IEventListener<T>> listeners;
            if (EventListeners.TryGetValue(eventId, out listeners))
                listeners.Clear();
        }
    }

    public interface IEventListener
    {
        void OnEvent(int eventId);
    }

    public interface IEventListener<in TArgs>
    {
        void OnEvent(int eventId, TArgs args);
    }
}
