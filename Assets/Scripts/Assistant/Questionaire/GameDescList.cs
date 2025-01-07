using System;
using System.Collections.Generic;
using UnityEngine;

public class GameDescList : MonoBehaviour
{
    public Action<string> LoadGameDescCallback = null;

    [SerializeField] Transform _Container;
    [SerializeField] GameDescRow _GameDescRow;

    List<GameDescRow> _gameDescList = new List<GameDescRow>();
    

    private void OnEnable()
    {
        foreach (var gamedesc in _gameDescList)
        {
            if (gamedesc != null)
            {
                Destroy(gamedesc.gameObject);
            }
        }

        var gameDescList = JsonParser.GetGameDescList();
        for(int i = 0; i < gameDescList.Count; ++i)
        {
            var row = Instantiate<GameDescRow>(_GameDescRow, _Container);
            row.Init(gameDescList[i], LoadGameDesc, DeleteGameDesc);
            row.gameObject.SetActive(true);
            _gameDescList.Add(row);
        }
    }

    private void OnDisable()
    {
        LoadGameDescCallback = null;
    }

    public void LoadGameDesc(string gameDescName)
    {
        LoadGameDescCallback?.Invoke(gameDescName);
        gameObject.SetActive(false);
    }

    public void DeleteGameDesc(string gameDescName)
    {
        JsonParser.DeleteGameDesc(gameDescName);
        OnEnable();
    }

    public void OnClose()
    {
        gameObject.SetActive(false);
    }
}
