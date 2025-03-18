using System.Collections.Generic;

namespace T2G
{
    [Translator("create_project")]
    public class CreateProject_Translator : Translator
    {
        public override (bool succeeded, string message) Translate((string name, string value)[] arguments, ref List<Instruction> instructions)
        {
            Instruction instruction = new Instruction();
            instruction.ExecutionType = Instruction.EExecutionType.LocalCmd;
            instruction.State = Instruction.EInstructionState.Resolved;
            instruction.KeyWord = CmdCreateProject.CommandKey;
            instruction.ParamType = Instruction.EParameterType.SingleParameter;
            instruction.parameter = GetParamFromArguments(arguments, "path");
            instructions.Add(instruction);
            return (instruction.parameter != null, null);
        }
    }
}