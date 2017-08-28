using UnityEngine;

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

        /// <summary>
        /// Called when the component is enabled.
        /// </summary>
        protected virtual void OnEnable()
        {
            ManagedUpdate.InitializeIfNeeded();
            if (UsesUpdate)
                EventSystem.Global.AddListener(ManagedEvents.Update, this, ExecutionOrder);
            if (UsesLateUpdate)
                EventSystem.Global.AddListener(ManagedEvents.LateUpdate, this, ExecutionOrder);
            if (UsesFixedUpdate)
                EventSystem.Global.AddListener(ManagedEvents.FixedUpdate, this, ExecutionOrder);
        }

        /// <summary>
        /// Called when the component is disabled.
        /// </summary>
        protected virtual void OnDisable()
        {
            if (UsesUpdate)
                EventSystem.Global.RemoveListener(ManagedEvents.Update, this);
            if (UsesLateUpdate)
                EventSystem.Global.RemoveListener(ManagedEvents.LateUpdate, this);
            if (UsesFixedUpdate)
                EventSystem.Global.RemoveListener(ManagedEvents.FixedUpdate, this);
        }

        /// <inheritdoc />
        public virtual void OnEvent(int eventId)
        {
            switch (eventId)
            {
                case ManagedEvents.Update:
                    OnUpdate();
                    return;
                case ManagedEvents.LateUpdate:
                    OnLateUpdate();
                    return;
                case ManagedEvents.FixedUpdate:
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