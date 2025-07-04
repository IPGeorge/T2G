using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using T2G.Communicator;

namespace T2G
{

    public class GameGenerationManager
    {
        Dictionary<string, Action<List<string>, string, string>> PropertyNameParsers = 
            new Dictionary<string, Action<List<string>, string, string>>()
        {
            { "position", (inputs, objName, value) => { if(Utilities.ParseVector3(value, out var _)) inputs.Add($"set {objName} position {value}"); }},
            { "rotation", (inputs, objName, value) => { if(Utilities.ParseVector3(value, out var _)) inputs.Add($"set {objName} rotation {value}"); }},
            { "scale", (inputs, objName, value) => { if(Utilities.ParseVector3(value, out var _)) inputs.Add($"set {objName} scale {value}"); } },
            { "spawnpoint", (inputs, objName, value) => { inputs.Add($"place {objName} at {value}"); } }
        };

        static GameGenerationManager _instance = null;
        public static GameGenerationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameGenerationManager();
                }
                return _instance;
            }
        }


        public enum eResult
        {
            IsUnknown,
            IsRunning,
            Succeeded,
            Failed
        }

        public eResult ExecutionResult = eResult.IsUnknown;

        public async void StartGeneratingGameFromGameDesc(string gameDescText)
        {
            ExecutionResult = eResult.IsRunning;

            GameDescLite gameDesc = JsonUtility.FromJson<GameDescLite>(gameDescText);

            List<string> inputs = new List<string>();
            var fullProjectPath = gameDesc.GetFullProjectPath();
            inputs.Add($"create a new project {fullProjectPath}");
            inputs.Add($"initialize project {fullProjectPath}");
            inputs.Add($"Open project {fullProjectPath}");
            inputs.Add("[WaitForConnection]");
            for (int i = 0; i < gameDesc.Spaces.Length; ++i)
            {
                inputs.Add($"create a new space named {gameDesc.Spaces[i].Name}");
                for(int j = 0; j < gameDesc.Spaces[i].Objects.Length; ++j)
                {
                    string objectName = gameDesc.Spaces[i].Objects[j].Name;
                    if(string.IsNullOrEmpty(objectName))
                    {
                        objectName = gameDesc.Spaces[i].Objects[j].Desc + " - " + DateTime.Now.Ticks.ToString();
                    }
                    inputs.Add($"create a {gameDesc.Spaces[i].Objects[j].Desc} named {objectName}");
                    if (gameDesc.Spaces[i].Objects[j].Properties != null)
                    {
                        for (int k = 0; k < gameDesc.Spaces[i].Objects[j].Properties.Length; ++k)
                        {
                           if(gameDesc.Spaces[i].Objects[j].Properties[k].IndexOf("=") <= 0)
                            {
                                continue;
                            }
                            string[] propertyValue = gameDesc.Spaces[i].Objects[j].Properties[k].Split("=");
                            string key = propertyValue[0].ToLower();
                            if (PropertyNameParsers.ContainsKey(key) && !string.IsNullOrWhiteSpace(objectName))
                            {
                                PropertyNameParsers[key]?.Invoke(inputs, objectName, propertyValue[1]);
                            }
                        }
                    }
                    if(gameDesc.Spaces[i].Objects[j].SetValues != null)
                    {
                        for(int k = 0; k < gameDesc.Spaces[i].Objects[j].SetValues.Length; ++k)
                        {
                            string fieldName = gameDesc.Spaces[i].Objects[j].SetValues[k].Field;
                            string fieldValue = gameDesc.Spaces[i].Objects[j].SetValues[k].Values;
                            inputs.Add($"set {objectName} property {fieldName} to {fieldValue}");
                        }
                    }
                }
            }

            inputs.Add("save");

            for (int i = 0; i < inputs.Count; ++i)
            {
                if (inputs[i] == "[WaitForConnection]")
                {
                    bool connected = await CommunicatorClient.Instance.WaitForConnection(180.0f);
                    if(!connected)
                    {
                        ExecutionResult = eResult.Failed;
                        ConsoleController.Instance.WriteConsoleMessage(ConsoleController.eSender.Assistant, $"Game generation teminated!");
                        return;
                    }
                }
                else
                {
                    await ConsoleController.Instance.InputEndsProcess(inputs[i]);
                }
            }

            ConsoleController.Instance.WriteConsoleMessage(ConsoleController.eSender.Assistant, $"Game generation completed!");
            ExecutionResult = eResult.Succeeded;
        }
    }
}