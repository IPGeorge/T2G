#if UNITY_EDITOR

using System.Threading.Tasks;
using UnityEngine;

namespace T2G.Executor
{
    [Execution("set_font")]
    public class Set_Font_Execution : Execution
    {
        public override (eExecutionResult, string) Execute(Instruction instruction)
        {
            if (!ValidateInstructionKeyword(instruction.Action))
            {
                return (eExecutionResult.Failed, "Invalid instruction keyword! 'set_font' was expected.");
            }

            if (instruction.DataType != Instruction.EDataType.Json)
            {
                return (eExecutionResult.Failed, "Invalid instruction data!");
            }

            var jsonObj = GetInstructionJsonData(instruction);
            string attrib = jsonObj["attrib"];
            string value = jsonObj["value"];

            if (string.IsNullOrEmpty(attrib) || string.IsNullOrEmpty(value))
            {
                return (eExecutionResult.Failed, "Invalid font attribute or attribute value!");
            }
            else
            {
                if (GameObject.FindFirstObjectByType<TextPrinter>() == null)
                {
                    GameObject textPrinter = new GameObject("TextPrinter");
                    textPrinter.AddComponent<TextPrinter>();
                }

                switch (attrib)
                {
                    case "size":
                        if (int.TryParse(value, out var sizeValue))
                        {
                            TextPrinter.Instance.SetTextAttributes("size", sizeValue);
                        }
                        break;
                    case "color":
                        TextPrinter.Instance.SetTextAttributes("color", value);
                        break;
                    case "bold":
                    case "italic":
                        if (bool.TryParse(value, out var boolValue))
                        {
                            TextPrinter.Instance.SetTextAttributes(attrib, boolValue);
                        }
                        break;
                    default:
                        return (eExecutionResult.Failed, null);
                }
                return (eExecutionResult.Succeeded, null);
            }
        }
    }
}

#endif