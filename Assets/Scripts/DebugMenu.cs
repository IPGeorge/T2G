using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.IO;
using T2G;
using System.Threading.Tasks;
using System.Text;

public class DebugMenu : MonoBehaviour
{
    const string k_RegexMAtchTestDataFileName = "RegexTestData.txt";

    [SerializeField] GameObject[] _tabPanels;
    [SerializeField] TextMeshProUGUI _status;

    int _selectedIndex = 0;

    private void OnEnable()
    {
        LoadRegexMatchTestData();
        OnSelectedTab(_selectedIndex);
    }

    public void OnSelectedTab(int index)
    {
        if(index < 0 || index >= _tabPanels.Length)
        {
            return;
        }

        for(int i = 0; i < _tabPanels.Length; ++i)
        {
            _tabPanels[i].SetActive(i == index);
        }
        _selectedIndex = index;
    }

    #region Regex Match Test
    
    [SerializeField] TextMeshProUGUI _batchTestRegexMatchText;
    [SerializeField] TMP_InputField _inputTestRegexMatchText;

    HashSet<string> _regexMatchTestData = new HashSet<string>();

    void LoadRegexMatchTestData()
    {
        string path = Path.Combine(Application.persistentDataPath, k_RegexMAtchTestDataFileName);
        if (File.Exists(path))
        {
            var data = File.ReadAllLines(path);
            _regexMatchTestData = new HashSet<string>(data);
        }
        else
        {
            _regexMatchTestData.Clear();
        }
    }

    void SaveRegexMatchTestData()
    {
        string path = Path.Combine(Application.persistentDataPath, k_RegexMAtchTestDataFileName);
        File.WriteAllLines(path, _regexMatchTestData);
    }

    string GetIndexStringList(int[] indices)
    {
        StringBuilder sb = new StringBuilder();
        foreach(int index in indices)
        {
            sb.Append($"{index},");
        }
        string str = sb.ToString();
        return str.Substring(0, str.Length - 1);
    }

    public async void OnBatchTestRegex()
    {
        _batchTestRegexMatchText.text = string.Empty;
        foreach (var teesData in _regexMatchTestData)
        {
            var matches = RBP_Translation.TestRegexMatch(teesData);
            if(matches.Length > 0)
            {
                _batchTestRegexMatchText.text += $"<color=green>{teesData}: Matches={matches.Length.ToString()} [{GetIndexStringList(matches)}].</color>\n";
            }
            else
            {
                _batchTestRegexMatchText.text += $"<color=red>{teesData}: no match was found.</color>\n";  
            }
            _batchTestRegexMatchText.SetAllDirty();
            await Task.Yield();
        }
    }

    public void OnTestRegexMatch()
    {
        var matches = RBP_Translation.TestRegexMatch(_inputTestRegexMatchText.text);
        if (matches.Length > 0)
        {

            _status.text = $"<color=green>{_inputTestRegexMatchText.text}: matches={matches.Length.ToString()} [{GetIndexStringList(matches)}].</color>";
        }
        else
        {
            _status.text = $"<color=red>{_inputTestRegexMatchText.text}: no match was found.</color>";
        }
    }

    public void OnSaveToRegexMatchTestData()
    {
        _regexMatchTestData.Add(_inputTestRegexMatchText.text);
        SaveRegexMatchTestData();
    }

    [SerializeField] TMP_InputField _AssetInfo;
    [SerializeField] TMP_Dropdown _AssetType;
    [SerializeField] TextMeshProUGUI _FoundAssets;
    public async void OnTestSearchAssets()
    {
        string assetType = (_AssetType.value == 0) ? string.Empty : _AssetType.options[_AssetType.value].text;
        _FoundAssets.text = await ContentLibrary.SearchAssets(_AssetInfo.text, assetType);
    }

    #endregion Regex Match Test

}
