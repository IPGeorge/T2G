using SimpleJSON;
using System.Collections.Generic;
using SimpleJSON;

namespace T2G
{
    [Translator("build_structure")]
    public class BuildStructure_Translator : Translator
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
            instruction.State = Instruction.EInstructionState.Raw;
            instruction.Keyword = attributeName;
            instruction.DataType = Instruction.EDataType.JsonData;
            
            string shape = GetParamFromArguments(arguments, "shape");
            string structure = GetParamFromArguments(arguments, "structure");
            string element = GetParamFromArguments(arguments, "element");
            string name = GetParamFromArguments(arguments, "name");
            int width = 30;
            int height = 30;
            JSONObject jsonObj = new JSONObject();
            jsonObj.Add("name", name);
            jsonObj.Add("shape", shape);
            jsonObj.Add("structure", structure);
            jsonObj.Add("type", element);
            jsonObj.Add("width", width);
            jsonObj.Add("height", height);

            instruction.Data = jsonObj.ToString();
            instructions.Add(instruction);
            return (true, null);
        }
    }
}
