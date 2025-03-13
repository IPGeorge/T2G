using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace T2G
{
    public class CommandSystem : MonoBehaviour
    {
        private static CommandSystem _instance = null;
        public static CommandSystem Instance => _instance;

        private Dictionary<string, Type> _commandsRegistry = new Dictionary<string, Type>();

        void RegisterCommands()
        {
            _commandsRegistry.Add(CmdCreateProject.CommandKey.ToLower(), typeof(CmdCreateProject));
            _commandsRegistry.Add(CmdInitProject.CommandKey.ToLower(), typeof(CmdInitProject));
            _commandsRegistry.Add(CmdOpenProject.CommandKey.ToLower(), typeof(CmdOpenProject));
            _commandsRegistry.Add(CmdConnect.CommandKey.ToLower(), typeof(CmdConnect));
            _commandsRegistry.Add(CmdDisconnect.CommandKey.ToLower(), typeof(CmdDisconnect));
            _commandsRegistry.Add(CmdClear.CommandKey.ToLower(), typeof(CmdClear));
        }

        private void Awake()
        {
            _instance = this;
        }

        void Start()
        {
            RegisterCommands();
        }

        private void OnDestroy()
        {
            _commandsRegistry.Clear();
        }

        public bool IsCommand(string inputString)
        {
            inputString = inputString.Trim();
            int idx = inputString.IndexOf(" ");
            string cmd = idx > 0 ? inputString.Substring(0, idx) : inputString;
            return _commandsRegistry.ContainsKey(cmd);
        }

        public async Awaitable<bool> ExecuteCommand(string commandKey, params string[] args)
        {
            commandKey = commandKey.ToLower();
            if (!_commandsRegistry.ContainsKey(commandKey))
            {
                return false;
            }
            bool waitingForCompletion = true;
            var command = (Command)Activator.CreateInstance(_commandsRegistry[commandKey]);
            command.OnExecutionCompleted = (result, sender, message) => 
            {
                waitingForCompletion = false;
            };
            command.Execute(args);

            await Task.Run(async () => 
            { 
                while(waitingForCompletion)
                {
                    await Task.Delay(100);
                }
            });
            return true;
        }
    }
}