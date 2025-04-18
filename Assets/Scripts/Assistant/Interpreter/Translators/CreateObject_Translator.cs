using System;
using System.Collections.Generic;
using System.Reflection;
using SimpleJSON;

namespace T2G
{
    [Translator("create_object")]
    public class CreateObject_Translator : Translator
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
            instruction.State = Instruction.EInstructionState.Raw;
            instruction.Keyword = attribute.InstructionKey;
            instruction.DataType = Instruction.EDataType.JsonData;
            string name = GetParamFromArguments(arguments, "name");
            string type = GetParamFromArguments(arguments, "type").Trim();
            JSONObject jsonObj = new JSONObject();
            jsonObj.Add("name", string.IsNullOrEmpty(name) ? $"Obj{DateTime.Now.Ticks}" : name);
            jsonObj.Add("type", type);
            instruction.Data = jsonObj.ToString();

            instructions.Add(instruction);
            return (true, null);
        }
    }
}
