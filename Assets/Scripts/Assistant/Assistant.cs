using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;
using System;
using T2G.Communicator;
using System.Threading.Tasks;
using System.Collections;

namespace T2G
{
    public class Assistant : MonoBehaviour
    {
        public static Assistant Instance { get; private set; }
        public Interpreter Interpreter { get; private set; }

        GameDesc _gameDesc = new GameDesc();

        bool _waittingForResponse = false;
        public bool WaitingForResponse => _waittingForResponse;

        bool _response_succeeded = true;
        string _response_message = string.Empty;


        private void Awake()
        {
            Instance = this;
            CommunicatorClient.Instance.OnDisconnectedFromServer += OnDisconnectedFromServerHandler;
            CommunicatorClient.Instance.OnReceivedMessage += WaitForInstructionExecutionResponse;
            StartCoroutine(AutoConnectToServerAfterDisconnection());
        }

        void Start()
        {
            Interpreter = Interpreter.Instance;
            _gameDesc = new GameDesc();
        }

        private void OnDestroy()
        {
            CommunicatorClient.Instance.OnReceivedMessage -= WaitForInstructionExecutionResponse;
            CommunicatorClient.Instance.OnDisconnectedFromServer -= OnDisconnectedFromServerHandler;
        }

        void OnDisconnectedFromServerHandler()
        {
            
        }

        IEnumerator AutoConnectToServerAfterDisconnection()
        {
            while(true)
            {
                yield return new WaitForSeconds(5.0f);
                if (CommunicatorClient.Instance.ClientState == CommunicatorClient.eClientState.Disconnected)
                {
                    CommunicatorClient.Instance.StartClient(true);
                }
            }
        }

        public async Awaitable ProcessInput(string inputText, Action<string> response)
        {
            if (QuestionaireManager.Instance.IsActive)
            {
                bool answered = QuestionaireManager.Instance.AnswerQuestion(inputText, out Topic completedTopic);
                if (answered && completedTopic != null && completedTopic.TopicIsOver())
                {
                    ConsoleController.Instance.WriteConsoleMessage(ConsoleController.eSender.Assistant, "Working on it ...");
                    string answer = completedTopic.AnswersSummary;
                    await completedTopic.PostTopicProcess(answer);
                    if(!string.IsNullOrWhiteSpace(completedTopic.Instruction.ResolvedAssetPaths))
                    {
                        var procResult = await ProcessInstruction(completedTopic.Instruction, true);
                        var hasResponseMesasge = !string.IsNullOrEmpty(procResult.responseMessage);
                        response?.Invoke(hasResponseMesasge ? procResult.responseMessage : "Failed!");
                    }
                }
            }
            else
            {
                var result = await Interpreter.Instance.InterpretPrompt(inputText);
                
                if(QuestionaireManager.Instance.IsActive)
                {
                    response?.Invoke(null);
                    return;
                }

                if (result.instructions != null && result.instructions.Length > 0)
                {
                    bool prevSuccess = true;
                    int failedCount = 0;
                    bool hasResponseMesasge = false;

                    foreach (var instruction in result.instructions)
                    {
                        var procResult = await ProcessInstruction(instruction, prevSuccess);

                        prevSuccess = procResult.succeeded;
                        if (prevSuccess)
                        {
                            hasResponseMesasge = !string.IsNullOrEmpty(procResult.responseMessage);
                            if (hasResponseMesasge)
                            {
                                response?.Invoke(procResult.responseMessage);
                            }
                        }
                        else
                        {
                            failedCount++;
                        }
                    }

                    if (!hasResponseMesasge)
                    {
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
                    {
                        var cmdResult = await CommandSystem.Instance.ExecuteCommand(instruction.Action, instruction.Data);
                        result = cmdResult.succeeded;
                        responseMessage = cmdResult.response;
                    }
                    break;
                case Instruction.EExecutionType.EditingOp:
                    if(instruction.State == Instruction.EInstructionState.Raw)
                    {
                        var newInstruction = await ContentLibrary.ResolveInstruction(instruction);    //find the assets
                        if(instruction == null)
                        {
                            return (false, "Failed to resolve instruction!");
                        }
                        instruction = newInstruction;
                    }
                    
                    if (instruction.State == Instruction.EInstructionState.Resolved)
                    {
                        var response = await SendToProjectForExecution(instruction);
                        result = response.result;
                        responseMessage = response.message;
                    }
                    else if (instruction.State == Instruction.EInstructionState.MissingResource)
                    {
                        var response = await SendToProjectForExecution(instruction);
                        result = response.result;
                        responseMessage = response.message + " (Missing assets, the default is used.)";
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

        void WaitForInstructionExecutionResponse(eMessageType type, string message)
        {
            if(!_waittingForResponse)
            {
                return;
            }

            if(type == eMessageType.Message)
            {
                _response_succeeded = true;
                _response_message = message;
                ConsoleController.Instance.WriteConsoleMessage(ConsoleController.eSender.Assistant, message);
                _waittingForResponse = false;
            }
            else if(type == eMessageType.Response)
            {
                JSONObject jsonObj = JSON.Parse(message).AsObject;
                _response_succeeded = jsonObj["succeeded"].AsBool;
                _response_message = jsonObj["message"];
                _waittingForResponse = false;
            }
        }

        async Awaitable<(bool result, string message)> SendToProjectForExecution(Instruction instruction)
        {
            string json = JsonUtility.ToJson(instruction);
            await CommunicatorClient.Instance.SendMessageAsync(eMessageType.Instruction, json);

            _waittingForResponse = true;

            float waitTimeOut = 60.0f;      //hard-coded the waiting timeout seconds
            while(waitTimeOut > 0.0f)
            {
                if(!_waittingForResponse)
                {
                    return (_response_succeeded, _response_message);
                }
                await Task.Delay(100);
                waitTimeOut -= 0.1f;
            }
            QuestionaireManager.Instance.Clear();
            return (false, "Time out!");
        }
    }
}
