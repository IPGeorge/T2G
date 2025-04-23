#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
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

        static protected void PoolAssetsToImport(List<string> assetsToImport)
        {
            if (assetsToImport.Count > 0)
            {
                string poolPath = Path.Combine(Application.persistentDataPath, k_AssetsToImportPoolFileName);
                string prefabsPath = Path.Combine(Application.persistentDataPath, k_AssetsToInstantiateFileName);
                using (StreamWriter poolWriter = new StreamWriter(poolPath, File.Exists(poolPath)),
                       prefabsWriter = new StreamWriter(prefabsPath, File.Exists(prefabsPath)))
                {
                    for (int i = assetsToImport.Count - 1; i >= 0 ; --i)
                    {
                        poolWriter.WriteLine(assetsToImport[i]);
                        if(string.Compare(Path.GetExtension(assetsToImport[i]).ToLower(), "prefab") == 0)
                        {
                            prefabsWriter.WriteLine(assetsToImport[i]);
                        }
                    }
                }
            }
        }

        static void ImportOnePooledAsset(List<string> assetsToImport, string poolPath)
        {
            string assetPath = assetsToImport[0];
            assetsToImport.RemoveAt(0);
            File.WriteAllLines(poolPath, assetsToImport.ToArray());

            string sourceAssetPath = Path.Combine(SettingsT2G.RecoursePath, assetPath);
            string targetAssetPath = Path.Combine(Application.dataPath, assetPath);
            string targetDirectory = Path.GetDirectoryName(targetAssetPath);
            if(File.Exists(sourceAssetPath))
            {
                if(!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }
                File.Copy(sourceAssetPath, targetDirectory);
            }
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
                ImportOnePooledAsset(assetsToImport, poolPath);
                await Task.Yield();
            }
        }

        protected static async void InstantiatePooledPrefabs()
        {
            string prefabsPath = Path.Combine(Application.persistentDataPath, k_AssetsToInstantiateFileName);
            if (!File.Exists(prefabsPath))
            {
                return;
            }
            List<string> prefabsToInstantiate = new List<string>(File.ReadAllLines(prefabsPath));
            foreach(var prefab in prefabsToInstantiate)
            {
                string prefabPath = Path.Combine("Assets", prefab);
                GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                GameObject.Instantiate<GameObject>(prefabAsset);
                await Task.Yield();
            }
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