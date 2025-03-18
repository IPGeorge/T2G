#if UNITY_EDITOR

using SimpleJSON;
using System;
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
    }

    
    public class TextExecution : Execution
    {
        public async override Awaitable<(bool succeeded, string message)> Execute(Instruction instruction)
        {
            await Task.Delay(1000);
            return (true, "Test response Ok!");
        }
    }

/*
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class AddComponentAttribute : Attribute
    {
        public string AddonType { get; private set; }

        public AddComponentAttribute(string addonType)
        {
            AddonType = addonType;
        }
    }

    public abstract class AddAddonBase
    {
        public AddAddonBase() { }
        public abstract void AddAddon(GameObject gameObject, List<string> properties);
    }
*/
}

#endif