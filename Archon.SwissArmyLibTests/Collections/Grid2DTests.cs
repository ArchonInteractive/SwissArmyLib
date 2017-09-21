using System;
using NUnit.Framework;

namespace Archon.SwissArmyLib.Collections.Tests
{
    [TestFixture]
    public class Grid2DTests
    {
        [Test]
        public void Constructor_CorrectSize()
        {
            var grid = new Grid2D<int>(100, 50);

            Assert.AreEqual(100, grid.Width);
            Assert.AreEqual(50, grid.Height);
        }

        [Test]
        public void Constructor_AllCellsAreDefaultValue()
        {
            var grid = new Grid2D<int>(100, 50, 33);

            for (var x = 0; x < grid.Width; x++)
                for (var y = 0; y < grid.Height; y++)
                    Assert.AreEqual(33, grid[x, y], "({0},{1})", x, y);
        }

        [Test]
        public void AllCellsUsableAndUnique()
        {
            var grid = new Grid2D<int>(100, 50, 33);

            for (var x = 0; x < grid.Width; x++)
                for (var y = 0; y < grid.Height; y++)
                    grid[x, y] = x * grid.Height + y;

            for (var x = 0; x < grid.Width; x++)
                for (var y = 0; y < grid.Height; y++)
                    Assert.AreEqual(x * grid.Height + y, grid[x, y], "({0},{1})", x, y);
        }

        [Test]
        public void Clear_Default_AllCleared()
        {
            var grid = new Grid2D<int>(100, 50, 33);
            grid.Clear(1);

            grid.Clear();

            for (var x = 0; x < grid.Width; x++)
                for (var y = 0; y < grid.Height; y++)
                    Assert.AreEqual(33, grid[x, y], "({0},{1})", x, y);
        }

        public void Clear_Specific_AllCleared()
        {
            var grid = new Grid2D<int>(100, 50, 33);

            grid.Clear(1);

            for (var x = 0; x < grid.Width; x++)
                for (var y = 0; y < grid.Height; y++)
                    Assert.AreEqual(1, grid[x, y], "({0},{1})", x, y);
        }

        [Test]
        public void Fill_OnlyTouchesRect()
        {
            var width = 100;
            var height = 50;

            var grid = new Grid2D<int>(width, height, 33);

            int minX = 5, minY = 10;
            int maxX = width-5, maxY = height - 10;

            grid.Fill(1, minX, minY, maxX, maxY);

            for (var x = 0; x < grid.Width; x++)
            {
                for (var y = 0; y < grid.Height; y++)
                {
                    // outside rect
                    if (x < minX || x > maxX || y < minY || y > maxY)
                    {
                        Assert.AreEqual(33, grid[x, y], "({0},{1})", x, y);
                    }
                    else
                        Assert.AreEqual(1, grid[x, y], "({0},{1})", x, y);
                }
            }
        }

        [Test]
        public void OutOfBoundsThrowsException()
        {
            var grid = new Grid2D<int>(10, 10);
            var i = 0;
            
            // getter
            Assert.Catch<IndexOutOfRangeException>(() => i = grid[-1, 0]);
            Assert.Catch<IndexOutOfRangeException>(() => i = grid[0, -1]);
            Assert.Catch<IndexOutOfRangeException>(() => i = grid[10, 0]);
            Assert.Catch<IndexOutOfRangeException>(() => i = grid[0, 10]);

            // setter
            Assert.Catch<IndexOutOfRangeException>(() => grid[-1, 0] = 5);
            Assert.Catch<IndexOutOfRangeException>(() => grid[0, -1] = 5);
            Assert.Catch<IndexOutOfRangeException>(() => grid[10, 0] = 5);
            Assert.Catch<IndexOutOfRangeException>(() => grid[0, 10] = 5);
        }

        [TestCase(20, 25, Description = "Bigger")]
        [TestCase(5, 2, Description = "Smaller")]
        [TestCase(10, 15, Description = "Mixed")]
        public void Resize_CorrectSize(int toWidth, int toHeight)
        {
            var grid = new Grid2D<int>(10, 10);

            grid.Resize(toWidth, toHeight);

            Assert.AreEqual(toWidth, grid.Width);
            Assert.AreEqual(toHeight, grid.Height);
        }

        [Test]
        public void Resize_CellsStayTheSame()
        {
            int startWidth = 10, startHeight = 15;

            var grid = new Grid2D<int>(startWidth, startHeight);

            for (var x = 0; x < grid.Width; x++)
                for (var y = 0; y < grid.Height; y++)
                    grid[x, y] = x * startHeight + y;

            grid.Resize(20, 20);

            for (var x = 0; x < startWidth; x++)
                for (var y = 0; y < startHeight; y++)
                    Assert.AreEqual(x * startHeight + y, grid[x, y], "({0},{1})", x, y);
        }

        [Test]
        public void Resize_NewCellsHaveDefaultValue()
        {
            var startWidth = 10;
            var startHeight = 10;
            var defaultVal = 5;
            var grid = new Grid2D<int>(startWidth, startHeight, defaultVal);
            grid.Clear(10);

            grid.Resize(20, 20);

            for (var x = 0; x < grid.Width; x++)
            {
                for (var y = 0; y < grid.Height; y++)
                {
                    if (x >= startWidth || y >= startHeight)
                        Assert.AreEqual(defaultVal, grid[x, y], "({0},{1})", x, y);
                    else
                        Assert.AreEqual(10, grid[x, y], "({0},{1})", x, y);
                }
            }
        }
    }
}