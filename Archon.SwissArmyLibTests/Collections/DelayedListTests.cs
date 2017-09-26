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
        public void Clear_AllItemsRemoved()
        {
            var list = new DelayedList<object>();

            Fill(list, 5);
            list.ProcessPending();
            list.Clear();

            Assert.IsEmpty(list);
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
        public void PreItemAddition_CalledOnProcess()
        {
            var list = new DelayedList<object>();

            Fill(list, 5);

            list.PreItemAddition += items =>
            {
                Assert.Pass();
            };

            list.ProcessPending();

            Assert.Fail();
        }

        [Test]
        public void PreItemAddition_CorrectItems()
        {
            var list = new DelayedList<object>();

            var items = new List<object>();
            Fill(items, 5);
            list.AddRange(items);

            list.PreItemAddition += itemsBeingAdded =>
            {
                foreach (var item in items)
                    Assert.IsTrue(itemsBeingAdded.Contains(item));
            };

            list.ProcessPending();
        }

        [Test]
        public void PreItemRemoval_CalledOnProcess()
        {
            var list = new DelayedList<object>();

            Fill(list, 5);

            list.PreItemRemoval += items =>
            {
                Assert.Pass();
            };

            list.ProcessPending();

            Assert.Fail();
        }

        [Test]
        public void PreItemRemoval_CorrectItems()
        {
            var list = new DelayedList<object>();

            var items = new List<object>();
            Fill(items, 5);
            list.AddRange(items);
            list.ProcessPending();

            foreach (var item in items)
                list.Remove(item);

            list.PreItemRemoval += itemsBeingRemoved =>
            {
                foreach (var item in items)
                    Assert.IsTrue(itemsBeingRemoved.Contains(item));
            };

            list.ProcessPending();
        }
    }
}