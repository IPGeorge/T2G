using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace T2G
{

    public class GameGenerationManager
    {
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

        public async void StartGeneratingGameFromGameDesc(string gameDescText)
        {
            GameDescLite gameDesc = JsonUtility.FromJson<GameDescLite>(gameDescText);

            List<string> inputs = new List<string>();
            var fullProjectPath = gameDesc.GetFullProjectPath();
            inputs.Add($"create a new project {fullProjectPath}");
            inputs.Add($"initialize project {fullProjectPath}");
            inputs.Add($"Open project {fullProjectPath}");

            for (int i = 0; i < inputs.Count; ++i)
            {
                Debug.LogError($"Execute command: {inputs[i]}");
                await ConsoleController.Instance.InputEndsProcess(inputs[i]);
            }
        }

    }
}