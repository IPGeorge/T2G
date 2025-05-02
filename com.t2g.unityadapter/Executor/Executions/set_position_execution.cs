#if UNITY_EDITOR

using SimpleJSON;
using System.Threading.Tasks;
using T2G.Executor;
using UnityEditor;
using UnityEngine;

namespace T2G.Executor
{
    [Execution("set_position")]
    public class set_position_execution : Execution
    {
        public async override Awaitable<(bool succeeded, string message)> Execute(Instruction instruction)
        {
            if (!ValidateInstructionKeyword(instruction.Keyword))
            {
                return (false, "Invalid instruction keyword! 'set_position' was expected.");
            }

            string objName = string.Empty;
            string positionStr = string.Empty;
            switch (instruction.DataType)
            {
                case Instruction.EDataType.MultipleParameters:
                    {
                        string[] parameters = instruction.Data.Split(',');
                        if (parameters.Length > 0 && !string.IsNullOrWhiteSpace(parameters[0]))
                        {
                            objName = parameters[0];
                            positionStr = parameters[2];
                        }
                    }
                    break;
                case Instruction.EDataType.JsonData:
                    {
                        JSONObject jsonObj = JSON.Parse(instruction.Data).AsObject;
                        if (jsonObj.HasKey("name"))
                        {
                            objName = jsonObj["name"];
                            positionStr = jsonObj["position"];
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
                float[] posArr = Executor.ParseFloat3(positionStr);
                Vector3 position = new Vector3(posArr[0], posArr[1], posArr[2]);
                gameObj.transform.localPosition = position;
                Executor.ForceUpdateSceneView();
                await Task.Yield();
                return (true, $"{objName} was placed at {positionStr}");
            }
            else
            {
                return (false, $"Couldn't find {objName}!");
            }
        }
    }
}

#endif