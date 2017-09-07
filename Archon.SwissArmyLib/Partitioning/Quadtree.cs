using System;
using System.Collections.Generic;
using Archon.SwissArmyLib.Pooling;
using UnityEngine;

namespace Archon.SwissArmyLib.Partitioning
{
    /// <summary>
    /// A GC-friendly <see href="https://en.wikipedia.org/wiki/Quadtree">Quadtree</see> implementation.
    /// 
    /// Use the static <see cref="Create"/> and <see cref="Destroy"/> methods for creating and destroying trees.
    /// If you forget to <see cref="Destroy"/> a tree when you're done with it, it will instead be collected 
    /// by the GC as normal, but the nodes will not be recycled.
    /// </summary>
    /// <typeparam name="T">The type of items this quadtree should hold.</typeparam>
    public class Quadtree<T> : IPoolable, IDisposable
    {
        // We can't use PoolHelper since the constructor is private
        private static readonly Pool<Quadtree<T>> Pool = new Pool<Quadtree<T>>(() => new Quadtree<T>());

        /// <summary>
        /// Creates a Quadtree.
        /// </summary>
        /// <param name="size">The size of the tree's bounds.</param>
        /// <param name="maxItems">The amount of items a node can contain before splitting.</param>
        /// <param name="maxDepth">The maximum depth</param>
        /// <returns>The quadtree.</returns>
        public static Quadtree<T> Create(Rect size, int maxItems, int maxDepth)
        {
            var quadtree = Pool.Spawn();
            quadtree.Bounds = size;
            quadtree.MaxItems = maxItems;
            quadtree.MaxDepth = maxDepth;
            return quadtree;
        }

        private static Quadtree<T> CreateSubtree(Rect size, int maxItems, int depth, int maxDepth)
        {
            var quadtree = Create(size, maxItems, maxDepth);
            quadtree.Depth = depth;
            return quadtree;
        }

        /// <summary>
        /// Destroys a tree, making it available for being recycled.
        /// 
        /// Do not use the tree again after calling this.
        /// </summary>
        /// <param name="tree">The tree to destroy.</param>
        public static void Destroy(Quadtree<T> tree)
        {
            Pool.Despawn(tree);
        }

        private struct ItemBounds
        {
            public readonly T Item;
            public readonly Rect Bounds;

            public ItemBounds(T item, Rect bounds)
            {
                Item = item;
                Bounds = bounds;
            }
        }

        private readonly Quadtree<T>[] _subNodes = new Quadtree<T>[4];
        private readonly List<ItemBounds> _items = new List<ItemBounds>();

        /// <summary>
        /// Gets the bounds of this quadtree node.
        /// </summary>
        public Rect Bounds { get; private set; }

        /// <summary>
        /// Gets the depth of this quadtree node.
        /// </summary>
        public int Depth { get; private set; }

        /// <summary>
        /// Gets the maximum depth that the quadtree can go.
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

        private Quadtree()
        {

        }

        /// <summary>
        /// Inserts an item with the specified bounds into the quadtree.
        /// </summary>
        /// <param name="item">The item to insert.</param>
        /// <param name="bounds">The bounds of the item, used to place it correctly in the tree.</param>
        public void Insert(T item, Rect bounds)
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
        /// Removes an item with the specified bounds from the quadtree.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="bounds">The bounds used for previously inserting this item.</param>
        /// <returns>True if item was found and removed, otherwise false.</returns>
        public bool Remove(T item, Rect bounds)
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
        /// Retrieves all potential matches for the given <see cref="Rect"/>.
        /// 
        /// Careful, this method creates a new <see cref="HashSet{T}"/>. 
        /// You might want to use <see cref="Retrieve(Rect, HashSet{T})"/> instead if you call this often.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns>Potential matches.</returns>
        public HashSet<T> Retrieve(Rect rect)
        {
            var results = new HashSet<T>();
            Retrieve(rect, results);
            return results;
        }

        /// <summary>
        /// Retrieves all potential matches for the given <see cref="Rect"/> and adds them to <paramref name="results"/>.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="results">Where results will be added to.</param>
        public void Retrieve(Rect rect, HashSet<T> results)
        {
            if (IsSplit)
            {
                var containingNodes = GetContainingNodes(rect);
                if (containingNodes != SubNodes.None)
                {
                    for (var i = 0; i < _subNodes.Length; i++)
                    {
                        var subNode = 1 << i;
                        if (((byte)containingNodes & subNode) == subNode)
                            _subNodes[i].Retrieve(rect, results);
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
        /// Destroys the quadtree using <see cref="Destroy"/>.
        /// 
        /// Do not use the tree after calling this!
        /// </summary>
        public void Dispose()
        {
            Destroy(this);
        }

        private void Split()
        {
            var bounds = Bounds;
            var subNodeSize = bounds.size * 0.5f;
            var topLeft = new Rect(bounds.xMin, bounds.center.y, subNodeSize.x, subNodeSize.y);
            var topRight = new Rect(bounds.center, subNodeSize);
            var bottomLeft = new Rect(bounds.min, subNodeSize);
            var bottomRight = new Rect(bounds.center.x, bounds.yMin, subNodeSize.x, subNodeSize.y);

            var newDepth = Depth + 1;
            _subNodes[0] = CreateSubtree(topLeft, MaxItems, newDepth, MaxDepth);
            _subNodes[1] = CreateSubtree(topRight, MaxItems, newDepth, MaxDepth);
            _subNodes[2] = CreateSubtree(bottomLeft, MaxItems, newDepth, MaxDepth);
            _subNodes[3] = CreateSubtree(bottomRight, MaxItems, newDepth, MaxDepth);

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
            TopLeft = 1 << 0,
            TopRight = 1 << 1,
            BottomLeft = 1 << 2,
            BottomRight = 1 << 3,
            All = TopLeft | TopRight | BottomLeft | BottomRight
        }

        private SubNodes GetContainingNodes(Rect target)
        {
            var subnodes = SubNodes.None;

            var center = Bounds.center;

            var left = target.xMin < center.x;
            var right = target.xMax > center.x;
            var bottom = target.yMin < center.y;
            var top = target.yMax > center.y;

            if (left)
            {
                if (top)
                    subnodes |= SubNodes.TopLeft;
                if (bottom)
                    subnodes |= SubNodes.BottomLeft;
            }

            if (right)
            {
                if (top)
                    subnodes |= SubNodes.TopRight;
                if (bottom)
                    subnodes |= SubNodes.BottomRight;
            }

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
