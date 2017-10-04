using System;
using System.Collections.Generic;
using Archon.SwissArmyLib.Events;
using Archon.SwissArmyLib.Utils;
using UnityEngine;

namespace Archon.SwissArmyLib.Gravity
{
    /// <summary>
    /// A gravitational system to allow for a more flexible gravity instead of just a constant directional gravity.
    /// 
    /// Useful for planets, black holes, magnets etc.
    /// 
    /// Rigidbodies that should be affected should have the <see cref="GravitationalEntity"/> component (or <see cref="GravitationalEntity2D"/> if using 2d physics).
    /// 
    /// Add gravitational forces by implementing the <see cref="IGravitationalAffecter"/> interface and registering it in the system.
    /// See <see cref="SphericalGravitationalPoint"/> for a simple example implementation.
    /// 
    /// <remarks>You might want to set Unity's gravity to (0,0,0).</remarks>
    /// </summary>
    public class GravitationalSystem : IEventListener {

        private static readonly List<IGravitationalAffecter> Affecters = new List<IGravitationalAffecter>();
        private static readonly List<Rigidbody> Rigidbodies = new List<Rigidbody>();
        private static readonly List<Rigidbody2D> Rigidbodies2D = new List<Rigidbody2D>();

        static GravitationalSystem()
        {
            var instance = new GravitationalSystem();
            ServiceLocator.RegisterSingleton(instance);
            ServiceLocator.GlobalReset += () => ServiceLocator.RegisterSingleton(instance);
        }

        private GravitationalSystem()
        {
            ManagedUpdate.OnFixedUpdate.AddListener(this);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~GravitationalSystem()
        {
            ManagedUpdate.OnFixedUpdate.RemoveListener(this);
        }

        /// <summary>
        /// Registers a gravitational affecter to be part of the system.
        /// </summary>
        /// <param name="affecter">The affecter to register.</param>
        public static void Register(IGravitationalAffecter affecter)
        {
            if (ReferenceEquals(affecter, null))
                throw new ArgumentNullException("affecter");

            Affecters.Add(affecter);
        }

        /// <summary>
        /// Registers a <see cref="Rigidbody"/> that should be affected by gravitational forces in this system.
        /// </summary>
        /// <param name="rigidbody">The rigidbody to register.</param>
        public static void Register(Rigidbody rigidbody)
        {
            if (ReferenceEquals(rigidbody, null))
                throw new ArgumentNullException("rigidbody");

            Rigidbodies.Add(rigidbody);
        }

        /// <summary>
        /// Registers a <see cref="Rigidbody2D"/> that should be affected by gravitational forces in this system.
        /// </summary>
        /// <param name="rigidbody">The rigidbody to register.</param>
        public static void Register(Rigidbody2D rigidbody)
        {
            if (ReferenceEquals(rigidbody, null))
                throw new ArgumentNullException("rigidbody");

            Rigidbodies2D.Add(rigidbody);
        }

        /// <summary>
        /// Unregisters a gravitational affecter from the system, so it no longer affects entities.
        /// </summary>
        /// <param name="affecter">The affecter to unregister.</param>
        public static void Unregister(IGravitationalAffecter affecter)
        {
            if (ReferenceEquals(affecter, null))
                throw new ArgumentNullException("affecter");

            Affecters.Remove(affecter);
        }

        /// <summary>
        /// Unregisters a <see cref="Rigidbody"/> from the system, so it no longer is affected by gravitational forces in this system.
        /// </summary>
        /// <param name="rigidbody">The rigidbody to unregister.</param>
        public static void Unregister(Rigidbody rigidbody)
        {
            if (ReferenceEquals(rigidbody, null))
                throw new ArgumentNullException("rigidbody");

            Rigidbodies.Remove(rigidbody);
        }

        /// <summary>
        /// Unregisters a <see cref="Rigidbody2D"/> from the system, so it no longer is affected by gravitational forces in this system.
        /// </summary>
        /// <param name="rigidbody">The rigidbody to unregister.</param>
        public static void Unregister(Rigidbody2D rigidbody)
        {
            if (ReferenceEquals(rigidbody, null))
                throw new ArgumentNullException("rigidbody");

            Rigidbodies2D.Remove(rigidbody);
        }

        /// <summary>
        /// Gets the sum of all gravitational forces at a specific location.
        /// </summary>
        /// <param name="location">The location to test.</param>
        /// <returns>A vector representing the sum of gravitational force at <paramref name="location"/>.</returns>
        public static Vector3 GetGravityAtPoint(Vector3 location)
        {
            var gravity = new Vector3();

            var affecterCount = Affecters.Count;
            for (var i = 0; i < affecterCount; i++)
            {
                var force = Affecters[i].GetForceAt(location);
                gravity.x += force.x;
                gravity.y += force.y;
                gravity.z += force.z;
            }

            return gravity;
        }

        void IEventListener.OnEvent(int eventId)
        {
            if (eventId != ManagedUpdate.EventIds.FixedUpdate)
                return;

            var rigidbodyCount = Rigidbodies.Count;
            for (var i = 0; i < rigidbodyCount; i++)
            {
                var body = Rigidbodies[i];

                if (body.useGravity && !body.IsSleeping())
                {
                    var gravity = GetGravityAtPoint(body.position);
                    if (gravity.sqrMagnitude > 0.0001f)
                        body.AddForce(gravity);
                }
            }

            var rigidbody2DCount = Rigidbodies2D.Count;
            for (var i = 0; i < rigidbody2DCount; i++)
            {
                var body = Rigidbodies2D[i];
                var gravityScale = body.gravityScale;

                if (body.simulated && gravityScale > 0 && body.IsAwake())
                {
                    Vector2 gravity = GetGravityAtPoint(body.position);
                    if (gravity.sqrMagnitude < 0.0001f)
                        continue;

                    gravity.x *= gravityScale;
                    gravity.y *= gravityScale;
                    body.AddForce(gravity);
                }
            }
        }
    }
}