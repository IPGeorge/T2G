#if UNITY_EDITOR

using SimpleJSON;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace T2G.Executor
{
    [Execution("import_script")]
    public class import_script_execution : Execution
    {
        public async override Awaitable<(bool succeeded, string message)> Execute(Instruction instruction)
        {
            if (!ValidateInstructionKeyword(instruction.Keyword))
            {
                return (false, "Invalid instruction keyword! 'import_script' was expected.");
            }

            if (instruction.DataType != Instruction.EDataType.Empty ||
                instruction.State != Instruction.EInstructionState.Resolved)
            {
                return (false, "Invalid instruction data!");
            }

            bool result = await ContentLibrary.ImportAsset(instruction.ResolvedAssetPaths);
            var scriptName = Path.GetFileName(instruction.ResolvedAssetPaths);
            if (result)
            {
                return (true, $"{scriptName} was imported.");
            }
            else
            {
                return (false, $"Failed to import {scriptName}!"); 
            }
        }
    }
}
#endif
