using UnityEngine;
using SimpleJSON;
using System.Collections.Generic;
using System.IO;

namespace T2G
{
    [Translator("add_behavior")]
    public class AddBehavior_Translator : Translator
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
            instruction.Type = Instruction.EExecutionType.EditingOp;
            instruction.State = Instruction.EInstructionState.Resolved;
            instruction.Action = attributeName;
            instruction.DataType = Instruction.EDataType.JsonData;
            string behaviorName = GetParamFromArguments(arguments, "name", string.Empty);
            string objectName = GetParamFromArguments(arguments, "object", string.Empty);
            JSONObject jObj = new JSONObject();
            jObj.Add("behaviorName" , behaviorName);
            jObj.Add("objectName", objectName);
            instruction.Data = jObj.ToString();
            var result = QuestionaireManager.Instance.StartANewTopic(new AddBehaviorTopic(instruction));
            return (true, null);
        }
    }
}
