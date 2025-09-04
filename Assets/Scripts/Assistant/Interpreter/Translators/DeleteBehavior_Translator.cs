using SimpleJSON;
using System.Collections.Generic;
using System.Reflection;

namespace T2G
{
    [Translator("remove_behavior")]
    public class DeleteBehavior_Translator : Translator
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
            instruction.Type = Instruction.EExecutionType.EditingOp;
            instruction.State = Instruction.EInstructionState.Resolved;
            instruction.Action = attribute.InstructionKey;
            instruction.DataType = Instruction.EDataType.JsonData;
            string name = GetParamFromArguments(arguments, "name");
            string objName = GetParamFromArguments(arguments, "object");
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(objName))
            {
                return (false, null);
            }
            else
            {
                JSONObject jsonObj = new JSONObject();
                jsonObj.Add("behaviorName", name);
                jsonObj.Add("objectName", objName);
                instruction.Data = jsonObj.ToString();
                instructions.Add(instruction);
                return (true, null);
            }
        }
    }

}