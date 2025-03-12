using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace T2G
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class TranslatorAttribute : Attribute
    {
        public string InstructionKey { get; private set; }

        public TranslatorAttribute(string instructionKey)
        {
            InstructionKey = instructionKey;
        }
    }

    public abstract class Translator
    {
        /* Function Translate(...)
         * This abstrac function needs to be implemented in sub-classes to generate instructions. 
         * Inputs:
         *      arguments: an tuple array including pairs of parameter names and values
         *      instructions: returns the generated raw instructions
         * Returns: true-succeeded; false-failed
         */
        abstract public bool Translate((string name, string value)[] arguments, ref List<Instruction> instructions);

        protected string GetParamFromArguments((string name, string value)[] arguments, string paramName)
        {
            foreach (var argument in arguments)
            {
                if (string.Compare(argument.name, paramName) == 0)
                {
                    return argument.value;
                }
            }
            return null;
        }

        protected string GetJsonFromArguments((string name, string value)[] arguments, params string[] requiredParamNames)
        {
            HashSet<string> requiredNames = new HashSet<string>();
            bool expectParams = (requiredParamNames.Length > 0);
            
            if (expectParams)
            {
                foreach (string requiredName in requiredParamNames)
                {
                    requiredNames.Add(requiredName);
                }
            }

            JSONObject json = new JSONObject();
            foreach (var argument in arguments)
            {
                if (expectParams && requiredNames.Count <= 0)
                {
                    return json.ToString();
                }

                if (!expectParams || requiredNames.Contains(argument.name))
                {
                    json.Add(argument.name, argument.value);
                    if (expectParams)
                    {
                        requiredNames.Remove(argument.name);
                    }
                }
            }

            if (expectParams)
            {
                return (requiredNames.Count <= 0) ? json.ToString() : null;
            }
            else
            {
                return json.ToString();
            }
        }
    }


    public abstract class Translation
    {
        Dictionary<string, Translator> _tranlatorPool = new Dictionary<string, Translator>(); //(rule, translation)
        void Register_Translators()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var translatorClasses = assembly.GetTypes()
                .Where(type => type.IsClass && type.GetCustomAttributes(typeof(TranslatorAttribute), false).Any());
            foreach (var translatorClass in translatorClasses)
            {
                var attribute = translatorClass.GetCustomAttribute<TranslatorAttribute>();
                var translator = (Translator)(Activator.CreateInstance(translatorClass));
                _tranlatorPool.Add(attribute.InstructionKey, translator);
            }
        }

        public Translation()
        {
            Register_Translators();
        }

        protected List<Instruction> _instructionList = new List<Instruction>();
        
        public virtual bool Translate(string prompt, out Instruction[] instructions)
        {
            _instructionList.Clear();
            if (ParseInstructionData(prompt, out var key, out var arguments) &&
                _tranlatorPool.ContainsKey(key))
            {
                var translator = _tranlatorPool[key];
                translator.Translate(arguments, ref _instructionList);
            }
            instructions = _instructionList.ToArray();
            return (_instructionList.Count > 0);
        }

        abstract protected bool ParseInstructionData(string prompt, out string key, out (string name, string value)[] arguments);
    }
}
