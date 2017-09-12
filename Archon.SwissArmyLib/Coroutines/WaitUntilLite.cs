using System;
using System.Collections;
using Archon.SwissArmyLib.Pooling;
using UnityEngine;

namespace Archon.SwissArmyLib.Coroutines
{
    public sealed class WaitUntilLite : CustomYieldInstruction, IPoolableYieldInstruction
    {
        private static readonly Pool<WaitUntilLite> Pool = new Pool<WaitUntilLite>(() => new WaitUntilLite());

        public static IEnumerator Create(Func<bool> predicate)
        {
            var waiter = Pool.Spawn();
            waiter._predicate = predicate;
            return waiter;
        }

        private Func<bool> _predicate;

        private WaitUntilLite()
        {

        }

        /// <inheritdoc />
        public override bool keepWaiting
        {
            get { return !_predicate(); }
        }

        public void Despawn()
        {
            _predicate = null;
            Pool.Despawn(this);
        }
    }
}