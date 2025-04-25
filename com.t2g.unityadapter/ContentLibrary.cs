
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace T2G
{
    public class ContentLibrary
    {

#region  Find assets =====================================================================================
        public static async Awaitable<Instruction> ResolveInstruction(Instruction instruction)
        {
            if (instruction.State == Instruction.EInstructionState.Raw)
            {
                instruction = await ResolveAssets(instruction);
            }
            return instruction;
        }

        public async static Awaitable<Instruction> ResolveAssets(Instruction instruction)
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
            instruction.ResolvedAssetPaths = resultsArray[0];  //Simply use the first found. Ramdomly pick is possible. 
            return instruction;
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
                if (request.result == UnityWebRequest.Result.Success)
                {
                    assetPaths = request.downloadHandler.text;
                }
            }
            return  assetPaths;  //returns list of asset path (delimitor ',') or empty
        }

#endregion  Find assets ==================================================================================

#if UNITY_EDITOR
        //Importing assets ===============================================================================

        public enum EAssetType
        {
            Script,
            Prefab,
            Package,
            Model,
            Image,
            Audio,
            Video,
            Other
        }

        static Dictionary<string, EAssetType> s_AssetTypeMap = new Dictionary<string, EAssetType>()
        {
            { ".cs", EAssetType.Script },
            { ".prefab", EAssetType.Prefab },
            { ".unitypackage", EAssetType.Package },
            { ".fbx", EAssetType.Model },
            { ".png", EAssetType.Image },
            { ".jpg", EAssetType.Image },
            { ".tga", EAssetType.Image },
            { ".bmp", EAssetType.Image },
            { ".wav", EAssetType.Audio },
            { ".mp3", EAssetType.Audio },
            { ".avi", EAssetType.Video },
            { ".mp4", EAssetType.Video }
        };
        
        public static EAssetType GetAssetType(string assetPathName)
        {
            string assetExtension = Path.GetExtension(assetPathName);
            if(s_AssetTypeMap.ContainsKey(assetExtension))
            {
                return s_AssetTypeMap[assetExtension];
            }
            return EAssetType.Other;
        }

        public static async Awaitable<bool> ImportAsset(string assetPathName) 
        {
            if (!SettingsT2G.Loaded)
            {
                SettingsT2G.Load();
            }

            string sourceAssetPath = Path.Combine(SettingsT2G.RecoursePath, assetPathName);
            string targetAssetPath = Path.Combine(Application.dataPath, assetPathName);
            string targetDirectory = Path.GetDirectoryName(targetAssetPath);
            string targetAssetProjectPath = Path.Combine("Assets", assetPathName);
            if (!File.Exists(sourceAssetPath))
            {
                return false;
            }

            try
            {
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                var assetType = GetAssetType(assetPathName);
                if (assetType == EAssetType.Package)
                {
                    return await ImportPackage(assetPathName);
                }
                else
                {
                    File.Copy(sourceAssetPath, targetAssetPath);
                    AssetDatabase.ImportAsset(targetAssetProjectPath);
                }

                return true;
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        public static async Awaitable<bool> ImportPackage(string packagePathName)
        {
            if (!SettingsT2G.Loaded)
            {
                SettingsT2G.Load();
            }
            string packagePath = Path.Combine(SettingsT2G.RecoursePath, packagePathName);
            if (File.Exists(packagePath))
            {
                bool importing = true;
                AssetDatabase.ImportPackageCallback competeCallback = null;
                competeCallback = (handler) =>
                {
                    importing = false;
                    AssetDatabase.importPackageCompleted -= competeCallback;
                    AssetDatabase.Refresh();
                };
                AssetDatabase.importPackageCompleted += competeCallback;
                AssetDatabase.ImportPackage(packagePath, false);
                while(importing)
                {
                    await Task.Yield();
                }
                return true;
            }
            return false;
        }
#endif
    }
}
