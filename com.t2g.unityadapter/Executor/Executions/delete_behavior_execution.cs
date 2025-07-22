#if UNITY_EDITOR

using SimpleJSON;
using System;
using T2G.Executor;
using UnityEngine;

namespace T2G
{
    [Execution("remove_behavior")]
    public class delete_behavior_execution : Execution
    {
        public override (eExecutionResult, string) Execute(Instruction instruction)
        {
            if (!ValidateInstructionKeyword(instruction.Keyword))
            {
                return (eExecutionResult.Failed, "Invalid instruction keyword! 'remove_behavior' was expected.");
            }

            string behaviorName = string.Empty;
            string objName = string.Empty;

            if (instruction.DataType == Instruction.EDataType.JsonData)
            {
                JSONObject jsonObj = JSON.Parse(instruction.Data).AsObject;
                if (jsonObj.HasKey("behaviorName"))
                {
                    behaviorName = jsonObj["behaviorName"];
                }
                if (jsonObj.HasKey("objectName"))
                {
                    objName = jsonObj["objectName"];
                }
            }

            GameObject gameObj = GameObject.Find(objName.Trim());

            if (gameObj != null)
            {
                Type behaviorType = Executor.Executor.GetClassTypeByName(behaviorName);
                if (behaviorType != null)
                {
                    Executor.Executor.DeleteObjectComponent(gameObj, behaviorType);
                }
                return (eExecutionResult.Succeeded, $"{behaviorName} was removed from {objName}.");
            }
            else
            {
                return (eExecutionResult.Failed, $"Couldn't find game object: {objName}!");
            }
        }
    }
}

#endif