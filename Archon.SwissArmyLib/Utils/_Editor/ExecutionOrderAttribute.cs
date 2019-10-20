using System;
using UnityEngine;

namespace Archon.SwissArmyLib.Utils.Editor
{
    /// <summary>
    /// Changes the ScriptExecutionOrder of a <see cref="MonoBehaviour"/> if it's not already explicitly set (or if <see cref="Forced"/> is true).
    /// </summary>
    public class ExecutionOrderAttribute : Attribute
    {
        /// <summary>
        /// The order you want for the script to have.
        /// </summary>
        public int Order;

        /// <summary>
        /// Whether you want the order to be forcibly set and not just used as a default value.
        /// </summary>
        public bool Forced;
    }
}
