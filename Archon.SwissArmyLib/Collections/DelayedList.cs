using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Archon.SwissArmyLib.Collections
{
    /// <summary>
    ///     A list wrapper that delays adding or removing item from the list until <see cref="ProcessPending" /> is called.
    /// </summary>
    /// <typeparam name="T">The type of items this list should contain.</typeparam>
    public class DelayedList<T> : IList<T>
    {
        private readonly IList<T> _items;
        private readonly Queue<PendingChange> _changeQueue;

        private readonly ReadOnlyCollection<T> _readonlyCollection;

        private enum Action
        {
            Add,
            Remove,
            Clear
        }

        private struct PendingChange
        {
            public readonly Action Action;
            public readonly T Value;

            public PendingChange(Action action) : this()
            {
                Action = action;
            }

            public PendingChange(Action action, T value) : this(action)
            {
                Value = value;
            }
        }

        /// <summary>
        ///     Gets the amount of items in the list.
        /// </summary>
        public int Count
        {
            get { return _items.Count; }
        }

        bool ICollection<T>.IsReadOnly { get { return false; } }

        /// <summary>
        ///     Creates a new DelayedList which uses <see cref="List{T}" />.
        /// </summary>
        public DelayedList() : this(new List<T>())
        {
        }

        /// <summary>
        ///     Creates a new DelayedList which uses <see cref="List{T}" /> and has the specified initial capacity and change capacity.
        /// </summary>
        /// <param name="capacity">The initial capacity of the list.</param>
        /// <param name="changeCapacity">The initial capacity of the change queue.</param>
        public DelayedList(int capacity, int changeCapacity) : this(new List<T>(capacity), changeCapacity)
        {

        }

        /// <summary>
        ///     Creates a new DelayedList that wraps the given list.
        /// </summary>
        /// <param name="list">The list to wrap.</param>
        public DelayedList(IList<T> list)
        {
            _items = list;
            _readonlyCollection = new ReadOnlyCollection<T>(_items);
            _changeQueue = new Queue<PendingChange>();
        }

        /// <summary>
        ///     Creates a new DelayedList that wraps the given list.
        /// </summary>
        /// <param name="list">The list to wrap.</param>
        /// <param name="changeCapacity">The initial capacity of the change queue.</param>
        public DelayedList(IList<T> list, int changeCapacity)
        {
            _items = list;
            _readonlyCollection = new ReadOnlyCollection<T>(_items);
            _changeQueue = new Queue<PendingChange>(changeCapacity);
        }

        /// <summary>
        ///     A readonly version of the list containing processed items.
        /// </summary>
        public ReadOnlyCollection<T> BackingList
        {
            get { return _readonlyCollection; }
        }

        /// <summary>
        ///     Gets the item at a specific index.
        /// </summary>
        /// <param name="index">The index of the item.</param>
        /// <returns>The item at the specified index.</returns>
        public T this[int index]
        {
            get { return _items[index]; }
        }

        T IList<T>.this[int index]
        {
            get { return _items[index]; }
            set { throw new NotSupportedException("Inserting at a specific index is not supported."); }
        }

        /// <summary>
        ///     Gets an enumerator for the backing list.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <summary>
        ///     Gets an enumerator for the backing list.
        /// </summary>
        /// <returns>The enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Adds an item to the list the next time <see cref="ProcessPending" /> is called.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(T item)
        {
            _changeQueue.Enqueue(new PendingChange(Action.Add, item));
        }

        /// <summary>
        ///     Adds multiple items to the list the next time <see cref="ProcessPending" /> is called.
        /// </summary>
        /// <param name="items">The items to add.</param>
        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
                Add(item);
        }

        /// <summary>
        ///     Removes an item from the list the next time <see cref="ProcessPending" /> is called.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public bool Remove(T item)
        {
            _changeQueue.Enqueue(new PendingChange(Action.Remove, item));
            return true;
        }

        /// <summary>
        ///     Clears all items from the list the next time <see cref="ProcessPending"/> is called.
        /// 
        ///     <seealso cref="ClearInstantly"/>
        ///     <seealso cref="ClearPending"/>
        /// </summary>
        public void Clear()
        {
            ClearPending();
            _changeQueue.Enqueue(new PendingChange(Action.Clear));
        }

        /// <summary>
        ///     Clears all items from the list and all pending additions and removals instantly, without having to call <see cref="ProcessPending"/>.
        /// 
        ///     <seealso cref="Clear"/>
        /// </summary>
        public void ClearInstantly()
        {
            _items.Clear();
            _changeQueue.Clear();
        }

        /// <summary>
        ///     Clears all pending changes.
        /// 
        ///     <seealso cref="Clear"/>
        /// </summary>
        public void ClearPending()
        {
            _changeQueue.Clear();
        }

        /// <summary>
        ///     Checks whether the backing list currently contains a specific item.
        /// </summary>
        /// <param name="item">The item to check for.</param>
        /// <returns>True if found, false otherwise.</returns>
        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        /// <summary>
        ///     Copies the contents of the backing list to the specified array starting at the specified index.
        /// </summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">The index to start from.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        ///     Gets the index of an item in the list.
        /// </summary>
        /// <param name="item">The item to get the index for.</param>
        /// <returns>The index of the item, or -1 if not found.</returns>
        public int IndexOf(T item)
        {
            return _items.IndexOf(item);
        }

        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException("Inserting at a specific index is not supported.");
        }

        /// <summary>
        ///     Removes the value currently found at the specified index the next time <see cref="ProcessPending" /> is called.
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            Remove(_items[index]);
        }

        /// <summary>
        ///     Processes all pending additions and removals.
        /// </summary>
        public void ProcessPending()
        {
            var changeCount = _changeQueue.Count;
            for (var i = 0; i < changeCount; i++)
            {
                var change = _changeQueue.Dequeue();

                switch (change.Action)
                {
                    case Action.Add:
                        _items.Add(change.Value);
                        break;
                    case Action.Remove:
                        _items.Remove(change.Value);
                        break;
                    case Action.Clear:
                        _items.Clear();
                        break;
                }
            }
        }
    }
}