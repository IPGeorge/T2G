#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

namespace T2G.UnityAdapter
{
    public class ContentLibrary
    {
        public static int ImportScript(string scriptName, string dependencies)
            //0-imported
            //1-already exists, no import is needed
            //-1-failed
        {
            int retVal = -1;
            string scriptsFolderName = "Scripts";
            var sourcePath = Path.Combine(Settings.RecoursePath, scriptsFolderName, scriptName);
            var destDir = Path.Combine(Application.dataPath, scriptsFolderName);
            var destPath = Path.Combine(destDir, scriptName);
            if (File.Exists(destPath))
            {
                retVal = 1;
            }
            else if (File.Exists(sourcePath))
            {
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }
                File.Copy(sourcePath, destPath);
                var importPath = Path.Combine("Assets", scriptsFolderName, scriptName);
                var dependencyArray = dependencies.Split(',');
                foreach (var dependency in dependencyArray)
                {
                    sourcePath = Path.Combine(Settings.RecoursePath, scriptsFolderName, dependency);
                    destPath = Path.Combine(Application.dataPath, scriptsFolderName, dependency);
                    if (!File.Exists(destPath) && File.Exists(sourcePath))
                    {
                        File.Copy(sourcePath, destPath);
                    }
                }
                AssetDatabase.Refresh();
                retVal = 0;
            }

            return retVal;
        }

        public static int ImportPrefab(string prefabName, AssetDatabase.ImportPackageCallback CompletedHanddler)
        {
            if (!Settings.Loaded)
            {
                Settings.Load();
            }
            string prefabsFolderName = "Prefabs";
            string packagePath = Path.Combine(Settings.RecoursePath, prefabsFolderName, prefabName, $"{prefabName}.unitypackage");
            string assetPrefabPathFolder = Path.Combine(Application.dataPath, prefabsFolderName);
            string assetPrefabPath = Path.Combine(assetPrefabPathFolder, prefabName, $"{prefabName}.prefab");
            if(File.Exists(assetPrefabPath))
            {
                return 1;
            }
            else if (File.Exists(packagePath))
            {
                if (!Directory.Exists(assetPrefabPathFolder))
                {
                    Directory.CreateDirectory(assetPrefabPathFolder);
                }
                AssetDatabase.importPackageCompleted += CompletedHanddler;
                AssetDatabase.ImportPackage(packagePath, false);
                AssetDatabase.Refresh();
                return 0;
            }
            else
            {
                return -1;
            }
        }

        public static bool ImportPackage(string packageName, AssetDatabase.ImportPackageCallback CompletedHanddler)
        {
            if (!Settings.Loaded)
            {
                Settings.Load();
            }
            string packagePath = Path.Combine(Settings.RecoursePath, "Packages", packageName, $"{packageName}.unitypackage");
            if (File.Exists(packagePath))
            {
                AssetDatabase.importPackageCompleted += CompletedHanddler;
                AssetDatabase.ImportPackage(packagePath, false);
                AssetDatabase.Refresh();
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}

#endif