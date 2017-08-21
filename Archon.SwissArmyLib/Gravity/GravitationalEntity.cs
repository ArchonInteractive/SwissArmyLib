using UnityEngine;

namespace Archon.SwissArmyLib.Gravity
{
    public class GravitationalEntity : MonoBehaviour
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
