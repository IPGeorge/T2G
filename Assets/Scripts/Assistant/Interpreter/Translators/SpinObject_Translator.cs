using SimpleJSON;
using System.Collections.Generic;
using System.Reflection;

namespace T2G
{
    [Translator("spin_object")]
    public class SpinObject_Translator : Translator
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
            instruction.ExecutionType = Instruction.EExecutionType.EditingOp;
            instruction.State = Instruction.EInstructionState.Raw;
            instruction.Keyword = "import_script";
            instruction.DataType = Instruction.EDataType.Empty;
            instruction.Data = "spin controller";
            instructions.Add(instruction);

            instruction = new Instruction();
            instruction.ExecutionType = Instruction.EExecutionType.EditingOp;
            instruction.State = Instruction.EInstructionState.Resolved;
            instruction.Keyword = "add_script";
            instruction.RequiresPreviousSuccess = true;
            instruction.DataType = Instruction.EDataType.JsonData;
            string name = GetParamFromArguments(arguments, "name", string.Empty);
            JSONObject jsonObj = new JSONObject();
            jsonObj.Add("name", name);
            jsonObj.Add("scriptName", "SpinController");
            instruction.Data = jsonObj.ToString();
            instructions.Add(instruction);

            string speedStr = GetParamFromArguments(arguments, "speed", string.Empty);
            if (!string.IsNullOrEmpty(speedStr))
            {
                instruction = new Instruction();
                instruction.ExecutionType = Instruction.EExecutionType.EditingOp;
                instruction.State = Instruction.EInstructionState.Resolved;
                instruction.Keyword = "set_values";
                instruction.DataType = Instruction.EDataType.JsonData;
                JSONObject jsonValues = new JSONObject();
                jsonObj.Add("name", name);
                jsonObj.Add("speed", speedStr);
                instruction.Data = jsonObj.ToString();
                instructions.Add(instruction);
            }

            return (true, null);
        }
    }

}