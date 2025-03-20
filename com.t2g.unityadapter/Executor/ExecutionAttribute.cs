#if UNITY_EDITOR

using SimpleJSON;
using System;
using System.Reflection;
using System.Threading.Tasks;
using T2G.Communicator;
using UnityEditor;
using UnityEngine;

namespace T2G.Executor
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ExecutionAttribute : Attribute
    {
        public string Keyword { get; private set; }

        public ExecutionAttribute(string keyword)
        {
            Keyword = keyword;
        }
    }

    public abstract class Execution
    {
        public Execution() {  }

        public abstract Awaitable<(bool succeeded, string message)> Execute(Instruction instruction);
        protected virtual void StoreResponseForInitializeOnLoad()
        {
            EditorPrefs.SetBool(Defs.k_InstructionExecutionResponseMessage, true);
            EditorPrefs.SetString(Defs.k_InstructionExecutionResponseSucceeded, "Done!");
        }

        protected bool ValidateInstructionKeyword(string keyword)
        {
            var attribute = GetType().GetCustomAttribute<ExecutionAttribute>();
            return (string.Compare(keyword.ToLower(), attribute.Keyword.ToLower()) == 0);
        }
    }

    
    [Execution("test")]
    public class test_Execution : Execution
    {
        public async override Awaitable<(bool succeeded, string message)> Execute(Instruction instruction)
        {
            await Task.Delay(1000);
            return (true, "Ok!");
        }
    }

    [Execution("invalid")]
    public class invalid_Execution : Execution
    {
        public async override Awaitable<(bool succeeded, string message)> Execute(Instruction instruction)
        {
            await Task.Delay(100);
            return (true, "Invalid instruction!");
        }
    }

}

#endif