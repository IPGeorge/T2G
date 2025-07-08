#if UNITY_EDITOR

using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace T2G.Executor
{
    [Execution("set_font")]
    public class Set_Font_Execution : Execution
    {
        public async override Awaitable<(bool succeeded, string message)> Execute(Instruction instruction)
        {
            if (!ValidateInstructionKeyword(instruction.Keyword))
            {
                return (false, "Invalid instruction keyword! 'set_font' was expected.");
            }

            if (instruction.DataType != Instruction.EDataType.JsonData)
            {
                return (false, "Invalid instruction data!");
            }

            var jsonObj = GetInstructionJsonData(instruction);
            string attrib = jsonObj["attrib"];
            string value = jsonObj["value"];

            if (string.IsNullOrEmpty(attrib) || string.IsNullOrEmpty(value))
            {
                return (false, "Invalid font attribute or attribute value!");
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
                        return (false, null);
                }
                return (true, null);
            }
        }
    }
}

#endif