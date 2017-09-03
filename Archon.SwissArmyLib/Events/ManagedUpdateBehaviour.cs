using UnityEngine;
using EventIds = Archon.SwissArmyLib.Events.ManagedUpdate.EventIds;

namespace Archon.SwissArmyLib.Events
{
    /// <summary>
    /// A subclass of MonoBehaviour that uses <see cref="ManagedUpdate"/> for update events.
    /// </summary>
    public abstract class ManagedUpdateBehaviour : MonoBehaviour, IEventListener
    {
        /// <summary>
        /// Affects whether this components' events will be called before or after others'.
        /// 
        /// Basically a reimplementation of Unity's ScriptExecutionOrder.
        /// </summary>
        protected virtual int ExecutionOrder { get { return 0; } }

        /// <summary>
        /// Whether you want to use <see cref="OnUpdate"/>.
        /// </summary>
        protected abstract bool UsesUpdate { get; }

        /// <summary>
        /// Whether you want to use <see cref="OnLateUpdate"/>.
        /// </summary>
        protected abstract bool UsesLateUpdate { get; }

        /// <summary>
        /// Whether you want to use <see cref="OnFixedUpdate"/>.
        /// </summary>
        protected abstract bool UsesFixedUpdate { get; }

        private bool _startWasCalled;

        /// <summary>
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
        /// </summary>
        protected virtual void Start()
        {
            _startWasCalled = true;
            StartListening();
        }

        /// <summary>
        /// Called when the component is enabled.
        /// </summary>
        protected virtual void OnEnable()
        {
            // We don't want Update calls before Start has been called.
            if (_startWasCalled)
                StartListening();
        }

        /// <summary>
        /// Called when the component is disabled.
        /// </summary>
        protected virtual void OnDisable()
        {
            if (_startWasCalled)
                StopListening();
        }

        private void StartListening()
        {
            if (UsesUpdate)
                ManagedUpdate.OnUpdate.AddListener(this, ExecutionOrder);
            if (UsesLateUpdate)
                ManagedUpdate.OnLateUpdate.AddListener(this, ExecutionOrder);
            if (UsesFixedUpdate)
                ManagedUpdate.OnFixedUpdate.AddListener(this, ExecutionOrder);
        }

        private void StopListening()
        {
            if (UsesUpdate)
                ManagedUpdate.OnUpdate.RemoveListener(this);
            if (UsesLateUpdate)
                ManagedUpdate.OnLateUpdate.RemoveListener(this);
            if (UsesFixedUpdate)
                ManagedUpdate.OnFixedUpdate.RemoveListener(this);
        }

        /// <inheritdoc />
        public virtual void OnEvent(int eventId)
        {
            switch (eventId)
            {
                case EventIds.Update:
                    OnUpdate();
                    return;
                case EventIds.LateUpdate:
                    OnLateUpdate();
                    return;
                case EventIds.FixedUpdate:
                    OnFixedUpdate();
                    return;
            }
        }

        /// <summary>
        /// Called every frame if <see cref="UsesUpdate"/> is true and the component is enabled.
        /// </summary>
        protected virtual void OnUpdate() { }

        /// <summary>
        /// Called every frame just after <see cref="OnUpdate"/> if <see cref="UsesLateUpdate"/> is true and the component is enabled.
        /// </summary>
        protected virtual void OnLateUpdate() { }

        /// <summary>
        /// This function is called every fixed framerate frame if <see cref="OnFixedUpdate"/> is true and the component is enabled.
        /// </summary>
        protected virtual void OnFixedUpdate() { }
    }
}