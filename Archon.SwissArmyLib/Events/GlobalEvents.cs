using System;
using System.Collections.Generic;

namespace Archon.SwissArmyLib.Events
{
    /// <summary>
    /// A manager of events that do not belong to any specific object but instead can be listened to by anyone and invoked by anyone.
    /// 
    /// Useful for GameLoaded, MatchEnded and similar events.
    /// 
    /// This uses <see cref="Event"/> instances behind the scenes.
    /// 
    /// This version is for parameterless events. 
    /// See <see cref="GlobalEvents{T}"/> if you need to send data with the events.
    /// 
    /// Events are differentiated by an integer. You are expected to create constants to define your events.
    /// 
    /// <seealso cref="IEventListener"/>
    /// </summary>
    public static class GlobalEvents
    {
        private static readonly Dictionary<int, Event> Events = new Dictionary<int, Event>();

        /// <summary>
        /// Invokes an event.
        /// </summary>
        /// <param name="eventId">The id of the event.</param>
        public static void Invoke(int eventId)
        {
            Event e;
            if (Events.TryGetValue(eventId, out e))
                e.Invoke();
        }

        /// <summary>
        /// Adds a listener for an event.
        /// </summary>
        /// <param name="eventId">The id of the event.</param>
        /// <param name="listener">The listener to be called.</param>
        /// <param name="priority">The priority of the listener which affects the order which listeners are called in.</param>
        public static void AddListener(int eventId, IEventListener listener, int priority = 0)
        {
            if (ReferenceEquals(listener, null))
                throw new ArgumentNullException("listener");

            Event e;
            if (!Events.TryGetValue(eventId, out e))
                Events[eventId] = e = new Event(eventId);

            e.AddListener(listener, priority);
        }

        /// <summary>
        /// Adds a listener for an event.
        /// </summary>
        /// <param name="eventId">The id of the event.</param>
        /// <param name="listener">The listener to be called.</param>
        /// <param name="priority">The priority of the listener which affects the order which listeners are called in.</param>
        public static void AddListener(int eventId, Action listener, int priority = 0)
        {
            if (ReferenceEquals(listener, null))
                throw new ArgumentNullException("listener");

            Event e;
            if (!Events.TryGetValue(eventId, out e))
                Events[eventId] = e = new Event(eventId);

            e.AddListener(listener, priority);
        }

        /// <summary>
        /// Removes a listener for an event.
        /// </summary>
        /// <param name="eventId">The id of the event.</param>
        /// <param name="listener">The listener to remove.</param>
        public static void RemoveListener(int eventId, IEventListener listener)
        {
            if (ReferenceEquals(listener, null))
                throw new ArgumentNullException("listener");

            Event e;
            if (Events.TryGetValue(eventId, out e))
                e.RemoveListener(listener);
        }

        /// <summary>
        /// Removes a listener for an event.
        /// </summary>
        /// <param name="eventId">The id of the event.</param>
        /// <param name="listener">The listener to remove.</param>
        public static void RemoveListener(int eventId, Action listener)
        {
            if (ReferenceEquals(listener, null))
                throw new ArgumentNullException("listener");

            Event e;
            if (Events.TryGetValue(eventId, out e))
                e.RemoveListener(listener);
        }

        /// <summary>
        /// Removes the specified listener from all events.
        /// </summary>
        /// <param name="listener">The listener to unsubscribe from all events.</param>
        public static void RemoveListener(IEventListener listener)
        {
            if (ReferenceEquals(listener, null))
                throw new ArgumentNullException("listener");

            foreach (var eventId in Events.Keys)
                RemoveListener(eventId, listener);
        }

        /// <summary>
        /// Removes the specified listener from all events.
        /// </summary>
        /// <param name="listener">The listener to unsubscribe from all events.</param>
        public static void RemoveListener(Action listener)
        {
            if (ReferenceEquals(listener, null))
                throw new ArgumentNullException("listener");

            foreach (var eventId in Events.Keys)
                RemoveListener(eventId, listener);
        }

        /// <summary>
        /// Clears all listeners for all events.
        /// </summary>
        public static void Clear()
        {
            foreach (var listenerList in Events.Values)
                listenerList.Clear();
        }

        /// <summary>
        /// Clears all listeners for a single event.
        /// </summary>
        /// <param name="eventId">The id of the event.</param>
        public static void Clear(int eventId)
        {
            Event e;
            if (Events.TryGetValue(eventId, out e))
                e.Clear();
        }
    }

