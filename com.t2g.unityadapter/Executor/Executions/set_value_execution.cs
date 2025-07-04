
#if UNITY_EDITOR

using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
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
                return (false, "Invalid instruction keyword! 'set_value' was expected.");
            }

            if (instruction.DataType != Instruction.EDataType.JsonData)
            {
                return (false, "Invalid instruction data!");
            }

            GameObject gameObj = Selection.activeGameObject;
            var jsonObj = GetInstructionJsonData(instruction);
            string objName = jsonObj["objName"];
            string property = jsonObj["property"];
            string value = jsonObj["value"];

            if (!string.IsNullOrEmpty(objName))
            {
                var targetObj = GameObject.Find(objName);
                if (targetObj == null)
                {
                    return (false, $"{objName } was not found!");
                }
                else
                {
                    gameObj = targetObj;
                }
            }

            if (gameObj != null && !string.IsNullOrEmpty(property) && !string.IsNullOrEmpty(value))
            {
                var components = gameObj.GetComponents<Component>();
                foreach(var component in components)
                {
                    var componentType = component.GetType();
                    foreach (FieldInfo fi in componentType.GetFields())
                    {
                        if (string.Compare(fi.Name, property) == 0)
                        {
                            if (Executor.SetFieldValue(component, fi, value))
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
                        if (string.Compare(pi.Name, property) == 0)
                        {
                            if (Executor.SetPropertyValue(component, pi, value))
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

                    foreach(var method in componentType.GetMethods())
                    {
                        if(string.Compare(method.Name, "SetSpecificPropertyValue") == 0)
                        {
                            method.Invoke(component, new string[] { property, value });
                            return (true, null);
                        }
                    }
                }
            }

            return (false, $"Failed to set {property} value to be {value}!");
        }
    }
}
#endif