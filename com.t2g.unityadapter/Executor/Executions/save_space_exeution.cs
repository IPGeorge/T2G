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
        public async override Awaitable<(bool succeeded, string message)> Execute(Instruction instruction)
        {
            if (!ValidateInstructionKeyword(instruction.Keyword))
            {
                return (false, "Invalid instruction keyword! 'save_space' was expected.");
            }

            Executor.SaveActiveScene();
            await Task.Yield();
            return (true, $"{EditorSceneManager.GetActiveScene().name} was saved!");
        }
    }
}

#endif