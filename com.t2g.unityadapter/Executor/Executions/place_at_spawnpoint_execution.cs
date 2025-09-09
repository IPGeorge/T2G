#if UNITY_EDITOR

using SimpleJSON;
using System.Threading.Tasks;
using UnityEngine;

namespace T2G.Executor
{
    [Execution("place_at_spawnpoint")]
    public class place_at_spawnpoint_execution : Execution
    {
        public override (eExecutionResult, string) Execute(Instruction instruction)
        {
            if (!ValidateInstructionKeyword(instruction.Action))
            {
                return (eExecutionResult.Failed, "Invalid instruction keyword! 'place_at_spawnpoint' was expected.");
            }

            if (instruction.DataType != Instruction.EDataType.Json ||
                instruction.State != Instruction.EInstructionState.Resolved)
            {
                return (eExecutionResult.Failed, "Invalid instruction data!");
            }

            JSONObject jsonObjData = JSON.Parse(instruction.Data).AsObject;
            string objName = jsonObjData["objectName"];
            string spawnpointNames = jsonObjData["spawnpointNames"];
            string[] spawnpointNamesArray = spawnpointNames.Split(",");

            GameObject obj = GameObject.Find(objName);
            if(obj == null)
            {
                return (eExecutionResult.Failed, $"Couldn't find object {objName} in current space!");
            }

            string spawnpointName = null;
            GameObject spawnpoint = null;
            if (spawnpointNamesArray.Length > 1)
            {
                spawnpointName = spawnpointNamesArray[Random.Range(0, spawnpointNamesArray.Length)];
                spawnpoint = GameObject.Find(spawnpointName);
            }
            else if (spawnpointNamesArray.Length > 0)
            {
                spawnpointName = spawnpointNamesArray[0];
                spawnpoint = GameObject.Find(spawnpointName);
            }

            if (spawnpoint == null)
            {
                return (eExecutionResult.Failed, $"Couldn't find spawnpoint {spawnpointName} in current space!");
            }

            obj.transform.SetPositionAndRotation(spawnpoint.transform.position, spawnpoint.transform.rotation);
            Executor.SaveActiveScene();

            return (eExecutionResult.Succeeded, $"{objName} was placed at {spawnpointName}.");
        }
    }
}

#endif