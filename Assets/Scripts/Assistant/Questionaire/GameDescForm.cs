using UnityEngine;
using TMPro;
using SFB;
using T2G.UnityAdapter;
using System.IO;
using UnityEngine.UI;

public class GameDescForm : MonoBehaviour
{
    static readonly string k_DefaultGameDescNameKey = "DefaultGameDescName";

    [SerializeField] TMP_InputField _GameDescName;

    [SerializeField] TMP_InputField _GameTitle;
    [SerializeField] TMP_Dropdown _Genre;
    [SerializeField] TMP_Dropdown _ArtStyle;
    [SerializeField] TMP_InputField _Developer;

    [SerializeField] TMP_Dropdown _GameEngine;
    [SerializeField] TMP_InputField _Path;
    [SerializeField] TMP_InputField _ProjectName;

    [SerializeField] TMP_InputField _GameStory;

    [SerializeField] GameObject _ProfileView;
    [SerializeField] GameObject _JsonView;
    [SerializeField] GameObject _CommandsView;
    [SerializeField] Button _ViewProfileButton;
    [SerializeField] Button _ViewJsonButton;
    [SerializeField] Button _ViewCommandButton;
    [SerializeField] TMP_InputField _InputJson;
    [SerializeField] TMP_InputField _InputCommands;

    [SerializeField] TMP_Dropdown _SelectSampleGameDesc;

    [SerializeField] GameDescList _GameDescList;

    GameDesc _gameDesc;

    private void OnEnable()
    {
        _SelectSampleGameDesc.ClearOptions();
        for (int i = 0; i < SampleGameDescLibrary.SampleGameDescNames.Length; ++i)
        {
            _SelectSampleGameDesc.options.Add(new TMP_Dropdown.OptionData(SampleGameDescLibrary.SampleGameDescNames[i]));
        }

        var gameDescName = PlayerPrefs.GetString(k_DefaultGameDescNameKey, string.Empty);
        var gameDesc = JsonParser.LoadGameDesc(gameDescName);
        InitForm(gameDesc);
        SetViewPanel(0);
    }

    public void SetViewPanel(int viewIndex)
    {
        _ProfileView.SetActive(viewIndex == 0);
        _JsonView.SetActive(viewIndex == 1);
        _CommandsView.SetActive(viewIndex == 2);

        _ViewProfileButton.interactable = (viewIndex != 0);
        _ViewJsonButton.interactable = (viewIndex != 1);
        _ViewCommandButton.interactable = (viewIndex != 2); 

        if(viewIndex == 2)
        {
            _InputCommands.text = string.Empty;
            var instructions = Interpreter.Interpret(_InputJson.text);
            foreach (var cmd in instructions)
            {
                _InputCommands.text += cmd + "\n";
            }
        }
    }

    void InitForm(GameDesc gameDesc = null)
    {
        if (gameDesc == null)
        {
            _gameDesc = new GameDesc();
            _gameDesc.Name = "New Game";
            _gameDesc.Developer = Settings.User;
            _gameDesc.Project.Path = PlayerPrefs.GetString(Defs.k_ProjectPathname, string.Empty);
            _gameDesc.Project.Name = Path.GetFileName(_gameDesc.Project.Path);
            OnSave();
        }
        else
        {
            _gameDesc = gameDesc;
        }

        _GameDescName.text = _gameDesc.Name;
        _GameTitle.text = _gameDesc.Title;
        _Genre.value = _Genre.options.FindIndex(option => option.text.CompareTo(_gameDesc.Genre) == 0);
        _ArtStyle.value = _ArtStyle.options.FindIndex(option => option.text.CompareTo(_gameDesc.ArtStyle) == 0);
        _Developer.text = _gameDesc.Developer;
        _GameEngine.value = _GameEngine.options.FindIndex(option => option.text.CompareTo(_gameDesc.Project.Engine) == 0);
        _Path.text = _gameDesc.Project.Path;
        _ProjectName.text = _gameDesc.Project.Name;
        _GameStory.text = _gameDesc.GameStory;
        _InputJson.text = JsonParser.JSONText;
    }

    public void OnSelectPath()
    {
        string[] paths = StandaloneFileBrowser.OpenFolderPanel("Choose project path", string.Empty, false);
        if(paths.Length > 0)
        {
            _Path.text = paths[0];
        }
    }

    public void OnLoadGameDesc()
    {
        _GameDescList.LoadGameDescCallback = (gameDescName) =>
        {
            var gameDesc = JsonParser.LoadGameDesc(gameDescName);
            InitForm(gameDesc);
        };
        _GameDescList.gameObject.SetActive(true);
    }

    public void OnLoadSample()
    {
        GameDesc gameDesc = new GameDesc();
        if(SampleGameDescLibrary.GetSampleGameDesc(_SelectSampleGameDesc.value, ref gameDesc))
        {
            _gameDesc = gameDesc;
            JsonParser.Serialize(gameDesc);
            InitForm(gameDesc);
        }
    }

    public GameDesc GetGameDesc()
    {
        return _gameDesc;
    }

    public string GetGameDescJson()
    {
        return _InputJson.text;
    }

    public GameDesc GetGameDescBasics()
    {
        _gameDesc.Name = _GameDescName.text;
        _gameDesc.Title = _GameTitle.text;
        _gameDesc.Genre = _Genre.options[_Genre.value].text;
        _gameDesc.ArtStyle = _ArtStyle.options[_ArtStyle.value].text;
        _gameDesc.Developer = _Developer.text;
        _gameDesc.Project.Engine = _GameEngine.options[_GameEngine.value].text;
        _gameDesc.Project.Path = _Path.text;
        _gameDesc.Project.Name = _ProjectName.text;
        _gameDesc.GameStory = _GameStory.text;
        return _gameDesc;
    }

    public void OnSave()
    {
        var gameDesc = GetGameDescBasics();
        JsonParser.SerializeAndSave(gameDesc);
        PlayerPrefs.SetString(k_DefaultGameDescNameKey, gameDesc.Name);
    }

    public void OnCancel()
    {
        gameObject.SetActive(false);
    }

}
