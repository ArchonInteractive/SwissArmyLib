using System;
using NUnit.Framework;

namespace Archon.SwissArmyLib.Events.Tests
{
    [TestFixture]
    public class EventTests
    {
        public class Listener : IEventListener
        {
            public Action Callback;

            public Listener(Action callback)
            {
                Callback = callback;
            }

            public void OnEvent(int eventId)
            {
                Callback?.Invoke();
            }
        }

        [Test]
        public void AddListener_ListenerIsAdded()
        {
            var e = new Event(0);
            var listener = new Listener(null);

            e.AddListener(listener);

            Assert.IsTrue(e.HasListener(listener));
        }

        [Test]
        public void RemoveListener_ListenerIsRemoved()
        {
            var e = new Event(0);
            var listener = new Listener(null);

            e.AddListener(listener);
            e.RemoveListener(listener);

            Assert.IsFalse(e.HasListener(listener));
        }

        [Test]
        public void RemoveListener_Empty_NoChange()
        {
            var e = new Event(0);
            var listener = new Listener(null);

            e.RemoveListener(listener);

            Assert.IsEmpty(e.Listeners);
        }

        [Test]
        public void Clear_AllListenersRemoved()
        {
            var e = new Event(0);

            for (var i = 0; i < 10; i++)
                e.AddListener(new Listener(null));

            e.Clear();

            Assert.IsEmpty(e.Listeners);
        }

        [Test]
        public void Invoke_AllListenersCalled()
        {
            var e = new Event(0);
            var callCount = 0;
            for (var i = 0; i < 10; i++)
            {
                var listener = new Listener(() => callCount++);
                e.AddListener(listener);
            }

            e.Invoke();

            Assert.AreEqual(10, callCount);
        }

        [Test]
        public void Invoke_ListenerException_NoThrow()
        {
            var e = new Event(0);
            e.SuppressExceptions = true;

            for (var i = 0; i < 10; i++)
            {
                var listener = new Listener(() => throw new Exception());
                e.AddListener(listener);
            }

            Assert.DoesNotThrow(() => e.Invoke());
        }

        [Test]
        public void Invoke_AddListener_ListenerIsAddedNextCall()
        {
            var wasCalled = false;
            var e = new Event(0);
            var secondListener = new Listener(() => wasCalled = true);
            var firstListener = new Listener(() => e.AddListener(secondListener));

            e.AddListener(firstListener);
            e.Invoke();

            Assert.IsFalse(wasCalled);
            e.Invoke();

            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void Invoke_RemoveListener_ListenerIsRemovedNextCall()
        {
            var wasCalled = false;
            var e = new Event(0);
            var secondListener = new Listener(() => wasCalled = true);
            var firstListener = new Listener(() => e.RemoveListener(secondListener));

            e.AddListener(firstListener);
            e.AddListener(secondListener);
            e.Invoke();

            Assert.IsTrue(wasCalled);
            wasCalled = false;

            e.Invoke();
            Assert.IsFalse(wasCalled);
        }
    }
}
