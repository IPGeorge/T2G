using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CmdInitProject : Command
{
    public static readonly string CommandKey = "InitProject";
    static readonly string k_T2g_UnityAdapter = "com.t2g.unityadapter";
    static readonly string k_unity_ugui = "com.unity.ugui";
    static readonly string k_unity_ugui_version = "2.0.0";

    private string _projectPathName;

    public class Dependencies
    {
        [JsonProperty("dependencies")]
        public Dictionary<string, string> DependencyMap { get; set; }
    }

    public override bool Execute(params string[] args)
    {
        bool result = false;
        if (!PlayerPrefs.HasKey(Defs.k_UnityEditorPath))
        {
            OnExecutionCompleted?.Invoke(false, ConsoleController.eSender.Error, "Unity Editor path is not set!");
            return result;
        }
        string unityEditorPath = PlayerPrefs.GetString(Defs.k_UnityEditorPath);

        if (args.Length < 1)
        {
            string defaultPath = ConsoleController.Instance.ProjectPathName;
            int startIdx = defaultPath.IndexOf("[") + 1;
            defaultPath = defaultPath.Substring(startIdx, defaultPath.IndexOf("]") - startIdx);

            if (string.IsNullOrWhiteSpace(defaultPath))
            {
                OnExecutionCompleted?.Invoke(false, ConsoleController.eSender.Error, "The project's path argument is missing!");
                return result;
            }
            else
            {
                _projectPathName = defaultPath;
            }
        }
        else
        {
            _projectPathName = ConsoleController.Instance.ProjectPathName = args[0];
        }

        if (!Directory.Exists(_projectPathName))
        {
            OnExecutionCompleted?.Invoke(false, ConsoleController.eSender.Error, $"Project was not found.");
            return result;
        }

        string manifestFilePath = Path.Combine(_projectPathName, "Packages", "manifest.json");
        if (File.Exists(manifestFilePath))
        {
            string json = File.ReadAllText(manifestFilePath);
            Dependencies dependencies = JsonConvert.DeserializeObject<Dependencies>(json);

            if (PlayerPrefs.HasKey(Defs.k_ResourcePath))
            {
                string packagePath =  PlayerPrefs.GetString(Defs.k_ResourcePath);
                packagePath = "file:" + Path.Combine(packagePath, k_T2g_UnityAdapter);
                if (!dependencies.DependencyMap.ContainsKey(k_T2g_UnityAdapter))
                {
                    dependencies.DependencyMap.Add(k_T2g_UnityAdapter, packagePath);
                }
            }
            else
            {
                OnExecutionCompleted?.Invoke(true, ConsoleController.eSender.Error, $"Failed! T2G.UnityAdapter is missing.");
                return result;
            }

            if (!dependencies.DependencyMap.ContainsKey(k_unity_ugui))
            {
                dependencies.DependencyMap.Add(k_unity_ugui, k_unity_ugui_version);
            }

            json = JsonConvert.SerializeObject(dependencies, Formatting.Indented);
            File.WriteAllText(manifestFilePath, json);
            OnExecutionCompleted?.Invoke(true, ConsoleController.eSender.System, $"Succeeded!");
            result = true;
        }
        else
        {
            OnExecutionCompleted?.Invoke(true, ConsoleController.eSender.Error, $"Failed to open the manifest.json file!");
        }
        return result;
    }

    public override string GetKey()
    {
        return CommandKey.ToLower();
    }

    public override string[] GetArguments()
    {
        string[] args = { _projectPathName };
        return args;
    }
}
