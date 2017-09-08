using System;
using System.Collections.Generic;
using Archon.SwissArmyLib.Collections;
using Archon.SwissArmyLib.Pooling;
using UnityEngine;

namespace Archon.SwissArmyLib.Partitioning
{
    /// <summary>
    /// A simple GC-friendly two-dimensional <see href="https://en.wikipedia.org/wiki/Bin_(computational_geometry)">Bin (aka Spatial Grid)</see> implementation.
    /// 
    /// When you're done with the Bin, you should <see cref="Dispose"/> it so its resources can be freed in their object pool. If you forget this, no harm will be done but memory will be GC'ed.
    /// 
    /// <seealso cref="Bin3D{T}"/>
    /// </summary>
    /// <typeparam name="T">The type of items this Bin will hold.</typeparam>
    public class Bin2D<T> : IDisposable
    {
        private readonly Grid2D<LinkedList<T>> _grid;

        /// <summary>
        /// Gets the width (number of columns) of the Bin.
        /// </summary>
        public int Width { get { return _grid.Width; } }

        /// <summary>
        /// Gets the height (number of rows) of the Bin.
        /// </summary>
        public int Height { get { return _grid.Height; } }

        /// <summary>
        /// Gets the width of cells in the Bin.
        /// </summary>
        public float CellWidth { get; private set; }

        /// <summary>
        /// Gets the height of cells in the Bin.
        /// </summary>
        public float CellHeight { get; private set; }

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> for the items in the given cell.
        /// </summary>
        /// <param name="x">The x coordinate of the cell.</param>
        /// <param name="y">The y coordinate of the cell.</param>
        /// <returns></returns>
        public IEnumerable<T> this[int x, int y]
        {
            get { return _grid[x, y]; }
        }

        /// <summary>
        /// Creates a new Bin.
        /// </summary>
        /// <param name="gridWidth">The width of the grid.</param>
        /// <param name="gridHeight">The height of the grid.</param>
        /// <param name="cellWidth">The width of a cell.</param>
        /// <param name="cellHeight">The height of a cell.</param>
        public Bin2D(int gridWidth, int gridHeight, float cellWidth, float cellHeight)
        {
            _grid = new Grid2D<LinkedList<T>>(gridWidth, gridHeight);

            CellWidth = cellWidth;
            CellHeight = cellHeight;
        }

        /// <summary>
        /// Inserts an item with the given bounds into the Bin.
        /// </summary>
        /// <param name="item">The item to insert.</param>
        /// <param name="bounds">The bounds of the item.</param>
        public void Insert(T item, Rect bounds)
        {
            var internalBounds = GetInternalBounds(bounds);

            for (var y = internalBounds.MinY; y < internalBounds.MaxY; y++)
            {
                for (var x = internalBounds.MinX; x < internalBounds.MaxX; x++)
                {
                    var cell = _grid[x, y];

                    if (cell == null)
                        _grid[x, y] = cell = PoolHelper<LinkedList<T>>.Spawn();

                    cell.AddLast(item);
                }
            }
        }

        /// <summary>
        /// Removes an item which was inserted with the given bounds from the Bin.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="bounds">The bounds that the item was inserted with.</param>
        public void Remove(T item, Rect bounds)
        {
            var internalBounds = GetInternalBounds(bounds);

            for (var y = internalBounds.MinY; y < internalBounds.MaxY; y++)
            {
                for (var x = internalBounds.MinX; x < internalBounds.MaxX; x++)
                {
                    var cell = _grid[x, y];

                    if (cell == null)
                        continue;

                    cell.Remove(item);

                    if (cell.Count == 0)
                    {
                        PoolHelper<LinkedList<T>>.Despawn(cell);
                        _grid[x, y] = null;
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
        public void Update(T item, Rect prevBounds, Rect newBounds)
        {
            Remove(item, prevBounds);
            Insert(item, newBounds);
        }

        /// <summary>
        /// Gets all items in the Bin that could potentially intersect with the given bounds.
        /// </summary>
        /// <param name="bounds">The bounds to check for.</param>
        /// <param name="results">Where to add results to.</param>
        public void Retrieve(Rect bounds, HashSet<T> results)
        {
            var internalBounds = GetInternalBounds(bounds);

            for (var y = internalBounds.MinY; y < internalBounds.MaxY; y++)
            {
                for (var x = internalBounds.MinX; x < internalBounds.MaxX; x++)
                {
                    var cell = _grid[x, y];

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

        /// <summary>
        /// Removes all items from the Bin.
        /// </summary>
        public void Clear()
        {
            for (var y = 0; y < _grid.Height; y++)
            {
                for (var x = 0; x < _grid.Width; x++)
                {
                    var cell = _grid[x, y];

                    if (cell == null)
                        continue;

                    cell.Clear();
                    PoolHelper<LinkedList<T>>.Despawn(cell);

                    _grid[x, y] = null;
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

        private InternalBounds GetInternalBounds(Rect bounds)
        {
            var internalBounds = new InternalBounds
            {
                MinX = Mathf.Max(0, (int)(bounds.xMin / CellWidth)),
                MinY = Mathf.Max(0, (int)(bounds.yMin / CellHeight)),
                MaxX = Mathf.Min(Width - 1, (int)(bounds.xMax / CellWidth)),
                MaxY = Mathf.Min(Height - 1, (int)(bounds.yMax / CellHeight))
            };

            return internalBounds;
        }

        private struct InternalBounds
        {
            public int MinX, MinY, 
                MaxX, MaxY;
        }
    }
}