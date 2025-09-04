using System.Collections.Generic;
using System.Reflection;

namespace T2G
{
    [Translator("create_space")]
    public class CreateSpace_Translator : Translator
    {
        public override (bool succeeded, string message) Translate((string name, string value)[] arguments, ref List<Instruction> instructions)
        {
            instructions.Clear();

            var attribute = GetType().GetCustomAttribute<TranslatorAttribute>();
            if(attribute == null)
            {
                return (false, k_FailedToRetrieveAttribute);
            }
            
            Instruction instruction = new Instruction();
            instruction.Type = Instruction.EExecutionType.EditingOp;
            instruction.State = Instruction.EInstructionState.Resolved;
            instruction.Action = attribute.InstructionKey;
            instruction.DataType = Instruction.EDataType.SingleParameter;
            foreach (var argument in arguments)
            {
                if (string.Compare(argument.name, "name") == 0)
                {
                    instruction.Data = argument.value;
                    break;
                }
            }

            if (string.IsNullOrEmpty(instruction.Data))
            {
                return (false, k_MissingName);
            }

            instructions.Add(instruction);
            return (true, null);
        }
    }
}