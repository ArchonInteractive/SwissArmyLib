using Archon.SwissArmyLib.Utils.Inspector;
using UnityEngine;
using UnityEditor;

namespace Archon.SwissArmyLib.Editor
{
    /// <summary>
    /// Makes fields marked with <see cref="ReadonlyAttribute"/> uninteractable via the inspector.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadonlyAttribute))]
    public class ReadonlyDrawer : PropertyDrawer
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
