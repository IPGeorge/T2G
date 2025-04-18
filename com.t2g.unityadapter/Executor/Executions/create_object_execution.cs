#if UNITY_EDITOR

using SimpleJSON;
using System;
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
                                        //Relative path to the Resource Path (source)
                                        //The game project data path "/Assets". (target)
                                        //example "/Prefabs/Primitives/cube.prefab"

            Debug.LogError($"Create object 3: Name={name}, assets={instruction.ResolvedAssetPaths}");

            //Hookup asset updated callback or Oninitializeload
            for (int i = 0; i < assetPaths.Length; ++i)
            {
                //check file existing

                //copy if it doesn't exist
            }

            return (true, $"{name} was created!");
        }

        [InitializeOnLoadMethod]
        void CreateTheObject()
        {
            //GameObject.Instantiate<GameObject>();
        }
    }
}

#endif