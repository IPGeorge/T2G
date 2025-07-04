#if UNITY_EDITOR

using SimpleJSON;
using System.Threading.Tasks;
using T2G.Executor;
using UnityEditor;
using UnityEngine;

namespace T2G
{
    [Execution("delete_object")]
    public class delete_object_execution : Execution
    {
        public async override Awaitable<(bool succeeded, string message)> Execute(Instruction instruction)
        {
            if (!ValidateInstructionKeyword(instruction.Keyword))
            {
                return (false, "Invalid instruction keyword! 'delete_object' was expected.");
            }

            string objName = string.Empty;
            switch (instruction.DataType)
            {
                case Instruction.EDataType.SingleParameter:
                    objName = instruction.Data;
                    break;
                case Instruction.EDataType.MultipleParameters:
                    {
                        string[] parameters = instruction.Data.Split(',');
                        if (parameters.Length > 0 && !string.IsNullOrWhiteSpace(parameters[0]))
                        {
                            objName = parameters[0];
                        }
                    }
                    break;
                case Instruction.EDataType.JsonData:
                    {
                        JSONObject jsonObj = JSON.Parse(instruction.Data).AsObject;
                        if (jsonObj.HasKey("name"))
                        {
                            objName = jsonObj["name"];
                        }
                    }
                    break;
            }

            GameObject gameObj = null;
            if (string.IsNullOrWhiteSpace(objName))
            {
                gameObj = Selection.activeGameObject;
                objName = gameObj.name;
            }
            else
            {
                gameObj = GameObject.Find(objName);
            }

            if (gameObj != null)
            {
                GameObject.DestroyImmediate(gameObj);
                Executor.Executor.SaveActiveScene();
                await Task.Yield();
                return (true, $"{objName} was deleted.");
            }
            else
            {
                return (false, $"Couldn't find and delete {objName}!");
            }
        }
    }
}


#endif