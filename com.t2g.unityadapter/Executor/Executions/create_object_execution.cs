#if UNITY_EDITOR

using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace T2G.Executor
{
    [Execution("create_object")]
    public class create_object_execution : Execution
    {
        public static object EditorManager { get; private set; }

        public async override Awaitable<(bool succeeded, string message)> Execute(Instruction instruction)
        {
            if (!ValidateInstructionKeyword(instruction.Keyword))
            {
                return (false, "Invalid instruction keyword! 'create_object' was expected.");
            }

            if (instruction.DataType != Instruction.EDataType.JsonData ||
                instruction.State != Instruction.EInstructionState.Resolved)
            {
                return (false, "Invalid instruction data!");
            }
            
            JSONObject jsonObjData = JSON.Parse(instruction.Data).AsObject;
            string objName = jsonObjData["name"];
            string[] assetPaths = instruction.ResolvedAssetPaths.Split(',');
                //Source: Relative path to the Resource Path; Target: The game project path "/Assets"            
                //example "/Prefabs/Primitives/cube.prefab"
            string targetAssetPath;
            List<string> assetsToImport = new List<string>();
            string prefabToInstantiate = string.Empty;
            int i;
            for (i = 0; i < assetPaths.Length; ++i)
            {
                string extension = Path.GetExtension(assetPaths[i]).ToLower();
                if (string.Compare(extension, ".prefab") == 0)
                {
                    prefabToInstantiate = assetPaths[i];
                }
                targetAssetPath = Path.Combine(Application.dataPath, assetPaths[i]);
                if(File.Exists(targetAssetPath))
                {
                    continue;
                }
                assetsToImport.Add(assetPaths[i]);
            }

            PoolAssetsToImport(assetsToImport, prefabToInstantiate, objName);
            Executor.SetResponseForInitializeOnLoad($"{objName} was created.", $"Failed to create {objName}!");
            await ImportPooledAssets();
            bool succeeded = await InstantiatePooledPrefab(objName, prefabToInstantiate);
            var result = (true, Executor.GetSucceededResponseMessage());
            Executor.ClearResponseForInitializeOnLoad();    
            return result;
        }


        /* function CreatePooledObject
         * When reload happens, continue the improting and initiating task.         
         * */
        [InitializeOnLoadMethod]
        async static Awaitable CreatePooledObject()  
        {
            await ImportPooledAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            bool created = await InstantiatePooledPrefab();
            if (created)
            {
                Executor.SendExecutionResponse(true); //Send execution response 
                Executor.SaveActiveScene();
            }
            else
            {
                Executor.SendExecutionResponse(false);
            }
            Executor.ClearResponseForInitializeOnLoad();
        }
    }
}

#endif