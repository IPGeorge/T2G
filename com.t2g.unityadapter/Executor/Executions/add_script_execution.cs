#if UNITY_EDITOR

using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace T2G.Executor
{
    [Execution("add_script")]
    public class add_script_execution : Execution
    {
        public async override Awaitable<(bool succeeded, string message)> Execute(Instruction instruction)
        {
            if (!ValidateInstructionKeyword(instruction.Keyword))
            {
                return (false, "Invalid instruction keyword! 'add_script' was expected.");
            }

            if (instruction.DataType != Instruction.EDataType.JsonData)
            {
                return (false, "Invalid instruction data!");
            }

            GameObject gameObj = Selection.activeGameObject;
            var jsonObj = GetInstructionJsonData(instruction);
            string name = jsonObj["name"];
            string scriptName = jsonObj["scriptName"];

            if (!string.IsNullOrEmpty(jsonObj["name"]))
            {
                var targetObj = GameObject.Find(name);
                if (targetObj == null)
                {
                    return (false, "No target game object was found!");
                }
                else
                {
                    gameObj = targetObj;
                }
            }

            Type scriptType = Executor.GetClassTypeByName(scriptName);
            if (scriptType != null)
            {
                gameObj.AddComponent(scriptType);
                await Task.Yield();
                return (true, $"{scriptName} was added to {name}");
            }
            else
            {
                return (false, $"Failed t o add {scriptName} to {name} because its type is missing!");
            }
        }
    }
}

#endif