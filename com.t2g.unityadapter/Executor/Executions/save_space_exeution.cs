#if UNITY_EDITOR

using SimpleJSON;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace T2G.Executor
{
    [Execution("save_space")]
    public class save_space_exeution : Execution
    {
        public override (eExecutionResult, string) Execute(Instruction instruction)
        {
            if (!ValidateInstructionKeyword(instruction.Action))
            {
                return (eExecutionResult.Failed, "Invalid instruction keyword! 'save_space' was expected.");
            }

            Executor.SaveActiveScene();
            return (eExecutionResult.Succeeded, $"{EditorSceneManager.GetActiveScene().name} was saved!");
        }
    }
}

#endif