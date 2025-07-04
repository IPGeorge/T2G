#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using SimpleJSON;

namespace T2G.Executor
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ExecutionAttribute : Attribute
    {
        public string Keyword { get; private set; }

        public ExecutionAttribute(string keyword)
        {
            Keyword = keyword;
        }
    }

    public abstract class Execution
    {
        protected static readonly string k_AssetsToImportPoolFileName = "AssetsToImportPool.txt";
        protected static readonly string k_AssetsToInstantiateFileName = "AssetsToInstantiate.txt";

        public Execution() {  }

        public abstract Awaitable<(bool succeeded, string message)> Execute(Instruction instruction);

        protected string GetAttributeName()
        {
            var attribute = GetType().GetCustomAttribute<ExecutionAttribute>();
            if (attribute == null)
            {
                return null;
            }
            return attribute.Keyword;
        }

        protected bool ValidateInstructionKeyword(string keyword)
        {
            var attributeName = GetAttributeName();
            return (string.Compare(keyword.ToLower(), attributeName.ToLower()) == 0);
        }

        protected string GetInstructionParameter(Instruction instruction)
        {
            if(instruction.DataType != Instruction.EDataType.SingleParameter)
            {
                return null;
            }

            return instruction.Data;
        }

        protected string[] GetInstructionParameters(Instruction instruction)
        {
            if (instruction.DataType != Instruction.EDataType.MultipleParameters)
            {
                return null;
            }

            var parameters = instruction.Data.Split(',');
            if(parameters.Length > 0)
            {
                return parameters;
            }
            else
            {
                return null;
            }
        }

        protected JSONObject GetInstructionJsonData(Instruction instruction)
        {
            if (instruction.DataType != Instruction.EDataType.JsonData)
            {
                return null;
            }

            return JSON.Parse(instruction.Data).AsObject;
        }


        static protected void PoolAssetsToImport(List<string> assetsToImport, string newObjPrefab = null, string newObjName = null)
        {
            if (assetsToImport.Count > 0)
            {
                string poolPath = Path.Combine(Application.persistentDataPath, k_AssetsToImportPoolFileName);
                using (StreamWriter poolWriter = new StreamWriter(poolPath, File.Exists(poolPath)))
                {
                    for (int i = 0; i < assetsToImport.Count; ++i)
                    {
                        poolWriter.WriteLine(assetsToImport[i]);
                    }
                }
            }

            if(newObjPrefab != null)
            {
                string prefabPath = Path.Combine(Application.persistentDataPath, k_AssetsToInstantiateFileName);
                using (StreamWriter prefabsWriter = new StreamWriter(prefabPath, File.Exists(prefabPath)))
                {
                    if (string.Compare(Path.GetExtension(newObjPrefab).ToLower(), ".prefab") == 0)
                    {
                        prefabsWriter.WriteLine(newObjPrefab);
                        prefabsWriter.WriteLine(newObjName == null ? string.Empty : newObjName);
                    }
                    else
                    {
                        Debug.LogError($"Invalid new object prefab: {newObjPrefab}");
                    }
                }
            }
        }

        static async Awaitable ImportOnePooledAsset(List<string> assetsToImport, string poolPath)
        {
            string assetPath = assetsToImport[0];
            assetsToImport.RemoveAt(0);
            File.WriteAllLines(poolPath, assetsToImport.ToArray());
            await ContentLibrary.ImportAsset(assetPath);
        }

        protected static async Awaitable<bool> ImportPooledAssets()
        {
            string poolPath = Path.Combine(Application.persistentDataPath, k_AssetsToImportPoolFileName);
            if (!File.Exists(poolPath))
            {
                return false;
            }
            List<string> assetsToImport = new List<string>(File.ReadAllLines(poolPath));
            while(assetsToImport.Count > 0)
            {
                await ImportOnePooledAsset(assetsToImport, poolPath);
                await Task.Yield();
            }
            File.Delete(poolPath);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            return true;
        }

        protected static async Awaitable<(bool result, int exitCode)> InstantiatePooledPrefab(string objName = null, string prefabPoolPath = null)
        {
            while (EditorApplication.isCompiling)
            {
                await Task.Yield();
            }

            string prefabPath = string.Empty;
            if (string.IsNullOrEmpty(prefabPoolPath))
            {
                prefabPoolPath = Path.Combine(Application.persistentDataPath, k_AssetsToInstantiateFileName);
                if (File.Exists(prefabPoolPath))
                {
                    List<string> prefabsToInstantiate = new List<string>(File.ReadAllLines(prefabPoolPath));
                    if(prefabsToInstantiate.Count >= 2)
                    { 
                        prefabPath = Path.Combine("Assets", prefabsToInstantiate[0]);
                        objName = string.IsNullOrEmpty(prefabsToInstantiate[1]) ? 
                            $"obj{DateTime.Now.Ticks}":
                            prefabsToInstantiate[1];
                        File.Delete(prefabPoolPath);
                    }
                    else
                    {
                        File.Delete(prefabPoolPath);
                        return (false, 1);      //Invalid data file
                    }
                }
                else
                {
                    return (false, 0);          //No need to instantiate the prefab
                }
            }
            else
            {
                prefabPath = Path.Combine("Assets", prefabPoolPath);
                prefabPoolPath = Path.Combine(Application.persistentDataPath, k_AssetsToInstantiateFileName);
                if(File.Exists(prefabPoolPath))
                {
                    File.Delete(prefabPoolPath);
                }
            }

            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset == null)
            {
                return (false, 2);      //prefab is null
            }
            GameObject gameObj = GameObject.Instantiate<GameObject>(prefabAsset);

            if(string.IsNullOrEmpty(objName))
            {
                objName = $"Obj{DateTime.Now.Ticks.ToString()}";
            }
            gameObj.name = objName;

            string tag = gameObj.tag.ToLower();
            if (string.Compare(tag, "natual") == 0)
            {
                gameObj.transform.position = Vector3.zero;
                gameObj.transform.rotation = Quaternion.identity;
            }
            else if(string.Compare(tag, "located") != 0)
            {
                Executor.PlaceObjectInFrontOfSceneView(gameObj);
            }
            
            Executor.PutDownObject(gameObj);
            Selection.activeObject = gameObj;
            Executor.ForceUpdateEditorWindows();
            await Task.Yield();
            return (true, 0);
        }
    }

    
    [Execution("test")]
    public class test_Execution : Execution
    {
        public async override Awaitable<(bool succeeded, string message)> Execute(Instruction instruction)
        {
            await Task.Delay(1000);
            return (true, "Ok!");
        }
    }

    [Execution("invalid")]
    public class invalid_Execution : Execution
    {
        public async override Awaitable<(bool succeeded, string message)> Execute(Instruction instruction)
        {
            await Task.Delay(100);
            return (true, "Invalid instruction execution!");
        }
    }

}

#endif