#if UNITY_EDITOR

using SimpleJSON;
using UnityEngine;

namespace T2G.Executor
{
    [Execution("detach_from")]
    public class detach_from_execution : Execution
    {
        public override (eExecutionResult, string) Execute(Instruction instruction)
        {
            if (!ValidateInstructionKeyword(instruction.Keyword))
            {
                return (eExecutionResult.Failed, "Invalid instruction keyword! 'detach_from' was expected.");
            }

            if (instruction.DataType != Instruction.EDataType.JsonData ||
                instruction.State != Instruction.EInstructionState.Resolved)
            {
                return (eExecutionResult.Failed, "Invalid instruction data!");
            }

            JSONObject jsonObjData = JSON.Parse(instruction.Data).AsObject;
            string sourceName = jsonObjData["source"];
            string targetName = jsonObjData["target"];
            var sourceObj = GameObject.Find(sourceName.Trim());
            var targetObj = GameObject.Find(targetName.Trim());

            if (sourceObj == null || targetObj == null)
            {
                string missingObjName = sourceObj == null ? sourceName : targetName;
                return (eExecutionResult.Failed, $"Couldn't find object {missingObjName}");
            }

            sourceObj.transform.parent = null;
            Executor.ForceUpdateSceneView();

            return (eExecutionResult.Succeeded, $"{sourceName} was detached from {targetName}.");
        }
    }
}
#endif