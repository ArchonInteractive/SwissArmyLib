using Archon.SwissArmyLib.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace Archon.SwissArmyLib.Editor.Utils
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
            var readOnly = (ReadOnlyAttribute) attribute;

            if (readOnly.OnlyWhilePlaying && !Application.isPlaying)
                EditorGUI.PropertyField(position, property, label, true);
            else
            {
                GUI.enabled = false;
                EditorGUI.PropertyField(position, property, label, true);
                GUI.enabled = true;
            }
        }
    }
}
