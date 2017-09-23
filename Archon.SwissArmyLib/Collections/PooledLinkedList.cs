using System;
using System.Collections;
using System.Collections.Generic;
using Archon.SwissArmyLib.Pooling;

namespace Archon.SwissArmyLib.Collections
{
    /// <summary>
    ///     A wrapper for <see cref="LinkedList{T}" /> that recycles its <see cref="LinkedListNode{T}" /> instances to reduce
    ///     GC allocations.
    /// </summary>
    /// <typeparam name="T">Type of items the list should contain.</typeparam>
    public class PooledLinkedList<T> : ICollection<T>
    {
        private readonly LinkedList<T> _list;
        private readonly IPool<LinkedListNode<T>> _pool;

        /// <summary>
        ///     Initializes a new empty PooledLinkedList&lt;T&gt; with its own node pool.
        /// </summary>
        public PooledLinkedList()
            : this(new Pool<LinkedListNode<T>>(() => new LinkedListNode<T>(default(T))))
        {
        }

        /// <summary>
        ///     Initializes a new empty PooledLinkedList&lt;T&gt; that uses a specified node pool.
        /// </summary>
        /// <param name="nodePool">The pool that should be used spawning/despawning new nodes.</param>
        public PooledLinkedList(IPool<LinkedListNode<T>> nodePool)
        {
            if (nodePool == null)
                throw new ArgumentNullException("nodePool");

            _list = new LinkedList<T>();
            _pool = nodePool;
        }

        /// <summary>
        ///     Initializes a new PooledLinkedList&lt;T&gt; with its own node pool and the contents of the specified
        ///     <see cref="IEnumerable{T}" />.
        /// </summary>
        /// <param name="collection">A collection of items to fill with PooledLinkedList&lt;T&gt; with.</param>
        public PooledLinkedList(IEnumerable<T> collection)
            : this(collection, new Pool<LinkedListNode<T>>(() => new LinkedListNode<T>(default(T))))
        {
        }

        /// <summary>
        ///     Initializes a new PooledLinkedList&lt;T&gt; with a custom node pool and the contents of the specified
        ///     <see cref="IEnumerable{T}" />.
        /// </summary>
        /// <param name="collection">A collection of items to fill with PooledLinkedList&lt;T&gt; with.</param>
        /// <param name="nodePool">The pool that should be used spawning/despawning new nodes.</param>
        public PooledLinkedList(IEnumerable<T> collection, IPool<LinkedListNode<T>> nodePool)
            : this(nodePool)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            foreach (var item in collection)
                AddLast(item);
        }

        /// <summary>
        ///     Gets the <see cref="LinkedList{T}" /> instance that is wrapped.
        /// </summary>
        public LinkedList<T> BackingList
        {
            get { return _list; }
        }

        /// <summary>
        ///     Gets the object pool used for storing unused nodes.
        /// </summary>
        public IPool<LinkedListNode<T>> Pool
        {
            get { return _pool; }
        }

        /// <summary>
        ///     Gets the first node of the PooledLinkedList&lt;T&gt;.
        /// </summary>
        public LinkedListNode<T> First
        {
            get { return _list.First; }
        }

        /// <summary>
        ///     Gets the last node of the PooledLinkedList&lt;T&gt;.
        /// </summary>
        public LinkedListNode<T> Last
        {
            get { return _list.Last; }
        }

        /// <summary>
        ///     Gets the number of nodes contained in the PooledLinkedList&lt;T&gt;.
        /// </summary>
        public int Count
        {
            get { return _list.Count; }
        }

        /// <summary>
        ///     Removes all nodes from the PooledLinkedList&lt;T&gt;.
        /// </summary>
        public void Clear()
        {
            var current = First;
            while (current != null)
            {
                var temp = current;
                current = current.Next;
                _pool.Despawn(temp);
            }

            _list.Clear();
        }

        /// <summary>
        ///     Determines whether a value is in the PooledLinkedList&lt;T&gt;.
        /// </summary>
        /// <param name="item">The item to check for.</param>
        /// <returns>True if the list contains the item, otherwise false.</returns>
        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        /// <summary>
        ///     Copies the entire PooledLinkedList&lt;T&gt; to a compatible one-dimensional Array, starting at the specified index
        ///     of the target array.
        /// </summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">The index to start at.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        ///     Removes the first occurrence of the specified value from the PooledLinkedList&lt;T&gt;.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>True if found and removed, otherwise false.</returns>
        public bool Remove(T item)
        {
            var node = _list.Find(item);

            if (node != null)
            {
                _list.Remove(node);
                _pool.Despawn(node);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the PooledLinkedList&lt;T&gt;.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _list).GetEnumerator();
        }

