using System;
using System.Collections.Generic;
using UnityEngine;

namespace T2G.UnityAdapter
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ExecutionAttribute : Attribute
    {
        public string Instruction { get; private set; }

        public ExecutionAttribute(string instruction)
        {
            Instruction = instruction;
        }
    }

    public abstract class ExecutionBase
    {
        protected static GameObject s_currentObject = null;
        public ExecutionBase() {  }
        public static void SetCurrentObject(GameObject gameObject)
        {
            s_currentObject = gameObject;
        }
        public abstract void HandleExecution(Executor.Instruction instruction);
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class AddAddonAttribute : Attribute
    {
        public string AddonType { get; private set; }

        public AddAddonAttribute(string addonType)
        {
            AddonType = addonType;
        }
    }

    public abstract class AddAddonBase
    {
        public AddAddonBase() { }
        public abstract void AddAddon(GameObject gameObject, List<string> properties);
    }
}