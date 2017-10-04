using JetBrains.Annotations;
using UnityEngine;

namespace Archon.SwissArmyLib.Gravity
{
    /// <summary>
    /// A sphere-shaped gravitational point.
    /// 
    /// <remarks>The force is currently constant and not dependent on how close the entities are.</remarks>
    /// </summary>
    [AddComponentMenu("Archon/Gravity/SphericalGravitationalPoint")]
    public class SphericalGravitationalPoint : MonoBehaviour, IGravitationalAffecter
    {
        [SerializeField] private float _strength = 9.82f;
        [SerializeField] private float _radius = 1;
        [SerializeField] private AnimationCurve _dropoffCurve = AnimationCurve.Linear(0, 1, 1, 0);
        [SerializeField] private bool _isGlobal;

        private float _radiusSqr;
        private Transform _transform;

        /// <summary>
        /// The gravitational pull of this point.
        /// </summary>
        public float Strength
        {
            get { return _strength; }
            set { _strength = value; }
        }

        /// <summary>
        /// Gets or sets the radius of this gravitational point.
        /// 
        /// <remarks>If <see cref="IsGlobal"/> is true, then this property is ignored.</remarks>
        /// </summary>
        public float Radius
        {
            get { return _radius; }
            set
            {
                _radius = value;
                _radiusSqr = value * value;
            }
        }

        /// <summary>
        /// Gets or sets the dropoff curve of the gravitational force.
        /// </summary>
        public AnimationCurve DropoffCurve
        {
            get { return _dropoffCurve; }
            set { _dropoffCurve = value; }
        }

        /// <summary>
        /// Gets or sets whether this point should affect all entities regardless of whether they're in range.
        /// </summary>
        public bool IsGlobal
        {
            get { return _isGlobal; }
            set { _isGlobal = value; }
        }

        [UsedImplicitly]
        private void Awake()
        {
            _transform = transform;
            _radiusSqr = _radius * _radius;
        }

        [UsedImplicitly]
        private void OnEnable()
        {
            GravitationalSystem.Register(this);
        }

        [UsedImplicitly]
        private void OnDisable()
        {
            GravitationalSystem.Unregister(this);
        }

        [UsedImplicitly]
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(_transform.position, _radius);
        }

        /// <inheritdoc />
        public Vector3 GetForceAt(Vector3 location)
        {
            var deltaPos = _transform.position - location;

            var sqrDist = deltaPos.sqrMagnitude;

            if (_isGlobal || sqrDist < _radiusSqr)
            {
                var strength = _dropoffCurve.Evaluate(sqrDist / _radiusSqr) * _strength;
                var force = deltaPos.normalized;
                force.x *= strength;
                force.y *= strength;
                force.z *= strength;
                return force;
            }

            return new Vector3();
        }
    }
}