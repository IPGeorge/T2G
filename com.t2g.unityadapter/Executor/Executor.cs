#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using T2G.Communicator;
using UnityEngine;
using SimpleJSON;
using UnityEditor;

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
        }

        [InitializeOnLoadMethod]
        static void InitializeOnLoad()
        {
            if (EditorPrefs.HasKey(Defs.k_InstructionExecutionResponseMessage) &&
                EditorPrefs.HasKey(Defs.k_InstructionExecutionResponseSucceeded))
            {
                bool succeeded = EditorPrefs.GetBool(Defs.k_InstructionExecutionResponseMessage);
                string message = EditorPrefs.GetString(Defs.k_InstructionExecutionResponseSucceeded);
                JSONObject jsonObj = new JSONObject();
                jsonObj.Add("succeeded", succeeded);
                jsonObj.Add("message", message);
                CommunicatorServer.Instance.SendMessage(eMessageType.Response, jsonObj.ToString());
            }
        }

        public async Awaitable<bool> Execute(Instruction instruction)
        {
            if (instruction != null && _executionPool.ContainsKey(instruction.KeyWord))
            {
                var result = await _executionPool[instruction.KeyWord].Execute(instruction);

                //The following code will be executed when InitializeOnload doesn't happen
                JSONObject jsonObj = new JSONObject();
                jsonObj.Add("succeeded", result.succeeded);
                jsonObj.Add("message", result.message);
                CommunicatorServer.Instance.SendMessage(eMessageType.Response, jsonObj.ToString());
                return result.succeeded;
            }
            else
            {
                CommunicatorServer.Instance.SendMessage(eMessageType.Response, "");
                return false;
            }
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