        void ICollection<T>.Add(T item)
        {
            AddLast(item);
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        ///     Adds a new node containing the specified value after the specified existing node in the PooledLinkedList&lt;T&gt;.
        /// </summary>
        /// <param name="node">The node that <paramref name="value" /> should be added after.</param>
        /// <param name="value">The value to add.</param>
        /// <returns>The added node.</returns>
        public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T value)
        {
            var newNode = _pool.Spawn();
            newNode.Value = value;

            AddAfter(node, newNode);

            return newNode;
        }

        /// <summary>
        ///     Adds the specified new node after the specified existing node in the PooledLinkedList&lt;T&gt;.
        /// </summary>
        /// <param name="node">The node that <paramref name="newNode" /> should be added after.</param>
        /// <param name="newNode">The node to add.</param>
        public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)
        {
            _list.AddAfter(node, newNode);
        }

        /// <summary>
        ///     Adds a new node containing the specified value before the specified existing node in the PooledLinkedList&lt;T&gt;.
        /// </summary>
        /// <param name="node">The node that <paramref name="value" /> should be added before.</param>
        /// <param name="value">The value to add.</param>
        /// <returns>The added node.</returns>
        public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T value)
        {
            var newNode = _pool.Spawn();
            newNode.Value = value;

            AddBefore(node, newNode);

            return newNode;
        }

        /// <summary>
        ///     Adds the specified new node before the specified existing node in the PooledLinkedList&lt;T&gt;.
        /// </summary>
        /// <param name="node">The node that <paramref name="newNode" /> should be added before.</param>
        /// <param name="newNode">The node to add.</param>
        public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
        {
            _list.AddBefore(node, newNode);
        }

        /// <summary>
        ///     Adds a new node containing the specified value at the start of the PooledLinkedList&lt;T&gt;.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>The node that was added.</returns>
        public LinkedListNode<T> AddFirst(T value)
        {
            var newNode = _pool.Spawn();
            newNode.Value = value;

            AddFirst(newNode);

            return newNode;
        }

        /// <summary>
        ///     Adds the specified new node at the start of the PooledLinkedList&lt;T&gt;.
        /// </summary>
        /// <param name="node">The node to add.</param>
        public void AddFirst(LinkedListNode<T> node)
        {
            _list.AddFirst(node);
        }

        /// <summary>
        ///     Adds a new node containing the specified value at the end of the PooledLinkedList&lt;T&gt;.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>The node that was added.</returns>
        public LinkedListNode<T> AddLast(T value)
        {
            var newNode = _pool.Spawn();
            newNode.Value = value;

            AddLast(newNode);

            return newNode;
        }

        /// <summary>
        ///     Adds the specified new node at the end of the PooledLinkedList&lt;T&gt;.
        /// </summary>
        /// <param name="node">The node to add.</param>
        public void AddLast(LinkedListNode<T> node)
        {
            _list.AddLast(node);
        }

        /// <summary>
        ///     Finds the first node that contains the specified value.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <returns>The found node or null if none were found.</returns>
        public LinkedListNode<T> Find(T value)
        {
            return _list.Find(value);
        }

        /// <summary>
        ///     Finds the last node that contains the specified value.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <returns>The found node or null if none were found.</returns>
        public LinkedListNode<T> FindLast(T value)
        {
            return _list.FindLast(value);
        }

        /// <summary>
        ///     Removes the specified node from the PooledLinkedList&lt;T&gt;.
        /// </summary>
        /// <param name="node">The node to remove.</param>
        public void Remove(LinkedListNode<T> node)
        {
            _list.Remove(node);
            _pool.Despawn(node);
        }


        /// <summary>
        ///     Removes the node at the start of the PooledLinkedList&lt;T&gt;.
        /// </summary>
        public void RemoveFirst()
        {
            var first = First;
            _list.RemoveFirst();
            _pool.Despawn(first);
        }

        /// <summary>
        ///     Removes the node at the end of the PooledLinkedList&lt;T&gt;.
        /// </summary>
        public void RemoveLast()
        {
            var last = Last;
            _list.RemoveLast();
            _pool.Despawn(last);
        }
    }
}