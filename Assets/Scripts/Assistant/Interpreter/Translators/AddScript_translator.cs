using SimpleJSON;
using System.Collections.Generic;

namespace T2G
{
    [Translator("add_script")]
    public class AddScript_translator : Translator
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
            instruction.ExecutionType = Instruction.EExecutionType.EditingOp;
            instruction.State = Instruction.EInstructionState.Resolved;
            instruction.Keyword = attributeName;
            instruction.DataType = Instruction.EDataType.JsonData;
            string scriptFilePath = GetParamFromArguments(arguments, "filepath");
            string objectName = GetParamFromArguments(arguments, "object", string.Empty);
            if (!string.IsNullOrWhiteSpace(scriptFilePath))
            {
                JSONObject jsonObj = new JSONObject();
                jsonObj.Add("path", scriptFilePath);
                jsonObj.Add("object", objectName);
                instruction.Data = jsonObj.ToString();
                instructions.Add(instruction);
                return (true, null);
            }
            else
            {
                return (false, "Invalid script file path!");
            }
        }

    }
}
