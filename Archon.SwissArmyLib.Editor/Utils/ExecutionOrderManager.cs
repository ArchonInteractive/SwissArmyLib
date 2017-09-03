using System;
using Archon.SwissArmyLib.Utils.Editor;
using UnityEditor;

namespace Archon.SwissArmyLib.Editor.Utils
{
    /// <summary>
    /// Looks for classes using the <see cref="ExecutionOrderAttribute"/> and sets their execution order.
    /// </summary>
    [InitializeOnLoad]
    public class ExecutionOrderManager : UnityEditor.Editor
    {
        static ExecutionOrderManager()
        {
            foreach (var script in MonoImporter.GetAllRuntimeMonoScripts())
            {
                if (script.GetClass() == null) continue;

                var attributes = Attribute.GetCustomAttributes(script.GetClass(), typeof(ExecutionOrderAttribute));

                for (var i = 0; i < attributes.Length; i++)
                {
                    var attribute = (ExecutionOrderAttribute) attributes[i];

                    var currentOrder = MonoImporter.GetExecutionOrder(script);

                    if (currentOrder == attribute.Order)
                        continue;

                    if (currentOrder == 0 || attribute.Forced)
                        MonoImporter.SetExecutionOrder(script, attribute.Order);
                }
            }
        }
    }
}
