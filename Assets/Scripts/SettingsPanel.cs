using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using T2G;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField _UnityEditorPath;
    [SerializeField] private TMP_InputField _UserName;
    [SerializeField] private TMP_InputField _AssistantName;
    [SerializeField] private TMP_InputField _AssetsPath;

    private void OnEnable()
    {
        SettingsT2G.Load();
        _UnityEditorPath.text = SettingsT2G.UnityEditorPath;
        _AssetsPath.text = SettingsT2G.RecoursePath;
        _UserName.text = SettingsT2G.User;
        _AssistantName.text = SettingsT2G.Assistant;
    }

    public void OnSave()
    {
        SettingsT2G.UnityEditorPath = _UnityEditorPath.text;
        SettingsT2G.RecoursePath = _AssetsPath.text;
        SettingsT2G.User = _UserName.text;
        SettingsT2G.Assistant = _AssistantName.text;
        SettingsT2G.Save();
    }
}
