using System;
using SimpleJSON;

namespace T2G
{
    [Serializable]
    public class SettingsT2G
    {
        public static readonly string k_UnityEditorPath = "UnityEditorPath";
        public static readonly string k_UserName = "UserName";
        public static readonly string k_AssistantName = "AssistantName";
        public static readonly string k_ResourcePath = "ResoucePath";

        public static bool Loaded { get; private set; } = false;

        public static string UnityEditorPath;
        public static string RecoursePath;
        public static string User;
        public static string Assistant;

        public static string ToJson(bool reload = true)
        {
            if (reload)
            {
                Load();
            }
            JSONObject jsonObj = new JSONObject();
            jsonObj.Add("UnityEditorPath", UnityEditorPath);
            jsonObj.Add("RecoursePath", RecoursePath);
            jsonObj.Add("User", User);
            jsonObj.Add("Assistant", Assistant);
            return jsonObj.ToString();
        }

        public static void FromJson(string jsonData, bool save = true)
        {
            JSONObject jsonObj = (JSONObject)JSON.Parse(jsonData);
            UnityEditorPath = jsonObj["UnityEditorPath"];
            RecoursePath = jsonObj["RecoursePath"];
            User = jsonObj["User"];
            Assistant = jsonObj["Assistant"];
            if (save)
            {
                Save();
            }
            Loaded = true;
        }

        public static void Load()
        {
#if UNITY_EDITOR
            UnityEditorPath = UnityEditor.EditorPrefs.GetString(k_UnityEditorPath, string.Empty);
            RecoursePath = UnityEditor.EditorPrefs.GetString(k_ResourcePath, string.Empty);
            User = UnityEditor.EditorPrefs.GetString(k_UserName, "You");
            Assistant = UnityEditor.EditorPrefs.GetString(k_AssistantName, "Assistant");
            Loaded = true;
#else
            UnityEditorPath = PlayerPrefs.GetString(k_UnityEditorPath, string.Empty);
            RecoursePath = PlayerPrefs.GetString(k_ResourcePath, string.Empty);
            User = PlayerPrefs.GetString(k_UserName, "You");
            Assistant = PlayerPrefs.GetString(k_AssistantName, "Assistant");
            Loaded = true;
#endif
        }

        public static void Save()
        {
#if UNITY_EDITOR
            UnityEditor.EditorPrefs.SetString(k_UnityEditorPath, UnityEditorPath);
            UnityEditor.EditorPrefs.SetString(k_ResourcePath, RecoursePath);
            UnityEditor.EditorPrefs.SetString(k_UserName, User);
            UnityEditor.EditorPrefs.SetString(k_AssistantName, Assistant);
#else
            PlayerPrefs.SetString(k_UnityEditorPath, UnityEditorPath);
            PlayerPrefs.SetString(k_ResourcePath, RecoursePath);
            PlayerPrefs.SetString(k_UserName, User);
            PlayerPrefs.SetString(k_AssistantName, Assistant);
#endif
        }
    }

}