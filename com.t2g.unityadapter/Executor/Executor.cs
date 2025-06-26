#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using T2G.Communicator;
using UnityEngine;
using SimpleJSON;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using System.Collections;

namespace T2G.Executor
{
    public partial class Executor
    {
        static Executor _instance = null;
        public static Executor Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Executor();
                }
                return _instance;
            }
        }

        Dictionary<string, Execution> _executionPool = new Dictionary<string, Execution>();
        Queue<Instruction> _instructionQueue = new Queue<Instruction>();
        bool _isActive = false;
        bool _isBusy = false;
        bool _prevExecutionSucceeded = true;

        public Executor()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var executionClasses = assembly.GetTypes()
                .Where(type => type.IsClass && type.GetCustomAttributes(typeof(ExecutionAttribute), false).Any());
            foreach (var executionClass in executionClasses)
            {
                var attribute = executionClass.GetCustomAttribute<ExecutionAttribute>();
                var execution = Activator.CreateInstance(executionClass) as Execution;
                _executionPool.Add(attribute.Keyword, execution);
            }
            EditorCoroutineUtility.StartCoroutine(this.ExecuteQueuedInstructionCoroutine(), this);
        }

        ~Executor()
        {
            _isActive = false;
        }

         public static void SendExecutionResponse(bool? succeeded = null)
        {
            if (EditorPrefs.HasKey(Defs.k_InstructionExecutionHasResponseMessage))
            {
                bool hasResponse = EditorPrefs.GetBool(Defs.k_InstructionExecutionHasResponseMessage);
                if(succeeded == null)
                {
                    succeeded = EditorPrefs.GetBool(Defs.k_InstructionExecutionSucceeded, true);
                }

                string message = succeeded.Value ?
                    EditorPrefs.GetString(Defs.k_InstructionExecutionResponseSucceeded) :
                    EditorPrefs.GetString(Defs.k_InstructionExecutionResponseFailed);

                SendInstructionExecutionResponse(succeeded.Value, message);
                ClearResponseForInitializeOnLoad();
            }
            else
            {
                SendInstructionExecutionResponse(succeeded.Value, succeeded.Value ? "Done!" : "Failed!");
            }
        }

        static void SendInstructionExecutionResponse(bool succeeded, string message)
        {
            JSONObject jsonObj = new JSONObject();
            jsonObj.Add("succeeded", succeeded);
            jsonObj.Add("message", message);
            CommunicatorServer.Instance.SendMessage(eMessageType.Response, jsonObj.ToString());
        }

        public static void SetResponseForInitializeOnLoad(string messageTrue = "Done!", string messageFalse = "Failed!", bool? succeeded = null)
        {
            EditorPrefs.SetBool(Defs.k_InstructionExecutionHasResponseMessage, true);
            if(succeeded != null && !succeeded.Value)
            {
                EditorPrefs.SetBool(Defs.k_InstructionExecutionResponseSucceeded, false);
            }
            else
            {
                EditorPrefs.SetBool(Defs.k_InstructionExecutionResponseSucceeded, true);
            }

            EditorPrefs.SetString(Defs.k_InstructionExecutionResponseSucceeded, messageTrue);
            EditorPrefs.SetString(Defs.k_InstructionExecutionResponseFailed, messageFalse);
        }

        public static string GetSucceededResponseMessage()
        {
            string defaultResponse = "Done!";
            if (EditorPrefs.GetBool(Defs.k_InstructionExecutionHasResponseMessage, false))
            {
                return EditorPrefs.GetString(Defs.k_InstructionExecutionResponseSucceeded, defaultResponse);
            }
            else
            {
                return defaultResponse;
            }
        }

        public static string GetFailedResponseMessage()
        {
            string defaultResponse = "Failed!";
            if (EditorPrefs.GetBool(Defs.k_InstructionExecutionHasResponseMessage, false))
            {
                return EditorPrefs.GetString(Defs.k_InstructionExecutionResponseFailed, defaultResponse);
            }
            else
            {
                return defaultResponse;
            }
        }

        public static void ClearResponseForInitializeOnLoad()
        {
            EditorPrefs.SetBool(Defs.k_InstructionExecutionHasResponseMessage, false);
        }

        public void EnqueueInstruction(Instruction instruction)
        {
            if (instruction != null &&
                instruction.ExecutionType == Instruction.EExecutionType.EditingOp)
            {
                _instructionQueue.Enqueue(instruction);
            }
            else
            {
                instruction = new Instruction();
                instruction.Keyword = "invalid";
                instruction.ExecutionType = Instruction.EExecutionType.Void;
                instruction.DataType = Instruction.EDataType.Empty;
                instruction.Data = string.Empty;
                instruction.RequiresPreviousSuccess = false;
                instruction.State = Instruction.EInstructionState.Empty;
                _instructionQueue.Enqueue(instruction);
            }
        }

        public async Awaitable<bool> Execute(Instruction instruction)
        {
            if (instruction != null && _executionPool.ContainsKey(instruction.Keyword.ToLower()))
            {
                var result = await _executionPool[instruction.Keyword].Execute(instruction);
                SendInstructionExecutionResponse(result.succeeded, result.message); //is executed when InitializeOnload didn't happen
                return result.succeeded;
            }
            else
            {
                SendInstructionExecutionResponse(false, $"Invalid instruction key: {instruction.Keyword.ToLower()}");
                return false;
            }
        }

        async void ExecuteInstructionASync(Instruction instruction)
        {
            _isBusy = true;
            _prevExecutionSucceeded = await Execute(instruction);
            _isBusy = false;
        }

        IEnumerator ExecuteQueuedInstructionCoroutine()
        {
            _isActive = true;
            while(_isActive)
            {
                if (!_isBusy && _instructionQueue.Count > 0)
                {
                    var instruction = _instructionQueue.Dequeue();
                    if (!_prevExecutionSucceeded && instruction.RequiresPreviousSuccess)
                    {
                        SendInstructionExecutionResponse(_prevExecutionSucceeded,
                            "Skipped because the previous execution failed!");
                    }
                    else
                    {
                        ExecuteInstructionASync(instruction);
                    }
                }

                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}

#endif