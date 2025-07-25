
using SimpleJSON;
using System;
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

            _questions = new List<(string, string, bool)>(questions);
        }

        protected async Awaitable<string> GenerateBehaviorScript(string prompt)
        {
            string generatedCode = string.Empty;
            bool responseIsReady = false;
            CodeGenerator.Instance.OnCompleted += (result, responseMessage) =>
            {
                if(!string.IsNullOrWhiteSpace(responseMessage))
                {
                    generatedCode = responseMessage;
                }
                responseIsReady = true;
            };

            JSONObject jsonObj = JSON.Parse(_instruction.Data).AsObject;

            CodeGenerator.Instance.GenerateCode(prompt, jsonObj["behaviorName"]);
            while (!responseIsReady)
            {
                await Task.Delay(1000);
            }
            return generatedCode;
        }

        public override async Awaitable PostTopicProcess(string prompt) 
        {
            if(string.IsNullOrWhiteSpace(prompt) || _instruction == null)
            {
                return;
            }

            string behaviorScript = await GenerateBehaviorScript(prompt);
            if (!string.IsNullOrWhiteSpace(behaviorScript))
            {
                _instruction.ResolvedAssetPaths = behaviorScript;
            }
        }
    }
}