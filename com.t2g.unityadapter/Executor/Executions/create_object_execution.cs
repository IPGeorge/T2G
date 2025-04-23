#if UNITY_EDITOR

using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace T2G.Executor
{
    [Execution("create_object")]
    public class create_object_execution : Execution
    {
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
            string name = jsonObjData["name"];
            string[] assetPaths = instruction.ResolvedAssetPaths.Split(',');
            //Relative path to the Resource Path (source), The game project path "/Assets". (target)
            //example "/Prefabs/Primitives/cube.prefab"

            string targetAssetPath;
            List<string> assetsToImport = new List<string>();
            int i;
            for (i = assetPaths.Length - 1; i >= 0 ; --i)
            {
                targetAssetPath = Path.Combine(Application.dataPath, assetPaths[i]);
                if(File.Exists(targetAssetPath))
                {
                    continue;
                }
                assetsToImport.Add(assetPaths[i]);
            }
            
            PoolAssetsToImport(assetsToImport);
            Executor.SetResponseForInitializeOnLoad();
            await ImportPooledAssets();
            InstantiatePooledPrefabs();
            Executor.ClearResponseForInitializeOnLoad();
            return (true, "Done!");  //When the above 3 lines passed, send execution response
        }


        /* function CreatePooledObject
         * When reload happens, continue the improting and initiating task.         
         * */
        [InitializeOnLoadMethod]
        async static Awaitable CreatePooledObject()  
        {
            await ImportPooledAssets();
            InstantiatePooledPrefabs();
            Executor.SendExecutionResponse(); //Send execution response 
        }
    }
}

#endif