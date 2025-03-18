using System.Collections.Generic;
using UnityEngine;

namespace T2G
{
    [Translator("init_project")]
    public class InitProject_Translator : Translator
    {
        public override (bool succeeded, string message) Translate((string name, string value)[] arguments, ref List<Instruction> instructions)
        {
            instructions.Clear();
            Instruction instruction = new Instruction();
            instruction.ExecutionType = Instruction.EExecutionType.LocalCmd;
            instruction.State = Instruction.EInstructionState.Resolved;
            instruction.Keyword = CmdInitProject.CommandKey;
            instruction.ParamType = Instruction.EParameterType.SingleParameter;
            instruction.parameter = GetParamFromArguments(arguments, "path");
            if (instruction.parameter == null)
            {
                instruction.parameter = PlayerPrefs.GetString(Defs.k_GameProjectPath, string.Empty);
                if(string.IsNullOrEmpty(instruction.parameter))
                {
                    return (false, k_MissingPath);
                }
            }
            instructions.Add(instruction);
            return (true, null);
        }
    }
}