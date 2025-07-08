#if UNITY_EDITOR

using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace T2G.Executor
{
    [Execution("print_text")]
    public class Print_Text_Execution : Execution
    {
        public async override Awaitable<(bool succeeded, string message)> Execute(Instruction instruction)
        {
            if (!ValidateInstructionKeyword(instruction.Keyword))
            {
                return (false, "Invalid instruction keyword! 'print_text' was expected.");
            }

            if (instruction.DataType != Instruction.EDataType.JsonData)
            {
                return (false, "Invalid instruction data!");
            }

            var jsonObj = GetInstructionJsonData(instruction);
            string text = jsonObj["text"];
            string pos = jsonObj["position"];

            if(string.IsNullOrEmpty(text) || string.IsNullOrEmpty(pos))
            {
                return (false, null);
            }
            else
            {
                if(GameObject.FindFirstObjectByType<TextPrinter>() == null)
                {
                    GameObject textPrinter = new GameObject("TextPrinter");
                    textPrinter.AddComponent<TextPrinter>();
                }

                var startPos = Executor.ParseFloat2(pos);
                if (startPos == null)
                {
                    TextPrinter.Instance.PrintText(text, pos);      //e.g. top-left, center, bottom-right, etc.
                }
                else
                {
                    TextPrinter.Instance.PrintText(text, (int)startPos[0], (int)startPos[1]);
                }
                await Task.Yield();
                return (true, null);
            }
        }
    }
}

#endif