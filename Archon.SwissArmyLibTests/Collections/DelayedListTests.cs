using System.Collections.Generic;
using NUnit.Framework;

namespace Archon.SwissArmyLib.Collections.Tests
{
    [TestFixture]
    public class DelayedListTests
    {
        private static void Fill(ICollection<object> list, int amount)
        {
            for (var i = 0; i < amount; i++)
                list.Add(new object());
        }

        [Test]
        public void Add_Unprocessed_NotAddedYet()
        {
            var list = new DelayedList<object>();

            list.Add(new object());

            Assert.IsEmpty(list);
        }

        [Test]
        public void Add_Processed_ItemIsAdded()
        {
            var list = new DelayedList<object>();
            var item = new object();

            list.Add(item);
            list.ProcessPending();

            Assert.IsTrue(list.Contains(item));
        }

        [Test]
        public void AddThenRemove_Processed_NotAdded()
        {
            var list = new DelayedList<object>();
            var item = new object();

            list.Add(item);
            list.Remove(item);
            list.ProcessPending();

            Assert.IsFalse(list.Contains(item));
        }

        [Test]
        public void RemoveThenAdd_Processed_Added()
        {
            var list = new DelayedList<object>();
            var item = new object();

            list.Remove(item);
            list.Add(item);
            list.ProcessPending();

            Assert.IsTrue(list.Contains(item));
        }

        [Test]
        public void Remove_Unprocessed_NotRemovedYet()
        {
            var list = new DelayedList<object>();
            var item = new object();

            list.Add(item);
            list.ProcessPending();
            list.Remove(item);

            Assert.IsTrue(list.Contains(item));
        }

        [Test]
        public void Remove_Processed_ItemIsRemoved()
        {
            var list = new DelayedList<object>();
            var item = new object();

            list.Add(item);
            list.ProcessPending();
            list.Remove(item);
            list.ProcessPending();

            Assert.IsFalse(list.Contains(item));
        }

        [Test]
        public void Remove_Empty_Processed_NothingChanged()
        {
            var list = new DelayedList<object>();
            var item = new object();

            list.Remove(item);
            list.ProcessPending();

            Assert.IsEmpty(list);
        }

        [Test]
        public void Remove_NotInList_Processed_NothingChanged()
        {
            var list = new DelayedList<object>();
            var item = new object();

            Fill(list, 5);
            list.ProcessPending();
            list.Remove(item);
            list.ProcessPending();

            Assert.AreEqual(5, list.Count);
        }

        [Test]
        public void AddThenClear_Processed_NoItemsAdded()
        {
            var list = new DelayedList<object>();

            Fill(list, 5);
            list.Clear();
            list.ProcessPending();

            Assert.IsEmpty(list);
        }

        [Test]
        public void Clear_Processed_AllItemsRemoved()
        {
            var list = new DelayedList<object>();

            Fill(list, 5);
            list.ProcessPending();

            list.Clear();
            list.ProcessPending();

            Assert.IsEmpty(list);
        }

        [Test]
        public void ClearThenAdd_Processed_OnlyAddedItemLeft()
        {
            var list = new DelayedList<object>();
            var item = new object();

            Fill(list, 5);
            list.ProcessPending();

            list.Clear();
            list.Add(item);
            list.ProcessPending();

            Assert.AreEqual(1, list.Count);
            Assert.AreSame(item, list[0]);
        }

        [Test]
        public void ClearInstantly_Unprocessed_AllItemsRemoved()
        {
            var list = new DelayedList<object>();

            Fill(list, 5);
            list.ProcessPending();
            list.ClearInstantly();

            Assert.IsEmpty(list);
        }

        [Test]
        public void AddThenClearPending_Processed_NoItemsAdded()
        {
            var list = new DelayedList<object>();

            Fill(list, 5);
            list.ClearPending();
            list.ProcessPending();

            Assert.IsEmpty(list);
        }
    }
}