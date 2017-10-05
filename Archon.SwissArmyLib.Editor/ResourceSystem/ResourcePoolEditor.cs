using Archon.SwissArmyLib.ResourceSystem;
using UnityEditor;
using UnityEngine;

namespace Archon.SwissArmyLib.Editor.ResourceSystem
{
    /// <summary>
    /// Custom editor for <see cref="ResourcePoolBase"/> components.
    /// 
    /// Shows a health bar and debugging buttons.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ResourcePoolBase), true)]
    public class ResourcePoolEditor : UnityEditor.Editor
    {
        private float _addVal = 50;
        private float _removeVal = 50;

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            if (targets.Length == 1)
            {
                var resourcePool = (ResourcePoolBase)target;
                EditorGUILayout.Separator();
                var containerRect = EditorGUILayout.BeginHorizontal();
                var barRect = GUILayoutUtility.GetRect(containerRect.width, 20);
                EditorGUI.ProgressBar(barRect, resourcePool.Percentage, string.Format("{0:F1} / {1:F1}", resourcePool.Current, resourcePool.Max));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Separator();
            }

            DrawDefaultInspector();

            if (Application.isPlaying)
            {
                EditorGUILayout.Separator();

                EditorGUILayout.BeginHorizontal();
                _addVal = EditorGUILayout.FloatField(_addVal);
                if (GUILayout.Button("Add")) Add(_addVal);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                _removeVal = EditorGUILayout.FloatField(_removeVal);
                if (GUILayout.Button("Remove")) Remove(_removeVal);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Empty")) Empty();
                if (GUILayout.Button("Fill")) Fill();
                if (GUILayout.Button("Renew")) Renew();
                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Adds a resource amount to all targeted components.
        /// </summary>
        /// <param name="amount">Amount to add</param>
        protected void Add(float amount)
        {
            for (var i = 0; i < targets.Length; i++)
                ((ResourcePoolBase)targets[i]).Add(amount);
        }

        /// <summary>
        /// Removes a resource amount from all targeted components.
        /// </summary>
        /// <param name="amount">Amount to remove</param>
        protected void Remove(float amount)
        {
            for (var i = 0; i < targets.Length; i++)
                ((ResourcePoolBase)targets[i]).Remove(amount);
        }

        /// <summary>
        /// Empties all targeted components.
        /// </summary>
        protected void Empty()
        {
            for (var i = 0; i < targets.Length; i++)
                ((ResourcePoolBase)targets[i]).Empty();
        }

        /// <summary>
        /// Fills all targeted components.
        /// </summary>
        protected void Fill()
        {
            for (var i = 0; i < targets.Length; i++)
                ((ResourcePoolBase) targets[i]).Fill();
        }

        /// <summary>
        /// Renews all targeted components.
        /// </summary>
        protected void Renew()
        {
            for (var i = 0; i < targets.Length; i++)
                ((ResourcePoolBase)targets[i]).Renew();
        }
    }
}
