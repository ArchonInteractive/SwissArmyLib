using System;
using System.Collections;
using Archon.SwissArmyLib.Pooling;
using UnityEngine;

namespace Archon.SwissArmyLib.Coroutines
{
    public sealed class WaitWhileLite : CustomYieldInstruction, IPoolableYieldInstruction
    {
        private static readonly Pool<WaitWhileLite> Pool = new Pool<WaitWhileLite>(() => new WaitWhileLite());

        public static IEnumerator Create(Func<bool> predicate)
        {
            var waiter = Pool.Spawn();
            waiter._predicate = predicate;
            return waiter;
        }

        private Func<bool> _predicate;

        private WaitWhileLite()
        {

        }

        /// <inheritdoc />
        public override bool keepWaiting
        {
            get { return _predicate(); }
        }

        public void Despawn()
        {
            _predicate = null;
            Pool.Despawn(this);
        }
    }
}