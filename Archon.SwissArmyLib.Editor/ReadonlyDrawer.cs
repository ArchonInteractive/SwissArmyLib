using Archon.SwissArmyLib.Utils.Editor;
using UnityEngine;
using UnityEditor;

namespace Archon.SwissArmyLib.Editor
{
    /// <summary>
    /// Makes fields marked with <see cref="ReadOnlyAttribute"/> uninteractable via the inspector.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        /// <inheritdoc />
        public override void OnGUI(Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
}
