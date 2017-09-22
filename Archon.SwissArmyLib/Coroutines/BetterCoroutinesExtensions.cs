using System.Collections;
using UnityEngine;

namespace Archon.SwissArmyLib.Coroutines
{
    /// <summary>
    /// A bunch of helpful extensions for starting and stopping coroutines.
    /// </summary>
    public static class BetterCoroutinesExtensions
    {
        /// <summary>
        /// Starts a new coroutine.
        /// </summary>
        /// <param name="unityObject"></param>
        /// <param name="enumerator"></param>
        /// <param name="updateLoop">Which update loop should the coroutine be part of?</param>
        /// <returns>The id of the coroutine.</returns>
        public static int StartBetterCoroutine(this Object unityObject, IEnumerator enumerator,
            UpdateLoop updateLoop = UpdateLoop.Update)
        {
            return BetterCoroutines.Start(enumerator, updateLoop);
        }

        /// <summary>
        /// Starts a new coroutine with its lifetime linked to this component.
        /// The coroutine will be stopped when the linked component is disabled or destroyed.
        /// </summary>
        /// <param name="monoBehaviour">The component to link the coroutine to.</param>
        /// <param name="enumerator"></param>
        /// <param name="updateLoop">Which update loop should the coroutine be part of?</param>
        /// <returns>The id of the coroutine.</returns>
        public static int StartBetterCoroutineLinked(this MonoBehaviour monoBehaviour, IEnumerator enumerator,
            UpdateLoop updateLoop = UpdateLoop.Update)
        {
            return BetterCoroutines.Start(enumerator, monoBehaviour, updateLoop);
        }

        /// <summary>
        /// Starts a new coroutine with its lifetime linked to this gameobject.
        /// The coroutine will be stopped when the linked gameobject is disabled or destroyed.
        /// </summary>
        /// <param name="gameObject">The gameobject to link the coroutine to.</param>
        /// <param name="enumerator"></param>
        /// <param name="updateLoop">Which update loop should the coroutine be part of?</param>
        /// <returns>The id of the coroutine.</returns>
        public static int StartBetterCoroutineLinked(this GameObject gameObject, IEnumerator enumerator,
            UpdateLoop updateLoop = UpdateLoop.Update)
        {
            return BetterCoroutines.Start(enumerator, gameObject, updateLoop);
        }

        /// <summary>
        /// Stops a running coroutine prematurely. 
        /// 
        /// This will stop any child coroutines as well.
        /// </summary>
        /// <param name="unityObject"></param>
        /// <param name="coroutineId">The id of the coroutine to stop.</param>
        /// <returns>True if the coroutine was found and stopped, otherwise false.</returns>
        public static bool StopBetterCoroutine(this Object unityObject, int coroutineId)
        {
            return BetterCoroutines.Stop(coroutineId);
        }
    }
}
