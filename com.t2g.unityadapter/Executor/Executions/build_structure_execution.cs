#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace T2G.Executor
{
    [Execution("build_structure")]
    public class build_structure_execution : Execution
    {

        bool _importingPackage = false;

        public async override Awaitable<(bool succeeded, string message)> Execute(Instruction instruction)
        {
            if (!ValidateInstructionKeyword(instruction.Keyword))
            {
                return (false, "Invalid instruction keyword! 'build_structure' was expected.");
            }

            if (instruction.DataType != Instruction.EDataType.JsonData)
            {
                return (false, "Invalid instruction data!");
            }

            var jsonObj = GetInstructionJsonData(instruction);
            string objName = jsonObj["name"];
            string shape = jsonObj["shape"];
            string structure = jsonObj["structure"];
            int width = jsonObj["width"].AsInt;
            int height = jsonObj["height"].AsInt;
            string[] assetPaths = instruction.ResolvedAssetPaths.Split(',');
            int i, j;

            if(assetPaths.Length <= 0 || string.IsNullOrWhiteSpace(assetPaths[0]))
            {
                return (false, $"No element was specified for building the structure {objName}");
            }

            var ext1 = Path.GetExtension(assetPaths[0]).ToLower();
            var prefabPath = string.Empty;
            if(string.Compare(ext1, ".unitypackage") == 0)
            {
                prefabPath = assetPaths[1];
            }
            else if (string.Compare(ext1, ".prefab") == 0)
            {
                prefabPath = assetPaths[0];
            }
            else
            {
                Debug.LogError($"Wrong extenion {ext1}");
                return (false, null);
            }

            string prefabFilePath = Path.Combine(Application.dataPath, prefabPath);
            if(!File.Exists(prefabFilePath))
            {
                _importingPackage = true;
                AssetDatabase.onImportPackageItemsCompleted += (items) => {
                    _importingPackage = false;
                };
                ContentLibrary.ImportAsset(assetPaths[0]);
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }

            await WhatForImportingPackageCompleted();

            prefabPath = "Assets/" + prefabPath;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if(prefab == null)
            {
                Debug.LogError($"prefab '{prefabPath}' is null!");
                return (false, null);
            }

            GameObject strcutureObj = new GameObject(objName.Trim());
            strcutureObj.transform.position = Vector3.zero;
            //Executor.PlaceObjectInFrontOfSceneView(strcutureObj);

            switch (shape)
            {
                case "circle":
                    {
                        int radius = width / 2;
                        Vector3 vec = Vector3.forward * radius; 
                        for(int degree = -180; degree <= 180; ++ degree)
                        {
                            Quaternion rotation = Quaternion.Euler(0, degree, 0); // Rotate around Y-axis
                            Vector3 rotatedForward = rotation * vec;
                            var element = GameObject.Instantiate<GameObject>(prefab, strcutureObj.transform);
                            element.transform.localPosition = rotatedForward;
                        }
                    }
                    break;
                case "square":
                case "rectangle":
                    {
                        int halfWidth = width / 2;
                        int halfHeight = height / 2;
                        for (i = 0; i < width; ++i)
                        {
                            for (j = 0; j < height; ++j)
                            {
                                if (i == 0 || i == width - 1 || j == 0 || j == height - 1)
                                {
                                    var element = GameObject.Instantiate<GameObject>(prefab, strcutureObj.transform);
                                    element.transform.localPosition = new Vector3((halfWidth - width) + j, 0, (halfHeight - height) + i);
                                }
                            }
                        }
                    }
                    break;
            }

            Executor.SaveActiveScene();
            return (true, null);
        }

        async Awaitable WhatForImportingPackageCompleted()
        {
            while(_importingPackage)
            {
                await Task.Yield();
            }
        }
    }
}
#endif