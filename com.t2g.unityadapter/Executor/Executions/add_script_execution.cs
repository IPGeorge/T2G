#if UNITY_EDITOR

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace T2G.Executor
{
    [Execution("add_script")]
    public class add_script_execution : Execution
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
            string objName = jsonObj["object"];
            string scriptPath = jsonObj["path"];

            if(string.IsNullOrEmpty(scriptPath) || !File.Exists(scriptPath))
            {
                return (eExecutionResult.Failed, "Invalid target script path!");
            }

            string scriptPathFile = Path.GetFileName(scriptPath);
            string scriptName = Path.GetFileNameWithoutExtension(scriptPathFile);

            if (string.IsNullOrEmpty(objName))
            {
                objName = scriptName;
            }

            Executor.SetResponseForInitializeOnLoad($"Script {scriptName} was added.", $"Failed to add script {scriptName}!");
            EditorPrefs.SetString("AddScript_ObjName", objName);
            EditorPrefs.SetString("AddScript_ScirptName", scriptName);

            var importResult = ImportCustomerScript(objName, scriptPath);
            if (importResult.succeeded)
            {
                if (importResult.isIdentical)
                {
                    Executor.ClearResponseForInitializeOnLoad();
                    EditorPrefs.SetString("AddScript_ObjName", string.Empty);
                    EditorPrefs.SetString("AddScript_ScirptName", string.Empty);
                    Type scriptType = Executor.GetClassTypeByName(scriptName);
                    if (scriptType != null)
                    {
                        AddScriptToObject(objName, scriptType);
                        return (eExecutionResult.Succeeded, $"Script {scriptName} was added.");
                    }
                }
                else
                {

                    return (eExecutionResult.Void, string.Empty);
                    //This has a potential bug here when scriptName doesn't match the class name
                    //In that case, the existing script is imported again, the InitialOnLoadMEthod is not called
                    //Should avoid it later. 
                    //The sciptName must match the class name case sensitively
                }
            }

            Executor.ClearResponseForInitializeOnLoad();
            EditorPrefs.SetString("AddScript_ObjName", string.Empty);
            EditorPrefs.SetString("AddScript_ScirptName", string.Empty);
            return (eExecutionResult.Failed, $"Invalid script file path or class name. Failed to add script {scriptName}! ");
        }

        (bool succeeded, bool isIdentical) ImportCustomerScript(string objName, string sourceScriptPath)
        {
            //Simply copy the script into the project for now
            var scriptFileName = Path.GetFileName(sourceScriptPath);
            var scriptName = Path.GetFileNameWithoutExtension(scriptFileName);
            var targetPath = Path.Combine(Application.dataPath, "Scripts");
            var targetScriptFilePath = Path.Combine(targetPath, scriptFileName);
            bool isIdentical = false;

            if(!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            if (File.Exists(sourceScriptPath))
            {
                if (File.Exists(targetScriptFilePath))
                {
                    isIdentical = CompareTwoFiles(sourceScriptPath, targetScriptFilePath);
                }

                File.Copy(sourceScriptPath, targetScriptFilePath, true);
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                return (true, isIdentical);
            }
            return (false, false);
        }

        public static bool CompareTwoFiles(string filePath1, string filePath2)
        {
            // Quick check: compare file sizes first
            FileInfo file1 = new FileInfo(filePath1);
            FileInfo file2 = new FileInfo(filePath2);
            if (file1.Length != file2.Length)
                return false;

            // Compare contents line by line
            using (StreamReader reader1 = new StreamReader(filePath1))
            using (StreamReader reader2 = new StreamReader(filePath2))
            {
                string? line1, line2;
                while ((line1 = reader1.ReadLine()) != null &&
                       (line2 = reader2.ReadLine()) != null)
                {
                    if (line1 != line2)
                        return false;
                }

                // Check if one file has more lines
                return reader1.ReadLine() == null && reader2.ReadLine() == null;
            }
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
