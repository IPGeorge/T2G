using System.Collections.Generic;

namespace T2G
{
    [Translator("clear")]
    public class Clear_Translator : Translator
    {
        public override (bool succeeded, string message) Translate((string name, string value)[] arguments, ref List<Instruction> instructions)
        {
            instructions.Clear();
            Instruction instruction = new Instruction();
            instruction.ExecutionType = Instruction.EExecutionType.LocalCmd;
            instruction.State = Instruction.EInstructionState.Resolved;
            instruction.Keyword = CmdClear.CommandKey;
            instruction.DataType = Instruction.EDataType.Empty;
            instruction.Data = string.Empty;
            instructions.Add(instruction);
            return (true, null);
        }
    }
}