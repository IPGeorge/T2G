using SimpleJSON;
using System.Collections.Generic;
using System.IO;

namespace T2G
{
    [Translator("add_script")]
    public class AddScript_translator : Translator
    {
        string _scriptFilePath;

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
            instruction.DataType = Instruction.EDataType.Json;
            _scriptFilePath = GetParamFromArguments(arguments, "filepath");
            string objectName = GetParamFromArguments(arguments, "object", string.Empty);
            if (!string.IsNullOrWhiteSpace(_scriptFilePath))
            {
                JSONObject jsonObj = new JSONObject();

                if (!File.Exists(_scriptFilePath))
                {
                    SearchForScriptPath();
                }
                jsonObj.Add("path", _scriptFilePath);
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

        async void SearchForScriptPath()
        {
            _scriptFilePath = await ContentLibrary.SearchAssets(_scriptFilePath, "Script");
        }
    }
}
