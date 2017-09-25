using Archon.SwissArmyLib.Partitioning;
using NUnit.Framework;
using UnityEngine;
using Random = System.Random;

namespace Archon.SwissArmyLibTests.Partitioning.Tests
{
    [TestFixture]
    public class QuadtreeTests
    {
        private static Vector2 GetPoint(Quadtree<object> tree, float x, float y)
        {
            var actualX = Mathf.LerpUnclamped(tree.Bounds.xMin, tree.Bounds.xMax, x);
            var actualY = Mathf.LerpUnclamped(tree.Bounds.yMin, tree.Bounds.yMax, y);

            return new Vector2(actualX, actualY);
        }

        private static Rect GetRectAtPoint(Quadtree<object> tree, float x, float y)
        {
            return new Rect(GetPoint(tree, x, y), Vector2.zero);
        }

        private static Rect GetMinMaxRect(Quadtree<object> tree, float minX, float minY, float maxX, float maxY)
        {
            var min = GetPoint(tree, minX, minY);
            var max = GetPoint(tree, maxX, maxY);

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private static ItemWithBounds[] FillWithItems(Quadtree<object> tree, int amount, int seed = 0)
        {
            var random = new Random(seed);
            var items = new ItemWithBounds[amount];

            for (var i = 0; i < items.Length; i++)
            {
                var item = new ItemWithBounds(GetRectAtPoint(tree, (float)random.NextDouble(), (float)random.NextDouble()));

                items[i] = item;
                tree.Insert(item, item.Bounds);
            }

            return items;
        }

        [Test]
        public void Create_CorrectState()
        {
            var treeBounds = Rect.MinMaxRect(-500, -400, 300, 200);
            var tree = Quadtree<object>.Create(treeBounds, 3, 5);

            Assert.AreEqual(0, tree.Count);
            Assert.AreEqual(0, tree.Depth);
            Assert.AreEqual(5, tree.MaxDepth);
            Assert.AreEqual(3, tree.MaxItems);
            Assert.IsFalse(tree.IsSplit);

            Assert.AreEqual(-500, tree.Bounds.min.x);
            Assert.AreEqual(-400, tree.Bounds.min.y);
            Assert.AreEqual(300, tree.Bounds.max.x);
            Assert.AreEqual(200, tree.Bounds.max.y);
        }

        [Test]
        public void Insert_Centered_AddedInAllSubtrees()
        {
            var treeBounds = Rect.MinMaxRect(-500, -400, 300, 200);
            var tree = Quadtree<object>.Create(treeBounds, 3, 5);

            FillWithItems(tree, 3);

            var item = new object();
            var itemBounds = GetMinMaxRect(tree, 0.4f, 0.4f, 0.6f, 0.6f);

            tree.Insert(item, itemBounds);

            var topLeftResults = tree.Retrieve(GetRectAtPoint(tree, 0.4f, 0.6f));
            var topRightResults = tree.Retrieve(GetRectAtPoint(tree, 0.6f, 0.6f));
            var bottomLeftResults = tree.Retrieve(GetRectAtPoint(tree, 0.4f, 0.4f));
            var bottomRightResults = tree.Retrieve(GetRectAtPoint(tree, 0.6f, 0.4f));

            Assert.IsTrue(tree.IsSplit);
            Assert.IsTrue(topLeftResults.Contains(item));
            Assert.IsTrue(topRightResults.Contains(item));
            Assert.IsTrue(bottomLeftResults.Contains(item));
            Assert.IsTrue(bottomRightResults.Contains(item));
        }

        [Test]
        public void Insert_TooManyItems_TreeIsSplit()
        {
            var treeBounds = Rect.MinMaxRect(-500, -400, 300, 200);
            var tree = Quadtree<object>.Create(treeBounds, 2, 5);

            for (var i = 0; i < 5; i++)
                tree.Insert(null, GetRectAtPoint(tree, 0, 0));

            Assert.AreEqual(5, tree.Count);
            Assert.IsTrue(tree.IsSplit);
        }

        [Test]
        public void Insert_MaxDepth_NotSplit()
        {
            var treeBounds = Rect.MinMaxRect(-500, -400, 300, 200);
            var tree = Quadtree<object>.Create(treeBounds, 2, 0);
            
            for (var i = 0; i < 5; i++)
                tree.Insert(null, GetRectAtPoint(tree, 0, 0));

            Assert.AreEqual(5, tree.Count);
            Assert.IsFalse(tree.IsSplit);
        }

        [Test]
        public void Retrieve_Empty_NoResults()
        {
            var treeBounds = Rect.MinMaxRect(-500, -400, 300, 200);
            var tree = Quadtree<object>.Create(treeBounds, 3, 5);

            var results = tree.Retrieve(GetMinMaxRect(tree, -1, -1, 2, 2));

            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public void Retrieve_CorrectResults()
        {
            var treeBounds = Rect.MinMaxRect(-500, -400, 300, 200);
            var tree = Quadtree<object>.Create(treeBounds, 1, 5);
            
            // Looks somewhat like this:  
            //
            // +----------------+----------------+
            // |                |                |
            // |  +------------------+           |
            // |  |             |    |           |
            // |  |             |    |           |
            // |  |             |    |           |
            // |  |             |  +----------+  |
            // |  +------------------+        |  |
            // |                |  |          |  |
            // +---------------------------------+
            // |                |  |          |  |
            // |                |  |          |  |
            // |                |  |          |  |
            // |                |  |          |  |
            // |                |  |          |  |
            // |                |  |          |  |
            // |                |  +----------+  |
            // |                |                |
            // +----------------+----------------+

            var topItem = new ItemWithBounds(GetMinMaxRect(tree, 0.1f, 0.6f, 0.7f, 0.9f));
            var bottomItem = new ItemWithBounds(GetMinMaxRect(tree, 0.6f, 0.1f, 0.9f, 0.7f));

            tree.Insert(topItem, topItem.Bounds);
            tree.Insert(bottomItem, bottomItem.Bounds);

            // sorry for anybody who is reading this shitty code..

            var topLeft = tree.Retrieve(GetMinMaxRect(tree, 0, 0.51f, 0.49f, 1f));
            var topRight = tree.Retrieve(GetMinMaxRect(tree, 0.51f, 0.51f, 1, 1));
            var bottomLeft = tree.Retrieve(GetMinMaxRect(tree, 0, 0, 0.49f, 0.49f));
            var bottomRight = tree.Retrieve(GetMinMaxRect(tree, 0.51f, 0, 1, 0.49f));

            // better names anybody? please?
            // ReSharper disable InconsistentNaming
            var topRight_topLeft = tree.Retrieve(GetMinMaxRect(tree, 0.51f, 0.76f, 0.74f, 1));
            var topRight_topRight = tree.Retrieve(GetMinMaxRect(tree, 0.76f, 0.76f, 1, 1));
            var topRight_bottomLeft = tree.Retrieve(GetMinMaxRect(tree, 0.51f, 0.51f, 0.74f, 0.74f));
            var topRight_bottomRight = tree.Retrieve(GetMinMaxRect(tree, 0.76f, 0.51f, 1, 0.74f));
            // ReSharper restore InconsistentNaming

            Assert.IsTrue(tree.IsSplit);

            Assert.AreEqual(1, topLeft.Count);
            Assert.AreEqual(2, topRight.Count);
            Assert.AreEqual(0, bottomLeft.Count);
            Assert.AreEqual(1, bottomRight.Count);

            Assert.AreEqual(1, topRight_topLeft.Count);
            Assert.AreEqual(0, topRight_topRight.Count);
            Assert.AreEqual(2, topRight_bottomLeft.Count);
            Assert.AreEqual(1, topRight_bottomRight.Count);

            Assert.IsTrue(topLeft.Contains(topItem));
            Assert.IsTrue(bottomRight.Contains(bottomItem));
        }

        [Test]
        public void Remove_SubtreesMerged()
        {
            var treeBounds = Rect.MinMaxRect(-500, -400, 300, 200);
            var tree = Quadtree<object>.Create(treeBounds, 4, 5);

            var items = FillWithItems(tree, tree.MaxItems + 1);

            tree.Remove(items[0], items[0].Bounds);

            Assert.IsFalse(tree.IsSplit);
        }

        [Test]
        public void Remove_RemovedFromNodes()
        {
            var treeBounds = Rect.MinMaxRect(-500, -400, 300, 200);
            var tree = Quadtree<object>.Create(treeBounds, 4, 5);

            var items = FillWithItems(tree, 20);

            tree.Remove(items[0], items[0].Bounds);

            var results = tree.Retrieve(GetMinMaxRect(tree, -1, -1, 2, 2));

            Assert.IsFalse(results.Contains(items[0]));
        }

        [Test]
        public void Clear_AllRemoved()
        {
            var treeBounds = Rect.MinMaxRect(-500, -400, 300, 200);
            var tree = Quadtree<object>.Create(treeBounds, 3, 5);

            var random = new Random(0);

            for (var i = 0; i < 50; i++)
            {
                var x = (float) random.NextDouble();
                var y = (float) random.NextDouble();
                tree.Insert(null, GetRectAtPoint(tree, x, y));
            }

            tree.Clear();

            Assert.AreEqual(0, tree.Count);
            Assert.IsFalse(tree.IsSplit);
        }

        private class ItemWithBounds
        {
            public readonly Rect Bounds;

            public ItemWithBounds(Rect bounds)
            {
                Bounds = bounds;
            }
        }
    }
}
