#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor.SceneManagement;
using T2G.Communicator;
using SimpleJSON;
using UnityEditor;

namespace T2G.Executor
{
    public partial class Executor
    {
        public static void RespondCompletion(bool succeeded, string message = null)
        //Succeeded=[true: code=0(completed), code=1(postponed)]; [false: failed]
        {
            string response;
            if (!string.IsNullOrEmpty(message))
            {
                JSONObject jsonObj = new JSONObject();
                jsonObj.Add("succeeded", succeeded);
                jsonObj.Add("message", message);
                response = jsonObj.ToString();
                CommunicatorServer.Instance.SendMessage(eMessageType.Response, response);
            }
            else
            {
                response = succeeded ? "Done!" : "Failed!";
                CommunicatorServer.Instance.SendMessage(eMessageType.Message, response);
            }
        }

        public static float[] ParseFloat2(string float2String)
        {
            float[] fValue = new float[2] { 0.0f, 0.0f };
            int lastIndex = float2String.Length - 1;
            if ((float2String.Substring(0, 1).CompareTo("[") == 0 || float2String.Substring(0, 1).CompareTo("(") == 0) &&
                (float2String.Substring(lastIndex, 1).CompareTo("]") == 0 || float2String.Substring(lastIndex, 1).CompareTo(")") == 0))
            {
                float2String = float2String.Substring(1, float2String.Length - 2);
                var elements = float2String.Split(',');
                if (elements.Length == 2)
                {
                    for (int i = 0; i < 2; ++i)
                    {
                        float.TryParse(elements[i], out fValue[i]);
                    }
                    return fValue;
                }
            }
            return null;
        }

        public static float[] ParseFloat3(string float3String)
        {
            float[] fValue = new float[3] { 0.0f, 0.0f, 0.0f };
            int lastIndex = float3String.Length - 1;
            if ((float3String.Substring(0, 1).CompareTo("[") == 0 || float3String.Substring(0, 1).CompareTo("(") == 0) &&
                (float3String.Substring(lastIndex, 1).CompareTo("]") == 0 || float3String.Substring(lastIndex, 1).CompareTo(")") == 0))
            {
                float3String = float3String.Substring(1, float3String.Length - 2);
                var elements = float3String.Split(',');
                if (elements.Length == 3)
                {
                    for (int i = 0; i < 3; ++i)
                    {
                        float.TryParse(elements[i], out fValue[i]);
                    }
                    return fValue;
                }
            }
            return null;
        }

        public static float[] ParseFloat4(string float4String)
        {
            float[] fValue = new float[4] { 0.0f, 0.0f, 0.0f, 0.0f };
            int lastIndex = float4String.Length - 1;
            if ((float4String.Substring(0, 1).CompareTo("[") == 0 || float4String.Substring(0, 1).CompareTo("(") == 0) &&
                (float4String.Substring(lastIndex, 1).CompareTo("]") == 0) || (float4String.Substring(lastIndex, 1).CompareTo(")") == 0))
            {
                float4String = float4String.Substring(1, float4String.Length - 2);
                var elements = float4String.Split(',');
                if (elements.Length == 4)
                {
                    for (int i = 0; i < 4; ++i)
                    {
                        float.TryParse(elements[i], out fValue[i]);
                    }
                    return fValue;
                }
            }
            return null;
        }

