using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using T2G.Communicator;

namespace T2G
{
    public class SimAssistant : MonoBehaviour
    {
        [SerializeField] RectTransform _AssistantDialogsRectTransform;
        [SerializeField] GameDescForm _GameDescForm;

        GameDesc _gameDesc = new GameDesc();

        string[] _prompts = {
        "hello",
        "hi",
        "create a new game",
        "make a new game",
        "generate game",
        "Start generating"
    };

        string[] _responses = {
        "Hi {user}, I am {assistant}, your game development assistant. What can I do for you?",
        "Hello {user}, I am {assistant} who will assist you to develop games. What can I do for you?",
        "Okay! I need some initial information about the game, please fill up the Game Description form.",
        "Okay, Start generating the game ..."
    };


        Dictionary<string, List<int>> _promptResponseMap = new Dictionary<string, List<int>>();
        Dictionary<string, Func<string, int>> _responseActionMap = new Dictionary<string, Func<string, int>>();

        List<string> _matchedPrompts = new List<string>();

        static SimAssistant _instance = null;
        public static SimAssistant Instance => _instance;

        private void Awake()
        {
            _instance = this;

            _promptResponseMap.Add(_prompts[0], new List<int>(new int[] { 0, 1 }));
            _promptResponseMap.Add(_prompts[1], new List<int>(new int[] { 0, 1 }));
            _promptResponseMap.Add(_prompts[2], new List<int>(new int[] { 2 }));
            _promptResponseMap.Add(_prompts[3], new List<int>(new int[] { 2 }));
            _promptResponseMap.Add(_prompts[4], new List<int>(new int[] { 3 }));
            _promptResponseMap.Add(_prompts[5], new List<int>(new int[] { 3 }));

            _responseActionMap.Add(_responses[2], CollectGameProjectInformation);
            _responseActionMap.Add(_responses[3], GenerateGameFromGameDesc);
        }

        public void OnDestopPanelResized(float desktopHeight)
        {
            _AssistantDialogsRectTransform.offsetMin = new Vector2(0.0f, desktopHeight);
        }

        public void ProcessPrompt(string prompt, Action<string> callBack)
        {
            string promptKey = string.Empty;
            string responseMessage = "Sorry, I don't understand what you mean!";

            _matchedPrompts.Clear();
            if (Utilities.FindTopMatches(prompt, _prompts, 3, 0.5f, ref _matchedPrompts))
            {
                int count = _matchedPrompts.Count;
                if (count > 1)
                {
                    promptKey = _matchedPrompts[UnityEngine.Random.Range(0, count)];
                }
                else if (count > 0)
                {
                    promptKey = _matchedPrompts[0];
                }

                if (_promptResponseMap.TryGetValue(promptKey, out var responseOptions))
                {
                    count = responseOptions.Count;
                    if (count > 1)
                    {
                        responseMessage = _responses[responseOptions[UnityEngine.Random.Range(0, count)]];
                    }
                    else if (count > 0)
                    {
                        responseMessage = _responses[responseOptions[0]];
                    }

                    if (_responseActionMap.TryGetValue(responseMessage, out var function))
                    {
                        int result = (function?.Invoke(responseMessage)).Value;
                        if (result > 0)
                        {
                            responseMessage = _responses[result];
                        }
                    }
                }
            }
            else
            {
                var responseTask = PromptToInstruct(prompt);
                responseMessage = responseTask.Result;
            }
            callBack?.Invoke(responseMessage);
        }

        int CollectGameProjectInformation(string responseMessage)
        {
            _GameDescForm.gameObject.SetActive(true);
            return 0;
        }

        async Task CreateProjectFromGameDesc(GameDesc gameDesc)
        {
            string[] args = new string[1] { gameDesc.GetProjectPathName() };
            await CommandSystem.Instance.ExecuteCommand("CreateProject", args);
        }

        async Task InitProject(GameDesc gameDesc)
        {
            string[] args = new string[1] { gameDesc.GetProjectPathName() };
            await CommandSystem.Instance.ExecuteCommand("InitProject", args);
        }

        async Task OpenProject(GameDesc gameDesc)
        {
            string[] args = new string[1] { gameDesc.GetProjectPathName() };
            await CommandSystem.Instance.ExecuteCommand("OpenProject", args);
        }

        async Task<bool> Connect(float timeout = 300.0f)
        {
            bool retVal = true;
            string[] args = new string[1] { ((int)timeout).ToString() };
            ConsoleController.Instance.WaitForConnect = true;
            float timer = 0.0f;
            while (!CommunicatorClient.Instance.IsConnected && timer < timeout)
            {
                CommunicatorClient.Instance.StartClient(true);
                await Task.Delay(1000);
                timer += 1.0f;
            }
            bool saved = ConsoleController.Instance.WaitForConnect = true;
            if (timer >= timeout)
            {
                ConsoleController.Instance.WriteConsoleMessage(ConsoleController.eSender.Assistant, "Failed to connect to the server!");
                retVal = false;
            }
            ConsoleController.Instance.WaitForConnect = saved;

            return retVal;
        }

