using SimpleJSON;
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

        public string Keyword = string.Empty;
        public enum EDataType
        {
            Empty,
            SingleParameter,
            MultipleParameters,
            JsonData
        }
        public EDataType DataType = EDataType.SingleParameter;
        public string Data = string.Empty;       //JSON data

        public Instruction()
        {
            ExecutionType = EExecutionType.Void;
            RequiresPreviousSuccess = false;
            State = EInstructionState.Empty;
            DataType = EDataType.SingleParameter;
            Data = string.Empty;
        }

        public Instruction(JSONObject jsonObj)
        {
            ExecutionType = (EExecutionType)jsonObj["ExecutionType"].AsInt;
            RequiresPreviousSuccess = jsonObj["RequiresPreviousSuccess"].AsBool;
            State = (EInstructionState)jsonObj["State"].AsInt;
            DataType = (EDataType)jsonObj["DataType"].AsInt;
            Data = jsonObj["Data"];
        }
    }
}