    /// <summary>
    /// A manager of events that do not belong to any specific object but instead can be listened to by anyone and invoked by anyone.
    /// 
    /// Useful for GameLoaded, MatchEnded and similar events.
    /// 
    /// This uses <see cref="Event{T}"/> instances behind the scenes.
    /// 
    /// This version is for events with args. 
    /// See <see cref="GlobalEvents"/> if you don't need to send data with the events.
    /// 
    /// Events are differentiated by an integer. You are expected to create constants to define your events.
    /// 
    /// <seealso cref="IEventListener{T}"/>
    /// </summary>
    public static class GlobalEvents<T>
    {
        private static readonly Dictionary<int, Event<T>> Events = new Dictionary<int, Event<T>>();

        /// <summary>
        /// Invokes an event.
        /// </summary>
        /// <param name="eventId">The id of the event.</param>
        /// <param name="args">The event args.</param>
        public static void Invoke(int eventId, T args)
        {
            Event<T> e;
            if (Events.TryGetValue(eventId, out e))
                e.Invoke(args);
        }

        /// <summary>
        /// Adds a listener for an event.
        /// </summary>
        /// <param name="eventId">The id of the event.</param>
        /// <param name="listener">The listener to be called.</param>
        /// <param name="priority">The priority of the listener which affects the order which listeners are called in.</param>
        public static void AddListener(int eventId, IEventListener<T> listener, int priority = 0)
        {
            if (ReferenceEquals(listener, null))
                throw new ArgumentNullException("listener");

            Event<T> e;
            if (!Events.TryGetValue(eventId, out e))
                Events[eventId] = e = new Event<T>(eventId);

            e.AddListener(listener, priority);
        }

        /// <summary>
        /// Adds a listener for an event.
        /// </summary>
        /// <param name="eventId">The id of the event.</param>
        /// <param name="listener">The listener to be called.</param>
        /// <param name="priority">The priority of the listener which affects the order which listeners are called in.</param>
        public static void AddListener(int eventId, Action<T> listener, int priority = 0)
        {
            if (ReferenceEquals(listener, null))
                throw new ArgumentNullException("listener");

            Event<T> e;
            if (!Events.TryGetValue(eventId, out e))
                Events[eventId] = e = new Event<T>(eventId);

            e.AddListener(listener, priority);
        }

        /// <summary>
        /// Removes a listener for an event.
        /// </summary>
        /// <param name="eventId">The id of the event.</param>
        /// <param name="listener">The listener to remove.</param>
        public static void RemoveListener(int eventId, IEventListener<T> listener)
        {
            if (ReferenceEquals(listener, null))
                throw new ArgumentNullException("listener");

            Event<T> e;
            if (Events.TryGetValue(eventId, out e))
                e.RemoveListener(listener);
        }

        /// <summary>
        /// Removes a listener for an event.
        /// </summary>
        /// <param name="eventId">The id of the event.</param>
        /// <param name="listener">The listener to remove.</param>
        public static void RemoveListener(int eventId, Action<T> listener)
        {
            if (ReferenceEquals(listener, null))
                throw new ArgumentNullException("listener");

            Event<T> e;
            if (Events.TryGetValue(eventId, out e))
                e.RemoveListener(listener);
        }

        /// <summary>
        /// Removes the specified listener from all events.
        /// </summary>
        /// <param name="listener">The listener to unsubscribe from all events.</param>
        public static void RemoveListener(IEventListener<T> listener)
        {
            foreach (var eventId in Events.Keys)
                RemoveListener(eventId, listener);
        }

        /// <summary>
        /// Removes the specified listener from all events.
        /// </summary>
        /// <param name="listener">The listener to unsubscribe from all events.</param>
        public static void RemoveListener(Action<T> listener)
        {
            foreach (var eventId in Events.Keys)
                RemoveListener(eventId, listener);
        }

        /// <summary>
        /// Clears all listeners for all events.
        /// </summary>
        public static void Clear()
        {
            foreach (var listenerList in Events.Values)
                listenerList.Clear();
        }

        /// <summary>
        /// Clears all listeners for a single event.
        /// </summary>
        /// <param name="eventId">The id of the event.</param>
        public static void Clear(int eventId)
        {
            Event<T> e;
            if (Events.TryGetValue(eventId, out e))
                e.Clear();
        }
    }
}
