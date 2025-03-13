using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;
using System;

namespace T2G
{
    public class Assistant : MonoBehaviour
    {
        public static Assistant Instance { get; private set; }
        public Interpreter Interpreter { get; private set; }

        GameDesc _gameDesc = new GameDesc();

        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            Interpreter = Interpreter.Instance;
            _gameDesc = new GameDesc();
        }

        public async void ProcessInput(string inputText, Action<string> response)
        {
            var instructions = await Interpreter.Instance.InterpretPrompt(inputText);
            if (instructions != null && instructions.Length > 0)
            {
                bool prevSuccess = true;
                int failedCount = 0;
                foreach(var instruction in instructions)
                {
                    prevSuccess = await ProcessInstruction(instruction, prevSuccess);
                    if(!prevSuccess)
                    {
                        failedCount++;
                    }
                }
                    
                Debug.Log($"Processed input '{inputText}' and generated instructions: \n{instructions[0].parameter}");
                if (prevSuccess)
                {
                    response?.Invoke("Done!");
                }
                else
                {
                    response?.Invoke(failedCount == instructions.Length ? 
                        "Failed!" : 
                        $"Result: {instructions.Length - failedCount} succeeded; {failedCount} failed!");
                }
            }
            else
            {
                response?.Invoke("Sorry, I don't understand!");
            }
        }

        async Awaitable<bool> ProcessInstruction(Instruction instruction, bool previousSuccess)
        {
            bool result = false;
            if (instruction.RequiresPreviousSuccess && !previousSuccess)
            {
                return result;
            }

            switch(instruction.ExecutionType)
            {
                case Instruction.EExecutionType.LocalCmd:
                    result = await CommandSystem.Instance.ExecuteCommand(instruction.KeyWord, instruction.parameter);
                    break;
                case Instruction.EExecutionType.EditingOp:
                    if(instruction.State == Instruction.EInstructionState.Raw)
                    {
                        //ContentLibrary.SelectAssets(ref instruction);
                        if(instruction.State == Instruction.EInstructionState.Resolved)
                        {

                        }
                        
                    }
                    else if (instruction.State == Instruction.EInstructionState.Resolved)
                    {

                    }
                    else
                    {
                        result = true;
                    }
                    break;
                case Instruction.EExecutionType.GameDesc:
                    break;
                default:
                    break;
            }
            return result;
        }
    }
}
