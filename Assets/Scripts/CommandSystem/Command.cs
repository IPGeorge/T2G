using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using T2G;

#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class Command
{
    public Action<bool, ConsoleController.eSender, string> OnExecutionCompleted;
    public abstract string GetKey();
    public abstract string[] GetArguments();
    public string GetUnityEditorPath()
    {
#if UNITY_EDITOR
        if (!EditorPrefs.HasKey(SettingsT2G.k_UnityEditorPath))
        {
            OnExecutionCompleted?.Invoke(false, ConsoleController.eSender.Error, "Unity Editor path is not set!");
            return null;
        }
        return EditorPrefs.GetString(SettingsT2G.k_UnityEditorPath);

#else
        if (!PlayerPrefs.HasKey(SettingsT2G.k_UnityEditorPath))
        {
            OnExecutionCompleted?.Invoke(false, ConsoleController.eSender.Error, "Unity Editor path is not set!");
            return null;
        }
        return PlayerPrefs.GetString(SettingsT2G.k_UnityEditorPath);
#endif
    }
    public string GetResourcePath()
    {
#if UNITY_EDITOR
        if (EditorPrefs.HasKey(SettingsT2G.k_ResourcePath))
        {
            return EditorPrefs.GetString(SettingsT2G.k_ResourcePath);
        }
#else
        if (PlayerPrefs.HasKey(SettingsT2G.k_ResourcePath))
        {
            return PlayerPrefs.GetString(SettingsT2G.k_ResourcePath);
        }
#endif
        else
        {
            OnExecutionCompleted?.Invoke(true, ConsoleController.eSender.Error, "Could not find Resource Path!");
            return null;
        }
    }

    public abstract bool Execute(params string[] args);
}
