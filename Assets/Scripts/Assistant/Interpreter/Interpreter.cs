using System;
using System.Collections.Generic;
using System.Text;
using SimpleJSON;

public partial class Interpreter 
{
    static List<string> _instructions = new List<string>();
    static string _currentWorldName;
    static string _currentObjectName;

    public static string CurrentWorldName => _currentWorldName;
    public static string CurrentObjectName => _currentObjectName;

    public static string[] Interpret(string gameDescJson)
    {
        _instructions.Clear();
        if (gameDescJson != null)
        {
            JSONNode gameDescObj = JSON.Parse(gameDescJson);
            Interpret(gameDescObj);
        }
        _instructions.Add(Defs.k_EndOfGameGeneration);
        return _instructions.ToArray();
    }

    static void Interpret(JSONNode jsonNode)
    {
        if (jsonNode.IsNull)
        {
            return;
        }

        if (jsonNode.IsObject)
        {
            if(jsonNode.HasKey(Defs.k_GameDesc_CategoryKeyName))
            {
                InterpretByCategoryName(jsonNode.AsObject);
            }
        }
        if (jsonNode.IsArray)
        {
            var arr = jsonNode.AsArray;
            for(int i = 0; i < arr.Count; ++i)
            {
                Interpret(arr[i]);
            }
        }
        else if(jsonNode.IsBoolean)
        {
            //Do nothing
        }
        else if(jsonNode.IsNumber)
        {
            //Do nothing
        }
        else if(jsonNode.IsString)
        {
            //Do nothing
        }
    }

    static bool InterpretByCategoryName(JSONObject jsonObj)
    {
        var categoryName = jsonObj.GetValueOrDefault(Defs.k_GameDesc_CategoryKeyName, string.Empty);
        if(!categoryName.IsString)
        {
            return false;
        }
        string category = categoryName.Value;

        if (category.CompareTo(Defs.k_GameWorldCategory) == 0)
        {
            if(INS_CreateGameWorld(jsonObj, ref _currentWorldName))
            {
                var key = jsonObj.Keys.GetEnumerator();
                while (key.MoveNext())
                {
                    Interpret(jsonObj.GetValueOrDefault(key.Current, null));
                }
            }
        }
        else if (category.CompareTo(Defs.k_WorldObjectCategory) == 0)
        {
            if(INS_CreateObject(jsonObj, ref _currentObjectName))
            {
                var key = jsonObj.Keys.GetEnumerator();
                while (key.MoveNext())
                {
                    Interpret(jsonObj.GetValueOrDefault(key.Current, null));
                }
            }
        }
        else if (category.CompareTo(Defs.k_ObjectAddonCategory) == 0)
        {
            INS_AddAddon(jsonObj, _currentWorldName, _currentObjectName);
        }
        else if (category.CompareTo(Defs.k_GameDescCategory) == 0)
        {
            var packages = jsonObj.GetValueOrDefault(Defs.k_GameDesc_PackagesKey, null).AsArray;
            for(int i = 0; i < packages.Count; ++i)
            {
                string packageName = packages[i].ToString();
                _instructions.Add($"IMPORT_PACKAGE {packageName}");
            }

            JSONNode gameWorlds = jsonObj.GetValueOrDefault(Defs.k_GameDesc_GameWorldsKey, null);
            Interpret(gameWorlds);
        }

        return true;
    }
}
