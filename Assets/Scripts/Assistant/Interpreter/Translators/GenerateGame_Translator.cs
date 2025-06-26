using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace T2G
{
    [Translator("generate_game")]

    public class GenerateGame_Translator : Translator
    {
        public override (bool succeeded, string message) Translate((string name, string value)[] arguments, ref List<Instruction> instructions)
        {
            instructions.Clear();

            var attribute = GetType().GetCustomAttribute<TranslatorAttribute>();
            if (attribute == null)
            {
                return (false, k_FailedToRetrieveAttribute);
            }

            Instruction instruction = new Instruction();
            instruction.ExecutionType = Instruction.EExecutionType.LocalCmd;
            instruction.State = Instruction.EInstructionState.Resolved;
            instruction.Keyword = CmdGenerateGame.CommandKey;
            instruction.DataType = Instruction.EDataType.SingleParameter;
            instruction.Data = GetParamFromArguments(arguments, "path");
            instructions.Add(instruction);

            return (instruction.Data != null, null);
        }
    }
}