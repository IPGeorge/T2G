using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using T2G.Communicator;
using UnityEngine;

namespace T2G
{
    public class CmdOpenProject : Command
    {
        public static readonly string CommandKey = "open_project";

        private string _projectPathName;
        private Process _process;
        private EventHandler _eventHandler;

        public override async Awaitable<bool> Execute(params string[] args)
        {
            string unityEditorPath = GetUnityEditorPath();
            if (string.IsNullOrEmpty(unityEditorPath))
            {
                return false;
            }

            if (args.Length < 1 || string.IsNullOrEmpty(args[0]))
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
                        _projectPathName = defaultPath.Substring(startIdx, endIdx - startIdx);
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
            await Task.Yield();
            Thread thread = new Thread(() => StartOpenProjectThread(arguments, unityEditorPath, OnExecutionCompleted));
            thread.Start();
            await WaitforProjectEditorIsOpenedAndConnected();
            OnExecutionCompleted?.Invoke(true, ConsoleController.eSender.System, $"Project is openned!");
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

        static async Awaitable<bool> WaitforProjectEditorIsOpenedAndConnected()
        {
            return await WaitForConnected();
        }

        static async Awaitable<bool> WaitForConnected(float delaySeconds = 60.0f)
        {
            bool timeout = false;
            DateTime startDT = DateTime.Now; 
            while (!CommunicatorClient.Instance.IsConnected)
            {
                if((DateTime.Now - startDT).TotalSeconds > delaySeconds)
                {
                    timeout = true;
                    break;
                }
                await Task.Delay(1000);
            }
            return !timeout;
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
}