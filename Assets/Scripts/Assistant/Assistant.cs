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
                foreach(var instruction in instructions)
                {
                    if(instruction.ExecutionType == Instruction.EExecutionType.LocalCommand)
                    {
                        CommandSystem.Instance.ExecuteCommand((result, sender, message) =>
                        {
                        },
                        instruction.Key, instruction.parameter);
                    }

                    
                }
                    
                    Debug.Log($"Processed input '{inputText}' and generated instructions: \n{instructions[0].parameter}");


                response?.Invoke("Done!");
            }
            else
            {
                response?.Invoke("Sorry, I don't understand!");
            }
        }
    }
}