        public static void ParseAndSetValues(GameObject gameObject, string controller, string valuePairs)
        {
            Dictionary<string, string> fieldValues = new Dictionary<string, string>();
            var setValues = valuePairs.Substring(1, valuePairs.Length - 2).Split("\",\"");
            for (int i = 0; i < setValues.Length - 1; i += 2)
            {
                fieldValues.Add(setValues[i].Trim('"'), setValues[i + 1].Trim('"'));
            }

            //Set properties
            var component = gameObject.GetComponent(controller);
            Type type = component.GetType();
            var properties = type.GetProperties();
            for (int i = 0; i < properties.Length; ++i)
            {
                var propertyInfo = properties[i];
                if (fieldValues.ContainsKey(propertyInfo.Name))
                {
                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        propertyInfo.SetValue(component, fieldValues[propertyInfo.Name]);
                    }
                    else if (propertyInfo.PropertyType == typeof(int))
                    {
                        propertyInfo.SetValue(component, int.Parse(fieldValues[propertyInfo.Name]));
                    }
                    else if (propertyInfo.PropertyType == typeof(float))
                    {
                        propertyInfo.SetValue(component, float.Parse(fieldValues[propertyInfo.Name]));
                    }
                    else if (propertyInfo.PropertyType == typeof(bool))
                    {
                        propertyInfo.SetValue(component, bool.Parse(fieldValues[propertyInfo.Name]));
                    }
                    else if (propertyInfo.PropertyType == typeof(Color))
                    {
                        var valuesStr = fieldValues[propertyInfo.Name];
                        valuesStr = valuesStr.Substring(1, valuesStr.Length - 2);
                        var values = valuesStr.Split(",");
                        Color color = new Color(float.Parse(values[0]),
                            float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
                        propertyInfo.SetValue(component, color);
                    }
                    else if (propertyInfo.PropertyType == typeof(Vector3))
                    {
                        var valuesStr = fieldValues[propertyInfo.Name];
                        valuesStr = valuesStr.Substring(1, valuesStr.Length - 2);
                        var values = valuesStr.Split(",");
                        Vector3 vec3 = new Vector3(float.Parse(values[0]),
                            float.Parse(values[1]), float.Parse(values[2]));
                        propertyInfo.SetValue(component, vec3);
                    }
                    else if (propertyInfo.PropertyType == typeof(Vector2))
                    {
                        var valuesStr = fieldValues[propertyInfo.Name];
                        valuesStr = valuesStr.Substring(1, valuesStr.Length - 2);
                        var values = valuesStr.Split(",");
                        Vector2 vec2 = new Vector2(float.Parse(values[0]), float.Parse(values[1]));
                        propertyInfo.SetValue(component, vec2);
                    }
                    else if (propertyInfo.PropertyType == typeof(Vector4))
                    {
                        var valuesStr = fieldValues[propertyInfo.Name];
                        valuesStr = valuesStr.Substring(1, valuesStr.Length - 2);
                        var values = valuesStr.Split(",");
                        Vector4 vec4 = new Vector4(float.Parse(values[0]), float.Parse(values[1]),
                            float.Parse(values[2]), float.Parse(values[3]));
                        propertyInfo.SetValue(component, vec4);
                    }
                }
            }

            var fields = type.GetFields();
            for (int i = 0; i < fields.Length; ++i)
            {
                var fieldInfo = fields[i];
                if (fieldValues.ContainsKey(fieldInfo.Name))
                {
                    if (fieldInfo.FieldType == typeof(string))
                    {
                        fieldInfo.SetValue(component, fieldValues[fieldInfo.Name]);
                    }
                    else if (fieldInfo.FieldType == typeof(int))
                    {
                        fieldInfo.SetValue(component, int.Parse(fieldValues[fieldInfo.Name]));
                    }
                    else if (fieldInfo.FieldType == typeof(float))
                    {
                        fieldInfo.SetValue(component, float.Parse(fieldValues[fieldInfo.Name]));
                    }
                    else if (fieldInfo.FieldType == typeof(bool))
                    {
                        fieldInfo.SetValue(component, bool.Parse(fieldValues[fieldInfo.Name]));
                    }
                    else if (fieldInfo.FieldType == typeof(Color))
                    {
                        var valuesStr = fieldValues[fieldInfo.Name];
                        valuesStr = valuesStr.Substring(1, valuesStr.Length - 2);
                        var values = valuesStr.Split(",");
                        Color color = new Color(float.Parse(values[0]),
                            float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
                        fieldInfo.SetValue(component, color);
                    }
                    else if (fieldInfo.FieldType == typeof(Vector3))
                    {
                        var valuesStr = fieldValues[fieldInfo.Name];
                        valuesStr = valuesStr.Substring(1, valuesStr.Length - 2);
                        var values = valuesStr.Split(",");
                        Vector3 vec3 = new Vector3(float.Parse(values[0]),
                            float.Parse(values[1]), float.Parse(values[2]));
                        fieldInfo.SetValue(component, vec3);
                    }
                    else if (fieldInfo.FieldType == typeof(Vector2))
                    {
                        var valuesStr = fieldValues[fieldInfo.Name];
                        valuesStr = valuesStr.Substring(1, valuesStr.Length - 2);
                        var values = valuesStr.Split(",");
                        Vector2 vec2 = new Vector2(float.Parse(values[0]), float.Parse(values[1]));
                        fieldInfo.SetValue(component, vec2);
                    }
                    else if (fieldInfo.FieldType == typeof(Vector4))
                    {
                        var valuesStr = fieldValues[fieldInfo.Name];
                        valuesStr = valuesStr.Substring(1, valuesStr.Length - 2);
                        var values = valuesStr.Split(",");
                        Vector4 vec4 = new Vector4(float.Parse(values[0]), float.Parse(values[1]),
                            float.Parse(values[2]), float.Parse(values[3]));
                        fieldInfo.SetValue(component, vec4);
                    }
                }
            }
        }

        public static string GetPropertyValue(string propertyName, ref List<string> argList, bool removeProperty = true, int startIndex = 0)
        {
            string value = string.Empty;
            if (!string.IsNullOrEmpty(propertyName) && argList != null && argList.Count > 0)
            {
                for (int i = startIndex; i < argList.Count - 1; i += 2)
                {
                    if (argList[i].CompareTo(propertyName) == 0)
                    {
                        value = argList[i + 1];
                        if (removeProperty)
                        {
                            argList.RemoveRange(i, 2);
                        }
                    }
                }
            }
            return value.Trim('"');
        }

        public static string GetScriptClassName(string scriptName)
        {
            int idx = scriptName.IndexOf(".cs");
            if (idx > 0)
            {
                scriptName = scriptName.Substring(0, idx);
            }
            return scriptName;
        }

        public static bool FileExistsInProject(string fileName)
        {
            string[] founds = Directory.GetFiles(Application.dataPath, fileName, SearchOption.AllDirectories);
            return (founds != null && founds.Length > 0);
        }
        public static bool FilesExistInProject(string[] fileNames)
        {
            bool retVal = true;
            for (int i = 0; i < fileNames.Length; ++i)
            {
                string[] founds = Directory.GetFiles(Application.dataPath, fileNames[i], SearchOption.AllDirectories);
                if (founds == null || founds.Length == 0)
                {
                    retVal = false;
                    break;
                }
            }
            return retVal;
        }

        public static bool OpenWorld(string worldName)
        {
            bool retVal = false;
            if (!string.IsNullOrEmpty(worldName))
            {
                if (EditorSceneManager.GetActiveScene().name.CompareTo(worldName) == 0)
                {
                    retVal = true;
                }
                else
                {
                    //check existance
                    string filePath = Path.Combine(Application.dataPath, "Scenes", $"{worldName}.unity");
                    if (File.Exists(filePath))
                    {
                        EditorSceneManager.OpenScene($"Assets/Scenes/{worldName}.unity");
                    }
                }
            }
            return retVal;
        }

        public static bool SetFieldValue(object component, System.Reflection.FieldInfo fieldInfo, string value)
        {
            value = value.Trim('"');
            if (fieldInfo.FieldType == typeof(string))
            {
                fieldInfo.SetValue(component, value);
            }
            else if (fieldInfo.FieldType == typeof(float))
            {
                fieldInfo.SetValue(component, float.Parse(value));
            }
            else if (fieldInfo.FieldType == typeof(int))
            {
                fieldInfo.SetValue(component, int.Parse(value));
            }
            else if (fieldInfo.FieldType == typeof(bool))
            {
                fieldInfo.SetValue(component, bool.Parse(value));
            }
            else if (fieldInfo.FieldType == typeof(Vector2))
            {
                var float2 = Executor.ParseFloat2(value);
                Vector2 vector2 = new Vector2(float2[0], float2[1]);
                fieldInfo.SetValue(component, vector2);
            }
            else if (fieldInfo.FieldType == typeof(Vector3))
            {
                var float3 = Executor.ParseFloat3(value);
                Vector3 vector3 = new Vector3(float3[0], float3[1], float3[2]);
                fieldInfo.SetValue(component, vector3);
            }
            else if (fieldInfo.FieldType == typeof(Vector4))
            {
                var float4 = Executor.ParseFloat4(value);
                Vector3 vector4 = new Vector4(float4[0], float4[1], float4[2], float4[3]);
                fieldInfo.SetValue(component, vector4);
            }
            else if (fieldInfo.FieldType == typeof(Color))
            {
                var float4 = Executor.ParseFloat4(value);
                Color color = new Color(float4[0], float4[1], float4[2], float4[3]);
                fieldInfo.SetValue(component, color);
            }
            else
            {
                return false;
            }
            return true;
        }
        public static bool SetPropertyValue(object component, System.Reflection.PropertyInfo propertyInfo, string value)
        {
            value = value.Trim('"');
            if (propertyInfo.PropertyType == typeof(string))
            {
                propertyInfo.SetValue(component, value);
            }
            else if (propertyInfo.PropertyType == typeof(float))
            {
                propertyInfo.SetValue(component, float.Parse(value));
            }
            else if (propertyInfo.PropertyType == typeof(int))
            {
                propertyInfo.SetValue(component, int.Parse(value));
            }
            else if (propertyInfo.PropertyType == typeof(bool))
            {
                propertyInfo.SetValue(component, bool.Parse(value));
            }
            else if (propertyInfo.PropertyType == typeof(Vector2))
            {
                var float2 = Executor.ParseFloat2(value);
                Vector2 vector2 = new Vector2(float2[0], float2[1]);
                propertyInfo.SetValue(component, vector2);
            }
            else if (propertyInfo.PropertyType == typeof(Vector3))
            {
                var float3 = Executor.ParseFloat3(value);
                Vector3 vector3 = new Vector3(float3[0], float3[1], float3[2]);
                propertyInfo.SetValue(component, vector3);
            }
            else if (propertyInfo.PropertyType == typeof(Vector4))
            {
                var float4 = Executor.ParseFloat4(value);
                Vector3 vector4 = new Vector4(float4[0], float4[1], float4[2], float4[3]);
                propertyInfo.SetValue(component, vector4);
            }
            else if (propertyInfo.PropertyType == typeof(Color))
            {
                var float4 = Executor.ParseFloat4(value);
                Color color = new Color(float4[0], float4[1], float4[2], float4[3]);
                propertyInfo.SetValue(component, color);
            }
            else
            {
                return false;
            }
            return true;
        }

        public static List<string> GetSettingsList(string settingsString)
        {
            List<string> settingsList = new List<string>();
            string[] settingsArr = settingsString.Split(';');
            for (int i = 0; i < settingsArr.Length; ++i)
            {
                int idx = settingsArr[i].IndexOf("=");
                string key = settingsArr[i].Substring(0, idx);
                string value = settingsArr[i].Substring(idx + 1);
                settingsList.Add(key);
                settingsList.Add(value);
            }
            return settingsList;
        }

        public static void SaveActiveScene()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(activeScene.name))
            {
                EditorSceneManager.SaveScene(activeScene, Path.Combine(Defs.k_SpacesDirectory, Defs.k_DefaultSpaceName + ".unity"));
            }
            else
            {
                EditorSceneManager.SaveScene(activeScene);
            }
        }

