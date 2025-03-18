using System.Collections.Generic;

namespace T2G
{
    [Translator("disconnect")]
    public class Disconnect_Translator : Translator
    {
        public override (bool succeeded, string message) Translate((string name, string value)[] arguments, ref List<Instruction> instructions)
        {
            instructions.Clear();
            Instruction instruction = new Instruction();
            instruction.ExecutionType = Instruction.EExecutionType.LocalCmd;
            instruction.State = Instruction.EInstructionState.Resolved;
            instruction.KeyWord = CmdDisconnect.CommandKey;
            instruction.ParamType = Instruction.EParameterType.Empty;
            instruction.parameter = string.Empty;
            instructions.Add(instruction);
            return (true, null);
        }
    }
}