using SimpleJSON;
using System;
using System.Threading.Tasks;
using T2G.Executor;
using UnityEngine;

namespace T2G
{
    [Execution("create_object ")]
    public class create_object_execution : Execution
    {
        public async override Awaitable<(bool succeeded, string message)> Execute(Instruction instruction)
        {
            if(instruction.DataType != Instruction.EDataType.JsonData)
            {
                return (false, "Invalid instruction data!");
            }

            JSONObject jsonObjData = JSON.Parse(instruction.Data).AsObject;
            JSONObject jsonObjAssets = JSON.Parse(instruction.ResolvedData).AsObject;
            string name = jsonObjData["name"];
            string assetPaths = jsonObjAssets["results"]; //Relative path to
                                                          //The Resource Path (source)
                                                          //The game project data path "\Assets". (target)
                                                          //example "/Prefabs/Primitives/cube.prefab"
            string[] assetPathsArray = assetPaths.Split(',');

            //Hookup asset updated callback or Oninitializeload
            for(int i = 0; i < assetPathsArray.Length; ++i)
            {
                //check file existing

                //copy if it doesn't exist
            }

            return (true, $"{name} was created!");
        }

        void CreateTheObject()
        {
            //GameObject.Instantiate<GameObject>();
        }
    }
}
