using NUnit.Framework;

namespace Archon.SwissArmyLib.Collections.Tests
{
    [TestFixture]
    public class PrioritizedListTests
    {
        private static void Fill(PrioritizedList<object> list, int startPriority, int endPriority)
        {
            for (var priority = startPriority; priority <= endPriority; priority++)
                list.Add(new object(), priority);
        }

        [Test]
        public void Add_ItemIsAdded()
        {
            var item = new object();
            var list = new PrioritizedList<object>();

            list.Add(item);

            Assert.IsTrue(list.Contains(item));
        }

        [Test]
        public void Add_LowestPriority_IsAddedFirst()
        {
            var item = new object();
            var list = new PrioritizedList<object>();

            Fill(list, -5, 5);
            list.Add(item, -100);

            Assert.AreSame(item, list[0].Item);
        }

        [Test]
        public void Add_HighestPriority_IsAddedLast()
        {
            var item = new object();
            var list = new PrioritizedList<object>();

            Fill(list, -5, 5);
            list.Add(item, 100);

            Assert.AreSame(item, list[list.Count - 1].Item);
        }

        [Test]
        public void Add_SamePriority_IsAddedAfterOthers()
        {
            var item = new object();
            var list = new PrioritizedList<object>();

            Fill(list, -5, 5);
            list.Add(item, list[3].Priority);

            Assert.AreSame(item, list[4].Item);
        }

        [Test]
        public void Remove_TargetIsRemoved()
        {
            var item = new object();
            var list = new PrioritizedList<object> { item };

            Fill(list, -5, 5);
            list.Remove(item);

            Assert.IsFalse(list.Contains(item));
        }

        [Test]
        public void Remove_Empty_NoErrors()
        {
            var item = new object();
            var list = new PrioritizedList<object>();

            Assert.DoesNotThrow(() => list.Remove(item));
        }

        [Test]
        public void Contains_True()
        {
            var item = new object();
            var list = new PrioritizedList<object> { item };

            Fill(list, -5, 5);

            Assert.IsTrue(list.Contains(item));
        }

        [Test]
        public void Contains_False()
        {
            var item = new object();
            var list = new PrioritizedList<object>();

            Fill(list, -5, 5);

            Assert.IsFalse(list.Contains(item));
        }

        [Test]
        public void IndexOf_FindsItem()
        {
            var item = new object();
            var list = new PrioritizedList<object>();

            Fill(list, -5, 5);
            list.Add(item, list[4].Priority);

            var index = list.IndexOf(item);
            Assert.AreEqual(5, index);
        }

        [Test]
        public void Clear_EverythingIsRemoved()
        {
            var list = new PrioritizedList<object>();

            Fill(list, -5, 5);
            list.Clear();

            Assert.AreEqual(0, list.Count);
        }
    }
}