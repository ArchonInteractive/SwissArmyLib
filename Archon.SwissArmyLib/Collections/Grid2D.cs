namespace Archon.SwissArmyLib.Collections
{
    /// <summary>
    /// A generic two-dimensional grid.
    /// 
    /// <seealso cref="Grid3D{T}"/>
    /// </summary>
    /// <typeparam name="T">The type of content cells can contain.</typeparam>
    public class Grid2D<T>
    {
        private T[][] _data;

        /// <summary>
        /// Gets the width (number of columns) of the grid.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Gets the height (number of rows) of the grid.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Gets or sets the default values used for clearing or initializing new cells.
        /// </summary>
        public T DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the value of the cell located at the specified coordinate.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The contents of the cell.</returns>
        public T this[int x, int y]
        {
            get { return _data[y][x]; }
            set { _data[y][x] = value; }
        }

        /// <summary>
        /// Creates a new 2D Grid with the specified width and height. 
        /// Cells will be initialized with their type's default value.
        /// </summary>
        /// <param name="width">Number of columns</param>
        /// <param name="height">Number of rows</param>
        public Grid2D(int width, int height)
        {
            Width = width;
            Height = height;

            _data = CreateArrays(width, height);
        }

        /// <summary>
        /// Creates a new 2D Grid with the specified width and height. 
        /// Cells will be initialized with the value of <paramref name="defaultValue"/>.
        /// </summary>
        /// <param name="width">Number of columns</param>
        /// <param name="height">Number of rows</param>
        /// <param name="defaultValue">The value used for initializing new cells.</param>
        public Grid2D(int width, int height, T defaultValue) : this(width, height)
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
        /// <returns>The cell contents.</returns>
        public T Get(int x, int y)
        {
            return this[x, y];
        }

        /// <summary>
        /// Sets the value of the cell located at the specified coordinate.
        /// 
        /// <seealso cref="this"/>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value">The value to set the cell to.</param>
        public void Set(int x, int y, T value)
        {
            this[x, y] = value;
        }

        /// <summary>
        /// Clears the grid, setting every cell to <see cref="DefaultValue"/>.
        /// </summary>
        public void Clear()
        {
            Fill(DefaultValue, 0, 0, Width - 1, Height - 1);
        }

        /// <summary>
        /// Clears the grid, setting every cell to the given value.
        /// </summary>
        public void Clear(T clearValue)
        {
            Clear();
        }

        /// <summary>
        /// Fills everything in the specified rectangle to the given value.
        /// </summary>
        /// <param name="value">The value to fill the cells with.</param>
        /// <param name="minX">Bottom left corner's x value.</param>
        /// <param name="minY">Bottom left corner's y value.</param>
        /// <param name="maxX">Upper right corner's x value.</param>
        /// <param name="maxY">Upper right corner's y value.</param>
        public void Fill(T value, int minX, int minY, int maxX, int maxY)
        {
            for (var y = minY; y <= maxY; y++)
            {
                var widthArray = _data[y];

                for (var x = minX; x <= maxX; x++)
                    widthArray[x] = value;
            }
        }

        /// <summary>
        /// Resizes the Grid to the given size, keeping data the same but any new cells will be set to <see cref="DefaultValue"/>.
        /// </summary>
        /// <param name="width">The new width.</param>
        /// <param name="height">The new height.</param>
        public void Resize(int width, int height)
        {
            var oldData = _data;
            var oldWidth = Width;
            var oldHeight = Height;

            _data = CreateArrays(width, height);
            CopyArraysContents(oldData, _data);
            Fill(DefaultValue, oldWidth, oldHeight, width, height);
        }

        private static T[][] CreateArrays(int width, int height)
        {
            var arrays = new T[height][];

            for (var y = 0; y < arrays.Length; y++)
                arrays[y] = new T[width];

            return arrays;
        }

        private static void CopyArraysContents(T[][] src, T[][] dst)
        {
            var srcHeight = src.Length;
            var srcWidth = src[0].Length;

            var dstHeight = dst.Length;
            var dstWidth = dst[0].Length;

            for (var y = 0; y < srcHeight && y < dstHeight; y++)
            {
                var srcWidthArray = src[y];
                var dstWidthArray = dst[y];

                for (var x = 0; x < srcWidth && x < dstWidth; x++)
                    dstWidthArray[x] = srcWidthArray[x];
            }
        }
    }
}