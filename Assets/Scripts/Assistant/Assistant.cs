using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;
using System;
using T2G.Communicator;
using System.Threading.Tasks;

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
            var result = await Interpreter.Instance.InterpretPrompt(inputText);
            if (result.instructions != null && result.instructions.Length > 0)
            {
                bool prevSuccess = true;
                int failedCount = 0;
                foreach(var instruction in result.instructions)
                {
                    var procResult = await ProcessInstruction(instruction, prevSuccess);
                    prevSuccess = procResult.succeeded;
                    if (!prevSuccess)
                    {
                        failedCount++;
                    }
                    if(!string.IsNullOrEmpty(procResult.responseMessage))
                    {
                        response?.Invoke(procResult.responseMessage);
                    }
                }

                Debug.Log($"Processed input '{inputText}' and generated instructions: \n{result.instructions[0].parameter}");

                if (prevSuccess)
                {
                    response?.Invoke("Done!");
                }
                else
                {
                    response?.Invoke(failedCount == result.instructions.Length ? 
                        "Failed!" : 
                        $"Result: {result.instructions.Length - failedCount} succeeded; {failedCount} failed!");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(result.responseMessage))
                {
                    response?.Invoke("Sorry, I don't understand!");
                }
                else
                {
                    response?.Invoke(result.responseMessage);
                }
            }
        }

        async Awaitable<(bool succeeded, string responseMessage)> ProcessInstruction(Instruction instruction, bool previousSuccess)
        {
            bool result = false;
            if (instruction.RequiresPreviousSuccess && !previousSuccess)
            {
                return (result, "Failed due to dependency on a previously failed execution!");
            }

            string responseMessage = string.Empty;
            switch(instruction.ExecutionType)
            {
                case Instruction.EExecutionType.LocalCmd:
                    result = await CommandSystem.Instance.ExecuteCommand(instruction.Keyword, instruction.parameter);
                    break;
                case Instruction.EExecutionType.EditingOp:
                    if(instruction.State == Instruction.EInstructionState.Raw)
                    {
                        ContentLibrary.ResolveInstruction(ref instruction);    //Will fill up missing contents
                    }
                    
                    if (instruction.State == Instruction.EInstructionState.Resolved)
                    {
                        var response = await SendToProjectForExecution(instruction);
                        result = response.result;
                        responseMessage = response.message;
                    }
                    else if (instruction.State == Instruction.EInstructionState.ResolveWithMissingResource)
                    {
                        var response = await SendToProjectForExecution(instruction);
                        result = response.result;
                        responseMessage = response.message + " (Missing assets have been replaced with substitutes)";
                    }
                    else
                    {
                        result = false;
                    }
                    break;
                case Instruction.EExecutionType.GameDesc:  //Parse to generate instructions
                    result = false;
                    break;
                default:
                    break;
            }
            return (result, responseMessage);
        }

        async Awaitable<(bool result, string message)> SendToProjectForExecution(Instruction instruction)
        {
            string json = JsonUtility.ToJson(instruction);
            await CommunicatorClient.Instance.SendMessageAsync(eMessageType.Instruction, json);

            string messageStr = string.Empty;
            float waitTimeOut = 180.0f;      //hard-coded the waiting timeout seconds
            while(CommunicatorClient.Instance.IsConnected && waitTimeOut > 0.0f)
            {
                if (CommunicatorClient.Instance.PopReceivedMessage(out var response))
                {
                    bool result = true;
                    if (response.Type == eMessageType.Message)
                    {
                        messageStr = response.Message.ToString();
                    }
                    else if (response.Type == eMessageType.Response)
                    {
                        JSONObject responseObj = JSON.Parse(response.Message.ToString()).AsObject;
                        result = responseObj["succeeded"].AsBool;
                        messageStr = responseObj["message"];
                    }
                    else
                    {
                        messageStr = "Received unexpected type of response!";
                        result = false;
                    }
                    return (result, messageStr);
                }
                await Task.Delay(100);
                waitTimeOut -= 0.1f;
            }
            return (false, string.Empty);
        }
    }
}
