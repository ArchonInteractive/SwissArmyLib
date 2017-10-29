using UnityEngine;

namespace Archon.SwissArmyLib.Events.Loops
{
    /// <summary>
    /// Makes a <see cref="ManagedUpdateBehaviour"/> subclass get notified on an update.
    /// </summary>
    public interface IUpdateable
    {
        /// <summary>
        /// Called every frame.
        /// </summary>
        void OnUpdate();
    }

    /// <summary>
    /// Makes a <see cref="ManagedUpdateBehaviour"/> subclass get notified on a late update.
    /// </summary>
    public interface ILateUpdateable
    {
        /// <summary>
        /// Called every frame, after the regular update loop.
        /// </summary>
        void OnLateUpdate();
    }

    /// <summary>
    /// Makes a <see cref="ManagedUpdateBehaviour"/> subclass get notified on a fixed update.
    /// </summary>
    public interface IFixedUpdateable
    {
        /// <summary>
        /// Called every fixed update.
        /// </summary>
        void OnFixedUpdate();
    }

    /// <summary>
    /// Makes a <see cref="ManagedUpdateBehaviour"/> subclass get notified when a custom update loop ticks.
    /// </summary>
    public interface ICustomUpdateable
    {
        /// <summary>
        /// Gets the event ids for the custom update loops that should be listened to.
        /// </summary>
        /// <returns>The event ids to listen for.</returns>
        int[] GetCustomUpdateIds();

        /// <summary>
        /// Called whenever one of the custom update loops tick.
        /// </summary>
        /// <param name="eventId"></param>
        void OnCustomUpdate(int eventId);
    }

    /// <summary>
    /// A subclass of MonoBehaviour that uses <see cref="ManagedUpdate"/> for update events.
    /// 
    /// To receive updates implement one or more of the appropriate interfaces: 
    /// <see cref="IUpdateable"/>, <see cref="ILateUpdateable"/>, <see cref="IFixedUpdateable"/> and <see cref="ICustomUpdateable"/>.
    /// </summary>
    public abstract class ManagedUpdateBehaviour : MonoBehaviour, IEventListener
    {
        /// <summary>
        /// Affects whether this components' events will be called before or after others'.
        /// 
        /// Basically a reimplementation of Unity's ScriptExecutionOrder.
        /// </summary>
        protected virtual int ExecutionOrder { get { return 0; } }

        private bool _startWasCalled;
        private bool _isListening;
        private IUpdateable _updateable;
        private ILateUpdateable _lateUpdateable;
        private IFixedUpdateable _fixedUpdateable;
        private ICustomUpdateable _customUpdateable;

        private int[] _customUpdateIds;

        /// <summary>
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
        /// </summary>
        protected virtual void Start()
        {
            // ReSharper disable SuspiciousTypeConversion.Global
            _updateable = this as IUpdateable;
            _lateUpdateable = this as ILateUpdateable;
            _fixedUpdateable = this as IFixedUpdateable;
            _customUpdateable = this as ICustomUpdateable;
            // ReSharper restore SuspiciousTypeConversion.Global

            if (_updateable == null
                && _lateUpdateable == null
                && _fixedUpdateable == null
                && _customUpdateable == null)
            {
                Debug.LogWarning("This component doesn't implement any update interfaces.", this);
            }

            _startWasCalled = true;
            StartListening();
        }

        /// <summary>
        /// Called when the component is enabled.
        /// </summary>
        protected virtual void OnEnable()
        {
            // interface references aren't serialized when unity hot-reloads
            if (Application.isEditor)
            {
                // ReSharper disable SuspiciousTypeConversion.Global
                if (_updateable == null) _updateable = this as IUpdateable;
                if (_lateUpdateable == null) _lateUpdateable = this as ILateUpdateable;
                if (_fixedUpdateable == null) _fixedUpdateable = this as IFixedUpdateable;
                if (_customUpdateable == null) _customUpdateable = this as ICustomUpdateable;
                // ReSharper restore SuspiciousTypeConversion.Global
            }

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
            if (_isListening)
            {
                Debug.LogError("Attempt at starting to listen for updates, while already listening. Did you forget to call base.OnDisable()?");
                return;
            }

            var executionOrder = ExecutionOrder;

            if (_updateable != null)
                ManagedUpdate.OnUpdate.AddListener(this, executionOrder);
            if (_lateUpdateable != null)
                ManagedUpdate.OnLateUpdate.AddListener(this, executionOrder);
            if (_fixedUpdateable != null)
                ManagedUpdate.OnFixedUpdate.AddListener(this, executionOrder);

            if (_customUpdateable != null)
            {
                if (_customUpdateIds == null)
                    _customUpdateIds = _customUpdateable.GetCustomUpdateIds() ?? new int[0];

                for (var i = 0; i < _customUpdateIds.Length; i++)
                    ManagedUpdate.AddListener(_customUpdateIds[i], this, executionOrder);
            }

            _isListening = true;
        }

        private void StopListening()
        {
            if (!_isListening)
            {
                Debug.LogError("Attempted to stop listening for updates while not listening. Did you forget to call base.Start() or base.OnEnable()?");
                return;
            }

            if (_updateable != null)
                ManagedUpdate.OnUpdate.RemoveListener(this);
            if (_lateUpdateable != null)
                ManagedUpdate.OnLateUpdate.RemoveListener(this);
            if (_fixedUpdateable != null)
                ManagedUpdate.OnFixedUpdate.RemoveListener(this);

            if (_customUpdateable != null && _customUpdateIds != null)
            {
                for (var i = 0; i < _customUpdateIds.Length; i++)
                    ManagedUpdate.RemoveListener(_customUpdateIds[i], this);
            }

            _isListening = false;
        }

        /// <inheritdoc />
        public virtual void OnEvent(int eventId)
        {
            switch (eventId)
            {
                case ManagedUpdate.EventIds.Update:
                    _updateable.OnUpdate();
                    return;
                case ManagedUpdate.EventIds.LateUpdate:
                    _lateUpdateable.OnLateUpdate();
                    return;
                case ManagedUpdate.EventIds.FixedUpdate:
                    _fixedUpdateable.OnFixedUpdate();
                    return;
            }

            if (_customUpdateIds != null)
            {
                for (var i = 0; i < _customUpdateIds.Length; i++)
                {
                    if (_customUpdateIds[i] != eventId) continue;

                    _customUpdateable.OnCustomUpdate(eventId);
                    return;
                }
            }
        }
    }
}