#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

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
        protected static  readonly string k_AssetsToImportPoolFileName = "AssetsToImportPool.txt";
        protected static readonly string k_AssetsToInstantiateFileName = "AssetsToInstantiate.txt";

        public Execution() {  }

        public abstract Awaitable<(bool succeeded, string message)> Execute(Instruction instruction);

        protected bool ValidateInstructionKeyword(string keyword)
        {
            var attribute = GetType().GetCustomAttribute<ExecutionAttribute>();
            return (string.Compare(keyword.ToLower(), attribute.Keyword.ToLower()) == 0);
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

        protected static async Awaitable ImportPooledAssets()
        {
            string poolPath = Path.Combine(Application.persistentDataPath, k_AssetsToImportPoolFileName);
            if (!File.Exists(poolPath))
            {
                return;
            }
            List<string> assetsToImport = new List<string>(File.ReadAllLines(poolPath));
            while(assetsToImport.Count > 0)
            {
                await ImportOnePooledAsset(assetsToImport, poolPath);
                await Task.Yield();
            }
            File.Delete(poolPath);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        protected static async Awaitable<bool> InstantiatePooledPrefab(string objName = null, string prefabPoolPath = null)
        {
            Debug.Log($"create obj: {objName}, prefab: {prefabPoolPath}");
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
                        return false;
                    }
                }
                else
                {
                    return false;
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
                Debug.LogError($"Failed 2: path={prefabPath}; prefabAsset is null!");
                return false;
            }
            GameObject gameObj = GameObject.Instantiate<GameObject>(prefabAsset);

            if(string.IsNullOrEmpty(objName))
            {
                objName = $"Obj{DateTime.Now.Ticks.ToString()}";
            }
            gameObj.name = objName;
            Executor.PlaceObjectInFrontOfSceneView(gameObj);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            await Task.Yield();
            return true;
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