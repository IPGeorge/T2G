using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace T2G.UnityAdapter
{
    public partial class Executor
    {
        HashSet<Instruction> _instructionBuffer = new HashSet<Instruction>();

        public void PostponeInstruction(Instruction instruction)
        {
            if (!_instructionBuffer.Contains(instruction))
            {
                _instructionBuffer.Add(instruction);
            }
        }
    }

    [Execution("CREATE_WORLD")]
    public class ExecutionCreateWorld : ExecutionBase
    {
        public override void HandleExecution(Executor.Instruction instruction)
        {
            Action<string, List<string>> setupWorld = (sceneFile, args) =>
            {
                for (int i = 1; i < args.Count - 1; i += 2)
                {
                    if (args[i].CompareTo("-GRAVITY") == 0 && float.TryParse(args[i + 1], out var gravity))
                    {
                        Physics.gravity = Vector3.up * gravity;
                    }
                    if (args[i].CompareTo("-BOOTSTRAP") == 0)
                    {
                        int startIndex = sceneFile.IndexOf("Assets");
                        if (startIndex > 0)
                        {
                            sceneFile = sceneFile.Substring(startIndex);
                        }
                        var sceneList = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
                        if (args[i + 1].ToLower().CompareTo("true") == 0)
                        {
                            sceneList.Insert(0, new EditorBuildSettingsScene(sceneFile, true));
                        }
                        else
                        {
                            sceneList.Add(new EditorBuildSettingsScene(sceneFile, true));
                        }
                        EditorBuildSettings.scenes = sceneList.ToArray();

                        var buildProfile = BuildProfile.GetActiveBuildProfile();
                        if (buildProfile != null)
                        {
                            var profileSceneList = new List<EditorBuildSettingsScene>(buildProfile.scenes);
                            if (args[i + 1].ToLower().CompareTo("true") == 0)
                            {
                                profileSceneList.Insert(0, new EditorBuildSettingsScene(sceneFile, true));
                            }
                            else
                            {
                                profileSceneList.Add(new EditorBuildSettingsScene(sceneFile, true));
                            }
                            buildProfile.scenes = profileSceneList.ToArray();
                        }
                    }
                }
            };

            var activeScene = EditorSceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(activeScene.name))
            {
                EditorSceneManager.SaveScene(activeScene);
            }

            string scenesPath = Path.Combine(Application.dataPath, "Scenes");
            if (!Directory.Exists(scenesPath))
            {
                Directory.CreateDirectory(scenesPath);
            }

            string sceneFile = Path.Combine(scenesPath, instruction.Arguments[0] + ".unity");
            if (File.Exists(sceneFile))
            {
                EditorSceneManager.sceneOpened += (scene, mode) =>
                {
                    setupWorld(sceneFile, instruction.Arguments);
                    Executor.RespondCompletion(true);
                };
                EditorSceneManager.OpenScene(sceneFile, OpenSceneMode.Single);
            }
            else
            {
                EditorSceneManager.newSceneCreated += (scene, setup, mode) =>
                {
                    bool succeeded = EditorSceneManager.SaveScene(scene, sceneFile);
                    setupWorld(sceneFile, instruction.Arguments);
                    Executor.RespondCompletion(succeeded);
                };
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
            s_currentObject = null;
        }
    }

    [Execution("CREATE_OBJECT")]
    public class ExecutionCreateObject : ExecutionBase
    {
        [InitializeOnLoadMethod]
        static async void ProcessPendingInsrtuction()
        {
            var prefabName = EditorPrefs.GetString(Defs.k_Pending_NewPrefabObject, null);
            if (string.IsNullOrEmpty(prefabName))
            {
                return;
            }
            EditorPrefs.SetString(Defs.k_Pending_NewPrefabObject, string.Empty);

            //Wait for re-connection from the client
            float timer = 300.0f;  //Wait for 5 minutes 
            while (!CommunicatorServer.Instance.IsConnected)
            {
                await Task.Delay(100);
                timer -= 0.1f;
                if (timer <= 0.0f)
                {
                    Debug.LogError("[AddScipt.ProcessPendingInsrtuction] Timeout for client re-connection.");
                    return;
                }
            }

            //Find the actual path
            string prefabsDir = Path.Combine(Application.dataPath, "Prefabs");
            var files = Directory.GetFiles(prefabsDir, "*.prefab", SearchOption.AllDirectories);
            string prefabPath = string.Empty;
            foreach (var file in files)
            {
                string fn = Path.GetFileName(file);
                string prefabFile = prefabName + ".prefab";
                if (string.Compare(fn, prefabFile) == 0)
                {
                    int idx = file.IndexOf("Assets");
                    if (idx >= 0)
                    {
                        prefabPath = file.Substring(idx);
                    }
                    break;
                }
            }
            if (string.IsNullOrEmpty(prefabPath))
            {
                Executor.RespondCompletion(false, $"Missing prefab '{prefabName}' to create the object!");
            }
            else
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab != null)
                {
                    var args = EditorPrefs.GetString(Defs.k_Pending_Arguments, null);
                    string[] argsArr = args.Split(", ");
                    List<string> argList = new List<string>(argsArr);
                    GameObject newObj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);
                    newObj.name = Executor.GetPropertyValue("-OBJECT", ref argList);
                    var position = Executor.GetPropertyValue("-POSITION", ref argList);
                    var rotation = Executor.GetPropertyValue("-ROTATION", ref argList);
                    var scale = Executor.GetPropertyValue("-SCALE", ref argList);

                    if (!string.IsNullOrEmpty(position))
                    {
                        float[] float3 = Executor.ParseFloat3(position);
                        newObj.transform.position = new Vector3(float3[0], float3[1], float3[2]);
                    }
                    if (!string.IsNullOrEmpty(rotation))
                    {
                        float[] float3 = Executor.ParseFloat3(rotation);
                        Quaternion q = Quaternion.Euler(float3[0], float3[1], float3[2]);
                        newObj.transform.rotation = q;
                    }
                    if (!string.IsNullOrEmpty(scale))
                    {
                        float[] float3 = Executor.ParseFloat3(scale);
                        newObj.transform.localScale = new Vector3(float3[0], float3[1], float3[2]);
                    }

                    var controller = EditorPrefs.GetString(Defs.k_Pending_Controller, null);
                    if (!string.IsNullOrEmpty(controller))
                    {
                        var setValues = EditorPrefs.GetString(Defs.k_Pending_ControllerSetValues, null);
                        Executor.ParseAndSetValues(newObj, controller, setValues);
                    }

                    newObj.SetActive(true);
                    s_currentObject = newObj;
                    Executor.RespondCompletion(true, $"{prefabName} object was created!");
                }
                else
                {
                    Executor.RespondCompletion(false, $"Missing prefab '{prefabName}' to create the object!");
                }
            }
        }

        public override void HandleExecution(Executor.Instruction instruction)
        {
            var args = instruction.Arguments;
            string objName = args[0].Trim('"');
            Vector3 pos = Vector3.zero, rot = Vector3.zero, scale = Vector3.one;
            s_currentObject = null;

            string prefabName = Executor.GetPropertyValue("-PREFAB", ref args, false, 1);
            if (string.IsNullOrEmpty(prefabName))
            {
                for (int i = 1; i < instruction.Arguments.Count; i += 2)
                {
                    if (args[i].CompareTo("-WORLD") == 0)
                    {
                        string worldName = args[i + 1].Trim('"');
                        if (EditorSceneManager.GetActiveScene().name.CompareTo(worldName) != 0)
                        {
                            string worldPathFile = Path.Combine(Application.dataPath, worldName + ".unity");
                            if (File.Exists(worldPathFile))
                            {
                                EditorSceneManager.OpenScene(worldPathFile);
                            }
                            else
                            {
                                Executor.RespondCompletion(false, $"World {worldName} doesn't exist!");
                                return;
                            }
                        }
                    }
                    if (args[i].CompareTo("-POSITION") == 0)
                    {
                        float[] fValues = Executor.ParseFloat3(args[i + 1]);
                        pos = new Vector3(fValues[0], fValues[1], fValues[2]);
                    }
                    if (args[i].CompareTo("-ROTATION") == 0)
                    {
                        float[] fValues = Executor.ParseFloat3(args[i + 1]);
                        rot = new Vector3(fValues[0], fValues[1], fValues[2]);
                    }
                    if (args[i].CompareTo("-SCALE") == 0)
                    {
                        float[] fValues = Executor.ParseFloat3(args[i + 1]);
                        scale = new Vector3(fValues[0], fValues[1], fValues[2]);
                    }
                }

                s_currentObject = new GameObject(objName);
                s_currentObject.transform.position = pos;
                s_currentObject.transform.Rotate(rot);
                s_currentObject.transform.localScale = scale;

                Executor.RespondCompletion(true);
            }
            else
            {
                List<string> argList = new List<string>(instruction.Arguments);
                argList.Insert(0, "-OBJECT");
                string argsString = argList[0];
                for (int i = 1; i < argList.Count; ++i)
                {
                    argsString += $", {argList[i]}";
                }
                EditorPrefs.SetString(Defs.k_Pending_NewPrefabObject, prefabName);
                EditorPrefs.SetString(Defs.k_Pending_Arguments, argsString);
                ContentLibrary.ImportPackage(prefabName, ImportPackageCompletedHanddler);

                string controller = Executor.GetPropertyValue(Defs.k_GameDesc_PrefabControllerKey, ref args);
                if (!string.IsNullOrEmpty(controller))
                {
                    string setValues = Executor.GetPropertyValue(Defs.k_GameDesc_PrefabSetValuesKey, ref args);
                    EditorPrefs.SetString(Defs.k_Pending_Controller, controller);
                    EditorPrefs.SetString(Defs.k_Pending_ControllerSetValues, setValues);
                }
            }
        }

        public static void ImportPackageCompletedHanddler(string packageName)
        {
            ProcessPendingInsrtuction();
        }
    }

    [Execution("ADDON")]
    public class ExecutionADDON : ExecutionBase
    {
        static Dictionary<string, AddAddonBase> s_addonProcessor = new Dictionary<string, AddAddonBase>();

        public static void RegisterAddAddonExecutions()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var executionClasses = assembly.GetTypes()
                .Where(type => type.IsClass && type.GetCustomAttributes(typeof(AddAddonAttribute), false).Any());
            foreach (var executionClass in executionClasses)
            {
                var attribute = executionClass.GetCustomAttribute<AddAddonAttribute>();
                var execution = Activator.CreateInstance(executionClass) as AddAddonBase;
                s_addonProcessor.Add(attribute.AddonType, execution);
            }
        }

        public override void HandleExecution(Executor.Instruction instruction)
        {
            var argList = new List<string>(instruction.Arguments);
            string worldName = Executor.GetPropertyValue("-WORLD", ref argList, false);
            if (!Executor.OpenWorld(worldName))
            {
                Executor.RespondCompletion(false);
                return;
            }
            string addonType = Executor.GetPropertyValue("-TYPE", ref argList, false);
            if (s_addonProcessor.ContainsKey(addonType))
            {
                s_addonProcessor[addonType].AddAddon(s_currentObject, argList);
            }
            else
            {
                Executor.RespondCompletion(false);
            }
        }
    }

    [Execution("IMPORT_PACKAGE")]
    public class ExecutionImportPackage : ExecutionBase
    {
        public override void HandleExecution(Executor.Instruction instruction)
        {
            if(!Settings.Loaded)
            {
                Settings.Load();
            }

            var args = instruction.Arguments;
            var packageName = args[0].Trim('"');
            string packagePath = Path.Combine(Settings.RecoursePath, "Packages", packageName);
            AssetDatabase.importPackageCompleted += ImportPackageCompletedHanddler;
            AssetDatabase.importPackageFailed += ImportPackageFailedHanddler;
            EditorPrefs.SetString(Defs.k_Pending_ImportPackage, packageName);
            AssetDatabase.ImportPackage(packagePath, false);
            AssetDatabase.Refresh();
        }

        [InitializeOnLoadMethod]
        static async void ProcessPendingInsrtuction()
        {
            var packageName = EditorPrefs.GetString(Defs.k_Pending_ImportPackage, null);
            if (string.IsNullOrEmpty(packageName))
            {
                return;
            }
            EditorPrefs.SetString(Defs.k_Pending_ImportPackage, string.Empty);
            
            //Wait for re-connection from the client
            float timer = 300.0f;  //Wait for 5 minutes 
            while (!CommunicatorServer.Instance.IsConnected)
            {
                await Task.Delay(100);
                timer -= 0.1f;
                if (timer <= 0.0f)
                {
                    Debug.LogError("[ImportPackage.ProcessPendingInsrtuction] Timeout for client re-connection.");
                    return;
                }
            }
            Executor.RespondCompletion(true, $"Imported {packageName} package with reset!");
        }

        static void ImportPackageCompletedHanddler(string packageName)
        {
            EditorPrefs.SetString(Defs.k_Pending_ImportPackage, string.Empty);
            Executor.RespondCompletion(true, $"Imported {packageName} package!");
        }

        static void ImportPackageFailedHanddler(string packageName, string errorMessage)
        {
            EditorPrefs.SetString(Defs.k_Pending_ImportPackage, string.Empty);
            Executor.RespondCompletion(false, $"Failed to import {packageName} package!");
        }
    }
}