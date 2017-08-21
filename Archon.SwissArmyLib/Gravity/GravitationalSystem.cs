using System.Collections.Generic;
using UnityEngine;

namespace Archon.SwissArmyLib.Gravity
{
    public class GravitationalSystem : MonoBehaviour {

        public static GravitationalSystem Instance { get; private set; }

        private static readonly List<IGravitationalPoint> Points = new List<IGravitationalPoint>();
        private static readonly List<Rigidbody> Rigidbodies = new List<Rigidbody>();
        private static readonly List<Rigidbody2D> Rigidbodies2D = new List<Rigidbody2D>();

        static GravitationalSystem()
        {
            var gameObject = new GameObject("GravitationalSystem");
            DontDestroyOnLoad(gameObject);
            Instance = gameObject.AddComponent<GravitationalSystem>();
        }

        public static void Register(IGravitationalPoint point)
        {
            Points.Add(point);
        }

        public static void Register(Rigidbody rigidbody)
        {
            Rigidbodies.Add(rigidbody);
        }

        public static void Register(Rigidbody2D rigidbody)
        {
            Rigidbodies2D.Add(rigidbody);
        }

        public static void Unregister(IGravitationalPoint point)
        {
            Points.Remove(point);
        }

        public static void Unregister(Rigidbody rigidbody)
        {
            Rigidbodies.Remove(rigidbody);
        }

        public static void Unregister(Rigidbody2D rigidbody)
        {
            Rigidbodies2D.Remove(rigidbody);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("You shouldn't add GravitationalSystem to a GameObject manually.");
                Destroy(this);
            }
        }

        private void FixedUpdate()
        {
            for (var i = 0; i < Rigidbodies.Count; i++)
            {
                var body = Rigidbodies[i];

                if (body.useGravity && !body.IsSleeping())
                {
                    var gravity = GetGravityAtPoint(body.position);
                    body.velocity += gravity * Time.deltaTime;
                }
            }

            for (var i = 0; i < Rigidbodies2D.Count; i++)
            {
                var body = Rigidbodies2D[i];

                if (body.simulated && body.gravityScale > 0 && body.IsAwake())
                {
                    var gravity = GetGravityAtPoint(body.position);
                    body.velocity += (Vector2) gravity * body.gravityScale * Time.deltaTime;
                }
            }
        }

        public Vector3 GetGravityAtPoint(Vector3 location)
        {
            var gravity = Vector3.zero;

            for (var i = 0; i < Points.Count; i++)
                gravity += Points[i].GetForceAt(location);

            return gravity;
        }
    }

    public interface IGravitationalPoint
    {
        Vector3 GetForceAt(Vector3 location);
    }
}