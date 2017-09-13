using Archon.SwissArmyLib.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Archon.SwissArmyLib.Events
{
    /// <summary>
    /// A relay for Unity update events.
    /// Here's why you might want to use this:
    /// https://blogs.unity3d.com/2015/12/23/1k-update-calls/
    /// In short; avoid overhead of Native C++ --> Managed C# calls.
    /// 
    /// Also useful for non-MonoBehaviours that needs to be part of the update loop as well.
    /// 
    /// Events your can subscribe to:
    ///     <see cref="OnUpdate"/>
    ///     <see cref="OnLateUpdate"/>
    ///     <see cref="OnFixedUpdate"/>
    /// 
    /// <seealso cref="ManagedUpdateBehaviour"/>
    /// </summary>
    [AddComponentMenu("")]
    public sealed class ManagedUpdate : MonoBehaviour
    {
        /// <summary>
        /// Event handler that is called every update.
        /// </summary>
        public static readonly Event OnUpdate = new Event(EventIds.Update);

        /// <summary>
        /// Event handler that is called every update but after the regular Update.
        /// <seealso cref="OnUpdate"/>
        /// </summary>
        public static readonly Event OnLateUpdate = new Event(EventIds.LateUpdate);

        /// <summary>
        /// Event handler that is called every fixed update.
        /// </summary>
        public static readonly Event OnFixedUpdate = new Event(EventIds.FixedUpdate);

        /// <summary>
        /// Relayed event ids.
        /// </summary>
        public static class EventIds
        {
#pragma warning disable 1591
            public const int
                Update = -1000,
                LateUpdate = -1001,
                FixedUpdate = -1002;
#pragma warning restore 1591
        }

        static ManagedUpdate()
        {
            _instance = ServiceLocator.RegisterSingleton<ManagedUpdate>();
            ServiceLocator.GlobalReset += () =>
            {
                _instance = ServiceLocator.RegisterSingleton<ManagedUpdate>();
            };
        }

        private static ManagedUpdate _instance;

        [UsedImplicitly]
        private void Start()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("You should not create the ManagedUpdate component yourself, this component will be removed.");
                Destroy(this);
            }
        }

        [UsedImplicitly]
        private void Update()
        {
            OnUpdate.Invoke();
        }

        [UsedImplicitly]
        private void LateUpdate()
        {
            OnLateUpdate.Invoke();
        }

        [UsedImplicitly]
        private void FixedUpdate()
        {
            OnFixedUpdate.Invoke();
        }
    }
}
