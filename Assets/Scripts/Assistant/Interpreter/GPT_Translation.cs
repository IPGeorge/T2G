using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace T2G
{
    public class GPT_Translation : Translation  //Natual Language Process
    {
        private const string k_apiUrl = "http://localhost:4891/v1/chat/completions";
        private const string k_model = "Nous Hermes 2 Mistral DPO";
        private const string k_roleRequest = "Translate this prompt into a structured instruction.";

        protected override bool ParseInstructionData(string prompt, out string key, out (string name, string value)[] arguments)
        {
            key = string.Empty;
            arguments = null;
            return false;
        }

        public override async Awaitable<(bool succeeded, string message, Instruction[] instructions)> Translate(string prompt)
        {
            _instructionList.Clear();

            string message = string.Empty;
            _instructionList.Clear();

            string response = await SendPromptToLocalAPI("user", k_roleRequest);
            response = await SendPromptToLocalAPI("user", prompt);

            //ParseInstructions();

            return (_instructionList.Count > 0, message, _instructionList.ToArray());
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

        public static async Task<string> SendPromptToLocalAPI(string senderRole, string prompt)
        {
            string response = string.Empty;

            string jsonPayload = JsonUtility.ToJson(new ChatRequest
            {
                model = k_model,
                messages = new Message[]
                {
                    new Message { role = senderRole, content = prompt }
                },
                max_tokens = 512,
                temperature = 0.2f
            });

            UnityWebRequest request = new UnityWebRequest(k_apiUrl, "POST");

            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            await AwaitUnityWebRequest(request);
            //await request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseMessage = request.downloadHandler.text;
                JSONObject responseObj = JSON.Parse(responseMessage).AsObject;
                JSONArray choices = responseObj["choices"].AsArray;
                if (choices.Count > 0)
                {
                    JSONObject message = choices[0]["message"].AsObject;
                    response = message["content"];
                }
                else
                {
                    response = string.Empty;
                }
            }

            request.Dispose();
            return response;
        }

        private static Task AwaitUnityWebRequest(UnityWebRequest request)
        {
            var tcs = new TaskCompletionSource<bool>();
            var op = request.SendWebRequest();
            op.completed += _ => tcs.SetResult(true);
            return tcs.Task;
        }

        public static async Task<string> Train(string input, string output)
        {
            List<Message> messages = new List<Message>
            {
                new Message { role = "system", content =
                    "You are a compiler that converts natural language into STRICT JSON instructions.\n" +
                    "Always return valid JSON following this schema:\n" +
                    "{ \"Type\": int, \"State\": int, \"Keyword\": string, \"DataType\": int, " +
                    "\"Data\": {\"name\": string, \"type\": string}, \"Assets\": string }" },
                new Message { role = "user", content = "create a cylinder named Tower" },
                new Message { role = "assistant", content =
                    "{\"Type\":2,\"State\":1,\"Keyword\":\"create_object\",\"DataType\":3," +
                    "\"Data\":{\"name\":\"Tower\",\"type\":\"cylinder\"},\"Assets\":\"\"}" },
                new Message { role = "user",  content = "prompt"}
            };



            //await SendPromptToLocalAPI("system", forwardMessage);
            //await SendPromptToLocalAPI("system", schemaMessage);
            await SendPromptToLocalAPI("user", input);
            return await SendPromptToLocalAPI("assistant", output);
        }

        public static async Task<string> Test(string prompt)
        {
            string promptContext = k_roleRequest + "\n" + prompt;
            Debug.Log($"[Test Pprompt] {promptContext}");
            return await SendPromptToLocalAPI("user", promptContext);
        }
    }
}