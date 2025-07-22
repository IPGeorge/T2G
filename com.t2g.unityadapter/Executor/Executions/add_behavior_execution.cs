#if UNITY_EDITOR

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace T2G.Executor
{
    [Execution("add_behvior")]
    public class add_behavior_execution : Execution
    {
        public override (eExecutionResult, string) Execute(Instruction instruction)
        {
            if (!ValidateInstructionKeyword(instruction.Keyword))
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

            return (eExecutionResult.Succeeded, $"{behaviorName} to be continued!");


            //if (string.IsNullOrEmpty(scriptPath) || !File.Exists(scriptPath))
            //{
            //    return (eExecutionResult.Failed, "Invalid target script path!");
            //}

            //string scriptPathFile = Path.GetFileName(scriptPath);
            //string scriptName = Path.GetFileNameWithoutExtension(scriptPathFile);

            //if (string.IsNullOrEmpty(objName))
            //{
            //    objName = scriptName;
            //}

            //Type scriptType = Executor.GetClassTypeByName(scriptName);

            //if (scriptType == null)
            //{
            //    Executor.SetResponseForInitializeOnLoad($"Script {scriptName} was added.", $"Failed to add script {scriptName}!");
            //    EditorPrefs.SetString("AddScript_ObjName", objName);
            //    EditorPrefs.SetString("AddScript_ScirptName", scriptName);

            //    if (ImportCustomerScript(objName, scriptPath))
            //    {
            //        return (eExecutionResult.Void, string.Empty);
            //        //This has a potential bug here when scriptName doesn't match the class name
            //        //In that case, the existing script is imported again, the InitialOnLoadMEthod is not called
            //        //Should avoid it later. 
            //        //The sciptName must match the class name case sensitively
            //    }
            //    else
            //    {
            //        Executor.ClearResponseForInitializeOnLoad();
            //        EditorPrefs.SetString("AddScript_ObjName", string.Empty);
            //        EditorPrefs.SetString("AddScript_ScirptName", string.Empty);
            //        return (eExecutionResult.Failed, $"Invalid script file path or class name. Failed to add script {scriptName}! ");
            //    }
            //}
            //else
            //{
            //    AddScriptToObject(objName, scriptType);
            //    Executor.ClearResponseForInitializeOnLoad();
            //    return (eExecutionResult.Succeeded, $"{scriptName} was added.");
            //}
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