
using SimpleJSON;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace T2G
{

    public class AddBehaviorTopic : Topic
    {
        public AddBehaviorTopic(Instruction instruction) : base (instruction)
        {
            Title = "Okay, let's add the behavior";

            (string question, string defaultAnswer, bool includeInAnswer)[] questions = new (string, string, bool)[] {
                ("Please describe the bahavior's functionality.", "cancel", false)
            };

            Questions = new List<(string, string, bool)>(questions);
        }

        protected async Awaitable<string> GenerateBehaviorCode(string prompt)
        {
            ResponseMessage = string.Empty;
            bool responseIsReady = false;
            CodeGenerator.Instance.OnCompleted += (result, responseMessage) =>
            {
                ResponseMessage = responseMessage;
                responseIsReady = true;
            };
            CodeGenerator.Instance.GenerateCode(prompt);
            while (!responseIsReady)
            {
                await Task.Delay(1000);
            }
            return ResponseMessage;
        }

        public override async Awaitable PostTopicProcess(string prompt) 
        {
            Debug.LogError($"Generated prompot {prompt}");
            string behaviorCode = await GenerateBehaviorCode(prompt);
            Debug.LogError($"Generated code {behaviorCode}");
            if (!string.IsNullOrWhiteSpace(behaviorCode) && _instruction != null)
            {
                JSONObject jObj = JSON.Parse(_instruction.Data) as JSONObject;
                jObj.Add("script", behaviorCode);
                _instruction.Data = jObj.ToString();
            }
        }
    }
}