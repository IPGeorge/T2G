#if UNITY_EDITOR

using SimpleJSON;
using System.Threading.Tasks;
using T2G.Executor;
using UnityEditor;
using UnityEngine;

namespace T2G.Executor
{
    [Execution("set_rotation")]
    public class set_rotation_execution : Execution
    {
        public async override Awaitable<(bool succeeded, string message)> Execute(Instruction instruction)
        {
            if (!ValidateInstructionKeyword(instruction.Keyword))
            {
                return (false, "Invalid instruction keyword! 'set_rotation' was expected.");
            }

            string objName = string.Empty;
            string rotStr = string.Empty;
            switch (instruction.DataType)
            {
                case Instruction.EDataType.MultipleParameters:
                    {
                        string[] parameters = instruction.Data.Split(',');
                        if (parameters.Length > 0 && !string.IsNullOrWhiteSpace(parameters[0]))
                        {
                            objName = parameters[0];
                            rotStr = parameters[2];
                        }
                    }
                    break;
                case Instruction.EDataType.JsonData:
                    {
                        JSONObject jsonObj = JSON.Parse(instruction.Data).AsObject;
                        if (jsonObj.HasKey("name"))
                        {
                            objName = jsonObj["name"];
                            rotStr = jsonObj["eulerAngles"];
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
                float[] rotArr = Executor.ParseFloat3(rotStr);
                Vector3 rot = new Vector3(rotArr[0], rotArr[1], rotArr[2]);
                gameObj.transform.localRotation = Quaternion.Euler(rot);
                Executor.ForceUpdateSceneView();
                await Task.Yield();
                return (true, $"{objName} was rotated to {rotStr}");
            }
            else
            {
                return (false, $"Couldn't find {objName}!");
            }
        }
    }
}
#endif