using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using SimpleJSON;

public class CodeGenerator : MonoBehaviour
{
    private const string apiUrl = "http://localhost:4891/v1/chat/completions";
    private const string testPrompt = "Write only a C# Unity script that moves a GameObject with WASD input. No explanation.";

    public Action<bool, string> OnCompleted = null;
    static public CodeGenerator Instance { get; private set; }

    public enum EStatus
    {
        Idle,
        Busy,
        Completed,
        Failed
    }

    public EStatus Status { get; private set; } = EStatus.Idle;
    string _generatedCode = string.Empty;

    public string GetGeneratedCode(bool clear = true)
    {
        string generatedCode = (Status == EStatus.Completed) ? _generatedCode : string.Empty ;
        if (clear)
        {
            _generatedCode = string.Empty;
            Status = EStatus.Idle;
        }
        return generatedCode;
    }

    public bool GenerateCode(string prompt, string className = null)
    {
        if (Status != EStatus.Busy)
        {
            StartCoroutine(SendPromptToLocalAPI(prompt, className));
            return true;
        }
        else
        {
            return false;
        }
    }

    private void Start()
    {
        Instance = this;
        //GenerateCode(testPrompt);   //Comment this line when testing is not needed.
    }

    IEnumerator SendPromptToLocalAPI(string prompt, string className = null)
    {
        string specificPromoptRequest = "Wrtie only code without explanation.";

        if(className != null)
        {
            specificPromoptRequest += $"Use {className} as the class name";
        }

        Status = EStatus.Busy;
        _generatedCode = string.Empty;

        string jsonPayload = JsonUtility.ToJson(new ChatRequest
        {
            model = "Nous Hermes 2 Mistral DPO",
            messages = new Message[]
            {
                new Message { role = "user", content = prompt + specificPromoptRequest }
            },
            max_tokens = 512,
            temperature = 0.2f
        });

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseMessage = request.downloadHandler.text;
                JSONObject responseObj = JSON.Parse(responseMessage).AsObject;
                JSONArray choices = responseObj["choices"].AsArray;
                if(choices.Count > 0)
                {
                    JSONObject message = choices[0]["message"].AsObject;
                    _generatedCode = message["content"];
                    Status = EStatus.Completed;
                }
                else
                {
                    _generatedCode = string.Empty;
                    Status = EStatus.Failed;
                }
                OnCompleted?.Invoke(true, _generatedCode);

            }
            else
            {
                Status = EStatus.Failed;
                OnCompleted?.Invoke(false, request.error);
            }
        }
    }

    [Serializable]
    public class ChatRequest
    {
        public string model;
        public Message[] messages;
        public int max_tokens;
        public float temperature;
    }

    [Serializable]
    public class Message
    {
        public string role;
        public string content;
    }
}


