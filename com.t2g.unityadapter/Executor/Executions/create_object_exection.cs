using SimpleJSON;
using System;
using System.Threading.Tasks;
using T2G.Executor;
using UnityEngine;

namespace T2G
{
    [Execution("create_object ")]
    public class create_object_exection : Execution
    {
        public async override Awaitable<(bool succeeded, string message)> Execute(Instruction instruction)
        {
            if(instruction.DataType != Instruction.EDataType.JsonData)
            {
                return (false, "Invalid instruction data!");
            }

            ContentLibrary.ResolveInstruction(ref instruction);

            JSONObject jsonObj = JSON.Parse(instruction.Data).AsObject;
            string name = jsonObj["name"];
            string assetSourcePath = jsonObj["asset_path"];  //Relative path to
                                                             //The Resource Path (source)
                                                             //The game project data path "\Assets". (target)
                                                             //example "/Prefabs/Primitives/cube.prefab"


            return (true, $"{name} was created!");
        }
    }
}
