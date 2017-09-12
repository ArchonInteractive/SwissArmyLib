using System.Collections;
using Archon.SwissArmyLib.Pooling;
using UnityEngine;

namespace Archon.SwissArmyLib.Coroutines
{
    public sealed class WaitForWWW : CustomYieldInstruction, IPoolableYieldInstruction
    {
        private static readonly Pool<WaitForWWW> Pool = new Pool<WaitForWWW>(() => new WaitForWWW());

        public static IEnumerator Create(WWW wwwObject)
        {
            var waiter = Pool.Spawn();
            waiter._wwwObject = wwwObject;
            return waiter;
        }

        private WWW _wwwObject;

        private WaitForWWW()
        {
            
        }

        /// <inheritdoc />
        public override bool keepWaiting
        {
            get { return !_wwwObject.isDone; }
        }

        public void Despawn()
        {
            _wwwObject = null;
            Pool.Despawn(this);
        }
    }
}