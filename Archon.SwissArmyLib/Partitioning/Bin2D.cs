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
        private static readonly Pool<LinkedListNode<T>> SharedNodePool = new Pool<LinkedListNode<T>>(() => new LinkedListNode<T>(default(T)));
        private static readonly Pool<PooledLinkedList<T>> ListPool = new Pool<PooledLinkedList<T>>(() => new PooledLinkedList<T>(SharedNodePool));

        private readonly Grid2D<PooledLinkedList<T>> _grid;
        private readonly Vector2 _bottomLeft;
        private readonly Vector2 _topRight;

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
        /// The coordinate at which this bin's bottom left corner lies.
        /// </summary>
        public Vector2 Origin { get { return _bottomLeft; }}

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
            : this(gridWidth, gridHeight, cellWidth, cellHeight, Vector2.zero)
        {

        }

        /// <summary>
        /// Creates a new Bin.
        /// </summary>
        /// <param name="gridWidth">The width of the grid.</param>
        /// <param name="gridHeight">The height of the grid.</param>
        /// <param name="cellWidth">The width of a cell.</param>
        /// <param name="cellHeight">The height of a cell.</param>
        /// <param name="origin">The coordinate of the bottom left point of the grid.</param>
        public Bin2D(int gridWidth, int gridHeight, float cellWidth, float cellHeight, Vector2 origin) 
        {
            _grid = new Grid2D<PooledLinkedList<T>>(gridWidth, gridHeight);

            CellWidth = cellWidth;
            CellHeight = cellHeight;
            _bottomLeft = origin;
            _topRight = new Vector2(origin.x + gridWidth * cellWidth, origin.y + gridHeight * cellHeight);
        }

        /// <summary>
        /// Inserts an item with the given bounds into the Bin.
        /// </summary>
        /// <param name="item">The item to insert.</param>
        /// <param name="bounds">The bounds of the item.</param>
        public void Insert(T item, Rect bounds)
        {
            if (IsOutOfBounds(bounds))
                return;

            var internalBounds = GetInternalBounds(bounds);

            for (var y = internalBounds.MinY; y <= internalBounds.MaxY; y++)
            {
                for (var x = internalBounds.MinX; x <= internalBounds.MaxX; x++)
                {
                    var cell = _grid[x, y];

                    if (cell == null)
                        _grid[x, y] = cell = ListPool.Spawn();

                    cell.AddLast(item);
                }
            }
        }

        /// <summary>
        /// Goes through all cells and removes the specified item if they contain it.
        /// If you can you should use <see cref="Remove(T, Rect)"/> instead.
        /// </summary>
        /// <param name="item">The item to remove</param>
        public void Remove(T item)
        {
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var list = _grid[x, y];

                    if (list == null)
                        continue;

                    list.Remove(item);
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
            if (IsOutOfBounds(bounds))
                return;

            var internalBounds = GetInternalBounds(bounds);

            for (var y = internalBounds.MinY; y <= internalBounds.MaxY; y++)
            {
                for (var x = internalBounds.MinX; x <= internalBounds.MaxX; x++)
                {
                    var cell = _grid[x, y];

                    if (cell == null)
                        continue;

                    cell.Remove(item);

                    if (cell.Count == 0)
                    {
                        ListPool.Despawn(cell);
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
            if (IsOutOfBounds(bounds))
                return;

            var internalBounds = GetInternalBounds(bounds);

            for (var y = internalBounds.MinY; y <= internalBounds.MaxY; y++)
            {
                for (var x = internalBounds.MinX; x <= internalBounds.MaxX; x++)
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
                    ListPool.Despawn(cell);

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

        private bool IsOutOfBounds(Rect bounds)
        {
            return !(bounds.xMax > _bottomLeft.x 
                && bounds.xMin < _topRight.x
                && bounds.yMax > _bottomLeft.y
                && bounds.yMin < _topRight.y);
        }

        private InternalBounds GetInternalBounds(Rect bounds)
        {
            var internalBounds = new InternalBounds
            {
                MinX = Mathf.Max(0, (int)((bounds.xMin - _bottomLeft.x) / CellWidth)),
                MinY = Mathf.Max(0, (int)((bounds.yMin - _bottomLeft.y) / CellHeight)),
                MaxX = Mathf.Min(Width - 1, (int)((bounds.xMax - _bottomLeft.x) / CellWidth)),
                MaxY = Mathf.Min(Height - 1, (int)((bounds.yMax - _bottomLeft.y) / CellHeight))
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