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

        [InitializeOnLoadMethod]
        static void InitializeOnLoad()
        {
            if (EditorPrefs.HasKey(Defs.k_InstructionExecutionResponseMessage) &&
                EditorPrefs.HasKey(Defs.k_InstructionExecutionResponseSucceeded))
            {
                bool succeeded = EditorPrefs.GetBool(Defs.k_InstructionExecutionResponseMessage);
                string message = EditorPrefs.GetString(Defs.k_InstructionExecutionResponseSucceeded);
                SendInstructionExecutionResponse(succeeded, message);
            }
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
            if (instruction != null && _executionPool.ContainsKey(instruction.Keyword))
            {
                var result = await _executionPool[instruction.Keyword].Execute(instruction);
                //The following line will be executed when InitializeOnload doesn't happen
                SendInstructionExecutionResponse(result.succeeded, result.message);
                return result.succeeded;
            }
            else
            {
                SendInstructionExecutionResponse(false, "Invalid instruction key!");
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

        static void SendInstructionExecutionResponse(bool succeeded, string message)
        {
            JSONObject jsonObj = new JSONObject();
            jsonObj.Add("succeeded", succeeded);
            jsonObj.Add("message", message);
            CommunicatorServer.Instance.SendMessage(eMessageType.Response, jsonObj.ToString());
        }
    }

    /* deprecated
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

            Dictionary<string, ExecutionBase> _executionPool = new Dictionary<string, ExecutionBase>();

            public class Instruction
            {
                public string Command { get; set; }
                public List<string> Arguments { get; set; }

                public Instruction(string command, List<string> arguments)
                {
                    Command = command;
                    Arguments = arguments;
                }
            }

            private List<Instruction> Parse(string[] instructions)
            {
                List<Instruction> commands = new List<Instruction>();

                foreach (var instruction in instructions)
                {
                    var commandTuple = ParseInstruction(instruction);

                    commands.Add(new Instruction(commandTuple.command, commandTuple.arguments));
                }

                return commands;
            }

            private (string command, List<string> arguments) ParseInstruction(string instruction)
            {
                if (string.IsNullOrWhiteSpace(instruction))
                {
                    throw new ArgumentException("Input cannot be null or whitespace.");
                }
                // Regular expression to match the command and arguments
                var matches = Regex.Matches(instruction, @"[\""].+?[\""]|[^ ]+");
                if (matches.Count == 0)
                {
                    throw new ArgumentException("No command found in input.");
                }

                var command = matches[0].Value.Trim('"');
                var arguments = new List<string>();

                for (int i = 1; i < matches.Count; ++i)
                {
                    arguments.Add(matches[i].Value.Trim('"'));
                }

                return (command, arguments);
            }

            public Executor()
            {
                var assembly = Assembly.GetExecutingAssembly();
                var executionClasses = assembly.GetTypes()
                    .Where(type => type.IsClass && type.GetCustomAttributes(typeof(ExecutionAttribute), false).Any());
                foreach(var executionClass in executionClasses)
                {
                    var attribute = executionClass.GetCustomAttribute<ExecutionAttribute>();
                    var execution = Activator.CreateInstance(executionClass) as ExecutionBase;
                    _executionPool.Add(attribute.Instruction, execution);
                }

                ExecutionADDON.RegisterAddAddonExecutions();
            }

            private void Execute(List<Instruction> instructions)
            {
                foreach (var instruction in instructions)
                {
                    Execute(instruction);
                }
            }

            private void Execute(Instruction instruction)
            {
                if(string.Compare(instruction.Command, Defs.k_EndOfGameGeneration) == 0)
                {
                    //Start processing postponed instructions
                    foreach(var ins in _instructionBuffer)
                    {
                        Execute(ins);
                    }
                    RespondCompletion(true, "Game generation is completed!");
                    return;
                }

                if(_executionPool.ContainsKey(instruction.Command))
                {
                    _executionPool[instruction.Command].HandleExecution(instruction);
                }
                else
                {
                    Debug.LogWarning($"[Executor.Execute] Unrecognized command: {instruction.Command}");
                }
            }

            public bool Execute(string message)
            {
                if (message.Substring(0, 4).CompareTo("INS>") == 0)
                {
                    var commandTuple = ParseInstruction(message.Substring(4));
                    var command = new Instruction(commandTuple.command, commandTuple.arguments);
                    Execute(command);
                    return true;
                }
                return false;
            }
        }
    */
}

#endif