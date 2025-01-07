using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;

public class CmdOpenProject : Command
{
    public static readonly string CommandKey = "OpenProject";

    private string _projectPathName;
    private Process _process;
    private EventHandler _eventHandler;

    public override bool Execute(params string[] args)
    {
        if (!PlayerPrefs.HasKey(Defs.k_UnityEditorPath))
        {
            OnExecutionCompleted?.Invoke(false, ConsoleController.eSender.Error, "Unity Editor path is not set!");
            return false;
        }
        string unityEditorPath = PlayerPrefs.GetString(Defs.k_UnityEditorPath);

        if (args.Length < 1)
        {
            string defaultPath = ConsoleController.Instance.ProjectPathName;

             if (string.IsNullOrWhiteSpace(defaultPath))
            {
                OnExecutionCompleted?.Invoke(false, ConsoleController.eSender.Error, "The project's path argument is missing!");
                return false;
            }
            else
            {
                int startIdx = defaultPath.IndexOf("[") + 1;
                int endIdx = defaultPath.IndexOf("]");
                if (startIdx > 1 && endIdx > startIdx)
                {
                    _projectPathName = defaultPath.Substring(startIdx, endIdx -startIdx);
                }
                else
                {
                    OnExecutionCompleted?.Invoke(false, ConsoleController.eSender.Error, "Invalid project path!");
                    return false;
                }
            }
        }
        else
        {
            _projectPathName = ConsoleController.Instance.ProjectPathName = args[0];
        }

        if (!Directory.Exists(_projectPathName))
        {
            OnExecutionCompleted?.Invoke(false, ConsoleController.eSender.Error, $"Project was not found.");
            return false;
        }

        var arguments = $"-projectPath {_projectPathName}";

        OnExecutionCompleted?.Invoke(true, ConsoleController.eSender.System, $"Openning ...");
        Thread thread = new Thread(() => StartOpenProjectThread(arguments, unityEditorPath, OnExecutionCompleted));
        thread.Start();
        Thread delayThread = new Thread(()=> DelayAndShowOpendedMessage(OnExecutionCompleted, 5000));
        delayThread.Start();
        return true;
    }

    static void StartOpenProjectThread(string args, string unityEditorPath, Action<bool, ConsoleController.eSender, string> OnExecutionCompleted)
    {
        Process process = new Process();
        process.StartInfo.FileName = unityEditorPath;
        process.StartInfo.Arguments = args;

        try
        {
            process.Start();
            process.WaitForExit();

        }
        catch (Exception e)
        {
            OnExecutionCompleted?.Invoke(true, ConsoleController.eSender.Error, e.Message);
            process.Close();
        }
        finally
        {
            process.Close();
            OnExecutionCompleted?.Invoke(true, ConsoleController.eSender.System, $"Project is closed.");
        }
    }

    static void DelayAndShowOpendedMessage(Action<bool, ConsoleController.eSender, string> OnExecutionCompleted, int delayMiniseconds)
    {
        Thread.Sleep(delayMiniseconds);
        OnExecutionCompleted?.Invoke(true, ConsoleController.eSender.System, "Project is openned!");
    }

    public override string GetKey()
    {
        return CommandKey.ToLower();
    }

    public override string[] GetArguments()
    {
        string[] args = { _projectPathName };
        return args;
    }
}
