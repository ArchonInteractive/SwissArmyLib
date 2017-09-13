using System.Collections;
using Archon.SwissArmyLib.Pooling;

namespace Archon.SwissArmyLib.Coroutines
{
    /// <summary>
    /// Represents a coroutine. 
    /// 
    /// After the coroutine is stopped, you should not use it anymore as it might be recycled and point to a completely different coroutine.
    /// </summary>
    public interface IBetterCoroutine
    {
        /// <summary>
        /// Stops the coroutine prematurely.
        /// </summary>
        void Stop();
    }

    internal sealed class BetterCoroutine : IBetterCoroutine, IPoolable
    {
        internal bool IsDone;
        internal UpdateLoop UpdateLoop;
        internal IEnumerator Enumerator;
        internal BetterCoroutine Parent;
        internal BetterCoroutine Child;
        internal float WaitTillTime = float.MinValue;
        internal bool WaitTimeIsUnscaled;

        public void Stop()
        {
            BetterCoroutines.Stop(this);
        }

        void IPoolable.OnSpawned()
        {
            
        }

        void IPoolable.OnDespawned()
        {
            var poolableYieldInstruction = Enumerator as IPoolableYieldInstruction;
            if (poolableYieldInstruction != null)
                poolableYieldInstruction.Despawn();

            IsDone = false;
            UpdateLoop = UpdateLoop.Update;
            Enumerator = null;
            Parent = null;
            Child = null;
            WaitTillTime = float.MinValue;
            WaitTimeIsUnscaled = false;
        }
    }
}