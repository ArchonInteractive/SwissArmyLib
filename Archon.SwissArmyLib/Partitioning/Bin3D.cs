using System;
using System.Collections.Generic;
using Archon.SwissArmyLib.Collections;
using Archon.SwissArmyLib.Pooling;
using UnityEngine;

namespace Archon.SwissArmyLib.Partitioning
{
    /// <summary>
    /// A simple GC-friendly three-dimensional <see href="https://en.wikipedia.org/wiki/Bin_(computational_geometry)">Bin (aka Spatial Grid)</see> implementation.
    /// 
    /// When you're done with the Bin, you should <see cref="Dispose"/> it so its resources can be freed in their object pool. If you forget this, no harm will be done but memory will be GC'ed.
    /// 
    /// <seealso cref="Bin2D{T}"/>
    /// </summary>
    /// <typeparam name="T">The type of items this Bin will hold.</typeparam>
    public class Bin3D<T> : IDisposable
    {
        private static readonly Pool<LinkedListNode<T>> SharedNodePool = new Pool<LinkedListNode<T>>(() => new LinkedListNode<T>(default(T)));
        private static readonly Pool<PooledLinkedList<T>> ListPool = new Pool<PooledLinkedList<T>>(() => new PooledLinkedList<T>(SharedNodePool));

        private readonly Grid3D<PooledLinkedList<T>> _grid;

        /// <summary>
        /// Gets the width (number of columns) of the Bin.
        /// </summary>
        public int Width { get { return _grid.Width; } }

        /// <summary>
        /// Gets the height (number of rows) of the Bin.
        /// </summary>
        public int Height { get { return _grid.Height; } }

        /// <summary>
        /// Gets the depth (number of layers) of the Bin.
        /// </summary>
        public int Depth { get { return _grid.Depth; } }

        /// <summary>
        /// Gets the width of cells in the Bin.
        /// </summary>
        public float CellWidth { get; private set; }

        /// <summary>
        /// Gets the height of cells in the Bin.
        /// </summary>
        public float CellHeight { get; private set; }

        /// <summary>
        /// Gets the depth of cells in the Bin.
        /// </summary>
        public float CellDepth { get; private set; }

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> for the items in the given cell.
        /// </summary>
        /// <param name="x">The x coordinate of the cell.</param>
        /// <param name="y">The y coordinate of the cell.</param>
        /// <param name="z">The z coordinate of the cell.</param>
        /// <returns></returns>
        public IEnumerable<T> this[int x, int y, int z]
        {
            get { return _grid[x, y, z]; }
        }

        /// <summary>
        /// Creates a new Bin.
        /// </summary>
        /// <param name="gridWidth">The width of the grid.</param>
        /// <param name="gridHeight">The height of the grid.</param>
        /// <param name="gridDepth">The depth of the grid.</param>
        /// <param name="cellWidth">The width of a cell.</param>
        /// <param name="cellHeight">The height of a cell.</param>
        /// <param name="cellDepth">The depth of a cell.</param>
        public Bin3D(int gridWidth, int gridHeight, int gridDepth, float cellWidth, float cellHeight, float cellDepth)
        {
            _grid = new Grid3D<PooledLinkedList<T>>(gridWidth, gridHeight, gridDepth);

            CellWidth = cellWidth;
            CellHeight = cellHeight;
            CellDepth = cellDepth;
        }

        /// <summary>
        /// Inserts an item with the given bounds into the Bin.
        /// </summary>
        /// <param name="item">The item to insert.</param>
        /// <param name="bounds">The bounds of the item.</param>
        public void Insert(T item, Bounds bounds)
        {
            if (IsOutOfBounds(bounds))
                return;

            var internalBounds = GetInternalBounds(bounds);

            for (var z = internalBounds.MinZ; z <= internalBounds.MaxZ; z++)
            {
                for (var y = internalBounds.MinY; y <= internalBounds.MaxY; y++)
                {
                    for (var x = internalBounds.MinX; x <= internalBounds.MaxX; x++)
                    {
                        var cell = _grid[x, y, z];

                        if (cell == null)
                            _grid[x, y, z] = cell = ListPool.Spawn();

                        cell.AddLast(item);
                    }
                }
            }
        }

