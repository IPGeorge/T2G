using System.Collections.Generic;
using SimpleJSON;

namespace T2G
{
    [Translator("print_text")]
    public class PrintText_translator : Translator
    {
        public override (bool succeeded, string message) Translate((string name, string value)[] arguments, ref List<Instruction> instructions)
        {
            var attributeName = GetAttributeName();
            if (attributeName == null)
            {
                return (false, k_FailedToRetrieveAttribute);
            }
           
            var text = GetParamFromArguments(arguments, "text", string.Empty);
            var pos = GetParamFromArguments(arguments, "pos", string.Empty);

            if(string.IsNullOrEmpty(text))
            {
                return (false, null);
            }

            instructions.Clear();
            Instruction instruction = new Instruction();
            instruction.ExecutionType = Instruction.EExecutionType.EditingOp;
            instruction.State = Instruction.EInstructionState.Resolved;
            instruction.Keyword = attributeName;
            instruction.DataType = Instruction.EDataType.JsonData;
            JSONObject jsonObj = new JSONObject();
            jsonObj.Add("text", text);
            jsonObj.Add("position", pos);
            instruction.Data = jsonObj.ToString();
            instructions.Add(instruction);
            return (true, null);
        }
    }
}
