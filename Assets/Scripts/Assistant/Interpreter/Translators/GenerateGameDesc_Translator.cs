using SimpleJSON;
using System.Collections.Generic;

namespace T2G
{
    [Translator("generate_gamedesc")]
    public class GenerateGameDesc_Translator : Translator
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

            string filePath = GetParamFromArguments(arguments, "filePath", string.Empty);

            if (!string.IsNullOrEmpty(filePath))
            {
                instruction = new Instruction();
                instruction.ExecutionType = Instruction.EExecutionType.EditingOp;
                instruction.State = Instruction.EInstructionState.Resolved;
                instruction.Keyword = attributeName;
                instruction.DataType = Instruction.EDataType.SingleParameter;
                instruction.Data = filePath; 
                instructions.Add(instruction);
            }

            return (true, null);
        }
    }
}