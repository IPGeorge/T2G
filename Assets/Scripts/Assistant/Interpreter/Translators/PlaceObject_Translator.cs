using SimpleJSON;
using System;
using System.Collections.Generic;

namespace T2G
{
    [Translator("place_at_spawnpoint")]
    public class PlaceObject_Translator : Translator 
    {
        public override (bool succeeded, string message) Translate((string name, string value)[] arguments, ref List<Instruction> instructions)
        {
            instructions.Clear();

            var attributeName = GetAttributeName();
            if (attributeName == null)
            {
                return (false, k_FailedToRetrieveAttribute);
            }

            Instruction instruction = new Instruction();
            instruction.ExecutionType = Instruction.EExecutionType.EditingOp;
            instruction.State = Instruction.EInstructionState.Resolved;
            instruction.Keyword = attributeName;
            instruction.DataType = Instruction.EDataType.JsonData;
            string objName = GetParamFromArguments(arguments, "objectName");
            string spawnpointNames = GetParamFromArguments(arguments, "spawnpointNames");
            if(string.IsNullOrEmpty(objName) || string.IsNullOrEmpty(spawnpointNames))
            {
                return (false, null);
            }
            JSONObject jsonObj = new JSONObject();
            jsonObj.Add("objectName", objName);
            jsonObj.Add("spawnpointNames", spawnpointNames);
            instruction.Data = jsonObj.ToString();
            instruction.DataType = Instruction.EDataType.JsonData;
            instructions.Add(instruction);
            return (true, null);
        }
    }
}