        async Task GenerateGameAsync(GameDesc gameDesc, string gameDescJson)
        {
            string currentPrj = PlayerPrefs.GetString(Defs.k_GameProjectPath, string.Empty);
            string generatPrj = gameDesc.GetProjectPathName();
            bool skipCreatingProject = CommunicatorClient.Instance.IsConnected && (currentPrj.CompareTo(generatPrj) == 0);
            if (!skipCreatingProject)
            {
                ConsoleController console = ConsoleController.Instance;
                await CreateProjectFromGameDesc(gameDesc);
                console.WriteConsoleMessage(ConsoleController.eSender.Assistant, "Project was created. initilaizing the project ...");
                await InitProject(gameDesc);
                console.WriteConsoleMessage(ConsoleController.eSender.Assistant, "Project was initialized, opening the project in ...");
                await OpenProject(gameDesc);

                bool connected = await Connect();
                if (connected)
                {
                    ConsoleController.Instance.HandleOnConnectedToServer();
                }
                else
                {
                    console.WriteConsoleMessage(ConsoleController.eSender.Error, "Connection to the game project has timed out! ");
                    return;
                }
                console.WriteConsoleMessage(ConsoleController.eSender.Assistant, "Project was opened and connected!");
            }

            string[] instructions = await Interpreter.Instance.InterpretGameDesc(gameDescJson);

            int errorCode = 0;
            for (int i = 0; i < instructions.Length; ++i)
            {
                errorCode = await SendInstruction(instructions[i]);
                if (errorCode > 0)
                {
                    break;
                }
            }

            if (errorCode > 0)
            {
                ConsoleController.Instance.WriteConsoleMessage(ConsoleController.eSender.Assistant,
                $"Game generation was interrupted. ErrorCode: {errorCode}");
            }
        }

        async Task<int> SendInstruction(string instruction, float timeout = 60.0f)
        {
            float timer = 0.0f;
            float connectionTimeout = timeout;
            while (!CommunicatorClient.Instance.IsConnected)
            {
                CommunicatorClient.Instance.StartClient(true);
                await Task.Delay(1000);
                timer += 1.0f;
                if (timer >= connectionTimeout)
                {
                    ConsoleController.Instance.WriteConsoleMessage(ConsoleController.eSender.Error, "No connection!");
                    return 2;
                }
            }

            string receivedMessage = string.Empty;
            Action<eMessageType, string> waitForResponse = (type, message) =>
            {
                receivedMessage = message;
            };
            CommunicatorClient.Instance.OnReceivedMessage += waitForResponse;
            if (CommunicatorClient.Instance.SendMessage(eMessageType.Instruction, $"INS>{instruction}"))
            {
                ConsoleController.Instance.WriteConsoleMessage(ConsoleController.eSender.Assistant, $"Instruction>{instruction}");
                timer = 0.0f;
                while (string.IsNullOrEmpty(receivedMessage))
                {
                    await Task.Delay(100);
                    timer += 0.1f;
                    if (!CommunicatorClient.Instance.IsConnected)
                    {
                        CommunicatorClient.Instance.StartClient(true);
                    }
                    if (timer >= timeout)
                    {
                        if (!CommunicatorClient.Instance.IsConnected)
                        {
                            ConsoleController.Instance.WriteConsoleMessage(ConsoleController.eSender.System, "Disconnected!");
                            CommunicatorClient.Instance.OnReceivedMessage -= waitForResponse;
                            return 1;
                        }
                        else
                        {
                            ConsoleController.Instance.WriteConsoleMessage(ConsoleController.eSender.System, "Timeout!");
                            CommunicatorClient.Instance.OnReceivedMessage -= waitForResponse;
                            return 2;
                        }
                    }
                }
                CommunicatorClient.Instance.OnReceivedMessage -= waitForResponse;
            }
            return 0;
        }

        int GenerateGameFromGameDesc(string responseMessage)
        {
            if (!_GameDescForm.gameObject.activeSelf)
            {
                CollectGameProjectInformation(_responses[3]);
                return 2;
            }

            var gameDesc = _GameDescForm.GetGameDesc();
            var gameDescJson = _GameDescForm.GetGameDescJson();
            GenerateGameAsync(gameDesc, gameDescJson);
            return 0;
        }

        async Task<string> PromptToInstruct(string prompt)
        {
            int errorCode = 0;
            string[] instructions = new string[1]; //= await Interpreter.Instance.InterpretPrompt(prompt);

            if (instructions == null || instructions.Length == 0)
            {
                return null;
            }

            for (int i = 0; i < instructions.Length; ++i)
            {
                errorCode = await SendInstruction(instructions[i]);
                if (errorCode > 0)
                {
                    break;
                }
            }

            if (errorCode > 0)
            {
                ConsoleController.Instance.WriteConsoleMessage(ConsoleController.eSender.Assistant,
                    $"Prompt execution was interrupted. ErrorCode: {errorCode}");
                return "Failed!";
            }
            else
            {
                return "Done!";
            }
        }
    }
}