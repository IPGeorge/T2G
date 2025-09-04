using SimpleJSON;
using System.Collections.Generic;

namespace T2G
{
    [Translator("set_font")]

    public class SetFont_Translator : Translator
    {
        public override (bool succeeded, string message) Translate((string name, string value)[] arguments, ref List<Instruction> instructions)
        {
            var attributeName = GetAttributeName();
            if (attributeName == null)
            {
                return (false, k_FailedToRetrieveAttribute);
            }

            instructions.Clear();
            Instruction instruction = new Instruction();
            instruction.Type = Instruction.EExecutionType.EditingOp;
            instruction.State = Instruction.EInstructionState.Resolved;
            instruction.Action = attributeName;
            var attribName = GetParamFromArguments(arguments, "attrib");
            var attribValue = GetParamFromArguments(arguments, "value");
            if (string.IsNullOrEmpty(attribName) || string.IsNullOrEmpty(attribValue))
            {
                return (false, "Invalid font attribute or value!");
            }

            JSONObject jsonObj = new JSONObject();
            jsonObj.Add("attrib", attribName);          //size, color, bold, itallic, etc.
            jsonObj.Add("value", attribValue);          //24, (red or #FF0000), true/false, true/false 
            instruction.Data = jsonObj.ToString();
            instructions.Add(instruction);
            instruction.DataType = Instruction.EDataType.JsonData;
            instruction.Data = jsonObj.ToString();
            instructions.Add(instruction);
            return (true, null);
        }
    }
}
