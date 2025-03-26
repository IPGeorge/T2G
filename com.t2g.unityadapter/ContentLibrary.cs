
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

namespace T2G
{
    public class ContentLibrary
    {

#region  Find assets =====================================================================================
        public static bool ResolveInstruction(Instruction instruction)
        {
            if(instruction.State == Instruction.EInstructionState.Empty || 
                instruction.State == Instruction.EInstructionState.Resolved)
            {
                return true;
            }

            ResolveAssets(instruction);

            return true;
        }

        public async static void ResolveAssets(Instruction instruction)
        {
            var result = await SearchAssets(instruction.Data);
            JSONObject jsonObj = JSON.Parse(result).AsObject;
            JSONArray resultsArray = jsonObj["results"].AsArray;
            if(resultsArray.Count > 0)
            {
                instruction.State = Instruction.EInstructionState.Resolved;
            }
            else
            {
                instruction.State = Instruction.EInstructionState.MissingResource;
                result = await SearchAssets("default");
                jsonObj = JSON.Parse(result).AsObject;
                resultsArray = jsonObj["results"].AsArray;
            }
            instruction.ResolvedData = resultsArray[0];  //Simply use the first found. Ramdomly pick is possible. 
        }

        public async static Awaitable<string> SearchAssets(string objInfo, string assetType = "")
        {
            JSONObject jsonObj = JSON.Parse(objInfo).AsObject;
            string objName = jsonObj["name"];
            string tokens = jsonObj["type"];
            string url = $"http://localhost:5000/search?q={tokens}&type={assetType}";
            string assetPaths = string.Empty;
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                await request.SendWebRequest();
                assetPaths = request.downloadHandler.text;
            }
            return  assetPaths;
        }

#endregion  Find assets ==================================================================================

#if UNITY_EDITOR
        //Importing assets ===============================================================================

        public static int ImportScript(string scriptName, string dependencies)
            //0-imported
            //1-already exists, no import is needed
            //-1-failed
        {
            int retVal = -1;
            string scriptsFolderName = "Scripts";
            var sourcePath = Path.Combine(SettingsT2G.RecoursePath, scriptsFolderName, scriptName);
            var destDir = Path.Combine(Application.dataPath, scriptsFolderName);
            var destPath = Path.Combine(destDir, scriptName);
            if (File.Exists(destPath))
            {
                retVal = 1;
            }
            else if (File.Exists(sourcePath))
            {
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }
                File.Copy(sourcePath, destPath);
                var importPath = Path.Combine("Assets", scriptsFolderName, scriptName);
                var dependencyArray = dependencies.Split(',');
                foreach (var dependency in dependencyArray)
                {
                    sourcePath = Path.Combine(SettingsT2G.RecoursePath, scriptsFolderName, dependency);
                    destPath = Path.Combine(Application.dataPath, scriptsFolderName, dependency);
                    if (!File.Exists(destPath) && File.Exists(sourcePath))
                    {
                        File.Copy(sourcePath, destPath);
                    }
                }
                AssetDatabase.Refresh();
                retVal = 0;
            }

            return retVal;
        }

        public static int ImportPrefab(string prefabName, AssetDatabase.ImportPackageCallback CompletedHanddler)
        {
            if (!SettingsT2G.Loaded)
            {
                SettingsT2G.Load();
            }
            string prefabsFolderName = "Prefabs";
            string packagePath = Path.Combine(SettingsT2G.RecoursePath, prefabsFolderName, prefabName, $"{prefabName}.unitypackage");
            string assetPrefabPathFolder = Path.Combine(Application.dataPath, prefabsFolderName);
            string assetPrefabPath = Path.Combine(assetPrefabPathFolder, prefabName, $"{prefabName}.prefab");
            if(File.Exists(assetPrefabPath))
            {
                return 1;
            }
            else if (File.Exists(packagePath))
            {
                if (!Directory.Exists(assetPrefabPathFolder))
                {
                    Directory.CreateDirectory(assetPrefabPathFolder);
                }
                AssetDatabase.importPackageCompleted += CompletedHanddler;
                AssetDatabase.ImportPackage(packagePath, false);
                AssetDatabase.Refresh();
                return 0;
            }
            else
            {
                return -1;
            }
        }

        public static bool ImportPackage(string packageName, AssetDatabase.ImportPackageCallback CompletedHanddler)
        {
            if (!SettingsT2G.Loaded)
            {
                SettingsT2G.Load();
            }
            string packagePath = Path.Combine(SettingsT2G.RecoursePath, "Packages", packageName, $"{packageName}.unitypackage");
            if (File.Exists(packagePath))
            {
                AssetDatabase.importPackageCompleted += CompletedHanddler;
                AssetDatabase.ImportPackage(packagePath, false);
                AssetDatabase.Refresh();
                return true;
            }
            else
            {
                return false;
            }
        }
#endif
    }
}
