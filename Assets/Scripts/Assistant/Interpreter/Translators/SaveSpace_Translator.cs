using SimpleJSON;
using System.Collections.Generic;
using System.Reflection;

namespace T2G
{
    [Translator("save_space")]
    public class SaveSpace_Translator : Translator
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
            instruction.DataType = Instruction.EDataType.Empty;
            instructions.Add(instruction);
            return (true, null);
        }
    }
}