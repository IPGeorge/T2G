#if UNITY_EDITOR

using System.IO;
using UnityEngine;
using UnityEditor;

namespace T2G.Executor
{
    [Execution("make_prefab")]
    public class make_prefab_execution : Execution
    {
        public override (eExecutionResult, string) Execute(Instruction instruction)
        {
            if (!ValidateInstructionKeyword(instruction.Action))
            {
                return (eExecutionResult.Failed, "Invalid instruction keyword! 'make_prefab' was expected.");
            }

            if (instruction.DataType != Instruction.EDataType.SingleParameter)
            {
                return (eExecutionResult.Failed, "Invalid instruction data!");
            }

            string objName = instruction.Data.Trim();
            var obj = GameObject.Find(objName);
            if(obj == null)
            {
                return (eExecutionResult.Failed, $"Couldn't find {objName} in current space!");
            }
            
            string prefabsPath = Path.Combine(Application.dataPath, "Prefabs");
            string prefabPathName = Path.Combine(prefabsPath, objName + ".prefab");

            if(!Directory.Exists(prefabsPath))
            {
                Directory.CreateDirectory(prefabsPath);
            }

            PrefabUtility.SaveAsPrefabAsset(obj, prefabPathName, out var success);
            if (success)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return (eExecutionResult.Succeeded, $"{objName}.prefab was created!");
            }

            return (eExecutionResult.Failed, $"Failed to create prefab {objName}!");
        }
    }
}
#endif
