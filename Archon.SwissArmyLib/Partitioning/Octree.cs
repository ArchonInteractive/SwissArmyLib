using System;
using System.Collections.Generic;
using Archon.SwissArmyLib.Pooling;
using UnityEngine;

namespace Archon.SwissArmyLib.Partitioning
{
    /// <summary>
    /// A GC-friendly <see href="https://en.wikipedia.org/wiki/Octree">Octree</see> implementation.
    /// 
    /// Use the static <see cref="Create"/> and <see cref="Destroy"/> methods for creating and destroying trees.
    /// If you forget to <see cref="Destroy"/> a tree when you're done with it, it will instead be collected 
    /// by the GC as normal, but the nodes will not be recycled.
    /// </summary>
    /// <typeparam name="T">The type of items this octree should hold.</typeparam>
    public class Octree<T> : IPoolable, IDisposable
    {
        // We can't use PoolHelper since the constructor is private
        private static readonly Pool<Octree<T>> Pool = new Pool<Octree<T>>(() => new Octree<T>());

        /// <summary>
        /// Creates an Octree.
        /// </summary>
        /// <param name="bounds">The size of the tree's bounds.</param>
        /// <param name="maxItems">The amount of items a node can contain before splitting.</param>
        /// <param name="maxDepth">The maximum depth</param>
        /// <returns>The octree.</returns>
        public static Octree<T> Create(Bounds bounds, int maxItems, int maxDepth)
        {
            var tree = Pool.Spawn();
            tree.Bounds = bounds;
            tree.MaxItems = maxItems;
            tree.MaxDepth = maxDepth;
            return tree;
        }

        private static Octree<T> CreateSubtree(Bounds bounds, int maxItems, int depth, int maxDepth)
        {
            var tree = Create(bounds, maxItems, maxDepth);
            tree.Depth = depth;
            return tree;
        }

        /// <summary>
        /// Destroys a tree, making it available for being recycled.
        /// 
        /// Do not use the tree again after calling this.
        /// </summary>
        /// <param name="tree">The tree to destroy.</param>
        public static void Destroy(Octree<T> tree)
        {
            Pool.Despawn(tree);
        }

        private struct ItemBounds
        {
            public readonly T Item;
            public readonly Bounds Bounds;

            public ItemBounds(T item, Bounds bounds)
            {
                Item = item;
                Bounds = bounds;
            }
        }

        private readonly Octree<T>[] _subNodes = new Octree<T>[8];
        private readonly List<ItemBounds> _items = new List<ItemBounds>();

        /// <summary>
        /// Gets the bounds of this octree node.
        /// </summary>
        public Bounds Bounds { get; private set; }

        /// <summary>
        /// Gets the depth of this octree node.
        /// </summary>
        public int Depth { get; private set; }

        /// <summary>
        /// Gets the maximum depth that the octree can go.
        /// </summary>
        public int MaxDepth { get; private set; }

        /// <summary>
        /// Gets the maximum amount of items this node can contain before splitting.
        /// </summary>
        public int MaxItems { get; private set; }

        /// <summary>
        /// Gets whether this node has been split.
        /// </summary>
        public bool IsSplit { get { return _subNodes[0] != null; } }

        /// <summary>
        /// Gets the total amount of items from this node and down.
        /// </summary>
        public int Count { get; private set; }

        private Octree()
        {

        }

        /// <summary>
        /// Inserts an item with the specified bounds into the octree.
        /// </summary>
        /// <param name="item">The item to insert.</param>
        /// <param name="bounds">The bounds of the item, used to place it correctly in the tree.</param>
        public void Insert(T item, Bounds bounds)
        {
            Count++;

            if (IsSplit)
            {
                var containingNodes = GetContainingNodes(bounds);
                if (containingNodes != SubNodes.None && containingNodes != SubNodes.All)
                {
                    for (var i = 0; i < _subNodes.Length; i++)
                    {
                        var subNode = 1 << i;
                        if (((byte)containingNodes & subNode) == subNode)
                            _subNodes[i].Insert(item, bounds);
                    }

                    return;
                }
            }

            _items.Add(new ItemBounds(item, bounds));

            if (!IsSplit
                && Depth < MaxDepth
                && _items.Count > MaxItems)
            {
                Split();
            }
        }

