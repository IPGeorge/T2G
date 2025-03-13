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
            LocalCmd,            //executed locally
            EditingOp,           //Send to the engine for execution
            GameDesc             //GameDesc data into instructions to create a project
        }
        public EExecutionType ExecutionType = EExecutionType.Void;
        public bool RequiresPreviousSuccess = false;    //Indicates if it depends on previous success.

        public enum EInstructionState
        {
            Empty,
            Raw,
            Resolved,
            ResolveWithMissingResource
        }
        public EInstructionState State = EInstructionState.Empty;

        public string KeyWord = string.Empty;
        public enum EParameterType
        {
            Empty,
            SingleParameter,
            MultipleParameters,
            JsonData
        }
        public EParameterType ParamType = EParameterType.SingleParameter;
        public string parameter = string.Empty;       //JSON data
    }
}