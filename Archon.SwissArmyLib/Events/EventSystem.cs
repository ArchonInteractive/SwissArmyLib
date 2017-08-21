using System;
using System.Collections.Generic;
using UnityEngine;

namespace Archon.SwissArmyLib.Events
{
    public static class EventSystem
    {
        private static readonly Dictionary<int, List<IEventListener>> EventListeners = new Dictionary<int, List<IEventListener>>();

        public static void Invoke(int eventId)
        {
            List<IEventListener> listeners;
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

        public static void AddListener(int eventId, IEventListener listener)
        {
            List<IEventListener> listeners;
            if (!EventListeners.TryGetValue(eventId, out listeners))
                EventListeners[eventId] = listeners = new List<IEventListener>();

            listeners.Add(listener);
        }

        public static void RemoveListener(int eventId, IEventListener listener)
        {
            List<IEventListener> listeners;
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
            List<IEventListener> listeners;
            if (EventListeners.TryGetValue(eventId, out listeners))
                listeners.Clear();
        }
    }

    public static class EventSystem<T>
    {
        private static readonly Dictionary<int, List<IEventListener<T>>> EventListeners = new Dictionary<int, List<IEventListener<T>>>();

        public static void Invoke(int eventId, T args)
        {
            List<IEventListener<T>> listeners;
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

        public static void AddListener(int eventId, IEventListener<T> listener)
        {
            List<IEventListener<T>> listeners;
            if (!EventListeners.TryGetValue(eventId, out listeners))
                EventListeners[eventId] = listeners = new List<IEventListener<T>>();

            listeners.Add(listener);
        }

        public static void RemoveListener(int eventId, IEventListener<T> listener)
        {
            List<IEventListener<T>> listeners;
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
            List<IEventListener<T>> listeners;
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
