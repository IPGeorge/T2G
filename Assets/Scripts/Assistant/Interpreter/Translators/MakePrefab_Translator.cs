using System;
using System.Collections.Generic;
using SimpleJSON;

namespace T2G
{
    [Translator("make_prefab")]
    public class MakePrefab_Translator : Translator
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
            instruction.DataType = Instruction.EDataType.SingleParameter;
            instruction.Data = GetParamFromArguments(arguments, "name");
            instructions.Add(instruction);
            return (true, null);
        }
    }
}