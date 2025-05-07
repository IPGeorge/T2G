
#if UNITY_EDITOR

using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using SimpleJSON;
using System.Reflection;

namespace T2G.Executor
{
    [Execution("set_value")]
    public class set_value_execution : Execution
    {
        public async override Awaitable<(bool succeeded, string message)> Execute(Instruction instruction)
        {
            if (!ValidateInstructionKeyword(instruction.Keyword))
            {
                return (false, "Invalid instruction keyword! 'add_script' was expected.");
            }

            if (instruction.DataType != Instruction.EDataType.JsonData)
            {
                return (false, "Invalid instruction data!");
            }

            GameObject gameObj = Selection.activeGameObject;
            var jsonObj = GetInstructionJsonData(instruction);
            string name = jsonObj["name"];
            string scriptName = jsonObj["scriptName"];
            string fieldName = jsonObj["fieldName"];
            string fieldValue = jsonObj["value"];
            string fieldType = jsonObj["dataType"];       //string, bool, int, float, float2(Vectro2), float3(Vector3), float4(Vectro4), Color 

            if (!string.IsNullOrEmpty(jsonObj["name"]))
            {
                var targetObj = GameObject.Find(name);
                if (targetObj == null)
                {
                    return (false, "No target game object was found!");
                }
                else
                {
                    gameObj = targetObj;
                }
            }

            if (gameObj != null && !string.IsNullOrEmpty(fieldName) && !string.IsNullOrEmpty(fieldValue))
            {
                var components = gameObj.GetComponents<Component>();
                foreach(var component in components)
                {
                    var componentType = component.GetType();
                    if (string.Compare(componentType.Name, scriptName) == 0 || string.IsNullOrEmpty(scriptName))
                    {
                        foreach (FieldInfo fi in componentType.GetFields())
                        {
                            if (string.Compare(fi.Name, fieldName) == 0)
                            {
                                if(Executor.SetFieldValue(component, fi, fieldValue))
                                {
                                    await Task.Yield();
                                    return (true, null);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        foreach (PropertyInfo pi in componentType.GetProperties())
                        {
                            Debug.LogError($"{fieldName}: {fieldType} --> actual propertyName={pi.Name}: {pi.PropertyType.Name}");
                            if (string.Compare(pi.Name, fieldName) == 0)
                            {
                                if(Executor.SetPropertyValue(component, pi, fieldValue))
                                {
                                    await Task.Yield();
                                    return (true, null);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return (false, $"Failed to set value for {scriptName}.{fieldName}!");
        }
    }
}
#endif