        /// <summary>
        /// Removes an item with the specified bounds from the octree.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="bounds">The bounds used for previously inserting this item.</param>
        /// <returns>True if item was found and removed, otherwise false.</returns>
        public bool Remove(T item, Bounds bounds)
        {
            if (IsSplit)
            {
                var containingNodes = GetContainingNodes(bounds);
                if (containingNodes != SubNodes.None && containingNodes != SubNodes.All)
                {
                    var removed = false;
                    for (var i = 0; i < _subNodes.Length; i++)
                    {
                        var subNode = 1 << i;
                        if (((byte) containingNodes & subNode) == subNode)
                            removed |= _subNodes[i].Remove(item, bounds);
                    }

                    if (removed)
                    {
                        Count--;
                        if (Count <= MaxItems)
                            Merge();
                    }

                    return removed;
                }
            }

            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i].Item.Equals(item))
                {
                    _items.RemoveAt(i);
                    Count--;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Retrieves all potential matches for the given <see cref="Bounds"/>.
        /// 
        /// Careful, this method creates a new <see cref="HashSet{T}"/>. 
        /// You might want to use <see cref="Retrieve(Bounds, HashSet{T})"/> instead if you call this often.
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns>Potential matches.</returns>
        public HashSet<T> Retrieve(Bounds bounds)
        {
            var results = new HashSet<T>();
            Retrieve(bounds, results);
            return results;
        }

        /// <summary>
        /// Retrieves all potential matches for the given <see cref="Bounds"/> and adds them to <paramref name="results"/>.
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="results">Where results will be added to.</param>
        public void Retrieve(Bounds bounds, HashSet<T> results)
        {
            if (IsSplit)
            {
                var containingNodes = GetContainingNodes(bounds);
                if (containingNodes != SubNodes.None)
                {
                    for (var i = 0; i < _subNodes.Length; i++)
                    {
                        var subNode = 1 << i;
                        if (((byte)containingNodes & subNode) == subNode)
                            _subNodes[i].Retrieve(bounds, results);
                    }
                }
            }

            for (var i = 0; i < _items.Count; i++)
                results.Add(_items[i].Item);
        }

        /// <summary>
        /// Clears all items in this node and removes subnodes.
        /// </summary>
        public void Clear()
        {
            _items.Clear();
            Count = 0;

            if (IsSplit)
            {
                for (var i = 0; i < _subNodes.Length; i++)
                {
                    Destroy(_subNodes[i]);
                    _subNodes[i] = null;
                }
            }
        }

        /// <summary>
        /// Destroys the octree using <see cref="Destroy"/>.
        /// 
        /// Do not use the tree after calling this!
        /// </summary>
        public void Dispose()
        {
            Destroy(this);
        }

        private void Split()
        {
            var newDepth = Depth + 1;
            var bounds = Bounds;

            var subNodeSize = bounds.size * 0.5f;

            for (var z = 0; z < 2; z++)
            {
                for (var y = 0; y < 2; y++)
                {
                    for (var x = 0; x < 2; x++)
                    {
                        var min = bounds.min + new Vector3(x * subNodeSize.x, y * subNodeSize.y, z * subNodeSize.z);
                        var max = min + subNodeSize;

                        var innerBounds = new Bounds();
                        innerBounds.SetMinMax(min, max);
                        _subNodes[z * 4 + y * 2 + x] = CreateSubtree(innerBounds, MaxItems, newDepth, MaxDepth);
                    }
                }
            }

            for (var i = _items.Count - 1; i >= 0; i--)
            {
                var item = _items[i];

                var containingNodes = GetContainingNodes(item.Bounds);

                if (containingNodes == SubNodes.None || containingNodes == SubNodes.All)
                    continue;

                for (var j = 0; j < _subNodes.Length; j++)
                {
                    var subNode = 1 << j;
                    if (((byte) containingNodes & subNode) == subNode)
                        _subNodes[j].Insert(item.Item, item.Bounds);
                }

                _items.RemoveAt(i);
            }
        }

        private void Merge()
        {
            for (var i = 0; i < _subNodes.Length; i++)
            {
                var subNode = _subNodes[i];

                if (subNode.IsSplit)
                    subNode.Merge();

                for (var j = 0; j < subNode._items.Count; j++)
                {
                    var item = subNode._items[j];
                    if (!_items.Contains(item))
                        _items.Add(item);
                }

                Destroy(subNode);
                _subNodes[i] = null;
            }
        }

        [Flags]
        private enum SubNodes
        {
            None = 0,
            FrontTopLeft = 1 << 0,
            FrontTopRight = 1 << 1,
            FrontBottomLeft = 1 << 2,
            FrontBottomRight = 1 << 3,
            BackTopLeft = 1 << 4,
            BackTopRight = 1 << 5,
            BackBottomLeft = 1 << 6,
            BackBottomRight = 1 << 7,

            Front = FrontTopLeft | FrontTopRight | FrontBottomLeft | FrontBottomRight,
            Back = ~Front,
            Top = FrontTopLeft | FrontTopRight | BackTopLeft | BackTopRight,
            Bottom = ~Top,
            Left = FrontTopLeft | FrontBottomLeft | BackTopLeft | BackBottomLeft,
            Right = ~Left,

            All = Front | Back
        }

        private SubNodes GetContainingNodes(Bounds target)
        {
            var subnodes = SubNodes.All;

            var center = Bounds.center;

            var front = target.min.z < center.z;
            var back = target.max.z > center.z;

            var bottom = target.min.y < center.y;
            var top = target.max.y > center.y;

            var left = target.min.x < center.x;
            var right = target.max.x > center.x;

            if (!front)
                subnodes &= ~SubNodes.Front;

            if (!back)
                subnodes &= ~SubNodes.Back;

            if (!bottom)
                subnodes &= ~SubNodes.Bottom;

            if (!top)
                subnodes &= ~SubNodes.Top;

            if (!left)
                subnodes &= ~SubNodes.Left;

            if (!right)
                subnodes &= ~SubNodes.Right;

            return subnodes;

        }

        void IPoolable.OnSpawned()
        {
            
        }

        void IPoolable.OnDespawned()
        {
            Clear();
            Depth = 0;
        }
    }
}
