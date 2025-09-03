using System;
using System.Collections.Generic;
using SimpleJSON;

namespace T2G
{
    [Translator("attach_to")]
    public class AttachTo_Translator : Translator
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
            instruction.Action = attributeName;
            instruction.DataType = Instruction.EDataType.JsonData;
            string sourceName = GetParamFromArguments(arguments, "source");
            string targetName = GetParamFromArguments(arguments, "target").Trim();
            JSONObject jsonObj = new JSONObject();
            jsonObj.Add("source", sourceName);
            jsonObj.Add("target", targetName);
            instruction.Data = jsonObj.ToString();

            instructions.Add(instruction);
            return (true, null);
        }
    }
}