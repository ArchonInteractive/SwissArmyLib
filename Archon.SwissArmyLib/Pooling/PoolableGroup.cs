using System;
using System.Collections.Generic;
using System.Linq;
using Archon.SwissArmyLib.Utils.Editor;
using JetBrains.Annotations;
using UnityEngine;

namespace Archon.SwissArmyLib.Pooling
{
    /// <summary>
    /// Manages a list of IPoolable components found in the hierarchy of this GameObject and notifies them when it is spawned and despawned.
    /// </summary>
    [AddComponentMenu("Archon/Poolable Group")]
    public sealed class PoolableGroup : MonoBehaviour, IPoolable, ISerializationCallbackReceiver
    {
        [SerializeField, ReadOnly, UsedImplicitly]
        private List<MonoBehaviour> _poolableComponents = new List<MonoBehaviour>();

        void IPoolable.OnSpawned()
        {
            for (var i = 0; i < _poolableComponents.Count; i++)
                ((IPoolable)_poolableComponents[i]).OnSpawned();
        }

        void IPoolable.OnDespawned()
        {
            for (var i = 0; i < _poolableComponents.Count; i++)
                ((IPoolable) _poolableComponents[i]).OnDespawned();
        }

        /// <summary>
        /// Manually add a poolable object to be notified when this component is spawned or despawned.
        /// 
        /// Useful if you dynamically add IPoolable components at runtime.
        /// </summary>
        /// <param name="poolable">The poolable object that should be notified.</param>
        public void AddManually<T>(T poolable) where T : MonoBehaviour, IPoolable
        {
            if (ReferenceEquals(poolable, null))
                throw new ArgumentNullException("poolable");

            _poolableComponents.Add(poolable);
        }

        /// <summary>
        /// Manually removes a poolable object so that it no longer is notified when this component is spawned or despawned.
        /// </summary>
        /// <param name="poolable">The poolable object that should no longer be notified.</param>
        public void RemoveManually<T>(T poolable) where T : MonoBehaviour, IPoolable
        {
            if (ReferenceEquals(poolable, null))
                throw new ArgumentNullException("poolable");

            _poolableComponents.Remove(poolable);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (!Application.isPlaying)
            {
                _poolableComponents.Clear();
                var children = GetComponentsInChildren<IPoolable>(true).Cast<MonoBehaviour>();
                _poolableComponents.AddRange(children);
                _poolableComponents.Remove(this);
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            
        }
    }
}
