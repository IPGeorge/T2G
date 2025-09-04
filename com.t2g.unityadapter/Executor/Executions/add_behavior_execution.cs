#if UNITY_EDITOR

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace T2G.Executor
{
    [Execution("add_behavior")]
    public class add_behavior_execution : Execution
    {
        public override (eExecutionResult, string) Execute(Instruction instruction)
        {
            if (!ValidateInstructionKeyword(instruction.Action))
            {
                return (eExecutionResult.Failed, "Invalid instruction keyword! 'add_script' was expected.");
            }

            if (instruction.DataType != Instruction.EDataType.JsonData)
            {
                return (eExecutionResult.Failed, "Invalid instruction data!");
            }

            var jsonObj = GetInstructionJsonData(instruction);
            string objectName = jsonObj["objectName"];
            string behaviorName = jsonObj["behaviorName"];
            string script = instruction.Assets;

            if(string.IsNullOrWhiteSpace(objectName) || 
                string.IsNullOrWhiteSpace(behaviorName) || 
                string.IsNullOrWhiteSpace(script))
            {
                return (eExecutionResult.Failed, "Object name, behavior name, or script is missing!");
            }

            string scriptFileName = behaviorName + ".cs";
            string scriptDirectoryPath = Path.Combine(Application.dataPath, "Scripts");
            string tmpScriptFilePath = Path.Combine(Application.persistentDataPath, scriptFileName);

            if(!Directory.Exists(scriptDirectoryPath))
            {
                Directory.CreateDirectory(scriptDirectoryPath);
            }

            Type scriptType = Executor.GetClassTypeByName(behaviorName);

            if (scriptType == null)
            {
                File.WriteAllText(tmpScriptFilePath, script);
                Executor.SetResponseForInitializeOnLoad($"Behavior {behaviorName} was added.", $"Failed to add script {behaviorName}!");
                EditorPrefs.SetString("AddScript_ObjName", objectName);
                EditorPrefs.SetString("AddScript_ScirptName", behaviorName);

                if (ImportCustomerScript(objectName, tmpScriptFilePath))
                {
                    return (eExecutionResult.Void, string.Empty);
                    //This has a potential bug here when scriptName doesn't match the class name
                    //In that case, the existing script is imported again, the InitialOnLoadMEthod is not called
                    //Should avoid it later. 
                    //The sciptName must match the class name case sensitively
                }
                else
                {
                    Executor.ClearResponseForInitializeOnLoad();
                    EditorPrefs.SetString("AddScript_ObjName", string.Empty);
                    EditorPrefs.SetString("AddScript_ScirptName", string.Empty);
                    return (eExecutionResult.Failed, $"Invalid script file path or class name. Failed to add script {behaviorName}! ");
                }
            }
            else
            {
                AddScriptToObject(objectName, scriptType);
                Executor.ClearResponseForInitializeOnLoad();
                return (eExecutionResult.Succeeded, $"Behavior {behaviorName} was added.");
            }
        }

        bool ImportCustomerScript(string objName, string sourceScriptPath)
        {
            //Simply copy the script into the project for now
            var scriptFileName = Path.GetFileName(sourceScriptPath);
            var scriptName = Path.GetFileNameWithoutExtension(scriptFileName);
            var targetPath = Path.Combine(Application.dataPath, "Scripts");
            var targetScriptFilePath = Path.Combine(targetPath, scriptFileName);

            if (File.Exists(targetScriptFilePath))       //No overwrite copy is supported for now.
            {
                return false;
            }

            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            if (File.Exists(sourceScriptPath))
            {
                File.Copy(sourceScriptPath, targetScriptFilePath, true);
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                return true;
            }
            return false;
        }

        static void AddScriptToObject(string objName, Type scriptType)
        {
            var targetObj = GameObject.Find(objName);
            if (targetObj == null)
            {
                targetObj = new GameObject(objName);
            }
            if (scriptType != null && targetObj.GetComponent(scriptType) == null)
            {
                targetObj.AddComponent(scriptType);
            }
        }

        [InitializeOnLoadMethod]
        static void InitializeOnloadAddingScriptToObject()
        {
            var objName = EditorPrefs.GetString("AddScript_ObjName", string.Empty);
            var scriptName = EditorPrefs.GetString("AddScript_ScirptName", string.Empty);
            Executor.ClearResponseForInitializeOnLoad();
            EditorPrefs.SetString("AddScript_ObjName", string.Empty);
            EditorPrefs.SetString("AddScript_ScirptName", string.Empty);

            if (string.IsNullOrEmpty(objName) || string.IsNullOrEmpty(scriptName))
            {
                return;     //Do not run.
            }
            else
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

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