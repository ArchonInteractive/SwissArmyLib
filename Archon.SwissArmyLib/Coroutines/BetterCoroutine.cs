using System.Collections;
using Archon.SwissArmyLib.Pooling;
using Archon.SwissArmyLib.Utils;
using UnityEngine;

namespace Archon.SwissArmyLib.Coroutines
{
    internal sealed class BetterCoroutine : IPoolable
    {
        internal int Id;
        internal bool IsDone;
        internal bool IsPaused;
        internal bool IsParentPaused;
        internal UpdateLoop UpdateLoop;
        internal IEnumerator Enumerator;
        internal BetterCoroutine Parent;
        internal BetterCoroutine Child;

        internal float WaitTillTime = float.MinValue;
        internal bool WaitTimeIsUnscaled;
        internal bool WaitingForEndOfFrame;

        internal bool IsLinkedToObject;
        internal GameObject LinkedObject;
        internal bool IsLinkedToComponent;
        internal MonoBehaviour LinkedComponent;

        void IPoolable.OnSpawned()
        {
            
        }

        void IPoolable.OnDespawned()
        {
            var poolableYieldInstruction = Enumerator as IPoolableYieldInstruction;
            if (poolableYieldInstruction != null)
                poolableYieldInstruction.Despawn();

            Id = -1;
            IsDone = false;
            IsPaused = false;
            IsParentPaused = false;
            UpdateLoop = UpdateLoop.Update;
            Enumerator = null;
            Parent = null;
            Child = null;

            WaitTillTime = float.MinValue;
            WaitTimeIsUnscaled = false;
            WaitingForEndOfFrame = false;

            LinkedObject = null;
            LinkedComponent = null;
            IsLinkedToObject = false;
            IsLinkedToComponent = false;
        }
    }
}