using UnityEngine;
using EventIds = Archon.SwissArmyLib.Events.ManagedUpdate.EventIds;

namespace Archon.SwissArmyLib.Events
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
    /// Makes a <see cref="ManagedUpdateBehaviour"/> subclass get notified on a frame-based interval update.
    /// </summary>
    public interface IFrameIntervalUpdateable
    {
        /// <summary>
        /// Called every Nth frame according to <see cref="ManagedUpdate.FrameInterval"/>.
        /// </summary>
        void OnFrameIntervalUpdate();
    }

    /// <summary>
    /// Makes a <see cref="ManagedUpdateBehaviour"/> subclass get notified on a time-based interval update.
    /// </summary>
    public interface ITimeIntervalUpdateable
    {
        /// <summary>
        /// Called every Nth unscaled second according to <see cref="ManagedUpdate.TimeInterval"/>.
        /// </summary>
        void OnTimeIntervalUpdate();
    }

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

        private bool _startWasCalled;
        private bool _isListening;
        private IUpdateable _updateable;
        private ILateUpdateable _lateUpdateable;
        private IFixedUpdateable _fixedUpdateable;
        private IFrameIntervalUpdateable _frameIntervalUpdateable;
        private ITimeIntervalUpdateable _timeIntervalUpdateable;

        /// <summary>
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
        /// </summary>
        protected virtual void Start()
        {
            // ReSharper disable SuspiciousTypeConversion.Global
            _updateable = this as IUpdateable;
            _lateUpdateable = this as ILateUpdateable;
            _fixedUpdateable = this as IFixedUpdateable;
            _frameIntervalUpdateable = this as IFrameIntervalUpdateable;
            _timeIntervalUpdateable = this as ITimeIntervalUpdateable;
            // ReSharper restore SuspiciousTypeConversion.Global

            if (_updateable == null
                && _lateUpdateable == null
                && _fixedUpdateable == null
                && _frameIntervalUpdateable == null
                && _timeIntervalUpdateable == null)
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
                if (_frameIntervalUpdateable == null) _frameIntervalUpdateable = this as IFrameIntervalUpdateable;
                if (_timeIntervalUpdateable == null) _timeIntervalUpdateable = this as ITimeIntervalUpdateable;
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

            if (_updateable != null)
                ManagedUpdate.OnUpdate.AddListener(this, ExecutionOrder);
            if (_lateUpdateable != null)
                ManagedUpdate.OnLateUpdate.AddListener(this, ExecutionOrder);
            if (_fixedUpdateable != null)
                ManagedUpdate.OnFixedUpdate.AddListener(this, ExecutionOrder);
            if (_frameIntervalUpdateable != null)
                ManagedUpdate.OnFrameIntervalUpdate.AddListener(this, ExecutionOrder);
            if (_timeIntervalUpdateable != null)
                ManagedUpdate.OnTimeIntervalUpdate.AddListener(this, ExecutionOrder);

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
            if (_frameIntervalUpdateable != null)
                ManagedUpdate.OnFrameIntervalUpdate.RemoveListener(this);
            if (_timeIntervalUpdateable != null)
                ManagedUpdate.OnTimeIntervalUpdate.RemoveListener(this);

            _isListening = false;
        }

        /// <inheritdoc />
        public virtual void OnEvent(int eventId)
        {
            switch (eventId)
            {
                case EventIds.Update:
                    _updateable.OnUpdate();
                    return;
                case EventIds.LateUpdate:
                    _lateUpdateable.OnLateUpdate();
                    return;
                case EventIds.FixedUpdate:
                    _fixedUpdateable.OnFixedUpdate();
                    return;
                case EventIds.FrameIntervalUpdate:
                    _frameIntervalUpdateable.OnFrameIntervalUpdate();
                    return;
                case EventIds.TimeIntervalUpdate:
                    _timeIntervalUpdateable.OnTimeIntervalUpdate();
                    return;
            }
        }
    }
}