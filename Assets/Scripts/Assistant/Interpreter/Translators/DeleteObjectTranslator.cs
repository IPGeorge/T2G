using SimpleJSON;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace T2G
{
    [Translator("delete_object")]
    public class DeleteObjectTranslator : Translator
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
            instruction.DataType = Instruction.EDataType.JsonData;
            string name = GetParamFromArguments(arguments, "name");
            if (string.IsNullOrEmpty(name))
            {
                return (false, null);
            }
            else
            {
                JSONObject jsonObj = new JSONObject();
                jsonObj.Add("name", name);
                instruction.Data = jsonObj.ToString();
                instructions.Add(instruction);
                return (true, null);
            }
        }
    }
}
