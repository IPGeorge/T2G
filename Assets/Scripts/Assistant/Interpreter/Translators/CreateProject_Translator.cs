using System.Collections.Generic;

namespace T2G
{
    [Translator("create_project")]
    public class CreateProject_Translator : Translator
    {
        public override bool Translate((string name, string value)[] arguments, ref List<Instruction> instructions)
        {
            Instruction instruction = new Instruction();
            instruction.ExecutionType = Instruction.EExecutionType.LocalCommand;
            instruction.State = Instruction.EInstructionState.Resolved;
            instruction.Key = "CreateProject";
            instruction.ParamType = Instruction.EParameterType.StringValue;
            instruction.parameter = GetParamFromArguments(arguments, "path");
            instructions.Add(instruction);
            return (instruction.parameter != null);
        }
    }
}