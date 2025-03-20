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
    [Execution("create_space")]
    public class create_space_exection : Execution
    {
        public async override Awaitable<(bool succeeded, string message)> Execute(Instruction instruction)
        {
            if(!ValidateInstructionKeyword(instruction.Keyword))
            {
                return (false, "Invalid instruction keyword!");
            }

            string spacesPath = Path.Combine(Application.dataPath, "Spaces");
            if (!Directory.Exists(spacesPath))
            {
                Directory.CreateDirectory(spacesPath);
            }

            string spaceName = string.Empty;
            switch(instruction.DataType)
            {
                case Instruction.EDataType.SingleParameter:
                    spaceName = instruction.Data;
                    break;
                case Instruction.EDataType.MultipleParameters:
                    {
                        string[] parameters = instruction.Data.Split(',');
                        if(parameters.Length > 0 && !string.IsNullOrWhiteSpace(parameters[0]))
                        {
                            spaceName = parameters[0];
                        }
                    }
                    break;
                case Instruction.EDataType.JsonData:
                    {
                        JSONObject jsonObj = JSON.Parse(instruction.Data).AsObject;
                        if(jsonObj.HasKey("name"))
                        {
                            spaceName = jsonObj["name"];
                        }
                    }
                    break;
            }

            Debug.LogError($"{instruction.DataType} - {instruction.Data} : spaceName = {spaceName}");

            //Save active space
            var activeScene = EditorSceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(activeScene.name))
            {
                EditorSceneManager.SaveScene(activeScene, Path.Combine(Defs.k_SpacesDirectory, Defs.k_DefaultSpaceName + ".unity"));
            }
            else
            {
                EditorSceneManager.SaveScene(activeScene);
            }

            //create or open the target space
            if(string.IsNullOrWhiteSpace(spaceName))
            {
                spaceName = Defs.k_DefaultSpaceName + ".unity";
            }
            else
            {
                spaceName += ".unity";
            }

            string spaceFile = Path.Combine(spacesPath, spaceName);
            string space = Path.Combine(Defs.k_SpacesDirectory, spaceName);
            if (File.Exists(spaceFile))
            {
                bool isOpened = false;
                EditorSceneManager.sceneOpened += (scene, mode) =>
                {
                    isOpened = true;
                };
                EditorSceneManager.OpenScene(space, OpenSceneMode.Single);

                await Task.Run(()=> { while(!isOpened) { Task.Yield(); } });
            }
            else
            {
                bool isCreated = false;
                EditorSceneManager.newSceneCreated += (scene, setup, mode) =>
                {
                    EditorSceneManager.SaveScene(scene, space);
                    isCreated = true;
                };
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                await Task.Run(() => { while (!isCreated) { Task.Yield(); } });
            }
            return (true, $"Entered {spaceName}.");
        }
    }
}

#endif