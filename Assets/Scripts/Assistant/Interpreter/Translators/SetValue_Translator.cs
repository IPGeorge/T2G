using SimpleJSON;
using System.Collections.Generic;

namespace T2G
{
    [Translator("set_value")]
    public class SetValue_Translator : Translator
    {
        public override (bool succeeded, string message) Translate((string name, string value)[] arguments, ref List<Instruction> instructions)
        {
            instructions.Clear();

            var attributeName = GetAttributeName();
            if (attributeName == null)
            {
                return (false, k_FailedToRetrieveAttribute);
            }

            Instruction instruction = new Instruction();

            string objName = GetParamFromArguments(arguments, "objName", string.Empty);
            string property = GetParamFromArguments(arguments, "property", string.Empty);
            string value = GetParamFromArguments(arguments, "value", string.Empty);

            if (!string.IsNullOrEmpty(property) && !string.IsNullOrEmpty(value))
            {
                instruction = new Instruction();
                instruction.ExecutionType = Instruction.EExecutionType.EditingOp;
                instruction.State = Instruction.EInstructionState.Resolved;
                instruction.Keyword = "set_value";
                instruction.DataType = Instruction.EDataType.JsonData;
                JSONObject jsonValues = new JSONObject();
                jsonValues.Add("objName", objName);
                jsonValues.Add("property", property);
                jsonValues.Add("value", value);
                instruction.Data = jsonValues.ToString();
                instructions.Add(instruction);
            }

            return (true, null);
        }
    }
}