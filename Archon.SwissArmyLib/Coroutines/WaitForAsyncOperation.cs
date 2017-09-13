using System.Collections;
using Archon.SwissArmyLib.Pooling;
using UnityEngine;

namespace Archon.SwissArmyLib.Coroutines
{
    internal sealed class WaitForAsyncOperation : CustomYieldInstruction, IPoolableYieldInstruction
    {
        private static readonly Pool<WaitForAsyncOperation> Pool = new Pool<WaitForAsyncOperation>(() => new WaitForAsyncOperation());

        public static IEnumerator Create(AsyncOperation operation)
        {
            var waiter = Pool.Spawn();
            waiter._operation = operation;
            return waiter;
        }

        private AsyncOperation _operation;

        private WaitForAsyncOperation()
        {
            
        }

        /// <inheritdoc />
        public override bool keepWaiting
        {
            get { return !_operation.isDone; }
        }

        public void Despawn()
        {
            _operation = null;
            Pool.Despawn(this);
        }
    }
}