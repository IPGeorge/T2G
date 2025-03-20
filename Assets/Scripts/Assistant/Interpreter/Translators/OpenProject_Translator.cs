using System.Collections.Generic;
using UnityEngine;

namespace T2G
{
    [Translator("open_project")]
    public class OpenProject_Translator : Translator
    {
        public override (bool succeeded, string message) Translate((string name, string value)[] arguments, ref List<Instruction> instructions)
        {
            instructions.Clear();
            Instruction instruction = new Instruction();
            instruction.ExecutionType = Instruction.EExecutionType.LocalCmd;
            instruction.State = Instruction.EInstructionState.Resolved;
            instruction.Keyword = CmdOpenProject.CommandKey;
            instruction.DataType = Instruction.EDataType.SingleParameter;
            instruction.Data = GetParamFromArguments(arguments, "path");
            if (instruction.Data == null)
            {
                instruction.Data = PlayerPrefs.GetString(Defs.k_GameProjectPath, string.Empty);
                if (string.IsNullOrEmpty(instruction.Data))
                {
                    return (false, k_MissingPath);
                }
            }
            instructions.Add(instruction);
            return (true, null);
        }
    }
}