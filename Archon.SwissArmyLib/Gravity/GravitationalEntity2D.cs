using UnityEngine;

namespace Archon.SwissArmyLib.Gravity
{
    /// <summary>
    /// Makes this <see cref="GameObject"/>'s <see cref="Rigidbody2D"/> part of the gravitational system.
    /// </summary>
    [AddComponentMenu("Archon/Gravity/GravitationalEntity2D")]
    [RequireComponent(typeof(Rigidbody2D))]
    public class GravitationalEntity2D : MonoBehaviour
    {
        private Rigidbody2D _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            GravitationalSystem.Register(_rigidbody);
        }

        private void OnDisable()
        {
            GravitationalSystem.Unregister(_rigidbody);
        }
    }
}