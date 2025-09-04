using System.Collections.Generic;
using System.Reflection;

namespace T2G
{
    [Translator("hello")]
    public class HelloTranslator : Translator
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
            instruction.Type = Instruction.EExecutionType.LocalCmd;
            instruction.State = Instruction.EInstructionState.Empty;
            instruction.Action = attribute.InstructionKey;
            instruction.DataType = Instruction.EDataType.Empty;
            instruction.Data = string.Empty;
            instructions.Add(instruction);
            return (true, null);
        }
    }
}