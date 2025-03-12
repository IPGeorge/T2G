using System;
using UnityEngine;

namespace T2G
{
    [Serializable]
    public class Instruction
    {
        public enum EExecutionType
        {
            Void,
            LocalCommand,          //executed locally
            ParseGameDesc,             //Parse GameDesc data intro instructions
            EngineInstruction          //Send to the engine for execution
        }
        public EExecutionType ExecutionType = EExecutionType.Void;
        public bool DependentCommand = false;

        public enum EInstructionState
        {
            Empty,
            Raw,
            Resolved
        }
        public EInstructionState State = EInstructionState.Empty;

        public enum EParameterType
        {
            StringValue,
            Parameters,
            JsonData
        }

        public string Key = string.Empty;
        public EParameterType ParamType = EParameterType.StringValue;
        public string parameter = string.Empty;       //JSON data
    }
}