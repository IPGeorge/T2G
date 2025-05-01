using SimpleJSON;
using System.Collections.Generic;
using System.Reflection;

namespace T2G
{
    [Translator("select_object")]

    public class select_object : Translator
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
            instruction.ExecutionType = Instruction.EExecutionType.EditingOp;
            instruction.State = Instruction.EInstructionState.Resolved;
            instruction.Keyword = attribute.InstructionKey;
            instruction.DataType = Instruction.EDataType.SingleParameter;
            string name = GetParamFromArguments(arguments, "name");
            if (string.IsNullOrEmpty(name))
            {
                return (false, null);
            }
            else
            {
                instruction.Data = name;
                instructions.Add(instruction);
                return (true, null);
            }
        }
    }
}