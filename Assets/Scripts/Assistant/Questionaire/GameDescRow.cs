using System;
using UnityEngine;
using TMPro;

public class GameDescRow : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _name;

    Action<string> _loadCallback = null;
    Action<string> _deleteCallback = null;

    public void Init(string gameDescName, Action<string> loadCallback, Action<string> deleteCallback)
    {
        _name.text = gameDescName;
        _loadCallback = loadCallback;
        _deleteCallback = deleteCallback;
    }

    public void OnLoad()
    {
        _loadCallback?.Invoke(_name.text);
    }

    public void OnDelete()
    {
        _deleteCallback?.Invoke(_name.text);
    }
}
