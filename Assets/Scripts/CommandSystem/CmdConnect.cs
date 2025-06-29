using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using T2G.Communicator;
using System.Threading.Tasks;

namespace T2G
{
    public class CmdConnect : Command
    {
        public static readonly string CommandKey = "connect";

        public override bool Execute(params string[] args)
        {
            float timeoutScale = 1.0f;
            if (args.Length > 0)
            {
                float.TryParse(args[0], out timeoutScale);
            }
            CommunicatorClient.Instance.StartClient();
            Task.Run(async () => { 
                bool connected = await CommunicatorClient.Instance.WaitForConnection(timeoutScale);
                OnExecutionCompleted?.Invoke(true, 
                    ConsoleController.eSender.System, 
                    connected ? "Connected!" : "Timeout!");
            });
            return true;
        }

        public override string GetKey()
        {
            return CommandKey.ToLower();
        }

        public override string[] GetArguments()
        {
            string[] args = { };
            return args;
        }
    }
}