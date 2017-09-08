namespace Archon.SwissArmyLib.Collections
{
    /// <summary>
    /// A generic three-dimensional grid.
    /// 
    /// <seealso cref="Grid2D{T}"/>
    /// </summary>
    /// <typeparam name="T">The type of content cells can contain.</typeparam>
    public class Grid3D<T>
    {
        private T[][][] _data;

        /// <summary>
        /// Gets the width (number of columns) of the grid.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Gets the height (number of rows) of the grid.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Gets the depth (number of layers) of the grid.
        /// </summary>
        public int Depth { get; private set; }

        /// <summary>
        /// Gets or sets the default values used for clearing or initializing new cells.
        /// </summary>
        public T DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the value of the cell located at the specified coordinate.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns>The contents of the cell.</returns>
        public T this[int x, int y, int z]
        {
            get { return _data[z][y][x]; }
            set { _data[z][y][x] = value; }
        }

        /// <summary>
        /// Creates a new 3D Grid with the specified width, height and depth. 
        /// Cells will be initialized with their type's default value.
        /// </summary>
        /// <param name="width">Number of columns</param>
        /// <param name="height">Number of rows</param>
        /// <param name="depth">Number of layers</param>
        public Grid3D(int width, int height, int depth)
        {
            Width = width;
            Height = height;
            Depth = depth;

            _data = CreateArrays(width, height, depth);
        }

        /// <summary>
        /// Creates a new 3D Grid with the specified width, height and depth. 
        /// Cells will be initialized with the value of <paramref name="defaultValue"/>.
        /// </summary>
        /// <param name="width">Number of columns</param>
        /// <param name="height">Number of rows</param>
        /// <param name="depth">Number of layers</param>
        /// <param name="defaultValue">The value used for initializing new cells.</param>
        public Grid3D(int width, int height, int depth, T defaultValue) : this(width, height, depth)
        {
            DefaultValue = defaultValue;
            Clear();
        }

        /// <summary>
        /// Gets the value of the cell located at the specified coordinate.
        /// 
        /// <seealso cref="this"/>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns>The cell contents.</returns>
        public T Get(int x, int y, int z)
        {
            return this[x, y, z];
        }

        /// <summary>
        /// Sets the value of the cell located at the specified coordinate.
        /// 
        /// <seealso cref="this"/>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="value">The value to set the cell to.</param>
        public void Set(int x, int y, int z, T value)
        {
            this[x, y, z] = value;
        }

        /// <summary>
        /// Clears the grid, setting every cell to <see cref="DefaultValue"/>.
        /// </summary>
        public void Clear()
        {
            Fill(DefaultValue, 0, 0, 0, Width - 1, Height - 1, Depth - 1);
        }

        /// <summary>
        /// Clears the grid, setting every cell to the given value.
        /// </summary>
        public void Clear(T clearValue)
        {
            Clear();
        }

        /// <summary>
        /// Fills everything in the specified cube to the given value.
        /// </summary>
        /// <param name="value">The value to fill the cells with.</param>
        /// <param name="minX">Bottom left front corner's x value.</param>
        /// <param name="minY">Bottom left front corner's y value.</param>
        /// <param name="minZ">Bottom left front corner's z value.</param>
        /// <param name="maxX">Upper right back corner's x value.</param>
        /// <param name="maxY">Upper right back corner's y value.</param>
        /// <param name="maxZ">Upper right back corner's z value.</param>
        public void Fill(T value, int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
        {
            for (var z = minZ; z < maxZ; z++)
            {
                var heightArray = _data[z];

                for (var y = minY; y <= maxY; y++)
                {
                    var widthArray = heightArray[y];

                    for (var x = minX; x <= maxX; x++)
                        widthArray[x] = value;
                }
            }
        }

        /// <summary>
        /// Resizes the Grid to the given size, keeping data the same but any new cells will be set to <see cref="DefaultValue"/>.
        /// </summary>
        /// <param name="width">The new width.</param>
        /// <param name="height">The new height.</param>
        /// <param name="depth">The new depth.</param>
        public void Resize(int width, int height, int depth)
        {
            var oldData = _data;
            var oldWidth = Width;
            var oldHeight = Height;
            var oldDepth = Depth;

            _data = CreateArrays(width, height, depth);
            CopyArraysContents(oldData, _data);
            Fill(DefaultValue, oldWidth, oldHeight, oldDepth, width, height, depth);
        }

        private static T[][][] CreateArrays(int width, int height, int depth)
        {
            var depthArray = new T[depth][][];

            for (var z = 0; z < depth; z++)
            {
                var heightArray = new T[height][];

                for (var y = 0; y < height; y++)
                    heightArray[y] = new T[width];

                depthArray[z] = heightArray;
            }

            return depthArray;
        }

        private static void CopyArraysContents(T[][][] src, T[][][] dst)
        {
            var srcHeight = src.Length;
            var srcWidth = src[0].Length;
            var srcDepth = src[0][0].Length;

            var dstHeight = dst.Length;
            var dstWidth = dst[0].Length;
            var dstDepth = dst[0][0].Length;

            for (var z = 0; z < srcDepth && z < dstDepth; z++)
            {
                var srcHeightArray = src[z];
                var dstHeightArray = dst[z];

                for (var y = 0; y < srcHeight && y < dstHeight; y++)
                {
                    var srcWidthArray = srcHeightArray[y];
                    var dstWidthArray = dstHeightArray[y];

                    for (var x = 0; x < srcWidth && x < dstWidth; x++)
                        dstWidthArray[x] = srcWidthArray[x];
                }
            }
        }
    }
}