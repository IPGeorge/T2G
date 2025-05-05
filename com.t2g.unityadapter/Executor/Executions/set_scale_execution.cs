#if UNITY_EDITOR

using SimpleJSON;
using System.Threading.Tasks;
using T2G.Executor;
using UnityEditor;
using UnityEngine;

namespace T2G.Executor
{
    [Execution("set_scale")]
    public class set_scale_execution : Execution
    {
        public async override Awaitable<(bool succeeded, string message)> Execute(Instruction instruction)
        {
            if (!ValidateInstructionKeyword(instruction.Keyword))
            {
                return (false, "Invalid instruction keyword! 'set_scale' was expected.");
            }

            string objName = string.Empty;
            string scaleStr = string.Empty;
            switch (instruction.DataType)
            {
                case Instruction.EDataType.MultipleParameters:
                    {
                        string[] parameters = instruction.Data.Split(',');
                        if (parameters.Length > 0 && !string.IsNullOrWhiteSpace(parameters[0]))
                        {
                            objName = parameters[0];
                            scaleStr = parameters[2];
                        }
                    }
                    break;
                case Instruction.EDataType.JsonData:
                    {
                        JSONObject jsonObj = JSON.Parse(instruction.Data).AsObject;
                        if (jsonObj.HasKey("name"))
                        {
                            objName = jsonObj["name"];
                            scaleStr = jsonObj["scale"];
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
                float[] scaleArr = Executor.ParseFloat3(scaleStr);
                Vector3 scale = new Vector3(scaleArr[0], scaleArr[1], scaleArr[2]);
                gameObj.transform.localScale = scale;
                Executor.ForceUpdateSceneView();
                await Task.Yield();
                return (true, $"{objName} was scaled to {scaleStr}");
            }
            else
            {
                return (false, $"Couldn't find {objName}!");
            }
        }
    }
}

#endif