using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class CodeGenerator : MonoBehaviour
{
    static public CodeGenerator Instance { get; private set; }

    public void GenerateCode(string prompt)
    {
        StartCoroutine(SendPromptToLocalAPI(prompt));
    }

    private void Start()
    {
        Instance = this;
        GenerateCode("Write Unity c# code to control a game object to move up and down with the up and down arrow keys. The maximum and minimum y position range is -10 and 10.");
    }

    IEnumerator SendPromptToLocalAPI(string prompt)
    {
        using (UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:8000/generate", CreateForm(prompt)))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = www.downloadHandler.text;
                Debug.Log("Generated Code:\n" + jsonResponse);
            }
            else
            {
                Debug.LogError("Error: " + www.error);
            }
        }
    }

    private WWWForm CreateForm(string prompt)
    {
        WWWForm form = new WWWForm();
        form.AddField("prompt", prompt);
        return form;
    }
}


