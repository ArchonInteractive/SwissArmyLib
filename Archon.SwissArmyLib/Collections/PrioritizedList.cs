using System;
using System.Collections;
using System.Collections.Generic;

namespace Archon.SwissArmyLib.Collections
{
    /// <summary>
    /// Represents an item and its priority.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    public struct PrioritizedItem<T> : IEquatable<PrioritizedItem<T>>
    {
        /// <summary>
        /// The item that is prioritized.
        /// </summary>
        public readonly T Item;

        /// <summary>
        /// The priority of the item.
        /// </summary>
        public readonly int Priority;

        /// <summary>
        /// Creates a new prioritized item.
        /// </summary>
        public PrioritizedItem(T item, int priority)
        {
            Item = item;
            Priority = priority;
        }

        /// <inheritdoc />
        public bool Equals(PrioritizedItem<T> other)
        {
            if (default(T) == null)
                return ReferenceEquals(Item, other.Item);

            return EqualityComparer<T>.Default.Equals(Item, other.Item);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is PrioritizedItem<T> && Equals((PrioritizedItem<T>) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(Item);
        }

        public static bool operator ==(PrioritizedItem<T> left, PrioritizedItem<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PrioritizedItem<T> left, PrioritizedItem<T> right)
        {
            return !left.Equals(right);
        }
    }

    /// <summary>
    /// Specifies the direction of a sort operation.
    /// </summary>
    public enum ListSortDirection
    {
        /// <summary>
        /// Sorts in ascending order.
        /// </summary>
        Ascending,

        /// <summary>
        /// Sorts in descending order.
        /// </summary>
        Descending,
    }

    /// <summary>
    /// A list of items sorted by their priority.
    /// </summary>
    /// <typeparam name="T">The type of the prioritized items.</typeparam>
    public class PrioritizedList<T> : IList<PrioritizedItem<T>>
    {
        /// <summary>
        /// Gets the sorting direction used for prioritized items in this list.
        /// 
        /// <b>Ascending</b>: Lower priorities are placed first.
        /// <b>Descending</b>: Lower priorities are placed last.
        /// </summary>
        public readonly ListSortDirection SortDirection;

        private readonly List<PrioritizedItem<T>> _items;

        /// <summary>
        /// Gets the amount of items in the list.
        /// </summary>
        public int Count { get { return _items.Count; }}

        bool ICollection<PrioritizedItem<T>>.IsReadOnly { get { return false; } }

        /// <summary>
        /// Gets the item at the specified index.
        /// </summary>
        /// <param name="index">The index for the item to retrieve.</param>
        /// <returns>The item at the specified index.</returns>
        public PrioritizedItem<T> this[int index]
        {
            get { return _items[index]; }
        }

        PrioritizedItem<T> IList<PrioritizedItem<T>>.this[int index]
        {
            get { return _items[index]; }
            set
            {
                throw new NotImplementedException("Setting a specific index is not supported.");
            }
        }

        /// <summary>
        /// Creates a new PrioritizedList with the specified sort direction.
        /// </summary>
        /// <param name="sortDirection">Whether items with lower priorities are placed first in the list (ascending) or last (descending).</param>
        public PrioritizedList(ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            SortDirection = sortDirection;
            _items = new List<PrioritizedItem<T>>();
        }

        /// <summary>
        /// Creates a new PrioritizedList with the specified sort direction and initial capacity.
        /// </summary>
        /// <param name="capacity">The number of elements that the new list can initially store.</param>
        /// <param name="sortDirection">Whether items with lower priorities are placed first in the list (ascending) or last (descending).</param>
        public PrioritizedList(int capacity, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            SortDirection = sortDirection;
            _items = new List<PrioritizedItem<T>>(capacity);
        }

        /// <summary>
        /// Adds a prioritized item to the list.
        /// </summary>
        /// <param name="item">The prioritized item to add.</param>
        public void Add(PrioritizedItem<T> item)
        {
            var itemCount = _items.Count;

            if (itemCount == 0)
            {
                _items.Add(item);
                return;
            }

            var lastItemPriority = _items[itemCount - 1].Priority;

            if (SortDirection == ListSortDirection.Ascending && item.Priority >= lastItemPriority
                || SortDirection == ListSortDirection.Descending && item.Priority <= lastItemPriority)
            {
                _items.Add(item);
                return;
            }

            for (var i = 0; i < itemCount; i++)
            {
                if (SortDirection == ListSortDirection.Ascending && item.Priority < _items[i].Priority
                    || SortDirection == ListSortDirection.Descending && item.Priority > _items[i].Priority)
                {
                    _items.Insert(i, item);
                    return;
                }
            }
        }

        /// <summary>
        /// Adds an item to the list with priority 0.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(T item)
        {
            Add(item, 0);
        }

        /// <summary>
        /// Adds an item to the list with the specified priority.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="priority">The priority to give the item.</param>
        public void Add(T item, int priority)
        {
            var prioritizedItem = new PrioritizedItem<T>(item, priority);
            Add(prioritizedItem);
        }

        /// <summary>
        /// Removes a prioritized item from the list.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>True if found and removed, false otherwise.</returns>
        public bool Remove(PrioritizedItem<T> item)
        {
            var comparer = EqualityComparer<PrioritizedItem<T>>.Default;

            var itemCount = _items.Count;
            for (var i = 0; i < itemCount; i++)
            {
                if (comparer.Equals(item, _items[i]))
                {
                    _items.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes an item from the list.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>True if found and removed, false otherwise.</returns>
        public bool Remove(T item)
        {
            var comparer = EqualityComparer<T>.Default;

            var itemCount = _items.Count;
            for (var i = 0; i < itemCount; i++)
            {
                if (comparer.Equals(item, _items[i].Item))
                {
                    _items.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes the item found at the specified index.
        /// </summary>
        /// <param name="index">The index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
        }

        /// <summary>
        /// Clears all items from the list.
        /// </summary>
        public void Clear()
        {
            _items.Clear();
        }

        /// <summary>
        /// Checks whether the list contains the specified prioritized item.
        /// </summary>
        /// <param name="item">The item to check if the list contains.</param>
        /// <returns>True if found, false otherwise.</returns>
        public bool Contains(PrioritizedItem<T> item)
        {
            var comparer = EqualityComparer<PrioritizedItem<T>>.Default;

            var itemCount = _items.Count;
            for (var i = 0; i < itemCount; i++)
            {
                if (comparer.Equals(item, _items[i]))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks whether the list contains the specified item.
        /// </summary>
        /// <param name="item">The item to check if the list contains.</param>
        /// <returns>True if found, false otherwise.</returns>
        public bool Contains(T item)
        {
            var comparer = EqualityComparer<T>.Default;

            var itemCount = _items.Count;
            for (var i = 0; i < itemCount; i++)
            {
                if (comparer.Equals(item, _items[i].Item))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Copies the list prioritized items to an array starting at the specified index.
        /// </summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">The index to start at.</param>
        public void CopyTo(PrioritizedItem<T>[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Copies the list items to an array starting at the specified index.
        /// </summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">The index to start at.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            var arrayLength = array.Length;
            var itemCount = _items.Count;
            for (var i = arrayIndex; i < arrayLength && i < itemCount; i++)
                array[i] = _items[i].Item;
        }

        /// <summary>
        /// Gets the index of a prioritized item in the list.
        /// </summary>
        /// <param name="item">The item to get the index for.</param>
        /// <returns>The index of the item in the list or -1 if not found.</returns>
        public int IndexOf(PrioritizedItem<T> item)
        {
            var comparer = EqualityComparer<PrioritizedItem<T>>.Default;

            for (var i = 0; i < _items.Count; i++)
            {
                if (comparer.Equals(item, _items[i]))
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Gets the index of a item in the list.
        /// </summary>
        /// <param name="item">The item to get the index for.</param>
        /// <returns>The index of the item in the list or -1 if not found.</returns>
        public int IndexOf(T item)
        {
            var comparer = EqualityComparer<T>.Default;

            for (var i = 0; i < _items.Count; i++)
            {
                if (comparer.Equals(item, _items[i].Item))
                    return i;
            }

            return -1;
        }

        void IList<PrioritizedItem<T>>.Insert(int index, PrioritizedItem<T> item)
        {
            throw new NotImplementedException("Inserting at a specific index is not supported.");
        }

        /// <summary>
        /// Gets an enumerator for the list items.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<PrioritizedItem<T>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
