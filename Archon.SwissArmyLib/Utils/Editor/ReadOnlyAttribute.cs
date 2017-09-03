using UnityEngine;

namespace Archon.SwissArmyLib.Utils.Editor
{
    /// <summary>
    /// Marks the field to be unchangable via the inspector.
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute
    {
        /// <summary>
        /// Whether it should only be readonly during play mode.
        /// </summary>
        public bool OnlyWhilePlaying;
    }
}
