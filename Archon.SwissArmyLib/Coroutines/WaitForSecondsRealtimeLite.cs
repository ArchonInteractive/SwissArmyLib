using System.Collections;
using Archon.SwissArmyLib.Pooling;
using UnityEngine;

namespace Archon.SwissArmyLib.Coroutines
{
    internal sealed class WaitForSecondsRealtimeLite : CustomYieldInstruction, IPoolableYieldInstruction
    {
        private static readonly Pool<WaitForSecondsRealtimeLite> Pool = new Pool<WaitForSecondsRealtimeLite>(() => new WaitForSecondsRealtimeLite());

        public static IEnumerator Create(float seconds)
        {
            var waiter = Pool.Spawn();
            waiter._expirationTime = Time.realtimeSinceStartup + seconds;
            return waiter;
        }

        private float _expirationTime;

        private WaitForSecondsRealtimeLite()
        {
        }

        /// <inheritdoc />
        public override bool keepWaiting
        {
            get { return Time.realtimeSinceStartup < _expirationTime; }
        }

        public void Despawn()
        {
            _expirationTime = 0;
            Pool.Despawn(this);
        }
    }
}