#if UNITY_EDITOR

using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace T2G.Executor
{
    [Execution("add_script")]
    public class add_script_execution : Execution
    {
        public async override Awaitable<(bool succeeded, string message)> Execute(Instruction instruction)
        {
            if (!ValidateInstructionKeyword(instruction.Keyword))
            {
                return (false, "Invalid instruction keyword! 'add_script' was expected.");
            }

            if (instruction.DataType != Instruction.EDataType.JsonData)
            {
                return (false, "Invalid instruction data!");
            }

            var jsonObj = GetInstructionJsonData(instruction);
            string objName = jsonObj["object"];
            string scriptPath = jsonObj["path"];

            if(string.IsNullOrEmpty(scriptPath) || !File.Exists(scriptPath))
            {
                return (false, "Invalid target script path!");
            }

            string scriptPathFile = Path.GetFileName(scriptPath);
            string scriptName = Path.GetFileNameWithoutExtension(scriptPathFile);

            if (string.IsNullOrEmpty(objName))
            {
                objName = scriptName;
            }

            Type scriptType = Executor.GetClassTypeByName(scriptName);

            if (scriptType == null)
            {
                Executor.SetResponseForInitializeOnLoad($"Script {scriptName} was added.", $"Failed to add script {scriptName}!");
                ImportCustomerScript(objName, scriptPath);
                await Task.Yield();
            }
           
            AddScriptToObject(objName, scriptType);
            Executor.ClearResponseForInitializeOnLoad();
            return (true, $"{scriptName} was added.");
        }

        void ImportCustomerScript(string objName, string sourceScriptPath)
        {
            //Simply copy the script into the project for now
            var scriptFileName = Path.GetFileName(sourceScriptPath);
            var scriptName = Path.GetFileNameWithoutExtension(scriptFileName);
            var targetPath = Path.Combine(Application.dataPath, "Scripts");
            var targetscriptFilePath = Path.Combine(targetPath, scriptFileName);

            EditorPrefs.SetString("AddScript_ObjName", objName);
            EditorPrefs.SetString("AddScript_ScirptName", scriptName);

            if(!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
            File.Copy(sourceScriptPath, targetscriptFilePath, true);
            AssetDatabase.Refresh();
        }

        static void AddScriptToObject(string objName, Type scriptType)
        {
            var targetObj = GameObject.Find(objName);
            if (targetObj == null)
            {
                Debug.LogWarning("TargetObject not found, create it.");
                targetObj = new GameObject(objName);
            }
            if (scriptType != null)
            {
                targetObj.AddComponent(scriptType);
            }
        }

        [InitializeOnLoadMethod]
        static void ContinueAddingScriptToObject()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            var objName = EditorPrefs.GetString("AddScript_ObjName", string.Empty);
            var scriptName = EditorPrefs.GetString("AddScript_ScirptName", string.Empty);

            if (string.IsNullOrEmpty(objName) || string.IsNullOrEmpty(scriptName))
            {
                Executor.SendExecutionResponse(false);
            }
            else
            {
                Type scriptType = Executor.GetClassTypeByName(scriptName);
                if (scriptType == null)
                {
                    Executor.SendExecutionResponse(false);
                }
                else
                {
                    AddScriptToObject(objName, scriptType);
                    Executor.SendExecutionResponse(true);
                }
            }
        }
    }
}

#endif
