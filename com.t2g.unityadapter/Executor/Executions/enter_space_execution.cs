#if UNITY_EDITOR

using SimpleJSON;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using T2G.Executor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace T2G
{
    [Execution("enter_space")]
    public class enter_space_exection : Execution
    {
        public async override Awaitable<(bool succeeded, string message)> Execute(Instruction instruction)
        {
            if (!ValidateInstructionKeyword(instruction.Keyword))
            {
                return (false, "Invalid instruction keyword!");
            }

            string spaceName = string.Empty;
            switch (instruction.DataType)
            {
                case Instruction.EDataType.SingleParameter:
                    spaceName = instruction.Data;
                    break;
                case Instruction.EDataType.MultipleParameters:
                    {
                        string[] parameters = instruction.Data.Split(',');
                        if (parameters.Length > 0 && !string.IsNullOrWhiteSpace(parameters[0]))
                        {
                            spaceName = parameters[0];
                        }
                    }
                    break;
                case Instruction.EDataType.JsonData:
                    {
                        JSONObject jsonObj = JSON.Parse(instruction.Data).AsObject;
                        if (jsonObj.HasKey("name"))
                        {
                            spaceName = jsonObj["name"];
                        }
                    }
                    break;
            }

            string spacesPath = Path.Combine(Application.dataPath, "Spaces");
            string spaceFile = Path.Combine(spacesPath, spaceName + ".unity");
            string space = Path.Combine(Defs.k_SpacesDirectory, spaceName + ".unity");
            if (!Directory.Exists(spacesPath) || string.IsNullOrWhiteSpace(spaceName) || !File.Exists(spaceFile))
            {
                return (false, $"Space doesn't exist!");
            }

            var activeScene = EditorSceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(activeScene.name))
            {
                EditorSceneManager.SaveScene(activeScene, Path.Combine(Defs.k_SpacesDirectory, Defs.k_DefaultSpaceName + ".unity"));
            }
            else
            {
                EditorSceneManager.SaveScene(activeScene);
            }

            bool isOpened = false;
            EditorSceneManager.sceneOpened += (scene, mode) =>
            {
                isOpened = true;
            };
            EditorSceneManager.OpenScene(space, OpenSceneMode.Single);

            await Task.Run(() => { while (!isOpened) { Task.Yield(); } });
            return (true, $"Entered {spaceName}.");
        }
    }
}

#endif