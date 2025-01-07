using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using T2G.UnityAdapter;
using System.Threading.Tasks;

public class CmdConnect : Command
{
    public static readonly string CommandKey = "Connect";

    public override bool Execute(params string[] args)
    {
        float timeoutScale = 1.0f;
        if (args.Length > 0)
        {
            float.TryParse(args[0], out timeoutScale);
        }
        CommunicatorClient.Instance.StartClient();
         Task.Run(async () => { await WaitForConnection(timeoutScale); });
        return true;
    }

    async Task WaitForConnection(float timeout)
    {
        while (CommunicatorClient.Instance.ClientState == CommunicatorClient.eClientState.Connecting)
        {
            await Task.Delay((int)(timeout));
        }

        if (CommunicatorClient.Instance.IsConnected)
        {
            OnExecutionCompleted?.Invoke(true, ConsoleController.eSender.System, "Connected!");
        }
        else
        {
            OnExecutionCompleted?.Invoke(false, ConsoleController.eSender.System, "Timeout!");
        }
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
