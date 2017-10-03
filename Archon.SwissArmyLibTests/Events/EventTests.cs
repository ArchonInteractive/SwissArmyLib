using System;
using NUnit.Framework;

namespace Archon.SwissArmyLib.Events.Tests
{
    [TestFixture]
    public class EventTests
    {
        public class InterfaceListener : IEventListener
        {
            public Action Callback;

            public InterfaceListener(Action callback)
            {
                Callback = callback;
            }

            public void OnEvent(int eventId)
            {
                Callback?.Invoke();
            }
        }

        [Test]
        public void AddListener_Interface_ListenerIsAdded()
        {
            var e = new Event(0);
            var listener = new InterfaceListener(null);

            e.AddListener(listener);

            Assert.IsTrue(e.HasListener(listener));
        }

        [Test]
        public void AddListener_Delegate_ListenerIsAdded()
        {
            var e = new Event(0);
            Action listener = () => { };

            e.AddListener(listener);

            Assert.IsTrue(e.HasListener(listener));
        }

        [Test]
        public void RemoveListener_Interface_ListenerIsRemoved()
        {
            var e = new Event(0);
            var listener = new InterfaceListener(null);

            e.AddListener(listener);
            e.RemoveListener(listener);

            Assert.IsFalse(e.HasListener(listener));
        }

        [Test]
        public void RemoveListener_Delegate_ListenerIsRemoved()
        {
            var e = new Event(0);
            Action listener = () => { };

            e.AddListener(listener);
            e.RemoveListener(listener);

            Assert.IsFalse(e.HasListener(listener));
        }

        [Test]
        public void RemoveListener_Empty_NoChange()
        {
            var e = new Event(0);
            var interfaceListener = new InterfaceListener(null);
            Action delegateListener = () => { };

            e.RemoveListener(interfaceListener);
            e.RemoveListener(delegateListener);

            Assert.IsEmpty(e.Listeners);
        }

        [Test]
        public void Clear_AllListenersRemoved()
        {
            var e = new Event(0);

            for (var i = 0; i < 10; i++)
            {
                e.AddListener(new InterfaceListener(null));
                e.AddListener(() => { });
            }

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
                e.AddListener(new InterfaceListener(() => callCount++));
                e.AddListener(() => callCount++);
            }

            e.Invoke();

            Assert.AreEqual(20, callCount);
        }

        [Test]
        public void Invoke_ListenerException_NoThrow()
        {
            var e = new Event(0);
            e.SuppressExceptions = true;

            for (var i = 0; i < 10; i++)
            {
                e.AddListener(new InterfaceListener(() => throw new Exception()));
                e.AddListener(() => throw new Exception());
            }

            Assert.DoesNotThrow(() => e.Invoke());
        }

        [Test]
        public void Invoke_AddListener_ListenerIsAddedNextCall()
        {
            var wasCalled = false;
            var e = new Event(0);
            var secondListener = new InterfaceListener(() => wasCalled = true);
            var firstListener = new InterfaceListener(() => e.AddListener(secondListener));

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
            var secondListener = new InterfaceListener(() => wasCalled = true);
            var firstListener = new InterfaceListener(() => e.RemoveListener(secondListener));

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
