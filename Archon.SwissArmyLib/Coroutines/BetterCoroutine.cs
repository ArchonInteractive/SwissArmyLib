using System.Collections;
using Archon.SwissArmyLib.Pooling;

namespace Archon.SwissArmyLib.Coroutines
{
    public interface IBetterCoroutine
    {
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
            IsDone = true;
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