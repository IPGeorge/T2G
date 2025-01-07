using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections;

public class CommandSystem : MonoBehaviour
{
    private static CommandSystem _instance = null;
    public static CommandSystem Instance => _instance;
       
    private Dictionary<string, Type> _commandsRegistry = new Dictionary<string, Type>();

    class CommandRecord { public Command Command; public string[] Args; };

    Queue<CommandRecord> _executionQueue = new Queue<CommandRecord>();

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

    private void OnEnable()
    {
        StartCoroutine(ExecuteQueuedCommand());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
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

    public bool ExecuteCommand(Action<bool, ConsoleController.eSender, string> OnExecutionCompleted, 
        string commandKey, params string[] args)
    {
        commandKey = commandKey.ToLower();
        if (!_commandsRegistry.ContainsKey(commandKey))
        {
            return false;
        }

        var command = (Command)Activator.CreateInstance(_commandsRegistry[commandKey]);
        command.OnExecutionCompleted = OnExecutionCompleted;
        _executionQueue.Enqueue(new CommandRecord() { Command = command, Args = args });
        return true;
    }

    IEnumerator ExecuteQueuedCommand()
    {
        bool isBusy = false;
        while (gameObject.activeSelf)
        {
            if (!isBusy && _executionQueue.Count > 0)
            {
                isBusy = true;
                var cmdRec = _executionQueue.Dequeue();
                cmdRec.Command.OnExecutionCompleted += (succeeded, sender, message) => {
                    isBusy = false;
                };
                cmdRec.Command.Execute(cmdRec.Args);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    public bool ExecuteCommandImmediately(Action<bool, ConsoleController.eSender, string> OnExecutionCompleted,
    string commandKey, params string[] args)
    {
        if (_commandsRegistry.ContainsKey(commandKey))
        {
            var command = (Command)Activator.CreateInstance(_commandsRegistry[commandKey]);
            command.OnExecutionCompleted = OnExecutionCompleted;
            return command.Execute(args);
        }
        return false;
    }
}
