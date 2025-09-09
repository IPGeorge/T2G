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

        public enum EInstructionState
        {
            Init,
            Raw,
            Resolved,
            MissingResource
        }
        public enum EDataType
        {
            Empty,
            SingleParameter,
            MultipleParameters,
            Json
        }

        public EExecutionType Type = EExecutionType.Void;
        public bool RequiresPreviousSuccess = false;    //Indicates if it depends on previous success.
        public EInstructionState State = EInstructionState.Init;
        public string Action = string.Empty;
        public EDataType DataType = EDataType.SingleParameter;
        public string Data = string.Empty;       
        public string Assets = string.Empty; 

        public Instruction()
        {
            Type = EExecutionType.Void;
            RequiresPreviousSuccess = false;
            State = EInstructionState.Init;
            DataType = EDataType.SingleParameter;
            Data = string.Empty;
            Assets = string.Empty;
        }

        public Instruction(JSONObject jsonObj)
        {
            Type = (EExecutionType)jsonObj["Type"].AsInt;
            RequiresPreviousSuccess = jsonObj["RequiresPreviousSuccess"].AsBool;
            State = (EInstructionState)jsonObj["State"].AsInt;
            DataType = (EDataType)jsonObj["DataType"].AsInt;
            Data = jsonObj["Data"];
            Assets = jsonObj["Assets"];
        }
    }
}