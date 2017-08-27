using System.Collections.Generic;
using UnityEngine;

namespace Archon.SwissArmyLib.Pooling
{
    /// <summary>
    /// Manages a list of IPoolable components found in the hierarchy of this GameObject and notifies them when it is spawned and despawned.
    /// </summary>
    [AddComponentMenu("Archon/Poolable Group")]
    public sealed class PoolableGroup : MonoBehaviour, IPoolable
    {
        private readonly List<IPoolable> _poolableComponents = new List<IPoolable>();

        private void Awake()
        {
            GetComponentsInChildren(_poolableComponents);
            _poolableComponents.Remove(this);
        }

        public void OnSpawned()
        {
            for (var i = 0; i < _poolableComponents.Count; i++)
                _poolableComponents[i].OnSpawned();
        }

        public void OnDespawned()
        {
            for (var i = 0; i < _poolableComponents.Count; i++)
                _poolableComponents[i].OnDespawned();
        }

        /// <summary>
        /// Manually add a poolable object to be notified when this component is spawned or despawned.
        /// 
        /// Useful if you dynamically add IPoolable components at runtime.
        /// </summary>
        /// <param name="poolable">The poolable object that should be notified.</param>
        public void AddManually(IPoolable poolable)
        {
            _poolableComponents.Add(poolable);
        }

        /// <summary>
        /// Manually removes a poolable object so that it no longer is notified when this component is spawned or despawned.
        /// </summary>
        /// <param name="poolable">The poolable object that should no longer be notified.</param>
        public void RemoveManually(IPoolable poolable)
        {
            _poolableComponents.Remove(poolable);
        }
    }
}
