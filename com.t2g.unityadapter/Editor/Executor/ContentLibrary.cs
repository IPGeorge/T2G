using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace T2G.UnityAdapter
{
    public class ContentLibrary
    {
        static readonly string k_ImportedPackagesKey = "ImportedPackagesKey";

        static HashSet<string> _Imported = new HashSet<string>();

        [InitializeOnLoadMethod]
        static void LoadImported()
        {
            string importedString = EditorPrefs.GetString(k_ImportedPackagesKey, string.Empty);
            if(!string.IsNullOrEmpty(importedString))
            {
                _Imported = new HashSet<string>(importedString.Split(","));
            }
        }

        static void RegisterToImported(string assetName, bool save = true)
        {
            _Imported.Add(assetName);
            if(save)
            {
                string importedString = string.Empty;

                int cnt = 0;
                foreach (var name in _Imported)
                {
                    importedString += (cnt < _Imported.Count - 1) ? (name + ",") : name; 
                }
                EditorPrefs.SetString(k_ImportedPackagesKey, importedString);
            }
        }

        public static bool ImportScript(string scriptName, string dependencies)
        {
            bool retVal = true;

            if(_Imported.Contains(scriptName))
            {
                return retVal;
            }

            string scriptsFolderName = "Scripts";
            var sourcePath = Path.Combine(Settings.RecoursePath, scriptsFolderName, scriptName);
            var destDir = Path.Combine(Application.dataPath, scriptsFolderName);
            var destPath = Path.Combine(destDir, scriptName);
            if (File.Exists(sourcePath))
            {
                if (File.Exists(destPath))
                {
                    retVal = true;
                }
                else
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
                        if (File.Exists(sourcePath))
                        {
                            File.Copy(sourcePath, destPath);
                            RegisterToImported(dependency, false);
                        }
                    }
                    RegisterToImported(scriptName);
                    AssetDatabase.Refresh();
                }
            }
            else
            {
                retVal = false;
            }

            return retVal;
        }

        public static bool ImportPackage(string prefabName, AssetDatabase.ImportPackageCallback CompletedHanddler)
        {
            if (!Settings.Loaded)
            {
                Settings.Load();
            }

            if(_Imported.Contains(prefabName))
            {
                return true;
            }

            string packagePath = Path.Combine(Settings.RecoursePath, "Prefabs", prefabName, $"{prefabName}.unitypackage");
            AssetDatabase.importPackageCompleted += CompletedHanddler;
            AssetDatabase.ImportPackage(packagePath, false);
            RegisterToImported(prefabName);
            AssetDatabase.Refresh();
            return true;
        }
    }
}
