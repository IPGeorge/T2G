using UnityEngine;
using SimpleJSON;
using System.Collections.Generic;
using System;
using System.IO;

public class JsonParser
{
    public static string JsonSample = @"{ 
        ""array"": [1, 2, 3],
        ""boolean"": true,
        ""color"": ""gold"",
        ""null"": null,
        ""number"": 123,
        ""object"": {
            ""a"": ""b"",
            ""c"": ""d""
        },
        ""string"": ""Hello World""
    }";

    static readonly int k_Indent = 4;
    
    public static string JSONText;

    public static JSONObject SerializeObject(object obj)
    {
        JSONObject jsonObject = null;
        if(obj == null)
        {
            return jsonObject;
        }
        var objType = obj.GetType();
        if (objType.IsClass)
        {
            jsonObject = new JSONObject();
            
            var fields = objType.GetFields();
            foreach (var field in fields)
            {
                var fieldValue = field.GetValue(obj);
                if (fieldValue == null)
                {
                    jsonObject.Add(field.Name, null);
                }
                else if (field.FieldType.IsArray)
                {
                    jsonObject.Add(field.Name, SerializeArray(fieldValue));
                }
                else if (field.FieldType == typeof(string))
                {
                    jsonObject.Add(field.Name, (string)fieldValue);
                }
                else if(field.FieldType.IsPrimitive)  //TODO: consider standarize this block  
                {
                    if (field.FieldType == typeof(float))
                    {
                        jsonObject.Add(field.Name, (float)fieldValue);
                    }
                    else if (field.FieldType == typeof(double))
                    {
                        jsonObject.Add(field.Name, (double)fieldValue);
                    }
                    else if (field.FieldType == typeof(int))
                    {
                        jsonObject.Add(field.Name, (int)fieldValue);
                    }
                    else if (field.FieldType == typeof(long))
                    {
                        jsonObject.Add(field.Name, (long)fieldValue);
                    }
                    else if (field.FieldType == typeof(ulong))
                    {
                        jsonObject.Add(field.Name, (ulong)fieldValue);
                    }
                    else if (field.FieldType == typeof(bool))
                    {
                        jsonObject.Add(field.Name, (bool)fieldValue);
                    }
                }
                else if (field.FieldType.IsClass)
                {
                    jsonObject.Add(field.Name, SerializeObject(fieldValue));
                }
                else
                {
                    jsonObject.Add(field.Name, "Unsopported field type");
                }
            }
        }

        return jsonObject;
    }

    static JSONArray SerializeArray(object array)
    {
        System.Collections.IList list = (System.Collections.IList)array;
        var elementType = array.GetType().GetElementType();
        JSONArray arr = new JSONArray();

        for (int i = 0; i < list.Count; ++i)
        {
            if (elementType == typeof(string))
            {
                arr.Add((string)list[i]);
            }
            else if (elementType.IsPrimitive)
            {
                if (elementType == typeof(float))
                {
                    arr.Add((float)list[i]);
                }
                else if (elementType == typeof(double))
                {
                    arr.Add((double)list[i]);
                }
                else if (elementType == typeof(int))
                {
                    arr.Add((int)list[i]);
                }
                else if (elementType == typeof(long))
                {
                    arr.Add((long)list[i]);
                }
                else if (elementType == typeof(ulong))
                {
                    arr.Add((ulong)list[i]);
                }
                else if (elementType == typeof(bool))
                {
                    arr.Add((bool)list[i]);
                }
            }
            else if (elementType.IsClass)
            {
                arr.Add(SerializeObject(list[i]));
            }
            else
            {
                arr.Add(null);
            }
        }
        return arr;
    }


    public static bool SerializeAndSave(GameDesc gameDesc)
    {
        string json = Serialize(gameDesc);
        if(string.IsNullOrEmpty(json))
        {
            return false;
        }
        var path = Path.Combine(Application.persistentDataPath, gameDesc.Name + ".gamedesc");
        File.WriteAllText(path, json);
        return true;
    }

    public static string Serialize(GameDesc gameDesc)
    {
        var jsonObj = SerializeObject(gameDesc);
        JSONText = jsonObj.ToString(k_Indent);
        return JSONText;
    }

    public static List<string> GetGameDescList()
    {
        List<string> list = new List<string>();
        var gameDescs = Directory.GetFiles(Application.persistentDataPath, "*.gamedesc");
        for(int i = 0; i < gameDescs.Length; ++i)
        {
            list.Add(Path.GetFileNameWithoutExtension(gameDescs[i]));
        }
        return list;
    }

    public static string LoadGameDescJsonString(string gameDescName)
    {
        var path = Path.Combine(Application.persistentDataPath, gameDescName + ".gamedesc");
        if (!File.Exists(path))
        {
            return null;
        }
        return File.ReadAllText(path);
    }

    public static JSONObject LoadGameDescJsonObject(string gameDescName)
    {
        JSONObject jsonObject = null;
        string json = JSONText = LoadGameDescJsonString(gameDescName);
        if(json != null)
        {
            jsonObject = JSON.Parse(json).AsObject;
        }
        return jsonObject;
    }

    public static GameDesc LoadGameDesc(string gameDescName)
    {
        GameDesc gameDesc = null;
        var gameDescJsonObj = LoadGameDescJsonObject(gameDescName);
        if (gameDescJsonObj != null)
        {
            gameDesc = Deseialialize(gameDescJsonObj);
        }
        return gameDesc;
    }

    public static GameDesc Deseialialize(JSONObject gameDescJsonObj)
    {
        GameDesc gameDesc = null;
        if (gameDescJsonObj != null)
        {
            gameDesc = new GameDesc();
            DeseialializeObject(gameDesc, gameDescJsonObj);
        }
        return gameDesc;
    }

    static void DeseialializeObject(object obj, JSONObject jsonObj)
    {
        if(obj == null || jsonObj == null)
        {
            return;
        }

        var fields = obj.GetType().GetFields();
        foreach (var field in fields)
        {
            if (jsonObj.HasKey(field.Name))
            {
                if (field.FieldType.IsArray)
                {
                    var elementType = field.FieldType.GetElementType();
                    JSONArray jsonArray = jsonObj[field.Name].AsArray;
                    if(jsonArray == null)
                    {
                        field.SetValue(obj, null);
                    }
                    else
                    {
                        int arrLen = jsonArray.Count;
                        var fieldArr = Array.CreateInstance(elementType, arrLen);
                        field.SetValue(obj, fieldArr);
                        for (int i = 0; i < arrLen; ++i)
                        {
                            if (elementType == typeof(string))
                            {
                                fieldArr.SetValue(jsonArray[i].Value, i);
                            }
                            else if (elementType.IsPrimitive)
                            {
                                if (elementType == typeof(float))
                                {
                                    fieldArr.SetValue(jsonArray[i].AsFloat, i);
                                }
                                else if (elementType == typeof(double))
                                {
                                    fieldArr.SetValue(jsonArray[i].AsDouble, i);
                                }
                                else if (elementType == typeof(int))
                                {
                                    fieldArr.SetValue(jsonArray[i].AsInt, i);
                                }
                                else if (elementType == typeof(long))
                                {
                                    fieldArr.SetValue(jsonArray[i].AsLong, i);
                                }
                                else if (elementType == typeof(ulong))
                                {
                                    fieldArr.SetValue(jsonArray[i].AsULong, i);
                                }
                                else if (elementType == typeof(bool))
                                {
                                    fieldArr.SetValue(jsonArray[i].AsBool, i);
                                }
                            }
                            else if (elementType.IsClass)
                            {
                                var jsonElement = jsonArray[i].AsObject;
                                if (jsonElement != null)
                                {
                                    var elementObj = Activator.CreateInstance(elementType);
                                    DeseialializeObject(elementObj, jsonElement.AsObject);
                                    fieldArr.SetValue(elementObj, i);
                                }
                                else
                                {
                                    fieldArr.SetValue(null, i);
                                }
                            }
                        }
                    }
                }
                else if (field.FieldType == typeof(string))
                {
                    var strVal = jsonObj[field.Name];
                    field.SetValue(obj, strVal.Value);
                }
                else if (field.FieldType.IsPrimitive)
                {
                    if (field.FieldType == typeof(float))
                    {
                        field.SetValue(obj, jsonObj[field.Name].AsFloat);
                    }
                    else if (field.FieldType == typeof(double))
                    {
                        field.SetValue(obj, jsonObj[field.Name].AsDouble);
                    }
                    else if (field.FieldType == typeof(int))
                    {
                        field.SetValue(obj, jsonObj[field.Name].AsInt);
                    }
                    else if (field.FieldType == typeof(long))
                    {
                        field.SetValue(obj, jsonObj[field.Name].AsLong);
                    }
                    else if (field.FieldType == typeof(ulong))
                    {
                        field.SetValue(obj, jsonObj[field.Name].AsULong);
                    }
                    else if (field.FieldType == typeof(bool))
                    {
                        field.SetValue(obj, jsonObj[field.Name].AsBool);
                    }
                }
                else if (field.FieldType.IsClass)
                {
                    var jsonObject = jsonObj[field.Name].AsObject;
                    if (jsonObj != null)
                    {
                        var fieldObject = Activator.CreateInstance(field.FieldType);
                        DeseialializeObject(fieldObject, jsonObj[field.Name].AsObject);
                        field.SetValue(obj, fieldObject);
                    }
                    else
                    {
                        field.SetValue(obj, null);
                    }
                }
            }
        }
    }

    public static void DeleteGameDesc(string gameDescName)
    {
        var path = Path.Combine(Application.persistentDataPath, gameDescName + ".gamedesc");
        if(File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
