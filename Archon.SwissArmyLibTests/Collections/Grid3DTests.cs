using System;
using NUnit.Framework;

namespace Archon.SwissArmyLib.Collections.Tests
{
    [TestFixture]
    public class Grid3DTests
    {
        [Test]
        public void Constructor_CorrectSize()
        {
            var grid = new Grid3D<int>(100, 50, 24);

            Assert.AreEqual(100, grid.Width);
            Assert.AreEqual(50, grid.Height);
            Assert.AreEqual(24, grid.Depth);
        }

        [Test]
        public void Constructor_AllCellsAreDefaultValue()
        {
            var grid = new Grid3D<int>(100, 50, 24, 33);

            for (var x = 0; x < grid.Width; x++)
                for (var y = 0; y < grid.Height; y++)
                    for (var z = 0; z < grid.Depth; z++)
                        Assert.AreEqual(33, grid[x, y, z], "({0},{1},{2})", x, y, z);
        }

        [Test]
        public void AllCellsUsableAndUnique()
        {
            var grid = new Grid3D<int>(100, 50, 24, 33);

            for (var x = 0; x < grid.Width; x++)
                for (var y = 0; y < grid.Height; y++)
                    for (var z = 0; z < grid.Depth; z++)
                        grid[x, y, z] = x * grid.Height + y * grid.Depth + z;

            for (var x = 0; x < grid.Width; x++)
                for (var y = 0; y < grid.Height; y++)
                    for (var z = 0; z < grid.Depth; z++)
                        Assert.AreEqual(x * grid.Height + y * grid.Depth + z, grid[x, y, z], "({0},{1},{2})", x, y, z);
        }

        [Test]
        public void Clear_Default_AllCleared()
        {
            var grid = new Grid3D<int>(100, 50, 24, 33);
            grid.Clear(1);

            grid.Clear();

            for (var x = 0; x < grid.Width; x++)
                for (var y = 0; y < grid.Height; y++)
                    for (var z = 0; z < grid.Depth; z++)
                        Assert.AreEqual(33, grid[x, y, z], "({0},{1},{2})", x, y, z);
        }

        public void Clear_Specific_AllCleared()
        {
            var grid = new Grid3D<int>(100, 50, 33);

            grid.Clear(1);

            for (var x = 0; x < grid.Width; x++)
                for (var y = 0; y < grid.Height; y++)
                    for (var z = 0; z < grid.Depth; z++)
                        Assert.AreEqual(1, grid[x, y, z], "({0},{1},{2})", x, y, z);
        }

        [Test]
        public void Fill_OnlyTouchesCube()
        {
            var width = 100;
            var height = 50;
            var depth = 24;

            var grid = new Grid3D<int>(width, height, depth, 33);

            int minX = 5, minY = 10, minZ = 8;
            int maxX = width-5, maxY = height - 10, maxZ = depth - 8;

            grid.Fill(1, minX, minY, minZ, maxX, maxY, maxZ);

            for (var x = 0; x < grid.Width; x++)
            {
                for (var y = 0; y < grid.Height; y++)
                {
                    for (var z = 0; z < grid.Depth; z++)
                    {
                        // outside cube
                        if (x < minX || x > maxX 
                            || y < minY || y > maxY
                            || z < minZ || z > maxZ)
                        {
                            Assert.AreEqual(33, grid[x, y, z], "({0},{1},{2})", x, y, z);
                        }
                        else
                            Assert.AreEqual(1, grid[x, y, z], "({0},{1},{2})", x, y, z);
                    }
                }
            }
        }

        [Test]
        public void OutOfBoundsThrowsException()
        {
            var grid = new Grid3D<int>(10, 10, 10);
            var i = 0;
            
            // getter
            Assert.Catch<IndexOutOfRangeException>(() => i = grid[-1, 0, 0]);
            Assert.Catch<IndexOutOfRangeException>(() => i = grid[0, -1, 0]);
            Assert.Catch<IndexOutOfRangeException>(() => i = grid[0, 0, -1]);
            Assert.Catch<IndexOutOfRangeException>(() => i = grid[10, 0, 0]);
            Assert.Catch<IndexOutOfRangeException>(() => i = grid[0, 10, 0]);
            Assert.Catch<IndexOutOfRangeException>(() => i = grid[0, 0, 10]);

            // setter
            Assert.Catch<IndexOutOfRangeException>(() => grid[-1, 0, 0] = 5);
            Assert.Catch<IndexOutOfRangeException>(() => grid[0, -1, 0] = 5);
            Assert.Catch<IndexOutOfRangeException>(() => grid[0, 0, -1] = 5);
            Assert.Catch<IndexOutOfRangeException>(() => grid[10, 0, 0] = 5);
            Assert.Catch<IndexOutOfRangeException>(() => grid[0, 10, 0] = 5);
            Assert.Catch<IndexOutOfRangeException>(() => grid[0, 0, 10] = 5);
        }

        [TestCase(20, 25, 23, Description = "Bigger")]
        [TestCase(5, 2, 8, Description = "Smaller")]
        [TestCase(10, 15, 8, Description = "Mixed")]
        public void Resize_CorrectSize(int toWidth, int toHeight, int toDepth)
        {
            var grid = new Grid3D<int>(10, 10, 10);

            grid.Resize(toWidth, toHeight, toDepth);

            Assert.AreEqual(toWidth, grid.Width);
            Assert.AreEqual(toHeight, grid.Height);
            Assert.AreEqual(toDepth, grid.Depth);
        }

        [Test]
        public void Resize_CellsStayTheSame()
        {
            int startWidth = 10, 
                startHeight = 15,
                startDepth = 20;

            var grid = new Grid3D<int>(startWidth, startHeight, startDepth);

            for (var x = 0; x < grid.Width; x++)
                for (var y = 0; y < grid.Height; y++)
                    for (var z = 0; z < grid.Depth; z++)
                        grid[x, y, z] = x * startHeight + y * startDepth + z;

            grid.Resize(20, 20, 25);

            for (var x = 0; x < startWidth; x++)
                for (var y = 0; y < startHeight; y++)
                    for (var z = 0; z < startDepth; z++)
                        Assert.AreEqual(x * startHeight + y * startDepth + z, grid[x, y, z], "({0},{1},{2})", x, y, z);
        }

        [Test]
        public void Resize_NewCellsHaveDefaultValue()
        {
            var startWidth = 10;
            var startHeight = 10;
            var startDepth = 10;
            var defaultVal = 5;
            var grid = new Grid3D<int>(startWidth, startHeight, startDepth, defaultVal);
            grid.Clear(10);

            grid.Resize(20, 20, 20);

            for (var x = 0; x < grid.Width; x++)
            {
                for (var y = 0; y < grid.Height; y++)
                {
                    for (var z = 0; z < grid.Depth; z++)
                    {
                        if (x >= startWidth || y >= startHeight || z >= startDepth)
                            Assert.AreEqual(defaultVal, grid[x, y, z], "({0},{1},{2})", x, y, z);
                        else
                            Assert.AreEqual(10, grid[x, y, z], "({0},{1},{2})", x, y, z);
                    }
                }
            }
        }
    }
}