        /// <summary>
        /// Removes an item which was inserted with the given bounds from the Bin.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="bounds">The bounds that the item was inserted with.</param>
        public void Remove(T item, Bounds bounds)
        {
            if (IsOutOfBounds(bounds))
                return;

            var internalBounds = GetInternalBounds(bounds);

            for (var z = internalBounds.MinZ; z <= internalBounds.MaxZ; z++)
            {
                for (var y = internalBounds.MinY; y <= internalBounds.MaxY; y++)
                {
                    for (var x = internalBounds.MinX; x <= internalBounds.MaxX; x++)
                    {
                        var cell = _grid[x, y, z];

                        if (cell == null)
                            continue;

                        cell.Remove(item);

                        if (cell.Count == 0)
                        {
                            ListPool.Despawn(cell);
                            _grid[x, y, z] = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes and reinserts an item with new bounds, essentially moving it.
        /// </summary>
        /// <param name="item">The item to update.</param>
        /// <param name="prevBounds">The bounds that the item was inserted with earlier.</param>
        /// <param name="newBounds">The new bounds to insert the item with.</param>
        public void Update(T item, Bounds prevBounds, Bounds newBounds)
        {
            Remove(item, prevBounds);
            Insert(item, newBounds);
        }

        /// <summary>
        /// Gets all items in the Bin that could potentially intersect with the given bounds.
        /// </summary>
        /// <param name="bounds">The bounds to check for.</param>
        /// <param name="results">Where to add results to.</param>
        public void Retrieve(Bounds bounds, HashSet<T> results)
        {
            if (IsOutOfBounds(bounds))
                return;

            var internalBounds = GetInternalBounds(bounds);

            for (var z = internalBounds.MinZ; z <= internalBounds.MaxZ; z++)
            {
                for (var y = internalBounds.MinY; y <= internalBounds.MaxY; y++)
                {
                    for (var x = internalBounds.MinX; x <= internalBounds.MaxX; x++)
                    {
                        var cell = _grid[x, y, z];

                        if (cell == null)
                            continue;

                        var current = cell.First;
                        while (current != null)
                        {
                            results.Add(current.Value);
                            current = current.Next;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes all items from the Bin.
        /// </summary>
        public void Clear()
        {
            for (var z = 0; z < _grid.Depth; z++)
            {
                for (var y = 0; y < _grid.Height; y++)
                {
                    for (var x = 0; x < _grid.Width; x++)
                    {
                        var cell = _grid[x, y, z];

                        if (cell == null)
                            continue;

                        cell.Clear();
                        ListPool.Despawn(cell);

                        _grid[x, y, z] = null;
                    }
                }
            }
        }

        /// <summary>
        /// Frees (clears) used resources that can be recycled. 
        /// 
        /// Call this when you're done with the Bin.
        /// </summary>
        public void Dispose()
        {
            Clear();
        }

        private bool IsOutOfBounds(Bounds bounds)
        {
            return !(bounds.max.x > 0
                     && bounds.min.x < Width * CellWidth
                     && bounds.max.y > 0
                     && bounds.min.y < Height * CellHeight
                     && bounds.max.z > 0
                     && bounds.min.z < Depth * CellDepth);
        }

        private InternalBounds GetInternalBounds(Bounds bounds)
        {
            var min = bounds.min;
            var max = bounds.max;

            var internalBounds = new InternalBounds
            {
                MinX = Mathf.Max(0, (int)(min.x / CellWidth)),
                MinY = Mathf.Max(0, (int)(min.y / CellHeight)),
                MinZ = Mathf.Max(0, (int)(min.z / CellDepth)),
                MaxX = Mathf.Min(Width - 1, (int)(max.x / CellWidth)),
                MaxY = Mathf.Min(Height - 1, (int)(max.y / CellHeight)),
                MaxZ = Mathf.Min(Depth - 1, (int)(max.z / CellDepth))
            };

            return internalBounds;
        }

        private struct InternalBounds
        {
            public int MinX, MinY, MinZ,
                MaxX, MaxY, MaxZ;
        }
    }
}