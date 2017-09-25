using System.Collections.Generic;
using System.Linq;
using Archon.SwissArmyLib.Partitioning;
using NUnit.Framework;
using UnityEngine;

namespace Archon.SwissArmyLibTests.Partitioning.Tests
{
    [TestFixture]
    public class Bin2DTests
    {
        private static Rect GetRectInsideCell(int x, int y, float cellWidth, float cellHeight)
        {
            const float margin = 0.0001f;
            return new Rect(x + margin, y + margin, cellWidth - margin * 2, cellHeight - margin * 2);
        }

        private static Rect GetRectInsideCells(int minX, int minY, int maxX, int maxY, float cellWidth,
            float cellHeight)
        {
            var min = GetRectInsideCell(minX, minY, cellWidth, cellHeight);
            var max = GetRectInsideCell(maxX, maxY, cellWidth, cellHeight);

            return Rect.MinMaxRect(min.xMin, min.yMin, max.xMax, max.yMax);
        }

        [Test]
        public void Constructor_CorrectSize()
        {
            var bin = new Bin2D<object>(9, 18, 1, 2);

            Assert.AreEqual(9, bin.Width);
            Assert.AreEqual(18, bin.Height);
            Assert.AreEqual(1, bin.CellWidth, 0.001f);
            Assert.AreEqual(2, bin.CellHeight, 0.001f);
        }

        [TestCase(0, 8)]
        [TestCase(4, 5)]
        public void Insert_InsideCell_CorrectCell(int expectedX, int expectedY)
        {
            var bin = new Bin2D<object>(9, 9, 1, 1);
            var val = new object();

            bin.Insert(val, GetRectInsideCell(expectedX, expectedY, 1, 1));

            for (var x = 0; x < bin.Width; x++)
            {
                for (var y = 0; y < bin.Height; y++)
                {
                    if (x == expectedX && y == expectedY)
                        continue;

                    Assert.IsNull(bin[x, y]);
                }
            }

            Assert.IsNotNull(bin[expectedX, expectedY]);
            Assert.IsTrue(bin[expectedX, expectedY].Contains(val));
        }

        [TestCase(-1, 0)]
        [TestCase(9, 0)]
        [TestCase(0, -1)]
        [TestCase(0, 9)]
        public void Insert_OutOfBounds_NotAdded(int cellX, int cellY)
        {
            var bin = new Bin2D<object>(9, 9, 1, 1);
            var val = new object();

            bin.Insert(val, GetRectInsideCell(cellX, cellY, 1, 1));

            for (var x = 0; x < bin.Width; x++)
                for (var y = 0; y < bin.Height; y++)
                    Assert.IsNull(bin[x, y]);
        }

        [TestCase(1, 1, 7, 1)] // overlap x
        [TestCase(1, 1, 1, 7)] // overlap y
        [TestCase(1, 1, 7, 7)] // overlap both
        [TestCase(-1, -1, 9, 9)] // fully covered
        [TestCase(-1, -1, -1, -1)] // out of bounds
        public void Insert_Overlap_CorrectCells(int minX, int minY, int maxX, int maxY)
        {
            var bin = new Bin2D<object>(9, 9, 1, 1);
            var val = new object();

            var rect = GetRectInsideCells(minX, minY, maxX, maxY, 1, 1);
            bin.Insert(val, rect);

            for (var x = 0; x < bin.Width; x++)
            {
                for (var y = 0; y < bin.Height; y++)
                {
                    if (x >= minX && y >= minY
                        && x <= maxX && y <= maxY)
                    {
                        Assert.IsNotNull(bin[x, y]);
                        Assert.IsTrue(bin[x, y].Contains(val));
                    }
                    else
                        Assert.IsNull(bin[x, y]);
                }
            }
        }

        [Test]
        public void Retrieve_Empty_NoResults()
        {
            var bin = new Bin2D<object>(9, 9, 1, 1);
            var rect = GetRectInsideCells(-1, -1, bin.Width, bin.Height, bin.CellWidth, bin.CellHeight);
            var results = new HashSet<object>();

            bin.Retrieve(rect, results);

            Assert.IsEmpty(results);
        }

        [TestCase(0, 0, 0, 0)]
        [TestCase(8, 8, 8, 8)]
        [TestCase(4, 1, 4, 1)]
        [TestCase(-1, -1, 9, 9)] // full overlap
        [TestCase(1, 1, 7, 7)]
        [TestCase(4, 1, 7, 3)]
        public void Retrieve_CorrectResults(int minX, int minY, int maxX, int maxY)
        {
            var bin = new Bin2D<object>(9, 9, 1, 1);
            var results = new HashSet<object>();
            var val = new object();
            var bounds = GetRectInsideCells(minX, minY, maxX, maxY, bin.CellWidth, bin.CellHeight);

            bin.Insert(val, bounds);
            bin.Retrieve(bounds, results);

            Assert.True(results.Contains(val));
        }


        [Test]
        public void Remove_NoBounds_RemovedFromCells()
        {
            var bin = new Bin2D<object>(9, 9, 1, 1);
            var results = new HashSet<object>();
            var val = new object();
            var bounds = GetRectInsideCells(3, 6, 7, 8, bin.CellWidth, bin.CellHeight);

            bin.Insert(val, bounds);
            bin.Remove(val);
            bin.Retrieve(bounds, results);

            Assert.False(results.Contains(val));
        }

        [Test]
        public void Remove_WithBounds_RemovedFromCells()
        {
            var bin = new Bin2D<object>(9, 9, 1, 1);
            var results = new HashSet<object>();
            var val = new object();
            var bounds = GetRectInsideCells(3, 6, 7, 8, bin.CellWidth, bin.CellHeight);

            bin.Insert(val, bounds);
            bin.Remove(val, bounds);
            bin.Retrieve(bounds, results);

            Assert.False(results.Contains(val));
        }

        [Test]
        public void Clear_AllCellsEmpty()
        {
            var bin = new Bin2D<object>(9, 9, 1, 1);
            var results = new HashSet<object>();
            var fullBounds = GetRectInsideCells(-1, -1, bin.Width, bin.Height, bin.CellWidth, bin.CellHeight);

            bin.Retrieve(fullBounds, results);

            Assert.AreEqual(0, results.Count);
        }
    }
}