        public static void AddSceneToBuildSettingsSceneList(string scenePathName)
        {
            var scenes = EditorBuildSettings.scenes;
            int len = scenes.Length;
            EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[len + 1];
            System.Array.Copy(scenes, newScenes, len);
            newScenes[len] = new EditorBuildSettingsScene(scenePathName, true);
            EditorBuildSettings.scenes = newScenes;
        }

        public static bool PlaceObjectInFrontOfSceneView(GameObject gameObject)
        {
            // Get the current Scene View camera
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null || sceneView.camera == null)
            {
                Debug.LogWarning("No active Scene View camera found.");
                return false;
            }

            Camera cam = sceneView.camera;
            Vector3 forward = cam.transform.forward;
            Vector3 position = cam.transform.position + forward * 5f;   // 5 units in front of camera
            position.y = cam.transform.position.y;                      //make the new object the same hight with the camera 
            gameObject.transform.position = position;
            //gameObject.transform.rotation = Quaternion.identity;      //Comment this line to keep the prefab's original rotation
            Selection.activeGameObject = gameObject;
            return true;
        }

        public static void ForceUpdateEditorWindows()
        {
            if (Selection.activeGameObject == null)
            {
                return;
            }

            EditorUtility.SetDirty(Selection.activeGameObject);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
            //SceneView.lastActiveSceneView.FrameSelected(); //Auto change scnee view camera focus. Is not needed here. 

            EditorApplication.DirtyHierarchyWindowSorting();
            EditorApplication.RepaintHierarchyWindow();

            var inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            EditorWindow.GetWindow(inspectorType)?.Repaint();
        }

        public static void ForceUpdateSceneView()
        {
            if (Selection.activeGameObject != null)
            {
                EditorUtility.SetDirty(Selection.activeGameObject);
            }
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            SceneView.RepaintAll();
        }

        public static System.Type GetClassTypeByName(string typeName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            System.Type type = null;
            foreach (var assembly in assemblies)
            {
                type = assembly.GetType(typeName);
                if (type != null)
                {
                    break;
                }
            }
            return type;
        }

        public static bool PutDownObject(GameObject target)
        {
            if (Physics.defaultPhysicsScene == null || target == null)
            {
                return false;
            }

            var colliders = target.GetComponentsInChildren<Collider>();
            if (colliders.Length == 0)
            {
                return false;
            }

            var bounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach (Collider collider in colliders)
            {
                bounds.Encapsulate(collider.bounds);
            }

            float maxDistance = Mathf.Max(bounds.size.y * 3.0f, 10000.0f);
            if (Physics.Raycast(target.transform.position, Vector3.down, out var hit, maxDistance))
            {
                Vector3 newPos = target.transform.position;
                newPos.y = hit.point.y - bounds.min.y;
                target.transform.position = newPos;
                return true;
            }

            return false;
        }
    }
}
#endif