using System;
using System.Collections.ObjectModel;
using Archon.SwissArmyLib.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Archon.SwissArmyLib.Events
{
    /// <summary>
    /// A simple event handler that supports using both interface and delegate listeners.
    /// 
    /// This is the parameterless version. 
    /// See <see cref="Event{T}"/> if you need to send data with the event.
    /// 
    /// Interface listeners are required to implement the <see cref="IEventListener"/> interface.
    /// 
    /// Events are differentiated by an integer. You are expected to create constants to define your events and make them unique.
    /// </summary>
    public class Event
    {
        private readonly DelayedList<PrioritizedItem<Listener>> _listeners;
        private bool _isIterating;
        private readonly int _id;

        /// <summary>
        /// Gets the ID of this event.
        /// </summary>
        public int Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Gets or sets whether listener exceptions should be logged. 
        /// </summary>
        public bool SuppressExceptions { get; set; }

        /// <summary>
        /// Gets a readonly collection of current listeners.
        /// </summary>
        public ReadOnlyCollection<PrioritizedItem<Listener>> Listeners
        {
            get
            {
                if (!_isIterating)
                    _listeners.ProcessPending();
                return _listeners.BackingList;
            }
        }

        /// <summary>
        /// Creates a new Event with the specified ID.
        /// </summary>
        /// <param name="id">The id of the event.</param>
        public Event(int id)
        {
            _id = id;
            _listeners = new DelayedList<PrioritizedItem<Listener>>(new PrioritizedList<Listener>());
        }

        /// <summary>
        /// Creates a new Event with the specified ID and initial listener capacity.
        /// </summary>
        /// <param name="id">The id of the event.</param>
        /// <param name="initialListenerCapacity">The initial capacity for listeners.</param>
        public Event(int id, int initialListenerCapacity)
        {
            _id = id;
            _listeners = new DelayedList<PrioritizedItem<Listener>>(new PrioritizedList<Listener>(initialListenerCapacity));
        }

        /// <summary>
        /// Adds a listener for the event with an optional call-order priority.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        /// <param name="priority">The priority of the listener compared to other listeners. Controls whether the listener is called before or after other listeners.</param>
        public void AddListener(IEventListener listener, int priority = 0)
        {
            if (ReferenceEquals(listener, null))
                throw new ArgumentNullException("listener");

            _listeners.Add(new PrioritizedItem<Listener>(new Listener(listener), priority));
        }

        /// <summary>
        /// Adds a listener for the event with an optional call-order priority.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        /// <param name="priority">The priority of the listener compared to other listeners. Controls whether the listener is called before or after other listeners.</param>
        public void AddListener(Action listener, int priority = 0)
        {
            if (ReferenceEquals(listener, null))
                throw new ArgumentNullException("listener");

            _listeners.Add(new PrioritizedItem<Listener>(new Listener(listener), priority));
        }

        /// <summary>
        /// Removes a listener from the event.
        /// </summary>
        /// <param name="listener">The listener to remove</param>
        public void RemoveListener(IEventListener listener)
        {
            if (ReferenceEquals(listener, null))
                throw new ArgumentNullException("listener");

            _listeners.Remove(new PrioritizedItem<Listener>(new Listener(listener), 0));
        }

        /// <summary>
        /// Removes a listener from the event.
        /// </summary>
        /// <param name="listener">The listener to remove</param>
        public void RemoveListener(Action listener)
        {
            if (ReferenceEquals(listener, null))
                throw new ArgumentNullException("listener");

            _listeners.Remove(new PrioritizedItem<Listener>(new Listener(listener), 0));
        }

        /// <summary>
        /// Checks whether the specified listener is currently listening to this event.
        /// </summary>
        /// <param name="listener">The listener to check.</param>
        /// <returns>True if listening, otherwise false.</returns>
        public bool HasListener(IEventListener listener)
        {
            if (ReferenceEquals(listener, null))
                throw new ArgumentNullException("listener");

            return HasListenerInternal(listener);
        }

        /// <summary>
        /// Checks whether the specified listener is currently listening to this event.
        /// </summary>
        /// <param name="listener">The listener to check.</param>
        /// <returns>True if listening, otherwise false.</returns>
        public bool HasListener(Action listener)
        {
            if (ReferenceEquals(listener, null))
                throw new ArgumentNullException("listener");

            return HasListenerInternal(listener);
        }

        private bool HasListenerInternal(object listener)
        {
            if (!_isIterating)
                _listeners.ProcessPending();

            var listenerCount = _listeners.Count;
            for (var i = 0; i < listenerCount; i++)
            {
                var current = _listeners[i];
                if (ReferenceEquals(current.Item.InterfaceListener, listener)
                    || ReferenceEquals(current.Item.DelegateListener, listener))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Notifies all listeners that the event occured.
        /// </summary>
        public void Invoke()
        {
            _listeners.ProcessPending();

            _isIterating = true;
            var listenerCount = _listeners.Count;
            for (var i = 0; i < listenerCount; i++)
            {
                var listener = _listeners[i].Item;

                // gotta wrap it up so one guy doesn't spoil it for everyone
                try
                {
                    if (listener.InterfaceListener != null)
                        listener.InterfaceListener.OnEvent(_id);
                    else
                        listener.DelegateListener();
                }
                catch (Exception e)
                {
                    if (!SuppressExceptions)
                    {
                        var context = listener.InterfaceListener as Object;
                        if (context != null)
                            Debug.LogException(e, context);
                        else
                            Debug.LogException(e);
                    }
                }
            }
            _isIterating = false;
        }

        /// <summary>
        /// Clears all listeners
        /// </summary>
        public void Clear()
        {
            _listeners.Clear();
        }

        /// <summary>
        /// Represents either a delegate or interface listener.
        /// </summary>
        public struct Listener : IEquatable<Listener>
        {
            /// <summary>
            /// Gets this listener's delegate reference.
            /// </summary>
            public readonly Action DelegateListener;

            /// <summary>
            /// Gets this listener's interface reference.
            /// </summary>
            public readonly IEventListener InterfaceListener;

            internal Listener(Action listener)
            {
                DelegateListener = listener;
                InterfaceListener = null;
            }

            internal Listener(IEventListener listener)
            {
                InterfaceListener = listener;
                DelegateListener = null;
            }

            /// <inheritdoc />
            public bool Equals(Listener other)
            {
                return ReferenceEquals(DelegateListener, other.DelegateListener) &&
                       ReferenceEquals(InterfaceListener, other.InterfaceListener);
            }

            /// <inheritdoc />
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is Listener && Equals((Listener) obj);
            }

            /// <inheritdoc />
            public override int GetHashCode()
            {
                unchecked
                {
                    return ((DelegateListener != null ? DelegateListener.GetHashCode() : 0) * 397) ^
                           (InterfaceListener != null ? InterfaceListener.GetHashCode() : 0);
                }
            }

            public static bool operator ==(Listener left, Listener right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Listener left, Listener right)
            {
                return !left.Equals(right);
            }
        }
    }

    /// <summary>
    /// A simple event handler that supports using both interface and delegate listeners.
    /// 
    /// This is the parameterized version. 
    /// See <see cref="Event"/> if you don't need to send data with the event.
    /// 
    /// Interface listeners are required to implement the <see cref="IEventListener{T}"/> interface.
    /// 
    /// Events are differentiated by an integer. You are expected to create constants to define your events and make them unique.
    /// </summary>
    public class Event<T>
    {
        private readonly DelayedList<PrioritizedItem<Listener>> _listeners;
        private bool _isIterating;
        private readonly int _id;

        /// <summary>
        /// Gets the ID of this event.
        /// </summary>
        public int Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Gets or sets whether listener exceptions should be logged. 
        /// </summary>
        public bool SuppressExceptions { get; set; }

        /// <summary>
        /// Gets a readonly collection of current listeners.
        /// </summary>
        public ReadOnlyCollection<PrioritizedItem<Listener>> Listeners
        {
            get
            {
                if (!_isIterating)
                    _listeners.ProcessPending();
                return _listeners.BackingList;
            }
        }

        /// <summary>
        /// Creates a new Event with the specified ID.
        /// </summary>
        /// <param name="id">The id of the event.</param>
        public Event(int id)
        {
            _id = id;
            _listeners = new DelayedList<PrioritizedItem<Listener>>(new PrioritizedList<Listener>());
        }

        /// <summary>
        /// Creates a new Event with the specified ID and initial listener capacity.
        /// </summary>
        /// <param name="id">The id of the event.</param>
        /// <param name="initialListenerCapacity">The initial capacity for listeners.</param>
        public Event(int id, int initialListenerCapacity)
        {
            _id = id;
            _listeners = new DelayedList<PrioritizedItem<Listener>>(new PrioritizedList<Listener>(initialListenerCapacity));
        }

        /// <summary>
        /// Adds a listener for the event with an optional call-order priority.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        /// <param name="priority">The priority of the listener compared to other listeners. Controls whether the listener is called before or after other listeners.</param>
        public void AddListener(IEventListener<T> listener, int priority = 0)
        {
            if (ReferenceEquals(listener, null))
                throw new ArgumentNullException("listener");

            _listeners.Add(new PrioritizedItem<Listener>(new Listener(listener), priority));
        }

        /// <summary>
        /// Adds a listener for the event with an optional call-order priority.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        /// <param name="priority">The priority of the listener compared to other listeners. Controls whether the listener is called before or after other listeners.</param>
        public void AddListener(Action<T> listener, int priority = 0)
        {
            if (ReferenceEquals(listener, null))
                throw new ArgumentNullException("listener");

            _listeners.Add(new PrioritizedItem<Listener>(new Listener(listener), priority));
        }

        /// <summary>
        /// Removes a listener from the event.
        /// </summary>
        /// <param name="listener">The listener to remove</param>
        public void RemoveListener(IEventListener<T> listener)
        {
            if (ReferenceEquals(listener, null))
                throw new ArgumentNullException("listener");

            _listeners.Remove(new PrioritizedItem<Listener>(new Listener(listener), 0));
        }

        /// <summary>
        /// Removes a listener from the event.
        /// </summary>
        /// <param name="listener">The listener to remove</param>
        public void RemoveListener(Action<T> listener)
        {
            if (ReferenceEquals(listener, null))
                throw new ArgumentNullException("listener");

            _listeners.Remove(new PrioritizedItem<Listener>(new Listener(listener), 0));
        }

        /// <summary>
        /// Checks whether the specified listener is currently listening to this event.
        /// </summary>
        /// <param name="listener">The listener to check.</param>
        /// <returns>True if listening, otherwise false.</returns>
        public bool HasListener(IEventListener<T> listener)
        {
            if (ReferenceEquals(listener, null))
                throw new ArgumentNullException("listener");

            return HasListenerInternal(listener);
        }

        /// <summary>
        /// Checks whether the specified listener is currently listening to this event.
        /// </summary>
        /// <param name="listener">The listener to check.</param>
        /// <returns>True if listening, otherwise false.</returns>
        public bool HasListener(Action<T> listener)
        {
            if (ReferenceEquals(listener, null))
                throw new ArgumentNullException("listener");

            return HasListenerInternal(listener);
        }

        private bool HasListenerInternal(object listener)
        {
            if (!_isIterating)
                _listeners.ProcessPending();

            var listenerCount = _listeners.Count;
            for (var i = 0; i < listenerCount; i++)
            {
                var current = _listeners[i];
                if (ReferenceEquals(current.Item.InterfaceListener, listener)
                    || ReferenceEquals(current.Item.DelegateListener, listener))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Notifies all listeners that the event occured.
        /// </summary>
        public void Invoke(T args)
        {
            _listeners.ProcessPending();

            _isIterating = true;
            var listenerCount = _listeners.Count;
            for (var i = 0; i < listenerCount; i++)
            {
                // gotta wrap it up so one guy doesn't spoil it for everyone
                try
                {
                    var listener = _listeners[i].Item;
                    if (listener.InterfaceListener != null)
                        listener.InterfaceListener.OnEvent(_id, args);
                    else
                        listener.DelegateListener(args);
                }
                catch (Exception e)
                {
                    if (!SuppressExceptions)
                        Debug.LogError(e);
                }
            }
            _isIterating = false;
        }

        /// <summary>
        /// Clears all listeners
        /// </summary>
        public void Clear()
        {
            _listeners.Clear();
        }

        /// <summary>
        /// Represents either a delegate or interface listener.
        /// </summary>
        public struct Listener : IEquatable<Listener>
        {
            /// <summary>
            /// Gets this listener's delegate reference.
            /// </summary>
            public readonly Action<T> DelegateListener;

            /// <summary>
            /// Gets this listener's interface reference.
            /// </summary>
            public readonly IEventListener<T> InterfaceListener;

            internal Listener(Action<T> listener)
            {
                DelegateListener = listener;
                InterfaceListener = null;
            }

            internal Listener(IEventListener<T> listener)
            {
                InterfaceListener = listener;
                DelegateListener = null;
            }

            /// <inheritdoc />
            public bool Equals(Listener other)
            {
                return ReferenceEquals(DelegateListener, other.DelegateListener) &&
                       ReferenceEquals(InterfaceListener, other.InterfaceListener);
            }

            /// <inheritdoc />
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is Listener && Equals((Listener) obj);
            }

            /// <inheritdoc />
            public override int GetHashCode()
            {
                unchecked
                {
                    return ((DelegateListener != null ? DelegateListener.GetHashCode() : 0) * 397) ^
                           (InterfaceListener != null ? InterfaceListener.GetHashCode() : 0);
                }
            }

            public static bool operator ==(Listener left, Listener right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Listener left, Listener right)
            {
                return !left.Equals(right);
            }
        }
    }
}