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
        public async override Awaitable<(eExecutionResult, string)> ExecuteAsync(Instruction instruction)
        {
            if (!ValidateInstructionKeyword(instruction.Action))
            {
                return (eExecutionResult.Failed, "Invalid instruction keyword! 'import_script' was expected.");
            }

            if (instruction.DataType != Instruction.EDataType.Empty ||
                instruction.State != Instruction.EInstructionState.Resolved)
            {
                return (eExecutionResult.Failed, "Invalid instruction data!");
            }

            bool result = await ContentLibrary.ImportAsset(instruction.ResolvedAssetPaths);
            var scriptName = Path.GetFileName(instruction.ResolvedAssetPaths);
            if (result)
            {
                return (eExecutionResult.Succeeded, $"{scriptName} was imported.");
            }
            else
            {
                return (eExecutionResult.Failed, $"Failed to import {scriptName}!"); 
            }
        }
    }
}
#endif
