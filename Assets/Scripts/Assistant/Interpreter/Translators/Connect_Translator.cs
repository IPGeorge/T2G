using System.Collections.Generic;

namespace T2G
{
    [Translator("connect")]
    public class Connect_Translator : Translator
    {
        public override bool Translate((string name, string value)[] arguments, ref List<Instruction> instructions)
        {
            instructions.Clear();
            Instruction instruction = new Instruction();
            instruction.ExecutionType = Instruction.EExecutionType.LocalCmd;
            instruction.State = Instruction.EInstructionState.Resolved;
            instruction.KeyWord = CmdConnect.CommandKey;
            instruction.ParamType = Instruction.EParameterType.Empty;
            instruction.parameter = string.Empty;
            instructions.Add(instruction);
            return true;
        }
    }
}