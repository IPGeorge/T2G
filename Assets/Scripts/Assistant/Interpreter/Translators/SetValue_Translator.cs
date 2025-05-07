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

            string objName = GetParamFromArguments(arguments, "name", string.Empty);
            string fieldName = GetParamFromArguments(arguments, "property", string.Empty);
            string fieldValue = GetParamFromArguments(arguments, "value", string.Empty);
            string scriptName = GetParamFromArguments(arguments, "script", string.Empty);

            if (!string.IsNullOrEmpty(fieldName) && !string.IsNullOrEmpty(fieldValue))
            {
                instruction = new Instruction();
                instruction.ExecutionType = Instruction.EExecutionType.EditingOp;
                instruction.State = Instruction.EInstructionState.Resolved;
                instruction.Keyword = "set_value";
                instruction.DataType = Instruction.EDataType.JsonData;
                JSONObject jsonValues = new JSONObject();
                jsonValues.Add("name", objName);
                jsonValues.Add("fieldName", fieldName);
                jsonValues.Add("value", fieldValue);
                jsonValues.Add("scriptName", scriptName);
                jsonValues.Add("dataType", "float");
                instruction.Data = jsonValues.ToString();
                instructions.Add(instruction);
            }

            return (true, null);
        }

    }
}