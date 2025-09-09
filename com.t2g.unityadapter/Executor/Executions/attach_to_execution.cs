#if UNITY_EDITOR

using SimpleJSON;
using UnityEngine;

namespace T2G.Executor
{
    [Execution("attach_to")]
    public class attach_to_execution : Execution
    {
        public override (eExecutionResult, string) Execute(Instruction instruction)
        {
            if (!ValidateInstructionKeyword(instruction.Action))
            {
                return (eExecutionResult.Failed, "Invalid instruction keyword! 'attach_to' was expected.");
            }

            if (instruction.DataType != Instruction.EDataType.Json ||
                instruction.State != Instruction.EInstructionState.Resolved)
            {
                return (eExecutionResult.Failed, "Invalid instruction data!");
            }

            JSONObject jsonObjData = JSON.Parse(instruction.Data).AsObject;
            string sourceName = jsonObjData["source"];
            string targetName = jsonObjData["target"];
            var sourceObj = GameObject.Find(sourceName.Trim());
            var targetObj = GameObject.Find(targetName.Trim());

            if(sourceObj == null || targetObj == null)
            {
                string missingObjName = sourceObj == null ? sourceName : targetName;
                return (eExecutionResult.Failed, $"Couldn't find object {missingObjName}");
            }

            sourceObj.transform.parent = targetObj.transform;
            Executor.ForceUpdateSceneView();

            return (eExecutionResult.Succeeded, $"{sourceName} was attached to {targetName}.");
        }
    }
}
#endif