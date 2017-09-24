using System.Collections.Generic;
using System.Linq;
using Archon.SwissArmyLib.Partitioning;
using NUnit.Framework;
using UnityEngine;

namespace Archon.SwissArmyLibTests.Partitioning.Tests
{
    [TestFixture]
    public class Bin3DTests
    {
        private static Bounds GetBoundsInsideCell(int x, int y, int z, float cellWidth, float cellHeight, float cellDepth)
        {
            const float margin = 0.0001f;
            return new Bounds(new Vector3(x + cellWidth / 2, y + cellHeight / 2, z + cellDepth / 2),
                new Vector3(cellWidth - margin * 2, cellHeight - margin * 2, cellDepth - margin * 2));
        }

        private static Bounds GetBoundsInsideCells(int minX, int minY, int minZ, int maxX, int maxY, int maxZ, float cellWidth,
            float cellHeight, float cellDepth)
        {
            var min = GetBoundsInsideCell(minX, minY, minZ, cellWidth, cellHeight, cellDepth);
            var max = GetBoundsInsideCell(maxX, maxY, maxZ, cellWidth, cellHeight, cellDepth);

            min.Encapsulate(max);

            return min;
        }

        [Test]
        public void Constructor_CorrectSize()
        {
            var bin = new Bin3D<string>(9, 18, 7, 1, 2, 3);

            Assert.AreEqual(9, bin.Width);
            Assert.AreEqual(18, bin.Height);
            Assert.AreEqual(7, bin.Depth);

            Assert.AreEqual(1, bin.CellWidth, 0.001f);
            Assert.AreEqual(2, bin.CellHeight, 0.001f);
            Assert.AreEqual(3, bin.CellDepth, 0.001f);
        }

        [TestCase(0, 8, 0)]
        [TestCase(4, 5, 7)]
        public void Insert_InsideCell_CorrectCell(int expectedX, int expectedY, int expectedZ)
        {
            var bin = new Bin3D<object>(9, 9, 9, 1, 1, 1);
            var val = new object();

            bin.Insert(val, GetBoundsInsideCell(expectedX, expectedY, expectedZ, 1, 1, 1));

            for (var x = 0; x < bin.Width; x++)
            {
                for (var y = 0; y < bin.Height; y++)
                {
                    for (var z = 0; z < bin.Depth; z++)
                    {
                        if (x == expectedX && y == expectedY)
                            continue;

                        Assert.IsNull(bin[x, y, z]);
                    }
                }
            }

            Assert.IsNotNull(bin[expectedX, expectedY, expectedZ]);
            Assert.IsTrue(bin[expectedX, expectedY, expectedZ].Contains(val));
        }

        [TestCase(-1, 0, 0)]
        [TestCase(0, -1, 0)]
        [TestCase(0, 0, -1)]
        [TestCase(9, 0, 0)]
        [TestCase(0, 9, 0)]
        [TestCase(0, 0, 9)]
        public void Insert_OutOfBounds_NotAdded(int cellX, int cellY, int cellZ)
        {
            var bin = new Bin3D<object>(9, 9, 9, 1, 1, 1);
            var val = new object();

            bin.Insert(val, GetBoundsInsideCell(cellX, cellY, cellZ, 1, 1, 1));

            for (var x = 0; x < bin.Width; x++)
                for (var y = 0; y < bin.Height; y++)
                    for (var z = 0; z < bin.Depth; z++)
                        Assert.IsNull(bin[x, y, z]);
        }

        [TestCase(1, 1, 1, 7, 1, 1)] // overlap x
        [TestCase(1, 1, 1, 1, 7, 1)] // overlap y
        [TestCase(1, 1, 1, 1, 1, 7)] // overlap z
        [TestCase(1, 1, 1, 7, 7, 7)] // overlap all
        [TestCase(-1, -1, -1, 9, 9, 9)] // fully covered
        [TestCase(-1, -1, -1, -1, -1, -1)] // out of bounds
        public void Insert_Overlap_CorrectCells(int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
        {
            var bin = new Bin3D<object>(9, 9, 9, 1, 1, 1);
            var val = new object();

            var rect = GetBoundsInsideCells(minX, minY, minZ, maxX, maxY, maxZ, 1, 1, 1);
            bin.Insert(val, rect);

            for (var x = 0; x < bin.Width; x++)
            {
                for (var y = 0; y < bin.Height; y++)
                {
                    for (var z = 0; z < bin.Depth; z++)
                    {
                        if (x >= minX && y >= minY && z >= minZ
                            && x <= maxX && y <= maxY && z <= maxZ)
                        {
                            Assert.IsNotNull(bin[x, y, z]);
                            Assert.IsTrue(bin[x, y, z].Contains(val));
                        }
                        else
                            Assert.IsNull(bin[x, y, z]);
                    }
                }
            }
        }

        [Test]
        public void Retrieve_Empty_NoResults()
        {
            var bin = new Bin3D<object>(9, 9, 9, 1, 1, 1);
            var rect = GetBoundsInsideCells(-1, -1, -1, bin.Width, bin.Height, bin.Depth, bin.CellWidth, bin.CellHeight, bin.CellDepth);
            var results = new HashSet<object>();

            bin.Retrieve(rect, results);

            Assert.IsEmpty(results);
        }

        [TestCase(0, 0, 0, 0, 0, 0)]
        [TestCase(8, 8, 8, 8, 8, 8)]
        [TestCase(4, 1, 4, 1, 4, 1)]
        [TestCase(-1, -1, -1, 9, 9, 9)] // full overlap
        [TestCase(1, 1, 1, 7, 7, 7)]
        [TestCase(4, 1, 5, 7, 3, 7)]
        public void Retrieve_CorrectResults(int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
        {
            var bin = new Bin3D<object>(9, 9, 9, 1, 1, 1);
            var results = new HashSet<object>();
            var val = new object();
            var bounds = GetBoundsInsideCells(minX, minY, minZ, maxX, maxY, maxZ, bin.CellWidth, bin.CellHeight, bin.CellDepth);

            bin.Insert(val, bounds);
            bin.Retrieve(bounds, results);

            Assert.True(results.Contains(val));
        }

        [Test]
        public void Remove_RemovedFromCells()
        {
            var bin = new Bin3D<object>(9, 9, 9, 1, 1, 1);
            var results = new HashSet<object>();
            var val = new object();
            var bounds = GetBoundsInsideCells(3, 6, 4, 7, 8, 6, bin.CellWidth, bin.CellHeight, bin.CellDepth);

            bin.Insert(val, bounds);
            bin.Remove(val, bounds);
            bin.Retrieve(bounds, results);

            Assert.False(results.Contains(val));
        }

        [Test]
        public void Clear_AllCellsEmpty()
        {
            var bin = new Bin3D<object>(9, 9, 9, 1, 1, 1);
            var results = new HashSet<object>();
            var fullBounds = GetBoundsInsideCells(-1, -1, -1, bin.Width, bin.Height, bin.Depth, bin.CellWidth, bin.CellHeight, bin.CellDepth);

            bin.Retrieve(fullBounds, results);

            Assert.AreEqual(0, results.Count);
        }
    }
}
