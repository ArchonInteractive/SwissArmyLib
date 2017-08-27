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
    /// <see cref="EventSystem"/> is used for managing the events. 
    /// See <see cref="ManagedEvents"/> for the events you can listen for.
    /// 
    /// You can either put this component on a GameObject in your scene 
    /// or call <see cref="InitializeIfNeeded"/> to create the instance.
    /// 
    /// <seealso cref="ManagedUpdateBehaviour"/>
    /// </summary>
    [AddComponentMenu("Archon/ManagedUpdate")]
    public sealed class ManagedUpdate : MonoBehaviour
    {
        private static ManagedUpdate _instance;

        /// <summary>
        /// Creates the ManagedUpdate instance if it doesn't already exist.
        /// </summary>
        public static void InitializeIfNeeded()
        {
            if (_instance == null)
                _instance = ServiceLocator.RegisterSingleton<ManagedUpdate>();
        }

        [UsedImplicitly]
        private void Start()
        {
            if (_instance == null || _instance != this)
            {
                InitializeIfNeeded();
                Destroy(this);
            }
        }

        [UsedImplicitly]
        private void Update()
        {
            EventSystem.Invoke(ManagedEvents.Update);
        }

        [UsedImplicitly]
        private void LateUpdate()
        {
            EventSystem.Invoke(ManagedEvents.LateUpdate);
        }

        [UsedImplicitly]
        private void FixedUpdate()
        {
            EventSystem.Invoke(ManagedEvents.FixedUpdate);
        }
    }

    /// <summary>
    /// Relayed event ids.
    /// 
    /// <seealso cref="ManagedUpdate"/>
    /// </summary>
    public static class ManagedEvents
    {
#pragma warning disable 1591
        public const int
            Update = -1000,
            LateUpdate = -1001,
            FixedUpdate = -1002;
#pragma warning restore 1591
    }
}
