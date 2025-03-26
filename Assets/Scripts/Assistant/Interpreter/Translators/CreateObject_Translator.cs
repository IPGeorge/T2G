using System;
using System.Collections.Generic;
using SimpleJSON;

namespace T2G
{
    [Translator("create_object")]
    public class CreateObject_Translator : Translator
    {
        public override (bool succeeded, string message) Translate((string name, string value)[] arguments, ref List<Instruction> instructions)
        {
            Instruction instruction = new Instruction();
            instruction.ExecutionType = Instruction.EExecutionType.LocalCmd;
            instruction.State = Instruction.EInstructionState.Resolved;
            instruction.Keyword = CmdCreateProject.CommandKey;
            instruction.DataType = Instruction.EDataType.JsonData;

            string name = GetParamFromArguments(arguments, "name");
            string type = GetParamFromArguments(arguments, "type");
            JSONObject jsonObj = new JSONObject();
            jsonObj.Add("name", string.IsNullOrEmpty(name) ? $"NoName{DateTime.Now.Ticks}" : name);
            jsonObj.Add("type", type);
            instruction.Data = jsonObj.ToString();

            instructions.Add(instruction);
            return (true, null);
        }
    }
}
